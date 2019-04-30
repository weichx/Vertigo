using System;
using UnityEngine;

namespace Vertigo {

    public class ParameterTexture {

        private int idx;
        internal Texture2D texture;
        private bool changed;
        private Color32[] buffer;
        private bool needsUpload;
        private int width;
        private int height;

        private static readonly int ShaderProperty_Tex;
        private static readonly int ShaderProperty_Width;
        private static readonly int ShaderProperty_Height;

        static ParameterTexture() {
            ShaderProperty_Tex = Shader.PropertyToID("_VertigoParameterTexture");
            ShaderProperty_Width = Shader.PropertyToID("_VertigoParameterTexture_Width");
            ShaderProperty_Height = Shader.PropertyToID("_VertigoParameterTexture_Height");
        }
        
        public ParameterTexture(int width = 32, int height = 32) {
            this.width = width;
            this.height = height;
            buffer = new Color32[width * height];
            texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            needsUpload = false;
        }

        public int Width => width;
        public int Height => height;

        public void Clear() {
            idx = 0;
            Array.Clear(buffer, 0, buffer.Length);
            needsUpload = true;
        }

        public void Upload() {
            if (needsUpload) {
                
                if (width != texture.width || height != texture.height) {
                    texture.Resize(width, height, TextureFormat.RGBA32, false);
                }

                texture.SetPixels32(buffer);
                texture.Apply(false, false);
            }

            Shader.SetGlobalTexture(ShaderProperty_Tex, texture);
            Shader.SetGlobalInt(ShaderProperty_Width, width);
            Shader.SetGlobalInt(ShaderProperty_Height, height);
        }

        public int SetParameter(Color32 color) {
            int retn = idx;
            if (buffer.Length <= retn) {
                width *= 2;
                height *= 2;
                Array.Resize(ref buffer, width * height);
            }

            needsUpload = true;
            buffer[retn] = color;
            idx++;
            return retn;
        }

        public void SetParameter(int targetIdx, TextureChannel channel, byte value) {
            if (targetIdx < 0 || targetIdx >= idx) {
                throw new Exception("Invalid target index for parameter texture." +
                                    " Tried to set index {targetIndex} but texture only has {idx}" +
                                    " indices available. Use SetParameter(value) instead to allocate a new index");
            }

            switch (channel) {
                case TextureChannel.R:
                    buffer[targetIdx].r = value;
                    break;
                case TextureChannel.G:
                    buffer[targetIdx].g = value;
                    break;
                case TextureChannel.B:
                    buffer[targetIdx].b = value;
                    break;
                case TextureChannel.A:
                    buffer[targetIdx].a = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(channel), channel, null);
            }
        }

        public void SetParameter(int targetIdx, TextureChannel channel, float value) {
            if (targetIdx < 0 || targetIdx >= idx) {
                throw new Exception("Invalid target index for parameter texture." +
                                    " Tried to set index {targetIndex} but texture only has {idx}" +
                                    " indices available. Use SetParameter(value) instead to allocate a new index");
            }

            if (value > 1) {
                value = 1;
            }
            else if (value < 0) {
                value = 0;
            }

            switch (channel) {
                case TextureChannel.R:
                    buffer[targetIdx].r = (byte) (value * byte.MaxValue);
                    break;
                case TextureChannel.G:
                    buffer[targetIdx].g = (byte) (value * byte.MaxValue);
                    break;
                case TextureChannel.B:
                    buffer[targetIdx].b = (byte) (value * byte.MaxValue);
                    break;
                case TextureChannel.A:
                    buffer[targetIdx].a = (byte) (value * byte.MaxValue);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(channel), channel, null);
            }
        }

    }

}