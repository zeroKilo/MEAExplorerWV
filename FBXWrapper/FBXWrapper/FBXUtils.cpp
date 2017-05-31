#include "stdafx.h"
#include <fbxsdk.h>
#include "FBXUtils.h"

#ifdef IOS_REF
#undef  IOS_REF
#define IOS_REF (*(pManager->GetIOSettings()))
#endif


void FBXWrapper::FBXUtils::InitializeSdkObjects(FbxManager*& pManager, FbxScene*& pScene)
{
	pManager = FbxManager::Create();
	if (!pManager)
	{
		FBXSDK_printf("Error: Unable to create FBX Manager!\n");
		exit(1);
	}
	else FBXSDK_printf("Autodesk FBX SDK version %s\n", pManager->GetVersion());
	FbxIOSettings* ios = FbxIOSettings::Create(pManager, IOSROOT);
	pManager->SetIOSettings(ios);
	FbxString lPath = FbxGetApplicationDirectory();
	pManager->LoadPluginsDirectory(lPath.Buffer());
	pScene = FbxScene::Create(pManager, "My Scene");
	if (!pScene)
	{
		FBXSDK_printf("Error: Unable to create FBX scene!\n");
		exit(1);
	}
}

void FBXWrapper::FBXUtils::DestroySdkObjects(FbxManager* pManager, bool pExitStatus)
{
	if (pManager) pManager->Destroy();
	if (pExitStatus) FBXSDK_printf("Program Success!\n");
}

bool FBXWrapper::FBXUtils::SaveScene(FbxManager* pManager, FbxDocument* pScene, const char* pFilename)
{
	return SaveScene(pManager, pScene, pFilename, 0, false);
}

bool FBXWrapper::FBXUtils::SaveScene(FbxManager* pManager, FbxDocument* pScene, const char* pFilename, int pFileFormat, bool pEmbedMedia)
{
	int lMajor, lMinor, lRevision;
	bool lStatus = true;
	FbxExporter* lExporter = FbxExporter::Create(pManager, "");
	if (pFileFormat < 0 || pFileFormat >= pManager->GetIOPluginRegistry()->GetWriterFormatCount())
	{
		pFileFormat = pManager->GetIOPluginRegistry()->GetNativeWriterFormat();
		int lFormatIndex, lFormatCount = pManager->GetIOPluginRegistry()->GetWriterFormatCount();
		for (lFormatIndex = 0; lFormatIndex<lFormatCount; lFormatIndex++)
		{
			if (pManager->GetIOPluginRegistry()->WriterIsFBX(lFormatIndex))
			{
				FbxString lDesc = pManager->GetIOPluginRegistry()->GetWriterFormatDescription(lFormatIndex);
				const char *lASCII = "ascii";
				if (lDesc.Find(lASCII) >= 0)
				{
					pFileFormat = lFormatIndex;
					break;
				}
			}
		}
	}
	IOS_REF.SetBoolProp(EXP_FBX_MATERIAL, true);
	IOS_REF.SetBoolProp(EXP_FBX_TEXTURE, true);
	IOS_REF.SetBoolProp(EXP_FBX_EMBEDDED, pEmbedMedia);
	IOS_REF.SetBoolProp(EXP_FBX_SHAPE, true);
	IOS_REF.SetBoolProp(EXP_FBX_GOBO, true);
	IOS_REF.SetBoolProp(EXP_FBX_ANIMATION, true);
	IOS_REF.SetBoolProp(EXP_FBX_GLOBAL_SETTINGS, true);
	if (lExporter->Initialize(pFilename, pFileFormat, pManager->GetIOSettings()) == false)
	{
		FBXSDK_printf("Call to FbxExporter::Initialize() failed.\n");
		FBXSDK_printf("Error returned: %s\n\n", lExporter->GetStatus().GetErrorString());
		return false;
	}
	FbxManager::GetFileFormatVersion(lMajor, lMinor, lRevision);
	FBXSDK_printf("FBX file format version %d.%d.%d\n\n", lMajor, lMinor, lRevision);
	lExporter->SetFileExportVersion("FBX201400");
	lStatus = lExporter->Export(pScene);
	lExporter->Destroy();
	return lStatus;
}

void FBXWrapper::FBXUtils::SetGlobalDefaultPosition(FbxNode* pNode, FbxAMatrix pGlobalPosition)
{
	FbxAMatrix lLocalPosition;
	FbxAMatrix lParentGlobalPosition;

	if (pNode->GetParent())
	{
		lParentGlobalPosition = GetGlobalDefaultPosition(pNode->GetParent());
		lLocalPosition = lParentGlobalPosition.Inverse() * pGlobalPosition;
	}
	else
	{
		lLocalPosition = pGlobalPosition;
	}

	pNode->LclTranslation.Set(lLocalPosition.GetT());
	pNode->LclRotation.Set(lLocalPosition.GetR());
	pNode->LclScaling.Set(lLocalPosition.GetS());
}

FbxAMatrix FBXWrapper::FBXUtils::GetGlobalDefaultPosition(FbxNode* pNode)
{
	FbxAMatrix lLocalPosition;
	FbxAMatrix lGlobalPosition;
	FbxAMatrix lParentGlobalPosition;

	lLocalPosition.SetT(pNode->LclTranslation.Get());
	lLocalPosition.SetR(pNode->LclRotation.Get());
	lLocalPosition.SetS(pNode->LclScaling.Get());

	if (pNode->GetParent())
	{
		lParentGlobalPosition = GetGlobalDefaultPosition(pNode->GetParent());
		lGlobalPosition = lParentGlobalPosition * lLocalPosition;
	}
	else
	{
		lGlobalPosition = lLocalPosition;
	}

	return lGlobalPosition;
}
