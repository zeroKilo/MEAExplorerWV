using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginTexturesWV
{
    public class TextureAsset
    {
        public int formatType;
        public int formatID;
        public ushort width;
        public ushort height;
        public ushort depth;
        public ushort sliceCount;
        public byte firstMip;
        public int mipCount;
        public List<int> mipDataSizes;
        public byte[] chunkid;
        public byte[] data;

        public TextureAsset(byte[] buff)
        {
            data = buff;
            ReadData();
        }

        private void ReadData()
        {
            MemoryStream m = new MemoryStream(data);
            m.Seek(8, 0);
            formatType = Helpers.ReadInt(m);
            formatID = Helpers.ReadInt(m);
            m.Seek(6, SeekOrigin.Current);
            width = Helpers.ReadUShort(m);
            height = Helpers.ReadUShort(m);
            depth = Helpers.ReadUShort(m);
            sliceCount = Helpers.ReadUShort(m);
            mipCount = m.ReadByte();
            firstMip = (byte)m.ReadByte();
            chunkid = new byte[16];
            m.Read(chunkid, 0, 16);
            mipDataSizes = new List<int>();
            for (int i = 0; i < Math.Min(mipCount, 14); i++)
                mipDataSizes.Add(Helpers.ReadInt(m));
        }

        public static int[] KnownFormats = {0x36, 0x37};

        public bool isKnownFormat()
        {
            if (KnownFormats.Contains<int>(formatID))
                return true;
            return false;
        }

        public void WriteMainData(Stream s, int headerSize, bool hasPitch, bool compressed, bool usesMips, bool isDepth)
        {
            Helpers.WriteInt(s, 0x20534444);
            Helpers.WriteInt(s, headerSize);
            int flags = 0x00001007;
            if (hasPitch && !compressed)
                flags |= 0x8;
            if (isDepth)
                flags |= 0x20000;
            if (hasPitch && compressed)
                flags |= 0x80000;
            Helpers.WriteInt(s, flags);
            int factor = (int)Math.Pow(2, firstMip);
            Helpers.WriteInt(s, height / factor);
            Helpers.WriteInt(s, width / factor);
            Helpers.WriteInt(s, mipDataSizes[0]);
            Helpers.WriteInt(s, depth);
            Helpers.WriteInt(s, mipCount - firstMip);
            for (int i = 0; i < 11; i++)
                Helpers.WriteInt(s, 0);
        }

        public int MakePixelFlags(bool alpha, bool fourCC, bool rgb, bool luminance, bool bump)
        {
            int flags = 0;
            if (alpha)      flags |= 0x2;
            if (fourCC)     flags |= 0x4;
            if (rgb)        flags |= 0x40;
            if (luminance)  flags |= 0x20000;
            if (bump)       flags |= 0x80000;
            return flags;
        }

        public void WritePixelFormat(Stream s, int size, int flags, int fourCC, int bitCount, int rMask, int gMask, int bMask, int aMask)
        {
            Helpers.WriteInt(s, size);
            Helpers.WriteInt(s, flags);
            Helpers.WriteInt(s, fourCC);
            Helpers.WriteInt(s, bitCount);
            Helpers.WriteInt(s, rMask);
            Helpers.WriteInt(s, gMask);
            Helpers.WriteInt(s, bMask);
            Helpers.WriteInt(s, aMask);
        }

        public void WriteDDSHeader(Stream s)
        {
            int flags;
            switch (formatID)
            {
                case 0x36:
                case 0x37:
                    WriteMainData(s, 124, true, false, true, false);
                    flags = MakePixelFlags(false, true, false, false, false);
                    WritePixelFormat(s, 32, flags, 0x31545844, 0, 0, 0, 0, 0);
                    break;
            }
            Helpers.WriteInt(s, 0);
            Helpers.WriteInt(s, 0);
            Helpers.WriteInt(s, 0);
            Helpers.WriteInt(s, 0);
            Helpers.WriteInt(s, 0);
        }
    }
}
