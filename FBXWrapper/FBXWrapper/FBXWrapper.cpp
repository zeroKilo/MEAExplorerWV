#include "stdafx.h"
#include <fbxsdk.h>
#include "FBXWrapper.h"
#include "FBXUtils.h"

#ifdef IOS_REF
#undef  IOS_REF
#define IOS_REF (*(pManager->GetIOSettings()))
#endif

using namespace System::Runtime::InteropServices;

FBXWrapper::FBXVector4::FBXVector4(double x, double y, double z, double w)
{
   vector = new FbxVector4(x, y, z, w);
}

FBXWrapper::FBXVector4::FBXVector4()
{
   vector = new FbxVector4(0, 0, 0, 0);
}

FBXWrapper::FBXVector4::FBXVector4(FbxVector4 v)
{
   vector = &v;
}

List<double>^ FBXWrapper::FBXVector4::ToList()
{
	List<double>^ result = gcnew List<double>();
	result->Add(vector->mData[0]);
	result->Add(vector->mData[1]);
	result->Add(vector->mData[2]);
	result->Add(vector->mData[3]);
	return result;
}



FBXWrapper::FBXAMatrix::FBXAMatrix()
{
   mat = new FbxAMatrix();
}

FBXWrapper::FBXAMatrix::FBXAMatrix(FbxAMatrix* m)
{
	mat = m;
}

FBXWrapper::FBXVector4^ FBXWrapper::FBXAMatrix::getR()
{
   return gcnew FBXVector4(mat->GetR());
}

void FBXWrapper::FBXAMatrix::SetRow(int Y, FBXVector4^ row)
{
   mat->SetRow(Y, *row->vector);
}

void FBXWrapper::FBXAMatrix::SetTRS(FBXVector4^ lT, FBXVector4^ lR, FBXVector4^ lS)
{
	mat->SetTRS(*lT->vector, *lR->vector, *lS->vector);
}



FBXWrapper::FBXCluster::FBXCluster(FbxCluster* c)
{
	cluster = c;
}

void FBXWrapper::FBXCluster::SetLink(Object^ node)
{
	cluster->SetLink(((FBXNode^)node)->node);
}

void FBXWrapper::FBXCluster::AddControlPointIndex(int idx, double weight)
{
	cluster->AddControlPointIndex(idx, weight);
}

void FBXWrapper::FBXCluster::SetTransformMatrix(FBXAMatrix^ mat)
{
	cluster->SetTransformMatrix(*mat->mat);
}

void FBXWrapper::FBXCluster::SetTransformLinkMatrix(FBXAMatrix^ mat)
{
	cluster->SetTransformLinkMatrix(*mat->mat);
}

FBXWrapper::FBXCluster^ FBXWrapper::FBXCluster::Create(Object^ scene, String^ name)
{
	FBXScene^ s = (FBXScene^)scene;
	IntPtr ptrToNativeString = Marshal::StringToHGlobalAnsi(name);
	const char* lMeshName = static_cast<char*>(ptrToNativeString.ToPointer());
	FBXCluster^ result = gcnew FBXCluster(FbxCluster::Create(s->scene, lMeshName));
	Marshal::FreeHGlobal(ptrToNativeString);
	return result;
}



FBXWrapper::FBXGeometryElementNormal::FBXGeometryElementNormal(FbxGeometryElementNormal* n)
{
	normal = n;
}

void FBXWrapper::FBXGeometryElementNormal::SetMappingMode(MappingMode mode)
{
	normal->SetMappingMode((FbxLayerElement::EMappingMode)mode);
}

void FBXWrapper::FBXGeometryElementNormal::Add(FBXVector4 ^v)
{
	normal->GetDirectArray().Add(*v->vector);
}



FBXWrapper::FBXGeometryElementBinormal::FBXGeometryElementBinormal(FbxGeometryElementBinormal* n)
{
	binormal = n;
}

void FBXWrapper::FBXGeometryElementBinormal::SetMappingMode(MappingMode mode)
{
	binormal->SetMappingMode((FbxLayerElement::EMappingMode)mode);
}

void FBXWrapper::FBXGeometryElementBinormal::Add(FBXVector4 ^v)
{
	binormal->GetDirectArray().Add(*v->vector);
}



FBXWrapper::FBXGeometryElementTangent::FBXGeometryElementTangent(FbxGeometryElementTangent* t)
{
	tangent = t;
}

void FBXWrapper::FBXGeometryElementTangent::SetMappingMode(MappingMode mode)
{
	tangent->SetMappingMode((FbxLayerElement::EMappingMode)mode);
}

void FBXWrapper::FBXGeometryElementTangent::Add(FBXVector4 ^v)
{
	tangent->GetDirectArray().Add(*v->vector);
}



FBXWrapper::FBXGeometryElementMaterial::FBXGeometryElementMaterial(FbxGeometryElementMaterial* m)
{
	mat = m;
}

void FBXWrapper::FBXGeometryElementMaterial::SetMappingMode(MappingMode mode)
{
	mat->SetMappingMode((FbxLayerElement::EMappingMode)mode);
}

void FBXWrapper::FBXGeometryElementMaterial::SetReferenceMode(ReferenceMode mode)
{
	mat->SetReferenceMode((FbxLayerElement::EReferenceMode)mode);
}



FBXWrapper::FBXGeometryElementUV::FBXGeometryElementUV(FbxGeometryElementUV* t)
{
	uv = t;
}

void FBXWrapper::FBXGeometryElementUV::SetMappingMode(MappingMode mode)
{
	uv->SetMappingMode((FbxLayerElement::EMappingMode)mode);
}

void FBXWrapper::FBXGeometryElementUV::SetReferenceMode(ReferenceMode mode)
{
	uv->SetReferenceMode((FbxLayerElement::EReferenceMode)mode);	
}

void FBXWrapper::FBXGeometryElementUV::SetIndexArrayCount(int count)
{
	uv->GetIndexArray().SetCount(count);
}

void FBXWrapper::FBXGeometryElementUV::Add(FBXVector4 ^v)
{
	uv->GetDirectArray().Add(*new FbxVector2(v->vector->mData[0], v->vector->mData[1]));
}

FBXWrapper::FBXNodeAttribute::FBXNodeAttribute(FbxNodeAttribute* a)
{
	attrib = a;
}

Object^ FBXWrapper::FBXNodeAttribute::ToGeometry()
{
	return gcnew FBXGeometry((FbxGeometry*)attrib);
}

FBXWrapper::NodeAttribType FBXWrapper::FBXNodeAttribute::GetAttributeType()
{
	return (NodeAttribType)attrib->GetAttributeType();
}


FBXWrapper::FBXNode::FBXNode(FbxNode* n)
{
	node = n;
}

void FBXWrapper::FBXNode::AddChild(FBXNode^ n)
{
	node->AddChild(n->node);
}

FBXWrapper::FBXNode^ FBXWrapper::FBXNode::FindChild(String^ name)
{
	IntPtr ptrToNativeString = Marshal::StringToHGlobalAnsi(name);
	const char* lBoneName = static_cast<char*>(ptrToNativeString.ToPointer());
	FBXNode^ result = gcnew FBXNode(node->FindChild(lBoneName));	
	Marshal::FreeHGlobal(ptrToNativeString);
	return result;
}

FBXWrapper::FBXNode^ FBXWrapper::FBXNode::GetParent()
{	
	return gcnew FBXNode(node->GetParent());
}

bool FBXWrapper::FBXNode::Equals(FBXNode^ n)
{
	return node == n->node;
}

void FBXWrapper::FBXNode::SetNodeAttribute(Object^ obj)
{
	if(obj->GetType() == FBXSkeleton::typeid)
	{
		FBXSkeleton^ s = (FBXSkeleton^)obj;
		node->SetNodeAttribute(s->skeleton);
	}
	if(obj->GetType() == FBXMesh::typeid)
	{
		FBXMesh^ m = (FBXMesh^)obj;
		node->SetNodeAttribute(m->mesh);
	}
}

void FBXWrapper::FBXNode::AddMaterial(Object^ scene, String^ name)
{
	FBXScene^ s = (FBXScene^)scene;
	IntPtr pStr = Marshal::StringToHGlobalAnsi(name);
	const char* lname = static_cast<char*>(pStr.ToPointer());
	FbxSurfacePhong* lMaterial = FbxSurfacePhong::Create(s->scene, lname);
	node->AddMaterial(lMaterial);
	Marshal::FreeHGlobal(pStr);
}

FBXWrapper::FBXAMatrix^ FBXWrapper::FBXNode::EvaluateGlobalTransform()
{
	return gcnew FBXAMatrix(&node->EvaluateGlobalTransform());
}

FBXWrapper::FBXNode^ FBXWrapper::FBXNode::Create(Object^ scene, String^ name)
{
	FBXScene^ s = (FBXScene^)scene;
	IntPtr ptrToNativeString = Marshal::StringToHGlobalAnsi(name);
	const char* lMeshName = static_cast<char*>(ptrToNativeString.ToPointer());
	FBXNode^ result = gcnew FBXNode(FbxNode::Create(s->scene, lMeshName));
	Marshal::FreeHGlobal(ptrToNativeString);
	return result;
}

FBXWrapper::FBXNodeAttribute^ FBXWrapper::FBXNode::GetNodeAttribute()
{
	return gcnew FBXNodeAttribute(node->GetNodeAttribute());
}

Object^ FBXWrapper::FBXNode::GetMesh()
{
	return gcnew FBXMesh(node->GetMesh());
}



FBXWrapper::FBXScene::FBXScene()
{
	scene = nullptr;
}

FBXWrapper::FBXNode^ FBXWrapper::FBXScene::GetRootNode()
{
	return gcnew FBXNode(scene->GetRootNode());
}

void FBXWrapper::FBXScene::AddPose(Object^ pose)
{
	FBXPose^ p = (FBXPose^)pose;
	scene->AddPose(p->pose);
}



FBXWrapper::FBXManager::FBXManager()
{
	manager = nullptr;
}



FBXWrapper::FBXMesh::FBXMesh(FbxMesh* m)
{
	mesh = m;
}

FBXWrapper::FBXMesh^ FBXWrapper::FBXMesh::Create(FBXScene^ scene, String^ name)
{
	IntPtr ptrToNativeString = Marshal::StringToHGlobalAnsi(name);
	const char* lMeshName = static_cast<char*>(ptrToNativeString.ToPointer());
	FBXMesh^ result = gcnew FBXMesh(FbxMesh::Create(scene->scene, lMeshName));
	Marshal::FreeHGlobal(ptrToNativeString);
	return result;
}

FBXWrapper::FBXGeometryElementNormal^ FBXWrapper::FBXMesh::CreateElementNormal()
{
	return gcnew FBXGeometryElementNormal(mesh->CreateElementNormal());
}

FBXWrapper::FBXGeometryElementBinormal^ FBXWrapper::FBXMesh::CreateElementBinormal()
{
	return gcnew FBXGeometryElementBinormal(mesh->CreateElementBinormal());
}

FBXWrapper::FBXGeometryElementTangent^ FBXWrapper::FBXMesh::CreateElementTangent()
{
	return gcnew FBXGeometryElementTangent(mesh->CreateElementTangent());
}

FBXWrapper::FBXGeometryElementMaterial^ FBXWrapper::FBXMesh::CreateElementMaterial()
{
	return gcnew FBXGeometryElementMaterial(mesh->CreateElementMaterial());
}

FBXWrapper::FBXGeometryElementUV^ FBXWrapper::FBXMesh::CreateElementUV(String^ MeshName)
{
	IntPtr ptrToNativeString = Marshal::StringToHGlobalAnsi(MeshName);
	const char* lMeshName = static_cast<char*>(ptrToNativeString.ToPointer());
	FBXGeometryElementUV^ result = gcnew FBXGeometryElementUV(mesh->CreateElementUV(lMeshName));
	Marshal::FreeHGlobal(ptrToNativeString);
	return result;
}

void FBXWrapper::FBXMesh::InitControlPoints(int count)
{
	mesh->InitControlPoints(count);
}

void FBXWrapper::FBXMesh::SetControlPoint(int n, FBXVector4^ v)
{
	mesh->mControlPoints[n] = *v->vector;
}

void FBXWrapper::FBXMesh::BeginPolygon()
{
	mesh->BeginPolygon();
}

void FBXWrapper::FBXMesh::BeginPolygon(int i)
{
	mesh->BeginPolygon(i);
}

void FBXWrapper::FBXMesh::EndPolygon()
{
	mesh->EndPolygon();
}

void FBXWrapper::FBXMesh::AddPolygon(int idx)
{
	mesh->AddPolygon(idx);
}

void FBXWrapper::FBXMesh::AddDeformer(Object^ shape)
{
	mesh->AddDeformer(((FBXBlendShape^)shape)->shape);
}

int FBXWrapper::FBXMesh::GetControlPointsCount()
{
	return mesh->GetControlPointsCount();
}



FBXWrapper::FBXSkeleton::FBXSkeleton(FbxSkeleton* skel)
{
	skeleton = skel;
}

FBXWrapper::FBXSkeleton^ FBXWrapper::FBXSkeleton::Create(FBXScene^ scene, String^ name)
{
	IntPtr pStr = Marshal::StringToHGlobalAnsi(name);
	const char* str = static_cast<char*>(pStr.ToPointer());
	FBXWrapper::FBXSkeleton^ result = gcnew FBXSkeleton(FbxSkeleton::Create(scene->scene, str));
	Marshal::FreeHGlobal(pStr);	
	return result;
}

void FBXWrapper::FBXSkeleton::SetSkeletonType(SkelType type)
{
	skeleton->SetSkeletonType((FbxSkeleton::EType)type);
}

void FBXWrapper::FBXSkeleton::SetSize(double d)
{
	skeleton->Size.Set(d);
}



FBXWrapper::FBXSkin::FBXSkin(FbxSkin* s)
{
	skin = s;
}

void FBXWrapper::FBXSkin::AddCluster(FBXCluster^ cluster)
{
	skin->AddCluster(cluster->cluster);
}

int FBXWrapper::FBXSkin::GetClusterCount()
{
	return skin->GetClusterCount();
}

FBXWrapper::FBXNode^ FBXWrapper::FBXSkin::GetClusterLink(int i)
{
	return gcnew FBXNode(skin->GetCluster(i)->GetLink());
}

FBXWrapper::FBXSkin^ FBXWrapper::FBXSkin::Create(FBXScene^ scene, String^ name)
{
	IntPtr pStr = Marshal::StringToHGlobalAnsi(name);
	const char* str = static_cast<char*>(pStr.ToPointer());
	FBXWrapper::FBXSkin^ result = gcnew FBXSkin(FbxSkin::Create(scene->scene, str));
	Marshal::FreeHGlobal(pStr);
	return result;
}



FBXWrapper::FBXGeometry::FBXGeometry(FbxGeometry* g)
{
	geo = g;
}

void FBXWrapper::FBXGeometry::AddDeformer(FBXSkin^ skin)
{
	geo->AddDeformer(skin->skin);
}

int FBXWrapper::FBXGeometry::GetDeformerCount(EDeformerType type)
{
	return geo->GetDeformerCount((FbxDeformer::EDeformerType)type);
}

FBXWrapper::FBXSkin^ FBXWrapper::FBXGeometry::GetSkinDeformer(int n)
{	
	FbxDeformer* d = geo->GetDeformer(n, (FbxDeformer::EDeformerType)EDeformerType::eSkin, 0);
	return gcnew FBXSkin((FbxSkin*)d);
}



FBXWrapper::FBXPose::FBXPose(FbxPose* p)
{
	pose = p;
}

FBXWrapper::FBXPose^ FBXWrapper::FBXPose::Create(FBXScene^ scene, String^ name)
{
	IntPtr pStr = Marshal::StringToHGlobalAnsi(name);
	const char* str = static_cast<char*>(pStr.ToPointer());
	FBXPose^ result = gcnew FBXPose(FbxPose::Create(scene->scene, str));
	Marshal::FreeHGlobal(pStr);
	return result;
}

void FBXWrapper::FBXPose::SetIsBindPose(bool b)
{
	pose->SetIsBindPose(b);
}

void FBXWrapper::FBXPose::Add(FBXNode^ n, FBXAMatrix^ mat)
{
	pose->Add(n->node, *mat->mat);
}



FBXWrapper::FBXShape::FBXShape(FbxShape* s)
{
	shape = s;
}

void FBXWrapper::FBXShape::InitControlPoints(int n)
{
	shape->InitControlPoints(n);
}

FBXWrapper::FBXShape^ FBXWrapper::FBXShape::Create(FBXScene^ scene, String^ name)
{
	IntPtr pStr = Marshal::StringToHGlobalAnsi(name);
	const char* str = static_cast<char*>(pStr.ToPointer());
	FBXShape^ result = gcnew FBXShape(FbxShape::Create(scene->scene, str));
	Marshal::FreeHGlobal(pStr);
	return result;
}

List<FBXWrapper::FBXVector4^>^ FBXWrapper::FBXShape::GetControlPoints()
{
	List<FBXVector4^>^ result = gcnew List<FBXVector4^>();
	int count = shape->mControlPoints.GetCount();
	for (int i = 0; i < count; i++)
		result->Add(gcnew FBXVector4(shape->mControlPoints[i]));
	return result;
}



FBXWrapper::FBXBlendShape::FBXBlendShape(FbxBlendShape* s)
{
	shape = s;
}

FBXWrapper::FBXBlendShape^ FBXWrapper::FBXBlendShape::Create(FBXScene^ scene, String^ name)
{
	IntPtr pStr = Marshal::StringToHGlobalAnsi(name);
	const char* str = static_cast<char*>(pStr.ToPointer());
	FBXBlendShape^ result = gcnew FBXBlendShape(FbxBlendShape::Create(scene->scene, str));
	Marshal::FreeHGlobal(pStr);
	return result;
}

void FBXWrapper::FBXBlendShape::AddBlendShapeChannel(Object^ c)
{
	shape->AddBlendShapeChannel(((FBXBlendShapeChannel^)c)->channel);
}



FBXWrapper::FBXBlendShapeChannel::FBXBlendShapeChannel(FbxBlendShapeChannel* c)
{
	channel = c;
}

FBXWrapper::FBXBlendShapeChannel^ FBXWrapper::FBXBlendShapeChannel::Create(FBXScene^ scene, String^ name)
{
	IntPtr pStr = Marshal::StringToHGlobalAnsi(name);
	const char* str = static_cast<char*>(pStr.ToPointer());
	FBXBlendShapeChannel^ result = gcnew FBXBlendShapeChannel(FbxBlendShapeChannel::Create(scene->scene, str));
	Marshal::FreeHGlobal(pStr);
	return result;
}

void FBXWrapper::FBXBlendShapeChannel::AddTargetShape(FBXShape^ shape)
{
	channel->AddTargetShape(shape->shape);
}



void FBXWrapper::FBXHelper::InitializeSdkObjects(FBXManager^ manager, FBXScene^ scene)
{
	FbxManager* m = manager->manager;
	FbxScene* s = scene->scene;
	FBXUtils::InitializeSdkObjects(m, s);
	manager->manager = m;
	scene->scene = s;
}

void FBXWrapper::FBXHelper::DestroySdkObjects(FBXManager^ manager, bool result)
{
	FbxManager* m = manager->manager;
	FBXUtils::DestroySdkObjects(m, result);
	manager->manager = m;
}

bool FBXWrapper::FBXHelper::SaveScene(FBXManager^ manager, FBXScene^ scene, String^ targetDir)
{
	return SaveScene(manager, scene, targetDir, 0, false);
}

bool FBXWrapper::FBXHelper::SaveScene(FBXManager^ manager, FBXScene^ scene, String^ targetDir, int pFileFormat, bool pEmbedMedia)
{
	IntPtr pStr = Marshal::StringToHGlobalAnsi(targetDir);
	const char* str = static_cast<char*>(pStr.ToPointer());
	bool result = FBXUtils::SaveScene(manager->manager, scene->scene, str);
	Marshal::FreeHGlobal(pStr);
	return result;
}

void FBXWrapper::FBXHelper::SetGlobalDefaultPosition(FBXNode^ n, FBXAMatrix^ m)
{
	FBXUtils::SetGlobalDefaultPosition(n->node, *m->mat);
}