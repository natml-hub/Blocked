Shader "Hidden/Blocked" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Map ("Segmentation Map", 2D) = "white" {}
    }
    SubShader {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass {

            CGPROGRAM
            #include "UnityCG.cginc"

            #pragma vertex vert_img
            #pragma fragment frag

            sampler2D _MainTex;
            sampler2D _Map;
            int _Strength;
            float2 _Aspect;

            fixed4 frag (v2f_img i) : SV_Target {
                // Compute pixelation UV
                float2 scale = _Strength * _Aspect;
                float2 pixelationUV = round(i.uv * scale) / scale;
                // Sample
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 pixelatedCol = tex2D(_MainTex, pixelationUV);
                float alpha = tex2D(_Map, i.uv);
                // Blend
                return lerp(col, pixelatedCol, alpha);
            }
            ENDCG
        }
    }
}
