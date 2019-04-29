using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace Vertigo.Editor {

    [CustomEditor(typeof(TextureAtlas))]
    public class TextureAtlasEditor : UnityEditor.Editor {

        public override void OnInspectorGUI() {
            TextureAtlas textureAtlas = (TextureAtlas) target;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("textures"));
            bool changed = EditorGUI.EndChangeCheck();
            if (changed) {
                Debug.Log("Changed");
            }

            SpriteAtlas atlas;
        }

        [MenuItem("Assets/Create/Vertigo Texture Atlas")]
        public static void CreateAsset() {
            ScriptableObjectUtility.CreateAsset<TextureAtlas>();
        }

    }

    public static class ScriptableObjectUtility {

        public static void CreateAsset<T>() where T : ScriptableObject {
            T asset = ScriptableObject.CreateInstance<T>();

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "") {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "") {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof(T).ToString() + ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }

    }

}