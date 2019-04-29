using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Vertigo {

    public enum VertigoEffectFeature {

        SdfRendering,
        GPULines,
        ParameterTexture
        
    }

    // ctx.Fill(effect, effectData, shapes):
    // effect.data.y = x;
    // render;
    
    // effect.material = material;

    public struct RenderState {

        public Color strokeColor;
        public Color fillColor;
        
        public float strokeWidth;
        public float strokeOpacity;
        public float fillOpacity;
        
        public Texture2D texture0;
        public Texture2D texture1;
        public Texture2D texture2;
        public StrokePlacement strokePlacement;
        public ColorMode strokeColorMode;
        
        public RenderSettings renderSettings;

    }
    
    // effects must be the same to batched
    // effect data is per-shape, no notion of that per batch
    // data might set textures though

    // ctx.AddEffect();
    // ctx.RegisterEffect(effect);
    
    //ctx.Stroke(effect, shape);
    
    public struct VertigoState {

        public float3x2 transform;
        public Rect scissorRect;
        public RenderState renderState;
        public RenderSettings renderSettings;

    }

    public class ShadowOutline : VertigoEffect<ShadowData> {
        
        private readonly ShapeMeshBuffer buffer = new ShapeMeshBuffer();

        public ShadowOutline(Material material) : base(material) { }
        
        public bool CanBatch(List<ShapeBatch> batches, in ShadowData testBatch) {
            return false;
        }

        public override Material GetMaterialToDraw(int drawCallId, in VertigoState state, ShapeBatch shapeBatch, MaterialPropertyBlock block) {
            block.SetTexture("_MainTex", state.renderState.texture0);
            return base.GetMaterialToDraw(drawCallId, in state, shapeBatch, block);
        }

        protected override void ModifyShapeMesh(ShapeBatch shapeBatch, in ShadowData data, in MeshSlice slice, in Shape shape) {
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