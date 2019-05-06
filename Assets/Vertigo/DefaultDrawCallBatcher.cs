using System.Collections.Generic;
using UnityEngine;

namespace Vertigo {

    public interface IDrawCallBatcher {

        void AddDrawCall(GeometryCache cache, RangeInt shapeRange, VertigoMaterial material, in VertigoState state);

        void AddDrawCall(Mesh mesh, VertigoMaterial material, in VertigoState state);

        void Bake(int width, int height, in Matrix4x4 cameraMatrix, StructList<BatchDrawCall> output);

        void Clear();

    }

    // let Unity do all batching
    public class DefaultDrawCallBatcher : IDrawCallBatcher {

        protected readonly StructList<DrawCall> drawCallList;

        private static readonly List<Vector3> s_MeshVector3 = new List<Vector3>(0);
        private static readonly List<Vector4> s_MeshVector4 = new List<Vector4>(0);
        private static readonly List<Color> s_MeshColor = new List<Color>(0);
        private static readonly List<int> s_MeshInt = new List<int>(0);

        private static readonly StructList<Vector3> s_Vector3Scratch = new StructList<Vector3>(128);
        private static readonly StructList<Vector4> s_Vector4Scratch = new StructList<Vector4>(128);
        private static readonly StructList<Color> s_ColorScratch = new StructList<Color>(128);
        private static readonly StructList<int> s_IntScratch = new StructList<int>(256);

        protected readonly VertigoMesh.MeshPool meshPool;

        protected struct DrawCall {

            public VertigoMaterial material;
            public VertigoMesh mesh;
            public VertigoState renderState;

        }

        public DefaultDrawCallBatcher() {
            this.meshPool = new VertigoMesh.MeshPool();
            this.drawCallList = new StructList<DrawCall>();
        }

        public void Clear() {
            drawCallList.QuickClear();
        }

        public void AddDrawCall(GeometryCache cache, RangeInt shapeRange, VertigoMaterial material, in VertigoState state) {
            for (int i = shapeRange.start; i < shapeRange.end; i++) {
                AddDrawCallInternal(cache, cache.shapes[i], material, state);
            }
        }

        private void AddDrawCallInternal(GeometryCache cache, in GeometryShape shape, VertigoMaterial material, in VertigoState state) {
            int vertexStart = shape.vertexStart;
            int vertexCount = shape.vertexCount;
            int triangleStart = shape.triangleStart;
            int triangleCount = shape.triangleCount;

            s_Vector3Scratch.EnsureCapacity(vertexCount);
            s_Vector4Scratch.EnsureCapacity(vertexCount);
            s_ColorScratch.EnsureCapacity(vertexCount);
            s_IntScratch.EnsureCapacity(triangleCount);

            VertigoMesh vertigoMesh = meshPool.GetDynamic();
            Mesh mesh = vertigoMesh.mesh;

            s_Vector3Scratch.SetFromRange(cache.positions.array, vertexStart, vertexCount);
            ListAccessor<Vector3>.SetArray(s_MeshVector3, s_Vector3Scratch.array, vertexCount);
            mesh.SetVertices(s_MeshVector3);

            s_Vector3Scratch.SetFromRange(cache.normals.array, vertexStart, vertexCount);
            ListAccessor<Vector3>.SetArray(s_MeshVector3, s_Vector3Scratch.array, vertexCount);
            mesh.SetNormals(s_MeshVector3);

            s_Vector4Scratch.SetFromRange(cache.texCoord0.array, vertexStart, vertexCount);
            ListAccessor<Vector4>.SetArray(s_MeshVector4, s_Vector4Scratch.array, vertexCount);
            mesh.SetUVs(0, s_MeshVector4);

            s_Vector4Scratch.SetFromRange(cache.texCoord1.array, vertexStart, vertexCount);
            ListAccessor<Vector4>.SetArray(s_MeshVector4, s_Vector4Scratch.array, vertexCount);
            mesh.SetUVs(1, s_MeshVector4);

            s_ColorScratch.SetFromRange(cache.colors.array, vertexStart, vertexCount);
            ListAccessor<Color>.SetArray(s_MeshColor, s_ColorScratch.array, vertexCount);
            mesh.SetColors(s_MeshColor);

            s_IntScratch.SetFromRange(cache.triangles.array, triangleStart, triangleCount);
            for (int i = 0; i < s_IntScratch.size; i++) {
                s_IntScratch.array[i] -= vertexStart;
            }

            ListAccessor<int>.SetArray(s_MeshInt, s_IntScratch.array, triangleCount);
            mesh.SetTriangles(s_MeshInt, 0);

            drawCallList.Add(new DrawCall() {
                mesh = vertigoMesh,
                material = material,
                renderState = state
            });
        }

        public void AddDrawCall(Mesh mesh, VertigoMaterial material, in VertigoState state) { }

        public void Bake(int width, int height, in Matrix4x4 cameraMatrix, StructList<BatchDrawCall> output) {
            int drawCallCount = drawCallList.Count;
            DrawCall[] drawCalls = drawCallList.array;
            output.EnsureAdditionalCapacity(drawCallCount);
            for (int i = 0; i < drawCallCount; i++) {
                DrawCall call = drawCalls[i];
                output.AddUnsafe(new BatchDrawCall() {
                    material = call.material,
                    mesh = call.mesh,
                    state = call.renderState
                });
            }
        }

    }

}