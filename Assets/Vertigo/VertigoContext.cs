using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Vertigo {

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
        private static readonly MaterialPropertyBlock s_PropertyBlock;

        public readonly MaterialPool materialPool;
        private readonly IDrawCallBatcher batcher;
        private readonly StructList<BatchDrawCall> drawCalls;
        private VertigoState renderState;
        private Stack<VertigoState> stateStack;

        private ShadowCastingMode shadowCastingMode = ShadowCastingMode.Off;
        private bool receiveShadows = false;
        private Transform lightProbeAnchor = null;
        private LightProbeUsage lightProbeUsage = LightProbeUsage.Off;

        private Vector3 position;
        private Quaternion rotation;
        private Vector3 scale;

        private readonly ShapeGenerator shapeGenerator;
        private readonly GeometryCache geometryCache;
        private readonly GeometryGenerator geometryGenerator;

        private RangeInt currentShapeRange;

        public static MaterialPool DefaultMaterialPool { get; }

        public VertigoContext(IDrawCallBatcher batcher = null, MaterialPool materialPool = null) {
            if (batcher == null) {
                batcher = new DefaultDrawCallBatcher();
            }

            if (materialPool == null) {
                materialPool = DefaultMaterialPool;
            }

            this.batcher = batcher;
            this.materialPool = materialPool;
            this.stateStack = new Stack<VertigoState>();
            this.drawCalls = new StructList<BatchDrawCall>(16);
            this.position = Vector3.zero;
            this.rotation = Quaternion.identity;
            this.scale = Vector3.one;
            this.shapeGenerator = new ShapeGenerator();
            this.geometryGenerator = new GeometryGenerator();
            this.geometryCache = new GeometryCache();
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

        public void SetUVTiling(float x, float y) { }

        public void SetUVOffset(float x, float y) { }

        public void Fill(GeometryCache cache, RangeInt range, VertigoMaterial material) { }

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
            geometryGenerator.Fill(shapeGenerator, currentShapeRange, geometryCache);
            int count = geometryCache.shapeCount - start;

            if (uvRect.x != 0 || uvRect.y != 0 || uvRect.width != 1 || uvRect.height != 1) {
                geometryCache.GetTextureCoord0(start, s_ScratchVector4);
                Vector4[] uvs = s_ScratchVector4.array;
                float minX = uvRect.x;
                float minY = uvRect.y;
                for (int i = 0; i < s_ScratchVector4.size; i++) {
                    // map bounds uvs to different rect uvs
                    uvs[i].x = minX + (uvs[i].x * uvRect.width);
                    uvs[i].y = minY + (uvs[i].y * uvRect.height);
                }

                geometryCache.SetTexCoord1(start, s_ScratchVector4);
            }

            batcher.AddDrawCall(geometryCache, new RangeInt(start, count), material, renderState);
        }

        public void Flush(Camera camera) {
            RenderTexture targetTexture = camera.targetTexture;

            int width = Screen.width / 2;
            int height = Screen.height / 2;

            if (!ReferenceEquals(targetTexture, null)) {
                width = targetTexture.width / 2;
                height = targetTexture.height / 2;
            }

            Matrix4x4 rootMat = Matrix4x4.TRS(new Vector3(-width, height), Quaternion.identity, Vector3.one);
            batcher.Bake(drawCalls);
            batcher.Clear();

            int count = drawCalls.size;
            BatchDrawCall[] calls = drawCalls.array;

            for (int i = 0; i < count; i++) {
                Mesh mesh = calls[i].mesh.mesh;
                Material material = calls[i].material.material;
                UpdateMaterialPropertyBlock(calls[i].state);
                Graphics.DrawMesh(
                    mesh,
                    rootMat * calls[i].state.transform,
                    material,
                    0, camera, 0,
                    s_PropertyBlock,
                    shadowCastingMode,
                    receiveShadows,
                    lightProbeAnchor,
                    lightProbeUsage
                );
            }
            
            // create temp texture
            // set render target
            // do drawing
            // set render target
            
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
        }

        public void Clear() {
            currentShapeRange = new RangeInt(0, 0);
            shapeGenerator.Clear();
            geometryCache.Clear();
            geometryGenerator.ResetRenderState();
            renderState.transform = Matrix4x4.identity;

            for (int i = 0; i < drawCalls.Count; i++) {
                drawCalls[i].material.Release();
                drawCalls[i].mesh.Release();
            }

            drawCalls.QuickClear();
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

    }

}