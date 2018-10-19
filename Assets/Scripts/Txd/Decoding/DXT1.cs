using System;
using GrandTheftAuto.Diagnostics;
using GrandTheftAuto.Shared;
using UnityEngine;
using UnityTexture = UnityEngine.Texture;

namespace GrandTheftAuto.Txd.Decoding {
    public class DXT1 : Decoder {

        public DXT1() {
            if(SystemInfo.supportsComputeShaders) {
                shader = ResourcesHelper.DXT1Decoder.Value;
                kernel = shader.FindKernel("Decode");
                shader.GetKernelThreadGroupSizes(kernel, out threadsX, out threadsY, out threadsZ);
            }
        }

        public override UnityTexture DecodeTextureWithProcessor(BufferReader reader, int width, int height, RasterFormat rasterFormat) {
            using(new Timing("DXT1 Decoding")) {
                uint r, g, b;

                var texture = GetTexture2D(width, height, rasterFormat);
                var colors = new Color32[width * height];
                var c0 = new Color32();
                var c1 = new Color32();
                var c2 = new Color32();
                var c3 = new Color32();
                var black = (rasterFormat == RasterFormat.Format_1555) ? new Color32(0, 0, 0, 0) : new Color32(0, 0, 0, 255);

                reader.PrewarmBuffer((width / 4) * (height / 4) * 8);

                for(var y = 0; y < height; y += 4)
                    for(var x = 0; x < width; x += 4) {
                        var code = reader.ReadUInt32();
                        var indices = reader.ReadUInt32();

                        b = (code & 0x1F); //0000 0000 0000 0000 0000 0000 0001 1111  
                        g = (code & 0x7E0) >> 5; //0000 0000 0000 0000 0000 0111 1110 0000
                        r = (code & 0xF800) >> 11; //0000 0000 0000 0000 1111 1000 0000 0000

                        c0.r = (byte)(r << 3 | r >> 2);
                        c0.g = (byte)(g << 2 | g >> 3);
                        c0.b = (byte)(b << 3 | r >> 2);

                        b = (code & 0x1F0000) >> 16; //0000 0000 0001 1111 0000 0000 0000 0000
                        g = (code & 0x7E00000) >> 21; //0000 0111 1110 0000 0000 0000 0000 0000
                        r = (code & 0xF8000000) >> 27; //1111 1000 0000 0000 0000 0000 0000 0000

                        c1.r = (byte)(r << 3 | r >> 2);
                        c1.g = (byte)(g << 2 | g >> 3);
                        c1.b = (byte)(b << 3 | r >> 2);

                        if((code & 0xFFFF) > ((code & 0xFFFF0000) >> 16)) {
                            c2 = SumColor(MultiplyColor(c0, 0.66666666f), MultiplyColor(c1, 0.33333333f));
                            c3 = SumColor(MultiplyColor(c0, 0.33333333f), MultiplyColor(c1, 0.66666666f));
                        }
                        else {
                            c2 = SumColor(MultiplyColor(c0, 0.5f), MultiplyColor(c1, 0.5f));
                            c3 = black;
                        }

                        for(var yy = 0; yy < 4; yy++)
                            for(var xx = 0; xx < 4; xx++) {
                                switch(indices % 4) {
                                    case 0:
                                        black = c0;
                                        break;

                                    case 1:
                                        black = c1;
                                        break;

                                    case 2:
                                        black = c2;
                                        break;

                                    case 3:
                                        black = c3;
                                        break;

                                    default:
                                        throw new Exception("Invalid color index for DXT1");
                                }

                                colors[(height - 1 - y - yy) * width + (x + xx)] = black;
                                indices >>= 2;
                            }
                    }

                texture.SetPixels32(colors);
                texture.Apply(UseMipmaps, !Settings.Instance.compressTextures);

                return texture;
            }
        }

        public override UnityTexture DecodeTextureWithComputeShader(BufferReader reader, int width, int height, RasterFormat rasterFormat) {
            using(new Timing("DXT1 Decoding (Compute Shader)"))
            using(var buffer = new ComputeBuffer((width / 4) * (height / 4), 8)) {
                var texture = GetRenderTexture(width, height, rasterFormat);

                buffer.SetData(reader.ReadBytes((width / 4) * (height / 4) * 8));
                shader.SetInt("Width", width);
                shader.SetInt("Height", height);
                shader.SetBool("HasAlpha", rasterFormat == RasterFormat.Format_1555);
                shader.SetTexture(kernel, "Result", texture);
                shader.SetBuffer(kernel, "Chunks", buffer);
                shader.Dispatch(kernel, width / (int)threadsX / 4, height / (int)threadsY / 4, (int)threadsZ);

                if(UseMipmaps)
                    texture.GenerateMips();

                return texture;
            }
        }

    }
}