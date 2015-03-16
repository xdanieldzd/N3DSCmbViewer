using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using N3DSCmbViewer.Cmb;

namespace N3DSCmbViewer.ZSI
{
    class Setup
    {
        public int Offset { get; private set; }

        public List<Actor> Actors { get; private set; }
        public Actor SelectedActor { get; set; }

        public ModelHandler Model { get; private set; }

        public Setup(byte[] data, int offset)
        {
            Offset = offset;

            Actors = new List<Actor>();
            SelectedActor = null;

            Model = null;

            ulong command = ulong.MaxValue;
            while (command != ZSIHandler.EndHeaderCommand)
            {
                command = BitConverter.ToUInt64(data, offset).Reverse();
                switch ((byte)(command >> 56))
                {
                    case 0x01:
                        /* Room actors */
                        byte actorCount = (byte)(command >> 48);
                        uint actorOffset = (((uint)(command & 0xFFFFFFFF)).Reverse() + ZSIHandler.CommandsOffset);
                        for (int i = 0; i < actorCount; i++)
                        {
                            Actors.Add(new Actor(data, (int)(actorOffset + i * 0x10)));
                        }
                        break;

                    case 0x0A:
                        /* Mesh header (aka ".cmb reference" in this hack of OoT64's format that Grezzo did?) */
                        /* That said, implementation here is hacky & incomplete too! */
                        uint meshHeaderOffset = (((uint)(command & 0xFFFFFFFF)).Reverse() + ZSIHandler.CommandsOffset);
                        byte meshType = data[meshHeaderOffset];
                        byte meshCount = data[meshHeaderOffset + 1];
                        uint meshEntryStart = BitConverter.ToUInt32(data, (int)(meshHeaderOffset + 4)) + ZSIHandler.CommandsOffset;
                        uint meshEntryEnd = BitConverter.ToUInt32(data, (int)(meshHeaderOffset + 8)) + ZSIHandler.CommandsOffset;

                        if (meshType != 2 && meshCount != 1) throw new Exception("Unhandled mesh type OR mesh count! (Not 0x02, 0x01)");

                        uint modelOffset1 = BitConverter.ToUInt32(data, (int)(meshEntryStart + 8));
                        uint modelOffset2 = BitConverter.ToUInt32(data, (int)(meshEntryStart + 12));

                        if (modelOffset1 != 0 && modelOffset2 != 0) throw new Exception("Unhandled model offsets! (Both are non-zero)");

                        uint modelOffset = (modelOffset1 != 0 ? modelOffset1 : modelOffset2) + ZSIHandler.CommandsOffset;

                        Model = new ModelHandler(data, (int)modelOffset, (int)(data.Length - modelOffset));
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
    }
}
