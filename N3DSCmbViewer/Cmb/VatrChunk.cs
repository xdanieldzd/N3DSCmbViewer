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
        public const int BoneIndexLookupArray_OoT = 6;
        public const int BoneWeightArray_OoT = 7;

        public const int VertexArray_MM = 0;
        public const int NormalArray_MM = 1;
        public const int ColorArray_MM = 3;
        public const int TextureCoordArray_MM = 4;
        public const int BoneIndexLookupArray_MM = 7;
        public const int BoneWeightArray_MM = 8;

        public byte[] Vertices { get { return Arrays[BaseCTRChunk.IsMajora3D ? VertexArray_MM : VertexArray_OoT]; } }
        public byte[] Normals { get { return Arrays[BaseCTRChunk.IsMajora3D ? NormalArray_MM : NormalArray_OoT]; } }
        public byte[] Colors { get { return Arrays[BaseCTRChunk.IsMajora3D ? ColorArray_MM : ColorArray_OoT]; } }
        public byte[] TextureCoords { get { return Arrays[BaseCTRChunk.IsMajora3D ? TextureCoordArray_MM : TextureCoordArray_OoT]; } }
        public byte[] BoneIndexLookup { get { return Arrays[BaseCTRChunk.IsMajora3D ? BoneIndexLookupArray_MM : BoneIndexLookupArray_OoT]; } }
        public byte[] BoneWeights { get { return Arrays[BaseCTRChunk.IsMajora3D ? BoneWeightArray_MM : BoneWeightArray_OoT]; } }

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
            sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "Unknown (max vert idx?): 0x{0:X}\n", MaxVertexIndexOrSomething);
            for (int i = 0; i < Sizes.Length; i++)
            {
                sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "Array #{0} -> Size: 0x{1:X}, Offset: 0x{2:X}", i, Sizes[i], Offsets[i]);
                if (!BaseCTRChunk.IsMajora3D)
                {
                    if (i == VertexArray_OoT) sb.AppendLine(" (vertices)");
                    else if (i == NormalArray_OoT) sb.AppendLine(" (normals)");
                    else if (i == ColorArray_OoT) sb.AppendLine(" (colors)");
                    else if (i == TextureCoordArray_OoT) sb.AppendLine(" (tex coords)");
                    else if (i == BoneIndexLookupArray_OoT) sb.AppendLine(" (bone index lookup)");
                    else if (i == BoneWeightArray_OoT) sb.AppendLine(" (bone weights)");
                    else sb.AppendLine();
                }
                else
                {
                    if (i == VertexArray_MM) sb.AppendLine(" (vertices)");
                    else if (i == NormalArray_MM) sb.AppendLine(" (normals)");
                    else if (i == ColorArray_MM) sb.AppendLine(" (colors)");
                    else if (i == TextureCoordArray_MM) sb.AppendLine(" (tex coords)");
                    else if (i == BoneIndexLookupArray_MM) sb.AppendLine(" (bone index lookup)");
                    else if (i == BoneWeightArray_MM) sb.AppendLine(" (bone weights)");
                    else sb.AppendLine();
                }
            }

            return sb.ToString();
        }
    }
}
