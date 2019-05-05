using System.Collections.Generic;
using UnityEngine;

public static class VertigoUtil {

    internal struct SpriteData {

        public Rect uvBounds;
        public Vector2[] uvs;
        public Vector2[] vertices;
        public ushort[] triangles;

    }

    private static Dictionary<int, SpriteData> s_SpriteUVMap;

    internal static SpriteData GetSpriteData(Sprite sprite) {
        if (s_SpriteUVMap == null) {
            s_SpriteUVMap = new Dictionary<int, SpriteData>();
        }
        
        SpriteData retn = new SpriteData();
        if (s_SpriteUVMap.TryGetValue(sprite.GetInstanceID(), out retn)) {
            return retn;
        }

        Vector2[] uvs = sprite.uv;

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;
        
        for (int i = 0; i < uvs.Length; i++) {
            if (uvs[i].x < minX) minX = uvs[i].x;
            if (uvs[i].y < minY) minY = uvs[i].y;
            if (uvs[i].x > maxX) maxX = uvs[i].x;
            if (uvs[i].y > maxY) maxY = uvs[i].y;
        }

        Rect uvBounds = new Rect(minX, minY, maxX - minX, maxY - minY);
        retn.uvBounds = uvBounds;
        retn.vertices = sprite.vertices;
        retn.uvs = uvs;
        retn.triangles = sprite.triangles;
        s_SpriteUVMap.Add(sprite.GetInstanceID(), retn);
        return retn;
    }
    
    public static Rect RectFromSpriteUV(Sprite sprite) {
        if (ReferenceEquals(sprite, null)) return default;
        return GetSpriteData(sprite).uvBounds;
    }

}