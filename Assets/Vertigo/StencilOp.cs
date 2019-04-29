using System;

namespace Vertigo {

    [Flags]
    public enum StencilOp : byte {

        Keep = 0,
        Zero = 1 << 0,
        Replace = 1 << 1,
        IncrementSaturate = 1 << 2,
        DecrementSaturate = 1 << 3,
        Invert = 1 << 4,
        IncrementWrap = 1 << 5,
        DecrementWrap = 1 << 6,

    }

}