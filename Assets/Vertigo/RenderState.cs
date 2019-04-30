using UnityEngine;

namespace Vertigo {

    public struct RenderState {

        public Color strokeColor;
        public Color fillColor;
        
        public float strokeWidth;
        public float strokeOpacity;
        public float fillOpacity;
        
        public Texture2D texture0;
        public Texture2D texture1;
        public Texture2D texture2;
        public StrokePlacement strokePlacement;
        public ColorMode strokeColorMode;
        
        public RenderSettings renderSettings;

    }

}