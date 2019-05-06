using System.Runtime.InteropServices;
using UnityEngine;

namespace Vertigo {

    [StructLayout(LayoutKind.Explicit)]
    public struct RenderSettings {

        [FieldOffset(0)] internal int blendArgs;
        [FieldOffset(0)] public BlendMode blendArgSrc;
        [FieldOffset(2)] public BlendMode blendArgDst;

        [FieldOffset(4)] internal int blendOpStencilRefMasks;
        [FieldOffset(4)] public BlendOp blendOp;
        [FieldOffset(5)] public byte stencilRefValue;
        [FieldOffset(6)] public byte stencilReadMask;
        [FieldOffset(7)] public byte stencilWriteMask;

        [FieldOffset(8)] internal int stencilOpAndComp;
        [FieldOffset(8)] public CompareFunction stencilComp;
        [FieldOffset(9)] public StencilOp stencilPassOp;
        [FieldOffset(10)] public StencilOp stencilFailOp;
        [FieldOffset(11)] public StencilOp stencilZFail;

        [FieldOffset(12)] internal int zWriteColorMaskCullMode;
        [FieldOffset(12)] public bool zWrite;
        [FieldOffset(13)] public byte colorMask; // really 4 bools but bools take 32 bits
        [FieldOffset(14)] public byte cullMode;
        [FieldOffset(15)] public byte zTest;

        [FieldOffset(16)] 
        public Texture mask;
        [FieldOffset(20)] 
        public float maskSoftness;

        public bool IsEqualTo(RenderSettings other) {
            return blendArgs == other.blendArgs
                   && blendOpStencilRefMasks == other.blendOpStencilRefMasks
                   && zWriteColorMaskCullMode == other.zWriteColorMaskCullMode && mask == other.mask && maskSoftness == other.maskSoftness;
        }

        public static bool operator ==(RenderSettings a, RenderSettings b) {
            return a.IsEqualTo(b);
        }

        public static bool operator !=(RenderSettings a, RenderSettings b) {
            return !a.IsEqualTo(b);
        }

    }

}