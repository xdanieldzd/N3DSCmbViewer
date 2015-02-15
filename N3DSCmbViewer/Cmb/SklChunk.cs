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

            foreach (Bone bone in Bones.Where(x => x.ParentBoneID != ushort.MaxValue)) bone.ParentBone = Bones.FirstOrDefault(x => x.BoneID == bone.ParentBoneID);
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

            public ushort BoneID { get; private set; }
            public ushort ParentBoneID { get; private set; }
            public float ScaleX { get; set; }
            public float ScaleY { get; set; }
            public float ScaleZ { get; set; }
            public float RotationX { get; set; }
            public float RotationY { get; set; }
            public float RotationZ { get; set; }
            public float TranslationX { get; set; }
            public float TranslationY { get; set; }
            public float TranslationZ { get; set; }

            public float UnknownMM { get; set; }

            public Bone ParentBone { get; set; }

            public Bone(byte[] data, int offset)
            {
                BoneID = BitConverter.ToUInt16(data, offset + 0x0);
                ParentBoneID = BitConverter.ToUInt16(data, offset + 0x2);
                ScaleX = BitConverter.ToSingle(BitConverter.GetBytes(BitConverter.ToUInt32(data, offset + 0x4)), 0);
                ScaleY = BitConverter.ToSingle(BitConverter.GetBytes(BitConverter.ToUInt32(data, offset + 0x8)), 0);
                ScaleZ = BitConverter.ToSingle(BitConverter.GetBytes(BitConverter.ToUInt32(data, offset + 0xC)), 0);
                RotationX = BitConverter.ToSingle(BitConverter.GetBytes(BitConverter.ToUInt32(data, offset + 0x10)), 0);
                RotationY = BitConverter.ToSingle(BitConverter.GetBytes(BitConverter.ToUInt32(data, offset + 0x14)), 0);
                RotationZ = BitConverter.ToSingle(BitConverter.GetBytes(BitConverter.ToUInt32(data, offset + 0x18)), 0);
                TranslationX = BitConverter.ToSingle(BitConverter.GetBytes(BitConverter.ToUInt32(data, offset + 0x1C)), 0);
                TranslationY = BitConverter.ToSingle(BitConverter.GetBytes(BitConverter.ToUInt32(data, offset + 0x20)), 0);
                TranslationZ = BitConverter.ToSingle(BitConverter.GetBytes(BitConverter.ToUInt32(data, offset + 0x24)), 0);

                if (BaseCTRChunk.IsMajora3D)
                    UnknownMM = BitConverter.ToSingle(BitConverter.GetBytes(BitConverter.ToUInt32(data, offset + 0x28)), 0);
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendFormat("-- {0} --\n", this.GetType().Name);
                sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,
                    "Bone ID: 0x{0:X}, Parent bone ID: 0x{1:X}\nScale XYZ: {2}, {3}, {4}\nRotation XYZ: {5}, {6}, {7}\nTranslation XYZ: {8}, {9}, {10}\n",
                    BoneID, ParentBoneID, ScaleX, ScaleY, ScaleZ, RotationX, RotationY, RotationZ, TranslationX, TranslationY, TranslationZ);
                sb.AppendLine();

                return sb.ToString();
            }

            public Matrix4 GetMatrix()
            {
                Matrix4 matrix = Matrix4.Identity;
                matrix *= Matrix4.CreateScale(ScaleX, ScaleY, ScaleZ);
                matrix *= Matrix4.CreateRotationX(RotationX);
                matrix *= Matrix4.CreateRotationY(RotationY);
                matrix *= Matrix4.CreateRotationZ(RotationZ);
                matrix *= Matrix4.CreateTranslation(TranslationX, TranslationY, TranslationZ);

                if (ParentBone != null) matrix *= ParentBone.GetMatrix();

                return matrix;
            }
        }
    }
}
