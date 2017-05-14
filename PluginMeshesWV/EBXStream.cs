using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PluginMeshesWV
{
    public class EBXStream
    {
        public Stream data;
        public MemoryStream stbl;
        public bool ErrorLoading = false;

        public struct StreamingPartitionHeader
        {
            public int magic;
            public int metaSize;
            public int payloadSize;
            public int importCount;
            public ushort typeCount;
            public ushort numGUIDRepeater;
            public ushort unknown;
            public ushort typeDescriptorCount;
            public ushort fieldDescriptorCount;
            public ushort typeStringTableSize;
            public int stringTableSize;
            public int arrayCount;
            public int arrayOffset;
            public Guid guid;
            public int _arraySectionstart;
        }

        public struct Guid
        {
            public uint data1;
            public ushort data2;
            public ushort data3;
            public byte[] data4;
            public Guid(Stream s)
            {
                data1 = Helpers.ReadUInt(s);
                data2 = Helpers.ReadUShort(s);
                data3 = Helpers.ReadUShort(s);
                data4 = new byte[8];
                s.Read(data4, 0, 8);
            }

            public string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0}-{1}-{2}-{3}", data1.ToString("X8"), data2.ToString("X4"), data3.ToString("X4"), Helpers.ByteArrayToHexString(data4));
                return sb.ToString();
            }
        }

        public struct StreamingPartitionImportEntry
        {
            public Guid partitionGuid;
            public Guid instanceGuid;
            public StreamingPartitionImportEntry(Stream s)
            {
                partitionGuid = new Guid(s);
                instanceGuid = new Guid(s);
            }
        }

        public struct KeyWordDicStruct
        {
            public string keyword;
            public int hash;
            public int offset;
        }

        public struct StreamingPartitionFieldDescriptor
        {
            public int fieldNameHash;
            public ushort flagBits;
            public ushort fieldTypeIndex;
            public int fieldOffset;
            public int secondaryOffset;
            public string _name;
            public int _index;
            public byte _type;
        }

        public struct StreamingPartitionTypeDescriptor
        {
            public int typeNameHash;
            public int layoutDescriptorIndex;
            public byte fieldCount;
            public byte alignment;
            public ushort typeFlags;
            public ushort instanceSize;
            public ushort secondaryInstanceSize;
            public string _name;
            public ushort _type;
            public int _index;
        }

        public struct StreamingPartitionTypeEntry
        {
            public ushort typeDescriptorIndex;
            public ushort repetitions;
            public StreamingPartitionTypeEntry(Stream s)
            {
                typeDescriptorIndex = Helpers.ReadUShort(s);
                repetitions = Helpers.ReadUShort(s);
            }
        }

        public struct StreamingPartitionArrayEntry
        {
            public int offset;
            public int elementCount;
            public int typeDescriptorIndex;
            public List<byte[]> data;
            public StreamingPartitionArrayEntry(Stream s)
            {
                offset = Helpers.ReadInt(s);
                elementCount = Helpers.ReadInt(s);
                typeDescriptorIndex = Helpers.ReadInt(s);
                data = new List<byte[]>();
            }
        }

        public struct Field
        {
            public StreamingPartitionFieldDescriptor Descriptor;
            public object data;
            public long offset;
        }

        public struct Type
        {
            public StreamingPartitionTypeDescriptor Descriptor;
            public List<Field> Fields;
        }

        public struct TypeEntryStruct
        {
            public Guid GUID;
            public string name;
            public Type type;
        }


        public StreamingPartitionHeader Header;
        public List<StreamingPartitionImportEntry> imports;
        public List<KeyWordDicStruct> keyWordDic;
        public List<StreamingPartitionFieldDescriptor> fieldDescriptors;
        public List<StreamingPartitionTypeDescriptor> typeDescriptors;
        public List<StreamingPartitionTypeEntry> typeList;
        public List<StreamingPartitionArrayEntry> arrayList;
        public byte[] keywordarea;
        public List<string> stringTable;
        public List<TypeEntryStruct> typeEntryList;

        public EBXStream(Stream s)
        {
            data = s;
            Process();
        }

        public void Process()
        {
            int State = 0;
            while (State != 10 && State != -1)
            {
                try
                {
                    switch (State)
                    {
                        case 0://process header
                            ProcessHeader(data);
                            if (Header.magic != 0x0fb2d1ce)
                            {
                                State = -1;//Invalid
                                break;
                            }
                            State = 1;
                            break;
                        case 1://Process meta
                            ProcessMeta(data);
                            State = 2;
                            break;
                        case 2://process string table
                            byte[] buff = new byte[Header.stringTableSize];
                            data.Read(buff, 0, Header.stringTableSize);
                            stbl = new MemoryStream(buff);
                            ProcessStringTable();
                            State = 3;
                            break;
                        case 3://process payload layout  
                            ProcessPayload(data);
                            State = 4;
                            break;
                        case 4:

                            State = 10;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    ErrorLoading = true;
                    State = -1;
                }
            }
        }

        public void ProcessHeader(Stream s)
        {
            Header = new StreamingPartitionHeader();
            Header.magic = Helpers.ReadInt(s);
            if (Header.magic != 0x0fb2d1ce)
                return;
            Header.metaSize = Helpers.ReadInt(s);
            Header.payloadSize = Helpers.ReadInt(s);
            Header.importCount = Helpers.ReadInt(s);
            Header.typeCount = Helpers.ReadUShort(s);
            Header.numGUIDRepeater = Helpers.ReadUShort(s);
            Header.unknown = Helpers.ReadUShort(s);
            Header.typeDescriptorCount = Helpers.ReadUShort(s);
            Header.fieldDescriptorCount = Helpers.ReadUShort(s);
            Header.typeStringTableSize = Helpers.ReadUShort(s);
            Header.stringTableSize = Helpers.ReadInt(s);
            Header.arrayCount = Helpers.ReadInt(s);
            Header.arrayOffset = Helpers.ReadInt(s);
            Header.guid = new Guid(s);
            Helpers.ReadLong(s);
            Header._arraySectionstart = Header.metaSize + Header.stringTableSize + Header.arrayOffset;
        }

        public void ProcessMeta(Stream s)
        {
            imports = new List<StreamingPartitionImportEntry>();
            for (int i = 0; i < Header.importCount; i++)
                imports.Add(new StreamingPartitionImportEntry(s));
            ReadKeyWords(s);
            ReadFieldDescriptors(s);
            ReadTypeDescriptors(s);
            typeList = new List<StreamingPartitionTypeEntry>();
            for (int i = 0; i < Header.typeCount; i++)
                typeList.Add(new StreamingPartitionTypeEntry(s));
            while (s.Position % 0x10 != 0)
                s.Seek(1, SeekOrigin.Current);
            arrayList = new List<StreamingPartitionArrayEntry>();
            for (int i = 0; i < Header.arrayCount; i++)
                arrayList.Add(new StreamingPartitionArrayEntry(s));
            while (s.Position % 0x10 != 0)
                s.Seek(1, SeekOrigin.Current);
        }
        private void ReadKeyWords(Stream s)
        {
            keywordarea = new byte[Header.typeStringTableSize];
            s.Read(keywordarea, 0, Header.typeStringTableSize);
            MemoryStream m = new MemoryStream(keywordarea);
            m.Seek(0, 0);
            keyWordDic = new List<KeyWordDicStruct>();
            long start = m.Position;
            while (m.Position < m.Length)
            {
                long pos = m.Position;
                string keyword = Helpers.ReadNullString(m);
                int hash = Helpers.HashFNV1(keyword);
                bool found = false;
                foreach (KeyWordDicStruct st in keyWordDic)
                    if (st.hash == hash)
                    {
                        found = true;
                        break;
                    }
                if (!found)
                {
                    KeyWordDicStruct st = new KeyWordDicStruct();
                    st.keyword = keyword;
                    st.hash = hash;
                    st.offset = (int)(pos - start);
                    keyWordDic.Add(st);
                }
            }

        }
        private void ReadFieldDescriptors(Stream s)
        {
            fieldDescriptors = new List<StreamingPartitionFieldDescriptor>();
            for (int i = 0; i < Header.fieldDescriptorCount; i++)
            {
                StreamingPartitionFieldDescriptor f = new StreamingPartitionFieldDescriptor();
                f.fieldNameHash = Helpers.ReadInt(s);
                foreach (KeyWordDicStruct key in keyWordDic)
                    if (key.hash == f.fieldNameHash)
                    {
                        f._name = key.keyword;
                        break;
                    }
                f.flagBits = Helpers.ReadUShort(s);
                f._type = (byte)((f.flagBits >> 4) & 0x1F);
                f.fieldTypeIndex = Helpers.ReadUShort(s);
                f.fieldOffset = Helpers.ReadInt(s);
                f.secondaryOffset = Helpers.ReadInt(s);
                if (f._name == "$")
                    f.fieldOffset -= 8;
                f._index = i;
                fieldDescriptors.Add(f);
            }
        }
        private void ReadTypeDescriptors(Stream s)
        {
            typeDescriptors = new List<StreamingPartitionTypeDescriptor>();
            for (int i = 0; i < Header.typeDescriptorCount; i++)
            {
                StreamingPartitionTypeDescriptor f = new StreamingPartitionTypeDescriptor();
                f.typeNameHash = Helpers.ReadInt(s);
                foreach (KeyWordDicStruct key in keyWordDic)
                    if (key.hash == f.typeNameHash)
                    {
                        f._name = key.keyword;
                        break;
                    }
                f.layoutDescriptorIndex = Helpers.ReadInt(s);
                f.fieldCount = (byte)s.ReadByte();
                f.alignment = (byte)s.ReadByte();
                f.typeFlags = Helpers.ReadUShort(s);
                f._type = (byte)((f.typeFlags >> 4) & 0x1F);
                f.instanceSize = Helpers.ReadUShort(s);
                f.secondaryInstanceSize = Helpers.ReadUShort(s);
                f._index = i;
                typeDescriptors.Add(f);
            }
        }

        public void ProcessStringTable()
        {
            stringTable = new List<string>();
            stbl.Seek(0, 0);
            while (stbl.Position < stbl.Length)
            {
                byte test = (byte)stbl.ReadByte();
                if (test == 0)
                    break;
                stbl.Seek(-1, SeekOrigin.Current);
                stringTable.Add(Helpers.ReadNullString(stbl));
            }
        }

        public void ProcessPayload(Stream s)
        {
            typeEntryList = new List<TypeEntryStruct>();
            int guidcount = 0;
            for (int i = 0; i < Header.typeCount; i++)
            {
                for (int j = 0; j < typeList[i].repetitions; j++)
                {
                    while ((s.Position % typeDescriptors[typeList[i].typeDescriptorIndex].alignment) != 0)
                        s.Seek(1, SeekOrigin.Current);
                    TypeEntryStruct entry = new TypeEntryStruct();
                    entry.name = typeDescriptors[typeList[i].typeDescriptorIndex]._name;
                    if (guidcount < Header.numGUIDRepeater)
                        entry.GUID = new Guid(s);
                    else
                    {
                        MemoryStream m2 = new MemoryStream(new byte[16]);
                        entry.GUID = new Guid(m2);
                    }
                    entry.type = ReadType(s, typeList[i].typeDescriptorIndex);
                    typeEntryList.Add(entry);
                }
                guidcount++;
            }
        }

        private int RefreshCount = 0;

        private Type ReadType(Stream s, int idx)
        {
            Type res = new Type();
            res.Descriptor = typeDescriptors[idx];
            res.Fields = new List<Field>();
            for (int i = 0; i < res.Descriptor.fieldCount; i++)
                res.Fields.Add(ReadField(s, res.Descriptor.layoutDescriptorIndex + i));
            if (res.Descriptor.alignment != 0)
                while (s.Position % res.Descriptor.alignment != 0)
                    s.Seek(1, SeekOrigin.Current);
            return res;
        }

        private Field ReadField(Stream s, int idx)
        {
            Field res = new Field();
            res.Descriptor = fieldDescriptors[idx];
            List<Field> list;
            byte[] buff;
            res.offset = s.Position;
            switch (res.Descriptor._type)
            {
                case 0:
                case 2:
                    res.data = ReadType(s, res.Descriptor.fieldTypeIndex);
                    break;
                case 3:
                    uint u = Helpers.ReadUInt(s);
                    res.data = u;
                    break;
                case 4:
                    int index = Helpers.ReadInt(s);
                    list = new List<Field>();
                    if (index < 0)
                    {
                        res.data = list;
                        break;
                    }
                    long pos = s.Position;
                    s.Seek(Header._arraySectionstart + arrayList[index].offset, 0);
                    StreamingPartitionTypeDescriptor tp = typeDescriptors[arrayList[index].typeDescriptorIndex];
                    for (int i = 0; i < arrayList[index].elementCount; i++)
                        list.Add(ReadField(s, tp.layoutDescriptorIndex));
                    s.Seek(pos, 0);
                    res.data = list;
                    break;
                case 7:
                    int offset = Helpers.ReadInt(s);
                    if (offset == -1)
                    {
                        res.data = "";
                        break;
                    }
                    stbl.Seek(offset, 0);
                    res.data = Helpers.ReadNullString(stbl);
                    break;
                case 8:
                    offset = Helpers.ReadInt(s);
                    StreamingPartitionTypeDescriptor cdesc = typeDescriptors[res.Descriptor.fieldTypeIndex];
                    string value = "";
                    if (cdesc.fieldCount != 0)
                        for (int i = cdesc.layoutDescriptorIndex; i < cdesc.layoutDescriptorIndex + cdesc.fieldCount; i++)
                            if (fieldDescriptors[i].fieldOffset == offset)
                            {
                                value = fieldDescriptors[i]._name;
                                break;
                            }
                    res.data = value;
                    break;
                case 0xa:
                case 0xb:
                case 0xc:
                    res.data = (byte)s.ReadByte();
                    break;
                case 0xd:
                case 0xe:
                    res.data = Helpers.ReadUShort(s);
                    break;
                case 0xf:
                case 0x10:
                    res.data = Helpers.ReadUInt(s);
                    break;
                case 0x13:
                    res.data = Helpers.ReadFloat(s);
                    break;
                case 0x11:
                case 0x12:
                case 0x14:
                    res.data = Helpers.ReadULong(s);
                    break;
                case 0x15:
                    buff = new byte[0x10];
                    s.Read(buff, 0, 0x10);
                    res.data = buff;
                    break;
                case 0x16:
                    buff = new byte[0x14];
                    s.Read(buff, 0, 0x14);
                    res.data = buff;
                    break;
                case 0x17:
                    break;
                default:
                    break;
            }
            return res;
        }

        public static TreeNode TypeToNode(Type t)
        {
            TreeNode res = new TreeNode("Type " + t.Descriptor._name + "[" + t.Descriptor.typeFlags.ToString("X4") + "][" + t.Descriptor.alignment.ToString("X2") + "] #" + t.Descriptor._index);
            foreach (Field f in t.Fields)
                res.Nodes.Add(FieldToNode(f));
            return res;
        }

        public static TreeNode FieldToNode(Field f)
        {
            TreeNode res = new TreeNode("Field " + f.Descriptor._name + "[" + f.Descriptor.flagBits.ToString("X4") + "] #" + f.Descriptor._index);
            switch (f.Descriptor._type)
            {
                case 0:
                case 2:
                    res.Nodes.Add(TypeToNode((Type)f.data));
                    break;
                case 4:
                    List<Field> list = (List<Field>)f.data;
                    foreach (Field f2 in list)
                        res.Nodes.Add(FieldToNode(f2));
                    break;
                case 7:
                case 8:
                    res.Nodes.Add((string)f.data);
                    break;
                case 0xa:
                case 0xb:
                case 0xc:
                    res.Nodes.Add(((byte)f.data).ToString("X2"));
                    break;
                case 0xd:
                case 0xe:
                    res.Nodes.Add(((ushort)f.data).ToString("X4"));
                    break;
                case 3:
                case 0xf:
                case 0x10:
                    res.Nodes.Add(((uint)f.data).ToString("X8"));
                    break;
                case 0x11:
                case 0x12:
                case 0x14:
                    res.Nodes.Add(((ulong)f.data).ToString("X16"));
                    break;
                case 0x15:
                case 0x16:
                    res.Nodes.Add(Helpers.ByteArrayToHexString((byte[])f.data));
                    break;
                case 0x13:
                    res.Nodes.Add(((float)f.data).ToString());
                    break;
                case 0x17:
                    break;
                default:
                    break;
            }
            return res;
        }

        public string toXML()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<EbxFile Guid=\"");
            sb.Append(Header.guid.ToString());
            sb.Append("\">\n");
            foreach (TypeEntryStruct ins in typeEntryList)
                sb.Append(InstanceToXML(ins));
            string temp = "";
            try
            {
                temp = sb.ToString(); //check for outofmem on big xml
            }
            catch (Exception)
            {
            }
            return temp;
        }

        public string InstanceToXML(TypeEntryStruct ins)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Helpers.MakeTabs(1) + "<" + ins.name + " Guid=\"");
            sb.Append(ins.GUID.ToString());
            sb.Append("\">\n");
            sb.Append(MakeComplexFieldXML(ins.type, 2));
            sb.Append(Helpers.MakeTabs(1) + "</" + ins.name + ">\n");
            return sb.ToString();
        }

        public string MakeComplexFieldXML(Type cfield, int tab)
        {
            StringBuilder sb = new StringBuilder();
            string tabs = Helpers.MakeTabs(tab);
            if (cfield.Descriptor._name != "array")
                sb.AppendFormat(tabs + "<{0}>\n", cfield.Descriptor._name);
            foreach (Field f in cfield.Fields)
                sb.Append(MakeFieldXML(f, tab + 1));
            if (cfield.Descriptor._name != "array")
                sb.AppendFormat(tabs + "</{0}>\n", cfield.Descriptor._name);
            return sb.ToString();
        }

        public string MakeFieldXML(Field field, int tab)
        {
            StringBuilder sb = new StringBuilder();
            string tabs = Helpers.MakeTabs(tab);
            string tabs2 = Helpers.MakeTabs(tab + 1);
            StreamingPartitionFieldDescriptor desc = field.Descriptor;
            if (desc._name == "$")
                return MakeComplexFieldXML((Type)field.data, tab);
            sb.AppendFormat(tabs + "<{0}>\n", desc._name);
            byte realtype = (byte)((field.Descriptor.flagBits >> 4) & 0x1F);
            if (field.data == null)
                return "";
            switch (realtype)
            {
                case 0:
                case 2:
                    sb.Append(MakeComplexFieldXML((Type)field.data, tab + 1));
                    break;
                case 4:
                    foreach (Field f in (List<Field>)field.data)
                        sb.Append(MakeFieldXML(f, tab + 1));
                    break;
                case 0x7:
                case 0x8:
                    sb.AppendFormat(tabs2 + "{0}\n", (string)field.data);
                    break;
                case 0xa:
                case 0xb:
                case 0xc:
                    sb.AppendFormat(tabs2 + "0x{0}\n", ((byte)field.data).ToString("X2"));
                    break;
                case 0xd:
                case 0xe:
                    sb.AppendFormat(tabs2 + "0x{0}\n", ((ushort)field.data).ToString("X4"));
                    break;
                case 0x3:
                case 0xf:
                case 0x10:
                    sb.AppendFormat(tabs2 + "0x{0}\n", ((uint)field.data).ToString("X8"));
                    break;
                case 0x13:
                    sb.AppendFormat(tabs2 + "{0}f\n", ((float)field.data).ToString());
                    break;
                case 0x15:
                case 0x16:
                    sb.AppendFormat(tabs2 + "{0}\n", Helpers.ByteArrayToHexString((byte[])field.data));
                    break;
                case 0x17:
                    break;
                default:
                    break;
            }
            sb.AppendFormat(tabs + "</{0}>\n", desc._name);
            return sb.ToString();
        }
    }
}
