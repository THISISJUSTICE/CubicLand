Shader "Custom/GolemCubeLit" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Main Texture", 2D) = "white" {}
        _NormalTex ("Normal Map", 2D) = "bump" {}

        _CrackTex ("Crack Texture", 2D) = "white" {}
        _CrackNormalTex ("Crack Normal Map", 2D) = "bump" {}
        _Range ("Crack Range", Range(0, 1)) = 0
        _FadeAlpha ("Fade Alpha", Range(0, 1)) = 0.3
    }

    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Range;
                float _FadeAlpha;
            CBUFFER_END

            TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);
            TEXTURE2D(_NormalTex);  SAMPLER(sampler_NormalTex);
            TEXTURE2D(_CrackTex);   SAMPLER(sampler_CrackTex);
            TEXTURE2D(_CrackNormalTex); SAMPLER(sampler_CrackNormalTex);

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.uv2 = IN.uv2;
                OUT.normalWS = normalize(TransformObjectToWorldNormal(IN.normalOS));
                return OUT;
            }

            float3 BlendNormals(float3 n1, float3 n2, float t)
            {
                n1 = normalize(n1);
                n2 = normalize(n2);
                return normalize(lerp(n1, n2, t));
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float2 uv_Main = IN.uv;
                float2 uv_Crack = IN.uv2;

                // 기본 텍스처
                float4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv_Main);
                float3 baseNormal = UnpackNormal(SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, uv_Main));
                
                float3 finalColor = baseColor.rgb * _Color.rgb;
                float finalAlpha = baseColor.a * _Color.a;
                float3 finalNormal = baseNormal;

                // 크랙 영역 계산
                float2 centeredUV = uv_Crack - 0.5;
                float distance = length(centeredUV);

                float4 crackColor = SAMPLE_TEXTURE2D(_CrackTex, sampler_CrackTex, uv_Crack);
                crackColor.rgb *= _Color.rgb;
                float3 crackNormal = UnpackNormal(SAMPLE_TEXTURE2D(_CrackNormalTex, sampler_CrackNormalTex, uv_Crack));

                if (distance <= _Range)
                {
                    float mask = smoothstep(_Range - 0.05, _Range, distance);
                    finalNormal = BlendNormals(baseNormal, crackNormal, 1.0 - mask);
                    finalColor = lerp(finalColor, crackColor.rgb, _FadeAlpha);
                }
                else
                {
                    finalNormal = baseNormal;
                    finalColor = lerp(finalColor, crackColor.rgb, _FadeAlpha);
                }

                // 수정된 라이트 가져오기
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float3 lightColor = mainLight.color;

                float NdotL = saturate(dot(normalize(IN.normalWS), lightDir));

                float3 lighting = lightColor * NdotL;
                
                // Ambient 추가 (없으면 검정색 됨)
                lighting += 0.1; // 기본 최소 밝기 (없으면 완전 깜깜해짐)

                float3 color = finalColor * lighting;

                return float4(color, finalAlpha);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
