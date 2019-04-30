using UnityEngine;

namespace Vertigo {

    public struct PendingDrawCall {

        public bool isStroke;
        public VertigoState state;
        public int effectDataIndex;
        public RangeInt shapeRange;
        public VertigoEffect effect;
        
    }

}