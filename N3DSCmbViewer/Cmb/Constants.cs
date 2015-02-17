using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N3DSCmbViewer.Cmb
{
    static class Constants
    {
        public enum DataTypes : ushort
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

        public enum TextureFormats : ushort
        {
            RGBANativeDMP = 0x6752,
            RGBNativeDMP = 0x6754,
            AlphaNativeDMP = 0x6756,
            LuminanceNativeDMP = 0x6757,
            LuminanceAlphaNativeDMP = 0x6758,
            ETC1RGB8NativeDMP = 0x675A,
            ETC1AlphaRGB8A4NativeDMP = 0x675B
        };
    }
}
