using System.Collections.Generic;
using UnityEngine;

namespace Vertigo {

    public class VertigoMaterial {

        public readonly Material material;
        internal readonly string[] keywords;
        internal readonly VertigoMaterial parent;

        internal readonly LightList<VertigoMaterial> instances;
        internal bool isActive;

        internal VertigoMaterial(Material material, IList<string> keywords) {
            this.material = material;
            this.keywords = new string[keywords.Count];
            this.instances = new LightList<VertigoMaterial>(4);
            this.isActive = true;
        }

        internal VertigoMaterial(VertigoMaterial parent) {
            this.material = new Material(parent.material);
            this.keywords = parent.keywords;
            this.parent = parent;
        }

        public bool isShared => parent == null;

        public VertigoMaterial GetInstance() {
            VertigoMaterial retn = null;
            if (instances.Count > 0) {
                retn = instances.RemoveLast();
            }
            else {
                retn = new VertigoMaterial(this);
            }

            retn.isActive = true;
            return retn;
        }

        public void Release() {
            if (isActive) {
                parent?.instances.Add(this);
                isActive = false;
            }
        }

        public void SetInt(int key, int value) {
            material.SetInt(key, value);
        }

        public void SetFloat(int key, float value) {
            material.SetFloat(key, value);
        }

    }

}