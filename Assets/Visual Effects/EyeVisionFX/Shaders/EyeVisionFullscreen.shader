Shader "GameFX/EyeVisionFullscreen"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "EyeVisionFullscreen"
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag

            // Core URP declarations and built-in full-screen blit helpers.
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // These are global shader values supplied by EyeVisionFXController.
            float _EyeFXBlink;       // 0 = fully open, 1 = fully shut
            float _EyeFXRedness;     // 0..1
            float _EyeFXBlurPixels;  // blur radius in pixels
            float _EyeFXDarkness;    // 0..1
            

            half3 ReadScene(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X_LOD(
                    _BlitTexture, sampler_LinearClamp, saturate(uv), 0).rgb;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord.xy;
                float blur = max(0.0, _EyeFXBlurPixels);
                half3 sceneColor = ReadScene(uv);

                // Nine-tap, single-pass approximation of Gaussian blur.
                // The zero-blur branch avoids unnecessary texture samples.
                if (blur > 0.01)
                {
                    float2 d = _BlitTexture_TexelSize.xy * blur;
                    half3 sum = sceneColor * 4.0h;
                    sum += ReadScene(uv + float2(d.x, 0.0)) * 2.0h;
                    sum += ReadScene(uv - float2(d.x, 0.0)) * 2.0h;
                    sum += ReadScene(uv + float2(0.0, d.y)) * 2.0h;
                    sum += ReadScene(uv - float2(0.0, d.y)) * 2.0h;
                    sum += ReadScene(uv + float2(d.x, d.y));
                    sum += ReadScene(uv + float2(d.x, -d.y));
                    sum += ReadScene(uv + float2(-d.x, d.y));
                    sum += ReadScene(uv - float2(d.x, d.y));
                    sceneColor = sum / 16.0h;
                }

                float red = saturate(_EyeFXRedness);
                float darkness = saturate(_EyeFXDarkness);
                float blink = saturate(_EyeFXBlink);

                // Red-tinted vision: maintain some image information during damage.
                half3 woundedColor = sceneColor * half3(0.92, 0.16, 0.14)
                                   + half3(0.17, 0.012, 0.012);
                sceneColor = lerp(sceneColor, woundedColor, red);

                // Darken the outside of the visual field while wounded/blurry.
                float2 fromCenter = (uv - 0.5) * float2(1.3, 1.0);
                float vignette = smoothstep(0.22, 0.76, length(fromCenter));
                float vignetteStrength = saturate(red * 0.35 + blur * 0.012);
                sceneColor *= (1.0 - vignette * vignetteStrength);
                sceneColor *= (1.0 - darkness);

                // Two soft eyelids slide inward from the top and bottom.
                // y=0 at center, y=1 at the edges; threshold moves to center.
                float y = abs(uv.y - 0.5) * 2.0;
                const float edgeSoftness = 0.036;
                float eyelidBoundary = 1.0 - blink;
                float covered = smoothstep(
                    eyelidBoundary - edgeSoftness,
                    eyelidBoundary + edgeSoftness,
                    y);

                // At the endpoints: completely transparent / completely black.
                covered *= smoothstep(0.0, 0.02, blink);
                covered = lerp(covered, 1.0, smoothstep(0.94, 1.0, blink));
                sceneColor = lerp(sceneColor, half3(0, 0, 0), covered);

                return half4(sceneColor, 1.0);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
