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
        public enum TextureType
        {
            Texture2D = 0,
            TextureCube = 1,
            Texture3D = 2,
            Texture2DArray = 3,
            Texture1DArray = 4,
            Texture1D = 5
        }
        public TextureType formatType;
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
            formatType = (TextureType)Helpers.ReadInt(m);
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

        public static int[] KnownFormats = { 0x12, 0x36, 0x37 };

        public bool isKnownFormat()
        {
            if (KnownFormats.Contains<int>(formatID))
                return true;
            return false;
        }

        public uint MakeFourCC(string input)
        {
            byte[] buff = new byte[4];
            for (int i = 0; i < 4; i++)
                buff[i] = (byte)input[i];
            return BitConverter.ToUInt32(buff, 0);
        }

        public uint MakeFlags(bool hasPitch, bool hasMips, bool hasLinearSize, bool hasDepth)
        {
            uint result = 0x1007;
            if (hasPitch)       result |= 0x8;
            if (hasMips)        result |= 0x20000;
            if (hasLinearSize)  result |= 0x80000;
            if (hasDepth)       result |= 0x800000;
            return result;
        }

        public uint MakeFormatFlags(bool alphaPixel, bool alpha, bool fourCC, bool rgb, bool yuv, bool luminance)
        {
            uint result = 0;
            if (alphaPixel) result |= 0x1;
            if (alpha)      result |= 0x2;
            if (fourCC)     result |= 0x4;
            if (rgb)        result |= 0x40;
            if (yuv)        result |= 0x200;
            if (luminance)  result |= 0x20000;
            return result;
        }

        public uint MakeDX10Flags(bool isCube)
        {
            if (isCube)
                return 4;
            else
                return 0;
        }

        public uint MakeCaps(bool complex, bool hasMips)
        {
            uint result = 0x1000;
            if (complex) result |= 0x8;
            if (hasMips) result |= 0x400000;
            return result;
        }

        public uint MakeCaps2(bool isCube, bool cubePosX, bool cubeNegX, bool cubePosY, bool cubeNegY, bool cubePosZ, bool cubeNegZ, bool isVolume)
        {
            uint result = 0;
            if (isCube)   result |= 0x200;
            if (cubePosX) result |= 0x400;
            if (cubeNegX) result |= 0x800;
            if (cubePosY) result |= 0x1000;
            if (cubeNegY) result |= 0x2000;
            if (cubePosZ) result |= 0x4000;
            if (cubeNegZ) result |= 0x8000;
            if (isVolume) result |= 0x200000;
            return result;
        }

        public void WriteMainData(Stream s, int headerSize, bool hasPitch, bool hasMips, bool hasLinearSize, bool hasDepth)
        {
            Helpers.WriteInt(s, 0x20534444);
            Helpers.WriteInt(s, headerSize);
            Helpers.WriteUInt(s, MakeFlags(hasPitch, hasMips, hasLinearSize, hasDepth));
            int factor = (int)Math.Pow(2, firstMip);
            Helpers.WriteInt(s, height / factor);
            Helpers.WriteInt(s, width / factor);
            Helpers.WriteInt(s, mipDataSizes[0]);
            Helpers.WriteInt(s, depth);
            Helpers.WriteInt(s, mipCount - firstMip);
            for (int i = 0; i < 11; i++)
                Helpers.WriteInt(s, 0);
        }

        public void WritePixelFormat(Stream s, uint size, uint flags, uint fourCC, uint bitCount, uint rMask, uint gMask, uint bMask, uint aMask)
        {
            Helpers.WriteUInt(s, size);
            Helpers.WriteUInt(s, flags);
            Helpers.WriteUInt(s, fourCC);
            Helpers.WriteUInt(s, bitCount);
            Helpers.WriteUInt(s, rMask);
            Helpers.WriteUInt(s, gMask);
            Helpers.WriteUInt(s, bMask);
            Helpers.WriteUInt(s, aMask);
        }

        public void WriteDX10Header(Stream s, uint dxgiFormat, uint dimension, uint flags, uint arraySize, uint flags2)
        {
            Helpers.WriteUInt(s, dxgiFormat);
            Helpers.WriteUInt(s, dimension);
            Helpers.WriteUInt(s, flags);
            Helpers.WriteUInt(s, arraySize);
            Helpers.WriteUInt(s, flags2);
        }

        public void WriteDDSHeader(Stream s)
        {
            switch (formatID)
            {
                case 0x12:                    
                    WriteMainData(s, 124, false, true, false, false);
                    WritePixelFormat(s, 32, MakeFormatFlags(false, true, false, true, false, false), 0, 32, 0xFF, 0xFF00, 0xFF0000, 0xFF000000);
                    Helpers.WriteUInt(s, MakeCaps(false, true));
                    Helpers.WriteUInt(s, MakeCaps2(false, false, false, false, false, false, false, false));
                    break;
                case 0x36:
                case 0x37:
                    WriteMainData(s, 124, false, true, false, false);
                    WritePixelFormat(s, 32, MakeFormatFlags(false, false, true, false, false, false), MakeFourCC("DXT1"), 0, 0, 0, 0, 0);
                    Helpers.WriteUInt(s, MakeCaps(false, true));
                    Helpers.WriteUInt(s, MakeCaps2(false, false, false, false, false, false, false, false));
                    break;                
            }
            Helpers.WriteInt(s, 0);
            Helpers.WriteInt(s, 0);
            Helpers.WriteInt(s, 0);
        }        
    }
}
