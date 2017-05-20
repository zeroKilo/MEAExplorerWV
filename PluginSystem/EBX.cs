using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PluginSystem
{
    public class EBX
    {
        public bool _isvalid = false;
        public EBXHeader header;
        public Dictionary<FBGuid, FBGuid> imports;
        public Dictionary<int, string> keywords;
        public List<EBXLayoutDesc> lDescriptors;
        public List<EBXTypeDesc> tDescriptors;
        public List<EBXType> types;
        public List<EBXArray> arrays;
        public List<string> strings;
        public List<EBXNodeType> nodelist;

        public byte[] rawBuffer;
        public int guidcount;

        public EBX(Stream s)
        {
            rawBuffer = new byte[s.Length];
            s.Read(rawBuffer, 0, (int)s.Length);
            s.Seek(0, 0);
            header = new EBXHeader(s);
            if (!header._isvalid) return;
            ReadImports(s);
            ReadKeyWords(s);
            ReadLayoutDescriptors(s);
            ReadTypeDescriptors(s);
            ReadTypes(s);
            ReadArrays(s);
            ReadStrings(s);
            long pos = s.Position;
            try
            {
                ReadPayload(s);
                _isvalid = true;
            }
            catch { }
            if (!_isvalid)
                try
                {
                    s.Seek(pos, 0);
                    ReadPayload2(s);
                    _isvalid = true;
                }
                catch { }

        }

        private void ReadImports(Stream s)
        {
            imports = new Dictionary<FBGuid, FBGuid>();
            for (int i = 0; i < header.importCount; i++)
                imports.Add(new FBGuid(s), new FBGuid(s));
        }

        private void ReadKeyWords(Stream s)
        {
            long start = s.Position;
            keywords = new Dictionary<int, string>();
            while (s.Position - start < header.typeStringTableSize)
            {
                string word = Helpers.ReadNullString(s);
                int hash = Helpers.HashFNV1(word);
                if (word != "") keywords.Add(hash, word);
            }
            keywords = keywords.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        }

        private void ReadLayoutDescriptors(Stream s)
        {
            lDescriptors = new List<EBXLayoutDesc>();
            for (int i = 0; i < header.fieldDescriptorCount; i++)
                lDescriptors.Add(new EBXLayoutDesc(s, this));
        }

        private void ReadTypeDescriptors(Stream s)
        {
            tDescriptors = new List<EBXTypeDesc>();
            for (int i = 0; i < header.typeDescriptorCount; i++)
                tDescriptors.Add(new EBXTypeDesc(s, this));
        }

        private void ReadTypes(Stream s)
        {
            types = new List<EBXType>();
            for (int i = 0; i < header.typeCount; i++)
                types.Add(new EBXType(s));
            while (s.Position % 0x10 != 0)
                s.Seek(1, SeekOrigin.Current);
        }

        private void ReadArrays(Stream s)
        {
            arrays = new List<EBXArray>();
            for (int i = 0; i < header.arrayCount; i++)
                arrays.Add(new EBXArray(s));
            while (s.Position % 0x10 != 0)
                s.Seek(1, SeekOrigin.Current);
        }

        private void ReadStrings(Stream s)
        {
            long start = s.Position;
            strings = new List<string>();
            while (s.Position - start < header.stringTableSize)
            {
                string str = Helpers.ReadNullString(s);
                if (str != "")
                    strings.Add(str);
            }
        }

        private void ReadPayload(Stream s)
        {
            nodelist = new List<EBXNodeType>();
            guidcount = 0;
            for (int i = 0; i < header.typeCount; i++)
            {
                for (int j = 0; j < types[i].count; j++)
                {
                    EBXTypeDesc desc = tDescriptors[types[i].typeDescIndex];
                    Helpers.AlignStream(s, desc.alignment);
                    nodelist.Add(ReadTypeNode(s, desc, guidcount < header.numGUIDRepeater));
                }
                guidcount++;
            }
        }

        private EBXNodeType ReadTypeNode(Stream s, EBXTypeDesc desc, bool hasGUID)
        {
            EBXNodeType node = new EBXNodeType();
            node.Text = keywords[desc.nameHash];
            if (hasGUID)
                node.guid = new FBGuid(s);
            node.typeDesc = desc;
            node.fields = new List<EBXNodeField>();
            for (int i = 0; i < desc.fieldCount; i++)
                node.fields.Add(ReadFieldNode(s, lDescriptors[desc.layoutDescIndex + i]));
            Helpers.AlignStream(s, desc.alignment);
            return node;
        }

        private EBXNodeField ReadFieldNode(Stream s, EBXLayoutDesc layout)
        {
            EBXNodeField node = new EBXNodeField();
            node.layout = layout;
            node.offset = s.Position;
            node.Text = keywords[layout.nameHash];
            long pos;
            int offset;
            byte[] buff;
            EBXTypeDesc typeDesc;
            byte t = layout.GetFieldType();
            switch (t)
            {
                case 0:
                case 2:
                    node.data = ReadTypeNode(s, tDescriptors[layout.typeIndex], false);
                    break;
                case 4:
                    int index = Helpers.ReadInt(s);
                    pos = s.Position;
                    if (index < 0)
                        node.data = new List<EBXNodeField>();
                    else
                    {
                        EBXArray arrDesc = arrays[index];
                        s.Seek(header.arrayBlockOffset + arrDesc.offset, 0);
                        typeDesc = tDescriptors[arrDesc.typeDescIndex];
                        List<EBXNodeField> list = new List<EBXNodeField>();
                        for (int i = 0; i < arrDesc.count; i++)
                            list.Add(ReadFieldNode(s, lDescriptors[typeDesc.layoutDescIndex]));
                        node.data = list;
                    }
                    s.Seek(pos, 0);
                    break;
                case 7:
                    offset = Helpers.ReadInt(s);
                    pos = s.Position;
                    if (offset == -1)
                        node.data = "(null)";
                    else
                    {
                        s.Seek(header.metaSize + offset, 0);
                        node.data = Helpers.ReadNullString(s);
                    }
                    s.Seek(pos, 0);
                    break;
                case 8:
                    offset = Helpers.ReadInt(s);
                    typeDesc = tDescriptors[layout.typeIndex];
                    if (typeDesc.fieldCount == 0)
                        node.data = "";
                    else
                        for (int i = typeDesc.layoutDescIndex; i < typeDesc.layoutDescIndex + typeDesc.fieldCount; i++)
                            if (lDescriptors[i].fieldOffset == offset)
                            {
                                node.data = keywords[lDescriptors[i].nameHash];
                                break;
                            }
                    break;
                case 0xA:
                case 0xB:
                case 0xC:
                    node.data = (byte)s.ReadByte();
                    break;
                case 0xD:
                case 0xE:
                    node.data = Helpers.ReadUShort(s);
                    break;
                case 3:
                case 0xF:
                case 0x10:
                    node.data = Helpers.ReadUInt(s);
                    break;
                case 0x13:
                    node.data = Helpers.ReadFloat(s);
                    break;
                case 0x11:
                case 0x12:
                case 0x14:
                case 0x17:
                    node.data = Helpers.ReadULong(s);
                    break;
                case 0x15:
                    buff = new byte[0x10];
                    s.Read(buff, 0, 0x10);
                    node.data = buff;
                    break;
                case 0x16:
                    buff = new byte[0x14];
                    s.Read(buff, 0, 0x14);
                    node.data = buff;
                    break;
                default:
                    throw new Exception("Unknown FieldType : 0x" + layout.GetFieldType().ToString("X"));
            }
            return node;
        }
        #region hack
        public uint payloadoffset;
        private void ReadPayload2(Stream s)
        {
            nodelist = new List<EBXNodeType>();
            guidcount = 0;
            for (int i = 0; i < header.typeCount; i++)
            {
                for (int j = 0; j < types[i].count; j++)
                {
                    EBXTypeDesc desc = tDescriptors[types[i].typeDescIndex];
                    Helpers.AlignStream(s, desc.alignment);
                    payloadoffset = (uint)s.Position;
                    if (guidcount < header.numGUIDRepeater)
                        payloadoffset += 0x10;
                    nodelist.Add(ReadTypeNode2(s, desc, guidcount < header.numGUIDRepeater));
                    s.Seek(payloadoffset + desc.instanceSize, 0);
                }
                guidcount++;
            }
        }

        private EBXNodeType ReadTypeNode2(Stream s, EBXTypeDesc desc, bool hasGUID)
        {
            EBXNodeType node = new EBXNodeType();
            node.Text = keywords[desc.nameHash];
            if (hasGUID)
                node.guid = new FBGuid(s);
            node.typeDesc = desc;
            node.fields = new List<EBXNodeField>();
            for (int i = 0; i < desc.fieldCount; i++)
                node.fields.Add(ReadFieldNode2(s, lDescriptors[desc.layoutDescIndex + i]));
            Helpers.AlignStream(s, desc.alignment);
            return node;
        }

        private EBXNodeField ReadFieldNode2(Stream s, EBXLayoutDesc layout)
        {
            EBXNodeField node = new EBXNodeField();
            node.layout = layout;
            node.offset = layout.fieldOffset;
            node.Text = keywords[layout.nameHash];
            long pos;
            int offset;
            byte[] buff;
            EBXTypeDesc typeDesc;
            byte t = layout.GetFieldType();
            s.Seek(payloadoffset + layout.fieldOffset, 0);
            switch (t)
            {
                case 0:
                case 2:
                    node.data = ReadTypeNode2(s, tDescriptors[layout.typeIndex], false);
                    break;
                case 4:
                    int index = Helpers.ReadInt(s);
                    pos = s.Position;
                    if (index < 0)
                        node.data = new List<EBXNodeField>();
                    else
                    {
                        EBXArray arrDesc = arrays[index];
                        s.Seek(header.arrayBlockOffset + arrDesc.offset, 0);
                        typeDesc = tDescriptors[arrDesc.typeDescIndex];
                        List<EBXNodeField> list = new List<EBXNodeField>();
                        for (int i = 0; i < arrDesc.count; i++)
                            list.Add(ReadFieldNode2(s, lDescriptors[typeDesc.layoutDescIndex]));
                        node.data = list;
                    }
                    break;
                case 7:
                    offset = Helpers.ReadInt(s);
                    pos = s.Position;
                    if (offset == -1)
                        node.data = "(null)";
                    else
                    {
                        s.Seek(header.metaSize + offset, 0);
                        node.data = Helpers.ReadNullString(s);
                    }
                    break;
                case 8:
                    offset = Helpers.ReadInt(s);
                    typeDesc = tDescriptors[layout.typeIndex];
                    if (typeDesc.fieldCount == 0)
                        node.data = "";
                    else
                        for (int i = typeDesc.layoutDescIndex; i < typeDesc.layoutDescIndex + typeDesc.fieldCount; i++)
                            if (lDescriptors[i].fieldOffset == offset)
                            {
                                node.data = keywords[lDescriptors[i].nameHash];
                                break;
                            }
                    break;
                case 0xA:
                case 0xB:
                case 0xC:
                    node.data = (byte)s.ReadByte();
                    break;
                case 0xD:
                case 0xE:
                    node.data = Helpers.ReadUShort(s);
                    break;
                case 3:
                case 0xF:
                case 0x10:
                    node.data = Helpers.ReadUInt(s);
                    break;
                case 0x13:
                    node.data = Helpers.ReadFloat(s);
                    break;
                case 0x11:
                case 0x12:
                case 0x14:
                case 0x17:
                    node.data = Helpers.ReadULong(s);
                    break;
                case 0x15:
                    buff = new byte[0x10];
                    s.Read(buff, 0, 0x10);
                    node.data = buff;
                    break;
                case 0x16:
                    buff = new byte[0x14];
                    s.Read(buff, 0, 0x14);
                    node.data = buff;
                    break;
                default:
                    throw new Exception("Unknown FieldType : 0x" + layout.GetFieldType().ToString("X"));
            }
            return node;
        }
        #endregion
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(header.ToString());
            sb.AppendLine("Imports:");
            foreach (KeyValuePair<FBGuid, FBGuid> pair in imports)
                sb.AppendLine(" " + pair.Key + " - " + pair.Value);
            sb.AppendLine();
            sb.AppendLine("Keywords:");
            foreach (KeyValuePair<int, string> pair in keywords)
                sb.AppendLine(" " + pair.Key.ToString("X8") + " - " + pair.Value);
            sb.AppendLine();
            return sb.ToString();
        }

        public TreeNode ToNode()
        {
            TreeNode result = new TreeNode("EBX");
            foreach (EBXNodeType ebx in nodelist)
                result.Nodes.Add(ebx.ToNode(this));
            result.Expand();
            return result;
        }

        public string PrintFieldDescriptors()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < header.fieldDescriptorCount; i++)
            {
                sb.AppendLine("Descriptor " + i.ToString() + " : ");
                sb.AppendLine(lDescriptors[i].ToString());
            }
            return sb.ToString();
        }

        public string PrintTypeDescriptors()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < header.typeDescriptorCount; i++)
            {
                sb.AppendLine("Descriptor " + i.ToString() + " : ");
                sb.AppendLine(tDescriptors[i].ToString());
            }
            return sb.ToString();
        }

        public string PrintFields()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < header.typeCount; i++)
                sb.AppendLine(types[i].ToString());
            return sb.ToString();
        }

        public string PrintArrays()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < header.arrayCount; i++)
            {
                sb.AppendLine("Array Descriptor " + i.ToString() + " : ");
                sb.AppendLine(arrays[i].ToString());
            }
            return sb.ToString();
        }

        public string PrintStringTable()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < strings.Count; i++)
                sb.AppendLine(i.ToString("X4") + " : " + strings[i]);
            return sb.ToString();
        }

        public class EBXHeader
        {
            public bool _isvalid = false;
            public uint magic;
            public uint metaSize;
            public uint payloadSize;
            public uint importCount;
            public ushort typeCount;
            public ushort numGUIDRepeater;
            public ushort unk01;
            public ushort typeDescriptorCount;
            public ushort fieldDescriptorCount;
            public ushort typeStringTableSize;
            public uint stringTableSize;
            public uint arrayCount;
            public uint arrayOffset;
            public FBGuid GUID;
            public ulong unk02;

            public uint arrayBlockOffset;

            public EBXHeader(Stream s)
            {
                magic = Helpers.ReadUInt(s);
                if (magic != 0x0fb2d1ce) return;
                metaSize = Helpers.ReadUInt(s);
                payloadSize = Helpers.ReadUInt(s);
                importCount = Helpers.ReadUInt(s);
                typeCount = Helpers.ReadUShort(s);
                numGUIDRepeater = Helpers.ReadUShort(s);
                unk01 = Helpers.ReadUShort(s);
                typeDescriptorCount = Helpers.ReadUShort(s);
                fieldDescriptorCount = Helpers.ReadUShort(s);
                typeStringTableSize = Helpers.ReadUShort(s);
                stringTableSize = Helpers.ReadUInt(s);
                arrayCount = Helpers.ReadUInt(s);
                arrayOffset = Helpers.ReadUInt(s);
                GUID = new FBGuid(s);
                unk02 = Helpers.ReadULong(s);
                arrayBlockOffset = metaSize + stringTableSize + arrayOffset;
                _isvalid = true;
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Header:");
                sb.AppendLine("Magic                   : 0x" + magic.ToString("X8"));
                sb.AppendLine("Meta Size               : 0x" + metaSize.ToString("X8"));
                sb.AppendLine("Payload Size            : 0x" + payloadSize.ToString("X8"));
                sb.AppendLine("Import Count            : 0x" + importCount.ToString("X8"));
                sb.AppendLine("Type Count              : 0x" + typeCount.ToString("X4"));
                sb.AppendLine("GUID Repeater           : 0x" + numGUIDRepeater.ToString("X4"));
                sb.AppendLine("Unknown 01              : 0x" + unk01.ToString("X4"));
                sb.AppendLine("Type Descriptors        : 0x" + typeDescriptorCount.ToString("X4"));
                sb.AppendLine("Field Descriptors       : 0x" + fieldDescriptorCount.ToString("X4"));
                sb.AppendLine("Type String Table Size  : 0x" + typeStringTableSize.ToString("X4"));
                sb.AppendLine("String Table Size       : 0x" + stringTableSize.ToString("X8"));
                sb.AppendLine("Array Count             : 0x" + arrayCount.ToString("X8"));
                sb.AppendLine("Array Offset            : 0x" + arrayOffset.ToString("X8"));
                sb.AppendLine("GUID                    : " + GUID);
                sb.AppendLine("Unknown 02              : 0x" + unk02.ToString("X16"));
                return sb.ToString();
            }
        }

        public class EBXLayoutDesc
        {
            public int nameHash;
            public ushort flags;
            public ushort typeIndex;
            public int fieldOffset;
            public int secondaryOffset;

            private EBX parent;
            public EBXLayoutDesc(Stream s, EBX ebx)
            {
                parent = ebx;
                nameHash = Helpers.ReadInt(s);
                flags = Helpers.ReadUShort(s);
                typeIndex = Helpers.ReadUShort(s);
                fieldOffset = Helpers.ReadInt(s);
                if (ebx.keywords[nameHash] == "$")
                    fieldOffset -= 8;
                secondaryOffset = Helpers.ReadInt(s);
            }

            public string GetName()
            {
                return parent.keywords[nameHash];
            }

            public byte GetFieldType()
            {
                return (byte)((flags >> 4) & 0x1F);
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(" Name             : 0x" + nameHash.ToString("X8") + " " + GetName());
                sb.AppendLine(" Flags            : 0x" + flags.ToString("X4") + "(" + GetFieldType() + ")");
                sb.AppendLine(" Type Index       : 0x" + typeIndex.ToString("X4"));
                sb.AppendLine(" Field Offset     : 0x" + fieldOffset.ToString("X8"));
                sb.AppendLine(" Secondary Offset : 0x" + secondaryOffset.ToString("X8"));
                return sb.ToString();
            }
        }

        public class EBXTypeDesc
        {
            public int nameHash;
            public int layoutDescIndex;
            public byte fieldCount;
            public byte alignment;
            public ushort flags;
            public ushort instanceSize;
            public ushort secondaryInstanceSize;

            private EBX parent;
            public EBXTypeDesc(Stream s, EBX ebx)
            {
                parent = ebx;
                nameHash = Helpers.ReadInt(s);
                layoutDescIndex = Helpers.ReadInt(s);
                fieldCount = (byte)s.ReadByte();
                alignment = (byte)s.ReadByte();
                flags = Helpers.ReadUShort(s);
                instanceSize = Helpers.ReadUShort(s);
                secondaryInstanceSize = Helpers.ReadUShort(s);
            }

            public string GetName()
            {
                return parent.keywords[nameHash];
            }

            public byte GetFieldType()
            {
                return (byte)((flags >> 4) & 0x1F);
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(" Name                    : 0x" + nameHash.ToString("X8") + " " + GetName());
                sb.AppendLine(" Field Desc Index        : 0x" + layoutDescIndex.ToString("X8"));
                sb.AppendLine(" Field Count             : 0x" + fieldCount.ToString("X2"));
                sb.AppendLine(" Alignment               : 0x" + alignment.ToString("X2"));
                sb.AppendLine(" Flags                   : 0x" + flags.ToString("X4") + "(" + GetFieldType() + ")");
                sb.AppendLine(" Instance Size           : 0x" + instanceSize.ToString("X4"));
                sb.AppendLine(" Secondary Instance Size : 0x" + secondaryInstanceSize.ToString("X4"));
                return sb.ToString();
            }
        }

        public class EBXType
        {
            public ushort typeDescIndex;
            public ushort count;

            public EBXType(Stream s)
            {
                typeDescIndex = Helpers.ReadUShort(s);
                count = Helpers.ReadUShort(s);
            }

            public override string ToString()
            {
                return typeDescIndex.ToString("X4") + " x " + count;
            }
        }

        public class EBXArray
        {
            public int offset;
            public int count;
            public int typeDescIndex;

            public EBXArray(Stream s)
            {
                offset = Helpers.ReadInt(s);
                count = Helpers.ReadInt(s);
                typeDescIndex = Helpers.ReadInt(s);
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Offset          : 0x" + offset.ToString("X8"));
                sb.AppendLine("Count           : 0x" + count.ToString("X8"));
                sb.AppendLine("Type Desc Index : 0x" + typeDescIndex.ToString("X8"));
                return sb.ToString();
            }
        }

        public class EBXNodeType : TreeNode
        {
            public FBGuid guid;
            public EBXTypeDesc typeDesc;
            public List<EBXNodeField> fields;
            public TreeNode ToNode(EBX ebx)
            {
                TreeNode result = new TreeNode(ebx.keywords[typeDesc.nameHash]);
                foreach (EBXNodeField field in fields)
                    result.Nodes.Add(field.ToNode(ebx));
                return result;
            }
        }

        public class EBXNodeField : TreeNode
        {
            public EBXLayoutDesc layout;
            public long offset;
            public object data;

            public TreeNode ToNode(EBX ebx)
            {
                TreeNode result = new TreeNode(ebx.keywords[layout.nameHash]);
                byte t = layout.GetFieldType();
                switch (t)
                {
                    case 3:
                        if ((uint)data - 1 < ebx.nodelist.Count && (int)(uint)data > 0)
                            result.Text = "ref " + ebx.nodelist[(int)(uint)data - 1].Text;
                        else
                            result.Nodes.Add("import ref " + ((uint)data & 0x7FFFFFFF));
                        break;
                    case 0:
                    case 2:
                        result.Nodes.Add(((EBXNodeType)data).ToNode(ebx));
                        break;
                    case 4:
                        List<EBXNodeField> list = (List<EBXNodeField>)data;
                        foreach (EBXNodeField field in list)
                            result.Nodes.Add(field.ToNode(ebx));
                        break;
                    case 7:
                    case 8:
                        result.Nodes.Add((string)data);
                        break;
                    case 0xA:
                    case 0xB:
                    case 0xC:
                        result.Nodes.Add(((byte)data).ToString("X"));
                        break;
                    case 0xD:
                    case 0xE:
                        result.Nodes.Add(((ushort)data).ToString("X"));
                        break;
                    case 0xF:
                    case 0x10:
                        result.Nodes.Add(((uint)data).ToString("X"));
                        break;
                    case 0x13:
                        result.Nodes.Add(((float)data).ToString());
                        break;
                    case 0x11:
                    case 0x12:
                    case 0x14:
                    case 0x17:
                        result.Nodes.Add(((ulong)data).ToString("X"));
                        break;
                    case 0x15:
                    case 0x16:
                        string s = "";
                        foreach (byte b in (byte[])data)
                            s += b.ToString("X2");
                        result.Nodes.Add(s);
                        break;
                    default:
                        result.Nodes.Add("unknown field type 0x" + t.ToString("X"));
                        break;
                }
                return result;
            }
        }

        public class FBGuid
        {
            public uint data1;
            public ushort data2;
            public ushort data3;
            public byte[] data4;

            public FBGuid(Stream s)
            {
                data1 = Helpers.ReadUInt(s);
                data2 = Helpers.ReadUShort(s);
                data3 = Helpers.ReadUShort(s);
                data4 = new byte[8];
                s.Read(data4, 0, 8);
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0}-{1}-{2}-{3}", data1.ToString("X8"), data2.ToString("X4"), data3.ToString("X4"), Helpers.ByteArrayToHexString(data4));
                return sb.ToString();
            }

            public static bool operator ==(FBGuid g1, FBGuid g2)
            {
                if (g1.data1 == g2.data1 &&
                    g1.data2 == g2.data2 &&
                    g1.data3 == g2.data3 &&
                    g1.data4 == g2.data4)
                    return true;
                return false;
            }

            public static bool operator !=(FBGuid g1, FBGuid g2)
            {
                return !(g1 == g2);
            }

            public override bool Equals(object obj)
            {
                if (obj is FBGuid)
                    return (FBGuid)obj == this;
                return false;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
    }
}
