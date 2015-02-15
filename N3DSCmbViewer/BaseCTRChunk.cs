using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N3DSCmbViewer
{
    class BaseCTRChunk
    {
        /* TODO: MAKE THIS NOT STATIC IN HERE, MAKE IT CLEANER, THIS IS A STUPID WORKAROUND */
        public static bool IsMajora3D { get; set; }

        public virtual string ChunkTag { get { return default(string); } }

        public string Tag { get; private set; }
        public uint Length { get; private set; }
        public byte[] ChunkData { get; private set; }
        public BaseCTRChunk Parent { get; private set; }

        public int Offset { get; private set; }

        public BaseCTRChunk(byte[] data, int offset, BaseCTRChunk parent)
        {
            Offset = (parent != null ? parent.Offset : 0) + offset;

            Tag = Encoding.ASCII.GetString(data, offset, 4).TrimEnd(' ');
            CheckTag();

            /* TODO: CHECK THIS CRAP, MM3D CHUNK LENGTHS ARE TOO SHORT?!? CRAPPY WORKAROUND: MULTIPLY W/ 4 TO TRY AND LOAD MORE DATA */
            Length = (uint)Math.Min(data.Length - offset, BitConverter.ToUInt32(data, offset + 4) * 4);

            ChunkData = new byte[Length];
            Buffer.BlockCopy(data, offset, ChunkData, 0, ChunkData.Length);

            Parent = parent;
        }

        public void CheckTag()
        {
            if (Tag != ChunkTag) throw new Exception(string.Format("Trying to read chunk with tag '{0}' as {1}, expected tag '{2}'", Tag, this.GetType().Name, ChunkTag));
        }
    }
}
