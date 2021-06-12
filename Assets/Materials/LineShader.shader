Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Colour ("Colour", Color) = (1, 1, 1, 1)
        _Amplitude("Amplitude", Float) = 1
        _Speed("Speed", Float) = 1
        _Amount("Amount", Range(0.0,1.0)) = 1
        _AmountX("AmountX", Range(0.0,1.0)) = 1
        _AmountY("AmountY", Range(0.0,1.0)) = 1
        _FixX("FixX", Float) = 1
        _FixY("FixY", Float) = 1
    }
    SubShader
    {
        Tags{"Queue" = "Transparent"}
        LOD 100

        Pass
        {
            ZTest Off
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Colour;
            float _Amplitude;
            float _Speed;
            float _Amount;
            float _AmountX;
            float _AmountY;
            float _FixX;
            float _FixY;

            v2f vert (appdata v)
            {
                v2f o;
                float originalY = v.vertex.y;
                float originalX = v.vertex.x;
                if (!abs(originalY - _FixY) < 0.4f || !abs(originalX - _FixX) < 0.4f)
                {
                    v.vertex.x += sin(_Time.y * _Speed + v.vertex.y * _Amplitude) * _Amount * _AmountX;
                    v.vertex.y += sin(_Time.y * _Speed + v.vertex.x * _Amplitude) * _Amount * _AmountY;
                }
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) + _Colour;
                return col;
            }
            ENDCG
        }
    }
}
