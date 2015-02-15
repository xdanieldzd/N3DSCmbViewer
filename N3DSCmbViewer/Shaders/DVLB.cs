using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace N3DSCmbViewer.Shaders
{
    /*
     * Would've been impossible for me without:
     * http://www.3dbrew.org/wiki/SHBIN
     * http://3dbrew.org/wiki/Shader_Instruction_Set
     * https://github.com/smealum/aemstro
     * 
     * ...but note that my work here is 1) outdated compared to the 3dbrew wiki's current (Apr 14 2014) state and 2) doesn't -do- anything yet
     */

    class DVLB : IDisposable
    {
        public const string DVLBMagic = "DVLB";

        public bool Disposed { get; private set; }

        public string Magic { get; private set; }
        public uint NumberOfDVLEs { get; private set; }
        public uint[] DVLEOffsets { get; private set; }
        public DVLE[] DVLEs { get; private set; }

        public DVLP DVLP { get; private set; }

        public string Filename { get; set; }
        public byte[] Data { get; private set; }

        #region Constructor/Destructor/Disposal

        public DVLB(string filename)
        {
            Filename = filename;
            BinaryReader reader = new BinaryReader(File.Open(Filename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));
            Data = new byte[reader.BaseStream.Length];
            reader.Read(Data, 0, Data.Length);
            reader.Close();

            Load();
        }

        public DVLB(byte[] data, int offset, int length)
        {
            Filename = string.Empty;
            Data = new byte[length];
            Buffer.BlockCopy(data, offset, Data, 0, Data.Length);

            Load();
        }

        ~DVLB()
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

            Magic = Encoding.ASCII.GetString(Data, 0x00, 4);
            if (Magic != DVLBMagic) throw new Exception(string.Format("Trying to read data with tag '{0}' as {1}, expected tag '{2}'", Magic, this.GetType().Name, DVLBMagic));

            NumberOfDVLEs = BitConverter.ToUInt32(Data, 0x04);
            DVLEOffsets = new uint[NumberOfDVLEs];
            DVLEs = new DVLE[NumberOfDVLEs];

            for (int i = 0; i < NumberOfDVLEs; i++)
            {
                DVLEOffsets[i] = BitConverter.ToUInt32(Data, 0x08 + i * sizeof(uint));
                DVLEs[i] = new DVLE(this, DVLEOffsets[i]);
            }

            DVLP = new DVLP(this, 0x08 + (NumberOfDVLEs * sizeof(uint)));
        }
    }
}
