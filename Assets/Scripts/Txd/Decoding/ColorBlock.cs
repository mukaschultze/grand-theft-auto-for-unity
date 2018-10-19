using GrandTheftAuto.Diagnostics;
using GrandTheftAuto.Shared;
using UnityEngine;
using UnityTexture = UnityEngine.Texture;

namespace GrandTheftAuto.Txd.Decoding {
    public class ColorBlock : Decoder {

        public ColorBlock() {
            if(SystemInfo.supportsComputeShaders) {
                shader = ResourcesHelper.ColorBlockDecoder.Value;
                kernel = shader.FindKernel("Decode");
                shader.GetKernelThreadGroupSizes(kernel, out threadsX, out threadsY, out threadsZ);
            }
        }

        public override UnityTexture DecodeTextureWithProcessor(BufferReader reader, int width, int height, RasterFormat rasterFormat) {
            using(new Timing("Color Block Decoding")) {
                var texture = GetTexture2D(width, height, rasterFormat);
                var pixelCount = width * height;
                var colors = new Color32[pixelCount];
                var buffer = reader.ReadBytes(pixelCount * 4);

                for(int x = 0, i = 0; x < width; x++)
                    for(var y = 0; y < height; y++, i++)
                        colors[x + width * (height - y - 1)] = new Color32() {
                            b = buffer[i * 4 + 0],
                            g = buffer[i * 4 + 1],
                            r = buffer[i * 4 + 2],
                            a = buffer[i * 4 + 3]
                        };

                texture.SetPixels32(colors);
                texture.Apply(UseMipmaps, !Settings.Instance.compressTextures);

                return texture;
            }
        }

        public override UnityTexture DecodeTextureWithComputeShader(BufferReader reader, int width, int height, RasterFormat rasterFormat) {
            using(new Timing("Color Block Decoding (Compute Shader)"))
            using(var buffer = new ComputeBuffer(width * height, 4)) {
                var texture = GetRenderTexture(width, height, rasterFormat);

                buffer.SetData(reader.ReadBytes(width * height * 4));
                shader.SetInt("Width", width);
                shader.SetInt("Height", height);
                shader.SetTexture(kernel, "Result", texture);
                shader.SetBuffer(kernel, "Data", buffer);
                shader.Dispatch(kernel, width / (int)threadsX, height / (int)threadsY, (int)threadsZ);

                if(UseMipmaps)
                    texture.GenerateMips();

                return texture;
            }
        }

    }
}