using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Vertigo {

    public enum ShapeMode {

        Physical,
        SDF

    }
    
    public class VertigoContext {

        public static readonly int shaderKey_StencilRef;
        public static readonly int shaderKey_StencilReadMask;
        public static readonly int shaderKey_StencilWriteMask;
        public static readonly int shaderKey_StencilComp;
        public static readonly int shaderKey_StencilPassOp;
        public static readonly int shaderKey_StencilFailOp;
        public static readonly int shaderKey_ColorMask;
        public static readonly int shaderKey_Culling;
        public static readonly int shaderKey_ZWrite;
        public static readonly int shaderKey_BlendArgSrc;
        public static readonly int shaderKey_BlendArgDst;
        public static readonly int shaderKey_MaskTexture;
        public static readonly int shaderKey_MaskSoftness;
        private static readonly MaterialPropertyBlock s_PropertyBlock;

        public readonly MaterialPool materialPool;
        private readonly IDrawCallBatcher batcher;
        private VertigoState renderState;
        private Stack<VertigoState> stateStack;

        private Vector3 position;
        private Quaternion rotation;
        private Vector3 scale;

        private readonly ShapeGenerator shapeGenerator;
        private readonly GeometryCache geometryCache;
        private readonly GeometryGenerator geometryGenerator;

        private readonly Stack<RenderTexture> renderTextures;
        private readonly StructList<RenderCall> renderCalls;
        private readonly LightList<RenderTexture> renderTexturesToRelease;
        private RangeInt currentShapeRange;
        private ShapeMode defaultShapeMode;
        private VertigoMaterial strokeMaterial;
        private VertigoMaterial fillMaterial;
        public static MaterialPool DefaultMaterialPool { get; }


        public VertigoContext(ShapeMode shapeMode = ShapeMode.SDF, IDrawCallBatcher batcher = null, MaterialPool materialPool = null) {
            if (batcher == null) {
                batcher = new DefaultDrawCallBatcher();
            }

            if (materialPool == null) {
                materialPool = DefaultMaterialPool;
            }

            this.defaultShapeMode = shapeMode;

            this.renderTextures = new Stack<RenderTexture>();
            this.renderCalls = new StructList<RenderCall>();
            this.renderTexturesToRelease = new LightList<RenderTexture>();

            this.batcher = batcher;
            this.materialPool = materialPool;
            this.stateStack = new Stack<VertigoState>();
            this.position = Vector3.zero;
            this.rotation = Quaternion.identity;
            this.scale = Vector3.one;
            this.shapeGenerator = new ShapeGenerator();
            this.geometryGenerator = new GeometryGenerator();
            this.geometryCache = new GeometryCache();
        }

        public void SetMaterial(VertigoMaterial material) {
            this.fillMaterial = material;
            this.strokeMaterial = material;
        }
        
        public void SetFillMaterial(VertigoMaterial material) {
            this.fillMaterial = material;
        }
        
        public void SetStrokeMaterial(VertigoMaterial material) {
            this.strokeMaterial = material;
        }

        public void FillCircle(float x, float y, float radius, VertigoMaterial material = null) {
            material = material ?? fillMaterial;
            int pathId = shapeGenerator.Circle(x, y, radius);
            geometryGenerator.Fill(shapeGenerator, new RangeInt(pathId, 1), defaultShapeMode, geometryCache);
            batcher.AddDrawCall(geometryCache, new RangeInt(geometryCache.shapeCount - 1, 1), material, renderState);
        }
        
        public void FillEllipse(float x, float y, float rw, float rh, VertigoMaterial material = null) {
            material = material ?? fillMaterial;
            int pathId = shapeGenerator.Ellipse(x, y, rw, rh);
            geometryGenerator.Fill(shapeGenerator, new RangeInt(pathId, 1), defaultShapeMode, geometryCache);
            batcher.AddDrawCall(geometryCache, new RangeInt(geometryCache.shapeCount - 1, 1), material, renderState);
        }

        public void FillRoundedRect(float x, float y, float width, float height, float rTL, float rTR, float rBL, float rBR, VertigoMaterial material = null) {
            material = material ?? fillMaterial;
            int pathId = shapeGenerator.RoundedRect(x, y, width, height, rTL, rTR, rBL, rBR);
            geometryGenerator.Fill(shapeGenerator, new RangeInt(pathId, 1), defaultShapeMode, geometryCache);
            batcher.AddDrawCall(geometryCache, new RangeInt(geometryCache.shapeCount - 1, 1), material, renderState);
        }
        
        public void SaveState() {
            stateStack.Push(renderState);
        }

        public void RestoreState() {
            if (stateStack.Count == 0) {
                return;
            }

            renderState = stateStack.Pop();
        }

        public void Draw(GeometryCache cache, VertigoMaterial material) {
            batcher.AddDrawCall(cache, new RangeInt(0, cache.shapes.size), material, renderState);
        }

        public void SetStencilState(byte stencilRef, byte readMask, byte writeMask, CompareFunction compFn,
            StencilOp pass, StencilOp fail = StencilOp.Keep) { }

        public void SetStrokeColor(Color32 color) {
            geometryGenerator.SetStrokeColor(color);
        }

        public void SetStrokeWidth(float strokeWidth) {
            geometryGenerator.SetStrokeWidth(strokeWidth);
        }

        public void SetLineCap(LineCap lineCap) {
            geometryGenerator.SetLineCap(lineCap);
        }

        public void SetLineJoin(LineJoin lineJoin) {
            geometryGenerator.SetLineJoin(lineJoin);
        }

        public int Rect(float x, float y, float width, float height) {
            shapeGenerator.Rect(x, y, width, height);
            currentShapeRange.length++;
            return shapeGenerator.shapes.size - 1;
        }

        public void Circle(float x, float y, float radius) {
            shapeGenerator.Circle(x, y, radius);
            currentShapeRange.length++;
        }

        public void BeginShapeRange() {
            currentShapeRange.start = shapeGenerator.shapes.size;
            currentShapeRange.length = 0;
        }

        public void SetUVRect(in Rect rect) {
            uvRect = rect;
        }

        public void ResetUVState() {
            uvRect = new Rect(0, 0, 1, 1);
        }

        public void SetUVTiling(float x, float y) {
            throw new NotImplementedException();
        }

        public void SetUVOffset(float x, float y) {
            throw new NotImplementedException();
        }

        public void Fill(GeometryCache cache, RangeInt range, VertigoMaterial material) {
            throw new NotImplementedException();
        }

        public void DrawSprite(Sprite sprite, Rect rect, VertigoMaterial material) {
            int start = geometryCache.shapeCount;
            geometryGenerator.FillSprite(sprite, rect, geometryCache);
            batcher.AddDrawCall(geometryCache, new RangeInt(start, 1), material, renderState);
        }

        public void DrawSprite(Sprite sprite, VertigoMaterial material) {
            int start = geometryCache.shapeCount;
            geometryGenerator.FillSprite(sprite, default, geometryCache);
            batcher.AddDrawCall(geometryCache, new RangeInt(start, 1), material, renderState);
        }

        private Rect uvRect = new Rect(0, 0, 1, 1);
        private static readonly StructList<Vector4> s_ScratchVector4 = new StructList<Vector4>();

        public void Fill(VertigoMaterial material) {
            if (currentShapeRange.length == 0) {
                return;
            }

            int start = geometryCache.shapeCount;
            geometryGenerator.Fill(shapeGenerator, currentShapeRange, defaultShapeMode, geometryCache);
            int count = geometryCache.shapeCount - start;

            if (uvRect.x != 0 || uvRect.y != 0 || uvRect.width != 1 || uvRect.height != 1) {
                geometryCache.GetTextureCoord0(start, s_ScratchVector4);
                Vector4[] uvs = s_ScratchVector4.array;
                float minX = uvRect.x;
                float minY = uvRect.y;
                // todo -- might only work when uv rect is smaller than original
                for (int i = 0; i < s_ScratchVector4.size; i++) {
                    // map bounds uvs to different rect uvs
                    uvs[i].x = minX + (uvs[i].x * uvRect.width);
                    uvs[i].y = minY + (uvs[i].y * uvRect.height);
                }

                geometryCache.SetTexCoord0(start, s_ScratchVector4);
            }

            batcher.AddDrawCall(geometryCache, new RangeInt(start, count), material, renderState);
        }


        public RenderTexture Render() {
            RenderTexture targetTexture = RenderTexture.active;

            if (renderTextures.Count > 0) {
                targetTexture = renderTextures.Peek();
            }

            int width = Screen.width;
            int height = Screen.height;

            if (!ReferenceEquals(targetTexture, null)) {
                width = targetTexture.width;
                height = targetTexture.height;
            }

            Matrix4x4 cameraMatrix = Matrix4x4.identity;

            StructList<BatchDrawCall> drawCalls = StructList<BatchDrawCall>.Get();
//            Matrix4x4 rootMat = Matrix4x4.TRS(new Vector3(-(width / 2), height / 2), Quaternion.identity, Vector3.one);
            batcher.Bake(width, height, cameraMatrix, drawCalls);
            batcher.Clear();

            renderCalls.Add(new RenderCall() {
                // matrix?
                texture = targetTexture,
                drawCalls = drawCalls
            });

            return targetTexture;
        }

        public void PushRenderTexture(int width, int height, RenderTextureFormat format = RenderTextureFormat.Default) {
            RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 24, format);
            renderTextures.Push(renderTexture);
            renderTexturesToRelease.Add(renderTexture);
        }

        public void PopRenderTexture() {
            if (renderTextures.Count != 0) {
                renderTextures.Pop();
            }
        }

        public void Flush(Camera camera, CommandBuffer commandBuffer) {
            commandBuffer.Clear();

            Matrix4x4 cameraMatrix = camera.worldToCameraMatrix;

            for (int i = 0; i < renderCalls.Count; i++) {
                RenderTexture renderTexture = renderCalls[i].texture ? renderCalls[i].texture : RenderTexture.active;
                commandBuffer.SetRenderTarget(renderTexture);

                if (!ReferenceEquals(renderTexture, null)) { // use render texture
                    int width = renderTexture.width;
                    int height = renderTexture.height;
                    commandBuffer.ClearRenderTarget(true, true, Color.red);
                    Matrix4x4 projection = Matrix4x4.Ortho(-width, width, -height, height, 0.3f, 999999);
                    commandBuffer.SetViewProjectionMatrices(cameraMatrix, projection);
                }
                else { // use screen
                    commandBuffer.SetViewProjectionMatrices(cameraMatrix, camera.projectionMatrix);
                }

//                Render(renderCalls[i].drawCalls);
                int count = renderCalls[i].drawCalls.size;
                BatchDrawCall[] calls = renderCalls[i].drawCalls.array;

                for (int j = 0; j < count; j++) {
                    Mesh mesh = calls[j].mesh.mesh;
                    Material material = calls[j].material.material;
                    UpdateMaterialPropertyBlock(calls[j].state);
                    int passCount = material.passCount;
                    // todo -- only render specified passes
                    for (int k = 0; k < passCount; k++) {
                        commandBuffer.DrawMesh(mesh, calls[j].state.transform, material, 0, k, s_PropertyBlock);
                    }
                }
            }
        }

        public void SetMask(Texture texture, float softness) {
            renderState.renderSettings.mask = texture;
            renderState.renderSettings.maskSoftness = softness;
        }

        static VertigoContext() {
            s_PropertyBlock = new MaterialPropertyBlock();
            DefaultMaterialPool = new MaterialPool();
            shaderKey_StencilRef = Shader.PropertyToID("_Stencil");
            shaderKey_StencilReadMask = Shader.PropertyToID("_StencilReadMask");
            shaderKey_StencilWriteMask = Shader.PropertyToID("_StencilWriteMask");
            shaderKey_StencilComp = Shader.PropertyToID("_StencilComp");
            shaderKey_StencilPassOp = Shader.PropertyToID("_StencilOp");
            shaderKey_StencilFailOp = Shader.PropertyToID("_StencilOpFail");
            shaderKey_ColorMask = Shader.PropertyToID("_ColorMask");
            shaderKey_Culling = Shader.PropertyToID("_Culling");
            shaderKey_ZWrite = Shader.PropertyToID("_ZWrite");
            shaderKey_BlendArgSrc = Shader.PropertyToID("_SrcBlend ");
            shaderKey_BlendArgDst = Shader.PropertyToID("_DstBlend ");
            shaderKey_MaskTexture = Shader.PropertyToID("_MaskTexture");
            shaderKey_MaskSoftness = Shader.PropertyToID("_MaskSoftness");
        }

        private static void UpdateMaterialPropertyBlock(in VertigoState state) {
            s_PropertyBlock.SetInt(shaderKey_StencilRef, state.renderSettings.stencilRefValue);
            s_PropertyBlock.SetInt(shaderKey_StencilReadMask, state.renderSettings.stencilReadMask);
            s_PropertyBlock.SetInt(shaderKey_StencilWriteMask, state.renderSettings.stencilWriteMask);
            s_PropertyBlock.SetInt(shaderKey_StencilComp, (int) state.renderSettings.stencilComp);
            s_PropertyBlock.SetInt(shaderKey_StencilPassOp, (int) state.renderSettings.stencilPassOp);
            s_PropertyBlock.SetInt(shaderKey_StencilFailOp, (int) state.renderSettings.stencilFailOp);
            s_PropertyBlock.SetInt(shaderKey_ColorMask, state.renderSettings.colorMask);
            s_PropertyBlock.SetInt(shaderKey_Culling, state.renderSettings.cullMode);
            s_PropertyBlock.SetInt(shaderKey_ZWrite, state.renderSettings.zWrite ? 1 : 0);
            s_PropertyBlock.SetInt(shaderKey_BlendArgSrc, (int) state.renderSettings.blendArgSrc);
            s_PropertyBlock.SetInt(shaderKey_BlendArgDst, (int) state.renderSettings.blendArgDst);
//            s_PropertyBlock.SetTexture(shaderKey_MaskTexture, ReferenceEquals(state.renderSettings.mask, null) ? Texture2D.whiteTexture : state.renderSettings.mask);
           // s_PropertyBlock.SetFloat(shaderKey_MaskSoftness, state.renderSettings.maskSoftness);
        }

        public void Clear() {
            for (int i = 0; i < renderTexturesToRelease.Count; i++) {
                RenderTexture.ReleaseTemporary(renderTexturesToRelease[i]);
            }

            currentShapeRange = new RangeInt(0, 0);
            shapeGenerator.Clear();
            geometryCache.Clear();
            geometryGenerator.ResetRenderState();
            renderState.transform = Matrix4x4.identity;

            for (int i = 0; i < renderCalls.Count; i++) {
                StructList<BatchDrawCall> drawCalls = renderCalls[i].drawCalls;
                for (int j = 0; j < drawCalls.Count; j++) {
                    drawCalls[j].material.Release();
                    drawCalls[j].mesh.Release();
                }

                StructList<BatchDrawCall>.Release(ref drawCalls);
            }

            renderCalls.Clear();
            renderTextures.Clear();
            renderTexturesToRelease.Clear();
        }

        public void SetPosition(Vector3 position) {
            this.position = position;
            renderState.transform = Matrix4x4.TRS(position, rotation, scale);
        }

        public void SetPosition(Vector2 position) {
            this.position = new Vector3(position.x, position.y, this.position.z);
            renderState.transform = Matrix4x4.TRS(position, rotation, scale);
        }

        public void SetRotation(float angle) {
            rotation = Quaternion.Euler(0, 0, angle);
            renderState.transform = Matrix4x4.TRS(position, rotation, scale);
        }

        public void SetFillColor(Color32 color) {
            geometryGenerator.SetFillColor(color);
        }

        private struct RenderCall {

            public RenderTexture texture;
            public StructList<BatchDrawCall> drawCalls;

        }

    }

}