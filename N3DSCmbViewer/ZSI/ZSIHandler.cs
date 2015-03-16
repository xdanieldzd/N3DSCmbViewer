using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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
        public const ulong AlternateSetupsCommand = 0x1800000000000000;
        public const ulong EndHeaderCommand = 0x1400000000000000;

        public string Tag { get; private set; }
        public string CodenameString { get; private set; }

        public string Filename { get; set; }
        public byte[] Data { get; private set; }

        public List<Setup> Setups { get; private set; }
        public Setup SelectedSetup { get; set; }

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
                    foreach (Setup setup in Setups)
                    {
                        if (setup.Model != null)
                            setup.Model.Dispose();
                    }
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

            Setups = new List<Setup>();

            ulong firstCommand = BitConverter.ToUInt64(Data, (int)CommandsOffset).Reverse();
            if ((firstCommand & 0xFF00FFFF00000000) == AlternateSetupsCommand)
            {
                Setups.Add(new Setup(Data, (int)(CommandsOffset + 0x08)));

                int setupListOffset = (int)(((uint)(firstCommand & 0xFFFFFFFF)).Reverse() + CommandsOffset);
                for (int i = 0; i < (int)((firstCommand >> 48) & 0xFF); i++)
                {
                    int setupOffset = (int)(BitConverter.ToUInt32(Data, setupListOffset + i * 0x04) + CommandsOffset);
                    if (setupOffset != CommandsOffset) Setups.Add(new Setup(Data, setupOffset));
                }
            }
            else
                Setups.Add(new Setup(Data, (int)CommandsOffset));

            SelectedSetup = Setups.FirstOrDefault();
        }
    }
}
