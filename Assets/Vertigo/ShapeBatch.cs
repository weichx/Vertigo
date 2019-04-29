using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vertigo {

    internal struct ArrayView<T> where T : struct {

        internal int size;
        internal T[] array;

        public ArrayView(List<T> list) {
            this.array = ListAccessor<T>.GetArray(list);
            this.size = list.Count;
        }

    }

    public class ShapeBatch {

        internal readonly Mesh mesh;

        // todo could compress the lists by using a cut off point, after item x the final list begins
        // keeping the lists separate makes for better copy operations
        // could also move the final lists to another object, lets see if this becomes a problem

        internal readonly LightList<Vector3> initialPosition;
        internal readonly LightList<Vector4> initialTexCoord0;
        internal readonly LightList<Vector4> initialTexCoord1;
        internal readonly LightList<Vector4> initialTexCoord2;
        internal readonly LightList<Vector3> initialNormal;
        internal readonly LightList<Color> initialColor;
        internal readonly LightList<int> initialTriangles;

        // could also hold references to the internal list arrays and do a set on them when needed
        // I don't think we actually need the list instances other than to copy to the mesh, could share / use static arrays 

        internal readonly List<Vector3> finalPositionList;
        internal readonly List<Vector4> finalUV0List;
        internal readonly List<Vector4> finalUV1List;
        internal readonly List<Vector4> finalUV2List;
        internal readonly List<Vector3> finalNormalList;
        internal readonly List<Color> finalColorList;
        internal readonly List<int> finalTriangleList;

        private ArrayView<Vector3> positionOutput;
        private ArrayView<Vector3> normalOutput;
        private ArrayView<Vector4> texCoordOutput0;
        private ArrayView<Vector4> texCoordOutput1;
        private ArrayView<Vector4> texCoordOutput2;
        private ArrayView<Color> colorOutput;
        private ArrayView<int> triangleOutput;

        internal readonly LightList<ShapeMeshData> shapeList;

        private int triangleIndex;

        internal ShapeBatch() {
            this.initialNormal = new LightList<Vector3>(64);
            this.initialColor = new LightList<Color>(64);
            this.initialPosition = new LightList<Vector3>(64);
            this.initialTexCoord0 = new LightList<Vector4>(64);
            this.initialTexCoord1 = new LightList<Vector4>(64);
            this.initialTexCoord2 = new LightList<Vector4>(64);
            this.initialTriangles = new LightList<int>(128);

            this.finalNormalList = new List<Vector3>(64);
            this.finalColorList = new List<Color>(64);
            this.finalPositionList = new List<Vector3>(64);
            this.finalUV0List = new List<Vector4>(64);
            this.finalUV1List = new List<Vector4>(64);
            this.finalUV2List = new List<Vector4>(64);
            this.finalTriangleList = new List<int>(128);

            this.positionOutput = new ArrayView<Vector3>(finalPositionList);
            this.normalOutput = new ArrayView<Vector3>(finalNormalList);
            this.colorOutput = new ArrayView<Color>(finalColorList);
            this.texCoordOutput0 = new ArrayView<Vector4>(finalUV0List);
            this.texCoordOutput1 = new ArrayView<Vector4>(finalUV1List);
            this.texCoordOutput2 = new ArrayView<Vector4>(finalUV2List);

            this.triangleOutput = new ArrayView<int>(finalTriangleList);

            shapeList = new LightList<ShapeMeshData>();
            mesh = new Mesh();
            mesh.MarkDynamic();
        }

        private int triOffset = 0;
        public void AddMeshData(in MeshSlice slice) {
            if (slice.batch != this) {
                throw new Exception("Invalid slice for batch");
            }

            int vertexCount = slice.vertexCount;
            int triangleCount = slice.triangleCount;
            EnsureAdditionalCapacity(vertexCount, triangleCount);

            AddRangeUnsafe(ref positionOutput, initialPosition.Array, slice.vertexStart, slice.vertexCount);
            AddRangeUnsafe(ref normalOutput, initialNormal.Array, slice.vertexStart, slice.vertexCount);
            AddRangeUnsafe(ref colorOutput, initialColor.Array, slice.vertexStart, slice.vertexCount);
            AddRangeUnsafe(ref texCoordOutput0, initialTexCoord0.Array, slice.vertexStart, slice.vertexCount);
            AddRangeUnsafe(ref texCoordOutput1, initialTexCoord1.Array, slice.vertexStart, slice.vertexCount);
            AddRangeUnsafe(ref texCoordOutput2, initialTexCoord2.Array, slice.vertexStart, slice.vertexCount);

            int start = slice.triangleStart;
            int end = start + slice.triangleCount;

            int[] tris = triangleOutput.array;
            int size = triangleOutput.size;
            int startSize = positionOutput.size - vertexCount;
            
            // 0, 1, 2
            // 2, 3, 0
            
            // 4, 5, 6
            // 6, 7, 4
            
            // copy triangles
            // for each one offset by vertex start?
            for (int i = start; i < end; i++) {
                tris[size++] = startSize + initialTriangles.Array[i];
            }

            triangleOutput.size += end - start;
//            AddRangeUnsafe(ref triangleOutput, initialTriangles.Array, slice.triangleStart, slice.triangleCount);
        }

        public void AddMeshData(ShapeMeshBuffer buffer) {
            int vertexCount = buffer.positionList.Count;
            int triangleCount = buffer.triangleList.Count;
            EnsureAdditionalCapacity(vertexCount, triangleCount);

            AddRangeUnsafe(ref positionOutput, buffer.positionList.Array, vertexCount);
            AddRangeUnsafe(ref normalOutput, buffer.normalList.Array, vertexCount);
            AddRangeUnsafe(ref texCoordOutput0, buffer.texCoord0List.Array, vertexCount);
            AddRangeUnsafe(ref texCoordOutput1, buffer.texCoord1List.Array, vertexCount);
            AddRangeUnsafe(ref texCoordOutput2, buffer.texCoord2List.Array, vertexCount);
            AddRangeUnsafe(ref colorOutput, buffer.colorList.Array, vertexCount);
            AddRangeUnsafe(ref triangleOutput, buffer.triangleList.Array, triangleCount);
            // todo -- handle triangles properly
            
//            int start = slice.triangleStart;
//            int end = start + slice.triangleCount;
//
//            int[] tris = triangleOutput.array;
//            int size = triangleOutput.size;
//            int startSize = positionOutput.size - vertexCount;
//            for (int i = start; i < end; i++) {
//                tris[size++] = startSize + initialTriangles.Array[i];
//            }
//
//            triangleOutput.size += end - start;
        }

        internal void Clear() {
            mesh.Clear(true);
            // todo when this is stable just set count to 0, old data is safe to overwrite
            shapeList.QuickClear();

            initialNormal.QuickClear();
            initialColor.QuickClear();
            initialPosition.QuickClear();
            initialTexCoord0.QuickClear();
            initialTexCoord1.QuickClear();
            initialTexCoord2.QuickClear();
            initialTriangles.QuickClear();

            // these actually need to be cleared unless I manage the ListAccessors for them and just set count = 0
            finalNormalList.Clear();
            finalColorList.Clear();
            finalPositionList.Clear();
            finalUV0List.Clear();
            finalUV1List.Clear();
            finalUV2List.Clear();
            finalTriangleList.Clear();
        }

        private static void AddRange<T>(ref ArrayView<T> target, T[] source, int count) where T : struct {
            if (target.array.Length < target.size + count) {
                Array.Resize(ref target.array, (target.size + count) * 2);
            }

            Array.Copy(source, 0, target.array, target.size, count);
            target.size += count;
        }

        private static void AddRangeUnsafe<T>(ref ArrayView<T> target, T[] source, int count) where T : struct {
            Array.Copy(source, 0, target.array, target.size, count);
            target.size += count;
        }

        private static void AddRangeUnsafe<T>(ref ArrayView<T> target, T[] source, int start, int count) where T : struct {
            Array.Copy(source, start, target.array, target.size, count);
            target.size += count;
        }

        private void EnsureAdditionalCapacity(int vertexCount, int triangleCount) {
            if (positionOutput.array.Length <= positionOutput.size + vertexCount) {
                int newSize = (positionOutput.size + vertexCount) * 2;
                Array.Resize(ref positionOutput.array, newSize);
                Array.Resize(ref normalOutput.array, newSize);
                Array.Resize(ref texCoordOutput0.array, newSize);
                Array.Resize(ref texCoordOutput1.array, newSize);
                Array.Resize(ref texCoordOutput2.array, newSize);
                Array.Resize(ref colorOutput.array, newSize);
            }

            if (triangleOutput.array.Length <= triangleOutput.size + triangleCount) {
                Array.Resize(ref triangleOutput.array, (triangleOutput.size + triangleCount) * 2);
            }
        }

        internal void AddShapeMeshData(in ShapeMeshData shape) {
            shapeList.Add(shape);
        }

        internal MeshRange AddQuadWithRange(in Vertex v0, in Vertex v1, in Vertex v2, in Vertex v3) {
            MeshRange retn = new MeshRange();

            if (initialPosition.Count + 4 >= initialPosition.Array.Length) {
                initialPosition.EnsureAdditionalCapacity(4);
                initialNormal.EnsureAdditionalCapacity(4);
                initialColor.EnsureAdditionalCapacity(4);
                initialTexCoord0.EnsureAdditionalCapacity(4);
                initialTexCoord1.EnsureAdditionalCapacity(4);
                initialTexCoord2.EnsureAdditionalCapacity(4);
            }

            initialTriangles.EnsureAdditionalCapacity(6);

            Vector3[] positions = initialPosition.Array;
            Vector3[] normals = initialNormal.Array;
            Color[] colors = initialColor.Array;
            Vector4[] uv0 = initialTexCoord0.Array;
            Vector4[] uv1 = initialTexCoord1.Array;
            Vector4[] uv2 = initialTexCoord2.Array;
            int[] triangles = initialTriangles.Array;

            retn.triangleRange.start = initialTriangles.Count;
            retn.vertexRange.start = initialPosition.Count;

            int idx = initialPosition.Count;
            int triIdx = initialTriangles.Count;

            positions[idx + 0] = v0.position;
            positions[idx + 1] = v1.position;
            positions[idx + 2] = v2.position;
            positions[idx + 3] = v3.position;

            normals[idx + 0] = v0.normal;
            normals[idx + 1] = v1.normal;
            normals[idx + 2] = v2.normal;
            normals[idx + 3] = v3.normal;

            colors[idx + 0] = v0.color;
            colors[idx + 1] = v1.color;
            colors[idx + 2] = v2.color;
            colors[idx + 3] = v3.color;

            uv0[idx + 0] = v0.texCoord0;
            uv0[idx + 1] = v1.texCoord0;
            uv0[idx + 2] = v2.texCoord0;
            uv0[idx + 3] = v3.texCoord0;

            uv1[idx + 0] = v0.texCoord1;
            uv1[idx + 1] = v1.texCoord1;
            uv1[idx + 2] = v2.texCoord1;
            uv1[idx + 3] = v3.texCoord1;

            uv2[idx + 0] = v0.texCoord2;
            uv2[idx + 1] = v1.texCoord2;
            uv2[idx + 2] = v2.texCoord2;
            uv2[idx + 3] = v3.texCoord2;

            triangles[triIdx + 0] = triangleIndex + 0;
            triangles[triIdx + 1] = triangleIndex + 1;
            triangles[triIdx + 2] = triangleIndex + 2;
            triangles[triIdx + 3] = triangleIndex + 2;
            triangles[triIdx + 4] = triangleIndex + 3;
            triangles[triIdx + 5] = triangleIndex + 0;

            triangleIndex += 4;

            retn.triangleRange.length = 6;
            retn.vertexRange.length = 4;

            initialPosition.Count += 4;
            initialNormal.Count += 4;
            initialColor.Count += 4;
            initialTexCoord0.Count += 4;
            initialTexCoord1.Count += 4;
            initialTexCoord2.Count += 4;
            initialTriangles.Count += 6;

            return retn;
        }

        internal Mesh GetBakedMesh() {
            // todo -- figure out how to enable / disable unused channels, probably a root level variable 

            ListAccessor<Vector3>.SetArray(finalPositionList, positionOutput.array, positionOutput.size);
            ListAccessor<Vector3>.SetArray(finalNormalList, normalOutput.array, normalOutput.size);
            ListAccessor<Color>.SetArray(finalColorList, colorOutput.array, colorOutput.size);

            ListAccessor<Vector4>.SetArray(finalUV0List, texCoordOutput0.array, texCoordOutput0.size);
            ListAccessor<Vector4>.SetArray(finalUV1List, texCoordOutput1.array, texCoordOutput1.size);
            ListAccessor<Vector4>.SetArray(finalUV2List, texCoordOutput2.array, texCoordOutput2.size);

            ListAccessor<int>.SetArray(finalTriangleList, triangleOutput.array, triangleOutput.size);

            mesh.SetVertices(finalPositionList);
            mesh.SetNormals(finalNormalList);
            mesh.SetUVs(0, finalUV0List);
            mesh.SetUVs(1, finalUV1List);
            mesh.SetUVs(2, finalUV2List);
            mesh.SetColors(finalColorList);
            mesh.SetTriangles(finalTriangleList, 0);
            return mesh;
        }

    }

}