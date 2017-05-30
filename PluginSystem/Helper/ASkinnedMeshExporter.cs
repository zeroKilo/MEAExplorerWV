using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginSystem
{
    public abstract class ASkinnedMeshExporter
    {
        protected SkeletonAsset Skeleton;

        protected ASkinnedMeshExporter(SkeletonAsset _skel)
        {
            Skeleton = _skel;
        }
    }
}
