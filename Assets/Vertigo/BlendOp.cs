using System;

namespace Vertigo {

    [Flags]
    public enum BlendOp : byte {

        Add = 0,
        Subtract = 1 << 0,
        ReverseSubtract = 1 << 1,
        Min = 1 << 2,
        Max = 1 << 3

    }

}