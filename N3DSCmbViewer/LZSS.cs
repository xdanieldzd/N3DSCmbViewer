using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace N3DSCmbViewer
{
    public class LZSS
    {
        /* https://github.com/lue/MM3D/blob/master/src/lzs.cpp */
        public static byte[] Decompress(byte[] arcdata)
        {
            string tag = Encoding.ASCII.GetString(arcdata, 0, 4);
            uint unknown = BitConverter.ToUInt32(arcdata, 4);
            uint decompressedSize = BitConverter.ToUInt32(arcdata, 8);
            uint compressedSize = BitConverter.ToUInt32(arcdata, 12);

            if (arcdata.Length != compressedSize + 0x10) throw new Exception("compressed size mismatch");

            List<byte> outdata = new List<byte>();
            byte[] BUFFER = new byte[4096];
            for (int i = 0; i < BUFFER.Length; i++) BUFFER[i] = 0;
            byte flags8 = 0;
            ushort writeidx = 0xFEE;
            ushort readidx = 0;
            uint fidx = 0x10;

            while (fidx < arcdata.Length)
            {
                flags8 = arcdata[fidx];
                fidx++;

                for (int i = 0; i < 8; i++)
                {
                    if ((flags8 & 1) != 0)
                    {
                        outdata.Add(arcdata[fidx]);
                        BUFFER[writeidx] = arcdata[fidx];
                        writeidx++; writeidx %= 4096;
                        fidx++;
                    }
                    else
                    {
                        readidx = arcdata[fidx];
                        fidx++;
                        readidx |= (ushort)((arcdata[fidx] & 0xF0) << 4);
                        for (int j = 0; j < (arcdata[fidx] & 0x0F) + 3; j++)
                        {
                            outdata.Add(BUFFER[readidx]);
                            BUFFER[writeidx] = BUFFER[readidx];
                            readidx++; readidx %= 4096;
                            writeidx++; writeidx %= 4096;
                        }
                        fidx++;
                    }
                    flags8 >>= 1;
                    if (fidx >= arcdata.Length) break;
                }
            }

            if (decompressedSize != outdata.Count)
                throw new Exception(string.Format("Size mismatch: got {0} bytes after decompression, expected {1}.\n", outdata.Count, decompressedSize));

            return outdata.ToArray();
        }
    }
}
