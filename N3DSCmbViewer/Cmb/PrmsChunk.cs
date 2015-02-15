using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N3DSCmbViewer.Cmb
{
    [System.Diagnostics.DebuggerDisplay("{GetType()}")]
    class PrmsChunk : BaseCTRChunk
    {
        // "Primitives"?
        public override string ChunkTag { get { return "prms"; } }

        public enum SkinningModes : ushort
        {
            SingleBone = 0x0000,
            PerVertex = 0x0001,
            PerVertexNoTrans = 0x0002
        };

        public uint Unknown1 { get; private set; }
        public SkinningModes SkinningMode { get; private set; }
        public ushort BoneIndexCount { get; private set; }
        public uint BoneIndexOffset { get; private set; }
        public uint PrmOffset { get; private set; }

        public ushort[] BoneIndices { get; private set; }

        public PrmChunk PrmChunk { get; private set; }

        public PrmsChunk(byte[] data, int offset, BaseCTRChunk parent)
            : base(data, offset, parent)
        {
            Unknown1 = BitConverter.ToUInt32(ChunkData, 0x8);
            SkinningMode = (SkinningModes)BitConverter.ToUInt16(ChunkData, 0xC);
            BoneIndexCount = BitConverter.ToUInt16(ChunkData, 0xE);
            BoneIndexOffset = BitConverter.ToUInt32(ChunkData, 0x10);
            PrmOffset = BitConverter.ToUInt32(ChunkData, 0x14);

            BoneIndices = new ushort[BoneIndexCount];
            for (int i = 0; i < BoneIndices.Length; i++) BoneIndices[i] = BitConverter.ToUInt16(ChunkData, (int)(BoneIndexOffset + (i * 2)));

            PrmChunk = new PrmChunk(ChunkData, (int)PrmOffset, this);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("-- {0} --\n", this.GetType().Name);
            sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,
                "Unknown (1): 0x{0:X}, Skinning mode: {1}\nBone index count: 0x{2:X}, Bone index offset: 0x{3:X}\nPrm chunk offset: 0x{4:X}\n",
                Unknown1, SkinningMode, BoneIndexCount, BoneIndexOffset, PrmOffset);
            sb.AppendLine();

            for (int i = 0; i < BoneIndices.Length; i++)
            {
                sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "Bone index #{0}: 0x{1:X}\n", i, BoneIndices[i]);
            }

            sb.AppendLine();

            sb.Append(PrmChunk.ToString());

            return sb.ToString();
        }
    }
}
