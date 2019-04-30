using System;
using UnityEngine;

namespace Vertigo {

    [Flags]
    public enum VertexChannel {

        None = 0,
        Position = 1 << 0,
        Normal = 1 << 1,
        TexCoord0 = 1 << 2,
        TexCoord1 = 1 << 3,
        TexCoord2 = 1 << 4,
        Color = 1 << 5,
        
        PositionTexCoordColor = Position | TexCoord0 | Color,
        PositionColor = Position | Color,
        PositionColorNormal = Position | Color | Normal,
        PositionColorNormalTexCoord0 = Position | Color | Normal | TexCoord0,

        All = Position | Normal | TexCoord0 | TexCoord1 | TexCoord2 | Color

    }
    
    public struct Vertex {

        public Vector3 position;
        public Vector3 normal;
        public Color color;
        public Vector4 texCoord0;
        public Vector4 texCoord1;
        public Vector4 texCoord2;

    }

}