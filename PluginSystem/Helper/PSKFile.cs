using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginSystem
{
    public class PSKFile
    {
        public struct PSKPoint
        {
            public float x;
            public float y;
            public float z;

            public PSKPoint(float _x, float _y, float _z)
            {
                x = _x;
                y = _y;
                z = _z;
            }

            public PSKPoint(Vector v)
            {
                x = v.members[0];
                y = v.members[1];
                z = v.members[2];
            }
        }
        public struct PSKQuad
        {
            public float w;
            public float x;
            public float y;
            public float z;

            public PSKQuad(float _w, float _x, float _y, float _z)
            {
                w = _w;
                x = _x;
                y = _y;
                z = _z;
            }

            public PSKQuad(Vector v)
            {
                x = v.members[0];
                y = v.members[1];
                z = v.members[2];
                w = v.members[3];
            }
        }
        public struct PSKEdge
        {
            public UInt16 index;
            public UInt16 padding1;
            public float U;
            public float V;
            public byte material;
            public byte reserved;
            public UInt16 padding2;

            public PSKEdge(UInt16 _index, float _U, float _V, byte _material)
            {
                index = _index;
                padding1 = 0;
                U = _U;
                V = _V;
                material = _material;
                reserved = 0;
                padding2 = 0;
            }

            public PSKEdge(UInt16 _index, Vector _UV, byte _material)
            {
                index = _index;
                padding1 = 0;
                U = _UV.members[0];
                V = _UV.members[1];
                material = _material;
                reserved = 0;
                padding2 = 0;
            }
        }
        public struct PSKFace
        {
            public int v0;
            public int v1;
            public int v2;
            public byte material;
            public byte auxmaterial;
            public int smoothgroup;

            public PSKFace(int _v0, int _v1, int _v2, byte _material)
            {
                v0 = _v0;
                v1 = _v1;
                v2 = _v2;
                material = _material;
                auxmaterial = 0;
                smoothgroup = 0;
            }
        }
        public struct PSKMaterial
        {
            public string name;
            public int texture;
            public int polyflags;
            public int auxmaterial;
            public int auxflags;
            public int LODbias;
            public int LODstyle;

            public PSKMaterial(string _name, int _texture)
            {
                name = _name;
                texture = _texture;
                polyflags = 0;
                auxmaterial = 0;
                auxflags = 0;
                LODbias = 0;
                LODstyle = 0;
            }
        }
        public struct PSKBone
        {
            public string name;
            public int flags;
            public int childs;
            public int parent;
            public PSKQuad rotation;
            public PSKPoint location;
            public float length;
            public PSKPoint size;
            public int index;
        }
        public struct PSKWeight
        {
            public float weight;
            public int point;
            public int bone;

            public PSKWeight(float _weight, int _point, int _bone)
            {
                weight = _weight;
                point = _point;
                bone = _bone;
            }
        }
        public struct PSKExtraUV
        {
            public float U;
            public float V;

            public PSKExtraUV(float _U, float _V)
            {
                U = _U;
                V = _V;
            }
        }
        public struct PSKHeader
        {
            public string name;
            public int flags;
            public int size;
            public int count;
        }

        public List<PSKPoint> points;
        public List<PSKEdge> edges;
        public List<PSKFace> faces;
        public List<PSKMaterial> materials;
        public List<PSKBone> bones;
        public List<PSKWeight> weights;

        public PSKFile()
        {
        }

        public PSKFile(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            while (fs.Position < fs.Length)
            {
                PSKHeader h = ReadHeader(fs);
                switch (h.name)
                {
                    case "PNTS0000":
                        {
                            points = new List<PSKPoint>();
                            for (int i = 0; i < h.count; i++)
                            {
                                PSKPoint pskPoint = new PSKPoint();
                                pskPoint.x = Helpers.ReadFloat(fs);
                                pskPoint.z = Helpers.ReadFloat(fs);
                                pskPoint.y = Helpers.ReadFloat(fs);
                                points.Add(pskPoint);
                            }
                        }; break;
                    case "VTXW0000":
                        {
                            edges = new List<PSKEdge>();
                            for (int i = 0; i < h.count; i++)
                            {
                                PSKEdge pskEdge = new PSKEdge();
                                pskEdge.index = Helpers.ReadUShort(fs);
                                Helpers.ReadUShort(fs);
                                pskEdge.U = Helpers.ReadFloat(fs);
                                pskEdge.V = Helpers.ReadFloat(fs);
                                pskEdge.material = (byte)fs.ReadByte();
                                fs.ReadByte();
                                Helpers.ReadUShort(fs);
                                edges.Add(pskEdge);
                            }
                        }; break;
                    case "FACE0000":
                        {
                            faces = new List<PSKFace>();
                            for (int i = 0; i < h.count; i++)
                            {
                                PSKFace pskFace = new PSKFace(Helpers.ReadUShort(fs), Helpers.ReadUShort(fs), Helpers.ReadUShort(fs), (byte)fs.ReadByte());
                                fs.ReadByte();
                                Helpers.ReadInt(fs);
                                faces.Add(pskFace);
                            }
                        }; break;

                    case "MATT0000":
                        {
                            materials = new List<PSKMaterial>();
                            for (int i = 0; i < h.count; i++)
                            {
                                PSKMaterial pskMaterial = new PSKMaterial();
                                pskMaterial.name = ReadFixedString(fs, 64);
                                pskMaterial.texture = Helpers.ReadInt(fs);
                                Helpers.ReadInt(fs);
                                Helpers.ReadInt(fs);
                                Helpers.ReadInt(fs);
                                Helpers.ReadInt(fs);
                                Helpers.ReadInt(fs);
                                materials.Add(pskMaterial);
                            }
                        }; break;
                    case "REFSKELT":
                        {
                            bones = new List<PSKBone>();
                            for (int i = 0; i < h.count; i++)
                            {
                                PSKBone b = new PSKBone();
                                b.name = ReadFixedString(fs, 64);
                                Helpers.ReadInt(fs);
                                b.childs = Helpers.ReadInt(fs);
                                b.parent = Helpers.ReadInt(fs);
                                b.rotation.x = Helpers.ReadFloat(fs);
                                b.rotation.z = Helpers.ReadFloat(fs);
                                b.rotation.y = Helpers.ReadFloat(fs);
                                b.rotation.w = Helpers.ReadFloat(fs);
                                b.location.x = Helpers.ReadFloat(fs);
                                b.location.z = Helpers.ReadFloat(fs);
                                b.location.y = Helpers.ReadFloat(fs);
                                Helpers.ReadInt(fs);
                                Helpers.ReadInt(fs);
                                Helpers.ReadInt(fs);
                                Helpers.ReadInt(fs);
                                bones.Add(b);
                            }
                        }; break;
                    case "RAWWEIGHTS":
                        {
                            weights = new List<PSKWeight>();
                            for (int i = 0; i < h.count; i++)
                            {
                                PSKWeight w = new PSKWeight(Helpers.ReadFloat(fs), Helpers.ReadInt(fs), Helpers.ReadInt(fs));
                                weights.Add(w);
                            }
                        }; break;
                    default:
                        fs.Seek(h.size * h.count, SeekOrigin.Current);
                        break;
                }
            }
            fs.Close();
        }

        public MemoryStream SaveToMemory()
        {
            MemoryStream ms = new MemoryStream();
            Save(ms);

            return ms;
        }

        public void Save(String filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            Save(fs);
            fs.Close();
        }

        public void Save(Stream fs)
        {
            //FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            WriteHeader(fs, "ACTRHEAD", 0x1E83B9, 0, 0);
            WriteHeader(fs, "PNTS0000", 0x1E83B9, 0xC, points.Count);
            foreach (PSKPoint p in points)
            {
                Helpers.WriteFloat(fs, p.x);
                Helpers.WriteFloat(fs, p.z);
                Helpers.WriteFloat(fs, p.y);
            }
            WriteHeader(fs, "VTXW0000", 0x1E83B9, 0x10, edges.Count);
            foreach (PSKEdge e in edges)
            {
                Helpers.WriteUShort(fs, e.index);
                Helpers.WriteUShort(fs, 0);
                Helpers.WriteFloat(fs, e.U);
                Helpers.WriteFloat(fs, e.V);
                fs.WriteByte(e.material);
                fs.WriteByte(0);
                Helpers.WriteUShort(fs, 0);
            }
            WriteHeader(fs, "FACE0000", 0x1E83B9, 0xC, faces.Count);
            foreach (PSKFace f in faces)
            {
                Helpers.WriteUShort(fs, (ushort)f.v0);
                Helpers.WriteUShort(fs, (ushort)f.v1);
                Helpers.WriteUShort(fs, (ushort)f.v2);
                fs.WriteByte(f.material);
                fs.WriteByte(0);
                Helpers.WriteInt(fs, 0);
            }
            WriteHeader(fs, "MATT0000", 0x1E83B9, 0x58, materials.Count);
            foreach (PSKMaterial m in materials)
            {
                WriteFixedString(fs, m.name, 64);
                Helpers.WriteInt(fs, m.texture);
                Helpers.WriteInt(fs, 0);
                Helpers.WriteInt(fs, 0);
                Helpers.WriteInt(fs, 0);
                Helpers.WriteInt(fs, 0);
                Helpers.WriteInt(fs, 0);
            }
            WriteHeader(fs, "REFSKELT", 0x1E83B9, 0x78, bones.Count);
            foreach (PSKBone b in bones)
            {
                WriteFixedString(fs, b.name, 64);
                Helpers.WriteInt(fs, 0);
                Helpers.WriteInt(fs, b.childs);
                Helpers.WriteInt(fs, b.parent);
                Helpers.WriteFloat(fs, b.rotation.x);
                Helpers.WriteFloat(fs, b.rotation.z);
                Helpers.WriteFloat(fs, b.rotation.y);
                Helpers.WriteFloat(fs, b.rotation.w);
                Helpers.WriteFloat(fs, b.location.x);
                Helpers.WriteFloat(fs, b.location.z);
                Helpers.WriteFloat(fs, b.location.y);
                Helpers.WriteInt(fs, 0);
                Helpers.WriteInt(fs, 0);
                Helpers.WriteInt(fs, 0);
                Helpers.WriteInt(fs, 0);
            }
            WriteHeader(fs, "RAWWEIGHTS", 0x1E83B9, 0xC, weights.Count);
            foreach (PSKWeight w in weights)
            {
                Helpers.WriteFloat(fs, w.weight);
                Helpers.WriteInt(fs, w.point);
                Helpers.WriteInt(fs, w.bone);
            }
            //fs.Close();
        }

        private PSKHeader ReadHeader(FileStream fs)
        {
            PSKHeader res = new PSKHeader();
            res.name = ReadFixedString(fs, 20);
            res.flags = Helpers.ReadInt(fs);
            res.size = Helpers.ReadInt(fs);
            res.count = Helpers.ReadInt(fs);
            return res;
        }

        private void WriteHeader(Stream fs, string name, int flags, int size, int count)
        {
            WriteFixedString(fs, name, 20);
            Helpers.WriteInt(fs, flags);
            Helpers.WriteInt(fs, size);
            Helpers.WriteInt(fs, count);
        }        

        public string ReadFixedString(Stream fs, int len)
        {
            string s = "";
            byte b;
            for (int i = 0; i < len; i++)
                if ((b = (byte)fs.ReadByte()) != 0)
                    s += (char)b;
            return s;
        }        

        public void WriteFixedString(Stream fs, string s, int len)
        {
            for (int i = 0; i < len; i++)
                if (i < s.Length)
                    fs.WriteByte((byte)s[i]);
                else
                    fs.WriteByte(0);
        }
    }
}
