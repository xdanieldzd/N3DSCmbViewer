using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N3DSCmbViewer.Csab
{
    class Animation
    {
        CsabChunk parentCsab;

        public int Offset { get; private set; }

        public uint Unknown00 { get; private set; }
        public uint Unknown04 { get; private set; }
        public uint Unknown08 { get; private set; }
        public uint Unknown0C { get; private set; }
        public uint NumberOfUnknownStuffs { get; private set; }
        public uint Unknown14 { get; private set; }
        public uint NumberOfAnimNodes { get; private set; }
        public uint NumberOfBones { get; private set; }
        public ushort[] PerBoneIncides { get; private set; }
        public uint[] AnimNodeOffsets { get; private set; }

        public AnimNode[] AnimNodes { get; private set; }

        public Animation(CsabChunk parent, int offset)
        {
            parentCsab = parent;
            Offset = offset;

            int rofs = offset;

            Unknown00 = BitConverter.ToUInt32(parent.ChunkData, rofs);
            Unknown04 = BitConverter.ToUInt32(parent.ChunkData, rofs + 0x04);
            Unknown08 = BitConverter.ToUInt32(parent.ChunkData, rofs + 0x08);
            Unknown0C = BitConverter.ToUInt32(parent.ChunkData, rofs + 0x0C);
            NumberOfUnknownStuffs = BitConverter.ToUInt32(parent.ChunkData, rofs + 0x10);
            Unknown14 = BitConverter.ToUInt32(parent.ChunkData, rofs + 0x14);
            NumberOfAnimNodes = BitConverter.ToUInt32(parent.ChunkData, rofs + 0x18);
            NumberOfBones = BitConverter.ToUInt32(parent.ChunkData, rofs + 0x1C);

            rofs += 0x20;
            PerBoneIncides = new ushort[NumberOfBones];
            for (int i = 0; i < PerBoneIncides.Length; i++) PerBoneIncides[i] = BitConverter.ToUInt16(parent.ChunkData, rofs + (i * sizeof(ushort)));

            rofs += PerBoneIncides.Length * sizeof(ushort);
            AnimNodeOffsets = new uint[NumberOfAnimNodes];
            for (int i = 0; i < AnimNodeOffsets.Length; i++) AnimNodeOffsets[i] = BitConverter.ToUInt32(parent.ChunkData, rofs + (i * sizeof(uint)));

            AnimNodes = new AnimNode[NumberOfAnimNodes];
            for (int i = 0; i < AnimNodes.Length; i++) AnimNodes[i] = new AnimNode(parent, Offset + (int)AnimNodeOffsets[i]);
        }
    }
}
