using System.Collections.Generic;
using UnityEngine;

namespace Vertigo {

    public class ShadowOutline : VertigoEffect<ShadowData> {

        public ShadowOutline(Material material) : base(material) { }

        private readonly ShapeMeshBuffer buffer = new ShapeMeshBuffer();

//        public Material GetMaterial() {
//            return Shader.Find("");
//        }
        
        protected override void Fill(ShapeBatch shapeBatch, in ShadowData data, in MeshSlice slice, in Shape shape) {
            slice.FillShapeBuffer(buffer, VertexChannel.Position | VertexChannel.Color | VertexChannel.TexCoord0);

            int vertexCount = buffer.vertexCount;
            Vector3[] vertices = buffer.positionList.Array;

            for (int i = 0; i < vertexCount; i++) {
                vertices[i].x += data.offset.x;
                vertices[i].y += data.offset.y;
                vertices[i].z += 1;
            }

            shapeBatch.AddMeshData(buffer);
            shapeBatch.AddMeshData(slice);
            
//            return material;
            
        }

        private void ApplyShadow(List<UIVertex> baseVertices, List<UIVertex> outputVertices, Color32 color, float x, float y, bool useAlpha) {
            Vector3 offset = new Vector3(x, y, 0);

            for (int i = 0; i < baseVertices.Count; i++) {
                UIVertex vertex = baseVertices[i];
                vertex.position += offset;
                Color32 c = vertex.color;
                c.a = useAlpha ? (byte) (c.a * color.a / 255) : color.a;
                vertex.color = c;
                outputVertices[i] = vertex;
            }
        }

    }

}