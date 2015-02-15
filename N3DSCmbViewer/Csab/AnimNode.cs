using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N3DSCmbViewer.Csab
{
    class AnimNode
    {
        public const string AnimNodeTag = "anod";

        public string Tag { get; private set; }

        public uint BoneID { get; private set; }
        //

        public AnimNode(CsabChunk parentCsab, int offset)
        {
            Tag = Encoding.ASCII.GetString(parentCsab.ChunkData, offset, 4).TrimEnd(' ');

            if (Tag != AnimNodeTag) throw new Exception(string.Format("Trying to read data with tag '{0}' as {1}, expected tag '{2}'", Tag, this.GetType().Name, AnimNodeTag));

            //
        }
    }
}
