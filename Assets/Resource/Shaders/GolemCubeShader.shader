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
        Tags { "RenderType"="Opaque" }
        LOD 300

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        struct Input {
            float2 uv_MainTex;
            float2 uv_CrackTex;
        };

        fixed4 _Color;
        sampler2D _MainTex;
        sampler2D _NormalTex;

        sampler2D _CrackTex;
        sampler2D _CrackNormalTex;
        float _Range;
        float _FadeAlpha;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            fixed4 texColor = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = texColor.rgb * _Color.rgb;
            o.Alpha = texColor.a * _Color.a;
            fixed3 baseNormal = UnpackNormal(tex2D(_NormalTex, IN.uv_MainTex));

            // Crack
            fixed2 uv = IN.uv_CrackTex - 0.5;
            float distance = length(uv);

            if (distance <= _Range) {
                fixed4 crackColor = tex2D(_CrackTex, IN.uv_CrackTex);
                crackColor.rgb *= _Color.rgb;
                float mask = smoothstep(_Range - 0.05, _Range, distance);

                fixed3 crackNormal = UnpackNormal(tex2D(_CrackNormalTex, IN.uv_CrackTex));
                o.Normal = normalize(lerp(baseNormal, crackNormal, 1.0 - mask));

                o.Albedo = lerp(o.Albedo, crackColor.rgb, _FadeAlpha);
            }
            else{
                 fixed4 crackColor = tex2D(_CrackTex, IN.uv_CrackTex);
                crackColor.rgb *= _Color.rgb;

                o.Albedo = lerp(o.Albedo, crackColor.rgb, _FadeAlpha);
                o.Normal = baseNormal;
            }
        }
        ENDCG
    }
    FallBack "Standard"
}
