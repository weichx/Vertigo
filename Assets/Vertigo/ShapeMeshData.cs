using UnityEngine;

namespace Vertigo {

    public struct ShapeMeshData {

        public bool isSDF;
        public RangeInt vertexRange;
        public RangeInt triangleRange;
        public RangeInt creationRange;
        public ShapeType shapeType;
        public Rect bounds;
        public MeshRange meshRange;

    }

}