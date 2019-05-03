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

        public int vertexCount => positions.size;
        public int triangleCount => triangles.size;

        public GeometryCache() {
            shapes = new StructList<GeometryShape>();
            positions = new StructList<Vector3>(32);
            normals = new StructList<Vector3>(32);
            colors = new StructList<Color>(32);
            texCoord0 = new StructList<Vector4>(32);
            texCoord1 = new StructList<Vector4>(32);
            triangles = new StructList<int>(64);
        }

        public void EnsureAdditionalCapacity(int vertCount, int triCount) {
            positions.EnsureAdditionalCapacity(vertCount);
            normals.EnsureAdditionalCapacity(vertCount);
            colors.EnsureAdditionalCapacity(vertCount);
            texCoord0.EnsureAdditionalCapacity(vertCount);
            texCoord1.EnsureAdditionalCapacity(vertCount);
            triangles.EnsureAdditionalCapacity(triCount);
        }

        public bool SetVertexColors(int shapeIdx, Color color) {
            if (shapeIdx < 0 || shapeIdx > shapes.size) {
                return false;
            }

            GeometryShape shape = shapes[shapeIdx];
            int start = shape.vertexStart;
            int end = start + shape.vertexCount;
            Color[] c = this.colors.array;
            for (int i = start; i < end; i++) {
                c[i] = color;
            }

            return true;
        }

        public bool SetNormals(int shapeIdx, Vector3 normal) {
            if (shapeIdx < 0 || shapeIdx > shapes.size) {
                return false;
            }

            GeometryShape shape = shapes[shapeIdx];
            int start = shape.vertexStart;
            int end = start + shape.vertexCount;
            Vector3[] c = this.normals.array;
            for (int i = start; i < end; i++) {
                c[i] = normal;
            }

            return true;
        }

        public void Clear() {
            shapes.QuickClear();
            positions.QuickClear();
            normals.QuickClear();
            colors.QuickClear();
            texCoord0.QuickClear();
            texCoord1.QuickClear();
            triangles.QuickClear();
        }

    }

}