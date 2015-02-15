using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N3DSCmbViewer.Csab
{
    class CsabChunk : BaseCTRChunk
    {
        // "CTR Skeletal Animation Binary"?
        public override string ChunkTag { get { return "csab"; } }

        public uint Unknown08 { get; private set; }
        public uint Unknown0C { get; private set; }
        public uint NumberOfAnimations { get; private set; }
        public uint[] AnimationOffsets { get; private set; }

        public Animation[] Animations { get; private set; }

        public CsabChunk(byte[] data, int offset, BaseCTRChunk parent)
            : base(data, offset, parent)
        {
            Unknown08 = BitConverter.ToUInt32(ChunkData, 0x08);
            Unknown0C = BitConverter.ToUInt32(ChunkData, 0x0C);
            NumberOfAnimations = BitConverter.ToUInt32(ChunkData, 0x10);

            AnimationOffsets = new uint[NumberOfAnimations];
            for (int i = 0; i < AnimationOffsets.Length; i++) AnimationOffsets[i] = BitConverter.ToUInt32(ChunkData, 0x14 + (i * sizeof(uint)));

            Animations = new Animation[NumberOfAnimations];
            for (int i = 0; i < Animations.Length; i++) Animations[i] = new Animation(this, (int)AnimationOffsets[i]);
        }
    }
}
