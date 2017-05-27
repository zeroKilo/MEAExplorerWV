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

using SharpDX;
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
        public MeshAsset mesh;
        public RawVector3 mid = new RawVector3(0, 0, 0);

        public Dictionary<string, string> skeletons;
        public FBSkeleton skeleton = null;

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
            toolStripComboBox2.Items.Clear();
            toolStripComboBox2.Items.Add("None");
            toolStripComboBox2.Items.Add("From File...");
            toolStripComboBox2.SelectedIndex = 0;
            StringReader sr = new StringReader(Properties.Res.skeletonSHA1s);
            string line;
            skeletons = new Dictionary<string, string>();
            while ((line = sr.ReadLine()) != null)
            {
                string[] parts = line.Split(':');
                skeletons.Add(parts[0].Trim(), parts[1].Trim());
            }
            toolStripComboBox2.Items.AddRange(skeletons.Keys.ToArray());
            tabControl1.SelectedTab = tabPage3;
            this.ActiveControl = tv1;
        }

        public void LoadSpecific(DataInfo info)
        {
            string[] parts = info.bundle.Split('/');
            TreeNode curr = tv1.Nodes[0];
            for (int i = 0; i < parts.Length; i++)
            {
                bool found = false;
                foreach (TreeNode sub in curr.Nodes)
                    if (sub.Text == parts[i])
                    {
                        found = true;
                        curr = sub;
                        break;
                    }
                if (!found)
                    return;
            }
            tv1.SelectedNode = curr;
            parts = info.path.Replace(".res", "").Split('/');
            curr = tv2.Nodes[0];
            for (int i = 0; i < parts.Length; i++)
            {
                bool found = false;
                foreach (TreeNode sub in curr.Nodes)
                    if (sub.Text == parts[i])
                    {
                        found = true;
                        curr = sub;
                        break;
                    }
                if (!found)
                    return;
            }
            tv2.SelectedNode = curr;
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

        public void RefreshMeshes()
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
                    RefreshMeshes();
                    return;
                }
        }

        private void tv2_AfterSelect(object sender, TreeViewEventArgs e)
        {
            SelectMesh();
        }

        public void SelectMesh()
        {
            TreeNode sel = tv2.SelectedNode;
            if (sel == null)
                return;
            string path = Helpers.GetPathFromNode(sel);
            if (path.Length > 5)
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
                mesh = new MeshAsset(new MemoryStream(rawResBuffer));
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
                mid = new RawVector3((min.X + max.X) / 2f, (min.Y + max.Y) / 2f, (min.Z + max.Z) / 2f);
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
                RenderObject ro = new RenderObject(DXHelper.device, RenderObject.RenderType.TriListWire, DXHelper.pixelShader);
                ro.vertices = verts.ToArray();
                ro.InitGeometry();
                DXHelper.objects = new List<RenderObject>();
                DXHelper.objects.Add(ro);
                if (skeleton != null)
                {
                    RenderObject skel = MakeSkeletonMesh(skeleton);
                    for (int i = 0; i < skel.vertices.Length; i++)
                    {
                        skel.vertices[i].X -= mid.X;
                        skel.vertices[i].Y -= mid.Y;
                        skel.vertices[i].Z -= mid.Z;
                    }
                    skel.InitGeometry();
                    DXHelper.objects.Add(skel);
                }
                timer1.Enabled = true;
            }
        }

        private RenderObject MakeSkeletonMesh(FBSkeleton skel)
        {
            RenderObject ro = new RenderObject(DXHelper.device, RenderObject.RenderType.Lines, DXHelper.pixelShaderSel);
            List<RawVector3> verts = new List<RawVector3>();
            AddBoneToMesh(skel.RootBone, 
                          new Vector3(skel.RootBone.Location.members[0], skel.RootBone.Location.members[1], skel.RootBone.Location.members[2]), 
                          GetBoneMatrix(skel.RootBone), 
                          verts);
            ro.vertices = verts.ToArray();
            ro.InitGeometry();
            return ro;
        }

        private void AddBoneToMesh(FBBone parent, Vector3 ppos, Matrix pm, List<RawVector3> verts)
        {
            foreach (FBBone bone in parent.Children)
            {
                Vector3 pos = new Vector3(bone.Location.members[0], bone.Location.members[1], bone.Location.members[2]);
                Vector4 v = Vector3.Transform(pos, pm);
                pos.X = v.X + ppos.X;
                pos.Y = v.Y + ppos.Y;
                pos.Z = v.Z + ppos.Z;
                verts.Add(ppos);
                verts.Add(pos);
                Matrix m = GetBoneMatrix(bone);
                AddBoneToMesh(bone, pos, m * pm, verts);
            }
        }
        
        private Matrix GetBoneMatrix(FBBone bone)
        {
            return new Matrix(bone.Right.members[0], bone.Right.members[1], bone.Right.members[2], 0,
                                  bone.Up.members[0], bone.Up.members[1], bone.Up.members[2], 0,
                                  bone.Forward.members[0], bone.Forward.members[1], bone.Forward.members[2], 0,
                                  0, 0, 0, 1);
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
                SaveFileDialog saveFileDiag = new SaveFileDialog();
                saveFileDiag.Title = "Save as...";
                saveFileDiag.Filter = "*.obj|*.obj|*.psk|*.psk";//|*.fbx|*.fbx";
                saveFileDiag.FileName = mesh.header.shortName;
                if (saveFileDiag.ShowDialog() == DialogResult.OK)
                {
                    string extension = Path.GetExtension(saveFileDiag.FileName);
                    string targetFile = saveFileDiag.FileName;
                    var exporter = MeshExporter.GetExporterByExtension(extension, skeleton);
                    if (exporter != null)
                    {
                        exporter.ExportLod(mesh, n, targetFile);
                        MessageBox.Show("Done.");
                    }
                    else
                    {
                        MessageBox.Show("Unknown extension " + extension);
                    }
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

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            SelectSkeleton();
        }

        private void SelectSkeleton()
        {
            int n = toolStripComboBox2.SelectedIndex;
            if (n == -1)
                return;
            EBX ebx;
            switch (n)
            {
                case 0:
                    skeleton = null;
                    break;
                case 1:
                    try
                    {
                        OpenFileDialog openSkelFileDialog = new OpenFileDialog();
                        openSkelFileDialog.Title = "Select skeleton file";
                        openSkelFileDialog.Filter = "*.bin|*.bin";
                        if (openSkelFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            ebx = new EBX(new MemoryStream(File.ReadAllBytes(openSkelFileDialog.FileName)));
                            skeleton = new FBSkeleton(ebx);
                            RenderObject skel = MakeSkeletonMesh(skeleton);
                            for (int i = 0; i < skel.vertices.Length; i++)
                            {
                                skel.vertices[i].X -= mid.X;
                                skel.vertices[i].Y -= mid.Y;
                                skel.vertices[i].Z -= mid.Z;
                            }
                            skel.InitGeometry();
                            if (DXHelper.objects.Count == 2)
                                DXHelper.objects[1] = skel;
                            else
                                DXHelper.objects.Add(skel);
                        }
                    }
                    catch { }
                    break;
                default:
                    try
                    {
                        string sha1 = skeletons[toolStripComboBox2.Items[n].ToString()];
                        ebx = new EBX(new MemoryStream(main.Host.getDataBySha1(Helpers.HexStringToByteArray(sha1))));
                        skeleton = new FBSkeleton(ebx);
                        RenderObject skel = MakeSkeletonMesh(skeleton);
                        for (int i = 0; i < skel.vertices.Length; i++)
                        {
                            skel.vertices[i].X -= mid.X;
                            skel.vertices[i].Y -= mid.Y;
                            skel.vertices[i].Z -= mid.Z;
                        }
                        skel.InitGeometry();
                        if (DXHelper.objects.Count == 2)
                            DXHelper.objects[1] = skel;
                        else
                            DXHelper.objects.Add(skel);
                    }
                    catch { }
                    break;
            }
        }
    }
}
