using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace N3DSCmbViewer.Cmb
{
    [System.Diagnostics.DebuggerDisplay("{GetType()}")]
    class CmbChunk : BaseCTRChunk
    {
        // "CTR Model Binary"?
        public override string ChunkTag { get { return "cmb"; } }

        // OoT3D
        public const int SklChunkPointer_OoT = 0x24;
        public const int MatsChunkPointer_OoT = 0x28;
        public const int TexChunkPointer_OoT = 0x2C;
        public const int SklmChunkPointer_OoT = 0x30;
        public const int LutsChunkPointer_OoT = 0x34;
        public const int VatrChunkPointer_OoT = 0x38;
        public const int VertexIndicesPointer_OoT = 0x3C;
        public const int TextureDataPointer_OoT = 0x40;

        // MM3D
        public const int SklChunkPointer_MM = 0x24;
        public const int QtrsChunkPointer_MM = 0x28;
        public const int MatsChunkPointer_MM = 0x2C;
        public const int TexChunkPointer_MM = 0x30;
        public const int SklmChunkPointer_MM = 0x34;
        public const int LutsChunkPointer_MM = 0x38;
        public const int VatrChunkPointer_MM = 0x3C;
        public const int VertexIndicesPointer_MM = 0x40;
        public const int TextureDataPointer_MM = 0x44;

        public uint FileSize { get; private set; }
        public uint NumberOfChunks { get; private set; }
        public uint Unknown2 { get; private set; }
        public string CmbName { get; private set; }
        public uint NumberOfIndices { get; private set; }

        public SklChunk SklChunk { get; private set; }
        public MatsChunk MatsChunk { get; private set; }
        public TexChunk TexChunk { get; private set; }
        public SklmChunk SklmChunk { get; private set; }
        public VatrChunk VatrChunk { get; private set; }
        public byte[] Indices { get; private set; }
        public byte[] TextureData { get; private set; }

        int vtxIdxOffset, texDataOffset;

        public int TotalPrimitives { get; private set; }
        public int TotalVertices { get; private set; }
        public int TotalTexCoords { get; private set; }
        public int TotalColors { get; private set; }
        public int TotalNormals { get; private set; }

        public CmbChunk(byte[] data, int offset, BaseCTRChunk parent)
            : base(data, offset, parent)
        {
            FileSize = BitConverter.ToUInt32(ChunkData, 0x4);
            NumberOfChunks = BitConverter.ToUInt32(ChunkData, 0x8);
            Unknown2 = BitConverter.ToUInt32(ChunkData, 0xC);
            CmbName = Encoding.ASCII.GetString(ChunkData, 0x10, 16).TrimEnd('\0');
            NumberOfIndices = BitConverter.ToUInt32(ChunkData, 0x20);

            BaseCTRChunk.IsMajora3D = (NumberOfChunks == 0x0A);

            if (!BaseCTRChunk.IsMajora3D)
            {
                SklChunk = new SklChunk(ChunkData, (int)BitConverter.ToUInt32(ChunkData, SklChunkPointer_OoT), this);
                MatsChunk = new MatsChunk(ChunkData, (int)BitConverter.ToUInt32(ChunkData, MatsChunkPointer_OoT), this);
                TexChunk = new TexChunk(ChunkData, (int)BitConverter.ToUInt32(ChunkData, TexChunkPointer_OoT), this);
                SklmChunk = new SklmChunk(ChunkData, (int)BitConverter.ToUInt32(ChunkData, SklmChunkPointer_OoT), this);
                VatrChunk = new VatrChunk(ChunkData, (int)BitConverter.ToUInt32(ChunkData, VatrChunkPointer_OoT), this);

                vtxIdxOffset = (int)BitConverter.ToUInt32(ChunkData, VertexIndicesPointer_OoT);
                texDataOffset = (int)BitConverter.ToUInt32(ChunkData, TextureDataPointer_OoT);
            }
            else
            {
                SklChunk = new SklChunk(ChunkData, (int)BitConverter.ToUInt32(ChunkData, SklChunkPointer_MM), this);
                MatsChunk = new MatsChunk(ChunkData, (int)BitConverter.ToUInt32(ChunkData, MatsChunkPointer_MM), this);
                TexChunk = new TexChunk(ChunkData, (int)BitConverter.ToUInt32(ChunkData, TexChunkPointer_MM), this);
                SklmChunk = new SklmChunk(ChunkData, (int)BitConverter.ToUInt32(ChunkData, SklmChunkPointer_MM), this);
                VatrChunk = new VatrChunk(ChunkData, (int)BitConverter.ToUInt32(ChunkData, VatrChunkPointer_MM), this);

                vtxIdxOffset = (int)BitConverter.ToUInt32(ChunkData, VertexIndicesPointer_MM);
                texDataOffset = (int)BitConverter.ToUInt32(ChunkData, TextureDataPointer_MM);
            }

            Indices = new byte[NumberOfIndices * sizeof(ushort)];
            Buffer.BlockCopy(ChunkData, vtxIdxOffset, Indices, 0, Indices.Length);

            TextureData = new byte[ChunkData.Length - texDataOffset];
            Buffer.BlockCopy(ChunkData, texDataOffset, TextureData, 0, TextureData.Length);

            /* For information purposes, tho inexact if (probably) one array type isn't used by sepd (or smth) */
            TotalPrimitives = 0;
            foreach (SepdChunk sepd in SklmChunk.ShpChunk.SepdChunks) TotalPrimitives += sepd.TotalPrimitives;

            TotalVertices = TotalTexCoords = TotalColors = TotalNormals = 0;
            for (int i = 0; i < SklmChunk.ShpChunk.SepdChunks.Length; i++)
            {
                if (i == SklmChunk.ShpChunk.SepdChunks.Length - 1)
                {
                    SepdChunk sepdCurrent = SklmChunk.ShpChunk.SepdChunks[i];

                    if (VatrChunk.Sizes[VatrChunk.VertexArray_OoT] != 0) TotalVertices += (int)((VatrChunk.Sizes[VatrChunk.VertexArray_OoT] - sepdCurrent.VertexArrayOffset) / sepdCurrent.VertexSize);
                    if (VatrChunk.Sizes[VatrChunk.TextureCoordArray_OoT] != 0) TotalTexCoords += (int)((VatrChunk.Sizes[VatrChunk.TextureCoordArray_OoT] - sepdCurrent.TextureCoordArrayOffset) / sepdCurrent.TexCoordSize);
                    if (VatrChunk.Sizes[VatrChunk.ColorArray_OoT] != 0) TotalColors += (int)((VatrChunk.Sizes[VatrChunk.ColorArray_OoT] - sepdCurrent.ColorArrayOffset) / sepdCurrent.ColorSize);
                    if (VatrChunk.Sizes[VatrChunk.NormalArray_OoT] != 0) TotalNormals += (int)((VatrChunk.Sizes[VatrChunk.NormalArray_OoT] - sepdCurrent.NormalArrayOffset) / sepdCurrent.NormalSize);
                }
                else
                {
                    SepdChunk sepdCurrent = SklmChunk.ShpChunk.SepdChunks[i];
                    SepdChunk sepdNext = SklmChunk.ShpChunk.SepdChunks[i + 1];

                    if (VatrChunk.Sizes[VatrChunk.VertexArray_OoT] != 0) TotalVertices += (int)((sepdNext.VertexArrayOffset - sepdCurrent.VertexArrayOffset) / sepdCurrent.VertexSize);
                    if (VatrChunk.Sizes[VatrChunk.TextureCoordArray_OoT] != 0) TotalTexCoords += (int)((sepdNext.TextureCoordArrayOffset - sepdCurrent.TextureCoordArrayOffset) / sepdCurrent.TexCoordSize);
                    if (VatrChunk.Sizes[VatrChunk.ColorArray_OoT] != 0) TotalColors += (int)((sepdNext.ColorArrayOffset - sepdCurrent.ColorArrayOffset) / sepdCurrent.ColorSize);
                    if (VatrChunk.Sizes[VatrChunk.NormalArray_OoT] != 0) TotalNormals += (int)((sepdNext.NormalArrayOffset - sepdCurrent.NormalArrayOffset) / sepdCurrent.NormalSize);
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("-- {0} --\n", this.GetType().Name);
            sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,
                "Filesize: 0x{0:X}\nNumber of chunks: 0x{1:X}, Unknown: 0x{2:X}\nCmb name: {3}\nNumber of indices: 0x{4:X}\nIndices offset: 0x{5:X}, Texture offset: 0x{6:X}\n",
                FileSize, NumberOfChunks, Unknown2, CmbName, NumberOfIndices, vtxIdxOffset, texDataOffset);
            sb.AppendLine();

            sb.Append(SklChunk.ToString());
            sb.Append(MatsChunk.ToString());
            sb.Append(TexChunk.ToString());
            sb.Append(SklmChunk.ToString());
            sb.Append(VatrChunk.ToString());

            return sb.ToString();
        }
    }
}
