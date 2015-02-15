using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using N3DSCmbViewer.Cmb;

namespace N3DSCmbViewer.Csab
{
    class AnimHandler : IDisposable
    {
        public bool Disposed { get; private set; }

        //

        public string Filename { get; set; }
        public byte[] Data { get; private set; }

        public CsabChunk Root { get; private set; }

        public AnimHandler(string filename)
        {
            Filename = filename;
            BinaryReader reader = new BinaryReader(File.Open(Filename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));
            Data = new byte[reader.BaseStream.Length];
            reader.Read(Data, 0, Data.Length);
            reader.Close();

            Load();
        }

        public AnimHandler(byte[] data, int offset, int length)
        {
            Filename = string.Empty;
            Data = new byte[length];
            Buffer.BlockCopy(data, offset, Data, 0, Data.Length);

            Load();
        }

        ~AnimHandler()
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

        public void Load()
        {
            Disposed = false;

            Root = new CsabChunk(Data, 0, null);
        }
    }
}
