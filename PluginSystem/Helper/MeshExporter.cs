using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginSystem
{
    public class MeshExporter
    {
        public static IMeshExporter GetExporterByExtension(string extension, FBSkeleton skeleton = null)
        {
            IMeshExporter exporter;
            switch (extension)
            {
                case ".obj":
                    exporter = new OBJExporter();
                    break;
                case ".psk":
                    exporter = new PSKExporter(skeleton);
                    break;
                case ".fbx":
                    //exporter = new FBXExporter(skeleton);
                    //break;
                default:
                    exporter = null;
                    break;
            }
            return exporter;
        }


    } 
}
