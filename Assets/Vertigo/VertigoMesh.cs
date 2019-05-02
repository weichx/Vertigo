using UnityEngine;

namespace Vertigo {

    public class VertigoMesh {

        internal bool isActive;
        
        public readonly Mesh mesh;
        public readonly MeshPool origin;
        
        private readonly bool isDynamic;

        public VertigoMesh(MeshPool origin, bool isDynamic) {
            this.origin = origin;
            this.mesh = new Mesh();
            this.isDynamic = isDynamic;
            this.isActive = false;
            if (isDynamic) {
                this.mesh.MarkDynamic();
            }
        }

        public void Release() {
            if (isActive) {
                isActive = false;
                mesh.Clear(true);
                if (origin != null) {
                    if (isDynamic) {
                        origin.ReleaseDynamic(this);
                    }
                    else {
                        origin.ReleaseStatic(this);
                    }
                }
            }
        }

        public class MeshPool {

            private readonly LightList<VertigoMesh> dynamicPool;
            private readonly LightList<VertigoMesh> staticPool;

            public MeshPool() {
                this.dynamicPool = new LightList<VertigoMesh>();
                this.staticPool = new LightList<VertigoMesh>();
            }

            public VertigoMesh GetDynamic() {
                VertigoMesh retn = null;
                if (dynamicPool.Count > 0) {
                    retn = dynamicPool.RemoveLast();
                }
                else {
                    retn = new VertigoMesh(this, true);
                }

                retn.isActive = true;
                return retn;
            }

            public VertigoMesh GetStatic() {
                VertigoMesh retn = null;
                if (staticPool.Count > 0) {
                    retn = staticPool.RemoveLast();
                }
                else {
                    retn = new VertigoMesh(this, false);
                }

                retn.isActive = true;
                return retn;
            }

            public void ReleaseDynamic(VertigoMesh mesh) {
                dynamicPool.Add(mesh);
            }

            public void ReleaseStatic(VertigoMesh mesh) {
                staticPool.Add(mesh);
            }

        }

    }

}