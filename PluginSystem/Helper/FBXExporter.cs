using System;
using System.IO;
using System.Collections.Generic;
using FBXWrapper;

namespace PluginSystem
{
    public class FBXExporter : ASkinnedMeshExporter, IMeshExporter
    {
        public FBXExporter(SkeletonAsset skel) : base(skel) { }

        public void ExportLod(MeshAsset mesh, int lodIndex, string targetFile)
        {
            ExportMesh(mesh, lodIndex, targetFile);
        }

        public void ExportAllLods(MeshAsset mesh, string targetdir)
        {
            for (int i = 0; i < mesh.lods.Count; i++)
            {
                string targetFile = Path.Combine(targetdir, mesh.lods[i].shortName + ".fbx");
                ExportMesh(mesh, i, targetFile);
            }
        }

        public void ExportMesh(MeshAsset mesh, int lodIndex, string targetFile)
        {
            if (Skeleton != null)
            {
                ExportSkinnedMeshToFBX(Skeleton, mesh, lodIndex, targetFile, true);
            }
            else
            {
                ExportStaticMeshToFBX(mesh, lodIndex, targetFile, true);
            }
        }

        public bool ExportStaticMeshToFBX(MeshAsset mesh, int lodIndex, String targetdir, bool asOne)
        {
            FBXManager lSdkManager = new FBXManager();
            FBXScene lScene = new FBXScene();
            FBXHelper.InitializeSdkObjects(lSdkManager, lScene);
            FBXNode SceneRoot = lScene.GetRootNode();
            if (asOne)
            {
                FBXNode fbxMesh = CreateFbxMesh(mesh.lods[lodIndex], lScene);
                SceneRoot.AddChild(fbxMesh);
            }
            else
            {
                for (int i = 0; i < mesh.lods[lodIndex].sections.Count; i++)
                {
                    FBXNode CurrentSectionMesh = CreateFbxMesh(mesh.lods[lodIndex].sections[i], lScene);
                    SceneRoot.AddChild(CurrentSectionMesh);
                }
            }
            return SaveScene(targetdir, lSdkManager, lScene);
        }

        public bool ExportSkinnedMeshToFBX(SkeletonAsset skeleton, MeshAsset mesh, int lodIndex, String targetdir)
        {
            return ExportSkinnedMeshToFBX(skeleton, mesh, lodIndex, targetdir, false);
        }

        public bool ExportSkinnedMeshToFBX(SkeletonAsset skeleton, MeshAsset mesh, int lodIndex, String targetdir, bool asOne)
        {
            FBXManager lSdkManager = new FBXManager();
            FBXScene lScene = new FBXScene();
            FBXHelper.InitializeSdkObjects(lSdkManager, lScene);
            FBXNode SceneRoot = lScene.GetRootNode();
            FBXNode lSkeletonRoot = CreateFbxSkeleton(skeleton, lScene);
            SceneRoot.AddChild(lSkeletonRoot);
            if (asOne)
            {
                FBXNode fbxMesh = CreateFbxMesh(mesh.lods[lodIndex], lScene);
                CreateMeshSkinning(skeleton, mesh.lods[lodIndex], fbxMesh, lSkeletonRoot, lScene);
                SceneRoot.AddChild(fbxMesh);
                StoreRestPose(lScene, lSkeletonRoot, skeleton);
            }
            else
            {
                for (int i = 0; i < mesh.lods[lodIndex].sections.Count; i++)
                {
                    FBXNode CurrentSectionMesh = CreateFbxMesh(mesh.lods[lodIndex].sections[i], lScene);
                    CreateMeshSkinning(skeleton, mesh.lods[lodIndex].sections[i], CurrentSectionMesh, lSkeletonRoot, lScene);
                    SceneRoot.AddChild(CurrentSectionMesh);
                    StoreBindPose(lScene, CurrentSectionMesh);
                }
            }
            return SaveScene(targetdir, lSdkManager, lScene);
        }

        public bool ExportMeshWithMorph(SkeletonAsset Skeleton, MeshAsset mesh, int lodIndex, List<Vector> morphVertex, List<Vector> morphBones, String targetdir)
        {
            FBXManager lSdkManager = new FBXManager();
            FBXScene lScene = new FBXScene();
            FBXHelper.InitializeSdkObjects(lSdkManager, lScene);
            FBXNode SceneRoot = lScene.GetRootNode();
            FBXNode fbxMesh = CreateFbxMesh(mesh.lods[lodIndex], lScene);
            AddMorphToMesh(lScene, fbxMesh, morphVertex);
            SceneRoot.AddChild(fbxMesh);
            if (Skeleton != null)
            {
                FBXNode lSkeletonRoot = CreateFbxSkeleton(Skeleton, lScene);
                CreateMeshSkinning(Skeleton, mesh.lods[lodIndex], fbxMesh, lSkeletonRoot, lScene);
                UpdateSkeletonWithMorph(Skeleton, lSkeletonRoot, morphBones);
                SceneRoot.AddChild(lSkeletonRoot);
                StoreBindPose(lScene, fbxMesh);
            }
            return SaveScene(targetdir, lSdkManager, lScene);
        }


        private bool SaveScene(String targetdir, FBXManager pSdkManager, FBXScene pScene)
        {
            bool lResult = FBXHelper.SaveScene(pSdkManager, pScene, targetdir);
            FBXHelper.DestroySdkObjects(pSdkManager, lResult);
            return lResult;
        }

        private FBXNode CreateFbxMesh(MeshLodSection section, FBXScene pScene)
        {
            FBXMesh fbxMesh = FBXMesh.Create(pScene, section.matName);
            FBXGeometryElementNormal lGeometryElementNormal = fbxMesh.CreateElementNormal();
            lGeometryElementNormal.SetMappingMode(FBXWrapper.MappingMode.eByControlPoint);
            FBXGeometryElementBinormal lGeometryElementBiNormal = fbxMesh.CreateElementBinormal();
            lGeometryElementBiNormal.SetMappingMode(FBXWrapper.MappingMode.eByControlPoint);
            FBXGeometryElementTangent lGeometryElementTangent = fbxMesh.CreateElementTangent();
            lGeometryElementTangent.SetMappingMode(FBXWrapper.MappingMode.eByControlPoint);
            fbxMesh.InitControlPoints(section.vertices.Count);
            FBXGeometryElementMaterial lMaterialElement = fbxMesh.CreateElementMaterial();
            lMaterialElement.SetMappingMode(FBXWrapper.MappingMode.eByPolygon);
            lMaterialElement.SetReferenceMode(FBXWrapper.ReferenceMode.eIndexToDirect);
            FBXGeometryElementUV lUVDiffuseElement = fbxMesh.CreateElementUV(section.matName);
            lUVDiffuseElement.SetMappingMode(FBXWrapper.MappingMode.eByControlPoint);
            lUVDiffuseElement.SetReferenceMode(FBXWrapper.ReferenceMode.eIndexToDirect);
            lUVDiffuseElement.SetIndexArrayCount(section.vertices.Count);
            for (int j = 0; j < section.vertices.Count; j++)
            {
                FBXVector4 position = new FBXVector4(section.vertices[j].position.members[0], section.vertices[j].position.members[1], section.vertices[j].position.members[2], 0);
                FBXVector4 normal = new FBXVector4(section.vertices[j].normals.members[0], section.vertices[j].normals.members[1], section.vertices[j].normals.members[2], section.vertices[j].normals.members[3]);
                FBXVector4 textCoords = new FBXVector4(section.vertices[j].texCoords.members[0], (-section.vertices[j].texCoords.members[1] + 1), 0, 0);
                FBXVector4 bitangent = new FBXVector4(section.vertices[j].biTangents.members[0], section.vertices[j].biTangents.members[1], section.vertices[j].biTangents.members[2], section.vertices[j].biTangents.members[3]);
                FBXVector4 tangent = new FBXVector4(section.vertices[j].tangents.members[0], section.vertices[j].tangents.members[1], section.vertices[j].tangents.members[2], section.vertices[j].tangents.members[3]);
                fbxMesh.SetControlPoint(j, position);
                lGeometryElementNormal.Add(normal);
                lGeometryElementBiNormal.Add(bitangent);
                lGeometryElementTangent.Add(tangent);
                lUVDiffuseElement.Add(textCoords);
            }

            for (int j = 0; j < section.indicies.Count; j++)
            {
                if (j % 3 == 0)
                {
                    fbxMesh.EndPolygon();
                    fbxMesh.BeginPolygon();
                }
                fbxMesh.AddPolygon(section.indicies[j]);
            }
            fbxMesh.EndPolygon();
            FBXNode lMeshNode = FBXNode.Create(pScene, section.matName);
            lMeshNode.SetNodeAttribute(fbxMesh);
            lMeshNode.AddMaterial(pScene, section.matName);
            return lMeshNode;
        }

        private FBXNode CreateFbxMesh(MeshLOD lod, FBXScene pScene)
        {
            FBXMesh fbxMesh = FBXMesh.Create(pScene, lod.shortName);
            FBXNode lMeshNode = FBXNode.Create(pScene, lod.shortName);
            lMeshNode.SetNodeAttribute(fbxMesh);
            FBXGeometryElementNormal lGeometryElementNormal = fbxMesh.CreateElementNormal();
            lGeometryElementNormal.SetMappingMode(FBXWrapper.MappingMode.eByControlPoint);
            FBXGeometryElementBinormal lGeometryElementBiNormal = fbxMesh.CreateElementBinormal();
            lGeometryElementBiNormal.SetMappingMode(FBXWrapper.MappingMode.eByControlPoint);
            FBXGeometryElementTangent lGeometryElementTangent = fbxMesh.CreateElementTangent();
            lGeometryElementTangent.SetMappingMode(FBXWrapper.MappingMode.eByControlPoint);
            FBXGeometryElementMaterial lMaterialElement = fbxMesh.CreateElementMaterial();
            lMaterialElement.SetMappingMode(FBXWrapper.MappingMode.eByPolygon);
            lMaterialElement.SetReferenceMode(FBXWrapper.ReferenceMode.eIndexToDirect);
            int verticesCount = lod.GetLODTotalVertCount();
            fbxMesh.InitControlPoints(verticesCount);
            List<FBXGeometryElementUV> UVs = new List<FBXGeometryElementUV>();
            for (int i = 0; i < lod.sections.Count; i++)
            {
                MeshLodSection section = lod.sections[i];
                FBXGeometryElementUV lUVDiffuseElement = fbxMesh.CreateElementUV(section.matName);
                lUVDiffuseElement.SetMappingMode(FBXWrapper.MappingMode.eByControlPoint);
                lUVDiffuseElement.SetReferenceMode(FBXWrapper.ReferenceMode.eDirect);
                UVs.Add(lUVDiffuseElement);
            }
            int VertexOffset = 0;
            for (int i = 0; i < lod.sections.Count; i++)
            {
                MeshLodSection section = lod.sections[i];
                for (int j = 0; j < section.vertices.Count; j++)
                {
                    FBXVector4 position = new FBXVector4(section.vertices[j].position.members[0], section.vertices[j].position.members[1], section.vertices[j].position.members[2], 0);
                    FBXVector4 normal = new FBXVector4(section.vertices[j].normals.members[0], section.vertices[j].normals.members[1], section.vertices[j].normals.members[2], section.vertices[j].normals.members[3]);
                    FBXVector4 textCoords = new FBXVector4(section.vertices[j].texCoords.members[0], (-section.vertices[j].texCoords.members[1] + 1), 0, 0);
                    FBXVector4 bitangent = new FBXVector4(section.vertices[j].biTangents.members[0], section.vertices[j].biTangents.members[1], section.vertices[j].biTangents.members[2], section.vertices[j].biTangents.members[3]);
                    FBXVector4 tangent = new FBXVector4(section.vertices[j].tangents.members[0], section.vertices[j].tangents.members[1], section.vertices[j].tangents.members[2], section.vertices[j].tangents.members[3]);
                    fbxMesh.SetControlPoint(VertexOffset + j, position);
                    lGeometryElementNormal.Add(normal);
                    lGeometryElementBiNormal.Add(bitangent);
                    lGeometryElementTangent.Add(tangent);
                    int uvI = 0;
                    foreach (FBXGeometryElementUV uv in UVs)
                    {
                        if (uvI == i)
                            uv.Add(textCoords);
                        else
                            uv.Add(new FBXVector4(0, 0, 0, 0));
                        uvI++;
                    }
                }
                for (int j = 0; j < section.indicies.Count; j++)
                {
                    if (j % 3 == 0)
                    {
                        fbxMesh.EndPolygon();
                        fbxMesh.BeginPolygon(i);
                    }
                    fbxMesh.AddPolygon(VertexOffset + section.indicies[j]);
                }
                fbxMesh.EndPolygon();
                VertexOffset = VertexOffset + section.vertices.Count;
                lMeshNode.AddMaterial(pScene, section.matName);
            }
            return lMeshNode;
        }

        private FBXNode CreateFbxSkeleton(SkeletonAsset Skeleton, FBXScene pScene)
        {
            FBXSkeleton lSkeletonRootAttribute = FBXSkeleton.Create(pScene, "Skeleton");
            lSkeletonRootAttribute.SetSkeletonType(FBXWrapper.SkelType.eRoot);
            FBXNode lSkeletonRoot = FBXNode.Create(pScene, "Skeleton");
            lSkeletonRoot.SetNodeAttribute(lSkeletonRootAttribute);
            lSkeletonRoot.LclTranslation = new FBXVector4().ToList();
            FBXNode FbxSkeletonRootNode = CreateFbxBone(Skeleton.RootBone, pScene, lSkeletonRoot);
            lSkeletonRoot.AddChild(FbxSkeletonRootNode);
            SetBoneTransform(Skeleton, lSkeletonRoot);
            return lSkeletonRoot;
        }

        private FBXNode CreateFbxBone(FBBone bone, FBXScene pScene, FBXNode parent)
        {
            FBXSkeleton lSkeletonLimbNodeAttribute1 = FBXSkeleton.Create(pScene, bone.Name);
            lSkeletonLimbNodeAttribute1.SetSkeletonType(FBXWrapper.SkelType.eLimbNode);
            lSkeletonLimbNodeAttribute1.SetSize(1.0);
            FBXNode lSkeletonLimbNode1 = FBXNode.Create(pScene, bone.Name);
            lSkeletonLimbNode1.SetNodeAttribute(lSkeletonLimbNodeAttribute1);
            for (int i = 0; i < bone.Children.Count; i++)
            {
                FBBone childBone = bone.Children[i];
                FBXNode fbxChildBone = CreateFbxBone(childBone, pScene, lSkeletonLimbNode1);
                lSkeletonLimbNode1.AddChild(fbxChildBone);
            }
            return lSkeletonLimbNode1;
        }

        private FBXVector4 CalculateBoneRotation(FBBone bone)
        {
            FBXAMatrix fbxrotMat = new FBXAMatrix();
            FBXVector4 Forward = new FBXVector4(bone.Forward.members[0], bone.Forward.members[1], bone.Forward.members[2], 0);
            FBXVector4 Right = new FBXVector4(bone.Right.members[0], bone.Right.members[1], bone.Right.members[2], 0);
            FBXVector4 Up = new FBXVector4(bone.Up.members[0], bone.Up.members[1], bone.Up.members[2], 0);
            fbxrotMat.SetRow(0, Right);
            fbxrotMat.SetRow(1, Up);
            fbxrotMat.SetRow(2, Forward);
            fbxrotMat.SetRow(3, new FBXVector4(0, 0, 0, 1));
            FBXVector4 boneRot = fbxrotMat.getR();
            return boneRot;
        }

        private void CreateMeshSkinning(SkeletonAsset Skeleton, MeshLOD lod, FBXNode pFbxMesh, FBXNode pSkeletonRoot, FBXScene pScene)
        {
            Dictionary<string, List<VertexGroup.VertexWeight>> vg = VertexGroup.GetVertexGroups(Skeleton, lod);
            CreateMeshSkinning(vg, pFbxMesh, pSkeletonRoot, pScene);
        }

        private void CreateMeshSkinning(SkeletonAsset Skeleton, MeshLodSection section, FBXNode pFbxMesh, FBXNode pSkeletonRoot, FBXScene pScene)
        {
            Dictionary<string, List<VertexGroup.VertexWeight>> vg = VertexGroup.GetVertexGroups(Skeleton, section);
            CreateMeshSkinning(vg, pFbxMesh, pSkeletonRoot, pScene);
        }

        private void CreateMeshSkinning(Dictionary<string, List<VertexGroup.VertexWeight>> vg, FBXNode pFbxMesh, FBXNode pSkeletonRoot, FBXScene pScene)
        {
            FBXSkin lFbxSkin = FBXSkin.Create(pScene, "");
            FBXAMatrix lMeshMatTransform = pFbxMesh.EvaluateGlobalTransform();
            foreach (string key in vg.Keys)
            {
                List<VertexGroup.VertexWeight> bvg = vg[key];
                FBXCluster lCluster = FBXCluster.Create(pScene, key);
                FBXNode lFbxBone = pSkeletonRoot.FindChild(key);
                if (lFbxBone != null)
                {
                    lCluster.SetLink(lFbxBone);
                    foreach (VertexGroup.VertexWeight v in bvg)
                        lCluster.AddControlPointIndex(v.VertexIndex, v.Weight);
                    lFbxSkin.AddCluster(lCluster);
                    lCluster.SetTransformMatrix(lMeshMatTransform);
                    FBXAMatrix lBoneMatTransform = lFbxBone.EvaluateGlobalTransform();
                    lCluster.SetTransformLinkMatrix(lBoneMatTransform);
                }
            }

            FBXGeometry lFbxMeshAtt = (FBXGeometry)pFbxMesh.GetNodeAttribute().ToGeometry();
            if (lFbxMeshAtt != null)
                lFbxMeshAtt.AddDeformer(lFbxSkin);
        }

        private void UpdateSkeletonWithMorph(SkeletonAsset Skeleton, FBXNode pSkeletonNode, List<Vector> morphBones)
        {
            for (int i = 0; i < Skeleton.Bones.Count; i++)
            {
                FBBone fbbone = Skeleton.Bones[i];
                FBXNode fbxBone = pSkeletonNode.FindChild(fbbone.Name);
                Vector boneOffset = morphBones[i];
                List<double> tmp = fbxBone.LclTranslation;
                tmp[0] += boneOffset.members[0];
                tmp[1] += boneOffset.members[1];
                tmp[2] += boneOffset.members[2];
                fbxBone.LclTranslation = tmp;
            }
        }

        private void StoreBindPose(FBXScene pScene, FBXNode pMesh)
        {
            List<FBXNode> lClusteredFbxNodes = new List<FBXNode>();
            int i, j;
            FBXNodeAttribute att = pMesh.GetNodeAttribute();
            if (pMesh != null && att != null)
            {
                int lSkinCount = 0;
                int lClusterCount = 0;
                switch (pMesh.GetNodeAttribute().GetAttributeType())
                {
                    default:
                        break;
                    case NodeAttribType.eMesh:
                    case NodeAttribType.eNurbs:
                    case NodeAttribType.ePatch:
                        FBXGeometry geo = (FBXGeometry)att.ToGeometry();
                        lSkinCount = geo.GetDeformerCount(EDeformerType.eSkin);
                        for (i = 0; i < lSkinCount; ++i)
                        {
                            FBXSkin lSkin = geo.GetSkinDeformer(i);
                            lClusterCount += lSkin.GetClusterCount();
                        }
                        break;
                }
                if (lClusterCount != 0)
                {
                    for (i = 0; i < lSkinCount; ++i)
                    {

                        FBXGeometry geo = (FBXGeometry)att.ToGeometry();
                        FBXSkin lSkin = geo.GetSkinDeformer(i);
                        lClusterCount = lSkin.GetClusterCount();
                        for (j = 0; j < lClusterCount; ++j)
                        {
                            FBXNode lClusterNode = lSkin.GetClusterLink(j);
                            AddNodeRecursively(lClusteredFbxNodes, lClusterNode);
                        }
                    }
                    lClusteredFbxNodes.Add(pMesh);
                }
            }
            if (lClusteredFbxNodes.Count != 0)
            {
                FBXPose lPose = FBXPose.Create(pScene, "pose");
                lPose.SetIsBindPose(true);
                for (i = 0; i < lClusteredFbxNodes.Count; i++)
                {
                    FBXNode lKFbxNode = lClusteredFbxNodes[i];
                    FBXAMatrix lBindMatrix = lKFbxNode.EvaluateGlobalTransform();
                    lPose.Add(lKFbxNode, lBindMatrix);
                }
                pScene.AddPose(lPose);
            }
        }

        private void StoreRestPose(FBXScene pScene, FBXNode pSkeletonRoot, SkeletonAsset Skeleton)
        {
            Dictionary<String, FBBone> pose = Skeleton.ModelBones;
            FBXAMatrix lTransformMatrix = new FBXAMatrix();
            FBXVector4 lT, lR, lS;
            lS = new FBXVector4(1.0, 1.0, 1.0, 0);
            FBXPose lPose = FBXPose.Create(pScene, "A Bind Pose");
            lT = new FBXVector4();
            lR = new FBXVector4();
            lTransformMatrix.SetTRS(lT, lR, lS);
            FBXNode lKFbxNode = pSkeletonRoot;
            lPose.Add(lKFbxNode, lTransformMatrix);
            foreach (string key in pose.Keys)
            {
                FBBone bonePose = pose[key];
                FBXNode fbxBone = pSkeletonRoot.FindChild(key);
                FBXVector4 Forward = new FBXVector4(bonePose.Forward.members[0], bonePose.Forward.members[1], bonePose.Forward.members[2], 0);
                FBXVector4 Right = new FBXVector4(bonePose.Right.members[0], bonePose.Right.members[1], bonePose.Right.members[2], 0);
                FBXVector4 Up = new FBXVector4(bonePose.Up.members[0], bonePose.Up.members[1], bonePose.Up.members[2], 0);
                FBXVector4 Trans = new FBXVector4(bonePose.Location.members[0], bonePose.Location.members[1], bonePose.Location.members[2], 1);
                FBXAMatrix boneTransform = new FBXAMatrix();
                boneTransform.SetRow(0, Right);
                boneTransform.SetRow(1, Up);
                boneTransform.SetRow(2, Forward);
                boneTransform.SetRow(3, Trans);
                lPose.Add(fbxBone, boneTransform);
            }
            lPose.SetIsBindPose(true);
            pScene.AddPose(lPose);
        }

        private void SetBoneTransform(FBBone bone, FBXNode fbxBoneNode)
        {
            List<double> tmp = new List<double>();
            tmp.Add(bone.Location.members[0]);
            tmp.Add(bone.Location.members[1]);
            tmp.Add(bone.Location.members[2]);
            fbxBoneNode.LclTranslation = tmp;

            FBXVector4 rot = CalculateBoneRotation(bone);
            tmp = new List<double>();
            tmp.Add(rot.X);
            tmp.Add(rot.Y);
            tmp.Add(rot.Z);
            fbxBoneNode.LclRotation = tmp;
        }

        private void SetBoneTransform(SkeletonAsset Skeleton, FBXNode pSkeletonRoot)
        {
            Dictionary<String, FBBone> pose = Skeleton.ModelBones;
            foreach (string key in pose.Keys)
            {
                FBBone bonePose = pose[key];
                FBXNode fbxBone = pSkeletonRoot.FindChild(key);
                FBXVector4 ForwardM = new FBXVector4(bonePose.Forward.members[0], bonePose.Forward.members[1], bonePose.Forward.members[2], 0);
                FBXVector4 RightM = new FBXVector4(bonePose.Right.members[0], bonePose.Right.members[1], bonePose.Right.members[2], 0);
                FBXVector4 UpM = new FBXVector4(bonePose.Up.members[0], bonePose.Up.members[1], bonePose.Up.members[2], 0);
                FBXVector4 TransM = new FBXVector4(bonePose.Location.members[0], bonePose.Location.members[1], bonePose.Location.members[2], 1);
                FBXAMatrix transfoMatrix = new FBXAMatrix();
                transfoMatrix.SetRow(0, RightM);
                transfoMatrix.SetRow(1, UpM);
                transfoMatrix.SetRow(2, ForwardM);
                transfoMatrix.SetRow(3, TransM);
                FBXHelper.SetGlobalDefaultPosition(fbxBone, transfoMatrix);
            }
        }

        private void AddNodeRecursively(List<FBXNode> pNodeArray, FBXNode pNode)
        {
            if (pNode != null)
            {
                AddNodeRecursively(pNodeArray, pNode.GetParent());
                bool found = false;
                foreach (FBXNode n in pNodeArray)
                    if (n.Equals(pNode))
                    {
                        found = true;
                        break;
                    }
                if (found)
                    pNodeArray.Add(pNode);
            }
        }

        private void AddMorphToMesh(FBXScene pScene, FBXNode pFbxNode, List<Vector> morph)
        {
            FBXShape lShape = FBXShape.Create(pScene, "MorphShape");
            FBXMesh mesh = (FBXMesh)pFbxNode.GetMesh();
            int count = mesh.GetControlPointsCount();
            lShape.InitControlPoints(count);
            List<FBXVector4> lControlPoints = lShape.GetControlPoints();

            for (int i = 0; i < count; i++)
            {
                FBXVector4 cp = new FBXVector4(morph[i].members[0], morph[i].members[1], morph[i].members[2], 0);
                lControlPoints[i] = cp;
            }

            FBXBlendShape lBlendShape = FBXBlendShape.Create(pScene, "morph");
            FBXBlendShapeChannel lBlendShapeChannel = FBXBlendShapeChannel.Create(pScene, "morphchannel");
            mesh.AddDeformer(lBlendShape);
            lBlendShape.AddBlendShapeChannel(lBlendShapeChannel);
            lBlendShapeChannel.AddTargetShape(lShape);
        }
    }
}
