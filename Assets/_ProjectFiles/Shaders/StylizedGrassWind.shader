Shader "Custom/URP_StylizedGrassWind"
{
    Properties
    {
        [Header(Colors and Shading)]
        _BaseColor ("Root Color", Color) = (0.12, 0.28, 0.10, 1.0)
        _TipColor ("Tip Color", Color) = (0.45, 0.68, 0.22, 1.0)
        _SubsurfaceColor ("Backlight / Translucency Color", Color) = (0.65, 0.85, 0.30, 1.0)
        _SubsurfaceStrength ("Translucency Strength", Range(0.0, 2.0)) = 0.6
        _MainTex ("Albedo / Alpha Texture (Optional)", 2D) = "white" {}
        _Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.4
        _NormalUpBias ("Upward Normal Blend (Terrain Match)", Range(0.0, 1.0)) = 0.65

        [Header(Wind Direction and Gusts)]
        _WindDirection ("Primary Wind Direction (XZ)", Vector) = (1.0, 0.0, 0.4, 0.0)
        _WindSpeed ("Base Sway Speed", Float) = 1.8
        _WindStrength ("Base Sway Amplitude", Float) = 0.22
        _GustSpeed ("Rolling Gust Speed", Float) = 0.85
        _GustScale ("Rolling Gust World Scale", Float) = 0.08
        _GustStrength ("Rolling Gust Multiplier", Float) = 0.35

        [Header(Randomness and Turbulence)]
        _PerBladeRandomness ("Per-Blade Phase & Angle Variance", Range(0.0, 3.0)) = 1.35
        _TurbulenceFrequency ("Micro-Flutter Frequency", Float) = 3.7
        _TurbulenceStrength ("Cross-Wind Flutter Strength", Float) = 0.08
        _BendPower ("Blade Stiffness Curve (Root Anchor)", Range(1.0, 4.0)) = 2.0

        [Header(Masking)]
        [Toggle(_USE_VERTEX_COLOR_MASK)] _UseVertexColorMask ("Use Vertex Color R Instead of UV.y", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Cull Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _TipColor;
            float4 _SubsurfaceColor;
            float _SubsurfaceStrength;
            float4 _MainTex_ST;
            float _Cutoff;
            float _NormalUpBias;

            float4 _WindDirection;
            float _WindSpeed;
            float _WindStrength;
            float _GustSpeed;
            float _GustScale;
            float _GustStrength;

            float _PerBladeRandomness;
            float _TurbulenceFrequency;
            float _TurbulenceStrength;
            float _BendPower;
        CBUFFER_END

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        // Fast 2D hash for spatial randomness without needing an external noise texture
        float2 Hash22(float2 p)
        {
            p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
            return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
        }

        // Smooth value noise for rolling wind gusts across the terrain
        float ValueNoise(float2 p)
        {
            float2 i = floor(p);
            float2 f = frac(p);
            float2 u = f * f * (3.0 - 2.0 * f);

            float a = dot(Hash22(i + float2(0.0, 0.0)), f - float2(0.0, 0.0));
            float b = dot(Hash22(i + float2(1.0, 0.0)), f - float2(1.0, 0.0));
            float c = dot(Hash22(i + float2(0.0, 1.0)), f - float2(0.0, 1.0));
            float d = dot(Hash22(i + float2(1.0, 1.0)), f - float2(1.0, 1.0));

            return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y) * 0.5 + 0.5;
        }

        float3 ApplyGrassWind(float3 positionOS, float bendFactor)
        {
            // Anchor root: bendFactor is 0 at base, 1 at tip
            float weight = pow(saturate(bendFactor), _BendPower);
            if (weight <= 0.0001)
                return positionOS;

            float3 worldPos = TransformObjectToWorld(positionOS);
            // Use the object/cluster origin in world space so all vertices of a single blade share a coherent random seed
            float3 objectOriginWS = UNITY_MATRIX_M._m03_m13_m23;
            float2 spatialSeed = objectOriginWS.xz + floor(worldPos.xz * 2.0) * 0.37;

            // Per-blade random phase and directional angle offset
            float2 randVec = Hash22(spatialSeed);
            float phaseOffset = randVec.x * 6.28318 * _PerBladeRandomness;
            float angleJitter = randVec.y * 0.65 * _PerBladeRandomness; // Rotates wind direction per blade

            // Normalize primary wind direction in XZ and apply per-blade angular jitter
            float2 baseDir = normalize(_WindDirection.xz + float2(1e-5, 0.0));
            float sinA = sin(angleJitter);
            float cosA = cos(angleJitter);
            float2 bladeDir = float2(
                baseDir.x * cosA - baseDir.y * sinA,
                baseDir.x * sinA + baseDir.y * cosA
            );
            float2 perpDir = float2(-bladeDir.y, bladeDir.x);

            // 1. Traveling world-space gust wave
            float2 gustUV = worldPos.xz * _GustScale - baseDir * (_Time.y * _GustSpeed);
            float gustMask = ValueNoise(gustUV); // 0..1 rolling wave across the field

            // 2. Primary back-and-forth sway with per-blade phase desync
            float primaryWave = sin(_Time.y * _WindSpeed + phaseOffset + dot(worldPos.xz, baseDir) * 0.6);

            // 3. Secondary high-frequency perpendicular flutter (prevents linear robotic oscillation)
            float flutterWave = cos(_Time.y * _TurbulenceFrequency + phaseOffset * 1.7 + worldPos.x * 1.3);

            // Combine base sway + rolling gust surge
            float totalForward = (primaryWave * _WindStrength) + (gustMask * _GustStrength);
            float totalSideways = flutterWave * _TurbulenceStrength * (0.5 + gustMask);

            float3 worldOffset = float3(
                bladeDir.x * totalForward + perpDir.x * totalSideways,
                0.0,
                bladeDir.y * totalForward + perpDir.y * totalSideways
            ) * weight;

            // Slight downward droop as the blade bends horizontally so it doesn't stretch vertically
            float horizontalDisp = length(worldOffset.xz);
            worldOffset.y -= horizontalDisp * horizontalDisp * 0.35;

            // Convert world displacement back to object space
            float3 displacedWorldPos = worldPos + worldOffset;
            return TransformWorldToObject(displacedWorldPos);
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma shader_feature_local _USE_VERTEX_COLOR_MASK

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 positionWS  : TEXCOORD1;
                float3 normalWS    : TEXCOORD2;
                float  bendFactor  : TEXCOORD3;
                float  fogFactor   : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                #ifdef _USE_VERTEX_COLOR_MASK
                    float bendFactor = input.color.r;
                #else
                    float bendFactor = input.uv.y;
                #endif

                float3 displacedOS = ApplyGrassWind(input.positionOS.xyz, bendFactor);

                VertexPositionInputs posInputs = GetVertexPositionInputs(displacedOS);
                VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);

                // Blend mesh normal toward world-up so grass cards/blades shade smoothly with terrain
                float3 blendedNormalWS = normalize(lerp(normInputs.normalWS, float3(0.0, 1.0, 0.0), _NormalUpBias));

                output.positionCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                output.normalWS = blendedNormalWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.bendFactor = bendFactor;
                output.fogFactor = ComputeFogFactor(posInputs.positionCS.z);
                return output;
            }

            half4 frag(Varyings input, half facing : VFACE) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half4 texSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                clip(texSample.a - _Cutoff);

                // Root-to-tip color gradient
                half3 albedo = lerp(_BaseColor.rgb, _TipColor.rgb, saturate(input.bendFactor)) * texSample.rgb;

                // Flip normal on backfaces for double-sided blades
                float3 normalWS = normalize(input.normalWS) * (facing >= 0 ? 1.0 : -1.0);

                // Main directional light & shadows
                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord);

                half NdotL = saturate(dot(normalWS, mainLight.direction));
                half3 diffuse = mainLight.color * (NdotL * mainLight.shadowAttenuation);

                // Subsurface translucency when looking toward the sun through the grass tips
                float3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half backLight = pow(saturate(dot(viewDirWS, -mainLight.direction)), 3.0);
                half3 translucency = _SubsurfaceColor.rgb * mainLight.color * backLight * _SubsurfaceStrength * saturate(input.bendFactor) * mainLight.shadowAttenuation;

                // Ambient SH lighting + subtle root AO
                half3 ambient = SampleSH(normalWS) * lerp(0.55, 1.0, saturate(input.bendFactor));

                half3 finalColor = albedo * (diffuse + ambient) + translucency;
                finalColor = MixFog(finalColor, input.fogFactor);

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }

        // ShadowCaster Pass: applies the exact same wind displacement so shadows move with the blades
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Off

            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma multi_compile_instancing
            #pragma shader_feature_local _USE_VERTEX_COLOR_MASK

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings ShadowVert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                #ifdef _USE_VERTEX_COLOR_MASK
                    float bendFactor = input.color.r;
                #else
                    float bendFactor = input.uv.y;
                #endif

                float3 displacedOS = ApplyGrassWind(input.positionOS.xyz, bendFactor);
                float3 positionWS = TransformObjectToWorld(displacedOS);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 ShadowFrag(Varyings input) : SV_Target
            {
                half alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a;
                clip(alpha - _Cutoff);
                return 0;
            }
            ENDHLSL
        }
    }
}