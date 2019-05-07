using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

public static class VertigoUtil {

    [StructLayout(LayoutKind.Explicit)]
    public struct Union {

        [FieldOffset(0)] public float asFloat;
        [FieldOffset(0)] public int asInt;

    }

    public static float BytesToFloat(byte b0, byte b1, byte b2, byte b3) {
        int color = b0 | b1 << 8 | b2 << 16 | b3 << 24;

        Union color2Float;
        color2Float.asFloat = 0;
        color2Float.asInt = color;
        return color2Float.asFloat;
    }

    public static float ColorToFloat(Color c) {
        int color = (int) (c.r * 255) | (int) (c.g * 255) << 8 | (int) (c.b * 255) << 16 | (int) (c.a * 255) << 24;

        Union color2Float;
        color2Float.asFloat = 0;
        color2Float.asInt = color;

        return color2Float.asFloat;
    }

    public static float ColorToFloat(Color32 c) {
        int color = c.r | c.g << 8 | c.b << 16 | c.a << 24;

        Union color2Float;
        color2Float.asFloat = 0;
        color2Float.asInt = color;

        return color2Float.asFloat;
    }

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

    public static float Vector2ToFloat(Vector4 vec) {
        float x = vec.x;
        float y = vec.y;
        x = x < 0 ? 0 : 1 < x ? 1 : x;
        y = y < 0 ? 0 : 1 < y ? 1 : y;
        const int PRECISION = (1 << 12) - 1;
        return (Mathf.FloorToInt(y * PRECISION) << 12) + Mathf.FloorToInt(x * PRECISION);
    }

    public static float PackSizeVector(Vector2 size) {
        Union color2Float;
        color2Float.asFloat = 0;
        color2Float.asInt = ((int) (size.x * 10) << 16) | ((int)(size.y * 10) & 0xffff);
        return color2Float.asFloat;
    }

}