using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginSystem
{
    public class OBJExporter : IMeshExporter
    {
        public void ExportLod(MeshAsset mesh, int lodIndex, string targetFile)
        {
            byte[] data = ExportAsObj(mesh, mesh.lods[lodIndex]);
            File.WriteAllBytes(targetFile, data);
        }
        public void ExportAllLods(MeshAsset mesh, string targetdir)
        {
            foreach (MeshLOD lod in mesh.lods)
            {
                string targetFile = Path.Combine(targetdir, lod.shortName + ".obj");
                byte[] data = ExportAsObj(mesh, lod);
                File.WriteAllBytes(targetFile, data);
            }
        }


        ///
        private byte[] ExportAsObj(MeshAsset mesh, MeshLOD lod)
        {
            string[] subMeshNames = new string[lod.sections.Count];
            float[][] verts = new float[lod.sections.Count][];
            float[][] uvcords = new float[lod.sections.Count][];
            ushort[][] indices = new ushort[lod.sections.Count][];

            for (int i = 0; i < lod.sections.Count; i++)
            {
                subMeshNames[i] = lod.sections[i].matName;
                verts[i] = GetVerticesPositionsArray(lod.sections[i].vertices);
                uvcords[i] = GetUVCoordsArray(lod.sections[i].vertices);
                indices[i] = lod.sections[i].indicies.ToArray();
            }

            return convertToOBJ(mesh.header.shortName, subMeshNames, verts, uvcords, indices);
        }

        private static float[] GetVerticesPositionsArray(List<Vertex> vertices)
        {
            float[] verts = new float[vertices.Count * 3];
            int index = 0;
            foreach (Vertex v in vertices)
            {
                if (v.position.members.Length == 3)
                {
                    verts[index] = v.position.members[0];
                    verts[index + 1] = v.position.members[1];
                    verts[index + 2] = v.position.members[2];
                }
                index = index + 3;
            }
            return verts;
        }

        private static float[] GetUVCoordsArray(List<Vertex> vertices)
        {
            float[] verts = new float[vertices.Count * 2];
            int index = 0;
            foreach (Vertex v in vertices)
            {
                if (v.position.members.Length == 2)
                {
                    verts[index] = v.texCoords.members[0];
                    verts[index + 1] = v.texCoords.members[1];
                }
                index = index + 2;
            }
            return verts;
        }

        private byte[] convertToOBJ(String modelName, String[] subMeshName, float[][] verts, float[][] uvcords, ushort[][] indices)
        {
            List<byte> objFile = new List<byte>();
            foreach (byte b in (Encoding.UTF8.GetBytes((string)"o " + modelName + "\ns off\n")))
            {
                objFile.Add(b);
            }//object name and disable smoothing!

            int currentVertexCount = 1;//index starts not at 0 ;)
            for (int i = 0; i < subMeshName.Length; i++)
            {   //for each Submesh
                float[] subVert = verts[i];
                float[] subUVs = uvcords[i];
                ushort[] subIndices = indices[i];

                //subMeshName as group
                foreach (byte b in (Encoding.UTF8.GetBytes((String)"g " + subMeshName[i] + "\n")))
                {
                    objFile.Add(b);
                }

                //vert - "v 1.000000 -1.000000 -1.000000"
                for (int fi = 0; fi < subVert.Length; fi++)
                {
                    if (fi % 3 == 0)
                    {
                        foreach (byte b in (Encoding.UTF8.GetBytes((String)"v")))
                        {
                            objFile.Add(b);
                        }
                    }
                    foreach (byte b in (Encoding.UTF8.GetBytes((String)" " + subVert[fi].ToString())))
                    {
                        objFile.Add(b);
                    }
                    if (fi % 3 == 2)
                    {
                        foreach (byte b in (Encoding.UTF8.GetBytes((String)"\n")))
                        {
                            objFile.Add(b);
                        }
                    }
                }
                //uvs - "vt 0.500000 0.500000"
                for (int fi = 0; fi < subUVs.Length; fi++)
                {
                    if (fi % 2 == 0)
                    {
                        foreach (byte b in (Encoding.UTF8.GetBytes((String)"vt")))
                        {
                            objFile.Add(b);
                        }
                    }
                    foreach (byte b in (Encoding.UTF8.GetBytes((String)" " + subUVs[fi].ToString())))
                    {
                        objFile.Add(b);
                    }
                    if (fi % 2 == 1)
                    {
                        foreach (byte b in (Encoding.UTF8.GetBytes((String)"\n")))
                        {
                            objFile.Add(b);
                        }
                    }
                }

                //indices
                for (int fi = 0; fi < subIndices.Length; fi++)
                {
                    if (fi % 3 == 0)
                    {
                        foreach (byte b in (Encoding.UTF8.GetBytes((String)"f")))
                        {
                            objFile.Add(b);
                        }
                    }
                    String s = (currentVertexCount + subIndices[fi]).ToString();
                    foreach (byte b in (Encoding.UTF8.GetBytes((String)" " + s + "/" + s)))
                    {
                        objFile.Add(b);
                    }
                    if (fi % 3 == 2)
                    {
                        foreach (byte b in (Encoding.UTF8.GetBytes((String)"\n")))
                        {
                            objFile.Add(b);
                        }
                    }
                }
                //Frostbite's mesh starts each submesh at index 0, but Wavefront_OBJ does use the total index!
                currentVertexCount += subVert.Length / 3;
            }
            return objFile.ToArray();
        }
    }
}
