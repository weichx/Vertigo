using System;
using UnityEngine;

namespace Vertigo {

    [Serializable]
    public struct Shadow {

        public Vector2 offset;
        public Color color;
        [Range(0, 1)]
        public float blur;

    }
    
    [Serializable]
    public struct ShadowData {

        public Shadow[] shadows;

    }

}