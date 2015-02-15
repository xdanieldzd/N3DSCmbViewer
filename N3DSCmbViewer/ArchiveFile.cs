using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace N3DSCmbViewer
{
    class ArchiveFile
    {
        // http://pastebin.com/Dw7KRdSE by Twili
        // https://github.com/lue/MM3D/blob/master/src/gar.cpp by lue

        const uint magicWordZAR = 0x0152415A;   //"ZAR\x01" swapped
        const uint magicWordGAR = 0x02524147;   //"GAR\x02" swapped

        public uint MagicWord { get; private set; }
        public uint ArchiveSize { get; private set; }
        public ushort NumberOfFileTypes { get; private set; }
        public ushort NumberOfFiles { get; private set; }
        public uint FileTypesOffset { get; private set; }
        public uint FileInfoIndexOffset { get; private set; }
        public uint FileIndexOffset { get; private set; }
        public string CodenameString { get; private set; }

        public FileType[] FileTypes { get; private set; }
        public FileInfo[] FileInfos { get; private set; }
        public uint[] FileOffsets { get; private set; }

        byte[] archiveData;

        public ArchiveFile(byte[] data)
        {
            archiveData = data;
            Load();
        }

        public ArchiveFile(string fn)
        {
            BinaryReader br = new BinaryReader(File.Open(fn, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));
            archiveData = new byte[br.BaseStream.Length];
            br.Read(archiveData, 0, archiveData.Length);
            br.Close();

            Load();
        }

        private void Load()
        {
            MagicWord = BitConverter.ToUInt32(archiveData, 0);
            if (MagicWord != magicWordZAR && MagicWord != magicWordGAR)
                throw new Exception(string.Format("Trying to read ZAR or GAR with magic word 0x{0:X8}, expected 0x{1:X8} or 0x{2:X8}.", MagicWord, magicWordZAR, magicWordGAR));

            ArchiveSize = BitConverter.ToUInt32(archiveData, 0x04);
            NumberOfFileTypes = BitConverter.ToUInt16(archiveData, 0x08);
            NumberOfFiles = BitConverter.ToUInt16(archiveData, 0x0A);
            FileTypesOffset = BitConverter.ToUInt32(archiveData, 0x0C);
            FileInfoIndexOffset = BitConverter.ToUInt32(archiveData, 0x10);
            FileIndexOffset = BitConverter.ToUInt32(archiveData, 0x14);
            CodenameString = Encoding.ASCII.GetString(archiveData, 0x18, 8).TrimEnd('\0');

            FileTypes = new FileType[NumberOfFileTypes];
            for (int i = 0; i < FileTypes.Length; i++) FileTypes[i] = new FileType(this, archiveData, (int)FileTypesOffset + (i * FileType.SectionSize));

            FileInfos = new FileInfo[NumberOfFiles];
            for (int i = 0; i < FileInfos.Length; i++) FileInfos[i] = new FileInfo(this, archiveData, (int)FileInfoIndexOffset + (i * (MagicWord == magicWordZAR ? FileInfo.SectionSizeZAR : FileInfo.SectionSizeGAR)));

            FileOffsets = new uint[NumberOfFiles];
            Buffer.BlockCopy(archiveData, (int)FileIndexOffset, FileOffsets, 0, NumberOfFiles * sizeof(uint));
        }

        public byte[] GetFile(uint fileNo)
        {
            byte[] data = new byte[FileInfos[fileNo].FileSize];
            Buffer.BlockCopy(archiveData, (int)FileOffsets[fileNo], data, 0, data.Length);
            return data;
        }

        public class FileType
        {
            public const int SectionSize = 0x10;

            public ArchiveFile ParentArchive { get; private set; }

            public uint NumberOfFilesWithType { get; private set; }
            public uint FileNumberIndexOffset { get; private set; }
            public uint FileTypeNameOffset { get; private set; }
            public uint AssumedConstant { get; private set; }

            public string FileTypeName { get; private set; }
            public uint[] FileNumberIndex { get; private set; }

            public FileType(ArchiveFile parent, byte[] data, int offset)
            {
                ParentArchive = parent;

                NumberOfFilesWithType = BitConverter.ToUInt32(data, offset);
                FileNumberIndexOffset = BitConverter.ToUInt32(data, offset + 0x04);
                FileTypeNameOffset = BitConverter.ToUInt32(data, offset + 0x08);
                AssumedConstant = BitConverter.ToUInt32(data, offset + 0x0C);

                int nameLength = (int)(Array.IndexOf(data, (byte)'\0', (int)FileTypeNameOffset) - FileTypeNameOffset);
                if (nameLength > 0) FileTypeName = Encoding.ASCII.GetString(data, (int)FileTypeNameOffset, nameLength).TrimEnd('\0');

                FileNumberIndex = new uint[NumberOfFilesWithType];
                for (int i = 0; i < FileNumberIndex.Length; i++) FileNumberIndex[i] = BitConverter.ToUInt32(data, (int)(FileNumberIndexOffset + (i * 4)));
            }
        }

        public class FileInfo
        {
            public const int SectionSizeZAR = 0x08;
            public const int SectionSizeGAR = 0x0C;

            public ArchiveFile ParentArchive { get; private set; }

            public uint FileSize { get; private set; }
            public uint FileNameOffset { get; private set; }
            public uint FullPathOffset { get; private set; }

            public string FilePath { get; private set; }

            public FileInfo(ArchiveFile parent, byte[] data, int offset)
            {
                ParentArchive = parent;

                FileSize = BitConverter.ToUInt32(data, offset);
                FileNameOffset = BitConverter.ToUInt32(data, offset + 0x04);

                if (ParentArchive.MagicWord == ArchiveFile.magicWordGAR)
                {
                    FullPathOffset = BitConverter.ToUInt32(data, offset + 0x08);

                    int nameLength = (int)(Array.IndexOf(data, (byte)'\0', (int)FullPathOffset) - FullPathOffset);
                    if (nameLength > 0) FilePath = Encoding.ASCII.GetString(data, (int)FullPathOffset, nameLength).TrimEnd('\0');
                }
                else
                {
                    int nameLength = (int)(Array.IndexOf(data, (byte)'\0', (int)FileNameOffset) - FileNameOffset);
                    if (nameLength > 0) FilePath = Encoding.ASCII.GetString(data, (int)FileNameOffset, nameLength).TrimEnd('\0');
                }
            }
        }
    }
}
