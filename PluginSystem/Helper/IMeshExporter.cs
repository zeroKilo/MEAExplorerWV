using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginSystem
{
    public interface IMeshExporter
    {
        void ExportLod(MeshAsset mesh, int lodIndex, string targetfile);
        void ExportAllLods(MeshAsset mesh, string targetdir);
    }
}
