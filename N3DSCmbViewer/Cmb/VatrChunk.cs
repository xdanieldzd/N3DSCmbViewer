using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N3DSCmbViewer.Cmb
{
    [System.Diagnostics.DebuggerDisplay("{GetType()}")]
    class VatrChunk : BaseCTRChunk
    {
        // "Vertex Attributes"?
        public override string ChunkTag { get { return "vatr"; } }

        public const int VertexArray_OoT = 0;
        public const int NormalArray_OoT = 1;
        public const int ColorArray_OoT = 2;
        public const int TextureCoordArray_OoT = 3;
        public const int BoneIndexLookupArray_OoT = 7;

        public const int VertexArray_MM = 0;
        public const int NormalArray_MM = 1;
        public const int ColorArray_MM = 3;
        public const int TextureCoordArray_MM = 4;
        public const int BoneIndexLookupArray_MM = 7;

        public byte[] Vertices { get { return Arrays[BaseCTRChunk.IsMajora3D ? VertexArray_MM : VertexArray_OoT]; } }
        public byte[] Normals { get { return Arrays[BaseCTRChunk.IsMajora3D ? NormalArray_MM : NormalArray_OoT]; } }
        public byte[] Colors { get { return Arrays[BaseCTRChunk.IsMajora3D ? ColorArray_MM : ColorArray_OoT]; } }
        public byte[] TextureCoords { get { return Arrays[BaseCTRChunk.IsMajora3D ? TextureCoordArray_MM : TextureCoordArray_OoT]; } }
        public byte[] BoneIndexLookup { get { return Arrays[BaseCTRChunk.IsMajora3D ? BoneIndexLookupArray_MM : BoneIndexLookupArray_OoT]; } }

        public uint BoneIndexLookupSize { get { return Sizes[BaseCTRChunk.IsMajora3D ? BoneIndexLookupArray_MM : BoneIndexLookupArray_OoT]; } }

        public uint MaxVertexIndexOrSomething { get; private set; }

        public uint[] Sizes { get; private set; }
        public uint[] Offsets { get; private set; }
        public byte[][] Arrays { get; private set; }

        public VatrChunk(byte[] data, int offset, BaseCTRChunk parent)
            : base(data, offset, parent)
        {
            MaxVertexIndexOrSomething = BitConverter.ToUInt32(ChunkData, 0x8);

            int arrayCount = (BaseCTRChunk.IsMajora3D ? 9 : 8);

            Sizes = new uint[arrayCount];
            Offsets = new uint[arrayCount];
            Arrays = new byte[arrayCount][];

            for (int i = 0; i < arrayCount; i++)
            {
                Sizes[i] = BitConverter.ToUInt32(ChunkData, 0xC + (i * 8));
                Offsets[i] = BitConverter.ToUInt32(ChunkData, 0x10 + (i * 8));

                Arrays[i] = new byte[Sizes[i]];
                Buffer.BlockCopy(ChunkData, (int)Offsets[i], Arrays[i], 0, (int)Sizes[i]);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("-- {0} --\n", this.GetType().Name);
            /*sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,
                "Unknown: 0x{0:X}\n" +
                "Vertex array -> Size: 0x{1:X}, Offset: 0x{2:X}\n" +
                "Normal array -> Size: 0x{3:X}, Offset: 0x{4:X}\n" +
                "Color array -> Size: 0x{5:X}, Offset: 0x{6:X}\n" +
                "Tex coord array -> Size: 0x{7:X}, Offset: 0x{8:X}\n" +
                "Unknown array 1 -> Size: 0x{9:X}, Offset: 0x{10:X}\n" +
                "Unknown array 2 -> Size: 0x{11:X}, Offset: 0x{12:X}\n" +
                "Bone index lookup array -> Size: 0x{13:X}, Offset: 0x{14:X}\n" +
                "Unknown array 4 (anim-related?) -> Size: 0x{15:X}, Offset: 0x{16:X}\n",
                MaxVertexIndexOrSomething,
                VertexArraySize, VertexArrayOffset,
                NormalArraySize, NormalArrayOffset,
                ColorArraySize, ColorArrayOffset,
                TextureCoordArraySize, TextureCoordArrayOffset,
                UnknownArray5Size, UnknownArray5Offset,
                UnknownArray6Size, UnknownArray6Offset,
                BoneIndexLookupSize, BoneIndexLookupOffset,
                UnknownArray8Size, UnknownArray8Offset);
            */
            return sb.ToString();
        }
    }
}
