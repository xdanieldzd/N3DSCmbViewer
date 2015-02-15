using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace N3DSCmbViewer.Shaders
{
    class DVLE : IDisposable
    {
        public const string DVLEMagic = "DVLE";

        public enum ShaderTypes : byte { VertexShader = 0, GeometryShader = 1 };

        public bool Disposed { get; private set; }

        public string Magic { get; private set; }
        public ShaderTypes ShaderType { get; private set; }
        public uint ProgramMainOffset { get; private set; }
        public uint ProgramEndMainOffset { get; private set; }
        public uint Unknown0x10 { get; private set; }
        public uint Unknown0x14 { get; private set; }
        public uint UniformTableOffset { get; private set; }
        public uint NumberOfUniformEntries { get; private set; }
        public uint LabelTableOffset { get; private set; }
        public uint NumberOfLabelEntries { get; private set; }
        public uint UnknownTableOffset { get; private set; }
        public uint NumberOfUnknownEntries { get; private set; }
        public uint VariableTableOffset { get; private set; }
        public uint NumberOfVariableEntries { get; private set; }
        public uint SymbolTableOffset { get; private set; }
        public uint SizeOfSymbolTable { get; private set; }

        public UniformTableEntry[] UniformTable { get; private set; }
        public LabelTableEntry[] LabelTable { get; private set; }
        //unknown table
        public VariableTableEntry[] VariableTable { get; private set; }

        public DVLB ParentDVLB { get; private set; }
        public uint Offset { get; private set; }

        #region Constructor/Destructor/Disposal

        public DVLE(DVLB parent, uint offset)
        {
            ParentDVLB = parent;
            Offset = offset;

            Load();
        }

        ~DVLE()
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
                    // dispose all here
                }

                Disposed = true;
            }
        }

        #endregion

        public void Load()
        {
            Disposed = false;

            Magic = Encoding.ASCII.GetString(ParentDVLB.Data, (int)Offset + 0x00, 4);
            if (Magic != DVLEMagic) throw new Exception(string.Format("Trying to read data with tag '{0}' as {1}, expected tag '{2}'", Magic, this.GetType().Name, DVLEMagic));

            ShaderType = (ShaderTypes)ParentDVLB.Data[Offset + 0x06];
            ProgramMainOffset = BitConverter.ToUInt32(ParentDVLB.Data, (int)Offset + 0x08);
            ProgramEndMainOffset = BitConverter.ToUInt32(ParentDVLB.Data, (int)Offset + 0x0C);
            Unknown0x10 = BitConverter.ToUInt32(ParentDVLB.Data, (int)Offset + 0x10);
            Unknown0x14 = BitConverter.ToUInt32(ParentDVLB.Data, (int)Offset + 0x14);
            UniformTableOffset = BitConverter.ToUInt32(ParentDVLB.Data, (int)Offset + 0x18);
            NumberOfUniformEntries = BitConverter.ToUInt32(ParentDVLB.Data, (int)Offset + 0x1C);
            LabelTableOffset = BitConverter.ToUInt32(ParentDVLB.Data, (int)Offset + 0x20);
            NumberOfLabelEntries = BitConverter.ToUInt32(ParentDVLB.Data, (int)Offset + 0x24);
            UnknownTableOffset = BitConverter.ToUInt32(ParentDVLB.Data, (int)Offset + 0x28);
            NumberOfUnknownEntries = BitConverter.ToUInt32(ParentDVLB.Data, (int)Offset + 0x2C);
            VariableTableOffset = BitConverter.ToUInt32(ParentDVLB.Data, (int)Offset + 0x30);
            NumberOfVariableEntries = BitConverter.ToUInt32(ParentDVLB.Data, (int)Offset + 0x34);
            SymbolTableOffset = BitConverter.ToUInt32(ParentDVLB.Data, (int)Offset + 0x38);
            SizeOfSymbolTable = BitConverter.ToUInt32(ParentDVLB.Data, (int)Offset + 0x3C);

            UniformTable = new UniformTableEntry[NumberOfUniformEntries];
            for (int i = 0; i < UniformTable.Length; i++) UniformTable[i] = new UniformTableEntry(this, (int)UniformTableOffset + (i * UniformTableEntry.Size));

            LabelTable = new LabelTableEntry[NumberOfLabelEntries];
            for (int i = 0; i < LabelTable.Length; i++) LabelTable[i] = new LabelTableEntry(this, (int)LabelTableOffset + (i * LabelTableEntry.Size));

            //unknown table

            VariableTable = new VariableTableEntry[NumberOfVariableEntries];
            for (int i = 0; i < VariableTable.Length; i++) VariableTable[i] = new VariableTableEntry(this, (int)VariableTableOffset + (i * VariableTableEntry.Size));
        }

        public class UniformTableEntry
        {
            public const int Size = 0x14;

            public byte UniformID { get; private set; }
            public float X { get; private set; }
            public float Y { get; private set; }
            public float Z { get; private set; }
            public float W { get; private set; }

            public UniformTableEntry(DVLE parent, int offset)
            {
                int readOffset = (int)(parent.Offset + offset);
                UniformID = parent.ParentDVLB.Data[readOffset + 0x02];
                X = ConvertFloat24(BitConverter.ToUInt32(parent.ParentDVLB.Data, readOffset + 0x04));
                Y = ConvertFloat24(BitConverter.ToUInt32(parent.ParentDVLB.Data, readOffset + 0x08));
                Z = ConvertFloat24(BitConverter.ToUInt32(parent.ParentDVLB.Data, readOffset + 0x0C));
                W = ConvertFloat24(BitConverter.ToUInt32(parent.ParentDVLB.Data, readOffset + 0x10));
            }

            private float ConvertFloat24(uint val)
            {
                if (val == 0) return 0.0f;
                uint tmp = ((val >> 16) & 0xFF) + 0x40;
                uint outp = ((tmp << 23) | ((val & 0x800000) << 31) | ((val & 0xFFFF) << 7));
                return BitConverter.ToSingle(BitConverter.GetBytes(outp), 0);
            }
        }

        public class LabelTableEntry
        {
            public const int Size = 0x10;

            public byte LabelID { get; private set; }
            public uint LocationOffset { get; private set; }
            public uint Unknown { get; private set; }
            public uint SymbolOffset { get; private set; }

            public string LabelSymbol { get; private set; }

            public LabelTableEntry(DVLE parent, int offset)
            {
                int readOffset = (int)(parent.Offset + offset);
                LabelID = parent.ParentDVLB.Data[readOffset + 0x00];
                LocationOffset = BitConverter.ToUInt32(parent.ParentDVLB.Data, readOffset + 0x04);
                Unknown = BitConverter.ToUInt32(parent.ParentDVLB.Data, readOffset + 0x08);
                SymbolOffset = BitConverter.ToUInt32(parent.ParentDVLB.Data, readOffset + 0x0C);

                readOffset = (int)(parent.Offset + parent.SymbolTableOffset + SymbolOffset);
                int len = Array.IndexOf(parent.ParentDVLB.Data, (byte)0, readOffset) - readOffset;
                LabelSymbol = Encoding.ASCII.GetString(parent.ParentDVLB.Data, readOffset, len);
            }
        }

        public class VariableTableEntry
        {
            public const int Size = 0x08;

            public uint SymbolOffset { get; private set; }
            public ushort StartRegister { get; private set; }
            public ushort EndRegister { get; private set; }

            public string VariableSymbol { get; private set; }

            public VariableTableEntry(DVLE parent, int offset)
            {
                int readOffset = (int)(parent.Offset + offset);
                SymbolOffset = BitConverter.ToUInt32(parent.ParentDVLB.Data, readOffset + 0x00);
                StartRegister = BitConverter.ToUInt16(parent.ParentDVLB.Data, readOffset + 0x04);
                EndRegister = BitConverter.ToUInt16(parent.ParentDVLB.Data, readOffset + 0x06);

                readOffset = (int)(parent.Offset + parent.SymbolTableOffset + SymbolOffset);
                int len = Array.IndexOf(parent.ParentDVLB.Data, (byte)0, readOffset) - readOffset;
                VariableSymbol = Encoding.ASCII.GetString(parent.ParentDVLB.Data, readOffset, len);
            }
        }
    }
}
