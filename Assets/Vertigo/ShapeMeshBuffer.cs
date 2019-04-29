using UnityEngine;

namespace Vertigo {

    public class ShapeMeshBuffer {

        public readonly LightList<Vector3> positionList;
        public readonly LightList<Vector4> texCoord0List;
        public readonly LightList<Vector4> texCoord1List;
        public readonly LightList<Vector4> texCoord2List;
        public readonly LightList<Vector3> normalList;
        public readonly LightList<Color> colorList;
        public readonly LightList<int> triangleList;

        public ShapeMeshBuffer() {
            this.positionList = new LightList<Vector3>(32);
            this.normalList = new LightList<Vector3>(32);
            this.colorList = new LightList<Color>(32);
            this.texCoord0List = new LightList<Vector4>(32);
            this.texCoord1List = new LightList<Vector4>(32);
            this.texCoord2List = new LightList<Vector4>(32);
            this.triangleList = new LightList<int>(64);
        }

        public int vertexCount => positionList.Count;

        public void EnsureCapacity(int vertexCount, int triangleCount) {
            positionList.EnsureCapacity(vertexCount);
            normalList.EnsureCapacity(vertexCount);
            colorList.EnsureCapacity(vertexCount);
            texCoord0List.EnsureCapacity(vertexCount);
            texCoord1List.EnsureCapacity(vertexCount);
            texCoord2List.EnsureCapacity(vertexCount);
            triangleList.EnsureCapacity(triangleCount);
        }

    }

}