using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace N3DSCmbViewer.Cmb
{
    static class Constants
    {
        public enum PicaDataType : ushort
        {
            Byte = 0x1400,
            UnsignedByte = 0x1401,
            Short = 0x1402,
            UnsignedShort = 0x1403,
            Int = 0x1404,
            UnsignedInt = 0x1405,
            Float = 0x1406,

            UnsignedByte44DMP = 0x6760,
            Unsigned4BitsDMP = 0x6761,

            UnsignedShort4444 = 0x8033,
            UnsignedShort5551 = 0x8034,
            UnsignedShort565 = 0x8363
        };

        public enum PicaTextureFormat : ushort
        {
            RGBANativeDMP = 0x6752,
            RGBNativeDMP = 0x6754,
            AlphaNativeDMP = 0x6756,
            LuminanceNativeDMP = 0x6757,
            LuminanceAlphaNativeDMP = 0x6758,
            ETC1RGB8NativeDMP = 0x675A,
            ETC1AlphaRGB8A4NativeDMP = 0x675B
        };

        public enum PicaTextureEnvModeCombine : ushort
        {
            Replace = TextureEnvModeCombine.Replace,
            Modulate = TextureEnvModeCombine.Modulate,
            Add = TextureEnvModeCombine.Add,
            AddSigned = TextureEnvModeCombine.AddSigned,
            Interpolate = TextureEnvModeCombine.Interpolate,
            Subtract = TextureEnvModeCombine.Subtract,
            Dot3Rgb = TextureEnvModeCombine.Dot3Rgb,
            Dot3Rgba = TextureEnvModeCombine.Dot3Rgba,
            MultAdd = 0x6401,
            AddMult = 0x6402
        };

        public enum PicaTextureEnvModeSource : ushort
        {
            PrimaryColor = TextureEnvModeSource.PrimaryColor,
            FragmentPrimaryColorDMP = 0x6210,
            FragmentSecondaryColorDMP = 0x6211,
            Texture0 = TextureEnvModeSource.Texture0,
            Texture1 = TextureEnvModeSource.Texture1,
            Texture2 = TextureEnvModeSource.Texture2,
            Texture3 = TextureEnvModeSource.Texture3,
            PreviousBufferDMP = 0x8579,
            Constant = TextureEnvModeSource.Constant,
            Previous = TextureEnvModeSource.Previous
        };

        public enum PicaTextureEnvModeOperandRgb : ushort
        {
            SrcColor = TextureEnvModeOperandRgb.SrcColor,
            OneMinusSrcColor = TextureEnvModeOperandRgb.OneMinusSrcColor,
            SrcAlpha = TextureEnvModeOperandRgb.SrcAlpha,
            OneMinusSrcAlpha = TextureEnvModeOperandRgb.OneMinusSrcAlpha,
            SrcRDMP = 0x8580,
            OneMinusSrcRDMP = 0x8583,
            SrcGDMP = 0x8581,
            OneMinusSrcGDMP = 0x8584,
            SrcBDMP = 0x8582,
            OneMinusSrcBDMP = 0x8585,
        };

        public enum PicaTextureEnvModeOperandAlpha : ushort
        {
            SrcAlpha = TextureEnvModeOperandAlpha.SrcAlpha,
            OneMinusSrcAlpha = TextureEnvModeOperandAlpha.OneMinusSrcAlpha,
            SrcRDMP = 0x8580,
            OneMinusSrcRDMP = 0x8583,
            SrcGDMP = 0x8581,
            OneMinusSrcGDMP = 0x8584,
            SrcBDMP = 0x8582,
            OneMinusSrcBDMP = 0x8585,
        };
    }
}
