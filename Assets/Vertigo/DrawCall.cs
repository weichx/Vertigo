using Unity.Mathematics;
using UnityEngine;

namespace Vertigo {

    public struct DrawCall {

        public VertigoEffect effect;
        public int effectDataIndex;
        public RangeInt shapeRange;
        public float3x2 matrix;
        public bool isStroke;
        public RenderSettings renderSettings;
        public RangeInt pointRange;

        public bool CanBatchWith(ref DrawCall call) {
            if (call.effect != effect) {
                return false;
            }

            if (!call.renderSettings.IsEqualTo(renderSettings)) {
                return false;
            }

            if (call.effectDataIndex != effectDataIndex) {
                return false;
            }

            return true;
        }

    }

}