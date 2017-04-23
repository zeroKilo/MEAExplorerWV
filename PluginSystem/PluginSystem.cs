using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginSystem
{
    public class ChunkInfo
    {
        public string id;
        public string toc;
        public string bundle;
        public byte[] sha1;
        public ChunkInfo(string _id, string _toc, string _bundle, byte[] _sha1)
        {
            id = _id;
            toc = _toc;
            bundle = _bundle;
            sha1 = _sha1;
        }
    }

    public class DataInfo
    {
        public int type; //0=ebx
        public string path;
        public string toc;
        public string bundle;
        public byte[] sha1;
        public byte[] idata;
        public DataInfo(int _type, string _path, string _toc, string _bundle, byte[] _sha1, byte[] _idata)
        {
            type = _type;
            path = _path;
            toc = _toc;
            bundle = _bundle;
            sha1 = _sha1;
            idata = _idata;
        }
    }

    public interface IPlugin
    {
        IPluginHost Host { get; set; }
        string Name { get; }
        bool addToMainMenu { get; }
        bool addToContextMenu { get; }
        bool supportsAllResTypes { get; }
        List<uint> supportedResTypes { get; }
        void DoMain();
        void DoContextData(DataInfo info);
        string RunModJob(byte[] payload);
    }

    public interface IPluginHost
    {
        List<string> getTOCFiles();
        List<string> getTOCFileLabels();
        List<string> getBundleNames(string toc);
        List<DataInfo> getAllEBX(string toc, string bundle);
        List<DataInfo> getAllRES(string toc, string bundle);
        List<ChunkInfo> getAllTocCHUNKs(string toc);
        List<ChunkInfo> getAllBundleCHUNKs(string toc, string bundle);
        byte[] getDataBySha1(byte[] sha1);
        int setDataBySha1(byte[] data, byte[] sha1, string toc);
        void setAutoPatching(bool value);
        void AddModJob(string pname, string desc, byte[] payload);
    }
}
