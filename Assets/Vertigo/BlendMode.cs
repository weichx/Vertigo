using System;

namespace Vertigo {

    [Flags]
    public enum BlendMode : ushort {

        Zero = 0,
        One = 1 << 0,
        DstColor = 1 << 1,
        SrcColor = 1 << 2,
        OneMinusDstColor = 1 << 3,
        SrcAlpha = 1 << 4,
        OneMinusSrcColor = 1 << 5,
        DstAlpha = 1 << 6,
        OneMinusDstAlpha = 1 << 7,
        SrcAlphaSaturate = 1 << 8,
        OneMinusSrcAlpha = 1 << 9,

    }

}