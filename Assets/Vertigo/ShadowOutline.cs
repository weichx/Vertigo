using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Vertigo;

namespace Effect {

    public class ShadowOutline : VertigoEffect<ShadowData> {

        private readonly ShapeMeshBuffer buffer = new ShapeMeshBuffer();
        private static readonly int _MainTex = Shader.PropertyToID("_MainTex");

        public ShadowOutline(Material material) : base(material) { }

        public bool CanRunWithKeywords(List<string> keywords, ShapeBatch batch, in ShadowData data) {
            return false;
        }

        public override Material GetMaterialToDraw(int drawCallId, in VertigoState state, ShapeBatch shapeBatch, MaterialPropertyBlock block) {
            block.SetTexture(_MainTex, state.renderState.texture0);
            
            material.EnableKeyword("TONE_SEPIA");
            material.EnableKeyword("BLUR_7x7");
            material.EnableKeyword("GRADIENT");
            
            return base.GetMaterialToDraw(drawCallId, in state, shapeBatch, block);
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct Union {

            [FieldOffset(0)] public float asFloat;
            [FieldOffset(0)] public int asInt;

        }

        public float ColorToFloat(Color c) {
            int color = (int) (c.r * 255) | (int) (c.g * 255) << 8 | (int) (c.b * 255) << 16 | (int) (c.a * 255) << 24;

            Union color2Float;
            color2Float.asFloat = 0;
            color2Float.asInt = color;

            return color2Float.asFloat;
        }
        
        protected override void Apply(ShapeBatch shapeBatch, in VertigoState state, in ShadowData data, in MeshSlice slice) {

          //  shapeBatch.AddMeshData(slice, material, materialPropertyBlock);
            
            if (data.shadows != null) {
                
                for (int i = 0; i < data.shadows.Length; i++) {
                    
                    slice.FillShapeBuffer(buffer, VertexChannel.Position | VertexChannel.Color | VertexChannel.TexCoord0 | VertexChannel.TexCoord1);
                    
                    if (i < data.shadows.Length) {
                        Vector2 offset = data.shadows[i].offset;
                        Color color = data.shadows[i].color;
                        float blur = data.shadows[i].blur;

                        int vertexCount = buffer.vertexCount;
                        Vector3[] vertices = buffer.positionList.Array;
                        Vector4[] textureCoords0 = buffer.texCoord0List.Array;
                        Vector4[] textureCoords1 = buffer.texCoord0List.Array;
                        
                        int parameterIndex = shapeBatch.SetParameter(color);

                        const int shouldBlur = 1;
                        
                        for (int j = 0; j < vertexCount; j++) {
                            vertices[j].x += offset.x;
                            vertices[j].y += offset.y;
                            textureCoords0[j].z = ColorToFloat(color);
                            textureCoords0[j].w = blur;
//                            textureCoords1[j].x = ColorToFloat(color);
//                            textureCoords1[j].y = blur;
                        }

                        shapeBatch.AddMeshData(buffer);
                    }
                    else {
                        
                    }
                }
            }
            
            //slice.FillShapeBuffer(buffer, VertexChannel.Position | VertexChannel.Color | VertexChannel.TexCoord0 | VertexChannel.TexCoord1);

            shapeBatch.AddMeshData(slice);

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