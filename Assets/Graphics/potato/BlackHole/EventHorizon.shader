Shader "Custom/BlackHole/EventHorizon"
{
    Properties
    {
        _MainTex          ("Sprite",                 2D)           = "white" {}
        _InnerColor       ("Photon Ring Color",      Color)        = (1.0, 0.92, 0.5, 1)
        _OuterColor       ("Disk Outer Color",       Color)        = (0.6, 0.2, 0.9, 1)
        _HorizonRadius    ("Event Horizon Radius",   Range(0.02, 0.25)) = 0.08
        _PhotonRingWidth  ("Photon Ring Width",      Range(0.003, 0.05)) = 0.015
        _GapWidth         ("Gap Width",              Range(0.005, 0.08)) = 0.03
        _DiskMaxRadius    ("Disk Outer Radius",      Range(0.2,  0.5))   = 0.45
        _DiskFalloffPower ("Disk Falloff",           Range(0.5,  5.0))   = 2.0
        _RotationSpeed    ("Rotation Speed",         Float)        = 2.0
    }

    SubShader
    {
        // Render after all opaque geometry, standard transparent queue
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

        // Standard alpha blend — NOT additive.
        // The black disc works because black with alpha=1 simply replaces whatever
        // is behind it (dst * (1-1) + src * 1 = src = black).
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
                float4 _InnerColor;
                float4 _OuterColor;
                float  _HorizonRadius;
                float  _PhotonRingWidth;
                float  _GapWidth;
                float  _DiskMaxRadius;
                float  _DiskFalloffPower;
                float  _RotationSpeed;
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

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.posCS = TransformObjectToHClip(IN.posOS);
                OUT.uv    = IN.uv - 0.5; // centre UVs so origin = black hole centre
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float  r  = length(uv);

                // ── Precompute zone boundaries ────────────────────────────────────
                float rRingOuter = _HorizonRadius + _PhotonRingWidth;
                float rGapOuter  = rRingOuter     + _GapWidth;
                float rDiskOuter = _DiskMaxRadius;

                // ═════════════════════════════════════════════════════════════════
                // ZONE 1 — Event Horizon  (solid black, alpha = 1)
                // ═════════════════════════════════════════════════════════════════
                if (r < _HorizonRadius)
                    return half4(0.0, 0.0, 0.0, 1.0);

                // ═════════════════════════════════════════════════════════════════
                // ZONE 2 — Photon Ring  (bright, thin emission band)
                // ═════════════════════════════════════════════════════════════════
                if (r < rRingOuter)
                {
                    // Normalise position within ring width  (0 = inner, 1 = outer)
                    float t    = (r - _HorizonRadius) / _PhotonRingWidth;
                    // Bell curve: dim at both edges, bright in middle
                    float bell = smoothstep(0.0, 0.45, t) * smoothstep(1.0, 0.55, t);
                    // Overbright colour — still clamped at display but looks hot
                    half3 col  = _InnerColor.rgb * (1.0 + 3.0 * bell);
                    return half4(saturate(col), saturate(bell * 0.9 + 0.1));
                }

                // ═════════════════════════════════════════════════════════════════
                // ZONE 3 — Transparent Gap  (alpha = 0, shows background)
                // ═════════════════════════════════════════════════════════════════
                if (r < rGapOuter)
                    return half4(0.0, 0.0, 0.0, 0.0);

                // ═════════════════════════════════════════════════════════════════
                // ZONE 4 — Accretion Disk  (animated spiral, radial fade)
                // ═════════════════════════════════════════════════════════════════
                if (r < rDiskOuter)
                {
                    float theta       = atan2(uv.y, uv.x);
                    float time        = _Time.y * _RotationSpeed;
                    float spiralAngle = theta + r * 8.0 - time;

                    // Layered swirl bands
                    float swirl = sin(spiralAngle * 3.5) * cos(spiralAngle * 1.5);
                    swirl = smoothstep(-0.5, 1.0, swirl); // remap to [0,1]

                    // Radial gradient: 0 at inner edge, 1 at outer edge
                    float span  = max(rDiskOuter - rGapOuter, 0.0001);
                    float tDisk = saturate((r - rGapOuter) / span);

                    // Brightness falls off toward outer edge
                    float fade = pow(1.0 - tDisk, _DiskFalloffPower);

                    // Soft ramp at inner boundary — no hard pop
                    float innerRamp = smoothstep(rGapOuter, rGapOuter + 0.02, r);

                    // Colour: warm photon-ring hue → cool outer disk hue
                    half3 col = lerp(_InnerColor.rgb, _OuterColor.rgb, tDisk);
                    col *= (0.5 + 1.5 * swirl);

                    float alpha = swirl * fade * innerRamp;
                    return half4(col, saturate(alpha));
                }

                // ═════════════════════════════════════════════════════════════════
                // Outside disk — fully transparent
                // ═════════════════════════════════════════════════════════════════
                return half4(0.0, 0.0, 0.0, 0.0);
            }
            ENDHLSL
        }
    }

    FallBack "Sprites/Default"
}
