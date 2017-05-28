using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PluginSystem;

namespace TestPlugin3
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
                return "EBX GUID Plugin";
            }
        }

        public bool addToMainMenu { get { return true; } }
        public bool addToContextMenu { get { return false; } }
        public bool supportsAllResTypes { get { return false; } }
        public List<uint> supportedResTypes { get { return null; } }
        public void DoMain()
        {
            MainWindow w = new MainWindow();
            w.plug = this;
            w.ShowDialog();
        }
        public void DoContextData(DataInfo info) { }
        
        public string RunModJob(byte[] payload)
        {
            return "";
        }
    }
}
