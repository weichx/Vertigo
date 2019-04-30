using Unity.Mathematics;
using UnityEngine;

namespace Vertigo {

    public struct VertigoState {

        public float3x2 transform;
        public Rect scissorRect;
        public RenderState renderState;
        public RenderSettings renderSettings;

    }

}