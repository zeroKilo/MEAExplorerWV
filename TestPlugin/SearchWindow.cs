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

namespace TestPlugin
{
    public partial class SearchWindow : Form
    {
        public TestPluginClass plug;
        public List<string> tocFiles;
        public List<string> tocLabels;
        public Dictionary<string, List<string>> bundlePaths;
        public Dictionary<string, List<ChunkInfo>> tocChunks;
        public bool stop = false;
        public int MaxResults;

        public SearchWindow()
        {
            InitializeComponent();
        }

        private void SearchWindow_Load(object sender, EventArgs e)
        {
            tocLabels = plug.Host.getTOCFileLabels();
            tocFiles = plug.Host.getTOCFiles();
            bundlePaths = new Dictionary<string,List<string>>();
            tocChunks = new Dictionary<string, List<ChunkInfo>>();
            pb1.Value = pb1.Maximum = 0;
            foreach (string toc in tocFiles)
            {
                List<string> name = plug.Host.getBundleNames(toc);
                pb1.Maximum += name.Count;
                bundlePaths.Add(toc, name);
                tocChunks.Add(toc, plug.Host.getAllTocCHUNKs(toc));
            }
            toolStripComboBox1.Items.Clear();
            toolStripComboBox1.Items.Add("Bundle");
            toolStripComboBox1.Items.Add("EBX");
            toolStripComboBox1.Items.Add("RES");
            toolStripComboBox1.Items.Add("RESTYPE");
            toolStripComboBox1.Items.Add("CHUNK");
            toolStripComboBox1.SelectedIndex = 0;
            if (File.Exists("plugins\\Search Plugin\\config.txt"))
                try
                {
                    string[] lines = File.ReadAllLines("plugins\\Search Plugin\\config.txt");
                    foreach (string line in lines)
                    {
                        string[] parts = line.Split('=');
                        if (parts.Length == 2)
                            switch (parts[0].ToLower().Trim())
                            {
                                case "maxresults":
                                    MaxResults = Convert.ToInt32(parts[1].Trim());
                                    break;
                            }
                    }
                }
                catch { }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (toolStripTextBox1.Text.Length == 0)
                return;
            toolStripButton1.Enabled = false;
            stop = false;
            listBox1.Items.Clear();
            string s = toolStripTextBox1.Text.ToLower();
            pb1.Value = 0;
            switch (toolStripComboBox1.SelectedIndex)
            {
                case 0:
                    SearchBundle(s);
                    break;
                case 1:
                    SearchEBX(s);
                    break;
                case 2:
                    SearchRES(s);
                    break;
                case 3:
                    SearchRESType(Convert.ToInt32(s, 16));
                    break;
                case 4:
                    SearchTocChunk(s);
                    SearchCHUNKS(s);
                    break;
            }
            pb1.Value = 0;
            stop = false;
            toolStripButton1.Enabled = true;
        }

        public void SearchBundle(string s)
        {
            foreach (KeyValuePair<string, List<string>> pair in bundlePaths)
            {
                Status.Text = pair.Key;
                foreach (string bundle in pair.Value)
                    if (bundle.ToLower().Contains(s))
                        listBox1.Items.Add(bundle + " -> " + pair.Key);
            }
            Status.Text = "";
        }

        public void SearchTocChunk(string s)
        {
            foreach (KeyValuePair<string, List<ChunkInfo>> pair in tocChunks)
            {
                Status.Text = pair.Key;
                foreach (ChunkInfo info in pair.Value)
                    if (info.id.ToLower().Contains(s))
                        listBox1.Items.Add(Helpers.ByteArrayToHexString(info.sha1) + " -> " + info.id + " -> " + pair.Key);
            }
            Status.Text = "";
        }

        public void SearchEBX(string s)
        {
            foreach (KeyValuePair<string, List<string>> pair in bundlePaths)
                foreach (string bundle in pair.Value)
                {
                    pb1.Value++;
                    Status.Text = "(" + listBox1.Items.Count + " found) " + bundle;
                    Application.DoEvents();
                    if (stop)
                    {
                        Status.Text = ""; 
                        return;
                    }
                    foreach (DataInfo ebx in plug.Host.getAllEBX(pair.Key, bundle))
                        if (ebx.path.ToLower().Contains(s))
                        {
                            listBox1.Items.Add(Helpers.ByteArrayToHexString(ebx.sha1) + " -> " + pair.Key + " -> " + bundle + " -> " + ebx.path);
                            listBox1.SelectedIndex = listBox1.Items.Count - 1;
                            Application.DoEvents();
                            if (listBox1.Items.Count >= MaxResults)
                            {
                                listBox1.Items.Add("...too many results to display");
                                Status.Text = ""; 
                                return;
                            }
                        }
                }
        }

        public void SearchRES(string s)
        {
            foreach (KeyValuePair<string, List<string>> pair in bundlePaths)
                foreach (string bundle in pair.Value)
                {
                    pb1.Value++;
                    Status.Text = "(" + listBox1.Items.Count + " found) " + bundle;
                    Application.DoEvents();
                    if (stop)
                    {
                        Status.Text = "";
                        return;
                    }
                    foreach (DataInfo res in plug.Host.getAllRES(pair.Key, bundle))
                        if (res.path.ToLower().Contains(s))
                        {
                            listBox1.Items.Add(Helpers.ByteArrayToHexString(res.sha1) + " -> " + pair.Key + " -> " + bundle + " -> " + res.path);
                            listBox1.SelectedIndex = listBox1.Items.Count - 1;
                            Application.DoEvents();
                            if (listBox1.Items.Count >= MaxResults)
                            {
                                listBox1.Items.Add("...too many results to display");
                                Status.Text = ""; 
                                return;
                            }
                        }
                }
        }

        public void SearchRESType(int t)
        {
            foreach (KeyValuePair<string, List<string>> pair in bundlePaths)
                foreach (string bundle in pair.Value)
                {
                    pb1.Value++;
                    Status.Text = "(" + listBox1.Items.Count + " found) " + bundle;
                    Application.DoEvents();
                    if (stop)
                    {
                        Status.Text = "";
                        return;
                    }
                    foreach (DataInfo res in plug.Host.getAllRES(pair.Key, bundle))
                        if (res.type == t)
                        {
                            listBox1.Items.Add(Helpers.ByteArrayToHexString(res.sha1) + " -> " + pair.Key + " -> " + bundle + " -> " + res.path);
                            listBox1.SelectedIndex = listBox1.Items.Count - 1;
                            Application.DoEvents();
                            if (listBox1.Items.Count >= MaxResults)
                            {
                                listBox1.Items.Add("...too many results to display");
                                Status.Text = ""; 
                                return;
                            }
                        }
                }
        }

        public void SearchCHUNKS(string s)
        {
            foreach (KeyValuePair<string, List<string>> pair in bundlePaths)
                foreach (string bundle in pair.Value)
                {
                    pb1.Value++;
                    Status.Text = "(" + listBox1.Items.Count + " found) " + bundle;
                    Application.DoEvents();
                    if (stop)
                    {
                        Status.Text = "";
                        return;
                    }
                    foreach (ChunkInfo chunk in plug.Host.getAllBundleCHUNKs(pair.Key, bundle))
                        if (chunk.id.ToLower().Contains(s))
                        {
                            listBox1.Items.Add(Helpers.ByteArrayToHexString(chunk.sha1) + " -> " + pair.Key + " -> " + bundle + " -> " + chunk.id);
                            listBox1.SelectedIndex = listBox1.Items.Count - 1;
                            Application.DoEvents();
                            if (listBox1.Items.Count >= MaxResults)
                            {
                                listBox1.Items.Add("...too many results to display");
                                Status.Text = ""; 
                                return;
                            }
                        }
                }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            stop = true;
        }

        private void SearchWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            stop = true;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.txt|*.txt";
            d.FileName = "search_" + toolStripTextBox1.Text + ".txt";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string item in listBox1.Items)
                    sb.AppendLine(item);
                File.WriteAllText(d.FileName, sb.ToString());
                MessageBox.Show("Done.");
            }
        }
    }
}
