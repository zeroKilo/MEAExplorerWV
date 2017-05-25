using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginSystem
{
    public abstract class ASkinnedMeshExporter
    {
        protected FBSkeleton Skeleton;

        protected ASkinnedMeshExporter(FBSkeleton _skel)
        {
            Skeleton = _skel;
        }
    }
}
