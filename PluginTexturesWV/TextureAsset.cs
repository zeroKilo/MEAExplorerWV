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
        public TextureType formatType;
        public uint firstMipOffset;
        public uint secondMipOffset;
        public int formatID;
        public ushort width;
        public ushort height;
        public ushort depth;
        public ushort sliceCount;
        public byte firstMip;
        public byte firstRealMip;
        public int mipCount;
        public List<uint> mipDataSizes;
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
            firstMipOffset = Helpers.ReadUInt(m);
            secondMipOffset = Helpers.ReadUInt(m);
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
            mipDataSizes = new List<uint>();
            for (int i = 0; i < mipCount; i++)
                mipDataSizes.Add(Helpers.ReadUInt(m));
        }

        public static int[] KnownFormats = { 0x12, 0x36, 0x37, 0x3C, 0x3D, 0x3F, 0x42, 0x43 };

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
            int factor = (int)Math.Pow(2, firstRealMip);
            Helpers.WriteInt(s, height / factor);
            Helpers.WriteInt(s, width / factor);
            Helpers.WriteUInt(s, mipDataSizes[firstRealMip]);
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

        public void WriteDX10Header(Stream s, DXGI_FORMAT dxgiFormat, uint dimension, uint flags, uint arraySize, uint flags2)
        {
            Helpers.WriteUInt(s, (uint)dxgiFormat);
            Helpers.WriteUInt(s, dimension);
            Helpers.WriteUInt(s, flags);
            Helpers.WriteUInt(s, arraySize);
            Helpers.WriteUInt(s, flags2);
        }

        public byte GuessRealFirstMip(int size)
        {
            byte result = firstMip;
            uint sum = 0;
            if (mipCount != 0)
                for (int i = mipCount - 1; i >= 0; i--)
                {
                    sum += mipDataSizes[i];
                    if (sum == size)
                        result = (byte)i;
                }
            return result;
        }

        public byte[] MakeRawDDSBuffer(byte[] pixeldata)
        {
            MemoryStream result = new MemoryStream();
            firstRealMip = GuessRealFirstMip(pixeldata.Length);
            WriteDDSHeader(result);
            result.Write(pixeldata, 0, pixeldata.Length);
            return result.ToArray();
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
                case 0x3C:
                case 0x3D:
                case 0x3F:
                case 0x42:
                case 0x43:
                    WriteMainData(s, 124, false, true, true, false);
                    WritePixelFormat(s, 32, MakeFormatFlags(false, false, true, false, false, false), MakeFourCC("DX10"), 0, 0, 0, 0, 0);
                    Helpers.WriteUInt(s, MakeCaps(true, true));
                    Helpers.WriteUInt(s, MakeCaps2(false, false, false, false, false, false, false, false));
                    break;
            }
            Helpers.WriteInt(s, 0);
            Helpers.WriteInt(s, 0);
            Helpers.WriteInt(s, 0);
            switch (formatID)
            {
                case 0x3C:
                    WriteDX10Header(s, DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM, 3, 0, 1, 3);
                    break;
                case 0x3D:
                    WriteDX10Header(s, DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM_SRGB, 3, 0, 1, 3);
                    break;
                case 0x3F:
                    WriteDX10Header(s, DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM, 3, 0, 1, 0);
                    break;
                case 0x42:
                    WriteDX10Header(s, DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM, 3, 0, 1, 0);
                    break;
                case 0x43:
                    WriteDX10Header(s, DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM_SRGB, 3, 0, 1, 3);
                    break;
            }
        }

        public enum TextureType
        {
            Texture2D = 0,
            TextureCube = 1,
            Texture3D = 2,
            Texture2DArray = 3,
            Texture1DArray = 4,
            Texture1D = 5
        }

        public enum FB_FORMAT
        {
            Invalid = 0,
            R4G4_UNORM = 1,
            R4G4B4A4_UNORM = 2,
            R5G6B5_UNORM = 3,
            B5G6R5_UNORM = 4,
            R5G5B5A1_UNORM = 5,
            R8_UNORM = 6,
            R8_SNORM = 7,
            R8_SRGB = 8,
            R8_UINT = 9,
            R8_SINT = 10,
            R8G8_UNORM = 11,
            R8G8_SNORM = 12,
            R8G8_SRGB = 13,
            R8G8_UINT = 14,
            R8G8_SINT = 15,
            R8G8B8_UNORM = 16,
            R8G8B8_SRGB = 17,
            R8G8B8A8_UNORM = 18,
            R8G8B8A8_SNORM = 19,
            R8G8B8A8_SRGB = 20,
            R8G8B8A8_UINT = 21,
            R8G8B8A8_SINT = 22,
            B8G8R8A8_UNORM = 23,
            B8G8R8A8_SRGB = 24,
            R10G11B11_FLOAT = 25,
            R11G11B10_FLOAT = 26,
            R10G10B10A2_UNORM = 27,
            R10G10B10A2_UINT = 28,
            R9G9B9E5_FLOAT = 29,
            R16_FLOAT = 30,
            R16_UNORM = 31,
            R16_SNORM = 32,
            R16_UINT = 33,
            R16_SINT = 34,
            R16G16_FLOAT = 35,
            R16G16_UNORM = 36,
            R16G16_SNORM = 37,
            R16G16_UINT = 38,
            R16G16_SINT = 39,
            R16G16B16A16_FLOAT = 40,
            R16G16B16A16_UNORM = 41,
            R16G16B16A16_SNORM = 42,
            R16G16B16A16_UINT = 43,
            R16G16B16A16_SINT = 44,
            R32_FLOAT = 45,
            R32_UINT = 46,
            R32_SINT = 47,
            R32G32_FLOAT = 48,
            R32G32_UINT = 49,
            R32G32_SINT = 50,
            R32G32B32A32_FLOAT = 51,
            R32G32B32A32_UINT = 52,
            R32G32B32A32_SINT = 53,
            BC1_UNORM = 54,
            BC1_SRGB = 55,
            BC1A_UNORM = 56,
            BC1A_SRGB = 57,
            BC2_UNORM = 58,
            BC2_SRGB = 59,
            BC3_UNORM = 60,
            BC3_SRGB = 61,
            BC4_UNORM = 62,
            BC5_UNORM = 63,
            BC6U_FLOAT = 64,
            BC6S_FLOAT = 65,
            BC7_UNORM = 66,
            BC7_SRGB = 67,
            ETC1_UNORM = 68,
            ETC1_SRGB = 69,
            ETC2RGB_UNORM = 70,
            ETC2RGB_SRGB = 71,
            ETC2RGBA_UNORM = 72,
            ETC2RGBA_SRGB = 73,
            ETC2RGBA1_UNORM = 74,
            ETC2RGBA1_SRGB = 75,
            EAC_R11_UNORM = 76,
            EAC_R11_SNORM = 77,
            EAC_RG11_UNORM = 78,
            EAC_RG11_SNORM = 79,
            PVRTC1_4BPP_RGBA_UNORM = 80,
            PVRTC1_4BPP_RGBA_SRGB = 81,
            PVRTC1_4BPP_RGB_UNORM = 82,
            PVRTC1_4BPP_RGB_SRGB = 83,
            PVRTC1_2BPP_RGBA_UNORM = 84,
            PVRTC1_2BPP_RGBA_SRGB = 85,
            PVRTC1_2BPP_RGB_UNORM = 86,
            PVRTC1_2BPP_RGB_SRGB = 87,
            PVRTC2_4BPP_UNORM = 88,
            PVRTC2_4BPP_SRGB = 89,
            PVRTC2_2BPP_UNORM = 90,
            PVRTC2_2BPP_SRGB = 91,
            ASTC_4x4_UNORM = 92,
            ASTC_4x4_SRGB = 93,
            ASTC_5x4_UNORM = 94,
            ASTC_5x4_SRGB = 95,
            ASTC_5x5_UNORM = 96,
            ASTC_5x5_SRGB = 97,
            ASTC_6x5_UNORM = 98,
            ASTC_6x5_SRGB = 99,
            ASTC_6x6_UNORM = 100,
            ASTC_6x6_SRGB = 101,
            ASTC_8x5_UNORM = 102,
            ASTC_8x5_SRGB = 103,
            ASTC_8x6_UNORM = 104,
            ASTC_8x6_SRGB = 105,
            ASTC_8x8_UNORM = 106,
            ASTC_8x8_SRGB = 107,
            ASTC_10x5_UNORM = 108,
            ASTC_10x5_SRGB = 109,
            ASTC_10x6_UNORM = 110,
            ASTC_10x6_SRGB = 111,
            ASTC_10x8_UNORM = 112,
            ASTC_10x8_SRGB = 113,
            ASTC_10x10_UNORM = 114,
            ASTC_10x10_SRGB = 115,
            ASTC_12x10_UNORM = 116,
            ASTC_12x10_SRGB = 117,
            ASTC_12x12_UNORM = 118,
            ASTC_12x12_SRGB = 119,
            D24_UNORM_S8_UINT = 120,
            D24_FLOAT_S8_UINT = 121,
            D32_FLOAT_S8_UINT = 122,
            D16_UNORM = 123,
            D24_UNORM = 124,
            D32_FLOAT = 125
        }

        public enum DXGI_FORMAT
        {
            DXGI_FORMAT_UNKNOWN = 0,
            DXGI_FORMAT_R32G32B32A32_TYPELESS = 1,
            DXGI_FORMAT_R32G32B32A32_FLOAT = 2,
            DXGI_FORMAT_R32G32B32A32_UINT = 3,
            DXGI_FORMAT_R32G32B32A32_SINT = 4,
            DXGI_FORMAT_R32G32B32_TYPELESS = 5,
            DXGI_FORMAT_R32G32B32_FLOAT = 6,
            DXGI_FORMAT_R32G32B32_UINT = 7,
            DXGI_FORMAT_R32G32B32_SINT = 8,
            DXGI_FORMAT_R16G16B16A16_TYPELESS = 9,
            DXGI_FORMAT_R16G16B16A16_FLOAT = 10,
            DXGI_FORMAT_R16G16B16A16_UNORM = 11,
            DXGI_FORMAT_R16G16B16A16_UINT = 12,
            DXGI_FORMAT_R16G16B16A16_SNORM = 13,
            DXGI_FORMAT_R16G16B16A16_SINT = 14,
            DXGI_FORMAT_R32G32_TYPELESS = 15,
            DXGI_FORMAT_R32G32_FLOAT = 16,
            DXGI_FORMAT_R32G32_UINT = 17,
            DXGI_FORMAT_R32G32_SINT = 18,
            DXGI_FORMAT_R32G8X24_TYPELESS = 19,
            DXGI_FORMAT_D32_FLOAT_S8X24_UINT = 20,
            DXGI_FORMAT_R32_FLOAT_X8X24_TYPELESS = 21,
            DXGI_FORMAT_X32_TYPELESS_G8X24_UINT = 22,
            DXGI_FORMAT_R10G10B10A2_TYPELESS = 23,
            DXGI_FORMAT_R10G10B10A2_UNORM = 24,
            DXGI_FORMAT_R10G10B10A2_UINT = 25,
            DXGI_FORMAT_R11G11B10_FLOAT = 26,
            DXGI_FORMAT_R8G8B8A8_TYPELESS = 27,
            DXGI_FORMAT_R8G8B8A8_UNORM = 28,
            DXGI_FORMAT_R8G8B8A8_UNORM_SRGB = 29,
            DXGI_FORMAT_R8G8B8A8_UINT = 30,
            DXGI_FORMAT_R8G8B8A8_SNORM = 31,
            DXGI_FORMAT_R8G8B8A8_SINT = 32,
            DXGI_FORMAT_R16G16_TYPELESS = 33,
            DXGI_FORMAT_R16G16_FLOAT = 34,
            DXGI_FORMAT_R16G16_UNORM = 35,
            DXGI_FORMAT_R16G16_UINT = 36,
            DXGI_FORMAT_R16G16_SNORM = 37,
            DXGI_FORMAT_R16G16_SINT = 38,
            DXGI_FORMAT_R32_TYPELESS = 39,
            DXGI_FORMAT_D32_FLOAT = 40,
            DXGI_FORMAT_R32_FLOAT = 41,
            DXGI_FORMAT_R32_UINT = 42,
            DXGI_FORMAT_R32_SINT = 43,
            DXGI_FORMAT_R24G8_TYPELESS = 44,
            DXGI_FORMAT_D24_UNORM_S8_UINT = 45,
            DXGI_FORMAT_R24_UNORM_X8_TYPELESS = 46,
            DXGI_FORMAT_X24_TYPELESS_G8_UINT = 47,
            DXGI_FORMAT_R8G8_TYPELESS = 48,
            DXGI_FORMAT_R8G8_UNORM = 49,
            DXGI_FORMAT_R8G8_UINT = 50,
            DXGI_FORMAT_R8G8_SNORM = 51,
            DXGI_FORMAT_R8G8_SINT = 52,
            DXGI_FORMAT_R16_TYPELESS = 53,
            DXGI_FORMAT_R16_FLOAT = 54,
            DXGI_FORMAT_D16_UNORM = 55,
            DXGI_FORMAT_R16_UNORM = 56,
            DXGI_FORMAT_R16_UINT = 57,
            DXGI_FORMAT_R16_SNORM = 58,
            DXGI_FORMAT_R16_SINT = 59,
            DXGI_FORMAT_R8_TYPELESS = 60,
            DXGI_FORMAT_R8_UNORM = 61,
            DXGI_FORMAT_R8_UINT = 62,
            DXGI_FORMAT_R8_SNORM = 63,
            DXGI_FORMAT_R8_SINT = 64,
            DXGI_FORMAT_A8_UNORM = 65,
            DXGI_FORMAT_R1_UNORM = 66,
            DXGI_FORMAT_R9G9B9E5_SHAREDEXP = 67,
            DXGI_FORMAT_R8G8_B8G8_UNORM = 68,
            DXGI_FORMAT_G8R8_G8B8_UNORM = 69,
            DXGI_FORMAT_BC1_TYPELESS = 70,
            DXGI_FORMAT_BC1_UNORM = 71,
            DXGI_FORMAT_BC1_UNORM_SRGB = 72,
            DXGI_FORMAT_BC2_TYPELESS = 73,
            DXGI_FORMAT_BC2_UNORM = 74,
            DXGI_FORMAT_BC2_UNORM_SRGB = 75,
            DXGI_FORMAT_BC3_TYPELESS = 76,
            DXGI_FORMAT_BC3_UNORM = 77,
            DXGI_FORMAT_BC3_UNORM_SRGB = 78,
            DXGI_FORMAT_BC4_TYPELESS = 79,
            DXGI_FORMAT_BC4_UNORM = 80,
            DXGI_FORMAT_BC4_SNORM = 81,
            DXGI_FORMAT_BC5_TYPELESS = 82,
            DXGI_FORMAT_BC5_UNORM = 83,
            DXGI_FORMAT_BC5_SNORM = 84,
            DXGI_FORMAT_B5G6R5_UNORM = 85,
            DXGI_FORMAT_B5G5R5A1_UNORM = 86,
            DXGI_FORMAT_B8G8R8A8_UNORM = 87,
            DXGI_FORMAT_B8G8R8X8_UNORM = 88,
            DXGI_FORMAT_R10G10B10_XR_BIAS_A2_UNORM = 89,
            DXGI_FORMAT_B8G8R8A8_TYPELESS = 90,
            DXGI_FORMAT_B8G8R8A8_UNORM_SRGB = 91,
            DXGI_FORMAT_B8G8R8X8_TYPELESS = 92,
            DXGI_FORMAT_B8G8R8X8_UNORM_SRGB = 93,
            DXGI_FORMAT_BC6H_TYPELESS = 94,
            DXGI_FORMAT_BC6H_UF16 = 95,
            DXGI_FORMAT_BC6H_SF16 = 96,
            DXGI_FORMAT_BC7_TYPELESS = 97,
            DXGI_FORMAT_BC7_UNORM = 98,
            DXGI_FORMAT_BC7_UNORM_SRGB = 99,
            DXGI_FORMAT_AYUV = 100,
            DXGI_FORMAT_Y410 = 101,
            DXGI_FORMAT_Y416 = 102,
            DXGI_FORMAT_NV12 = 103,
            DXGI_FORMAT_P010 = 104,
            DXGI_FORMAT_P016 = 105,
            DXGI_FORMAT_420_OPAQUE = 106,
            DXGI_FORMAT_YUY2 = 107,
            DXGI_FORMAT_Y210 = 108,
            DXGI_FORMAT_Y216 = 109,
            DXGI_FORMAT_NV11 = 110,
            DXGI_FORMAT_AI44 = 111,
            DXGI_FORMAT_IA44 = 112,
            DXGI_FORMAT_P8 = 113,
            DXGI_FORMAT_A8P8 = 114,
            DXGI_FORMAT_B4G4R4A4_UNORM = 115,
            DXGI_FORMAT_P208 = 130,
            DXGI_FORMAT_V208 = 131,
            DXGI_FORMAT_V408 = 132,
        } 
    }
}
