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

        public void AddQuad() {
            shapes.Add(new GeometryShape() {
                vertexStart = vertexCount,
                vertexCount = 4,
                triangleStart = triangleCount,
                triangleCount = 6
            });
            triangles[triangleCount + 0] = vertexCount + 0;
            triangles[triangleCount + 1] = vertexCount + 1;
            triangles[triangleCount + 2] = vertexCount + 2;
            triangles[triangleCount + 3] = vertexCount + 2;
            triangles[triangleCount + 4] = vertexCount + 3;
            triangles[triangleCount + 5] = vertexCount + 0;
            positions.size += 4;
            normals.size += 4;
            colors.size += 4;
            texCoord0.size += 4;
            texCoord1.size += 4;
            triangles.size += 6;
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

    }

}