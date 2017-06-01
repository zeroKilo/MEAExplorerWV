using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using ZstdNet;

namespace PluginSystem
{
    public static class Helpers
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool LockWindowUpdate(IntPtr hWndLock);

        public class BinaryReader7Bit : BinaryReader
        {
            public BinaryReader7Bit(Stream stream) : base(stream) { }
            public new int Read7BitEncodedInt()
            {
                return base.Read7BitEncodedInt();
            }
        }

        public class BinaryWriter7Bit : BinaryWriter
        {
            public BinaryWriter7Bit(Stream stream) : base(stream) { }
            public new void Write7BitEncodedInt(int i)
            {
                base.Write7BitEncodedInt(i);
            }
        }

        public static void DeleteFileIfExist(string file)
        {
            if (File.Exists(file))
                File.Delete(file);
        }

        public static void RunShell(string file, string command)
        {
            Process process = new System.Diagnostics.Process();
            ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = file;
            startInfo.Arguments = command;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.WorkingDirectory = Path.GetDirectoryName(file) + "\\";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }

        public static Bitmap LoadImageCopy(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            Bitmap loaded = new Bitmap(fs);
            Bitmap result = loaded.Clone(new Rectangle(0, 0, loaded.Width, loaded.Height), PixelFormat.Format32bppRgb);
            fs.Close();
            return result;
        }

        public static bool ByteArrayCompare(byte[] b1, byte[] b2)
        {
            if (b1 == null || b2 == null || b1.Length != b2.Length)
                return false;
            for (int i = 0; i < b1.Length; i++)
                if (b1[i] != b2[i])
                    return false;
            return true;
        }

        public static int CountBlocks(Stream s)
        {
            long l = s.Length;
            int count = 0;
            while (s.Position < l)
            {
                SkipBlock(s);
                count++;
            }
            return count;
        }

        public static int SkipBlock(Stream s)
        {
            int uncsize = ReadLEInt(s);
            ushort flags = ReadLEUShort(s);
            ushort csize = ReadLEUShort(s);
            switch (flags)
            {
                case 0x0F70:
                    s.Seek(csize, SeekOrigin.Current);
                    break;
                case 0x0:
                case 0x0070:
                case 0x0071:
                    s.Seek(uncsize, SeekOrigin.Current);
                    break;
                default:
                    throw new Exception("Unknown Chunk Type 0x" + flags.ToString("X4"));
            }
            return uncsize;
        }

        public static byte[] DecryptChunkdata(byte[] data, byte[] key, int size)
        {
            var aes = AesCryptoServiceProvider.Create();
            aes.Mode = CipherMode.CBC;
            aes.KeySize = 128;
            aes.Padding = PaddingMode.PKCS7;
            MemoryStream input = new MemoryStream(data);
            input.Seek(0, 0);
            using (CryptoStream cs = new CryptoStream(input, aes.CreateDecryptor(key, key), CryptoStreamMode.Read))
            {
                MemoryStream m = new MemoryStream();
                cs.CopyTo(m, size);
                return m.ToArray();
            }
        }

        public static byte[] EncryptChunkdata(byte[] data, byte[] key)
        {
            var aes = AesCryptoServiceProvider.Create();
            aes.Mode = CipherMode.CBC;
            aes.KeySize = 128;
            aes.Padding = PaddingMode.PKCS7;
            MemoryStream result = new MemoryStream();
            using (CryptoStream cs = new CryptoStream(result, aes.CreateEncryptor(key, key), CryptoStreamMode.Write))
            {
                new MemoryStream(data).CopyTo(cs);
                cs.FlushFinalBlock();
                return result.ToArray();
            }
        }

        public static byte[] ZStdDecompress(byte[] data)
        {
            Decompressor dec = new Decompressor();
            return dec.Unwrap(data);
        }

        public static byte[] ZStdCompress(byte[] data)
        {
            Compressor dec = new Compressor();
            return dec.Wrap(data);
        }

        public static void AlignStream(Stream s, int align)
        {
            while (s.Position % align != 0)
                s.Seek(1, SeekOrigin.Current);
        }

        public static void WriteFloat(Stream s, float f)
        {
            s.Write(BitConverter.GetBytes(f), 0, 4);
        }

        public static void WriteInt(Stream s, int i)
        {
            s.Write(BitConverter.GetBytes(i), 0, 4);
        }

        public static void WriteUInt(Stream s, uint i)
        {
            s.Write(BitConverter.GetBytes(i), 0, 4);
        }

        public static void WriteShort(Stream s, short i)
        {
            s.Write(BitConverter.GetBytes(i), 0, 2);
        }

        public static void WriteUShort(Stream s, ushort i)
        {
            s.Write(BitConverter.GetBytes(i), 0, 2);
        }

        public static void WriteLEInt(Stream s, int i)
        {
            List<byte> t = new List<byte>(BitConverter.GetBytes(i));
            t.Reverse();
            s.Write(t.ToArray(), 0, 4);
        }

        public static void WriteLEUInt(Stream s, uint i)
        {
            List<byte> t = new List<byte>(BitConverter.GetBytes(i));
            t.Reverse();
            s.Write(t.ToArray(), 0, 4);
        }

        public static void WriteLEUShort(Stream s, ushort u)
        {
            byte[] buff = BitConverter.GetBytes(u);
            buff = buff.Reverse().ToArray();
            s.Write(buff, 0, 2);
        }

        public static int ReadInt(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            return BitConverter.ToInt32(buff, 0);
        }

        public static uint ReadUInt(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            return BitConverter.ToUInt32(buff, 0);
        }

        public static short ReadShort(Stream s)
        {
            byte[] buff = new byte[2];
            s.Read(buff, 0, 2);
            return BitConverter.ToInt16(buff, 0);
        }

        public static ushort ReadUShort(Stream s)
        {
            byte[] buff = new byte[2];
            s.Read(buff, 0, 2);
            return BitConverter.ToUInt16(buff, 0);
        }

        public static long ReadLong(Stream s)
        {
            byte[] buff = new byte[8];
            s.Read(buff, 0, 8);
            return BitConverter.ToInt64(buff, 0);
        }

        public static ulong ReadULong(Stream s)
        {
            byte[] buff = new byte[8];
            s.Read(buff, 0, 8);
            return BitConverter.ToUInt64(buff, 0);
        }

        public static float ReadFloat(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            return BitConverter.ToSingle(buff, 0);
        }

        public static int ReadLEInt(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            buff = buff.Reverse().ToArray();
            return BitConverter.ToInt32(buff, 0);
        }

        public static uint ReadLEUInt(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            buff = buff.Reverse().ToArray();
            return BitConverter.ToUInt32(buff, 0);
        }

        public static short ReadLEShort(Stream s)
        {
            byte[] buff = new byte[2];
            s.Read(buff, 0, 2);
            buff = buff.Reverse().ToArray();
            return BitConverter.ToInt16(buff, 0);
        }

        public static ushort ReadLEUShort(Stream s)
        {
            byte[] buff = new byte[2];
            s.Read(buff, 0, 2);
            buff = buff.Reverse().ToArray();
            return BitConverter.ToUInt16(buff, 0);
        }

        public static byte[] ReadFull(Stream s, uint size)
        {
            byte[] buff = new byte[size];
            int totalread = 0;
            while ((totalread += s.Read(buff, totalread, (int)(size - totalread))) < size) ;
            return buff;
        }

        public static string ReadNullString(Stream s)
        {
            string res = "";
            byte b;
            while ((b = (byte)s.ReadByte()) > 0 && s.Position < s.Length) res += (char)b;
            return res;
        }

        public static string ReadStringPointer(Stream s)
        {
            string result = "";
            long offset = (long)ReadULong(s);
            long pos = s.Position;
            s.Seek(offset, 0);
            result = ReadNullString(s);
            s.Seek(pos, 0);
            return result;
        }

        public static void WriteNullString(Stream s, string t)
        {
            foreach (char c in t)
                s.WriteByte((byte)c);
            s.WriteByte(0);
        }

        public static ulong ReadLEB128(Stream s)
        {
            ulong result = 0;
            byte shift = 0;
            while (true)
            {
                int i = s.ReadByte();
                if (i == -1) return result;
                byte b = (byte)i;
                result |= (ulong)((b & 0x7f) << shift);
                if ((b >> 7) == 0)
                    return result;
                shift += 7;
            }
        }

        public static void WriteLEB128(Stream s, int value)
        {
            int temp = value;
            while (temp != 0)
            {
                int val = (temp & 0x7f);
                temp >>= 7;

                if (temp > 0)
                    val |= 0x80;

                s.WriteByte((byte)val);
            }
        }

        public static bool MatchByteArray(byte[] a1, byte[] a2)
        {
            if (a1.Length != a2.Length)
                return false;
            for (int i = 0; i < a1.Length; i++)
                if (a1[i] != a2[i])
                    return false;
            return true;
        }

        public static byte[] HexStringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static byte[] StringAsByteArray(string str)
        {
            MemoryStream m = new MemoryStream();
            foreach (char c in str)
                m.WriteByte((byte)c);
            return m.ToArray();
        }

        public static string ByteArrayToHexString(byte[] data, int start = 0, int len = 0)
        {
            if (data == null)
                data = new byte[0];
            StringBuilder sb = new StringBuilder();
            if (start == 0)
                foreach (byte b in data)
                    sb.Append(b.ToString("X2"));
            else
                if (start > 0 && start + len <= data.Length)
                    for (int i = start; i < start + len; i++)
                        sb.Append(data[i].ToString("X2"));
                else
                    return "";
            return sb.ToString();
        }

        public static string ByteArrayAsString(byte[] data)
        {
            if (data == null)
                data = new byte[0];
            StringBuilder sb = new StringBuilder();
            foreach (byte b in data)
                sb.Append((char)b);
            return sb.ToString();
        }

        public static string MakeTabs(int count)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < count; i++)
                sb.Append("  ");
            return sb.ToString();
        }

        public static int DecompressLZ77(byte[] input, byte[] output, out int decompressedLength)
        {
            int inputPos = 0, outputPos = 0;
            try
            {
                while (true)
                {

                    bool isLookback = true;
                    bool skipParseCopyLength = false;
                    int lookbackLength = 1;
                    int copyLength = 1;
                    byte copyLengthMask = 0;

                    byte code = input[inputPos++];
                    if (code < 0x10)
                    {
                        isLookback = false;
                        copyLength += 2;
                        copyLengthMask = 0x0f;
                    }
                    else if (code < 0x20)
                    {
                        copyLength += 1;
                        copyLengthMask = 0x07;
                        lookbackLength |= (code & 0x08) << 11;
                        lookbackLength += 0x3fff;
                    }
                    else if (code < 0x40)
                    {
                        copyLength += 1;
                        copyLengthMask = 0x1f;
                    }
                    else
                    {
                        skipParseCopyLength = true;
                        copyLength += code >> 5;
                        lookbackLength += (code >> 2) & 0x07;
                        lookbackLength += input[inputPos++] * 8;
                    }

                    if (!isLookback || !skipParseCopyLength)
                    {
                        if ((code & copyLengthMask) == 0)
                        {
                            byte nextCode;
                            for (nextCode = input[inputPos++]; nextCode == 0; nextCode = input[inputPos++])
                            {
                                copyLength += 0xff;
                            }
                            copyLength += nextCode + copyLengthMask;
                        }
                        else
                        {
                            copyLength += code & copyLengthMask;
                        }

                        if (isLookback)
                        {
                            int lookbackCode = input[inputPos++];
                            lookbackCode |= input[inputPos++] << 8;
                            if (code < 0x20 && (lookbackCode >> 2) == 0) break;
                            lookbackLength += lookbackCode >> 2;
                            code = (byte)lookbackCode;
                        }
                    }

                    if (isLookback)
                    {
                        int lookbackPos = outputPos - lookbackLength;
                        for (int i = 0; i < copyLength; ++i)
                        {
                            output[outputPos++] = output[lookbackPos++];
                        }
                        copyLength = code & 0x03;
                    }

                    for (int i = 0; i < copyLength; ++i)
                    {
                        output[outputPos++] = input[inputPos++];
                    }
                }
            }
            catch
            { }

            decompressedLength = outputPos;
            if (inputPos == input.Length) return 0;
            else return inputPos < input.Length ? -8 : -4;
        }

        public static byte[] ComputeHash(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                return md5.ComputeHash(File.ReadAllBytes(filePath));
            }
        }

        public static int HashFNV1(string StrToHash, int hashseed = 5381, int hashprime = 33)
        {
            int Hash = hashseed;
            for (int i = 0; i < StrToHash.Length; i++)
            {
                byte b = (byte)StrToHash[i];
                Hash = (int)(Hash * hashprime) ^ b;
            }
            return Hash;
        }

        public static string GetFileNameWithOutExtension(string path)
        {
            return Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path);
        }

        public static string GetResType(uint type)
        {
            if (ResTypes.ContainsKey(type))
                return ResTypes[type];
            else if (type == 0)
                return "EBX";
            else
                return "0x" + type.ToString("X8");
        }

        public static TreeNode AddPath(TreeNode t, string path, char splitter = '/')
        {
            string[] parts = path.Split(splitter);
            TreeNode f = null;
            foreach (TreeNode c in t.Nodes)
                if (c.Text == parts[0].ToLower())
                {
                    f = c;
                    break;
                }
            if (f == null)
            {
                f = new TreeNode(parts[0].ToLower());
                t.Nodes.Add(f);
            }
            if (parts.Length > 1)
            {
                string subpath = path.Substring(parts[0].Length + 1, path.Length - 1 - parts[0].Length);
                f = AddPath(f, subpath, splitter);
            }
            return t;
        }

        public static bool isDataEntryNode(TreeNode t)
        {
            if (t == null)
                return false;
            if (t.Text == "ROOT")
                return true;
            return isDataEntryNode(t.Parent);
        }

        public static string GetPathFromNode(TreeNode t, string seperator = "/")
        {
            string result = t.Text;
            if (t.Parent != null)
                result = GetPathFromNode(t.Parent, seperator) + seperator + result;
            return result;
        }

        public static void ExpandTreeByLevel(TreeNode node, int level)
        {
            node.Expand();
            if (level > 0)
                foreach (TreeNode t in node.Nodes)
                    ExpandTreeByLevel(t, level - 1);
        }

        public static void CollapseAll(TreeNode t)
        {
            foreach (TreeNode t2 in t.Nodes)
                CollapseAll(t2);
            t.Collapse();
        }

        public static void NodeToXml(StringBuilder sb, TreeNode t, int depth)
        {
            string tabs = "";
            for (int i = 0; i < depth; i++)
                tabs += "\t";
            if (t.Nodes.Count != 0)
            {
                sb.AppendLine(tabs + "<" + t.Text + ">");
                foreach (TreeNode t2 in t.Nodes)
                    NodeToXml(sb, t2, depth + 1);
                sb.AppendLine(tabs + "</" + t.Text + ">");
            }
            else
                sb.AppendLine(tabs + t.Text);
        }

        public static void SelectNext(string text, TreeView tree)
        {
            text = text.ToLower();
            TreeNode t = tree.SelectedNode;
            if (t == null && tree.Nodes.Count != 0)
                t = tree.Nodes[0];
            while (true)
            {
                TreeNode t2 = FindNext(t, text);
                if (t2 != null)
                {
                    tree.SelectedNode = t2;
                    return;
                }
                else if (t.NextNode != null)
                    t = t.NextNode;
                else if (t.Parent != null && t.Parent.NextNode != null)
                    t = t.Parent.NextNode;
                else if (t.Parent != null && t.Parent.NextNode == null)
                    while (t.Parent != null)
                    {
                        t = t.Parent;
                        if (t != null && t.NextNode != null)
                        {
                            t = t.NextNode;
                            break;
                        }
                    }
                else
                    return;
                if (t.Text.ToLower().Contains(text))
                {
                    tree.SelectedNode = t;
                    return;
                }
            }
        }

        public static TreeNode GetTopMostNode(TreeNode node)
        {
            if (node.Parent == null)
                return node;
            else
                return GetTopMostNode(node.Parent);
        }

        public static TreeNode FindNext(TreeNode t, string text)
        {
            foreach (TreeNode t2 in t.Nodes)
            {
                if (t2.Text.ToLower().Contains(text))
                    return t2;
                if (t2.Nodes.Count != 0)
                {
                    TreeNode t3 = FindNext(t2, text);
                    if (t3 != null)
                        return t3;
                }
            }
            return null;
        }

        public static string SkipSubFolder(string path, int start)
        {
            string[] parts = path.Split('\\');
            StringBuilder sb = new StringBuilder();
            for (int i = start; i < parts.Length - 1; i++)
                sb.Append(parts[i] + "\\");
            sb.Append(parts[parts.Length - 1]);
            return sb.ToString();
        }

        public static Dictionary<uint, string> ResTypes = new Dictionary<uint, string>()
 
        #region data
        {
            {0x1091c8c5, "morphtargets"},
            {0x10f0e5a1, "shaderprogramdb"},
            {0x24a019cc, "material"},
            {0x2d47a5ff, "gfx"},
            {0x30b4a553, "occludermesh"},
            {0x319d8cd0, "ragdoll"},
            {0x36f3f2c0, "shaderdb"},
            {0x4864737b, "hkdestruction"},
            {0x49b156d4, "mesh"},
            {0x51a3c853, "ant"},
            {0x59c79990, "facefx"},
            {0x59ceeb57, "shaderdatabase"},
            {0x5bdfdefe, "lightingsystem"},
            {0x5c4954a6, "itexture"},
            {0x5e862e05, "talktable"},
            {0x6bb6d7d2, "streamingstub"},
            {0x70c5cb3e, "enlighten"},
            {0x76742dc8, "delayloadbundles"},
            {0x7aefc446, "staticenlighten"},
            {0x91043f65, "hknondestruction"},
            {0x957c32b1, "alttexture"},
            {0xa23e75db, "layercombinations"},
            {0xafecb022, "luac"},
            {0xc6cd3286, "static"},
            {0xc6dbee07, "mohwspecific"},
            {0xd070eed1, "animtrackdata"},
            {0xe156af73, "probeset"},
            {0xe36f0d59, "clothasset"},
            {0xeb228507, "headmoprh"},
            {0xefc70728, "zs"}
        };
        #endregion
    }

    public static class HalfUtils
    {
        private static readonly ushort[] FloatToHalfBaseTable;
        private static readonly byte[] FloatToHalfShiftTable;
        private static readonly int[] HalfToFloatExponentTable;
        private static readonly uint[] HalfToFloatMantissaTable;
        private static readonly uint[] HalfToFloatOffsetTable;

        static HalfUtils()
        {
            int num;
            HalfToFloatMantissaTable = new uint[0x800];
            HalfToFloatExponentTable = new int[0x40];
            HalfToFloatOffsetTable = new uint[0x40];
            FloatToHalfBaseTable = new ushort[0x200];
            FloatToHalfShiftTable = new byte[0x200];
            HalfToFloatMantissaTable[0] = 0;
            for (num = 1; num < 0x400; num++)
            {
                uint num2 = (uint)(num << 13);
                uint num3 = 0;
                while ((num2 & 0x800000) == 0)
                {
                    num3 -= 0x800000;
                    num2 = num2 << 1;
                }
                num2 &= 0xff7fffff;
                num3 += 0x38800000;
                HalfToFloatMantissaTable[num] = num2 | num3;
            }
            for (num = 0x400; num < 0x800; num++)
            {
                HalfToFloatMantissaTable[num] = (uint)(0x38000000 + ((num - 0x400) << 13));
            }
            HalfToFloatExponentTable[0] = 0;
            for (num = 1; num < 0x3f; num++)
            {
                if (num >= 0x1f)
                {
                    HalfToFloatExponentTable[num] = -2147483648 + ((num - 0x20) << 0x17);
                }
                else
                {
                    HalfToFloatExponentTable[num] = num << 0x17;
                }
            }
            HalfToFloatExponentTable[0x1f] = 0x47800000;
            HalfToFloatExponentTable[0x20] = -2147483648;
            HalfToFloatExponentTable[0x3f] = -947912704;
            HalfToFloatOffsetTable[0] = 0;
            for (num = 1; num < 0x40; num++)
            {
                HalfToFloatOffsetTable[num] = 0x400;
            }
            HalfToFloatOffsetTable[0x20] = 0;
            for (num = 0; num < 0x100; num++)
            {
                int num4 = num - 0x7f;
                if (num4 < -24)
                {
                    FloatToHalfBaseTable[num] = 0;
                    FloatToHalfBaseTable[num | 0x100] = 0x8000;
                    FloatToHalfShiftTable[num] = 0x18;
                    FloatToHalfShiftTable[num | 0x100] = 0x18;
                }
                else if (num4 < -14)
                {
                    FloatToHalfBaseTable[num] = (ushort)(((int)0x400) >> (-num4 - 14));
                    FloatToHalfBaseTable[num | 0x100] = (ushort)((((int)0x400) >> (-num4 - 14)) | 0x8000);
                    FloatToHalfShiftTable[num] = Convert.ToByte((int)(-num4 - 1));
                    FloatToHalfShiftTable[num | 0x100] = Convert.ToByte((int)(-num4 - 1));
                }
                else if (num4 <= 15)
                {
                    FloatToHalfBaseTable[num] = (ushort)((num4 + 15) << 10);
                    FloatToHalfBaseTable[num | 0x100] = (ushort)(((num4 + 15) << 10) | 0x8000);
                    FloatToHalfShiftTable[num] = 13;
                    FloatToHalfShiftTable[num | 0x100] = 13;
                }
                else if (num4 >= 0x80)
                {
                    FloatToHalfBaseTable[num] = 0x7c00;
                    FloatToHalfBaseTable[num | 0x100] = 0xfc00;
                    FloatToHalfShiftTable[num] = 13;
                    FloatToHalfShiftTable[num | 0x100] = 13;
                }
                else
                {
                    FloatToHalfBaseTable[num] = 0x7c00;
                    FloatToHalfBaseTable[num | 0x100] = 0xfc00;
                    FloatToHalfShiftTable[num] = 0x18;
                    FloatToHalfShiftTable[num | 0x100] = 0x18;
                }
            }
        }

        public static ushort Pack(float f)
        {
            FloatToUint num = new FloatToUint
            {
                floatValue = f
            };
            return (ushort)(FloatToHalfBaseTable[((int)(num.uintValue >> 0x17)) & 0x1ff] + ((num.uintValue & 0x7fffff) >> (FloatToHalfShiftTable[((int)(num.uintValue >> 0x17)) & 0x1ff] & 0x1f)));
        }

        public static float Unpack(ushort h)
        {
            FloatToUint num = new FloatToUint
            {
                uintValue = HalfToFloatMantissaTable[((int)HalfToFloatOffsetTable[h >> 10]) + (h & 0x3ff)] + ((uint)HalfToFloatExponentTable[h >> 10])
            };
            return num.floatValue;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct FloatToUint
        {
            [FieldOffset(0)]
            public float floatValue;
            [FieldOffset(0)]
            public uint uintValue;
        }
    }

    public class Vector
    {
        public float[] members;
        public Vector(params float[] v)
        {
            members = v;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("( ");
            foreach (float f in members)
                sb.Append(f + " ");
            sb.Append(")");
            return sb.ToString();
        }

        public static Vector operator *(Vector left, float right)
        {
            float[] result = new float[left.members.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = left.members[i] * right;
            }
            return new Vector(result);
        }
    }

    public class ArrayPointer
    {
        public uint count;
        public ulong pointer;
        public ArrayPointer(Stream s)
        {
            count = Helpers.ReadUInt(s);
            pointer = Helpers.ReadULong(s);
        }

        public override string ToString()
        {
            return "(" + count + " items @0x" + pointer.ToString("X") + ")";
        }
    }
}
