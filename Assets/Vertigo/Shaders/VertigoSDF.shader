Shader "Vertigo/VertigoSDF"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskTexture ("Mask", 2D) = "white" {}
        _MaskSoftness ("Mask",  Range (0.001, 1)) = 0
        _Radius ("Radius",  Range (1, 200)) = 0
        [Toggle]
        _InvertMask ("Invert Mask",  Int) = 0
    }
    SubShader
    {
        Tags {
         "RenderType"="Transparent"
         "Queue" = "Transparent"
        }
        LOD 100
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        // this stencil setting solves self-blending
        // does mean we have to issue the draw call twice probably
        // if we want to reset the stencil
//        Stencil {
//            Ref 0
//            Comp Equal
//            Pass IncrSat 
//            Fail IncrSat
//        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile __ SDF_RECTLIKE SDF_TRIANGLE SDF_TEXT SDF_RHOMBUS
            
            #include "UnityCG.cginc"
            #include "./VertigoStructs.cginc"
            #include "./VertigoSDFUtil.cginc"
            
            sampler2D _MainTex;
            sampler2D _MaskTexture;
            float4 _MainTex_ST;
            float4 _Color;
            float _Radius;
            float _MaskSoftness;
            float _InvertMask;
            
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.texCoord0 = v.texCoord0;
                o.sdfCoord = UnpackSDFCoordinates(v.texCoord1.z, v.texCoord1.w);
                o.sdfRadii = UnpackSDFRadii(v.texCoord1.x); 
                o.sdfParams = UnpackSDFParameters(v.texCoord1.y);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target {
                return SDFColor(i.sdfRadii, i.sdfCoord.zw, i.sdfCoord.xy, i.color);
            }
            
             //   fixed maskAlpha = saturate(tex2D(_MaskTexture, i.texCoord0.xy).a / _MaskSoftness);
            //    maskAlpha = lerp(1 - maskAlpha, maskAlpha, _InvertMask);
                
//            fixed4 color;
//            #if COLOR_ONLY
//                color =  fixed4(1, 0, 0, 1); //i.color;
//            #else
//                color = tex2D(_MainTex, i.texCoord0.xy);
//            #endif
//               // color.a *= maskAlpha;
//                return color;
//            }
            ENDCG
        }
    }
}
