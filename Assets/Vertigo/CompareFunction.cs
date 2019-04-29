using System;

namespace Vertigo {

    [Flags]
    public enum CompareFunction : byte {

        Disabled = 0,
        Never = 1 << 0,
        Less = 1 << 1,
        Equal = 1 << 2,
        LessEqual = 1 << 3,
        Greater = 1 << 4,
        NotEqual = 1 << 5,
        GreaterEqual = 1 << 6,
        Always = 1 << 7,

    }

}