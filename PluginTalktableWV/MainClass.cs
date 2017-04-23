using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PluginSystem;

namespace PluginTalktableWV
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
                return "Talk Table Plugin";
            }
        }

        public bool addToMainMenu { get { return false; } }
        public bool addToContextMenu { get { return true; } }
        public bool supportsAllResTypes { get { return false; } }
        public List<uint> supportedResTypes { get { return new List<uint>(new uint[] { 0x5e862e05 }); } }
        public void DoMain() { }
        public void DoContextData(DataInfo info) 
        {
            TalkTableEditor tte = new TalkTableEditor();
            tte.rawBuffer = host.getDataBySha1(info.sha1);
            tte.host = host;
            tte.info = info;
            tte.main = this;
            tte.ShowDialog();
            if (tte._exitSave)
            {
                int count = host.setDataBySha1(tte.rawBuffer, info.sha1, info.toc);
                MessageBox.Show("Import done with " + count + " replacement(s).");
            }
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
            return "Talktable Import done with " + count + " replacement(s).";
        }
    }
}
