Shader "Vertigo/Default"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags {
            "Queue"="Transparent"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
        Cull Off
        ZWrite Off
        ZClip Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "./Vertigo.cginc"
            #include "./VertigoBlur.cginc"
            #include "./VertigoColorEffect.cginc"

            float4 _MainTex_ST;
            sampler2D _MainTex;
            sampler2D _ParameterTex;
            fixed4 _Color;
            
            // Base Effect
            //  effect factor [0, 1]
            //  color factor = [0, 1]
            //  blur factor = [0, 1]
            //  effect mode = enum
            //  color mode = enum
            //  blur node = enum
            //  advanced blur = bool
            //  shadow blur = [0, 1]
            //  shadow style = enum
            //  shadow color = color
            //  effect color = color
            // Dissolve
            //  effect factor [0, 1]
            //  edge width [0, 1]
            //  edge softness [0, 1]
            //  edge color uint
            //  color mode = enum
            //  noise texture = sampler2D
            //  reversed = bool
            // HSV
            //   target color
            //   shift range [0, 1]
            //   hue -0.5, 0.5
            //   saturation -0.5, 0.5
            //   value -0.5 0.5
            // Shadow
            //  blur [0, 1]
            //  effect color = color
            //  use graphic alpah = bool
            // Shine
            //  effect factor [0, 1]
            //  width = [0, 1]
            //  rotation = [-180, 180]
            //  brightness = [0, 1]
            //  softness = [0, 1]
            //  gloss = [0, 1]
            // Transition
            //  effect mode = enum
            //  effect factor = [0, 1]
            //  disolveWidth = [0, 1]
            //  disolveSoftness = [0, 1]
            //  disolve color = color
            //  transition texture = sampler2D
            // Gradient
            //  colors Color[4]
            //  rotation[-180, 180] -> can reduce to 0,1
            //  offset1 = [-1, 1]
            //  offset2 = [-1, 1]
            //  color space
            
            #define VERTIGO_TRANSFORM_TEX(tex,name) (tex.xy * name##_ST.xy + name##_ST.zw)

            
            sampler2D _VertigoParameterTexture;
            int _VertigoParameterTexture_Width;
            int _VertigoParameterTexture_Height;
            float4 _MainTex_TexelSize;
            fixed4 _TextureSampleAdd;
            
            struct AppData {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                fixed4 color : COLOR;
                float3 normal : NORMAL;
                float4 vertData0 : TEXCOORD1;
                float4 vertData1 : TEXCOORD2;
            };

            struct v2f {
                float4 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float4 effectParams :TEXCOORD1; 
            };

            v2f vert (AppData v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv.xy = TRANSFORM_TEX(v.uv.xy, _MainTex);
                fixed4 color = UnpackColor(asuint(v.uv.z));
                
                o.color = color;
                o.effectParams = v.uv.w;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                
                half2 blur = i.effectParams.xy * _MainTex_TexelSize.xy * 2;
            #if VERTIGO_FEATURE_BLUR
                fixed4 c = Texture2DBlur7x7(_MainTex, i.uv.xy, blur);
                c += + _TextureSampleAdd;
            #endif
                c.rgb = i.color.rgb;
                
            #if VERTIGO_FEATURE__COLOR_EFFECT
                c = ApplyFillColorEffect(c, fixed4(i.color.rgb, 1), 0.5);
            #endif
                
               // c.a *= i.color.a;
                
                return c;
            }
            ENDCG
        }
    }
}
