#ifndef VERTIGO_STRUCT_INCLUDE
#define VERTIGO_STRUCT_INCLUDE

struct appdata {
    float4 vertex : POSITION;
    float4 texCoord0 : TEXCOORD0;
    float4 texCoord1 : TEXCOORD1;
    fixed4 color : COLOR;
};

struct v2f {
    float4 vertex : SV_POSITION;
    float4 texCoord0 : TEXCOORD0;
    float4 sdfCoord  : TEXCOORD1;
    float4 sdfData   : TEXCOORD2;
    float4 sdfRadii  : TEXCOORD3;
    float4 sdfParams  : TEXCOORD4;
    fixed4 color : COLOR;
};

#endif 