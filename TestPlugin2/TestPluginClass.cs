using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PluginSystem;

namespace TestPlugin2
{
    public class TestPluginClass : IPlugin
    {
        private IPluginHost host;
        public IPluginHost Host
        {
            get { return host; }
            set { host = value; }
        }

        public string Name
        {
            get
            {
                return "SHA1 Export/Import";
            }
        }

        public bool addToMainMenu { get { return true; } }
        public bool addToContextMenu { get { return true; } }
        public bool supportsAllResTypes { get { return true; } }
        public List<uint> supportedResTypes { get { return null; } }
        public void DoMain()
        {
            Form1 w = new Form1();
            w.plug = this;
            w.Height = 92;
            w.button3.Enabled = false;
            w.ShowDialog();
        }
        public void DoContextData(DataInfo info)
        {
            Form1 w = new Form1();
            w.plug = this;
            w.Height = 120;
            w.button3.Enabled = true;
            w.info = info;
            w.textBox1.Text = ByteArrayToHexString(info.sha1);
            w.textBox1.Enabled = false;
            w.ShowDialog();
        }

        public string ByteArrayToHexString(byte[] data, int start = 0, int len = 0)
        {
            if (data == null)
                data = new byte[0];
            StringBuilder sb = new StringBuilder();
            if (start == 0)
                foreach (byte b in data)
                    sb.Append(b.ToString("X2"));
            else
                if (start > 0 && start + len <= data.Length)
                    for (int i = start; i < start + len; i++)
                        sb.Append(data[i].ToString("X2"));
                else
                    return "";
            return sb.ToString();
        }
        
        public string RunModJob(byte[] payload)
        {
            MemoryStream m = new MemoryStream(payload);
            byte[] sha1 = new byte[0x14];
            m.Read(sha1, 0, 0x14);
            string toc = Helpers.ReadNullString(m);
            byte[] data = new byte[(int)(m.Length - m.Position)];
            m.Read(data, 0, data.Length);
            int count = host.setDataBySha1(data, sha1, toc);
            return "Import by SHA1 done with " + count + " replacement(s).";}
    }
}
