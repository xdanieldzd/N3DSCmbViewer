using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace N3DSCmbViewer.Cmb
{
    [System.Diagnostics.DebuggerDisplay("{GetType()}")]
    class PrmChunk : BaseCTRChunk
    {
        // "Primitive"?
        public override string ChunkTag { get { return "prm"; } }

        public uint Unknown1 { get; private set; }
        public uint Unknown2 { get; private set; }
        public Constants.DataTypes DataType { get; private set; }
        public ushort NumberOfIndices { get; private set; }
        public ushort FirstIndex { get; private set; }

        public DrawElementsType DrawElementsType { get; private set; }
        public int ElementSize { get; private set; }

        public PrmChunk(byte[] data, int offset, BaseCTRChunk parent)
            : base(data, offset, parent)
        {
            Unknown1 = BitConverter.ToUInt32(ChunkData, 0x8);
            Unknown2 = BitConverter.ToUInt32(ChunkData, 0xC);
            DataType = (Constants.DataTypes)BitConverter.ToUInt32(ChunkData, 0x10);
            NumberOfIndices = BitConverter.ToUInt16(ChunkData, 0x14);
            FirstIndex = BitConverter.ToUInt16(ChunkData, 0x16);

            DrawElementsType = DrawElementsType.UnsignedShort;
            ElementSize = sizeof(ushort);

            switch (DataType)
            {
                case Constants.DataTypes.GL_UNSIGNED_BYTE:
                    DrawElementsType = DrawElementsType.UnsignedByte;
                    ElementSize = sizeof(byte);
                    break;
                case Constants.DataTypes.GL_UNSIGNED_SHORT:
                    DrawElementsType = DrawElementsType.UnsignedShort;
                    ElementSize = sizeof(short);
                    break;
                case Constants.DataTypes.GL_UNSIGNED_INT:
                    DrawElementsType = DrawElementsType.UnsignedInt;
                    ElementSize = sizeof(int);
                    break;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("-- {0} --\n", this.GetType().Name);
            sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,
                "Unknown (1): 0x{0:X}, Unknown (2): 0x{1:X}, Index datatype: {2}\nNumber of indices: 0x{3:X}, First index: 0x{4:X}\n",
                Unknown1, Unknown2, DataType, NumberOfIndices, FirstIndex);
            sb.AppendLine();

            return sb.ToString();
        }
    }
}
