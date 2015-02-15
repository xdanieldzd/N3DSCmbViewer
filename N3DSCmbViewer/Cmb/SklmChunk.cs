using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N3DSCmbViewer.Cmb
{
    [System.Diagnostics.DebuggerDisplay("{GetType()}")]
    class SklmChunk : BaseCTRChunk
    {
        // "Skeletal Meshes"?
        public override string ChunkTag { get { return "sklm"; } }

        public const int MshsChunkPointer = 0x08;
        public const int ShpChunkPointer = 0x0C;

        public MshsChunk MshsChunk { get; private set; }
        public ShpChunk ShpChunk { get; private set; }

        public SklmChunk(byte[] data, int offset, BaseCTRChunk parent)
            : base(data, offset, parent)
        {
            MshsChunk = new MshsChunk(ChunkData, (int)BitConverter.ToUInt32(ChunkData, MshsChunkPointer), this);
            ShpChunk = new ShpChunk(ChunkData, (int)BitConverter.ToUInt32(ChunkData, ShpChunkPointer), this);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("-- {0} --\n", this.GetType().Name);
            sb.AppendLine();

            sb.Append(MshsChunk.ToString());
            sb.Append(ShpChunk.ToString());

            return sb.ToString();
        }
    }
}
