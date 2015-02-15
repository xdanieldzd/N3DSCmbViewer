using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N3DSCmbViewer.Cmb
{
    [System.Diagnostics.DebuggerDisplay("{GetType()}")]
    class MshsChunk : BaseCTRChunk
    {
        // "Meshes"?
        public override string ChunkTag { get { return "mshs"; } }

        public uint MeshCount { get; private set; }
        public ushort Unknown1 { get; private set; }
        public ushort Unknown2 { get; private set; }

        public Mesh[] Meshes { get; private set; }

        public MshsChunk(byte[] data, int offset, BaseCTRChunk parent)
            : base(data, offset, parent)
        {
            MeshCount = BitConverter.ToUInt32(ChunkData, 0x8);
            Unknown1 = BitConverter.ToUInt16(ChunkData, 0x0C);
            Unknown2 = BitConverter.ToUInt16(ChunkData, 0x0E);

            int meshDataSize = (BaseCTRChunk.IsMajora3D ? Mesh.DataSize_MM : Mesh.DataSize_OoT);

            Meshes = new Mesh[MeshCount];
            for (int i = 0; i < Meshes.Length; i++) Meshes[i] = new Mesh(ChunkData, 0x10 + (i * meshDataSize));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("-- {0} --\n", this.GetType().Name);
            sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "Number of meshes: 0x{0:X}, Unknown (1): 0x{1:X}, Unknown (2): 0x{2:X}\n", MeshCount, Unknown1, Unknown2);
            sb.AppendLine();

            foreach (Mesh mesh in Meshes) sb.Append(mesh.ToString());

            return sb.ToString();
        }

        [System.Diagnostics.DebuggerDisplay("{GetType()}")]
        public class Mesh
        {
            public const int DataSize_OoT = 0x04;
            public const int DataSize_MM = 0x0C;

            public ushort SepdID { get; private set; }
            public byte MaterialID { get; private set; }
            public byte Unknown { get; private set; }

            // MM3D only
            public uint Unknown2 { get; private set; }
            public uint Unknown3 { get; private set; }

            public Mesh(byte[] data, int offset)
            {
                SepdID = BitConverter.ToUInt16(data, offset);
                MaterialID = data[offset + 2];
                Unknown = data[offset + 3];

                if (BaseCTRChunk.IsMajora3D)
                {
                    Unknown2 = BitConverter.ToUInt32(data, offset + 4);
                    Unknown3 = BitConverter.ToUInt32(data, offset + 8);
                }
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendFormat("-- {0} --\n", this.GetType().Name);
                sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "Sepd ID: 0x{0:X}, Material ID: 0x{1:X}, Unknown: 0x{2:X}\n", SepdID, MaterialID, Unknown);
                sb.AppendLine();

                return sb.ToString();
            }
        }
    }
}
