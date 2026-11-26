Shader "Custom/StylizedDirtGroundURP"
{
    Properties
    {
        [Header(Base Dirt Palette)]
        _BaseDirtColor ("Base Dirt Color", Color) = (0.34, 0.26, 0.19, 1)
        _DryPatchColor ("Dry Packed Soil Color", Color) = (0.47, 0.37, 0.27, 1)
        _DarkSoilColor ("Deep Earth Color", Color) = (0.20, 0.15, 0.11, 1)
        _MacroScale ("Macro Color Patch Scale", Float) = 0.16
        _DetailNoiseScale ("Micro Grain Scale", Float) = 2.8
        _PainterlyContrast ("Painterly Blend Sharpness", Range(0.05, 1.0)) = 0.45

        [Header(Optional Detail Texture Fallback)]
        [NoScaleOffset] _DetailMap ("Detail Texture (Optional - Falls Back to Colors)", 2D) = "white" {}
        _TextureScale ("Texture World Tiling", Float) = 0.45
        _TextureBlend ("Texture Detail Influence", Range(0.0, 1.0)) = 0.35

        [Header(Crack Regions)]
        _CrackColor ("Crack Crevice Color", Color) = (0.11, 0.08, 0.06, 1)
        _CrackZoneScale ("Crack Region Scale", Float) = 0.11
        _CrackZoneThreshold ("Crack Region Coverage", Range(0.0, 1.0)) = 0.48
        _CrackCellScale ("Crack Pattern Scale", Float) = 1.5
        _CrackWidth ("Crack Line Width", Range(0.01, 0.25)) = 0.06
        _CrackDepth ("Crack Normal Depth", Range(0.0, 2.5)) = 1.1

        [Header(Scratch and Scuff Regions)]
        _ScratchColor ("Scratch Scuff Color", Color) = (0.53, 0.43, 0.32, 1)
        _ScratchZoneScale ("Scratch Region Scale", Float) = 0.13
        _ScratchZoneThreshold ("Scratch Region Coverage", Range(0.0, 1.0)) = 0.50
        _ScratchScale ("Scratch Density Scale", Float) = 3.0
        _ScratchStretch ("Scratch Elongation", Range(1.0, 12.0)) = 6.5
        _ScratchAngle ("Scratch Angle (Radians)", Range(0.0, 3.14159)) = 0.65
        _ScratchDepth ("Scratch Normal Depth", Range(0.0, 2.5)) = 0.7

        [Header(Surface Finish)]
        _BaseSmoothness ("Dry Dirt Smoothness", Range(0.0, 0.45)) = 0.10
        _PackedSmoothness ("Scuffed / Packed Smoothness", Range(0.0, 0.55)) = 0.20
        _BumpScale ("Micro Grain Bump Strength", Range(0.0, 1.5)) = 0.28

        // Hidden URP pass compatibility properties
        [HideInInspector] _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            float4 _BaseMap_ST;
            half _Cutoff;

            half4 _BaseDirtColor;
            half4 _DryPatchColor;
            half4 _DarkSoilColor;
            float _MacroScale;
            float _DetailNoiseScale;
            float _PainterlyContrast;

            float _TextureScale;
            float _TextureBlend;

            half4 _CrackColor;
            float _CrackZoneScale;
            float _CrackZoneThreshold;
            float _CrackCellScale;
            float _CrackWidth;
            float _CrackDepth;

            half4 _ScratchColor;
            float _ScratchZoneScale;
            float _ScratchZoneThreshold;
            float _ScratchScale;
            float _ScratchStretch;
            float _ScratchAngle;
            float _ScratchDepth;

            float _BaseSmoothness;
            float _PackedSmoothness;
            float _BumpScale;
        CBUFFER_END

        TEXTURE2D(_DetailMap);
        SAMPLER(sampler_DetailMap);

        float Hash21(float2 p)
        {
            float3 p3 = frac(float3(p.xyx) * 0.1031);
            p3 += dot(p3, p3.yzx + 33.33);
            return frac((p3.x + p3.y) * p3.z);
        }

        float2 Hash22(float2 p)
        {
            float3 p3 = frac(float3(p.xyx) * float3(0.1031, 0.1030, 0.0973));
            p3 += dot(p3, p3.yzx + 33.33);
            return frac((p3.xx + p3.yz) * p3.zy);
        }

        float ValueNoise(float2 p)
        {
            float2 i = floor(p);
            float2 f = frac(p);
            float2 u = f * f * (3.0 - 2.0 * f);

            float a = Hash21(i);
            float b = Hash21(i + float2(1.0, 0.0));
            float c = Hash21(i + float2(0.0, 1.0));
            float d = Hash21(i + float2(1.0, 1.0));

            return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
        }

        float FBM(float2 p)
        {
            float value = 0.0;
            float amp = 0.5;
            float2x2 rot = float2x2(0.80, -0.60, 0.60, 0.80);

            [unroll]
            for (int i = 0; i < 4; i++)
            {
                value += amp * ValueNoise(p);
                p = mul(rot, p) * 2.03 + float2(17.1, 31.7);
                amp *= 0.5;
            }
            return value;
        }

        float VoronoiEdgeDistance(float2 x)
        {
            float2 n = floor(x);
            float2 f = frac(x);

            float2 mg, mr;
            float md = 8.0;

            [unroll]
            for (int j = -1; j <= 1; j++)
            {
                [unroll]
                for (int i = -1; i <= 1; i++)
                {
                    float2 g = float2((float)i, (float)j);
                    float2 o = Hash22(n + g);
                    float2 r = g + o - f;
                    float d = dot(r, r);

                    if (d < md)
                    {
                        md = d;
                        mr = r;
                        mg = g;
                    }
                }
            }

            md = 8.0;
            [unroll]
            for (int j2 = -1; j2 <= 1; j2++)
            {
                [unroll]
                for (int i2 = -1; i2 <= 1; i2++)
                {
                    float2 g = mg + float2((float)i2, (float)j2);
                    float2 o = Hash22(n + g);
                    float2 r = g + o - f;

                    if (dot(mr - r, mr - r) > 0.00001)
                    {
                        md = min(md, dot(0.5 * (mr + r), normalize(r - mr)));
                    }
                }
            }

            return md;
        }

        float2 Rotate2D(float2 p, float angle)
        {
            float s, c;
            sincos(angle, s, c);
            return float2(c * p.x - s * p.y, s * p.x + c * p.y);
        }

        float ComputeScratches(float2 uv)
        {
            float2 warp = float2(FBM(uv * 0.45), FBM(uv * 0.45 + float2(9.2, 4.7))) * 0.65;
            float2 p1 = Rotate2D(uv + warp, _ScratchAngle);
            p1.x /= max(1.0, _ScratchStretch);

            float n1 = ValueNoise(p1 * _ScratchScale);
            float n2 = ValueNoise(p1 * (_ScratchScale * 2.1) + float2(5.3, 1.9));
            float streak1 = smoothstep(0.68, 0.92, n1 * 0.65 + n2 * 0.35);

            float2 p2 = Rotate2D(uv - warp * 0.5, _ScratchAngle + 0.42);
            p2.x /= max(1.0, _ScratchStretch * 0.85);
            float n3 = ValueNoise(p2 * (_ScratchScale * 1.35) + float2(13.4, 7.1));
            float streak2 = smoothstep(0.72, 0.94, n3);

            return saturate(max(streak1, streak2 * 0.75));
        }

        float3 PerturbNormalWorld(float height, float3 normalWS, float3 positionWS, float strength)
        {
            float3 dpdx = ddx(positionWS);
            float3 dpdy = ddy(positionWS);
            float dhdx = ddx(height);
            float dhdy = ddy(height);

            float3 r1 = cross(dpdy, normalWS);
            float3 r2 = cross(normalWS, dpdx);
            float det = dot(dpdx, r1);

            if (abs(det) < 1e-7) return normalWS;

            float3 surfGrad = sign(det) * (dhdx * r1 + dhdy * r2);
            return normalize(abs(det) * normalWS - strength * surfGrad);
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 staticLightmapUV : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 2);
                float4 fogFactorAndVertexLight : TEXCOORD3;
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);

                output.positionCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                output.normalWS = NormalizeNormalPerVertex(normInputs.normalWS);

                OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
                OUTPUT_SH(output.normalWS, output.vertexSH);

                half3 vertexLight = VertexLighting(posInputs.positionWS, output.normalWS);
                half fogFactor = ComputeFogFactor(posInputs.positionCS.z);
                output.fogFactorAndVertexLight = float4(fogFactor, vertexLight);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 normalWS = NormalizeNormalPerPixel(input.normalWS);
                float2 worldUV = input.positionWS.xz;

                // 1. Macro dirt color blending
                float macroNoise = FBM(worldUV * _MacroScale);
                float midDetail = FBM(worldUV * (_MacroScale * 3.1) + float2(4.2, 8.6));
                float combinedMacro = saturate(macroNoise * 0.72 + midDetail * 0.28);

                float halfSharp = (1.0 - _PainterlyContrast) * 0.45;
                float dryMask = smoothstep(0.55 - halfSharp, 0.55 + halfSharp, combinedMacro);
                float darkMask = smoothstep(0.42 + halfSharp, 0.42 - halfSharp, combinedMacro);

                half3 albedo = _BaseDirtColor.rgb;
                albedo = lerp(albedo, _DryPatchColor.rgb, dryMask);
                albedo = lerp(albedo, _DarkSoilColor.rgb, darkMask * 0.85);

                float microGrain = ValueNoise(worldUV * _DetailNoiseScale);
                albedo *= lerp(0.92, 1.08, microGrain);

                // 2. Optional Detail Texture (falls back 100% to color fields when None)
                half4 texSample = SAMPLE_TEXTURE2D(_DetailMap, sampler_DetailMap, worldUV * _TextureScale);
                albedo = lerp(albedo, albedo * texSample.rgb, _TextureBlend);

                // 3. Localized Crack Zones
                float crackRegionNoise = FBM(worldUV * _CrackZoneScale + float2(21.4, 11.8));
                float crackRegionMask = smoothstep(_CrackZoneThreshold - 0.12, _CrackZoneThreshold + 0.12, crackRegionNoise);

                float2 crackWarp = float2(ValueNoise(worldUV * 0.8), ValueNoise(worldUV * 0.8 + float2(6.1, 3.4))) * 0.35;
                float voronoiEdge = VoronoiEdgeDistance((worldUV + crackWarp) * _CrackCellScale);
                float crackLine = 1.0 - smoothstep(_CrackWidth * 0.35, _CrackWidth, voronoiEdge);
                float crackMask = crackLine * crackRegionMask;

                albedo = lerp(albedo, _CrackColor.rgb, crackMask * 0.9);

                // 4. Localized Scratch / Scuff Zones
                float scratchRegionNoise = FBM(worldUV * _ScratchZoneScale + float2(53.7, 39.2));
                float scratchRegionMask = smoothstep(_ScratchZoneThreshold - 0.12, _ScratchZoneThreshold + 0.12, scratchRegionNoise) * (1.0 - crackRegionMask * 0.65);

                float scratchLines = ComputeScratches(worldUV);
                float scratchMask = scratchLines * scratchRegionMask;

                albedo = lerp(albedo, _ScratchColor.rgb, scratchMask * 0.72);

                // 5. Procedural Height -> World Normal Perturbation
                float totalHeight = microGrain * _BumpScale
                                  - crackMask * _CrackDepth
                                  - scratchMask * _ScratchDepth;
                normalWS = PerturbNormalWorld(totalHeight, normalWS, input.positionWS, 0.35);

                // 6. Dry Earth Smoothness
                float smoothness = lerp(_BaseSmoothness, _PackedSmoothness, saturate(dryMask * 0.5 + scratchMask));
                smoothness = lerp(smoothness, 0.02, crackMask);

                // 7. URP PBR Lighting
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                inputData.fogCoord = input.fogFactorAndVertexLight.x;
                inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
                inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo;
                surfaceData.metallic = 0.0;
                surfaceData.specular = half3(0.0, 0.0, 0.0);
                surfaceData.smoothness = smoothness;
                surfaceData.normalTS = half3(0.0, 0.0, 1.0);
                surfaceData.occlusion = lerp(1.0, 0.45, crackMask);
                surfaceData.alpha = 1.0;

                half4 finalColor = UniversalFragmentPBR(inputData, surfaceData);
                finalColor.rgb = MixFog(finalColor.rgb, inputData.fogCoord);
                return finalColor;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }

            ZWrite On

            HLSLPROGRAM
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthNormalsPass.hlsl"
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}