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

namespace TestPlugin3
{
    public partial class MainWindow : Form
    {
        public TestPluginClass plug;
        public List<string> tocFiles;
        public List<string> tocLabels;
        public Dictionary<string, List<string>> bundlePaths;
        public Dictionary<string, List<ChunkInfo>> tocChunks;
        public bool stop = false;
        public int MaxResults;
        public SortedDictionary<string, byte[]> ebxDump = new SortedDictionary<string,byte[]>();

        public MainWindow()
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
            if (File.Exists("guids.bin"))
            {
                byte[] buff = PluginSystem.Helpers.ZStdDecompress(File.ReadAllBytes("guids.bin"));
                MemoryStream m = new MemoryStream(buff);
                m.Seek(0, 0);
                StreamReader sr = new StreamReader(m);
                string line;
                ebxDump = new SortedDictionary<string, byte[]>();
                while ((line = sr.ReadLine()) != null)
                    ebxDump.Add(line.Substring(0x21), Helpers.HexStringToByteArray(line.Substring(0, 0x20)));
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            pb1.Value = 0;
            pb1.Maximum = 1;
            foreach (KeyValuePair<string, List<string>> p in bundlePaths)
                pb1.Maximum += p.Value.Count;
            stop = false;
            long skipped = 0;
            long found = 0;
            foreach (string key in bundlePaths.Keys)
            {
                rtb1.AppendText("Loading " + key + " ...\r\n");
                rtb1.SelectionStart = rtb1.Text.Length;
                rtb1.SelectionLength = 0;
                rtb1.ScrollToCaret();
                Application.DoEvents();
                foreach (string bundle in bundlePaths[key])
                {
                    List<DataInfo> ebxlist = plug.Host.getAllEBX(key, bundle);
                    rtb1.AppendText(" Processing bundle " + bundle + " ...\r\n");
                    rtb1.SelectionStart = rtb1.Text.Length;
                    rtb1.SelectionLength = 0;
                    rtb1.ScrollToCaret();
                    pb1.Value++;
                    Application.DoEvents();
                    int count = 0;
                    foreach (DataInfo ebx in ebxlist)
                    {
                        count++;
                        Status.Text = "(known " + ebxDump.Keys.Count + ")(found " + found + ")(skipped " + skipped + ") " + count + "/" + ebxlist.Count + " " + ebx.path;
                        Application.DoEvents();
                        if (ebxDump.ContainsKey(ebx.path))
                        {
                            skipped++;
                            continue;
                        }
                        byte[] data = plug.Host.getDataBySha1(ebx.sha1);
                        if ( data != null && data.Length >= 0x38)
                        {
                            MemoryStream m = new MemoryStream(data);
                            m.Seek(0x28, 0);
                            byte[] guid = new byte[0x10];
                            m.Read(guid, 0, 0x10);
                            ebxDump.Add(ebx.path, guid);
                            found++;
                        }
                        if (stop) break;
                    }
                    GC.Collect();
                    if (stop) break;
                }
                Status.Text = "";
                if (stop) break;
            }
            pb1.Value = 0;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            stop = true;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.bin|*.bin";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StringBuilder sb = new StringBuilder();
                foreach (KeyValuePair<string, byte[]> pair in ebxDump)
                    sb.AppendLine(Helpers.ByteArrayToHexString(pair.Value) + " " + pair.Key);
                byte[] buff = Encoding.UTF8.GetBytes(sb.ToString());
                File.WriteAllBytes(d.FileName, Helpers.ZStdCompress(buff));
                MessageBox.Show("Done.");
            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.bin|*.bin";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MemoryStream m = new MemoryStream(Helpers.ZStdDecompress(File.ReadAllBytes(d.FileName)));
                StreamReader sr = new StreamReader(m);
                string line;
                ebxDump = new SortedDictionary<string,byte[]>();
                while((line = sr.ReadLine()) != null)
                    if (line.Trim() != "")
                        ebxDump.Add(line.Substring(0x21), Helpers.HexStringToByteArray(line.Substring(0, 0x20)));
                MessageBox.Show("Done.");
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] guid = Helpers.HexStringToByteArray(toolStripTextBox1.Text.Replace("-", "").Trim());
                if (guid.Length != 0x10)
                    return;
                foreach (KeyValuePair<string, byte[]> pair in ebxDump)
                    if (Helpers.ByteArrayCompare(pair.Value, guid))
                    {
                        rtb1.Text = "EBXPath for guid " + Helpers.ByteArrayToHexString(guid) + " = " + pair.Key;
                        return;
                    }
                rtb1.Text = "Nothing found for guid " + Helpers.ByteArrayToHexString(guid);
            }
            catch { }
        }
    }
}
