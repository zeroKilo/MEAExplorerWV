#pragma once
#include "stdafx.h"
#include <fbxsdk.h>

namespace FBXWrapper {

	public ref class FBXUtils
	{
	public:	
		static void InitializeSdkObjects(FbxManager*& pManager, FbxScene*& pScene);
		static void DestroySdkObjects(FbxManager* pManager, bool pExitStatus);
		static bool SaveScene(FbxManager* pManager, FbxDocument* pScene, const char* pFilename, int pFileFormat, bool pEmbedMedia);
		static bool SaveScene(FbxManager* pManager, FbxDocument* pScene, const char* pFilename);
		static void SetGlobalDefaultPosition(FbxNode* pNode, FbxAMatrix pGlobalPosition);
		static FbxAMatrix GetGlobalDefaultPosition(FbxNode* pNode);
	};
}