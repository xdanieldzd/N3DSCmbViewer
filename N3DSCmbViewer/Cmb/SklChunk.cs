using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace N3DSCmbViewer.Cmb
{
    [System.Diagnostics.DebuggerDisplay("{GetType()}")]
    class SklChunk : BaseCTRChunk
    {
        // "Skeleton"?
        public override string ChunkTag { get { return "skl"; } }

        public uint BoneCount { get; private set; }
        public uint Unknown2 { get; private set; }

        public Bone[] Bones { get; private set; }

        public SklChunk(byte[] data, int offset, BaseCTRChunk parent)
            : base(data, offset, parent)
        {
            BoneCount = BitConverter.ToUInt32(ChunkData, 0x8);
            Unknown2 = BitConverter.ToUInt32(ChunkData, 0xC);

            Bones = new Bone[BoneCount];
            for (int i = 0; i < Bones.Length; i++) Bones[i] = new Bone(ChunkData, 0x10 + (i * (BaseCTRChunk.IsMajora3D ? Bone.DataSize_MM : Bone.DataSize_OoT)));

            foreach (Bone bone in Bones.Where(x => x.ParentBoneID != -1)) bone.ParentBone = Bones.FirstOrDefault(x => x.BoneID == bone.ParentBoneID);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("-- {0} --\n", this.GetType().Name);
            sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "Number of bones: 0x{0:X}, Unknown: 0x{1:X}\n", BoneCount, Unknown2);
            sb.AppendLine();

            foreach (Bone bone in Bones) sb.Append(bone.ToString());

            return sb.ToString();
        }

        [System.Diagnostics.DebuggerDisplay("{GetType()}")]
        public class Bone
        {
            public const int DataSize_OoT = 0x28;
            public const int DataSize_MM = 0x2C;

            public sbyte BoneID { get; private set; }
            public byte Unknown1 { get; private set; }
            public sbyte ParentBoneID { get; private set; }
            public byte Unknown2 { get; private set; }
            public Vector3 Scale { get; set; }
            public Vector3 Rotation { get; set; }
            public Vector3 Translation { get; set; }

            public uint UnknownMM { get; set; }

            public Bone ParentBone { get; set; }

            public Bone(byte[] data, int offset)
            {
                BoneID = (sbyte)data[offset + 0x0];
                Unknown1 = data[offset + 0x1];
                ParentBoneID = (sbyte)data[offset + 0x2];
                Unknown2 = data[offset + 0x3];
                Scale = new Vector3(
                    BitConverter.ToSingle(data, offset + 0x4),
                    BitConverter.ToSingle(data, offset + 0x8),
                    BitConverter.ToSingle(data, offset + 0xC));
                Rotation = new Vector3(
                    BitConverter.ToSingle(data, offset + 0x10),
                    BitConverter.ToSingle(data, offset + 0x14),
                    BitConverter.ToSingle(data, offset + 0x18));
                Translation = new Vector3(
                    BitConverter.ToSingle(data, offset + 0x1C),
                    BitConverter.ToSingle(data, offset + 0x20),
                    BitConverter.ToSingle(data, offset + 0x24));

                if (BaseCTRChunk.IsMajora3D)
                    UnknownMM = BitConverter.ToUInt32(data, offset + 0x28);
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendFormat("-- {0} --\n", this.GetType().Name);
                sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,
                    "Bone ID: {0}, Parent bone ID: {1}\nScale XYZ: {2}, {3}, {4}\nRotation XYZ: {5}, {6}, {7}\nTranslation XYZ: {8}, {9}, {10}\n",
                    BoneID, ParentBoneID, Scale.X, Scale.Y, Scale.Z, Rotation.X, Rotation.Y, Rotation.Z, Translation.X, Translation.Y, Translation.Z);
                sb.AppendLine();

                return sb.ToString();
            }

            public Matrix4 GetMatrix(bool useTrans)
            {
                Matrix4 matrix = Matrix4.Identity;
                matrix *= Matrix4.CreateScale(Scale);
                matrix *= Matrix4.CreateRotationX(Rotation.X);
                matrix *= Matrix4.CreateRotationY(Rotation.Y);
                matrix *= Matrix4.CreateRotationZ(Rotation.Z);
                if (useTrans) matrix *= Matrix4.CreateTranslation(Translation);

                if (ParentBone != null) matrix *= ParentBone.GetMatrix(useTrans);

                return matrix;
            }
        }
    }
}
