Shader "Custom/URP/SunStar2D"
{
    Properties
    {
        _MainTex          ("Sprite",                 2D)                = "white" {}
        _CoreColor        ("Core Color",             Color)             = (1.00, 0.98, 0.85, 1)
        _MidColor         ("Mid Color",              Color)             = (1.00, 0.60, 0.10, 1)
        _OuterColor       ("Outer Color",            Color)             = (1.00, 0.20, 0.02, 1)
        _CoronaColor      ("Corona / Ray Color",     Color)             = (1.00, 0.45, 0.05, 1)

        _Radius           ("Disk Radius",            Range(0.02, 0.48)) = 0.28
        _CoreRadius       ("Core Radius",            Range(0.01, 0.48)) = 0.10
        _EdgeSoftness     ("Edge Softness",          Range(0.001, 0.1)) = 0.015

        _RayCount         ("Ray Count",              Range(4, 64))      = 18
        _RaySharpness     ("Ray Sharpness",          Range(1, 64))      = 12.0
        _RayLength        ("Ray Length",             Range(0.0, 1.0))   = 0.55
        _SecondRayCount   ("Secondary Ray Count",    Range(0, 64))      = 12
        _SecondRayScale   ("Secondary Ray Scale",    Range(0.0, 1.0))   = 0.55

        _NoiseScale       ("Surface Noise Scale",    Range(1, 32))      = 8.0
        _NoiseStrength    ("Surface Noise Str",      Range(0.0, 1.0))   = 0.18
        _GranuleScale     ("Granule Scale",          Range(1, 128))     = 48.0
        _GranuleStr       ("Granule Strength",       Range(0.0, 1.0))   = 0.12

        _GlowRadius       ("Glow Radius",            Range(0.0, 2.0))   = 0.90
        _GlowStrength     ("Glow Strength",          Range(0.0, 4.0))   = 1.40
        _GlowFalloff      ("Glow Falloff",           Range(0.5, 8.0))   = 2.80

        _LimbDarken       ("Limb Darkening",         Range(0.0, 1.0))   = 0.55

        _RotateSpeed      ("Ray Rotate Speed",       Range(-2, 2))      = 0.04
        _PulseSpeed       ("Pulse Speed",            Range(0, 8))       = 1.20
        _PulseAmount      ("Pulse Amount",           Range(0.0, 0.3))   = 0.04
    }

    SubShader
    {
        Tags
        {
            "Queue"           = "Transparent"
            "RenderType"      = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType"     = "Plane"
        }

        Cull   Off
        ZWrite Off
        ZTest  LEqual
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _CoreColor;
                float4 _MidColor;
                float4 _OuterColor;
                float4 _CoronaColor;

                float  _Radius;
                float  _CoreRadius;
                float  _EdgeSoftness;

                float  _RayCount;
                float  _RaySharpness;
                float  _RayLength;
                float  _SecondRayCount;
                float  _SecondRayScale;

                float  _NoiseScale;
                float  _NoiseStrength;
                float  _GranuleScale;
                float  _GranuleStr;

                float  _GlowRadius;
                float  _GlowStrength;
                float  _GlowFalloff;

                float  _LimbDarken;

                float  _RotateSpeed;
                float  _PulseSpeed;
                float  _PulseAmount;
            CBUFFER_END

            struct Attributes
            {
                float4 posOS : POSITION;
                float2 uv    : TEXCOORD0;
            };

            struct Varyings
            {
                float4 posCS : SV_POSITION;
                float2 uv    : TEXCOORD0;
            };

            float hash21(float2 p)
            {
                p = frac(p * float2(127.1, 311.7));
                p += dot(p, p + 19.19);
                return frac(p.x * p.y);
            }
            float vnoise(float2 p)
            {
                float2 i = floor(p), f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(hash21(i),             hash21(i + float2(1,0)), f.x),
                            lerp(hash21(i+float2(0,1)), hash21(i + float2(1,1)), f.x), f.y);
            }
            float fbm(float2 p)
            {
                float v = 0.0, a = 0.5;
                for (int i = 0; i < 4; ++i) { v += a * vnoise(p); p *= 2.1; a *= 0.5; }
                return v;
            }
            float2 rot2(float2 v, float a)
            {
                float s, c; sincos(a, s, c);
                return float2(c*v.x - s*v.y, s*v.x + c*v.y);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.posCS = TransformObjectToHClip(IN.posOS);
                OUT.uv    = IN.uv - 0.5;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float  t  = _Time.y;
                float  r  = length(uv);

                float diskR    = _Radius * (1.0 + _PulseAmount * sin(_PulseSpeed * t));
                float diskMask = smoothstep(diskR + _EdgeSoftness, diskR - _EdgeSoftness, r);

                // ── Surface noise ─────────────────────────────────────
                float noise   = fbm(uv * _NoiseScale   + float2(t*0.07,  t*0.05)) * 2.0 - 1.0;
                float granule = vnoise(uv * _GranuleScale + float2(t*0.13, -t*0.09)) * 2.0 - 1.0;
                float tDisk   = saturate(r / max(diskR, 0.0001));
                float tNoise  = saturate(tDisk + noise * _NoiseStrength * (1.0 - tDisk)
                                               + granule * _GranuleStr);

                // ── Disk colour ───────────────────────────────────────
                half3 diskRGB = lerp(
                    lerp(_CoreColor.rgb, _MidColor.rgb, smoothstep(0.0, 0.55, tNoise)),
                    _OuterColor.rgb, smoothstep(0.55, 1.0, tNoise));
                diskRGB *= 1.0 - _LimbDarken * pow(tDisk, 2.5);
                diskRGB  = lerp(diskRGB, half3(1.0, 0.99, 0.95),
                                smoothstep(_CoreRadius, _CoreRadius * 0.4, r));

                // ── Glow ──────────────────────────────────────────────
                // Purely exponential — reaches zero on its own, no hard clip.
                // If you see a cut edge, scale up the sprite object so the quad
                // is larger than the glow's natural falloff distance.
                float  glowDist = max(0.0, r - diskR * 0.85);
                float  glowAmt  = _GlowStrength
                                * exp(-_GlowFalloff * glowDist / max(_GlowRadius, 0.001))
                                * (1.0 - diskMask);
                half3  glowRGB  = _CoronaColor.rgb * glowAmt;

                // ── Primary rays ──────────────────────────────────────
                float2 uvR1 = rot2(uv, _RotateSpeed * t);
                float  ray1 = pow(saturate(cos(atan2(uvR1.y, uvR1.x) * _RayCount * 0.5)),
                                  _RaySharpness);
                ray1 *= smoothstep(diskR * (1.0 + _RayLength), diskR * 1.02, r)
                      * smoothstep(diskR * 0.95, diskR * 1.05, r);

                // ── Secondary rays ────────────────────────────────────
                float ray2 = 0.0;
                if (_SecondRayCount > 0.5)
                {
                    float2 uvR2 = rot2(uv, -_RotateSpeed * 0.7 * t + 0.13);
                    ray2 = pow(saturate(cos(atan2(uvR2.y, uvR2.x) * _SecondRayCount * 0.5)),
                               _RaySharpness * 1.5) * _SecondRayScale;
                    ray2 *= smoothstep(diskR * (1.0 + _RayLength * 0.6), diskR * 1.02, r)
                          * smoothstep(diskR * 0.95, diskR * 1.05, r);
                }

                // ── Fine fringe corona ────────────────────────────────
                float2 uvF    = rot2(uv, _RotateSpeed * 2.3 * t + 0.5);
                float  fringe = pow(saturate(cos(atan2(uvF.y, uvF.x) * 18.0)), 28.0);
                fringe *= smoothstep(diskR * 1.18, diskR * 0.98 + 0.005, r)
                        * smoothstep(diskR * 0.98,  diskR * 0.98 + 0.012, r);

                half3 raysRGB = _CoronaColor.rgb * (ray1 + ray2 + fringe * 0.7)
                              * (1.0 - diskMask * 0.6);

                // ── Composite ─────────────────────────────────────────
                half3 finalRGB = diskRGB * diskMask + glowRGB + raysRGB;

                // Alpha: disk is solid 1. Outside the disk, alpha = glow intensity
                // so it tapers smoothly to zero with no visible boundary.
                // The glow exp() approaches zero asymptotically — the sprite quad
                // just needs to be wider than the visible glow (scale the object).
                half finalA = saturate(diskMask
                            + glowAmt
                            + (ray1 + ray2 + fringe * 0.7) * 0.8);

                return half4(finalRGB, finalA);
            }
            ENDHLSL
        }
    }

    FallBack "Sprites/Default"
}
