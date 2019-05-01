using UnityEngine;

namespace Vertigo {

    public struct GeometryShape {

        public int vertexStart;
        public int vertexCount;
        public int triangleStart;
        public int triangleCount;

    }
    
    public class GeometryCache {

        public StructList<GeometryShape> shapes;

        public StructList<Vector3> positions;
        public StructList<Vector3> normals;
        public StructList<Color> colors;    
        public StructList<Vector4> texCoord0;    
        public StructList<Vector4> texCoord1;
        public StructList<int> triangles;

        public int vertexCount;
        public int triangleCount;
        public int triangleIndex;

    }

}