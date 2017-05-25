using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PluginSystem;

namespace PluginMeshesWV
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
                return "Mesh Plugin";
            }
        }

        public bool addToMainMenu { get { return true; } }
        public bool addToContextMenu { get { return true; } }
        public bool supportsAllResTypes { get { return false; } }
        public List<uint> supportedResTypes { get { return new List<uint>(new uint[] { 0x49b156d4 }); } }
        public void DoMain() 
        {
            MainForm mf = new MainForm();
            mf.main = this;
            mf.Show();
        }
        public void DoContextData(DataInfo info)
        {
            MainForm mf = new MainForm();
            mf.main = this;
            mf.Show();
            mf.LoadSpecific(info);
        }

        public string RunModJob(byte[] payload) { return null; }
    }
}
