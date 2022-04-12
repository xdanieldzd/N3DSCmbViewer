using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace N3DSCmbViewer.Cmb
{
    /* Bunch of code taken from Tharsis, makes unevenly sized textures finally correct, for example */
    [System.Diagnostics.DebuggerDisplay("{GetType()}")]
    class TexChunk : BaseCTRChunk, IDisposable
    {
        // "Texture"?
        public override string ChunkTag { get { return "tex"; } }

        public enum Formats : uint
        {
            ETC1 = Constants.PicaTextureFormat.ETC1RGB8NativeDMP,
            ETC1A4 = Constants.PicaTextureFormat.ETC1AlphaRGB8A4NativeDMP,
            RGBA8 = ((uint)Constants.PicaDataType.UnsignedByte << 16 | Constants.PicaTextureFormat.RGBANativeDMP),
            RGB8 = ((uint)Constants.PicaDataType.UnsignedByte << 16 | Constants.PicaTextureFormat.RGBNativeDMP),
            RGBA4 = ((uint)Constants.PicaDataType.UnsignedShort4444 << 16 | Constants.PicaTextureFormat.RGBANativeDMP),
            RGBA5551 = ((uint)Constants.PicaDataType.UnsignedShort5551 << 16 | Constants.PicaTextureFormat.RGBANativeDMP),
            RGB565 = ((uint)Constants.PicaDataType.UnsignedShort565 << 16 | Constants.PicaTextureFormat.RGBNativeDMP),
            LA4 = ((uint)Constants.PicaDataType.UnsignedByte44DMP << 16 | Constants.PicaTextureFormat.LuminanceAlphaNativeDMP),
            LA8 = ((uint)Constants.PicaDataType.UnsignedByte << 16 | Constants.PicaTextureFormat.LuminanceAlphaNativeDMP),
            A8 = ((uint)Constants.PicaDataType.UnsignedByte << 16 | Constants.PicaTextureFormat.AlphaNativeDMP),
            L8 = ((uint)Constants.PicaDataType.UnsignedByte << 16 | Constants.PicaTextureFormat.LuminanceNativeDMP),

            /* Needs rewrite to support formats w/ <1byte per pixel */
            L4 = ((uint)Constants.PicaDataType.Unsigned4BitsDMP << 16 | Constants.PicaTextureFormat.LuminanceNativeDMP)
        };

        public uint TextureCount { get; private set; }

        public Texture[] Textures { get; private set; }
        public bool AreTexturesLoaded { get; private set; }

        static Aglex.Texture dummyTexture;

        bool disposed;

        static TexChunk()
        {
            dummyTexture = new Aglex.Texture(Properties.Resources.DummyTexture);
        }

        public TexChunk(byte[] data, int offset, BaseCTRChunk parent)
            : base(data, offset, parent)
        {
            TextureCount = BitConverter.ToUInt32(ChunkData, 0x8);

            Textures = new Texture[TextureCount];
            for (int i = 0; i < Textures.Length; i++) Textures[i] = new Texture(ChunkData, 0xC + (i * 0x24));

            AreTexturesLoaded = false;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("-- {0} --\n", this.GetType().Name);
            sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "Number of textures: 0x{0:X}\n", TextureCount);
            sb.AppendLine();

            foreach (Texture texture in Textures) sb.Append(texture.ToString());

            return sb.ToString();
        }

        ~TexChunk()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    foreach (Texture tex in Textures) if (GL.IsTexture(tex.GLID) && tex.GLID != dummyTexture.GetTextureID()) GL.DeleteTexture(tex.GLID);
                    dummyTexture.Dispose();
                }

                disposed = true;
            }
        }

        public void LoadTextures()
        {
            foreach (Texture tex in Textures) tex.Convert((Parent as CmbChunk).TextureData);

            AreTexturesLoaded = true;
        }

        [System.Diagnostics.DebuggerDisplay("{GetType()}")]
        public class Texture
        {
            [DllImport("ETC1Lib.dll", EntryPoint = "ConvertETC1", CallingConvention = CallingConvention.Cdecl)]
            private static extern void ConvertETC1(IntPtr dataOut, ref UInt32 dataOutSize, IntPtr dataIn, UInt16 width, UInt16 height, bool alpha);

            Dictionary<Formats, byte> bytesPerPixel = new Dictionary<Formats, byte>()
            {
                { Formats.ETC1, 4 },
                { Formats.ETC1A4, 4 },
                { Formats.RGBA8, 4 },
                { Formats.RGB8, 3 },
                { Formats.RGBA5551, 2 },
                { Formats.RGB565, 2 },
                { Formats.RGBA4, 2 },
                { Formats.LA8, 2 },
                { Formats.LA4, 1 },
                { Formats.L8, 1 },
                { Formats.A8, 1 },
            };

            static readonly int[] Convert5To8 =
            {
                0x00, 0x08, 0x10, 0x18, 0x20, 0x29, 0x31, 0x39,
                0x41, 0x4A, 0x52, 0x5A, 0x62, 0x6A, 0x73, 0x7B,
                0x83, 0x8B, 0x94, 0x9C, 0xA4, 0xAC, 0xB4, 0xBD,
                0xC5, 0xCD, 0xD5, 0xDE, 0xE6, 0xEE, 0xF6, 0xFF
            };

            public uint DataLength { get; private set; }
            public ushort Unknown04 { get; private set; }
            public ushort Unknown06 { get; private set; }
            public ushort Width { get; private set; }
            public ushort Height { get; private set; }
            public Formats Format { get; private set; }
            public uint DataOffset { get; private set; }
            public string Name { get; private set; }

            public int GLID { get; private set; }

            public Bitmap TexImage { get; private set; }

            public Texture(byte[] data, int offset)
            {
                DataLength = BitConverter.ToUInt32(data, offset);
                Unknown04 = BitConverter.ToUInt16(data, offset + 0x4);
                Unknown06 = BitConverter.ToUInt16(data, offset + 0x6);
                Width = BitConverter.ToUInt16(data, offset + 0x8);
                Height = BitConverter.ToUInt16(data, offset + 0xA);
                Format = (Formats)BitConverter.ToUInt32(data, offset + 0xC);
                DataOffset = BitConverter.ToUInt32(data, offset + 0x10);
                Name = Encoding.ASCII.GetString(data, offset + 0x14, 16).TrimEnd('\0');
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendFormat("-- {0} --\n", this.GetType().Name);
                sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,
                    "Data length: 0x{0:X}, Unknown (1): 0x{1:X}, Unknown (2): 0x{2:X}\nDimensions: {3}*{4} px\nFormat: {5}, Offset: 0x{6:X}\nName: {7}\n",
                    DataLength, Unknown04, Unknown06, Width, Height, Format, DataOffset, Name);
                sb.AppendLine();

                return sb.ToString();
            }

            public void Convert(byte[] originalTexData)
            {
                TexImage = new Bitmap(Width, Height);

                byte[] textureData = new byte[DataLength];
                System.Buffer.BlockCopy(originalTexData, (int)DataOffset, textureData, 0, textureData.Length);

                if (!Enum.IsDefined(typeof(Formats), Format) || !bytesPerPixel.ContainsKey(Format))
                {
                    /* Unknown/unsupported */
                    GLID = dummyTexture.GetTextureID();
                    return;
                }

                MemoryStream ms = new MemoryStream(originalTexData, (int)DataOffset, (int)DataLength);
                BinaryReader reader = new BinaryReader(ms);

                if (Format == Formats.ETC1 || Format == Formats.ETC1A4)
                {
                    try
                    {
                        /* Get compressed data & handle to it */
                        ushort[] input = new ushort[textureData.Length / sizeof(ushort)];
                        System.Buffer.BlockCopy(textureData, 0, input, 0, textureData.Length);
                        GCHandle pInput = GCHandle.Alloc(input, GCHandleType.Pinned);

                        /* Marshal data around, invoke ETC1Lib.dll for conversion, etc */
                        UInt32 dataSize = 0;
                        UInt16 marshalWidth = (ushort)Width, marshalHeight = (ushort)Height;
                        ConvertETC1(IntPtr.Zero, ref dataSize, IntPtr.Zero, marshalWidth, marshalHeight, (Format == Formats.ETC1A4));
                        uint[] output = new uint[dataSize];
                        GCHandle pOutput = GCHandle.Alloc(output, GCHandleType.Pinned);
                        ConvertETC1(pOutput.AddrOfPinnedObject(), ref dataSize, pInput.AddrOfPinnedObject(), marshalWidth, marshalHeight, (Format == Formats.ETC1A4));
                        pOutput.Free();
                        pInput.Free();

                        /* Unscramble if needed // could probably be done in ETC1Lib.dll, it's probably pretty damn ugly, but whatever... */
                        /* Non-square code blocks could need some cleanup, verification, etc. as well... */
                        uint[] finalized = new uint[output.Length];

                        if (marshalWidth == marshalHeight)
                        {
                            /* Perfect square, just copy over */
                            System.Buffer.BlockCopy(output, 0, finalized, 0, finalized.Length);
                        }
                        else if (marshalWidth > marshalHeight)
                        {
                            /* Wider than tall */
                            int numBlocks = (Math.Max(marshalWidth, marshalHeight) / Math.Min(marshalWidth, marshalHeight));
                            int rowNumBytes = (marshalWidth << 5);
                            int blockNumBytes = (rowNumBytes / numBlocks);
                            int lineNumBytes = (blockNumBytes / 8);
                            int source = 0, target = 0;

                            for (int y = 0; y < marshalHeight / 8; y++)
                            {
                                for (int b = 0; b < numBlocks; b++)
                                {
                                    for (int y2 = 0; y2 < 8; y2++)
                                    {
                                        source = (y * rowNumBytes) + (b * blockNumBytes) + (y2 * lineNumBytes);
                                        target = (y * rowNumBytes) + (y2 * lineNumBytes * numBlocks) + (b * lineNumBytes);
                                        System.Buffer.BlockCopy(output, source, finalized, target, lineNumBytes);
                                    }
                                }
                            }
                        }
                        else
                        {
                            /* Taller than wide */
                            int factor = (marshalHeight / marshalWidth);
                            int lineSize = (marshalWidth * 4);
                            int readOffset = 0, writeOffset = 0;

                            while (readOffset < output.Length)
                            {
                                for (int t = 0; t < 8; t++)
                                {
                                    for (int i = 0; i < factor; i++)
                                    {
                                        System.Buffer.BlockCopy(output, readOffset, finalized, writeOffset + (lineSize * 8 * i) + (t * lineSize), lineSize);
                                        readOffset += lineSize;
                                    }
                                }
                                writeOffset += (lineSize * factor * 8);
                            }
                        }

                        /* Finally create texture bitmap from decompressed/unscrambled data */
                        BitmapData bmpData = TexImage.LockBits(new Rectangle(0, 0, TexImage.Width, TexImage.Height), ImageLockMode.ReadWrite, TexImage.PixelFormat);
                        byte[] pixelData = new byte[bmpData.Height * bmpData.Stride];
                        Marshal.Copy(bmpData.Scan0, pixelData, 0, pixelData.Length);

                        System.Buffer.BlockCopy(finalized, 0, pixelData, 0, pixelData.Length);
                        for (int i = 0; i < pixelData.Length; i += 4)
                        {
                            byte tmp = pixelData[i];
                            pixelData[i] = pixelData[i + 2];
                            pixelData[i + 2] = tmp;
                        }

                        Marshal.Copy(pixelData, 0, bmpData.Scan0, pixelData.Length);
                        TexImage.UnlockBits(bmpData);
                    }
                    catch (System.IndexOutOfRangeException e)
                    {
                        System.Windows.Forms.MessageBox.Show(e.ToString());
                    }
                    catch (System.AccessViolationException e)
                    {
                        System.Windows.Forms.MessageBox.Show(e.ToString());
                    }
                }
                else
                {
                    BitmapData bmpData = TexImage.LockBits(new Rectangle(0, 0, TexImage.Width, TexImage.Height), ImageLockMode.ReadWrite, TexImage.PixelFormat);
                    byte[] pixelData = new byte[bmpData.Height * bmpData.Stride];
                    Marshal.Copy(bmpData.Scan0, pixelData, 0, pixelData.Length);

                    for (int y = 0; y < Height; y += 8)
                        for (int x = 0; x < Width; x += 8)
                            DecodeTile(8, 8, x, y, ref pixelData, bmpData.Stride, reader, Format);

                    Marshal.Copy(pixelData, 0, bmpData.Scan0, pixelData.Length);
                    TexImage.UnlockBits(bmpData);
                }
                ms.Close();

                /* Make OGL texture */
                System.Drawing.Imaging.BitmapData bmpDataOgl = TexImage.LockBits(new Rectangle(0, 0, TexImage.Width, TexImage.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, TexImage.PixelFormat);

                GLID = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, GLID);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, TexImage.Width, TexImage.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmpDataOgl.Scan0);
                TexImage.UnlockBits(bmpDataOgl);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

                //TexImage.Save(@"D:\temp\mm\" + this.Name + ".png", ImageFormat.Png);
            }

            private void DecodeColor(byte[] bytes, Formats format, out int alpha, out int red, out int green, out int blue)
            {
                int val = -1;

                alpha = red = green = blue = 0xFF;

                switch (format)
                {
                    case Formats.RGBA8:
                        val = BitConverter.ToInt32(bytes, 0);
                        red = ((val >> 24) & 0xFF);
                        green = ((val >> 16) & 0xFF);
                        blue = ((val >> 8) & 0xFF);
                        alpha = (val & 0xFF);
                        break;

                    case Formats.RGB8:
                        red = bytes[2];
                        green = bytes[1];
                        blue = bytes[0];
                        break;

                    case Formats.RGBA5551:
                        val = BitConverter.ToInt16(bytes, 0);
                        red = Convert5To8[(val >> 11) & 0x1F];
                        green = Convert5To8[(val >> 6) & 0x1F];
                        blue = Convert5To8[(val >> 1) & 0x1F];
                        alpha = (val & 0x0001) == 1 ? 0xFF : 0x00;
                        break;

                    case Formats.RGB565:
                        val = BitConverter.ToInt16(bytes, 0);
                        red = Convert5To8[(val >> 11) & 0x1F];
                        green = ((val >> 5) & 0x3F) * 4;
                        blue = Convert5To8[val & 0x1F];
                        break;

                    case Formats.RGBA4:
                        val = BitConverter.ToInt16(bytes, 0);
                        red = (((val >> 12) << 4) & 0xFF);
                        green = (((val >> 8) << 4) & 0xFF);
                        blue = (((val >> 4) << 4) & 0xFF);
                        alpha = ((val << 4) & 0xFF);
                        break;

                    case Formats.LA8:
                        val = BitConverter.ToInt16(bytes, 0);
                        red = green = blue = ((val >> 8) & 0xFF);
                        alpha = (val & 0xFF);
                        break;

                    case Formats.LA4:
                        val = bytes[0];
                        red = green = blue = (((val >> 4) << 4) & 0xFF);
                        alpha = (((val & 0xF) << 4) & 0xFF);
                        break;

                    case Formats.L8:
                        alpha = 0xFF;
                        red = green = blue = bytes[0];
                        break;

                    case Formats.A8:
                        alpha = bytes[0];
                        red = green = blue = 0xFF;
                        break;

                    case Formats.L4:
                        /* Temp, not yet working & probably wrong */
                        val = bytes[0];
                        red = green = blue = (((val >> 4) << 4) & 0xFF);
                        red = green = blue = (((val & 0xF) << 4) & 0xFF);
                        break;
                }
            }

            private void DecodeTile(int iconSize, int tileSize, int ax, int ay, ref byte[] pixelData, int stride, BinaryReader reader, Formats format)
            {
                if (tileSize == 0)
                {
                    byte[] bytes = new byte[bytesPerPixel[format]];
                    System.Buffer.BlockCopy(reader.ReadBytes(bytes.Length), 0, bytes, 0, bytes.Length);

                    int alpha, red, green, blue;
                    DecodeColor(bytes, format, out alpha, out red, out green, out blue);

                    pixelData[(ay * stride) + (ax * (stride / Width)) + 2] = (byte)red;
                    pixelData[(ay * stride) + (ax * (stride / Width)) + 1] = (byte)green;
                    pixelData[(ay * stride) + (ax * (stride / Width))] = (byte)blue;
                    pixelData[(ay * stride) + (ax * (stride / Width)) + 3] = (byte)alpha;
                }
                else
                    for (var y = 0; y < iconSize; y += tileSize)
                        for (var x = 0; x < iconSize; x += tileSize)
                            DecodeTile(tileSize, tileSize / 2, x + ax, y + ay, ref pixelData, stride, reader, format);
            }
        }
    }
}
