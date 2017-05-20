using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PluginSystem;
using Be.Windows.Forms;
using SharpDX.Mathematics.Interop;

namespace PluginMeshesWV
{
    public partial class MainForm : Form
    {
        public MainClass main;
        public List<string> tocFiles;
        public List<string> tocLabels;
        public Dictionary<string, List<string>> bundlePaths;
        public Dictionary<string, List<ChunkInfo>> tocChunks;
        public string currBundle;
        public List<DataInfo> ebx = new List<DataInfo>();
        public List<DataInfo> res = new List<DataInfo>();
        public List<ChunkInfo> chunks = new List<ChunkInfo>();
        public List<ChunkInfo> globalChunks = new List<ChunkInfo>();
        public List<uint> meshAssetTypes = new List<uint>(new uint[] { 0x49b156d4 });

        public string currPath = "";
        public string currToc = "";
        public byte[] rawEbxBuffer;
        public byte[] rawResBuffer;
        public byte[] rawLodBuffer;
        public EBX ebxObject;
        public Mesh mesh;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            tocLabels = main.Host.getTOCFileLabels();
            tocFiles = main.Host.getTOCFiles();
            globalChunks = new List<ChunkInfo>();
            foreach (string toc in tocFiles)
                if (toc.Contains("chunks"))
                    globalChunks.AddRange(main.Host.getAllTocCHUNKs(toc));
            bundlePaths = new Dictionary<string, List<string>>();
            tocChunks = new Dictionary<string, List<ChunkInfo>>();
            foreach (string toc in tocFiles)
            {
                bundlePaths.Add(toc, main.Host.getBundleNames(toc));
                tocChunks.Add(toc, main.Host.getAllTocCHUNKs(toc));
            }
            RefreshStuff();
            DXHelper.Init(pic1);
            timer1.Enabled = true;
        }

        public void RefreshStuff()
        {
            tv1.Nodes.Clear();
            TreeNode t = new TreeNode("ROOT");
            foreach (string toc in tocFiles)
                foreach (string bundle in bundlePaths[toc])
                    Helpers.AddPath(t, bundle);
            t.Expand();
            if (t.Nodes.Count != 0)
                t.Nodes[0].Expand();
            tv1.Nodes.Add(t);
        }

        public void RefreshTextures()
        {
            tv2.Nodes.Clear();
            TreeNode t = new TreeNode("ROOT");
            foreach (DataInfo info in res)
                if (meshAssetTypes.Contains((uint)info.type))
                    Helpers.AddPath(t, info.path);
            t.ExpandAll();
            tv2.Nodes.Add(t);
        }

        private void tv1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode sel = tv1.SelectedNode;
            if (sel == null)
                return;
            currBundle = Helpers.GetPathFromNode(sel);
            if (currBundle.Length > 5)
                currBundle = currBundle.Substring(5);
            chunks = new List<ChunkInfo>();
            chunks.AddRange(globalChunks);
            foreach (KeyValuePair<string, List<string>> pair in bundlePaths)
                if (pair.Value.Contains(currBundle))
                {
                    currToc = pair.Key;
                    res = main.Host.getAllRES(pair.Key, currBundle);
                    ebx = main.Host.getAllEBX(pair.Key, currBundle);
                    chunks.AddRange(main.Host.getAllBundleCHUNKs(pair.Key, currBundle));
                    RefreshTextures();
                    return;
                }
        }

        private void tv2_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode sel = tv2.SelectedNode;
            if (sel == null)
                return;
            string path = Helpers.GetPathFromNode(sel);
            if(path.Length > 5)
                path = path.Substring(5);
            foreach (DataInfo resInfo in res)
                if (resInfo.path == path)
                    foreach (DataInfo ebxInfo in ebx)
                        if (ebxInfo.path == path)
                        {                         
                            LoadMesh(resInfo.sha1, ebxInfo.sha1, path);
                            return;
                        }
        }

        public void LoadMesh(byte[] sha1res, byte[] sha1ebx, string path)
        {
            rtb2.Text = "";
            currPath = path;
            tv3.Nodes.Clear();
            rawResBuffer = main.Host.getDataBySha1(sha1res);
            rawEbxBuffer = main.Host.getDataBySha1(sha1ebx);
            if (rawEbxBuffer == null || rawResBuffer == null)
                return;
            try
            {
                hb1.ByteProvider = new DynamicByteProvider(rawResBuffer);
                hb2.ByteProvider = new DynamicByteProvider(new byte[0]);
                mesh = new Mesh(new MemoryStream(rawResBuffer));
                rtb2.Text = mesh.ToString();
                toolStripComboBox1.Items.Clear();
                for (int i = 0; i < mesh.lods.Count; i++)
                    toolStripComboBox1.Items.Add("LOD " + i);
                if (mesh.lods.Count > 0)
                    toolStripComboBox1.SelectedIndex = 0;
                ebxObject = new EBX(new MemoryStream(rawEbxBuffer));
                tv3.Nodes.Add(ebxObject.ToNode());
            }
            catch (Exception ex)
            {
                rtb2.Text = "ERROR!!!:\n" + ex.Message + "\n\n" + rtb2.Text;
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Helpers.SelectNext(toolStripTextBox1.Text, tv1);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Helpers.SelectNext(toolStripTextBox2.Text, tv2);
        }

        private void exportRawBufferToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (rawResBuffer != null && rawResBuffer.Length != 0)
            {
                SaveFileDialog d = new SaveFileDialog();
                d.Filter = "*.bin|*.bin";
                d.FileName = Path.GetFileName(currPath) + ".raw.bin";
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.WriteAllBytes(d.FileName, rawResBuffer);
                    MessageBox.Show("Done.");
                }
            }
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = toolStripComboBox1.SelectedIndex;
            byte[] id = mesh.lods[n].chunkID;
            string sid = Helpers.ByteArrayToHexString(id);
            rawLodBuffer = null;
            hb2.ByteProvider = new DynamicByteProvider(new byte[0]);
            foreach (ChunkInfo chunk in chunks)
                if (chunk.id == sid)
                {
                    rawLodBuffer = main.Host.getDataBySha1(chunk.sha1);
                    hb2.ByteProvider = new DynamicByteProvider(rawLodBuffer);
                    break;
                }
            if (rawLodBuffer != null)
            {
                mesh.lods[n].LoadVertexData(new MemoryStream(rawLodBuffer));
                timer1.Enabled = false;
                List<RawVector3> verts = new List<RawVector3>();
                for (int i = 0; i < mesh.lods[n].sections.Count; i++)
                {
                    MeshLodSection sec = mesh.lods[n].sections[i];
                    foreach (ushort idx in sec.indicies)
                    {
                        Vertex v = sec.vertices[idx];
                        if (v.position.members.Length == 3)
                            verts.Add(new RawVector3(v.position.members[0], v.position.members[1], v.position.members[2]));
                    }
                }
                RawVector3 min = new RawVector3(10000, 10000, 10000);
                RawVector3 max = new RawVector3(-10000, -10000, -10000);
                foreach (RawVector3 v in verts)
                {
                    if (v.X < min.X)
                        min.X = v.X;
                    if (v.Y < min.Y)
                        min.Y = v.Y;
                    if (v.Z < min.Z)
                        min.Z = v.Z;
                    if (v.X > max.X)
                        max.X = v.X;
                    if (v.Y > max.Y)
                        max.Y = v.Y;
                    if (v.Z > max.Z)
                        max.Z = v.Z;
                }
                RawVector3 mid = new RawVector3((min.X + max.X) / 2f, (min.Y + max.Y) / 2f, (min.Z + max.Z) / 2f);
                DXHelper.CamDis = (float)Math.Sqrt(Math.Pow(max.X - mid.X, 2) + Math.Pow(max.Y - mid.Y, 2) + Math.Pow(max.Z - mid.Z, 2)) * 1.5f;
                if (DXHelper.CamDis < 1f)
                    DXHelper.CamDis = 1f;
                for (int i = 0; i < verts.Count; i++)
                {
                    RawVector3 v = verts[i];
                    v.X -= mid.X;
                    v.Y -= mid.Y;
                    v.Z -= mid.Z;
                    verts[i] = v;
                }
                DXHelper.vertices = verts.ToArray();
                DXHelper.InitGeometry();
                timer1.Enabled = true;
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (tv1.SelectedNode == null)
                tv1.Nodes[0].ExpandAll();
            else
                tv1.SelectedNode.ExpandAll();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (tv2.SelectedNode == null)
                tv2.Nodes[0].ExpandAll();
            else
                tv2.SelectedNode.ExpandAll();
        }

        private void exportRawMeshBufferToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (rawLodBuffer != null && rawLodBuffer.Length != 0)
            {
                SaveFileDialog d = new SaveFileDialog();
                d.Filter = "*.bin|*.bin";
                d.FileName = Path.GetFileName(currPath) + ".raw.LOD.bin";
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.WriteAllBytes(d.FileName, rawLodBuffer);
                    MessageBox.Show("Done.");
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                DXHelper.CamRot += 0.01f;
                DXHelper.Render();
            }
            catch { }
        }

        private void pic1_SizeChanged(object sender, EventArgs e)
        {
            DXHelper.Resize(pic1);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DXHelper.Cleanup();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            
            int n = toolStripComboBox1.SelectedIndex;
            byte[] id = mesh.lods[n].chunkID;
            string sid = Helpers.ByteArrayToHexString(id);
            rawLodBuffer = null;
            hb2.ByteProvider = new DynamicByteProvider(new byte[0]);
            foreach (ChunkInfo chunk in chunks)
                if (chunk.id == sid)
                {
                    rawLodBuffer = main.Host.getDataBySha1(chunk.sha1);
                    hb2.ByteProvider = new DynamicByteProvider(rawLodBuffer);
                    break;
                }
            if (rawLodBuffer == null)
                foreach (ChunkInfo chunk in tocChunks[currToc])
                    if (chunk.id == sid)
                    {
                        rawLodBuffer = main.Host.getDataBySha1(chunk.sha1);
                        hb2.ByteProvider = new DynamicByteProvider(rawLodBuffer);
                        return;
                    }
            if (rawLodBuffer != null)
            {
                mesh.lods[n].LoadVertexData(new MemoryStream(rawLodBuffer));
                SaveFileDialog d = new SaveFileDialog();
                d.Filter = "*.obj|*.obj";
                d.FileName = mesh.header.shortName + ".obj";
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        for (int i = 0; i < mesh.lods[n].sections.Count(); i++)
                        {
                            StringBuilder sb = new StringBuilder();
                            MeshLodSection sec = mesh.lods[n].sections[i];
                            if (sec.primType != PrimType.PrimitiveType_TriangleList)
                                continue;
                            bool hasTex = sec.vertices[0].texCoords.members.Length == 2;
                            bool hasNorm = sec.vertices[0].normals.members.Length == 2;
                            for (int j = 0; j < sec.vertCount; j++)
                                sb.AppendLine("v " + sec.vertices[j].position.members[0] + " " + sec.vertices[j].position.members[1] + " " + sec.vertices[j].position.members[2]);
                            if (hasTex)
                                for (int j = 0; j < sec.vertCount; j++)
                                    sb.AppendLine("vt " + sec.vertices[j].texCoords.members[0] + " " + sec.vertices[j].position.members[1]);
                            if (hasNorm)
                                for (int j = 0; j < sec.vertCount; j++)
                                    sb.AppendLine("vn " + sec.vertices[j].normals.members[0] + " " + sec.vertices[j].normals.members[1] + " " + sec.vertices[j].normals.members[2]);
                            string s = sb.ToString().Replace(",", ".");
                            sb = new StringBuilder();
                            sb.Append(s);
                            ushort u;
                            for (int j = 0; j < sec.triCount; j++)
                            {
                                sb.Append("f ");
                                for (int k = 0; k < 3; k++)
                                {
                                    u = (ushort)(sec.indicies[j * 3 + k] + 1);
                                    sb.Append(u + "/" + (hasTex ? u.ToString() : "") + "/" + (hasNorm ? u.ToString() : "") + " ");
                                }
                                sb.AppendLine();
                            }
                            string filename;
                            if (i == 0)
                                filename = d.FileName;
                            else
                                filename = Path.GetDirectoryName(d.FileName) + "\\" + Path.GetFileNameWithoutExtension(d.FileName) + ".sec" + i.ToString("D2") + ".obj";
                            if (File.Exists(filename))
                                File.Delete(filename);
                            File.WriteAllText(filename, sb.ToString());
                        }
                    }
                    catch { }
                    MessageBox.Show("Done.");
                }
            }
        }

        private void CollapseAll(TreeNode t)
        {
            foreach (TreeNode t2 in t.Nodes)
                CollapseAll(t2);
            t.Collapse();
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            if (tv1.SelectedNode == null)
                CollapseAll(tv1.Nodes[0]);
            else
                CollapseAll(tv1.SelectedNode);
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            if (tv2.SelectedNode == null)
                CollapseAll(tv2.Nodes[0]);
            else
                CollapseAll(tv2.SelectedNode);
        }

        private void copyNodePathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tv1.SelectedNode != null)
                Clipboard.SetText(Helpers.GetPathFromNode(tv1.SelectedNode));
        }

        private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
        {
            if (tv2.SelectedNode != null)
                Clipboard.SetText(Helpers.GetPathFromNode(tv2.SelectedNode));
        }
    }
}
