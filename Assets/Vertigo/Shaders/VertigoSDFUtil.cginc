#ifndef VERTIGO_SDF_INCLUDE
#define VERTIGO_SDF_INCLUDE

#include "./VertigoStructs.cginc"

struct SDFData {
    float radius;
    uint shapeType;
    float2 center;
    float2 size;
};

float RectSDF(float2 p, float2 b, float r) {
   float2 d = abs(p) - b + float2(r, r);
   return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - r;   
}

float EllipseSDF(float2 p, float2 r) {
    float k0 = length(p/r);
    float k1 = length(p/(r*r));
    return k0*(k0-1.0)/k1;
}

// https://www.shadertoy.com/view/MtScRG
// todo figure out parameters and coloring, this probably is returning alpha value that needs to be smoothsteped
float PolygonSDF(float2 p, int vertexCount, float radius) {
    // two pi
    float segmentAngle = 6.28318530718 / (float)vertexCount;
    float halfSegmentAngle = segmentAngle*0.5;

    float angleRadians = atan2(p.y, p.x);
    float repeat = (angleRadians % segmentAngle) - halfSegmentAngle;
    float inradius = radius*cos(halfSegmentAngle);
    float circle = length(p);
    float x = sin(repeat)*circle;
    float y = cos(repeat)*circle - inradius;

    float inside = min(y, 0.0);
    float corner = radius*sin(halfSegmentAngle);
    float outside = length(float2(max(abs(x) - corner, 0.0), y))*step(0.0, y);
    return inside + outside;
}

half2 UnpackToVec2(float value) {
	const int PACKER_STEP = 4096;
	const int PRECISION = PACKER_STEP - 1;
	half2 unpacked;

	unpacked.x = (value % (PACKER_STEP)) / (PACKER_STEP - 1);
	value = floor(value / (PACKER_STEP));

	unpacked.y = (value % PACKER_STEP) / (PACKER_STEP - 1);
	return unpacked;
}

float4 UnpackSDFCoordinates(float packedSize, float packedUVs) {
    uint intSize = asuint(packedSize);

    float width = ((intSize >> 16) & (1 << 16) - 1) / 10;
    float height = ( intSize & 0xffff) / 10;
    
    return float4(UnpackToVec2(packedUVs), width, height);
}

inline int and(int a, int b) {
    return a * b;
}

float4 UnpackSDFParameters(float packed) {
    uint packedInt = asuint(packed);
    int shapeType = packedInt & 0xff;
    return float4(shapeType, 0, 0, 0);
}

float4 UnpackSDFRadii(float packed) {
    uint packedRadii = asuint(packed);
    return float4(
        uint((packedRadii >>  0) & 0xff),
        uint((packedRadii >>  8) & 0xff),
        uint((packedRadii >> 16) & 0xff),
        uint((packedRadii >> 24) & 0xff)
    );
}
            
fixed4 SDFColor(float4 radii, float2 drawSurfaceSize, float2 uv, fixed4 color) {
    float fDist = 0;
    float halfStrokeWidth = 0;
    fixed4 fromColor = color;
    fixed4 toColor = fixed4(color.rgb, 0);
    
    float2 halfShapeSize = (drawSurfaceSize * 0.5) - halfStrokeWidth;
    
    float left = step(uv.x, 0.5); // 1 if left
    float bottom = step(uv.y, 0.5); // 1 if bottom
    float top = 1 - bottom;
    float right = 1 - left;
    
    float r = 0;
    r += and(top, left) * radii.x;
    r += and(top, right) * radii.y;
    r += and(bottom, left) * radii.z;
    r += and(bottom, 1 - left) * radii.w;
    
    float radius = drawSurfaceSize * ((r * 2) / 1000);
    float2 center = uv.xy - 0.5;
    
    // not sure why the + 0.5 is needed, maybe pixel alignment
    fDist = RectSDF((center * drawSurfaceSize) + float2(0.5, 0) , halfShapeSize, radius - halfStrokeWidth);
    // todo - lerp on shape type & use if def   
    //fDist = EllipseSDF((sdfData.center * drawSurfaceSize) + float2(0.5, 0), halfShapeSize);
    
    // todo finish upacking radii in vertex shader
    
    float fBlendAmount = smoothstep(-1, 1, fDist);
    
    return lerp(fromColor, toColor, fBlendAmount);
}
         
#endif // VERTIGO_SDF_INCLUDE