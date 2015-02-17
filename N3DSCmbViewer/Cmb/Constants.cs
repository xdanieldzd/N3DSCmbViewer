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
            GL_BYTE = 0x1400,
            GL_UNSIGNED_BYTE = 0x1401,
            GL_SHORT = 0x1402,
            GL_UNSIGNED_SHORT = 0x1403,
            GL_INT = 0x1404,
            GL_UNSIGNED_INT = 0x1405,
            GL_FLOAT = 0x1406,

            GL_UNSIGNED_BYTE_4_4_DMP = 0x6760,
            GL_UNSIGNED_4BITS_DMP = 0x6761,

            GL_UNSIGNED_SHORT_4_4_4_4 = 0x8033,
            GL_UNSIGNED_SHORT_5_5_5_1 = 0x8034,
            GL_UNSIGNED_SHORT_5_6_5 = 0x8363
        };

        public enum TextureFormats : ushort
        {
            GL_RGBA_NATIVE_DMP = 0x6752,
            GL_RGB_NATIVE_DMP = 0x6754,
            GL_ALPHA_NATIVE_DMP = 0x6756,
            GL_LUMINANCE_NATIVE_DMP = 0x6757,
            GL_LUMINANCE_ALPHA_NATIVE_DMP = 0x6758,
            GL_ETC1_RGB8_NATIVE_DMP = 0x675A,
            GL_ETC1_ALPHA_RGB8_A4_NATIVE_DMP = 0x675B
        };
    }
}
