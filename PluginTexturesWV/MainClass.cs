using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PluginSystem;

namespace PluginTexturesWV
{
    public class MainClass : IPlugin
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
                return "Texture Plugin";
            }
        }

        public bool addToMainMenu { get { return true; } }
        public bool addToContextMenu { get { return false; } }
        public bool supportsAllResTypes { get { return false; } }
        public List<uint> supportedResTypes { get { return null; } }
        public void DoMain() 
        {
            MainForm mf = new MainForm();
            mf.main = this;
            mf.Show();
        }
        public void DoContextData(DataInfo info) { }

        public string RunModJob(byte[] payload)
        {
            MemoryStream m = new MemoryStream(payload);
            byte[] sha1 = new byte[0x14];
            m.Read(sha1, 0, 0x14);
            string toc = Helpers.ReadNullString(m);
            byte[] data = new byte[(int)(m.Length - m.Position)];
            m.Read(data, 0, data.Length);
            int count = host.setDataBySha1(data, sha1, toc);
            return "Texture Import done with " + count + " replacement(s).";
        }
    }
}
