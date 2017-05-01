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

namespace PluginTexturesWV
{
    public partial class MainForm : Form
    {
        public MainClass main;
        public List<string> tocFiles;
        public List<string> tocLabels;
        public Dictionary<string, List<string>> bundlePaths;
        public Dictionary<string, List<ChunkInfo>> tocChunks;
        public string currBundle;
        public List<DataInfo> res = new List<DataInfo>();
        public List<ChunkInfo> chunks = new List<ChunkInfo>();
        public List<uint> texAssetTypes = new List<uint>(new uint[]{0x6bde20ba});

        public TextureAsset currTexture = null;
        public string currPath = "";
        public byte[] rawResBuffer;
        public byte[] rawChunkBuffer;
        public byte[] rawDDSBuffer;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            tocLabels = main.Host.getTOCFileLabels();
            tocFiles = main.Host.getTOCFiles();
            bundlePaths = new Dictionary<string, List<string>>();
            tocChunks = new Dictionary<string, List<ChunkInfo>>();
            foreach (string toc in tocFiles)
            {
                bundlePaths.Add(toc, main.Host.getBundleNames(toc));
                tocChunks.Add(toc, main.Host.getAllTocCHUNKs(toc));
            }
            RefreshStuff();
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
                if (texAssetTypes.Contains((uint)info.type))
                    Helpers.AddPath(t, info.path);
            t.ExpandAll();
            tv2.Nodes.Add(t);
        }

        private void tv1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode sel = tv1.SelectedNode;
            if (sel == null)
                return;
            if (sel.Nodes.Count == 0)
            {
                currBundle = Helpers.GetPathFromNode(sel).Substring(5);
                foreach(KeyValuePair<string,List<string>> pair in bundlePaths)
                    if (pair.Value.Contains(currBundle))
                    {
                        res = main.Host.getAllRES(pair.Key, currBundle);
                        chunks = main.Host.getAllBundleCHUNKs(pair.Key, currBundle);
                        RefreshTextures();
                    }
            }
        }

        private void tv2_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode sel = tv2.SelectedNode;
            if (sel == null)
                return;
            if (sel.Parent != null && sel.Nodes.Count == 0)
            {
                string path = Helpers.GetPathFromNode(sel).Substring(5);
                foreach (DataInfo info in res)
                    if (info.path == path)
                        LoadTexture(info.sha1, path);
            }
        }

        public void LoadTexture(byte[] sha1, string path)
        {
            currPath = path;
            rtb1.Text = "";
            currTexture = null;
            rawResBuffer = main.Host.getDataBySha1(sha1);
            exportResRawToolStripMenuItem.Enabled = false;
            if (rawResBuffer == null)
                return;
            exportResRawToolStripMenuItem.Enabled = true;
            ProcessTexture();
            exportAsDDSToolStripMenuItem.Enabled = currTexture.isKnownFormat();
            hb1.ByteProvider = new DynamicByteProvider(rawResBuffer);
            MemoryStream m = new MemoryStream(rawResBuffer);
            m.Seek(0x20, 0);
            byte[] id = new byte[0x10];
            m.Read(id, 0, 0x10);
            exportChunkRawToolStripMenuItem.Enabled = false;
            foreach(ChunkInfo info in chunks)
                if (Helpers.ByteArrayCompare(Helpers.HexStringToByteArray(info.id), id))
                {
                    rawChunkBuffer = main.Host.getDataBySha1(info.sha1);
                    if (rawChunkBuffer == null)
                        return;
                    exportChunkRawToolStripMenuItem.Enabled = true;
                    hb2.ByteProvider = new DynamicByteProvider(rawChunkBuffer);
                    if (currTexture.isKnownFormat())
                        MakePreview();
                    else
                        pic1.Image = null;
                }
        }

        public void ProcessTexture()
        {
            currTexture = new TextureAsset(rawResBuffer);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Texture Infos");
            sb.AppendLine("Name         : " + Path.GetFileName(currPath));
            sb.AppendLine("Type         : " + currTexture.formatType);
            sb.AppendLine("Format       : " + currTexture.formatID + " - 0x" 
                                            + currTexture.formatID.ToString("X2") 
                                            + (currTexture.isKnownFormat() ? " (supported)" : " (unsupported)")
                                            + " (" + Enum.GetName(typeof(TextureAsset.FB_FORMAT), currTexture.formatID) + ")");
            int factor = (int)Math.Pow(2, currTexture.firstMip);
            sb.AppendLine("Size X       : " + currTexture.width + " (" + currTexture.width / factor + ")");
            sb.AppendLine("Size Y       : " + currTexture.height + " (" + currTexture.height / factor + ")");
            sb.AppendLine("Depth        : " + currTexture.depth);
            sb.AppendLine("Slice Count  : " + currTexture.sliceCount);
            sb.AppendLine("Mip Count    : " + currTexture.mipCount);
            sb.AppendLine("Mip Start    : " + currTexture.firstMip);
            sb.AppendLine("Mip 1 Offset : 0x" + currTexture.firstMipOffset.ToString("X8"));
            sb.AppendLine("Mip 2 Offset : 0x" + currTexture.secondMipOffset.ToString("X8"));
            sb.AppendLine("Mip Sizes    : ");
            factor = 1;
            uint sum = 0;
            foreach (uint size in currTexture.mipDataSizes)
            {
                sb.Append(" -> " + currTexture.width / factor + "x" + currTexture.height / factor + " = ");
                sb.AppendLine("0x" + size.ToString("X") + " bytes");
                factor <<= 1;
                sum += size;
            }
            sb.AppendLine(" = 0x" + sum.ToString("X") + " bytes");
            rtb1.Text = sb.ToString();
        }

        public void MakePreview()
        {
            rawDDSBuffer = currTexture.MakeRawDDSBuffer(rawChunkBuffer);
            if (File.Exists("preview.dds"))
                File.Delete("preview.dds");
            if (File.Exists("preview.png"))
                File.Delete("preview.png");
            File.WriteAllBytes("preview.dds", rawDDSBuffer);
            Helpers.RunShell(Path.GetDirectoryName(Application.ExecutablePath) + "\\texconv.exe", "-ft png preview.dds");
            File.Delete("preview.dds");
            if (File.Exists("preview.png"))
            {
                pic1.Image = Helpers.LoadImageCopy("preview.png");
                File.Delete("preview.png");
            }
        }

        private void exportResRawToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void exportChunkRawToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (rawChunkBuffer != null && rawChunkBuffer.Length != 0)
            {
                SaveFileDialog d = new SaveFileDialog();
                d.Filter = "*.bin|*.bin";
                d.FileName = Path.GetFileName(currPath) + ".chunk.bin";
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.WriteAllBytes(d.FileName, rawChunkBuffer);
                    MessageBox.Show("Done.");
                }
            }
        }

        private void exportAsDDSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (rawDDSBuffer != null && rawDDSBuffer.Length != 0)
            {
                SaveFileDialog d = new SaveFileDialog();
                d.Filter = "*.dds|*.dds";
                d.FileName = Path.GetFileName(currPath) + ".dds";
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.WriteAllBytes(d.FileName, rawDDSBuffer);
                    MessageBox.Show("Done.");
                }
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
    }
}
