using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N3DSCmbViewer.Cmb
{
    [System.Diagnostics.DebuggerDisplay("{GetType()}")]
    class ShpChunk : BaseCTRChunk
    {
        // "Shapes"?
        public override string ChunkTag { get { return "shp"; } }

        public uint SepdCount { get; private set; }
        public uint Unknown1 { get; private set; }
        public ushort[] SepdOffsets { get; private set; }

        public SepdChunk[] SepdChunks { get; private set; }

        public ShpChunk(byte[] data, int offset, BaseCTRChunk parent)
            : base(data, offset, parent)
        {
            SepdCount = BitConverter.ToUInt32(ChunkData, 0x8);
            Unknown1 = BitConverter.ToUInt32(ChunkData, 0xC);

            SepdOffsets = new ushort[SepdCount];
            for (int i = 0; i < SepdOffsets.Length; i++) SepdOffsets[i] = BitConverter.ToUInt16(ChunkData, 0x10 + (i * 2));

            SepdChunks = new SepdChunk[SepdCount];
            for (int i = 0; i < SepdChunks.Length; i++) SepdChunks[i] = new SepdChunk(ChunkData, SepdOffsets[i], this);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("-- {0} --\n", this.GetType().Name);
            sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "Number of Sepds: 0x{0:X}, Unknown: 0x{1:X}\n", SepdCount, Unknown1);
            sb.AppendLine();

            foreach (SepdChunk sepd in SepdChunks) sb.Append(sepd.ToString());

            return sb.ToString();
        }
    }
}
