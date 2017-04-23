using System;
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
            toolStripComboBox1.Items.Add("CHUNK");
            toolStripComboBox1.SelectedIndex = 0;
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
                foreach (string bundle in pair.Value)
                    if (bundle.ToLower().Contains(s))
                        listBox1.Items.Add(bundle + " -> " + pair.Key);
        }

        public void SearchTocChunk(string s)
        {
            foreach (KeyValuePair<string, List<ChunkInfo>> pair in tocChunks)
                foreach (ChunkInfo info in pair.Value)
                    if (info.id.ToLower().Contains(s))
                        listBox1.Items.Add(info.id + " -> " + pair.Key);
        }

        public void SearchEBX(string s)
        {
            foreach (KeyValuePair<string, List<string>> pair in bundlePaths)
                foreach (string bundle in pair.Value)
                {
                    pb1.Value++;
                    Application.DoEvents();
                    if (stop) return;
                    foreach (DataInfo ebx in plug.Host.getAllEBX(pair.Key, bundle))
                        if (ebx.path.ToLower().Contains(s))
                        {
                            listBox1.Items.Add(ebx.path + " -> " + bundle + " -> " + pair.Key);
                            if (listBox1.Items.Count == 1000)
                            {
                                listBox1.Items.Add("...too many results to display");
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
                    Application.DoEvents();
                    if (stop) return;
                    foreach (DataInfo res in plug.Host.getAllRES(pair.Key, bundle))
                        if (res.path.ToLower().Contains(s))
                        {
                            listBox1.Items.Add(res.path + " -> " + bundle + " -> " + pair.Key);
                            if (listBox1.Items.Count == 1000)
                            {
                                listBox1.Items.Add("...too many results to display");
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
                    Application.DoEvents();
                    if (stop) return;
                    foreach (ChunkInfo chunk in plug.Host.getAllBundleCHUNKs(pair.Key, bundle))
                        if (chunk.id.ToLower().Contains(s))
                        {
                            listBox1.Items.Add(chunk.id + " -> " + bundle + " -> " + pair.Key);
                            if (listBox1.Items.Count == 1000)
                            {
                                listBox1.Items.Add("...too many results to display");
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
    }
}
