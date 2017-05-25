using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginSystem;

namespace PluginSystem
{
    public enum MeshType
    {
        MeshType_Rigid = 0,
        MeshType_Skinned = 1,
        MeshType_Composite = 2
    }
    public enum PrimType
    {
        PrimitiveType_PointList = 0,
        PrimitiveType_LineList = 1,
        PrimitiveType_LineStrip = 2,
        PrimitiveType_TriangleList = 3,
        PrimitiveType_TriangleStrip = 5,
        PrimitiveType_QuadList = 7,
        PrimitiveType_XenonRectList = 8,
    }

    public class MeshAsset
    {
        public MeshHeader header;
        public List<MeshLOD> lods;
        public MeshAsset(Stream s)
        {
            header = new MeshHeader(s);
            lods = new List<MeshLOD>();
            foreach (long l in header.lodOffsets)
                if (l != 0)
                {
                    s.Seek(l, 0);
                    lods.Add(new MeshLOD(s, header.sectionCount / header.lodCount));
                }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Header:");
            sb.AppendLine(header.ToString());
            for (int i = 0; i < lods.Count; i++)
            {
                sb.AppendLine(">LOD " + i);
                sb.AppendLine(lods[i].ToString());
            }
            return sb.ToString();
        }
    }

    public class MeshHeader
    {
        public Vector bbox_min;
        public Vector bbox_max;
        public ulong[] lodOffsets;
        public string name;
        public string shortName;
        public uint nameHash;
        public MeshType type;
        public uint unk01;
        public uint flags;
        public ushort lodCount;
        public ushort sectionCount;
        public ushort unk02;
        public ushort unk03;
        public ulong unk04;
        public ulong unk05;
        public MeshHeader(Stream s)
        {
            bbox_min = new Vector(Helpers.ReadFloat(s), Helpers.ReadFloat(s), Helpers.ReadFloat(s), Helpers.ReadFloat(s));
            bbox_max = new Vector(Helpers.ReadFloat(s), Helpers.ReadFloat(s), Helpers.ReadFloat(s), Helpers.ReadFloat(s));
            lodOffsets = new ulong[7];
            for (int i = 0; i < 7; i++)
                lodOffsets[i] = Helpers.ReadULong(s);
            name = Helpers.ReadStringPointer(s);
            shortName = Helpers.ReadStringPointer(s);
            nameHash = Helpers.ReadUInt(s);
            type = (MeshType)Helpers.ReadUInt(s);
            unk01 = Helpers.ReadUInt(s);
            flags = Helpers.ReadUInt(s);
            lodCount = Helpers.ReadUShort(s);
            sectionCount = Helpers.ReadUShort(s);
            unk02 = Helpers.ReadUShort(s);
            unk03 = Helpers.ReadUShort(s);
            unk04 = Helpers.ReadULong(s);
            unk05 = Helpers.ReadULong(s);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Bounding Box Min : " + bbox_min.ToString());
            sb.AppendLine("Bounding Box Max : " + bbox_max.ToString());
            sb.Append("LOD Pointers     : ");
            foreach (long l in lodOffsets)
                sb.Append("0x" + l.ToString("X") + " ");
            sb.AppendLine();
            sb.AppendLine("Name             : " + name);
            sb.AppendLine("Short Name       : " + shortName);
            sb.AppendLine("Name Hash        : 0x" + nameHash.ToString("X8"));
            sb.AppendLine("Mesh Type        : " + type);
            sb.AppendLine("Unknown01        : 0x" + unk01.ToString("X8"));
            sb.AppendLine("Flags            : 0x" + flags.ToString("X8"));
            sb.AppendLine("LOD Count        : " + lodCount);
            sb.AppendLine("Section Count    : " + sectionCount);
            sb.AppendLine("Unknown02        : 0x" + unk02.ToString("X4"));
            sb.AppendLine("Unknown03        : 0x" + unk02.ToString("X4"));
            sb.AppendLine("Unknown04        : 0x" + unk02.ToString("X16"));
            sb.AppendLine("Unknown05        : 0x" + unk02.ToString("X16"));
            return sb.ToString();
        }
    }

    public class MeshLOD
    {
        public MeshType type;
        public uint maxInstanceCount;
        public ArrayPointer sectionPointer;
        public ArrayPointer[] subIndicies;
        public uint unk01;
        public uint unk02;
        public uint indexDataSize;
        public uint vertexDataSize;
        public byte[] chunkID;
        public uint aviOffset;
        public string shaderDebugName;
        public string name;
        public string shortName;
        public uint nameHash;
        public ulong dataPointer;
        public uint bonePartCount;
        public List<MeshLodSection> sections;

        // added
        public uint bonePartOffset;
        public int[] bonesIndex;

        // count all vertices in the lod (simple sum of vertices in sections)
        public int GetLODTotalVertCount()
        {
            return sections.Sum(s => s.vertices.Count);
        }
        // end added

        public MeshLOD(Stream s, int sectionCount)
        {
            type = (MeshType)Helpers.ReadUInt(s);
            maxInstanceCount = Helpers.ReadUInt(s);
            sectionPointer = new ArrayPointer(s);
            subIndicies = new ArrayPointer[5];
            for (int i = 0; i < 5; i++)
                subIndicies[i] = new ArrayPointer(s);
            unk01 = Helpers.ReadUInt(s);
            unk02 = Helpers.ReadUInt(s);
            indexDataSize = Helpers.ReadUInt(s);
            vertexDataSize = Helpers.ReadUInt(s);
            chunkID = new byte[0x10];
            s.Read(chunkID, 0, 0x10);
            aviOffset = Helpers.ReadUInt(s);
            shaderDebugName = Helpers.ReadStringPointer(s);
            name = Helpers.ReadStringPointer(s);
            shortName = Helpers.ReadStringPointer(s);
            nameHash = Helpers.ReadUInt(s);
            dataPointer = Helpers.ReadULong(s);
            bonePartCount = Helpers.ReadUInt(s);

            // added
            bonePartOffset = Helpers.ReadUInt(s);
            bonesIndex = new int[bonePartCount];
            s.Seek(bonePartOffset, SeekOrigin.Begin);
            for (int i = 0; i < bonePartCount; i++)
            {
                bonesIndex[i] = Helpers.ReadInt(s);
            }
            // end added

            s.Seek((long)sectionPointer.pointer, 0);
            sections = new List<MeshLodSection>();
            for (int i = 0; i < sectionCount; i++)
                sections.Add(new MeshLodSection(s, i));
        }

        public void LoadVertexData(Stream s)
        {
            foreach (MeshLodSection section in sections)
                section.LoadVertexData(s, this);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("-Mesh Type            : " + type);
            sb.AppendLine("-Max Instance Count   : " + (int)maxInstanceCount);
            sb.AppendLine("-Section Pointer      : " + sectionPointer);
            sb.Append("-SubIndicies Pointers : ");
            foreach (ArrayPointer ap in subIndicies)
                sb.Append(ap.ToString() + " ");
            sb.AppendLine();
            sb.AppendLine("-Unknown01            : 0x" + unk01.ToString("X8"));
            sb.AppendLine("-Unknown02            : 0x" + unk01.ToString("X8"));
            sb.AppendLine("-Index Data Size      : 0x" + indexDataSize.ToString("X8"));
            sb.AppendLine("-Vertex Data Size     : 0x" + vertexDataSize.ToString("X8"));
            sb.Append("-Chunk ID             : ");
            foreach (byte b in chunkID)
                sb.Append(b.ToString("X2"));
            sb.AppendLine();
            sb.AppendLine("-Shader Debug Name    : " + shaderDebugName);
            sb.AppendLine("-Name                 : " + name);
            sb.AppendLine("-Short Name           : " + shortName);
            sb.AppendLine("-Name Hash            : 0x" + nameHash.ToString("X8"));
            sb.AppendLine("-Data Pointer         : 0x" + dataPointer.ToString("X"));
            sb.AppendLine("-Bone Part Count      : " + bonePartCount);
            foreach (MeshLodSection section in sections)
                sb.Append(section.ToString());
            return sb.ToString();
        }
    }

    public class MeshLodSection
    {
        public ulong unk01;
        public string matName;
        public ulong matIndex;
        public uint triCount;
        public uint indStart;
        public uint vertOffset;
        public uint vertCount;
        public byte unk02a;
        public PrimType primType;
        public byte unk02b;
        public byte unk02c;
        public uint unk03;
        public uint unk04;
        public VertexDescriptor[] vertDesc;
        public ulong vertexStride;
        public ulong unk06;
        public uint unk07;
        public float[] unk08;
        public ulong[] unk09;

        public int index;
        public List<Vertex> vertices;
        public List<ushort> indicies;

        public ushort[] boneIndices;

        public MeshLodSection(Stream s, int idx)
        {
            index = idx;
            unk01 = Helpers.ReadULong(s);
            matName = Helpers.ReadStringPointer(s);
            matIndex = Helpers.ReadULong(s);
            triCount = Helpers.ReadUInt(s);
            indStart = Helpers.ReadUInt(s);
            vertOffset = Helpers.ReadUInt(s);
            vertCount = Helpers.ReadUInt(s);
            unk02a = (byte)s.ReadByte();
            primType = (PrimType)s.ReadByte();
            unk02b = (byte)s.ReadByte();
            unk02c = (byte)s.ReadByte();
            unk03 = Helpers.ReadUInt(s);
            unk04 = Helpers.ReadUInt(s);
            vertDesc = new VertexDescriptor[16];
            for (int i = 0; i < 16; i++)
                vertDesc[i] = new VertexDescriptor(s);
            vertexStride = Helpers.ReadULong(s);
            unk06 = Helpers.ReadULong(s);
            unk07 = Helpers.ReadUInt(s);
            unk08 = new float[6];
            for (int i = 0; i < 6; i++)
                unk08[i] = Helpers.ReadFloat(s);
            unk09 = new ulong[6];
            for (int i = 0; i < 6; i++)
                unk09[i] = Helpers.ReadULong(s);

            //added
            long position = s.Position;
            boneIndices = new ushort[unk02c];
            s.Seek(unk03, SeekOrigin.Begin);
            for (int i = 0; i < unk02c; i++)
            {
                boneIndices[i] = Helpers.ReadUShort(s);
            }
            s.Seek(position, SeekOrigin.Begin);
            // end added
        }

        public void LoadVertexData(Stream s, MeshLOD lod)
        {
            s.Seek(vertOffset, 0);
            vertices = new List<Vertex>();
            for (int i = 0; i < vertCount; i++)
                vertices.Add(new Vertex(s, vertDesc, (int)vertexStride));
            s.Seek(lod.vertexDataSize + indStart * 2, 0);
            indicies = new List<ushort>();
            for (int i = 0; i < triCount * 3; i++)
                indicies.Add(Helpers.ReadUShort(s));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("->Section " + index);
            sb.AppendLine("--Unknown01           : 0x" + unk01.ToString("X"));
            sb.AppendLine("--Material Name       : " + matName);
            sb.AppendLine("--Material Index      : " + matIndex);
            sb.AppendLine("--Triangle Count      : " + triCount);
            sb.AppendLine("--Indicies Start      : " + indStart);
            sb.AppendLine("--Vertices Offset     : 0x" + vertOffset.ToString("X"));
            sb.AppendLine("--Vertices Count      : " + vertCount);
            sb.AppendLine("--Unknown02a          : 0x" + unk02a.ToString("X"));
            sb.AppendLine("--Primitive Type      : " + primType);
            sb.AppendLine("--Unknown02b          : 0x" + unk02b.ToString("X"));
            sb.AppendLine("--Unknown02c          : 0x" + unk02c.ToString("X"));
            sb.AppendLine("--Unknown03           : 0x" + unk03.ToString("X"));
            sb.AppendLine("--Unknown04           : 0x" + unk04.ToString("X"));
            sb.Append("--Vertex Descriptors  : ");
            foreach (VertexDescriptor desc in vertDesc)
                if (desc.offset != 0xFF)
                    sb.Append(desc.ToString());
            sb.AppendLine();
            sb.AppendLine("--Unknown05           : 0x" + vertexStride.ToString("X"));
            sb.AppendLine("--Unknown06           : 0x" + unk06.ToString("X"));
            sb.AppendLine("--Unknown07           : 0x" + unk07.ToString("X"));
            sb.Append("--Unknown08           : ");
            foreach (float f in unk08)
                sb.Append(f + " ");
            sb.AppendLine();
            sb.Append("--Unknown09           : ");
            foreach (ulong u in unk09)
                sb.Append("0x" + u.ToString("X") + " ");
            sb.AppendLine();
            return sb.ToString();
        }
    }

    public class VertexDescriptor
    {
        public ushort type;
        public byte offset;
        public byte unk01;
        public VertexDescriptor(Stream s)
        {
            type = Helpers.ReadUShort(s);
            offset = (byte)s.ReadByte();
            unk01 = (byte)s.ReadByte();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("(0x{0} 0x{1} 0x{2}) ", type.ToString("X"), offset.ToString("X"), unk01.ToString("X"));
            return sb.ToString();
        }
    }

    public class Vertex
    {
        public Vector biTangents = new Vector();
        public int[] boneIndices;
        public float[] boneWeights;
        public Vector normals = new Vector();
        public Vector position = new Vector();
        public Vector tangents = new Vector();
        public Vector texCoords = new Vector();

        public Vertex(Stream s, VertexDescriptor[] desc, int stride)
        {
            long pos = s.Position;
            foreach (VertexDescriptor d in desc)
                if (d.offset != 0xFF)
                {
                    s.Seek(pos + d.offset, SeekOrigin.Begin);
                    switch (d.type)
                    {
                        case 0x301:
                            position = new Vector(Helpers.ReadFloat(s), Helpers.ReadFloat(s), Helpers.ReadFloat(s));
                            break;
                        case 0x701:
                            position = new Vector(HalfUtils.Unpack(Helpers.ReadUShort(s)), HalfUtils.Unpack(Helpers.ReadUShort(s)), HalfUtils.Unpack(Helpers.ReadUShort(s)));
                            break;
                        case 0x806:
                            normals = new Vector(HalfUtils.Unpack(Helpers.ReadUShort(s)), HalfUtils.Unpack(Helpers.ReadUShort(s)), HalfUtils.Unpack(Helpers.ReadUShort(s)), HalfUtils.Unpack(Helpers.ReadUShort(s)));
                            break;
                        case 0x807:
                            tangents = new Vector(HalfUtils.Unpack(Helpers.ReadUShort(s)), HalfUtils.Unpack(Helpers.ReadUShort(s)), HalfUtils.Unpack(Helpers.ReadUShort(s)), HalfUtils.Unpack(Helpers.ReadUShort(s)));
                            break;
                        case 0x808:
                            biTangents = new Vector(HalfUtils.Unpack(Helpers.ReadUShort(s)), HalfUtils.Unpack(Helpers.ReadUShort(s)), HalfUtils.Unpack(Helpers.ReadUShort(s)), HalfUtils.Unpack(Helpers.ReadUShort(s)));
                            break;
                        case 0x621:
                            texCoords = new Vector(HalfUtils.Unpack(Helpers.ReadUShort(s)), HalfUtils.Unpack(Helpers.ReadUShort(s)));
                            break;
                        case 0xd04:
                            boneWeights = new float[4];
                            for (int k = 0; k < 4; k++)
                                boneWeights[k] = ((float)s.ReadByte()) / 255f;
                            break;
                        case 0xc02:
                            boneIndices = new int[4];
                            for (int m = 0; m < 4; m++)
                                boneIndices[m] = s.ReadByte();
                            break;
                        case 0x509:
                            s.Seek(2, SeekOrigin.Current);
                            break;
                        case 0xd1e:
                        case 0xd1f:
                        case 0x622:
                        case 0x623:
                        case 0x624:
                        case 0xc03:
                        case 0xd05:
                            s.Seek(4, SeekOrigin.Current);
                            break;
                    }
                }
            s.Seek(pos + stride, 0);
        }
    }

    public class VertexGroup
    {
        public struct VertexWeight
        {
            public int VertexIndex;
            public float Weight;

            public VertexWeight(int v, float w)
            {
                VertexIndex = v;
                Weight = w;
            }
        }

        public static Dictionary<string, List<VertexWeight>> GetVertexGroups(FBSkeleton skeleton, MeshLodSection section)
        {
            var VertexGroups = new Dictionary<string, List<VertexWeight>>();
            GetVertexGroups(skeleton, section, 0, ref VertexGroups);
            return VertexGroups;
        }

        private static int GetVertexGroups(FBSkeleton skeleton, MeshLodSection section, int offset, ref Dictionary<string, List<VertexWeight>> VertexGroupDict)
        {
            int VertexCount = offset;
            for (int i = 0; i < section.vertices.Count; i++)
            {
                for (int x = 0; x < 4; x++)
                {
                    float Weight = section.vertices[i].boneWeights[x];

                    // added condition to only add meaningful weights
                    if (Weight != 0.0f)
                    {
                        int BoneIndex = section.vertices[i].boneIndices[x];
                        int SubObjectBoneIndex = section.boneIndices[BoneIndex];

                        string bn = skeleton.Bones[SubObjectBoneIndex].Name;

                        if (!VertexGroupDict.ContainsKey(bn))
                        {
                            VertexGroupDict.Add(bn, new List<VertexWeight>());
                        }
                        VertexGroupDict[bn].Add(new VertexWeight(offset + i, Weight));
                    }
                }
                VertexCount++;
            }
            return VertexCount;
        }

        public static Dictionary<string, List<VertexWeight>> GetVertexGroups(FBSkeleton skeleton, MeshLOD lod)
        {
            var VertexGroupDict = new Dictionary<string, List<VertexWeight>>();
            int VertCount = 0;
            foreach (var section in lod.sections)
            {
                VertCount = GetVertexGroups(skeleton, section, VertCount, ref VertexGroupDict);
            }
            return VertexGroupDict;
        }
    }
}
