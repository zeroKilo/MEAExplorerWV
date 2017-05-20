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

namespace PluginEbxWV
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
        public List<ChunkInfo> chunks = new List<ChunkInfo>();
        public List<ChunkInfo> globalChunks = new List<ChunkInfo>();
        public string currPath = "";
        public string currToc = "";
        public byte[] currSha1 = null;

        public EBX ebxObj;
        public EBX.EBXNodeField field = null;

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
            parts = info.path.Replace(".ebx", "").Split('/');
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
                    ebx = main.Host.getAllEBX(pair.Key, currBundle);
                    chunks.AddRange(main.Host.getAllBundleCHUNKs(pair.Key, currBundle));
                    RefreshEBXTree();
                    return;
                }
        }

        public void RefreshEBXTree()
        {
            tv2.Nodes.Clear();
            TreeNode t = new TreeNode("ROOT");
            foreach (DataInfo info in ebx)
                    Helpers.AddPath(t, info.path);
            t.ExpandAll();
            tv2.Nodes.Add(t);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Helpers.SelectNext(toolStripTextBox1.Text, tv1);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Helpers.SelectNext(toolStripTextBox2.Text, tv2);
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

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            if (tv1.SelectedNode == null)
                Helpers.CollapseAll(tv1.Nodes[0]);
            else
                Helpers.CollapseAll(tv1.SelectedNode);
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            if (tv2.SelectedNode == null)
                Helpers.CollapseAll(tv2.Nodes[0]);
            else
                Helpers.CollapseAll(tv2.SelectedNode);
        }

        private void copyNodePathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tv1.SelectedNode != null)
                Clipboard.SetText(Helpers.GetPathFromNode(tv1.SelectedNode));
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (tv2.SelectedNode != null)
                Clipboard.SetText(Helpers.GetPathFromNode(tv2.SelectedNode));
        }

        private void tv2_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode sel = tv2.SelectedNode;
            if (sel == null)
                return;
            string path = Helpers.GetPathFromNode(sel);
            if (path.Length > 5)
                path = path.Substring(5);
            foreach (DataInfo ebxInfo in ebx)
                        if (ebxInfo.path == path)
                        {
                            LoadEBX(ebxInfo.sha1, path);
                            return;
                        }
        }

        public void LoadEBX(byte[] sha1, string path)
        {
            currPath = path;
            currSha1 = sha1;
            byte[] buff = main.Host.getDataBySha1(sha1);
            if (buff == null)
                return;
            ebxObj = new EBX(new MemoryStream(buff));
            DisplayEBX();
        }

        private void DisplayEBX()
        {
            rtb1.Text = ebxObj.ToString();
            rtb2.Text = ebxObj.PrintFieldDescriptors();
            rtb3.Text = ebxObj.PrintTypeDescriptors();
            rtb4.Text = ebxObj.PrintFields();
            rtb5.Text = ebxObj.PrintArrays();
            rtb6.Text = ebxObj.PrintStringTable();
            tv3.Nodes.Clear();
            tv3.Nodes.Add(ebxObj.ToNode());
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            Helpers.SelectNext(toolStripTextBox3.Text, tv3);
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            if (tv3.SelectedNode == null)
                tv3.Nodes[0].ExpandAll();
            else
                tv3.SelectedNode.ExpandAll();
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            if (tv3.SelectedNode == null)
                Helpers.CollapseAll(tv3.Nodes[0]);
            else
                Helpers.CollapseAll(tv3.SelectedNode);
        }

        private void tv3_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode t = tv3.SelectedNode;
            if (t == null)
                return;
            List<int> ind = new List<int>();
            ind.Add(t.Index);
            TreeNode t2 = t.Parent;
            while (t2 != null && t2.Text != "EBX")
            {
                ind.Insert(0, t2.Index);
                t2 = t2.Parent;
            }
            field = null;
            EBX.EBXNodeType type = ebxObj.nodelist[ind[0]];
            for (int i = 1; i < ind.Count; )
            {
                if (i == 1)
                    field = type.fields[ind[1]];
                i++;
                byte typ = field.layout.GetFieldType();
                switch (typ)
                {
                    case 0:
                    case 2:
                        if (i < ind.Count)
                        {
                            type = (EBX.EBXNodeType)field.data;
                            i++;
                            if (i < ind.Count)
                                field = type.fields[ind[i]];
                        }
                        break;
                    case 4:
                        if (i < ind.Count)
                            field = ((List<EBX.EBXNodeField>)field.data)[ind[i]];
                        if (field.data is EBX.EBXNodeType)
                        {
                            type = (EBX.EBXNodeType)field.data;
                            i++;
                            if (i < ind.Count - 1)
                                field = type.fields[ind[++i]];
                            else
                                i = ind.Count;
                        }
                        else
                            i = ind.Count;
                        break;
                }
            }
            bool enableEditing = false;
            if (field != null)
            {
                label1.Text = "#" + t.Index + " @0x" + field.offset.ToString("X8") + " " + ebxObj.keywords[field.layout.nameHash];
                switch (field.layout.GetFieldType())
                {
                    case 7:
                        textBox1.Text = (string)field.data;
                        break;
                    case 0xA:
                    case 0xB:
                    case 0xC:
                        textBox1.Text = ((byte)field.data).ToString("X");
                        enableEditing = true;
                        break;
                    case 0xD:
                    case 0xE:
                        textBox1.Text = ((ushort)field.data).ToString("X");
                        enableEditing = true;
                        break;
                    case 3:
                    case 0xF:
                    case 0x10:
                        textBox1.Text = ((uint)field.data).ToString("X");
                        enableEditing = true;
                        break;
                    case 0x13:
                        textBox1.Text = ((float)field.data).ToString();
                        enableEditing = true;
                        break;
                    case 0x11:
                    case 0x12:
                    case 0x14:
                        textBox1.Text = ((ulong)field.data).ToString("X");
                        enableEditing = true;
                        break;
                }
            }
            else
                label1.Text = "#" + t.Index;
            label1.Enabled =
            textBox1.Enabled =
            button1.Enabled = enableEditing;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TreeNode t = tv3.SelectedNode;
            if (field == null || t == null)
                return;
            List<int> ind = new List<int>();
            ind.Add(t.Index);
            while (t.Parent.Parent != null)
            {
                t = t.Parent;
                ind.Insert(0, t.Index);
            }
            try
            {
                byte typ = field.layout.GetFieldType();
                byte[] buff;
                switch (typ)
                {
                    case 0xA:
                    case 0xB:
                    case 0xC:
                        byte b = Convert.ToByte(textBox1.Text, 16);
                        ebxObj.rawBuffer[field.offset] = b;
                        break;
                    case 0xD:
                    case 0xE:
                        ushort us = Convert.ToUInt16(textBox1.Text, 16);
                        buff = BitConverter.GetBytes(us);
                        for (int i = 0; i < 2; i++)
                            ebxObj.rawBuffer[field.offset + i] = buff[i];
                        break;
                    case 3:
                    case 0xF:
                    case 0x10:
                        uint ui = Convert.ToUInt32(textBox1.Text, 16);
                        buff = BitConverter.GetBytes(ui);
                        for (int i = 0; i < 4; i++)
                            ebxObj.rawBuffer[field.offset + i] = buff[i];
                        break;
                    case 0x13:
                        float f = Convert.ToSingle(textBox1.Text);
                        buff = BitConverter.GetBytes(f);
                        for (int i = 0; i < 4; i++)
                            ebxObj.rawBuffer[field.offset + i] = buff[i];
                        break;
                    case 0x11:
                    case 0x12:
                    case 0x14:
                        ulong ul = Convert.ToUInt64(textBox1.Text, 16);
                        buff = BitConverter.GetBytes(ul);
                        for (int i = 0; i < 8; i++)
                            ebxObj.rawBuffer[field.offset + i] = buff[i];
                        break;
                }
                ebxObj = new EBX(new MemoryStream(ebxObj.rawBuffer));
                DisplayEBX();
                t = tv3.Nodes[0];
                foreach (int i in ind)
                    t = t.Nodes[i];
                tv3.SelectedNode = t;
                GC.Collect();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in input: " + ex.Message);
            }
        }

        private void exportAsXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.xml|*.xml";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<EBX>");
                foreach (TreeNode t in tv1.Nodes[0].Nodes)
                    Helpers.NodeToXml(sb, t, 1);
                sb.AppendLine("</EBX>");
                File.WriteAllText(d.FileName, sb.ToString());
                MessageBox.Show("Done.");
            }
        }

        private void exportAsBINToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ebxObj == null || !ebxObj._isvalid)
                return;
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.bin|*.bin";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllBytes(d.FileName, ebxObj.rawBuffer);
                MessageBox.Show("Done.");
            }
        }

        private void importChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ebxObj == null || !ebxObj._isvalid || currSha1 == null)
                return;
            int count = main.Host.setDataBySha1(ebxObj.rawBuffer, currSha1, currToc);
            MessageBox.Show("Import done with " + count + " replacements!");
        }

        private void addChangesAsModJobToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ebxObj == null || !ebxObj._isvalid || currSha1 == null)
                return;
            MemoryStream m = new MemoryStream();
            m.Write(currSha1, 0, 0x14);
            Helpers.WriteNullString(m, currToc);
            m.Write(ebxObj.rawBuffer, 0, ebxObj.rawBuffer.Length);
            main.Host.AddModJob(main.Name, "EBX Replacement for " + Path.GetFileName(currPath), m.ToArray());
            MessageBox.Show("Done.");
        }
    }
}
