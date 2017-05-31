// FBXWrapper.h

#pragma once

#include "stdafx.h"
#include <fbxsdk.h>
#include "FBXWrapper.h"
#include "FBXUtils.h"

using namespace System;
using namespace System::Collections::Generic;

namespace FBXWrapper {	
	public enum class MappingMode
	{
		eNone,
		eByControlPoint,
		eByPolygonVertex,
		eByPolygon,
		eByEdge,
		eAllSame
	};

	public enum class ReferenceMode
	{
		eDirect,
		eIndex,
		eIndexToDirect
	};

	public enum class SkelType
	{
		eRoot,
		eLimb,	
		eLimbNode,
		eEffector
	};

	public enum class NodeAttribType
	{
		eUnknown,
		eNull,
		eMarker,
		eSkeleton,
		eMesh,
		eNurbs,
		ePatch,
		eCamera,
		eCameraStereo,
		eCameraSwitcher,
		eLight,
		eOpticalReference,
		eOpticalMarker,
		eNurbsCurve,
		eTrimNurbsSurface,
		eBoundary,
		eNurbsSurface,
		eShape,
		eLODGroup,
		eSubDiv,
		eCachedEffect,
		eLine
	};

	public enum class EDeformerType
	{
		eUnknown,
		eSkin,
		eBlendShape,
		eVertexCache
	};

	public ref class FBXVector4
	{
		public:
			property double X 
			{ 
				double get()
				{
					return vector->mData[0];
				};
				void set(double f)
				{
					vector->mData[0] = f;
				}
			};
			property double Y 
			{ 
				double get()
				{
					return vector->mData[1];
				};
				void set(double f)
				{
					vector->mData[1] = f;
				}
			};
			property double Z 
			{ 
				double get()
				{
					return vector->mData[3];
				};
				void set(double f)
				{
					vector->mData[3] = f;
				}
			};         
			property double W
			{ 
				double get()
				{
					return vector->mData[3];
				};
				void set(double f)
				{
					vector->mData[3] = f;
				}
			};
			FBXVector4();
			FBXVector4(double x, double y, double z, double w);
			FBXVector4(FbxVector4 v);
			FbxVector4* vector;
			List<double>^ ToList();
	};

	public ref class FBXAMatrix
	{
		public:
			FbxAMatrix* mat;
			FBXAMatrix();
			FBXAMatrix(FbxAMatrix* m);
			FBXVector4^ getR();
			void SetRow(int Y, FBXVector4^ row);
			void SetTRS(FBXVector4^ lT, FBXVector4^ lR, FBXVector4^ lS);
	};

	public ref class FBXCluster
	{
		public:
			FbxCluster* cluster;
			FBXCluster(FbxCluster* c);
			void SetLink(Object^ node);
			void AddControlPointIndex(int idx, double weight);
			void SetTransformMatrix(FBXAMatrix^ mat);
			void SetTransformLinkMatrix(FBXAMatrix^ mat);
			static FBXCluster^ Create(Object^ scene, String^ name);
	};

	public ref class FBXGeometryElementNormal
	{
		public:
			FbxGeometryElementNormal* normal;
			FBXGeometryElementNormal(FbxGeometryElementNormal* n);
			void SetMappingMode(MappingMode mode);
			void Add(FBXVector4 ^v);
	};

	public ref class FBXGeometryElementBinormal
	{
		public:
			FbxGeometryElementBinormal* binormal;
			FBXGeometryElementBinormal(FbxGeometryElementBinormal* n);
			void SetMappingMode(MappingMode mode);
			void Add(FBXVector4 ^v);
	};

	public ref class FBXGeometryElementTangent
	{
		public:
			FbxGeometryElementTangent* tangent;
			FBXGeometryElementTangent(FbxGeometryElementTangent* n);
			void SetMappingMode(MappingMode mode);
			void Add(FBXVector4 ^v);
	};

	public ref class FBXGeometryElementMaterial
	{
		public:
			FbxGeometryElementMaterial* mat;
			FBXGeometryElementMaterial(FbxGeometryElementMaterial* m);
			void SetMappingMode(MappingMode mode);
			void SetReferenceMode(ReferenceMode mode);
	};

	public ref class FBXGeometryElementUV
	{
		public:
			FbxGeometryElementUV* uv;
			FBXGeometryElementUV(FbxGeometryElementUV* t);
			void SetMappingMode(MappingMode mode);
			void SetReferenceMode(ReferenceMode mode);
			void SetIndexArrayCount(int count);
			void Add(FBXVector4 ^v);
	};

	public ref class FBXNodeAttribute
	{
		public: 
			FbxNodeAttribute* attrib;
			FBXNodeAttribute(FbxNodeAttribute* a);
			NodeAttribType GetAttributeType();
			Object^ ToGeometry();
	};

	public ref class FBXNode
	{
		public:
			FbxNode* node;
			property List<double>^ LclTranslation
			{
				List<double>^ get()
				{
					List<double>^ result = gcnew List<double>();
					FbxDouble3 tmp = node->LclTranslation.Get();
					result->Add(tmp.mData[0]);
					result->Add(tmp.mData[1]);
					result->Add(tmp.mData[2]);
					return result;
				}
				void set(List<double>^ l)
				{
					FbxDouble3 tmp;
					tmp.mData[0] = l[0];
					tmp.mData[1] = l[1];
					tmp.mData[2] = l[2];
					node->LclTranslation.Set(tmp);
				}
			}
			property List<double>^ LclRotation
			{
				List<double>^ get()
				{
					List<double>^ result = gcnew List<double>();
					FbxDouble3 tmp = node->LclRotation.Get();
					result->Add(tmp.mData[0]);
					result->Add(tmp.mData[1]);
					result->Add(tmp.mData[2]);
					return result;
				}
				void set(List<double>^ l)
				{
					FbxDouble3 tmp;
					tmp.mData[0] = l[0];
					tmp.mData[1] = l[1];
					tmp.mData[2] = l[2];
					node->LclRotation.Set(tmp);
				}
			}

			FBXNode(FbxNode* n);

			static FBXNode^ Create(Object^ scene, String^ name);
			void AddChild(FBXNode^ node);
			FBXNode^ FindChild(String^ name);
			FBXNode^ GetParent();
			void SetNodeAttribute(Object^ obj);
			void AddMaterial(Object^ scene, String^ name);
			FBXAMatrix^ EvaluateGlobalTransform();
			FBXNodeAttribute^ GetNodeAttribute();
			bool Equals(FBXNode^ n);
			Object^ GetMesh();
	};

	public ref class FBXManager
	{
		public:
			FbxManager* manager;
			FBXManager();
	};

	public ref class FBXScene
	{
		public:
			FbxScene* scene;
			FBXScene();
			FBXNode^ GetRootNode();
			void AddPose(Object^ pose);
	};

	public ref class FBXMesh
	{
		public:
			FbxMesh* mesh;

			FBXMesh(FbxMesh* m);

			static FBXMesh^ Create(FBXScene^ scene, String^ name);
			FBXGeometryElementNormal^ CreateElementNormal();
			FBXGeometryElementBinormal^ CreateElementBinormal();
			FBXGeometryElementTangent^ CreateElementTangent();
			FBXGeometryElementMaterial^ CreateElementMaterial();
			FBXGeometryElementUV^ CreateElementUV(String^ MeshName);
			void InitControlPoints(int count);
			void SetControlPoint(int n, FBXVector4^ v);
			void BeginPolygon();
			void BeginPolygon(int i);
			void EndPolygon();
			void AddPolygon(int idx);
			void AddDeformer(Object^ shape);
			int GetControlPointsCount();
	};

	public ref class FBXSkeleton
	{
		public:
			FbxSkeleton* skeleton;
			FBXSkeleton(FbxSkeleton* skel);
			void SetSkeletonType(SkelType type);
			void SetSize(double d);
			static FBXSkeleton^ Create(FBXScene^ scene, String^ name);
	};

	public ref class FBXSkin
	{
		public:
			FbxSkin* skin;
			FBXSkin(FbxSkin* s);
			void AddCluster(FBXCluster^ c);
			int GetClusterCount();
			FBXNode^ GetClusterLink(int i);
			static FBXSkin^ Create(FBXScene^ scene, String^ name);
	};

	public ref class FBXGeometry
	{
		public:
			FbxGeometry* geo;
			FBXGeometry(FbxGeometry* g);
			void AddDeformer(FBXSkin^ skin);
			int GetDeformerCount(EDeformerType type);
			FBXSkin^ GetSkinDeformer(int n);
	};

	public ref class FBXPose
	{
		public:
			FbxPose* pose;
			FBXPose(FbxPose* p);
			void SetIsBindPose(bool b);
			void Add(FBXNode^ n, FBXAMatrix^ mat);
			static FBXPose^ Create(FBXScene^ scene, String^ name);
	};

	public ref class FBXShape
	{
		public:
			FbxShape* shape;
			FBXShape(FbxShape* s);
			void InitControlPoints(int n);
			List<FBXVector4^>^ GetControlPoints();
			static FBXShape^ Create(FBXScene^ scene, String^ name);
	};

	public ref class FBXBlendShape
	{
		public:
			FbxBlendShape* shape;
			FBXBlendShape(FbxBlendShape* s);
			void AddBlendShapeChannel(Object^ c);
			static FBXBlendShape^ Create(FBXScene^ scene, String^ name);
	};

	public ref class FBXBlendShapeChannel
	{
		public:
			FbxBlendShapeChannel* channel;
			FBXBlendShapeChannel(FbxBlendShapeChannel* c);
			void AddTargetShape(FBXShape^ shape);
			static FBXBlendShapeChannel^ Create(FBXScene^ scene, String^ name);
	};

	public ref class FBXHelper
	{
		public:
			FBXUtils FbxUtils;
			static void InitializeSdkObjects(FBXManager^ manager, FBXScene^ scene);
			static void DestroySdkObjects(FBXManager^ manager, bool result);
			static bool SaveScene(FBXManager^ manager, FBXScene^ scene, String^ targetDir);
			static bool SaveScene(FBXManager^ manager, FBXScene^ scene, String^ targetDir, int pFileFormat, bool pEmbedMedia);
			static void SetGlobalDefaultPosition(FBXNode^ n, FBXAMatrix^ m);
	};
}
