using UnityEngine;

namespace Vertigo {

    public struct Shape {

        public RangeInt pointRange;
        public ShapeType type;
        public Rect bounds;

        public Shape(ShapeType type, RangeInt pointRange = default, Rect bounds = default) {
            this.type = type;
            this.pointRange = pointRange;
            this.bounds = bounds;
        }

    }

}