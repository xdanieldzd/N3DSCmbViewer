using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace N3DSCmbViewer.Shaders
{
    class DVLP : IDisposable
    {
        public const string DVLPMagic = "DVLP";

        public bool Disposed { get; private set; }

        public string Magic { get; private set; }
        public uint ShaderBinaryOffset { get; private set; }
        public uint SizeOfShaderBinary { get; private set; }
        public uint ShaderInstructionExtensionTableOffset { get; private set; }
        public uint NumberOfShaderInstrExtTblEntries { get; private set; }
        public uint FilenameSymbolTableOffset { get; private set; }

        public byte[] ShaderBinary { get; private set; }
        public ulong[] ShaderInstructionExtensionTable { get; private set; }

        public DVLB ParentDVLB { get; private set; }
        public uint Offset { get; private set; }

        #region Constructor/Destructor/Disposal

        public DVLP(DVLB parent, uint offset)
        {
            ParentDVLB = parent;
            Offset = offset;

            Load();
        }

        ~DVLP()
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
            if (Magic != DVLPMagic) throw new Exception(string.Format("Trying to read data with tag '{0}' as {1}, expected tag '{2}'", Magic, this.GetType().Name, DVLPMagic));

            ShaderBinaryOffset = BitConverter.ToUInt32(ParentDVLB.Data, (int)Offset + 0x08);
            SizeOfShaderBinary = BitConverter.ToUInt32(ParentDVLB.Data, (int)Offset + 0x0C);
            ShaderInstructionExtensionTableOffset = BitConverter.ToUInt32(ParentDVLB.Data, (int)Offset + 0x10);
            NumberOfShaderInstrExtTblEntries = BitConverter.ToUInt32(ParentDVLB.Data, (int)Offset + 0x14);
            FilenameSymbolTableOffset = BitConverter.ToUInt32(ParentDVLB.Data, (int)Offset + 0x18);

            ShaderBinary = new byte[SizeOfShaderBinary * sizeof(uint)];
            Buffer.BlockCopy(ParentDVLB.Data, (int)(Offset + ShaderBinaryOffset), ShaderBinary, 0, ShaderBinary.Length);

            ShaderInstructionExtensionTable = new ulong[NumberOfShaderInstrExtTblEntries];
            Buffer.BlockCopy(ParentDVLB.Data, (int)(Offset + ShaderInstructionExtensionTableOffset), ShaderInstructionExtensionTable, 0, ShaderInstructionExtensionTable.Length * sizeof(ulong));

            // TODO  actually do stuff with the shader binary!
        }
    }
}
