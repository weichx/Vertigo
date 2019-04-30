using UnityEngine;

namespace Vertigo {

    public abstract class VertigoEffect {

        protected Material material;

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

        private static Material s_DefaultMaterial;

        static VertigoEffect() {
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

        public static VertigoEffect Default { get; set; }

        protected VertigoEffect(Material material) {
            this.material = material ? material : DefaultMaterial;
        }

        public virtual Material GetMaterialToDraw(int drawCallId, in VertigoState state, ShapeBatch shapeBatch, MaterialPropertyBlock block) {
            UpdateRenderSettings(state, block);
            return material;
        }

        public virtual void ReleaseMaterial(int drawCallId, Material material) { }

        public static Material DefaultMaterial {
            get {
                if (s_DefaultMaterial != null) {
                    return s_DefaultMaterial;
                }

                s_DefaultMaterial = new Material(Shader.Find("Vertigo/Default"));
                return s_DefaultMaterial;
            }
        }

        internal virtual void ModifyShapeMesh(ShapeBatch batch, int dataIndex, in MeshSlice slice, in Shape shape) { }

        public abstract int StoreState(int contextId);

        public abstract void ClearState();

        protected static void UpdateRenderSettings(in VertigoState state, MaterialPropertyBlock block) {
//            block.SetInt(shaderKey_StencilRef, state.renderSettings.stencilRefValue);
//            block.SetInt(shaderKey_StencilReadMask, state.renderSettings.stencilReadMask);
//            block.SetInt(shaderKey_StencilWriteMask, state.renderSettings.stencilWriteMask);
//            block.SetInt(shaderKey_StencilComp, (int) state.renderSettings.stencilComp);
//            block.SetInt(shaderKey_StencilPassOp, (int) state.renderSettings.stencilPassOp);
//            block.SetInt(shaderKey_StencilFailOp, (int) state.renderSettings.stencilFailOp);
//            block.SetInt(shaderKey_ColorMask, state.renderSettings.colorMask);
//            block.SetInt(shaderKey_Culling, state.renderSettings.cullMode);
//            block.SetInt(shaderKey_ZWrite, state.renderSettings.zWrite ? 1 : 0);
//            block.SetInt(shaderKey_BlendArgSrc, (int) state.renderSettings.blendArgSrc);
//            block.SetInt(shaderKey_BlendArgDst, (int) state.renderSettings.blendArgDst);
        }

        internal abstract void Apply(ShapeBatch shapeBatch, MeshSlice slice, VertigoState state, int effectDataIndex);

    }

    public abstract class VertigoEffect<T> : VertigoEffect where T : struct {

        public T data;
        private readonly StructList<T> stateBuffer;

        protected VertigoEffect(Material material) : base(material) {
            stateBuffer = new StructList<T>();
        }

        internal override void Apply(ShapeBatch shapeBatch, MeshSlice slice, VertigoState state, int effectDataIndex) {
            T effectData = default;
            if (effectDataIndex >= 0 && effectDataIndex <= stateBuffer.Count) {
                effectData = stateBuffer.Array[effectDataIndex];
            }

            Apply(shapeBatch, state, effectData, slice);
        }

        protected virtual void Apply(ShapeBatch batch, in VertigoState state, in T data, in MeshSlice meshSlice) {
            batch.AddMeshData(meshSlice);
        }

        public override int StoreState(int contextId) {
            stateBuffer.Add(data);
            return stateBuffer.Count - 1;
        }

        public override void ClearState() {
            stateBuffer.Clear();
        }

    }

}