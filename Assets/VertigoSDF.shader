Shader "Vertigo/VertigoSDF"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float4 texCoord0 : TEXCOORD0;
                float4 texCoord1 : TEXCOORD1;
                fixed4 color : COLOR;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float4 texCoord0 : TEXCOORD0;
                float4 texCoord1 : TEXCOORD1;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.texCoord0 = float4(TRANSFORM_TEX(v.texCoord0.xy, _MainTex).xy, 0, 0);
                o.texCoord1 = o.texCoord0;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.texCoord0.xy);
//                return col;
//                return col;
                return fixed4(1, 0, 0, 1);
            }
            ENDCG
        }
    }
}
