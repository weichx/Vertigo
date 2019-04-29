using System.Collections.Generic;
using UnityEngine;

namespace Vertigo {

    public class TextureAtlas : ScriptableObject {

        [SerializeField] public Texture2D atlas;
        public float maxWidth;
        public float maxHeight;

        public Texture2D[] textures;

        public Rect[] rectangles;

        public Dictionary<int, Rect> uvMap;
        
        public void Bake() {
            
        }

    }

}