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

namespace PluginTalktableWV
{
    public partial class TalkTableEditor : Form
    {
        public byte[] rawBuffer;
        public string tocPath;
        public TalkTableAsset table;
        public bool _exitSave = false;
        public PluginSystem.IPluginHost host;
        public PluginSystem.DataInfo info;
        public MainClass main;

        public TalkTableEditor()
        {
            InitializeComponent();
        }

        private void TalkTableEditor_Load(object sender, EventArgs e)
        {
            table = new TalkTableAsset();
            table.Read(new MemoryStream(rawBuffer));
            RefreshTable();
        }

        public void RefreshTable()
        {
            listBox1.Items.Clear();
            foreach (STR s in table.Strings)
                listBox1.Items.Add(s.Value);
        }

        private void exportToTXTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.txt|*.txt";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StringBuilder sb = new StringBuilder();                
                foreach (STR s in table.Strings)
                {
                    sb.Append(s.ID.ToString("X8") + " ");
                    sb.AppendLine(s.Value.Replace("\r", "\\r").Replace("\n", "\\n"));
                }
                File.WriteAllText(d.FileName, sb.ToString(), Encoding.Unicode);
                MessageBox.Show("Done.");
            }
        }

        private void importFromTXTToolStripMenuItem_Click(object sender, EventArgs e)
        {

            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.txt|*.txt";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] lines = File.ReadAllLines(d.FileName, Encoding.Unicode);
                table.Strings = new List<STR>();
                foreach (string line in lines)
                    if (line.Trim() != "")
                    {                        
                        STR str = new STR();
                        str.ID = Convert.ToUInt32(line.Substring(0, 8), 16);
                        str.Value = line.Substring(9).Replace("\\r", "\r").Replace("\\n", "\n");
                        table.Strings.Add(str);
                    }
                RefreshTable();
            }
        }

        private void saveAndCloseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MemoryStream m = new MemoryStream();
            table.Save(m);
            rawBuffer = m.ToArray();            
            _exitSave = true;
            this.Close();
        }

        private void addModJobToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MemoryStream m = new MemoryStream();
            table.Save(m);
            rawBuffer = m.ToArray();
            m = new MemoryStream();
            m.Write(info.sha1, 0, 0x14);
            List<string> list = main.Host.getTOCFileLabels();
            string tocname = null;
            foreach (string toc in list)
                if (info.toc.EndsWith(toc))
                {
                    tocname = toc;
                    break;
                }
            Helpers.WriteNullString(m, tocname);
            m.Write(rawBuffer, 0, rawBuffer.Length);
            host.AddModJob(main.Name, "Talktable Replacement", m.ToArray());
            MessageBox.Show("Done.");
        }
    }
}
