using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vertigo {

    public struct BatchDrawCall {

        public VertigoMesh mesh;
        public VertigoMaterial material;
        public VertigoState state;

    }

    public class MaterialPool {

        private readonly Dictionary<string, List<VertigoMaterial>> instanceMaterialMap;
        private readonly Dictionary<string, List<VertigoMaterial>> sharedMaterialMap;
        private static readonly LightList<string> s_Keywords = new LightList<string>(4);

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

        
        public VertigoMaterial GetInstance(string materialName, string keyword0 = null, string keyword1 = null, string keyword2 = null, string keyword3 = null) {
            if(keyword0 != null) s_Keywords.Add(keyword0);    
            if(keyword1 != null) s_Keywords.Add(keyword1);    
            if(keyword2 != null) s_Keywords.Add(keyword2);    
            if(keyword3 != null) s_Keywords.Add(keyword3);
            VertigoMaterial retn = GetInstance(materialName, s_Keywords);
            s_Keywords.Clear();
            return retn;
        }
        
        public VertigoMaterial GetInstance(string materialName, IList<string> keywords) {
            if (keywords != null) {
                SortKeywords(keywords);
            }

            if (sharedMaterialMap.TryGetValue(materialName, out List<VertigoMaterial> materials)) {
                for (int i = 0; i < materials.Count; i++) {
                    if (KeywordsMatch(keywords, materials[i])) {
                        return materials[i].GetInstance();
                    }
                }

                VertigoMaterial retn = CreateMaterial(materialName, keywords);
                if (retn.material == null) {
                    return null;
                }

                materials.Add(retn);
                return retn.GetInstance();
            }
            else {
                VertigoMaterial retn = CreateMaterial(materialName, keywords);
                materials = new List<VertigoMaterial>();
                materials.Add(retn);
                sharedMaterialMap.Add(materialName, materials);
                return retn.GetInstance();
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
                if (keywords != null) {
                    for (int i = 0; i < keywords.Count; i++) {
                        mat.EnableKeyword(keywords[i]);
                    }
                }
            }

            return new VertigoMaterial(mat, keywords);
        }

        // for mostly sorted or very small arrays bubble sort is actually really fast due to cache locality
        // and a low number of passes over the input list. its absolutely horrible for input
        // that is not mostly sorted. You better be sure you know what you're doing when using this!
        // I never expect to get more than 4 or 5 keywords so this is actually better than merge / quick / whatever
        // and also doesn't use Array.sort (which can allocate and involves more indirection)
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

            if (material.keywords == null) {
                return keywords.Count == 0;
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

    }

}