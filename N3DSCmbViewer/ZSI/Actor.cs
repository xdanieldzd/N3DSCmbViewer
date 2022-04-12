using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N3DSCmbViewer.ZSI
{
    public class Actor
    {
        public int Offset { get; private set; }

        /* The usual approximation */
        public ushort Number { get; private set; }
        public short PositionX { get; private set; }
        public short PositionY { get; private set; }
        public short PositionZ { get; private set; }
        public short RotationX { get; private set; }
        public short RotationY { get; private set; }
        public short RotationZ { get; private set; }
        public ushort Variable { get; private set; }
        public short Test1 { get; private set; }
        public short Test2 { get; private set; }
        public short Test3 { get; private set; }

        public Actor(byte[] data, int offset)
        {
            Offset = offset;

            Number = BitConverter.ToUInt16(data, offset);
            PositionX = BitConverter.ToInt16(data, offset + 2);
            PositionY = BitConverter.ToInt16(data, offset + 4);
            PositionZ = BitConverter.ToInt16(data, offset + 6);
            RotationX = BitConverter.ToInt16(data, offset + 8);
            RotationY = BitConverter.ToInt16(data, offset + 10);
            RotationZ = BitConverter.ToInt16(data, offset + 12);
            Variable = BitConverter.ToUInt16(data, offset + 14);
        }
    }
}
