using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PluginSystem
{
    public class FBBone
    {
        public string Name;
        public Vector Right;
        public Vector Up;
        public Vector Forward;
        public Vector Location;
        public int ParentIndex;
        public List<FBBone> Children;

        public FBBone(String InName)
        {
            Name = InName;
        }

        public float[][] CalculateBoneRotationMatrix()
        {
            float[][] RotMatrix = new float[4][];
            RotMatrix[0] = new float[4];
            RotMatrix[1] = new float[4];
            RotMatrix[2] = new float[4];
            RotMatrix[3] = new float[4];

            RotMatrix[0][0] = Right.members[0];
            RotMatrix[0][1] = Right.members[1];
            RotMatrix[0][2] = Right.members[2];
            RotMatrix[0][3] = 0.0f;

            RotMatrix[1][0] = Up.members[0];
            RotMatrix[1][1] = Up.members[1];
            RotMatrix[1][2] = Up.members[2];
            RotMatrix[1][3] = 0.0f;

            RotMatrix[2][0] = Forward.members[0];
            RotMatrix[2][1] = Forward.members[1];
            RotMatrix[2][2] = Forward.members[2];
            RotMatrix[2][3] = 0.0f;

            RotMatrix[3][0] = 0.0f;
            RotMatrix[3][1] = 0.0f;
            RotMatrix[3][2] = 0.0f;
            RotMatrix[3][3] = 1.0f;

            return RotMatrix;
        }

        public Vector CalculateBoneRotationQuat()
        {
            float[][] RotMatrix = CalculateBoneRotationMatrix();
            Vector Quat = new Vector(new float[4]);
            float tr = RotMatrix[0][0] + RotMatrix[1][1] + RotMatrix[2][2];
            float s;

            if (tr > 0.0f)
            {
                float InvS = 1.0f / (float)Math.Sqrt(tr + 1.0f);
                Quat.members[3] = 0.5f * (1.0f / InvS);
                s = 0.5f * InvS;

                Quat.members[0] = (RotMatrix[1][2] - RotMatrix[2][1]) * s;
                Quat.members[1] = (RotMatrix[2][0] - RotMatrix[0][2]) * s;
                Quat.members[2] = (RotMatrix[0][1] - RotMatrix[1][0]) * s;
            }
            else
            {
                int m = 0;
                if (RotMatrix[1][1] > RotMatrix[0][0])
                    m = 1;

                if (RotMatrix[2][2] > RotMatrix[m][m])
                    m = 2;

                int[] nxt = new int[] { 1, 2, 0 };
                int j = nxt[m];
                int k = nxt[j];

                s = RotMatrix[m][m] - RotMatrix[j][j] - RotMatrix[k][k] + 1.0f;
                float InvS = 1.0f / (float)Math.Sqrt(s);

                float[] qt = new float[4];
                qt[m] = 0.5f * (1.0f / InvS);
                s = 0.5f * InvS;

                qt[3] = (RotMatrix[j][k] - RotMatrix[k][j]) * s;
                qt[j] = (RotMatrix[m][j] + RotMatrix[j][m]) * s;
                qt[k] = (RotMatrix[m][k] + RotMatrix[k][m]) * s;

                Quat.members[0] = qt[0];
                Quat.members[1] = qt[1];
                Quat.members[2] = qt[2];
                Quat.members[3] = qt[3];
            }
            return Quat;
        }
    }

    public class FBSkeleton
    {
        public List<FBBone> Bones;
        public FBBone RootBone;
        public bool LocalTransform { get; set; }

        public FBSkeleton(EBX Ebx)
            : this(Ebx, "LocalPose")
        {
        }

        public FBSkeleton(EBX Ebx, string PoseNodeName)
        {
            LocalTransform = (PoseNodeName == "LocalPose");

            Bones = new List<FBBone>();

            string ebxXml = Ebx.ToXML().Replace("&", "_and_").Replace("$", "_dollar_");
            XDocument doc = XDocument.Parse(ebxXml);

            var skel = doc.Root.Descendants("SkeletonAsset");
            var skel2 = doc.Root.Element("SkeletonAsset");

            var boneNames = from bn in doc.Root.Descendants("BoneNames").Descendants("member")
                            select bn.Value;

            var hierarchy = from h in doc.Root.Descendants("Hierarchy").Descendants("member")
                            select h.Value;

            var localPoses = from lp in doc.Root.Descendants(PoseNodeName).Descendants("member").Descendants("LinearTransform")
                             select new
                             {
                                 right = new Vector(
                                     float.Parse(lp.Element("right").Element("Vec3").Element("x").Value.Trim().Replace("f", "")),
                                     float.Parse(lp.Element("right").Element("Vec3").Element("y").Value.Trim().Replace("f", "")),
                                     float.Parse(lp.Element("right").Element("Vec3").Element("z").Value.Trim().Replace("f", ""))),
                                 up = new Vector(
                                     float.Parse(lp.Element("up").Element("Vec3").Element("x").Value.Trim().Replace("f", "")),
                                     float.Parse(lp.Element("up").Element("Vec3").Element("y").Value.Trim().Replace("f", "")),
                                     float.Parse(lp.Element("up").Element("Vec3").Element("z").Value.Trim().Replace("f", ""))),
                                 forward = new Vector(
                                     float.Parse(lp.Element("forward").Element("Vec3").Element("x").Value.Trim().Replace("f", "")),
                                     float.Parse(lp.Element("forward").Element("Vec3").Element("y").Value.Trim().Replace("f", "")),
                                     float.Parse(lp.Element("forward").Element("Vec3").Element("z").Value.Trim().Replace("f", ""))),
                                 trans = new Vector(
                                     float.Parse(lp.Element("trans").Element("Vec3").Element("x").Value.Trim().Replace("f", "")),
                                     float.Parse(lp.Element("trans").Element("Vec3").Element("y").Value.Trim().Replace("f", "")),
                                     float.Parse(lp.Element("trans").Element("Vec3").Element("z").Value.Trim().Replace("f", "")))
                             };

            var zipped = boneNames.Zip(hierarchy, (b, p) => new { bn = b.Trim(), pi = p.Trim() })
                            .Zip(localPoses, (z, l) => new { bn = z.bn, pi = z.pi, lp = l });

            foreach (var z in zipped)
            {
                FBBone bone = new FBBone(z.bn);
                bone.ParentIndex = Convert.ToInt32(z.pi, 16);
                bone.Right = z.lp.right;
                bone.Forward = z.lp.forward;
                bone.Up = z.lp.up;
                bone.Location = z.lp.trans;
                Bones.Add(bone);
            }

            for (int i = 0; i < Bones.Count; i++)
            {
                Bones[i].Children = new List<FBBone>();
                for (int j = 0; j < Bones.Count; j++)
                {
                    if (Bones[j].ParentIndex == i)
                        Bones[i].Children.Add(Bones[j]);
                }

                if (Bones[i].ParentIndex == -1 && RootBone == null)
                    RootBone = Bones[i];
            }
        }
    }
}
