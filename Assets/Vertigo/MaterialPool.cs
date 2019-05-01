using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vertigo {

    internal struct BatchDrawCall {

        public Mesh mesh;
        public Material material;

    }

    public class Batcher {

        private StructList<Vector3> positionList;
        private StructList<Vector3> normalList;
        private StructList<Color> colorList;
        private StructList<Vector4> textCoordList0;
        private StructList<Vector4> textCoordList1;
        private StructList<int> triangleList;

        private StructList<BatchDrawCall> drawCalls;

        public void Draw(in Matrix4x4 transform, GeometryCache cache, int shapeIdx) {

            GeometryShape shape = cache.shapes.array[shapeIdx];
            
            int vertexStart = shape.vertexStart;
            int vertexCount = shape.vertexCount;
            int triangleStart = shape.triangleStart;
            int triangleCount = shape.triangleCount;
            
            positionList.AddRange(cache.positions.array, vertexStart, vertexCount);
            normalList.AddRange(cache.normals.array, vertexStart, vertexCount);
            colorList.AddRange(cache.colors.array, vertexStart, vertexCount);
            textCoordList0.AddRange(cache.texCoord0.array, vertexStart, vertexCount);
            textCoordList1.AddRange(cache.texCoord1.array, vertexStart, vertexCount);

            int[] triangles = cache.triangles.array;
            int triangleEnd = triangleStart + triangleCount;
            
            for (int i = triangleStart; i < triangleEnd; i++) {
                    
            }
            
        }

        public Mesh Bake() { }

    }

    public class VertigoCtx {

        public MaterialPool materialPool = new MaterialPool();

        public MaterialPool MaterialPool => materialPool;

        
        public void PushRenderTarget(RenderTexture texture) { }

        public void PopRenderTarget() { }

        public void ClearRenderTarget() { }

        public void SetMaterial(VertigoMaterial material) { }

        public void Draw(GeometryCache geometry, VertigoMaterial material) {

            // do culling per shape unless material says otherwise
            for (int i = 0; i < geometry.shapes.size; i++) { }
            
            // get current render target
            // get batcher for render target
            // batcher.AddDrawCall(matrix, geometry, material);
            
        }

        public float strokeWidth;
        public Color32 strokeColor;

        public void SetStrokeColor(Color32 color) {
            this.strokeColor = color;
        }

        public void SetStrokeWidth(float strokeWidth) {
            this.strokeWidth = strokeWidth;
        }

    }

    public class MaterialPool {

        private readonly Dictionary<string, List<VertigoMaterial>> instanceMaterialMap;
        private readonly Dictionary<string, List<VertigoMaterial>> sharedMaterialMap;

        private LightList<string> sortContainer;

        public MaterialPool() {
            instanceMaterialMap = new Dictionary<string, List<VertigoMaterial>>();
            sharedMaterialMap = new Dictionary<string, List<VertigoMaterial>>();
        }

        public VertigoMaterial GetShared(string materialName, IList<string> keywords = null) {

            if (keywords != null) {
                SortKeywords(keywords);
            }

            if (sharedMaterialMap.TryGetValue(materialName, out List<VertigoMaterial> materials)) {

                for (int i = 0; i < materials.Count; i++) {
                    if (KeywordsMatch(keywords, materials[i])) {
                        return materials[i];
                    }
                }

                VertigoMaterial retn = CreateMaterial(materialName, keywords);
                if (retn.material == null) {
                    return null;
                }

                materials.Add(retn);
                return retn;
            }
            else {
                VertigoMaterial retn = CreateMaterial(materialName, keywords);
                materials = new List<VertigoMaterial>();
                materials.Add(retn);
                sharedMaterialMap.Add(materialName, materials);
                return retn;
            }
        }

        public VertigoMaterial GetInstance(string materialName, IList<string> keywords = null) {

            if (keywords != null) {
                SortKeywords(keywords);
            }

            if (sharedMaterialMap.TryGetValue(materialName, out List<VertigoMaterial> materials)) {

                for (int i = 0; i < materials.Count; i++) {
                    if (KeywordsMatch(keywords, materials[i])) {
                        return materials[i];
                    }
                }

                VertigoMaterial retn = CreateMaterial(materialName, keywords);
                if (retn.material == null) {
                    return null;
                }

                materials.Add(retn);
                return retn;
            }
            else {
                VertigoMaterial retn = CreateMaterial(materialName, keywords);
                materials = new List<VertigoMaterial>();
                materials.Add(retn);
                sharedMaterialMap.Add(materialName, materials);
                return retn;
            }
        }

        private static VertigoMaterial CreateMaterial(string materialName, IList<string> keywords) {
            Material mat = Resources.Load<Material>(materialName);
            if (mat == null) {
                Shader shader = Shader.Find(materialName);
                if (shader == null) {
                    return null;
                }

                mat = new Material(shader);
                for (int i = 0; i < keywords.Count; i++) {
                    mat.EnableKeyword(keywords[i]);
                }
            }

            return new VertigoMaterial(mat, keywords);
        }

        // for mostly sorted or very small arrays bubble sort is actually really fast due to cache locality
        // and a low number of passes over the input list. its absolutely horrible for input
        // that is not mostly sorted. You better be sure you know what you're doing when using this!
        private static void SortKeywords(IList<string> keywords) {
            int n = keywords.Count;
            do {
                int sw = 0; // last swap index

                for (int i = 0; i < n - 1; i++) {
                    if (string.CompareOrdinal(keywords[i], keywords[i + 1]) > 0) {
                        string temp = keywords[i];
                        keywords[i] = keywords[i + 1];
                        keywords[i + 1] = temp;

                        //Save swap position
                        sw = i + 1;
                    }
                }

                //We do not need to visit all elements
                //we only need to go as far as the last swap
                n = sw;
            }

            //Once n = 1 then the whole list is sorted
            while (n > 1);
        }

        private static bool KeywordsMatch(IList<string> keywords, VertigoMaterial material) {
            if (keywords == null) {
                if (material.keywords == null || material.keywords.Length == 0) {
                    return true;
                }

                return false;
            }

            if (keywords.Count != material.keywords.Length) {
                return false;
            }

            for (int i = 0; i < material.keywords.Length; i++) {
                if (keywords[i] != material.keywords[i]) {
                    return false;
                }
            }

            return true;
        }

        public static void BubbleSort<T>(T[] array, int count, IComparer<T> cmp) {
            int n = count;
            do {
                int sw = 0; // last swap index

                for (int i = 0; i < n - 1; i++) {
                    if (cmp.Compare(array[i], array[i + 1]) > 0) {
                        T temp = array[i];
                        array[i] = array[i + 1];
                        array[i + 1] = temp;

                        //Save swap position
                        sw = i + 1;
                    }
                }

                //We do not need to visit all elements
                //we only need to go as far as the last swap
                n = sw;
            }

            //Once n = 1 then the whole list is sorted
            while (n > 1);

        }

    }

}