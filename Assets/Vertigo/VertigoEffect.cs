using System.Collections.Generic;
using UnityEngine;

namespace Vertigo {

    public abstract class VertigoEffect {

        internal Shader shader;
        internal string[] shaderKeywords;


        public VertigoEffect(Material material) {
            if (material != null) {
                this.shader = material.shader;
                this.shaderKeywords = material.shaderKeywords;
                ReadDataFromMaterial(material);
            }
            else {
                material = VertigoEffect.DefaultMaterial;
            }
        }

        private static Material s_DefaultMaterial;

        public static Material DefaultMaterial {
            get {
                if (s_DefaultMaterial != null) {
                    return s_DefaultMaterial;
                }

                s_DefaultMaterial = new Material(Shader.Find("Vertigo/Default"));
                return s_DefaultMaterial;
            }
        }


        public static VertigoEffect Default { get; set; }

        protected virtual void ReadDataFromMaterial(Material material) { }

        internal virtual void Fill(ShapeBatch batch, int dataIndex, in MeshSlice slice, in Shape shape) { }

        internal virtual void Stroke(int dataIndex, in MeshSlice slice, in Shape shape) { }

        public virtual void PopulateMaterialBlock(MaterialPropertyBlock block) { }

        public abstract int StoreState();

        public abstract void ClearState();

    }

    public abstract class VertigoEffect<T> : VertigoEffect where T : struct {

        public T data;
        private List<T> stateBuffer;

        protected VertigoEffect(Material material) : base(material) {
            stateBuffer = new List<T>(4);
        }

        internal override void Stroke(int stateId, in MeshSlice meshSlice, in Shape shape) {
            T state = default;
            if (stateId >= 0 && stateId <= stateBuffer.Count) {
                state = stateBuffer[stateId];
            }

            Stroke(state, meshSlice, shape);
        }

        internal override void Fill(ShapeBatch batch, int stateId, in MeshSlice meshSlice, in Shape shape) {
            T state = default;
            if (stateId >= 0 && stateId <= stateBuffer.Count) {
                state = stateBuffer[stateId];
            }

            Fill(batch, state, meshSlice, shape);
        }

        protected virtual void Fill(ShapeBatch batch, in T data, in MeshSlice meshSlice, in Shape shape) { }

        protected virtual void Stroke(in T data, in MeshSlice meshSlice, in Shape shape) { }

        public override int StoreState() {
            stateBuffer.Add(data);
            return stateBuffer.Count - 1;
        }

        public override void ClearState() {
            stateBuffer.Clear();
        }

    }

}