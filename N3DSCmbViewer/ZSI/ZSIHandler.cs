using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using N3DSCmbViewer.Cmb;

namespace N3DSCmbViewer.ZSI
{
    [System.Diagnostics.DebuggerDisplay("{GetType()}")]
    class ZSIHandler : IDisposable
    {
        public bool Disposed { get; private set; }

        public const string FileTag_OoT = "ZSI\x01";
        public const string FileTag_MM = "ZSI\x09";
        public const uint CommandsOffset = 0x10;
        public const ulong EndHeaderCommand = 0x1400000000000000;

        public string Tag { get; private set; }
        public string CodenameString { get; private set; }

        public string Filename { get; set; }
        public byte[] Data { get; private set; }

        public List<Actor> Actors { get; private set; }
        public Actor SelectedActor { get; set; }

        public ModelHandler Model { get; private set; }

        public ZSIHandler(string filename)
        {
            Filename = filename;
            BinaryReader reader = new BinaryReader(File.Open(Filename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));
            Data = new byte[reader.BaseStream.Length];
            reader.Read(Data, 0, Data.Length);
            reader.Close();

            Load();
        }

        public ZSIHandler(byte[] data, int offset, int length)
        {
            Filename = string.Empty;
            Data = new byte[length];
            Buffer.BlockCopy(data, offset, Data, 0, Data.Length);

            Load();
        }

        ~ZSIHandler()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    Model.Dispose();
                }

                Disposed = true;
            }
        }

        public void Load()
        {
            Disposed = false;

            Tag = Encoding.ASCII.GetString(Data, 0, 4);
            if (Tag != FileTag_OoT && Tag != FileTag_MM)
                throw new Exception(string.Format("Trying to read chunk with tag '{0}' as {1}, expected tag '{2}' OR '{3}'", Tag, this.GetType().Name, FileTag_OoT, FileTag_MM));

            CodenameString = Encoding.ASCII.GetString(Data, 4, 12).TrimEnd('\0');

            /* WIP */
            Actors = new List<Actor>();
            SelectedActor = null;

            ulong command = ulong.MaxValue;
            int offset = (int)CommandsOffset;
            while (command != EndHeaderCommand)
            {
                command = BitConverter.ToUInt64(Data, offset).Reverse();
                switch ((byte)(command >> 56))
                {
                    case 0x01:
                        /* Room actors */
                        byte actorCount = (byte)(command >> 48);
                        uint actorOffset = (((uint)(command & 0xFFFFFFFF)).Reverse() + CommandsOffset);
                        for (int i = 0; i < actorCount; i++)
                        {
                            Actors.Add(new Actor(Data, (int)(actorOffset + i * 0x10)));
                        }
                        break;

                    case 0x0A:
                        /* Mesh header (aka ".cmb reference" in this hack of OoT64's format that Grezzo did?) */
                        /* That said, implementation here is hacky & incomplete too! */
                        uint meshHeaderOffset = (((uint)(command & 0xFFFFFFFF)).Reverse() + CommandsOffset);
                        byte meshType = Data[meshHeaderOffset];
                        byte meshCount = Data[meshHeaderOffset + 1];
                        uint meshEntryStart = BitConverter.ToUInt32(Data, (int)(meshHeaderOffset + 4)) + CommandsOffset;
                        uint meshEntryEnd = BitConverter.ToUInt32(Data, (int)(meshHeaderOffset + 8)) + CommandsOffset;

                        if (meshType != 2 && meshCount != 1) throw new Exception("Unhandled mesh type OR mesh count! (Not 0x02, 0x01)");

                        uint modelOffset1 = BitConverter.ToUInt32(Data, (int)(meshEntryStart + 8));
                        uint modelOffset2 = BitConverter.ToUInt32(Data, (int)(meshEntryStart + 12));

                        if (modelOffset1 != 0 && modelOffset2 != 0) throw new Exception("Unhandled model offsets! (Both are non-zero)");

                        uint modelOffset = (modelOffset1 != 0 ? modelOffset1 : modelOffset2) + CommandsOffset;

                        Model = new ModelHandler(Data, (int)modelOffset, (int)(Data.Length - modelOffset));
                        break;
                }
                offset += 8;
            }
        }

        public void RenderActors()
        {
            // TODO  also get rid of immediate mode here, it's just easier to do it this way for now...
            GL.PushAttrib(AttribMask.AllAttribBits);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Lighting);
            GL.PointSize(15.0f);
            GL.UseProgram(0);
            GL.Begin(PrimitiveType.Points);
            foreach (Actor actor in Actors)
            {
                if (SelectedActor != null && actor == SelectedActor)
                    GL.Color4(Color4.Yellow);
                else
                    GL.Color4(Color4.GreenYellow);

                GL.Vertex3(actor.PositionX, actor.PositionY, actor.PositionZ);
            }
            GL.End();
            GL.PopAttrib();
            // END TODO
        }

        public class Actor
        {
            /* The usual approximation */
            public ushort Number { get; private set; }
            public short PositionX { get; private set; }
            public short PositionY { get; private set; }
            public short PositionZ { get; private set; }
            public short RotationX { get; private set; }
            public short RotationY { get; private set; }
            public short RotationZ { get; private set; }
            public ushort Variable { get; private set; }

            public Actor(byte[] data, int offset)
            {
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
}
