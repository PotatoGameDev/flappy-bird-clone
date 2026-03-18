Shader "Custom/BlackHole/GravitationalLens"
{
    Properties
    {
        _BlitTexture    ("Screen Texture", 2D) = "white" {}

        // ── Set by BlackHoleLens.cs every frame — do not edit in Inspector ────────
        _BlackHolePos   ("Black Hole Viewport Position (XY)", Vector) = (0.5, 0.5, 0, 0)
        _DeadzoneRadius ("Deadzone Radius (viewport, auto)", Float) = 0.03
        _FadeRadius     ("Fade-out Radius (viewport, auto)", Float) = 0.35

        // ── Tweak freely in the Inspector ─────────────────────────────────────────
        _LensStrength   ("Lens Strength",           Range(0.0, 0.5)) = 0.1
        _LensFalloff    ("Falloff Sharpness",        Range(1.0, 8.0)) = 2.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "GravitationalLens"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BlackHolePos;
                float  _LensStrength;
                float  _FadeRadius;
                float  _LensFalloff;
                float  _DeadzoneRadius;
            CBUFFER_END

            struct LensV { float4 posCS : SV_POSITION; float2 uv : TEXCOORD0; };

            LensV vert(Attributes IN)
            {
                LensV OUT;
                OUT.posCS = GetFullScreenTriangleVertexPosition(IN.vertexID);
                OUT.uv    = GetFullScreenTriangleTexCoord(IN.vertexID);
                return OUT;
            }

            half4 frag(LensV IN) : SV_Target
            {
                float2 uv    = IN.uv;
                float2 bhPos = _BlackHolePos.xy;

                float  aspect = _ScreenParams.x / _ScreenParams.y;
                float2 delta  = uv - bhPos;
                delta.x      *= aspect;
                float  dist   = length(delta);

                // Envelope:
                // - 0 inside deadzone (sprite covers this)
                // - ramps to 1 just outside deadzone
                // - fades to 0 at FadeRadius
                // Result: effect is strongest right outside the sprite, vanishes at FadeRadius
                float innerRamp = smoothstep(_DeadzoneRadius, _DeadzoneRadius * 1.5 + 0.001, dist);
                float outerFade = pow(1.0 - saturate((dist - _DeadzoneRadius) / max(_FadeRadius - _DeadzoneRadius, 0.0001)), _LensFalloff);
                float envelope  = innerRamp * outerFade;

                float2 dir  = delta / max(dist, 0.0001);
                dir.x      /= aspect;

                float2 warpedUV = uv - dir * (envelope * _LensStrength);

                if (warpedUV.x < 0.0 || warpedUV.x > 1.0 ||
                    warpedUV.y < 0.0 || warpedUV.y > 1.0)
                    return half4(0.0, 0.0, 0.0, 1.0);

                return SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, warpedUV);
            }
            ENDHLSL
        }
    }
}
