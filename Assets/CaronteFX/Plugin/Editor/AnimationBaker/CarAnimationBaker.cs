// ***********************************************************
//	Copyright 2016 Next Limit Technologies, http://www.nextlimit.com
//	All rights reserved.
//
//	THIS SOFTWARE IS PROVIDED 'AS IS' AND WITHOUT ANY EXPRESS OR
//	IMPLIED WARRANTIES, INCLUDING, WITHOUT LIMITATION, THE IMPLIED
//	WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.
//
// ***********************************************************

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CaronteFX.AnimationFlags;
using CaronteSharp;

namespace CaronteFX
{
  using TGOFrameData = Tuple3<uint, Transform, CarGOKeyframe>;

  /// <summary>
  /// Used to bake Caronte Simulations.
  /// </summary>
  public class CarAnimationBaker
  { 
    public enum EAnimationFileType
    {
      CRAnimationAsset,
      TextAsset
    }

    public enum EVertexCompressionMode
    {
      Box,
      Fiber
    }

    public enum EBakeMode
    {
      BakeToNew,
      BakeToExisting
    }

    //-----------------------------------------------------------------------------------
    //--------------------------------DATA MEMBERS---------------------------------------
    //-----------------------------------------------------------------------------------
    const int binaryVersion_ = 9;
    
    CarManager       manager_;
    CarEntityManager entityManager_;
    CarPlayer        player_;

    CarMeshCombiner skinnedMeshCombiner_;
    CarMeshCombiner frameMeshCombiner_;

    UnityEngine.Object meshesPrefab_;
    String             assetsPath_;
    String             fxName_;

    List< Tuple2<CNBody, bool> > listBodyNodeToBake_ = new List< Tuple2<CNBody, bool> >();
    
    Dictionary<uint, string>     idBodyToRelativePath_;
    Dictionary<uint, string[]>   idBodyToBonesRelativePaths_;
    Dictionary<uint, GameObject> idBodyToBakedGO_;
    Dictionary<uint, uint>       idBodyToFirstAppearanceFrame_;

    Dictionary<uint, CarGOKeyframe> idBodyToGOKeyframe_;
    Dictionary<uint, Vector2>       idBodyToVisibilityInterval_;

    Dictionary< GameObject, List<uint> > bakedSkinnedGOToListIdBody_;
    List< Bounds > listBakedSkinnedGOLocalBounds_;

    Dictionary<Mesh, Mesh> originalMeshToBakedMesh_;

    List<uint> listIdBodyTmp_;
    List<uint> listIdBodyTmp2_;

    List<GameObject> listGOTmp_;

    List<Transform>  listTransformTmp_;
    List<BoneWeight> listBoneWeightTmp_;
    List<Matrix4x4>  listBindPoseTmp_;

    HashSet<uint> setBoneBodies_;
    HashSet<uint> setVisibleBodies_;
    HashSet<uint> setBakedBodies_;

    uint currentBakeFrame_;

    long estimatedAnimationBytesSize_ = 0;
    const long maxAnimationBytesSize  = (long)( int.MaxValue - (1024*1024) ); //2GB Aprox. - Max Animation size.

    MeshComplex meshComplexForUpdate_ = new MeshComplex();

    int frame_;
    int bakingFrame_;

    int frameCount_;

    int frameStart_ = -1;
    int frameEnd_   = -1;

    float visibilityShift_    = 0;
    float visibilityRangeMin_ = 0;
    float visibilityRangeMax_ = 0;

    int nEstimatedSkinnedObjects_ = 0;

    private CRAnimation rootCRAnimation_;
    private CarAnimationPersistence animationPersistence_;

    private EAnimationFileType animationFileType_ = EAnimationFileType.TextAsset;
    public EAnimationFileType BakeAnimationFileType
    {
      get { return animationFileType_; }
      set { animationFileType_ = value; }
    }

    private bool bakeOverExisting_;
    public bool BakeOverExisting
    {
      get { return bakeOverExisting_; }
      set { bakeOverExisting_ = value; }
    }

    private string animationName_;
    public string AnimationName
    {
      get { return animationName_; }
      set { animationName_ = value;  }
    }
   
    private string bakeObjectName_;
    public string BakeObjectName
    {
      get { return bakeObjectName_; }
      set { bakeObjectName_ = value; }
    }

    private string bakeObjectPrefix_;
    public string BakeObjectPrefix
    {
      get { return bakeObjectPrefix_; }
      set { bakeObjectPrefix_ = value; }
    }

    private bool skinRopes_ = true;
    public bool SkinRopes
    {
      get { return skinRopes_; }
      set { skinRopes_ = value; }
    }

    private bool skinClothes_ = false;
    public bool SkinClothes
    {
      get { return skinClothes_; }
      set { skinClothes_ = value; }
    }

    private bool vertexCompression_ = true;
    public bool VertexCompression
    {
      get { return vertexCompression_; }
      set { vertexCompression_ = value; }
    }

    private EVertexCompressionMode vertexCompressionMode_ = 0;
    public EVertexCompressionMode VertexCompressionMode
    {
      get { return vertexCompressionMode_; }
      set { vertexCompressionMode_ = value; }
    }

    private EBakeMode bakeMode_ = EBakeMode.BakeToNew;
    public EBakeMode BakeMode
    {
      get { return bakeMode_; }
      set { bakeMode_ = value; }
    }

    private bool vertexTangents_ = false;
    public bool VertexTangents
    {
      get { return vertexTangents_; }
      set { vertexTangents_ = value; }
    }

    private bool bakeEvents_ = true;
    public bool BakeEvents
    {
      get { return bakeEvents_; }
      set { bakeEvents_ = value; }
    }

    private bool bakeVisibility_ = true;
    public bool BakeVisibility
    {
      get { return bakeEvents_; }
      set { bakeEvents_ = value; }
    }

    private bool alignData_ = true;
    public bool AlignData
    {
      get { return alignData_; }
      set { alignData_ = value; }
    }

    private bool bakeAllNodes_ = true;
    public bool BakeAllNodes
    {
      get { return bakeAllNodes_; }
      set { bakeAllNodes_ = value; }
    }

    public bool combineMeshesInFrame_ = false;
    public bool CombineMeshesInFrame
    {
      get { return combineMeshesInFrame_; }
      set { combineMeshesInFrame_ = value; }
    }

    private Transform optionalRootTransform_ = null;
    public Transform OptionalRootTransform
    {
      get { return optionalRootTransform_; }
      set { optionalRootTransform_ = value; }
    }

    private bool preserveCFXComponentsInFrameBake_ = false;
    public bool PreserveCFXComponentsInFrameBake
    {
      get { return preserveCFXComponentsInFrameBake_; }
      set { preserveCFXComponentsInFrameBake_ = value; }
    }

    private List<CNBody> listBodyNode_ = new List<CNBody>();
    public List<CNBody> ListBodyNode
    {
      get { return listBodyNode_; }
    }

    private BitArray bitArrNeedsBaking_;
    public BitArray BitArrNeedsBaking
    {
      get { return bitArrNeedsBaking_; }
    }

    private BitArray bitArrNeedsCollapsing_;
    public BitArray BitArrNeedsCollapsing
    {
      get { return bitArrNeedsCollapsing_; }
    }

    private GameObject rootGameObject_;
    public GameObject RootGameObject
    {
      get { return rootGameObject_; }
      set { rootGameObject_ = value; }
    }

    private CRAnimation existingBake_;
    public CRAnimation ExistingBake
    {
      get { return existingBake_; }
      set { existingBake_ = value; }
    }

    private bool removeInvisibleBodiesFromBake_ = true;
    public bool RemoveInvisibleBodiesFromBake
    {
      get { return removeInvisibleBodiesFromBake_; }
      set { removeInvisibleBodiesFromBake_ = value; }
    }

    const int nLocationComponents = 3;
    const int nRotationComponents = 4;
    const int nScaleComponents    = 3;

    const int nFloatBytes  = sizeof(float);
    const int nUInt16Bytes = sizeof(UInt16);
    const int nSByteBytes   = sizeof(sbyte);
    
    const int nBoxComponents = 6;
    const int nPositionComponents = 3;
    const int nNormalComponents   = 3;
    const int nTangentComponents  = 4;

    const int posVertexBytes = nPositionComponents * nFloatBytes;
    const int norVertexBytes = nNormalComponents * nFloatBytes;
    const int tanVertexBytes = nTangentComponents * nFloatBytes;

    const int boxSizeBytes = nBoxComponents * nFloatBytes;
    const int boxPosVertexBytes = nPositionComponents * nUInt16Bytes;
    const int boxNorVertexBytes = nNormalComponents * nSByteBytes;
    const int boxTanVertexBytes = nTangentComponents * nSByteBytes;

    const int fiberPosVertexBytes = 3 * nSByteBytes;

    const int boxBytes = nBoxComponents * nFloatBytes;
    
    const int rqBytes   = (nLocationComponents + nRotationComponents) * nFloatBytes;
    const int boneBytes = (nLocationComponents + nRotationComponents + nScaleComponents) * nFloatBytes;   

    //-----------------------------------------------------------------------------------
    public int FrameStart
    {
      get { return frameStart_; }
      set { frameStart_ = Mathf.Clamp(value, 0, player_.MaxFrames); }
    }

    public int FrameEnd
    {
      get { return frameEnd_; }
      set { frameEnd_ = Mathf.Clamp(value, 0, player_.MaxFrames); }
    }

    public int MaxFrames
    {
      get { return ( player_.MaxFrames ); }
    }

    public float FrameTime
    {
      get { return ( player_.FrameTime );  }
    }

    public int FPS
    {
      get { return ( player_.FPS ); }
    }

    private UnityEngine.Object MeshesPrefab
    {
      get
      {
        if (meshesPrefab_ == null)
        {
          string meshesPrefabPath = AssetDatabase.GenerateUniqueAssetPath(assetsPath_ + "/" + BakeObjectName +  "_meshes.prefab");
          meshesPrefab_ = PrefabUtility.CreateEmptyPrefab(meshesPrefabPath);
        }
        return meshesPrefab_;
      }
    }
    //----------------------------------------------------------------------------------
    public CarAnimationBaker( CarManager manager, CarEntityManager entityManager, CarPlayer player )
    {
      manager_       = manager;
      entityManager_ = entityManager;
      player_        = player;

      skinnedMeshCombiner_ = new CarMeshCombiner(true);
      frameMeshCombiner_   = new CarMeshCombiner(false);

      meshesPrefab_ = null;
      assetsPath_   = string.Empty;

      fxName_ = string.Empty;

      idBodyToRelativePath_         = new Dictionary<uint, string>();
      idBodyToBonesRelativePaths_   = new Dictionary<uint, string[]>();
      idBodyToBakedGO_              = new Dictionary<uint, GameObject>();
      idBodyToFirstAppearanceFrame_ = new Dictionary<uint, uint>();

      idBodyToGOKeyframe_         = new Dictionary<uint, CarGOKeyframe>();
      idBodyToVisibilityInterval_ = new Dictionary<uint, Vector2>();

      bakedSkinnedGOToListIdBody_    = new Dictionary<GameObject, List<uint>>();
      listBakedSkinnedGOLocalBounds_ = new List<Bounds>();

      originalMeshToBakedMesh_ = new Dictionary<Mesh,Mesh>();

      listIdBodyTmp_   = new List<uint>();
      listIdBodyTmp2_  = new List<uint>();
      listGOTmp_       = new List<GameObject>();

      listTransformTmp_  = new List<Transform>();
      listBoneWeightTmp_ = new List<BoneWeight>();
      listBindPoseTmp_   = new List<Matrix4x4>();

      setBoneBodies_       = new HashSet<uint>();
      setVisibleBodies_    = new HashSet<uint>();
      setBakedBodies_ = new HashSet<uint>();

      preserveCFXComponentsInFrameBake_ = false;
    }
    //----------------------------------------------------------------------------------
    public void ClearData()
    {
      skinnedMeshCombiner_.Clear();
      frameMeshCombiner_  .Clear();
      listBodyNodeToBake_.Clear();

      meshesPrefab_ = null;
      assetsPath_   = string.Empty;
      fxName_       = string.Empty;

      idBodyToRelativePath_        .Clear();
      idBodyToBonesRelativePaths_  .Clear();
      idBodyToBakedGO_             .Clear();
      idBodyToFirstAppearanceFrame_.Clear();

      idBodyToGOKeyframe_        .Clear();
      idBodyToVisibilityInterval_.Clear();

      bakedSkinnedGOToListIdBody_.Clear();
      listBakedSkinnedGOLocalBounds_.Clear();

      originalMeshToBakedMesh_.Clear();

      listIdBodyTmp_    .Clear();
      listIdBodyTmp_    .Clear();
      listGOTmp_        .Clear();

      listTransformTmp_ .Clear();
      listBoneWeightTmp_.Clear();
      listBindPoseTmp_  .Clear();

      setBoneBodies_   .Clear();
      setVisibleBodies_.Clear();
      setBakedBodies_.Clear();

      estimatedAnimationBytesSize_ = 0;
      nEstimatedSkinnedObjects_ = 0;
    }
    //----------------------------------------------------------------------------------
    public void BuildBakerInitData()
    {
      string fxDataName = manager_.FxData.name;

      animationName_    = fxDataName + "_anim";
      bakeObjectName_   = fxDataName + "_baked";
      bakeObjectPrefix_ = "b_";

      frameStart_ = 0;
      frameEnd_   = player_.MaxFrames;

      BuildListNodesForBaking();
    }
    //----------------------------------------------------------------------------------
    private void BuildListNodesForBaking()
    {
      manager_.GetListBodyNodesForBake( listBodyNode_ );

      int nBodyNodes = listBodyNode_.Count;

      bitArrNeedsBaking_     = new BitArray(nBodyNodes, true);
      bitArrNeedsCollapsing_ = new BitArray(nBodyNodes, true);

      for (int i = 0; i < nBodyNodes; i++ )
      {
        CNBody bodyNode = listBodyNode_[i];
        int nBodies = entityManager_.GetNumberOfBodiesFromBodyNode(bodyNode);

        bool isRigidbody = bodyNode is CNRigidbody;
        bool isAnimated  = bodyNode is CNAnimatedbody;

        bitArrNeedsBaking_[i]     = !isAnimated;
        bitArrNeedsCollapsing_[i] = ( (nBodies >= 5) && (isRigidbody && !isAnimated) );
      }  
    }
    //----------------------------------------------------------------------------------
    private void CalculateListBodyNodeToBake()
    {
      int nBodies = listBodyNode_.Count;
      for (int i = 0; i < nBodies; i++)
      {
        CNBody bodyNode = listBodyNode_[i];

        bool needsBake       = bitArrNeedsBaking_[i];
        bool needsCollapsing = bitArrNeedsCollapsing_[i];

        if (needsBake)
        {
          entityManager_.BuildListBodyIdFromBodyNode( bodyNode, listIdBodyTmp_ );
          foreach( uint idBody in listIdBodyTmp_)
          {
            setBakedBodies_.Add(idBody);
          }

          if ( bodyNode is CNRigidbody )
          {
            listBodyNodeToBake_.Add( Tuple2.New(bodyNode, needsCollapsing) );
          }
          else
          {
            listBodyNodeToBake_.Add( Tuple2.New(bodyNode, false) );
          }

          if ( needsCollapsing ||
               (bodyNode is CNRope && skinRopes_) ||
               (bodyNode is CNCloth && skinClothes_) )
          {
            nEstimatedSkinnedObjects_++;
          }
        }
      }
    }
    //----------------------------------------------------------------------------------
    public void BakeSimulationAsAnim()
    {
      ClearData();
      CalculateListBodyNodeToBake();

      fxName_ = manager_.FxData.name;
      frameCount_ = (frameEnd_ - frameStart_ + 1);  
        
      visibilityShift_    = FrameTime * frameStart_;
      visibilityRangeMin_ = 0f;
      visibilityRangeMax_ = (frameCount_ - 1) * FrameTime + (FrameTime * 0.001f); 
         
      string ext = (BakeAnimationFileType == EAnimationFileType.TextAsset) ? "bytes" : "asset";
      string animPath = EditorUtility.SaveFilePanelInProject("CaronteFX - Bake simulation", fxName_ + "_anim", ext, "Select animation file name...");

      if (animPath != string.Empty)
      {
        animationName_ = Path.GetFileNameWithoutExtension(animPath);
        assetsPath_    = Path.GetDirectoryName(animPath);

        InitBake(); 
        bool fitsInAnimationFile = CheckBodiesVisibilityEstimateAnimationSize();  
        if (fitsInAnimationFile)
        {
          CreateRootGameObject(bakeObjectName_, false);
          bool creationOk = CreateBakedObjects();  
          if (!creationOk)
          {
            FinishBake();
            return;
          }
          InitKeyframeData(); 
          BakeAnimBinaryFile();
          SetStartData(); 
        }
        FinishBake(); 
      }
    }
    //----------------------------------------------------------------------------------
    public void BakeCurrentFrame()
    {
      ClearData();
      CalculateListBodyNodeToBake();

      fxName_ = manager_.FxData.name + "_frame_" + player_.Frame;
      bool assetsPath = CarFileUtils.SaveFolderPanelInProject("Frame assets folder...", out assetsPath_);
      if (!assetsPath)
      {
        return;
      }    

      InitBake();
      CreateRootGameObject(bakeObjectName_ + "_frame_" + player_.Frame, true);
      CreateFrameBakedObjects();     
      FinishBake();
    }
    //----------------------------------------------------------------------------------
    private void InitBake()
    {
      SimulationManager.ClearBroadcast( UN_BROADCAST_MODE.BAKING );  
      SimulationManager.PauseHotOn();
    }
    //----------------------------------------------------------------------------------
    private void FinishBake()
    {      
      SimulationManager.PauseOn();
      SimulationManager.ClearBroadcast( UN_BROADCAST_MODE.SIMULATING_OR_REPLAYING );
      if (rootGameObject_ != null)
      {
        EditorGUIUtility.PingObject(rootGameObject_);
        Selection.activeGameObject = rootGameObject_;
      }
    }      
    //----------------------------------------------------------------------------------
    private void CreateRootGameObject(string bakeObjectName, bool isFrameBake)
    {
      bool useExistingBake = (BakeMode == EBakeMode.BakeToExisting) && (existingBake_ != null) && !isFrameBake;
      if ( useExistingBake )
      {
        rootGameObject_  = existingBake_.gameObject;
        rootCRAnimation_ = existingBake_;
      }
      else
      {
        string uniqueName = CarEditorUtils.GetUniqueGameObjectName(bakeObjectName);
        rootGameObject_ = new GameObject(uniqueName);

        if (!isFrameBake)
        {
          rootCRAnimation_ = rootGameObject_.AddComponent<CRAnimation>();
          if (optionalRootTransform_ != null)
          {
            Transform rootTr = rootGameObject_.transform;
            rootTr.localPosition = optionalRootTransform_.position;
            rootTr.localRotation = optionalRootTransform_.rotation;
            rootTr.localScale    = Vector3.one;
          }
        }
      }

      if (rootCRAnimation_ != null)
      {
        animationPersistence_ = rootCRAnimation_.AnimationPersistence;
        if (animationPersistence_ == null)
        {
          rootCRAnimation_.AnimationPersistence = CarAnimationUtils.CreateAnimationPersistence(rootGameObject_);
          animationPersistence_ = rootCRAnimation_.AnimationPersistence;
          EditorUtility.SetDirty(rootCRAnimation_);
        }
      }
    }
    //----------------------------------------------------------------------------------
    private bool CreateBakedObjects()
    {
      int nBodyNodes = listBodyNodeToBake_.Count;

      listIdBodyTmp_.Clear();  

      for (int i = 0; i < nBodyNodes; i++)
      {
        Tuple2<CNBody, bool> tupleBodyNodeNeedsCollapsing_ = listBodyNodeToBake_[i]; 

        CNBody bodyNode   = tupleBodyNodeNeedsCollapsing_.First;
        bool collapseNode = tupleBodyNodeNeedsCollapsing_.Second;

        bool isRope  = bodyNode as CNRope;
        bool isCloth = bodyNode as CNCloth;

        bool isSkinnedRope  = isRope && skinRopes_;
        bool isSkinnedCloth = isCloth && skinClothes_;       

        entityManager_.BuildListBodyIdFromBodyNode( bodyNode, listIdBodyTmp_ );

        if ( BakeMode == EBakeMode.BakeToExisting )
        {
          AssignExistingBodiesOfBodyNode(listIdBodyTmp_, isSkinnedRope || isSkinnedCloth);

          if (listIdBodyTmp_.Count != 0)
          {
            EditorUtility.DisplayDialog("CaronteFX - Bake into existing GameObject", "The simulation won't be baked. \n\nBake into existing GameObject requires baking the same bodies as the existing bake GameObject.\n\nPlease, make sure you are baking the same bodies or use the new bake GameObject mode.\n", "Ok");
            return false;
          }
        }

        if (listIdBodyTmp_.Count == 0)
        {
          continue;
        }

        string bodyNodeName = CarEditorUtils.GetUniqueChildGameObjectName(rootGameObject_, BakeObjectPrefix + bodyNode.Name);

        GameObject nodeGO = new GameObject(bodyNodeName);
        nodeGO.transform.parent = rootGameObject_.transform;
 

        if (collapseNode)
        {
          CreateRigidSkinnedGameObjects(bodyNodeName, bodyNode, listIdBodyTmp_, nodeGO);
        }
        else
        {
          if (isSkinnedRope)
          {
            CreateRopeSkinnedGameObjects(bodyNodeName, bodyNode, listIdBodyTmp_, nodeGO);
          }
          else if (isSkinnedCloth)
          {
            CreateClothSkinnedGameObjects(bodyNodeName, bodyNode, listIdBodyTmp_, nodeGO);
          }
          else
          {
            CreateBakedGameObjects(bodyNodeName, bodyNode, listIdBodyTmp_, nodeGO);
          }
        }
      }

      EditorUtility.SetDirty(animationPersistence_);
      return true;
    }
    //----------------------------------------------------------------------------------
    private void AssignExistingBodiesOfBodyNode(List<uint> listBodyId, bool isSkinnedMemento)
    {
      List<string> listBoneRelativePath = new List<string>();

      int nBodies = listBodyId.Count;
      for (int i = nBodies - 1; i >= 0; i--)
      {
        uint idBody = listBodyId[i];
        GameObject originalGO = entityManager_.GetGOFromIdBody(idBody);

        if ( !setVisibleBodies_.Contains(idBody) || originalGO == null ) 
        {
          listBodyId.RemoveAt(i);
          continue;
        }

        CarMementoOriginal carMementoOriginal = new CarMementoOriginal(originalGO, isSkinnedMemento);
        if (animationPersistence_.HasGameObjectBakeMemento(carMementoOriginal))
        {
          CarMementoTarget mementoTarget = animationPersistence_.GetGameObjectBakeMemento(carMementoOriginal);
          
          Transform bakedTr = rootGameObject_.transform.Find(mementoTarget.TargetPath);        
          if (bakedTr != null)
          {
            GameObject bakedGO = bakedTr.gameObject;

            MeshRenderer mrn = bakedGO.GetComponent<MeshRenderer>();
            bool hasMeshRenderer = (mrn != null);

            SkinnedMeshRenderer smr = bakedGO.GetComponent<SkinnedMeshRenderer>();
            bool hasSkinnedMeshRenderer = (smr != null);

            Renderer rn = bakedGO.GetComponent<Renderer>();
            bool hasRenderer = (rn != null);

            bool isRope  = entityManager_.IsRope(idBody);
            bool isCloth = entityManager_.IsCloth(idBody);

            //Rg skinned 
            if (!hasRenderer)
            {
              idBodyToBakedGO_     .Add(idBody, bakedGO);
              idBodyToRelativePath_.Add(idBody, mementoTarget.TargetPath);
   
              string skinnedRelativePath = mementoTarget.TargetPathSkinnedGO;
              if (skinnedRelativePath != null)
              {
                Transform skinnedTr = rootGameObject_.transform.Find(skinnedRelativePath);
                if (skinnedTr != null)
                {
                  GameObject skinnedGO = skinnedTr.gameObject;
                  if (!bakedSkinnedGOToListIdBody_.ContainsKey(skinnedGO))
                  {
                    bakedSkinnedGOToListIdBody_.Add(skinnedGO, new List<uint>());
                  }

                  bakedSkinnedGOToListIdBody_[skinnedGO].Add(idBody);     
                }
              }

              setBoneBodies_.Add(idBody);
              listBodyId.RemoveAt(i);
            }
            else if (hasMeshRenderer)
            {
              if ( (isRope  && skinRopes_)  ||
                   (isCloth && skinClothes_) )
              {
                continue;
              }
              
              idBodyToBakedGO_     .Add(idBody, bakedGO);
              idBodyToRelativePath_.Add(idBody, mementoTarget.TargetPath);

              listBodyId.RemoveAt(i);      
            }
            else if (hasSkinnedMeshRenderer)
            {

              if ( (isRope  && !skinRopes_)  ||
                   (isCloth && !skinClothes_) )
              {
                continue;
              }

              Transform rootBoneTr  = smr.rootBone;
              Transform[] arrBoneTr = smr.bones;

              if ( rootBoneTr == null ||
                   arrBoneTr == null )
              {
                continue;
              }
              
              GameObject rootBoneGO = rootBoneTr.gameObject;
              string relativeRootBonePath = rootBoneGO.GetRelativePathTo(rootGameObject_);

              int nBones = arrBoneTr.Length;   
              listBoneRelativePath.Clear();   

              bool okBones = true;
              for (int j = 0; j < nBones; j++)
              {     
                Transform boneTr = arrBoneTr[j];
                if (boneTr == null)
                {
                  okBones = false;
                  break;
                }

                GameObject boneGO = boneTr.gameObject;
                string relativeBonePath = boneGO.GetRelativePathTo(rootGameObject_);
                listBoneRelativePath.Add( relativeBonePath );
              }      


              if (okBones && listBoneRelativePath.Count > 0)
              {
                idBodyToBakedGO_.Add(idBody, rootBoneGO);
                idBodyToRelativePath_.Add(idBody, relativeRootBonePath);
                idBodyToBonesRelativePaths_.Add(idBody, listBoneRelativePath.ToArray());

                bakedSkinnedGOToListIdBody_.Add(bakedGO, new List<uint>() { idBody } );

                setBoneBodies_.Add(idBody);
                listBodyId.RemoveAt(i);
              }
            }           
          } 
        }
      }
    }
    //----------------------------------------------------------------------------------
    private void CreateFrameBakedObjects()
    {
      listIdBodyTmp_.Clear();
      int nBodyNodes = listBodyNodeToBake_.Count;

      List<uint> listIdBodyTotal = new List<uint>();

      for (int i = 0; i < nBodyNodes; i++)
      {
        Tuple2<CNBody, bool> tupleBodyNodeNeedsCollapsing_ = listBodyNodeToBake_[i]; 

        CNBody bodyNode = tupleBodyNodeNeedsCollapsing_.First;
        entityManager_.BuildListBodyIdFromBodyNode( bodyNode, listIdBodyTmp_ );
        listIdBodyTotal.AddRange(listIdBodyTmp_);
      }

      Matrix4x4 m_WORLD_to_LOCALCOMBINED = CalculateWorldToLocalMatrixSimulating(listIdBodyTotal);
      Vector3 position = -( m_WORLD_to_LOCALCOMBINED.GetColumn(3) );

      frameMeshCombiner_.SetWorldToLocalClearingInfo( m_WORLD_to_LOCALCOMBINED );

      for (int i = 0; i < nBodyNodes; i++)
      {
        Tuple2<CNBody, bool> tupleBodyNodeNeedsCollapsing_ = listBodyNodeToBake_[i]; 

        CNBody bodyNode = tupleBodyNodeNeedsCollapsing_.First;

        entityManager_.BuildListBodyIdFromBodyNode( bodyNode, listIdBodyTmp_ );

        if (listIdBodyTmp_.Count > 0)
        {
          if (combineMeshesInFrame_)
          {
            CreateFrameBakedGameObjectsCombined(listIdBodyTmp_, position);
          }
          else
          {
            string bodyNodeName = bodyNode.Name;

            string prefix = string.Empty;
            if (bakeObjectPrefix_ != string.Empty)
            {
              prefix += bakeObjectPrefix_ + "_";
            }

            GameObject nodeGO = new GameObject( prefix + bodyNodeName);

            nodeGO.transform.parent = rootGameObject_.transform;
            CreateFrameBakedGameObjects(listIdBodyTmp_, nodeGO, prefix);
          }        
        }
      }

      if ( frameMeshCombiner_.CanGenerateMesh() )
      {
        GenerateCombinedGameObject(position);
      }

      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh(); 
    }
    //----------------------------------------------------------------------------------
    private void CreateBakedGameObjects(string bodyNodeName, CNBody bodyNode, List<uint> listBodyId, GameObject nodeGO)
    {
      foreach( var idBody in listBodyId )
      { 
        GameObject originalGO = entityManager_.GetGOFromIdBody(idBody);

        if (!setVisibleBodies_.Contains(idBody) || originalGO == null ) 
        {
          continue;
        }

        GameObject bakedGO = (GameObject)UnityEngine.Object.Instantiate(originalGO);
        SetBakedGameObject( bakedGO, originalGO, nodeGO, idBody, listGOTmp_ );

        if ( bakedGO.HasMesh() )
        {
          bool isRope = entityManager_.IsRope(idBody);

          if ( isRope )
          {
            Tuple2<Mesh, Vector3> ropeInit = entityManager_.GetRopeInit(idBody);
            Vector3 center = ropeInit.Second;
            CarEditorUtils.SetMesh(bakedGO, ropeInit.First);

            bakedGO.transform.localPosition = center;
            bakedGO.transform.localRotation = Quaternion.identity;
            bakedGO.transform.localScale    = Vector3.one;

            AssetDatabase.AddObjectToAsset(ropeInit.First, MeshesPrefab);
          }
          else
          {
            Mesh mesh = bakedGO.GetMesh();

            bool isSoftbody = entityManager_.IsSoftbody(idBody);
            bool isCloth    = entityManager_.IsCloth(idBody);

            bool isInProject = AssetDatabase.Contains( mesh.GetInstanceID() );

            if (isSoftbody || isCloth || !isInProject)
            {
              Mesh bakedMesh;
              if ( originalMeshToBakedMesh_.ContainsKey(mesh) )
              {
                bakedMesh = originalMeshToBakedMesh_[mesh];
              }
              else
              {
                bakedMesh = UnityEngine.Object.Instantiate(mesh);
                bakedMesh.name = mesh.name;
                originalMeshToBakedMesh_.Add(mesh, bakedMesh);
                AssetDatabase.AddObjectToAsset(bakedMesh, MeshesPrefab);
              }

              CarEditorUtils.SetMesh(bakedGO, bakedMesh);
            }
          }
        }

        string relativePath = bodyNodeName + "/" + bakedGO.name;
        idBodyToRelativePath_.Add(idBody, relativePath);
        idBodyToBakedGO_     .Add(idBody, bakedGO);

        CarAnimationPersistence animationPersistence = rootCRAnimation_.AnimationPersistence;
        animationPersistence.AddGameObjectBakeMemento( new CarMementoOriginal(originalGO, false), relativePath );
      }
    }
    //----------------------------------------------------------------------------------
    private void CreateRigidSkinnedGameObjects( string bodyNodeName, CNBody bodyNode, List<uint> listBodyId, GameObject nodeGO )
    { 
      Vector3 world_center = CalculateCommonWorldCenter(listBodyId);

      Matrix4x4 matrix_WORLD_to_LOCAL = new Matrix4x4();
      matrix_WORLD_to_LOCAL.SetTRS( -world_center, Quaternion.identity, Vector3.one );

      skinnedMeshCombiner_.SetWorldToLocalClearingInfo( matrix_WORLD_to_LOCAL );

      string name = CarEditorUtils.GetUniqueChildGameObjectName(nodeGO, nodeGO.name + "_bones");
      GameObject rootBoneGO = new GameObject(name); 
      rootBoneGO.transform.parent = nodeGO.transform;

      listIdBodyTmp2_.Clear();

      List<Tuple2<GameObject, string>> listPath = new List<Tuple2<GameObject, string>>();

      int idBone = 0;
      int idSkinnedGO = 0;

      foreach( var idBody in listBodyId )
      {                 
        GameObject originalGO = entityManager_.GetGOFromIdBody(idBody);
        if (!setVisibleBodies_.Contains(idBody) || originalGO == null ) 
        {
          continue;
        }

        GameObject bakedBone = new GameObject();
        SetBakedBoneGameObject( bakedBone, originalGO, nodeGO, rootBoneGO, idBone );

        listIdBodyTmp2_.Add(idBody);

        if ( !skinnedMeshCombiner_.CanAddGameObject( originalGO ) )
        {
          GameObject skinnedGO = GenerateRgSkinnedGameObject(nodeGO, rootBoneGO, idSkinnedGO, world_center);
          idSkinnedGO++;

          BuildRgSkinnedPersistenceAndListBodies(skinnedGO, listIdBodyTmp2_, listPath );
        }

        string relativePath = bakedBone.GetRelativePathTo(rootGameObject_);

        skinnedMeshCombiner_.AddGameObject( originalGO, bakedBone );
        listPath.Add( Tuple2.New(originalGO, relativePath) );

        idBodyToRelativePath_.Add(idBody, relativePath);
        idBodyToBakedGO_     .Add(idBody, bakedBone);
        setBoneBodies_       .Add(idBody);

        idBone++;
      }

      if( skinnedMeshCombiner_.CanGenerateMesh() )
      {
        GameObject skinnedGO = GenerateRgSkinnedGameObject(nodeGO, rootBoneGO, idSkinnedGO, world_center);
        BuildRgSkinnedPersistenceAndListBodies(skinnedGO, listIdBodyTmp2_, listPath );
      }
    }
    //----------------------------------------------------------------------------------
    private void BuildRgSkinnedPersistenceAndListBodies(GameObject skinnedGO, List<uint> listIdBodyTmp, List<Tuple2<GameObject, string>> listGOPathPersistence)
    {
      bakedSkinnedGOToListIdBody_[skinnedGO] = new List<uint>(listIdBodyTmp);
      listIdBodyTmp.Clear();

      String skinnedGOPath = skinnedGO.GetRelativePathTo(rootGameObject_);
      foreach( var goPathPersitence in listGOPathPersistence )
      {
        GameObject originalGO = goPathPersitence.First;
        String bonePath = goPathPersitence.Second; 

        animationPersistence_.AddGameObjectBakeMemento( new CarMementoOriginal(originalGO, false), bonePath, skinnedGOPath );
      }
      listGOPathPersistence.Clear();
    }
    //----------------------------------------------------------------------------------
    private GameObject GenerateRgSkinnedGameObject(GameObject nodeGO, GameObject rootBoneGO, int idSkinnedGO, Vector3 worldCenter)
    {
      Material[] arrMaterial;
      Transform[] arrBone;

      Mesh skinnedMesh = skinnedMeshCombiner_.GenerateMesh(out arrMaterial, out arrBone);
      skinnedMesh.name = nodeGO.name + "_" + idSkinnedGO;

      GameObject go = new GameObject(nodeGO.name + "_skinned_" + idSkinnedGO); 
      go.transform.parent = nodeGO.transform;

      SkinnedMeshRenderer smr = go.AddComponent<SkinnedMeshRenderer>();

      smr.rootBone            = null; //rootBoneGO.transform;
      smr.bones               = arrBone;
      smr.sharedMesh          = skinnedMesh;   
      smr.sharedMaterials     = arrMaterial;
      smr.updateWhenOffscreen = true;

      go.transform.position = worldCenter;

      Mesh newMesh = UnityEngine.Object.Instantiate(skinnedMesh);
      newMesh.name = skinnedMesh.name;

      CarEditorUtils.SetMesh(go, newMesh);
      AssetDatabase.AddObjectToAsset(newMesh, MeshesPrefab);
      
      skinnedMeshCombiner_.Clear();

      return go;
    }
    //----------------------------------------------------------------------------------
    private void CreateRopeSkinnedGameObjects( string bodyNodeName, CNBody bodyNode, List<uint> listBodyId, GameObject nodeGO )
    { 
      foreach( var idBody in listBodyId )
      { 
        GameObject originalGO = entityManager_.GetGOFromIdBody(idBody);

        if (!setVisibleBodies_.Contains(idBody) || originalGO == null) 
        {
          continue;
        }

        GameObject bakedGO = (GameObject)UnityEngine.Object.Instantiate(originalGO);
        
        GameObject  rootBoneGameObject;
        Transform[] boneTransforms;
        GenerateRopeSkinnedGameObject( bakedGO, originalGO, nodeGO, idBody, listGOTmp_, out rootBoneGameObject, out boneTransforms );

        int nBones = boneTransforms.Length;

        string relativePath = rootBoneGameObject.GetRelativePathTo(rootGameObject_);
        idBodyToRelativePath_.Add(idBody, relativePath);

        string[] arrBoneRelativePath = new string[nBones];
        for (int i = 0; i < nBones; i++)
        {
          Transform tr = boneTransforms[i];
          arrBoneRelativePath[i] = relativePath + "/" + tr.gameObject.name;
        }

        idBodyToBakedGO_           .Add(idBody, rootBoneGameObject);
        idBodyToBonesRelativePaths_.Add(idBody, arrBoneRelativePath);
        setBoneBodies_             .Add(idBody);
        bakedSkinnedGOToListIdBody_.Add(bakedGO, new List<uint>() { idBody });

        string skinnedGORelativePath = bakedGO.GetRelativePathTo(rootGameObject_);
        animationPersistence_.AddGameObjectBakeMemento( new CarMementoOriginal(originalGO, true), skinnedGORelativePath );
      }
    }
    //----------------------------------------------------------------------------------
    private void CreateClothSkinnedGameObjects( string bodyNodeName, CNBody bodyNode, List<uint> listBodyId, GameObject nodeGO )
    { 
      foreach( var idBody in listBodyId )
      { 
        GameObject originalGO = entityManager_.GetGOFromIdBody(idBody);

        if (!setVisibleBodies_.Contains(idBody) || originalGO == null) 
        {
          continue;
        }

        GameObject bakedGO = (GameObject)UnityEngine.Object.Instantiate(originalGO);
        
        GameObject  rootBoneGameObject;
        Transform[] boneTransforms;
        GenerateClothSkinnedGameObject( bakedGO, originalGO, nodeGO, idBody, listGOTmp_, out rootBoneGameObject, out boneTransforms );

        int nBones = boneTransforms.Length;

        string relativePath = rootBoneGameObject.GetRelativePathTo(rootGameObject_);
        idBodyToRelativePath_.Add(idBody, relativePath);

        string[] arrBoneRelativePath = new string[nBones];
        for (int i = 0; i < nBones; i++)
        {
          Transform tr = boneTransforms[i];
          arrBoneRelativePath[i] = relativePath + "/" + tr.gameObject.name;
        }

        idBodyToBakedGO_           .Add(idBody, rootBoneGameObject);
        idBodyToBonesRelativePaths_.Add(idBody, arrBoneRelativePath);
        setBoneBodies_             .Add(idBody);

        bakedSkinnedGOToListIdBody_.Add(bakedGO, new List<uint>() { idBody });
      
        string skinnedGORelativePath = bakedGO.GetRelativePathTo(rootGameObject_);
        animationPersistence_.AddGameObjectBakeMemento( new CarMementoOriginal(originalGO, true), skinnedGORelativePath );
      }
    }
    //----------------------------------------------------------------------------------
    private void CreateFrameBakedGameObjects(List<uint> listBodyId, GameObject nodeGO, string prefix)
    {
      foreach( var idBody in listBodyId )
      { 
        Transform simulatingTr = entityManager_.GetBodyTransformRef(idBody);

        GameObject simulatingGO = simulatingTr.gameObject;
        if (simulatingGO.activeSelf)
        {
          GameObject bakedGO  = (GameObject)UnityEngine.Object.Instantiate(simulatingGO);

          bakedGO.name = prefix + simulatingGO.name;
          bakedGO.hideFlags = HideFlags.None;

          Caronte_Fx_Body fxBody = bakedGO.GetComponent<Caronte_Fx_Body>();
          if (fxBody != null && !preserveCFXComponentsInFrameBake_)
          {
            UnityEngine.Object.DestroyImmediate(fxBody);
          }

          bakedGO.transform.parent = nodeGO.transform;
          if ( bakedGO.HasMesh() )
          {
            Mesh mesh = bakedGO.GetMesh();
            bool isInProject = AssetDatabase.Contains( mesh.GetInstanceID() );
            
            if ( entityManager_.IsRope(idBody) )
            {
              Tuple2<Mesh, MeshUpdater> meshUpdater = entityManager_.GetBodyMeshRenderUpdaterRef(idBody);
              Mesh newMesh = UnityEngine.Object.Instantiate(meshUpdater.First);
              CarEditorUtils.SetMesh(bakedGO, newMesh);
              AssetDatabase.AddObjectToAsset(newMesh, MeshesPrefab);
            }
            else if ( !isInProject )
            {
              Mesh newMesh = UnityEngine.Object.Instantiate(mesh);
              newMesh.name = mesh.name;
              newMesh.hideFlags = HideFlags.None;
              CarEditorUtils.SetMesh(bakedGO, newMesh);
              AssetDatabase.AddObjectToAsset(newMesh, MeshesPrefab);
            }
          }
        }
      }
    }
    //----------------------------------------------------------------------------------
    private void CreateFrameBakedGameObjectsCombined(List<uint> listBodyId, Vector3 position)
    {
      foreach( var idBody in listBodyId )
      { 
        Transform simulatingTr  = entityManager_.GetBodyTransformRef(idBody);
        GameObject simulatingGO = simulatingTr.gameObject;

        if (simulatingGO.activeSelf)
        {
          if (!frameMeshCombiner_.CanAddGameObject( simulatingGO ) )
          {
            GenerateCombinedGameObject(position);
          }

          frameMeshCombiner_.AddGameObject( simulatingGO );
        }
      }
    }
    //----------------------------------------------------------------------------------
    private void GenerateCombinedGameObject(Vector3 position)
    {
      Material[] arrMaterial;

      Mesh mesh = frameMeshCombiner_.GenerateMesh(out arrMaterial);
      mesh.name = BakeObjectName;

      GameObject go = new GameObject(BakeObjectName + "_combined"); 
      go.transform.parent = rootGameObject_.transform;

      MeshFilter mf = go.AddComponent<MeshFilter>();
      mf.sharedMesh = mesh;

      MeshRenderer mr = go.AddComponent<MeshRenderer>();  
      mr.sharedMaterials = arrMaterial;
 
      go.transform.position = position;

      AssetDatabase.AddObjectToAsset(mesh, MeshesPrefab);
      
      frameMeshCombiner_.Clear();
    }
    //----------------------------------------------------------------------------------
    private Vector3 CalculateCommonWorldCenter(List<uint> listBodyId)
    {
      Bounds box_WORLD = new Bounds();
      int nBodies = listBodyId.Count;

      int bodyCount = 0;
      for( int a = 0; a < nBodies; a++ )
      {        
        uint idBody = listBodyId[a];
        GameObject originalGO = entityManager_.GetGOFromIdBody(idBody);

        if ( originalGO == null )
        {
          continue;
        }

        Mesh mesh = originalGO.GetMesh();

        if ( mesh == null )
        {
          continue;
        }

        Renderer rd = originalGO.GetComponent<Renderer>();
        if ( rd == null )
        {
          continue;
        }

        Matrix4x4 m_MODEL_to_WORLD = originalGO.transform.localToWorldMatrix;
        Bounds bounds_world = new Bounds();
        CarGeometryUtils.CreateBoundsTransformed( mesh.bounds, m_MODEL_to_WORLD, out bounds_world );

        if (bodyCount == 0)
        {
          box_WORLD = bounds_world;
        }
        else
        {
          box_WORLD.Encapsulate( bounds_world );
        }

        bodyCount++;
      }

      return box_WORLD.center;
    }
    //----------------------------------------------------------------------------------
    private Matrix4x4 CalculateWorldToLocalMatrixSimulating(List<uint> listBodyId)
    {
      Bounds box_WORLD = new Bounds();
      int nBodies = listBodyId.Count;

      for( int a = 0; a < nBodies; a++ )
      {        
        uint idBody = listBodyId[a];
        Transform simulatingTr = entityManager_.GetBodyTransformRef(idBody);
        if ( simulatingTr == null )
        {
          continue;
        }

        GameObject simulatingGO = simulatingTr.gameObject;
        Mesh mesh = simulatingGO.GetMesh();

        if ( mesh == null )
        {
          continue;
        }

        Renderer rd = simulatingGO.GetComponent<Renderer>();
        if ( rd == null )
        {
          continue;
        }

        Bounds bounds_world = rd.bounds;

        if (a == 0)
        {
          box_WORLD = bounds_world;
        }
        else
        {
          box_WORLD.Encapsulate( bounds_world );
        }
      }

      Matrix4x4 matrix_WORLD_to_LOCAL = new Matrix4x4();
      matrix_WORLD_to_LOCAL.SetTRS( -box_WORLD.center, Quaternion.identity, Vector3.one );

      return matrix_WORLD_to_LOCAL;
    }
    //----------------------------------------------------------------------------------
    private void InitKeyframeData()
    {
      SimulationManager.SetReplayingFrame( (uint)frameStart_, true );
      bool read = false;
      do
      {
        System.Threading.Thread.Sleep( 1 );
        read = SimulationManager.ReadSimulationBufferUniqueUnsafe( BroadcastStartDelegate, InitKeyFrame, null );
      } while (!read);
    }
    //----------------------------------------------------------------------------------
    private void BakeGOKeyFrames(int frame)
    {
      currentBakeFrame_ = (uint)frame;
      SimulationManager.SetReplayingFrame( (uint)frame, true );       
      bool read = false;
      do
      {
        System.Threading.Thread.Sleep( 1 );
        read = SimulationManager.ReadSimulationBufferUniqueUnsafe( BroadcastStartDelegate, BakeBodyKeyFrame, null );
      } while (!read);

      CalculateKeyframeWorldBoxes();
    }
    //----------------------------------------------------------------------------------
    private void CalculateKeyframeWorldBoxes()
    {
      listBakedSkinnedGOLocalBounds_.Clear();

      foreach(var pair in bakedSkinnedGOToListIdBody_)
      {
        GameObject bakedSkinnedGO = pair.Key;
        List<uint> listIdBody     = pair.Value;

        Bounds bounds = CalculateLocalBoxes(bakedSkinnedGO, listIdBody);           
        listBakedSkinnedGOLocalBounds_.Add( bounds );
      }
    }
    //----------------------------------------------------------------------------------
    private Bounds CalculateLocalBoxes(GameObject bakedSkinnedGO, List<uint> listIdBody)
    {
      Bounds worldBounds = new Bounds();

      bool isFirstValidBody = true;
      foreach(uint idBody in listIdBody)
      {
        CarGOKeyframe goKeyframe = idBodyToGOKeyframe_[idBody];
        
        if ( goKeyframe.HasFrameData() )
        {
          CaronteSharp.Box box = BodyManager.GetReplayingBoxWorld(idBody);
          Matrix4x4 m_WORLD_to_LOCAL = bakedSkinnedGO.transform.worldToLocalMatrix;

          Bounds bodyBounds = new Bounds();
          bodyBounds.SetMinMax(m_WORLD_to_LOCAL.MultiplyPoint3x4(box.vMin_), m_WORLD_to_LOCAL.MultiplyPoint3x4(box.vMax_));

          if (isFirstValidBody)
          {
            worldBounds = bodyBounds;
            isFirstValidBody = false;
          }
          else
          {
            worldBounds.Encapsulate(bodyBounds);
          }       
        }
      }

      return worldBounds; 
    }
    //----------------------------------------------------------------------------------
    private void CheckVisibilityEstimateFrameSize(int frame)
    {
      currentBakeFrame_ = (uint)frame;
      SimulationManager.SetReplayingFrame( (uint)frame, true );
      bool read = false;
      do
      {
        System.Threading.Thread.Sleep( 1 );
        read = SimulationManager.ReadSimulationBufferUniqueUnsafe( BroadcastStartDelegate, CheckBodyVisibilityEstimateFrameSize, null );
      } while(!read);
    }
    //----------------------------------------------------------------------------------
    private void BroadcastStartDelegate( bool doDisplayInvisibleBodies )
    {

    }
    //----------------------------------------------------------------------------------
    private void SetBakedGameObject( GameObject bakedGO, GameObject originalGO, GameObject nodeGO, uint idBody, List<GameObject> listGameObjectToDestroy )
    {
      listGameObjectToDestroy.Clear();
      entityManager_.GetGameObjectsBodyChildrenList( originalGO, bakedGO, listGameObjectToDestroy );
      foreach( GameObject go in listGameObjectToDestroy )
      {
        UnityEngine.Object.DestroyImmediate(go);
      }
      listGameObjectToDestroy.Clear();

      string name = CarEditorUtils.GetUniqueChildGameObjectName(nodeGO, BakeObjectPrefix + originalGO.name);

      bakedGO.name = name;

      bakedGO.transform.parent        = originalGO.transform.parent;
      bakedGO.transform.localPosition = originalGO.transform.localPosition;
      bakedGO.transform.localRotation = originalGO.transform.localRotation;
      bakedGO.transform.localScale    = originalGO.transform.localScale;

      bakedGO.transform.parent = nodeGO.transform;

      Mesh bakedMesh; 
      CarEditorUtils.ReplaceSkinnedMeshRendererForMeshRenderer( bakedGO, out bakedMesh );
      if (bakedMesh != null)
      {
        AssetDatabase.AddObjectToAsset(bakedMesh, MeshesPrefab);
      }
 
      Transform simTransform = entityManager_.GetBodyTransformRef(idBody);
      bakedGO.SetActive( simTransform.gameObject.activeInHierarchy );

      Caronte_Fx_Body cfxBody = bakedGO.GetComponent<Caronte_Fx_Body>();
      if (cfxBody != null)
      {
        UnityEngine.Object.DestroyImmediate( cfxBody );
      }
    }
    //----------------------------------------------------------------------------------
    private void GenerateRopeSkinnedGameObject( GameObject bakedGO, GameObject originalGO, GameObject nodeGO, uint idBody, List<GameObject> listAuxGameObjectToDestroy,
                                                out GameObject rootBonesGameObject, out Transform[] arrBoneTransform )
    {
      listAuxGameObjectToDestroy.Clear();
      entityManager_.GetGameObjectsBodyChildrenList( originalGO, bakedGO, listAuxGameObjectToDestroy );
      foreach( GameObject go in listAuxGameObjectToDestroy )
      {
        UnityEngine.Object.DestroyImmediate(go);
      }
      listAuxGameObjectToDestroy.Clear();

      bakedGO.name = CarEditorUtils.GetUniqueChildGameObjectName(nodeGO, BakeObjectPrefix + originalGO.name);

      CarEditorUtils.ReplaceMeshRendererForSkinnedMeshRenderer(bakedGO);

      Tuple2<Mesh, Vector3> ropeInit = entityManager_.GetRopeInit(idBody);   
 
      Mesh meshInit = ropeInit.First;

      Vector3 center;
      MeshComplex meshComplex = RopeManager.GetMeshRenderStraight(idBody, out center);
      
      Mesh meshInitStraight  = new Mesh();
      meshInitStraight.name = meshInit.name;

      meshInitStraight.vertices  = meshComplex.arrPosition_;
      meshInitStraight.normals   = meshComplex.arrNormal_;
      meshInitStraight.tangents  = meshComplex.arrTan_;
      meshInitStraight.uv        = meshComplex.arrUV_;
      meshInitStraight.triangles = meshComplex.arrIndex_;

      bakedGO.transform.localPosition = center;
      bakedGO.transform.localRotation = Quaternion.identity;
      bakedGO.transform.localScale    = Vector3.one;

      bakedGO.transform.parent = nodeGO.transform;

      InitSkinnedRope(idBody, bakedGO, nodeGO, meshInitStraight, out rootBonesGameObject, out arrBoneTransform);

      bakedGO.SetActive( true );

      Caronte_Fx_Body cfxBody = bakedGO.GetComponent<Caronte_Fx_Body>();
      if (cfxBody != null)
      {
        UnityEngine.Object.DestroyImmediate( cfxBody );
      }
    }
    //----------------------------------------------------------------------------------
    private void GenerateClothSkinnedGameObject( GameObject bakedGO, GameObject originalGO, GameObject nodeGO, uint idBody, List<GameObject> listAuxGameObjectToDestroy,
                                                 out GameObject rootBonesGameObject, out Transform[] arrBoneTransform )
    {
      listAuxGameObjectToDestroy.Clear();
      entityManager_.GetGameObjectsBodyChildrenList( originalGO, bakedGO, listAuxGameObjectToDestroy );
      foreach( GameObject go in listAuxGameObjectToDestroy )
      {
        UnityEngine.Object.DestroyImmediate(go);
      }
      listAuxGameObjectToDestroy.Clear();

      bakedGO.name = CarEditorUtils.GetUniqueChildGameObjectName(nodeGO, BakeObjectPrefix + originalGO.name);

      CarEditorUtils.ReplaceMeshRendererForSkinnedMeshRenderer(bakedGO);

      Mesh originalMesh = originalGO.GetMesh();
      
      bakedGO.transform.parent = nodeGO.transform;

      bakedGO.transform.position   = originalGO.transform.position;
      bakedGO.transform.rotation   = originalGO.transform.rotation;
      bakedGO.transform.localScale = originalGO.transform.lossyScale;
 
      InitSkinnedCloth(idBody, bakedGO, nodeGO, originalMesh, out rootBonesGameObject, out arrBoneTransform);

      bakedGO.SetActive( true );

      Caronte_Fx_Body cfxBody = bakedGO.GetComponent<Caronte_Fx_Body>();
      if (cfxBody != null)
      {
        UnityEngine.Object.DestroyImmediate( cfxBody );
      }
    }
    //----------------------------------------------------------------------------------
    private void InitSkinnedRope( uint idBody, GameObject bakedGO, GameObject nodeGO, Mesh meshToSkin, out GameObject bonesRootGO, out Transform[] arrBoneTransform )
    {
      CaronteSharp.SkelDefinition skelDefinition;
      RopeManager.QuerySkelDefinition(idBody, out skelDefinition);

      Matrix4x4 m_MODEL_to_WORLD     = Matrix4x4.TRS(skelDefinition.root_translation_MODEL_to_WORLD_, skelDefinition.root_rotation_MODEL_to_WORLD_, skelDefinition.root_scale_MODEL_to_WORLD_);
      Matrix4x4 m_MODEL_to_ROOT_BONE = m_MODEL_to_WORLD.inverse * bakedGO.transform.localToWorldMatrix;

      Mesh skinnedMesh;
      CarGeometryUtils.CreateMeshTransformed( meshToSkin, m_MODEL_to_ROOT_BONE, out skinnedMesh );

      listBoneWeightTmp_.Clear();

      int nVertex = skinnedMesh.vertexCount;
      for(int i = 0; i < nVertex; i++)
      {
        CaronteSharp.Tuple4_INDEX indexes = skelDefinition.arrIndexBone_[i];
        CaronteSharp.Tuple4_FLOAT weights = skelDefinition.arrWeightBone_[i];

        BoneWeight bw = new BoneWeight();
        bw.boneIndex0 = (int)indexes.a_;
        bw.weight0    = weights.a_;
        bw.boneIndex1 = (int)indexes.b_;
        bw.weight1    = weights.b_;

        listBoneWeightTmp_.Add( bw );
      }

      skinnedMesh.boneWeights = listBoneWeightTmp_.ToArray();

      string name = CarEditorUtils.GetUniqueChildGameObjectName(nodeGO, bakedGO.name + "_bones");
      bonesRootGO = new GameObject(name); 

      bonesRootGO.transform.localPosition = skelDefinition.root_translation_MODEL_to_WORLD_;
      bonesRootGO.transform.localRotation = skelDefinition.root_rotation_MODEL_to_WORLD_;
      bonesRootGO.transform.localScale    = skelDefinition.root_scale_MODEL_to_WORLD_;

      bonesRootGO.transform.parent = nodeGO.transform; 

      int nBones = skelDefinition.arrTranslation_BONE_to_MODEL_.Length;

      listBindPoseTmp_ .Clear();
      listTransformTmp_.Clear();

      for (int i = 0; i < nBones; i++)
      {
        GameObject bone = new GameObject(bakedGO.name + "_" + i);

        bone.transform.parent = bonesRootGO.transform;

        bone.transform.localPosition = skelDefinition.arrTranslation_BONE_to_MODEL_[i];
        bone.transform.localRotation = skelDefinition.arrRotation_BONE_to_MODEL_[i];
        bone.transform.localScale    = skelDefinition.arrScale_BONE_to_MODEL_[i];

        Matrix4x4 bindpose = bone.transform.worldToLocalMatrix * bonesRootGO.transform.localToWorldMatrix;

        listBindPoseTmp_ .Add(bindpose);
        listTransformTmp_.Add(bone.transform);

      }

      skinnedMesh.bindposes = listBindPoseTmp_.ToArray();
      arrBoneTransform      = listTransformTmp_.ToArray();


      //Set init state
      CaronteSharp.SkelState skelState;
      RopeManager.QuerySkelState(idBody, out skelState);

      bonesRootGO.transform.localPosition = skelState.root_translation_MODEL_to_WORLD_;
      bonesRootGO.transform.localRotation = skelState.root_rotation_MODEL_to_WORLD_;
      bonesRootGO.transform.localScale    = skelState.root_scale_MODEL_to_WORLD_;

      bakedGO.transform.localPosition = skelState.root_translation_MODEL_to_WORLD_;
      bakedGO.transform.localRotation = skelState.root_rotation_MODEL_to_WORLD_;
      bakedGO.transform.localScale    = skelState.root_scale_MODEL_to_WORLD_;

      for (int i = 0; i < nBones; i++)
      {
        GameObject bone = listTransformTmp_[i].gameObject;

        bone.transform.localPosition = skelState.arrTranslation_BONE_to_MODEL_[i];
        bone.transform.localRotation = skelState.arrRotation_BONE_to_MODEL_[i];
        bone.transform.localScale    = skelState.arrScale_BONE_to_MODEL_[i];
      }

      SkinnedMeshRenderer smr = bakedGO.GetComponent<SkinnedMeshRenderer>();
      smr.sharedMesh          = skinnedMesh;
      smr.bones               = arrBoneTransform;
      smr.rootBone            = bonesRootGO.transform;
      smr.updateWhenOffscreen = true;

      AssetDatabase.AddObjectToAsset(skinnedMesh, MeshesPrefab);
    }
    //----------------------------------------------------------------------------------
    private void InitSkinnedCloth( uint idBody, GameObject bakedGO, GameObject nodeGO, Mesh meshToSkin, out GameObject bonesRootGO, out Transform[] arrBoneTransform )
    {
      CaronteSharp.SkelDefinition skelDefinition;
      ClothManager.Cl_QuerySkelDefinition(idBody, out skelDefinition);

      Matrix4x4 m_MODEL_to_WORLD     = Matrix4x4.TRS(skelDefinition.root_translation_MODEL_to_WORLD_, skelDefinition.root_rotation_MODEL_to_WORLD_, skelDefinition.root_scale_MODEL_to_WORLD_);
      Matrix4x4 m_MODEL_to_ROOT_BONE = m_MODEL_to_WORLD.inverse * bakedGO.transform.localToWorldMatrix;

      Mesh skinnedMesh;
      CarGeometryUtils.CreateMeshTransformed( meshToSkin, m_MODEL_to_ROOT_BONE, out skinnedMesh );

      listBoneWeightTmp_.Clear();

      int nVertex = skinnedMesh.vertexCount;
      for(int i = 0; i < nVertex; i++)
      {
        CaronteSharp.Tuple4_INDEX indexes = skelDefinition.arrIndexBone_[i];
        CaronteSharp.Tuple4_FLOAT weights = skelDefinition.arrWeightBone_[i];

        BoneWeight bw = new BoneWeight();

        bw.boneIndex0 = (int)indexes.a_;
        bw.weight0    = weights.a_;
  
        bw.boneIndex1 = (int)indexes.b_;
        bw.weight1    = weights.b_;

        bw.boneIndex2 = (int)indexes.c_;
        bw.weight2    = weights.c_;

        bw.boneIndex3 = (int)indexes.d_;
        bw.weight3    = weights.d_;
 
        listBoneWeightTmp_.Add( bw );
      }

      skinnedMesh.boneWeights = listBoneWeightTmp_.ToArray();

      string name = CarEditorUtils.GetUniqueChildGameObjectName(nodeGO, bakedGO.name + "_bones");
      bonesRootGO = new GameObject(name); 

      bonesRootGO.transform.localPosition = skelDefinition.root_translation_MODEL_to_WORLD_;
      bonesRootGO.transform.localRotation = skelDefinition.root_rotation_MODEL_to_WORLD_;
      bonesRootGO.transform.localScale    = skelDefinition.root_scale_MODEL_to_WORLD_;

      bonesRootGO.transform.parent = nodeGO.transform; 

      int nBones = skelDefinition.arrTranslation_BONE_to_MODEL_.Length;

      listBindPoseTmp_ .Clear();
      listTransformTmp_.Clear();

      for (int i = 0; i < nBones; i++)
      {
        GameObject bone = new GameObject(bakedGO.name + "_" + i);

        bone.transform.parent = bonesRootGO.transform;

        bone.transform.localPosition = skelDefinition.arrTranslation_BONE_to_MODEL_[i];
        bone.transform.localRotation = skelDefinition.arrRotation_BONE_to_MODEL_[i];
        bone.transform.localScale    = skelDefinition.arrScale_BONE_to_MODEL_[i];

        Matrix4x4 bindpose = bone.transform.worldToLocalMatrix * bonesRootGO.transform.localToWorldMatrix;

        listBindPoseTmp_ .Add(bindpose);
        listTransformTmp_.Add(bone.transform);

      }

      skinnedMesh.bindposes = listBindPoseTmp_.ToArray();
      arrBoneTransform      = listTransformTmp_.ToArray();

      //Set init state
      CaronteSharp.SkelState skelState;
      ClothManager.Cl_QuerySkelState(idBody, out skelState);

      bonesRootGO.transform.localPosition = skelState.root_translation_MODEL_to_WORLD_;
      bonesRootGO.transform.localRotation = skelState.root_rotation_MODEL_to_WORLD_;
      bonesRootGO.transform.localScale    = skelState.root_scale_MODEL_to_WORLD_;

      bakedGO.transform.localPosition = skelState.root_translation_MODEL_to_WORLD_;
      bakedGO.transform.localRotation = skelState.root_rotation_MODEL_to_WORLD_;
      bakedGO.transform.localScale    = skelState.root_scale_MODEL_to_WORLD_;

      for (int i = 0; i < nBones; i++)
      {
        GameObject bone = listTransformTmp_[i].gameObject;

        bone.transform.localPosition = skelState.arrTranslation_BONE_to_MODEL_[i];
        bone.transform.localRotation = skelState.arrRotation_BONE_to_MODEL_[i];
        bone.transform.localScale    = skelState.arrScale_BONE_to_MODEL_[i];
      }

      SkinnedMeshRenderer smr = bakedGO.GetComponent<SkinnedMeshRenderer>();
      smr.sharedMesh          = skinnedMesh;
      smr.bones               = arrBoneTransform;
      smr.rootBone            = bonesRootGO.transform;
      smr.updateWhenOffscreen = true;

      AssetDatabase.AddObjectToAsset(skinnedMesh, MeshesPrefab);
    }
    //----------------------------------------------------------------------------------
    private void SetBakedBoneGameObject( GameObject bakedGO, GameObject originalGO, GameObject nodeGO, GameObject rootBoneGO, int idBone )
    {
      bakedGO.name = BakeObjectPrefix + originalGO.name + "_" + idBone;

      bakedGO.transform.parent = originalGO.transform.parent;

      bakedGO.transform.localPosition = originalGO.transform.localPosition;
      bakedGO.transform.localRotation = originalGO.transform.localRotation;
      bakedGO.transform.localScale    = originalGO.transform.localScale;

      bakedGO.transform.parent     = rootBoneGO.transform;
      bakedGO.transform.localScale = Vector3.one;
    }
    //----------------------------------------------------------------------------------
    private bool CheckBodiesVisibilityEstimateAnimationSize()
    {
      int updateFramesDelta = Mathf.Min( Mathf.Max(frameCount_ / 5, 1), 5);

      for (bakingFrame_ = frameStart_; bakingFrame_ <= frameEnd_; bakingFrame_++ )
      {
        frame_ = bakingFrame_ - frameStart_; 
   
        if ( (frame_ % updateFramesDelta) == 0 )
        {
          float progress = (float) frame_ / (float) frameCount_;
          string progressInfo = "Frame " + frame_ + " of " + frameCount_ + "."; 
          EditorUtility.DisplayProgressBar("CaronteFx - Checking visibility. ", progressInfo, progress );
        }

        CheckVisibilityEstimateFrameSize(bakingFrame_);
      }

      // Skinned boxes size + baked bodies header data (estimated to 1Kb per object)
      //_________________________________________________________________________________
      estimatedAnimationBytesSize_ += nEstimatedSkinnedObjects_ * frameCount_ * boxSizeBytes;
      estimatedAnimationBytesSize_ += setBakedBodies_.Count * 1000;
     
      SimulationManager.SetReplayingFrame( 0, true );
      EditorUtility.ClearProgressBar();

      CarDebug.Log("Estimated animation size: " + estimatedAnimationBytesSize_ + " Bytes.");

      if (estimatedAnimationBytesSize_ > maxAnimationBytesSize)
      {
        EditorUtility.DisplayDialog("CaronteFX - Animation too big", "Animation exceeds the maximum size (2GB), consider splitting the animation in parts", "Ok");
        return false;
      }

      return true;
    }
    //----------------------------------------------------------------------------------
    public void InitKeyFrame(BD_TYPE type, BodyInfo bodyInfo)
    {
      uint idBody = bodyInfo.idBody_;

      if( idBodyToBakedGO_.ContainsKey(idBody) )
      {     
        GameObject go = idBodyToBakedGO_[idBody];
        
        int nVertexCount = 0;
        int nBonesCount  = 0;

        if ( type == BD_TYPE.BODYMESH_ANIMATED_BY_VERTEX ||
             type == BD_TYPE.SOFTBODY || 
             type == BD_TYPE.CLOTH )
        {
          if (idBodyToBonesRelativePaths_.ContainsKey(idBody) )
          {
            nBonesCount = idBodyToBonesRelativePaths_[idBody].Length;
          }
          else
          {
            Mesh mesh = go.GetMesh();
            if (mesh != null)
            {
              nVertexCount = mesh.vertexCount;
            }
          }
        }

        Vector2 visibleTimeInterval = entityManager_.GetVisibleTimeInterval( idBody );

        float newStart = Mathf.Clamp(visibleTimeInterval.x - visibilityShift_, visibilityRangeMin_, visibilityRangeMax_);
        float newEnd   = Mathf.Clamp(visibleTimeInterval.y - visibilityShift_, visibilityRangeMin_, visibilityRangeMax_);

        if (bakeVisibility_)
        {
          visibleTimeInterval.x = newStart;
          visibleTimeInterval.y = newEnd;
        }
        else
        {
          visibleTimeInterval.x = 0f;
          visibleTimeInterval.y = float.MaxValue;
        }

        idBodyToVisibilityInterval_.Add( idBody, visibleTimeInterval );
        idBodyToGOKeyframe_        .Add( idBody, new CarGOKeyframe(nVertexCount, nBonesCount, vertexTangents_) );
      }
    }
    //----------------------------------------------------------------------------------
    public void BakeBodyKeyFrame( BD_TYPE type, BodyInfo bodyInfo )
    {
      uint idBody = bodyInfo.idBody_;
      if ( idBodyToGOKeyframe_.ContainsKey(idBody) )
      {
        CarGOKeyframe goKeyframe = idBodyToGOKeyframe_[idBody];
        WriteGOKeyframe(idBody, type, bodyInfo, goKeyframe);
      }
    }
    //----------------------------------------------------------------------------------
    private void WriteGOKeyframe(uint idBody, BD_TYPE type, BodyInfo bodyInfo, CarGOKeyframe goKeyframe)
    {
      bool isVisible = bodyInfo.broadcastFlag_.IsFlagSet(CaronteSharp.BROADCASTFLAG.VISIBLE);
      bool isGhost   = bodyInfo.broadcastFlag_.IsFlagSet(CaronteSharp.BROADCASTFLAG.GHOST);

      switch (type)
      {
        case BD_TYPE.RIGIDBODY:
        case BD_TYPE.BODYMESH_ANIMATED_BY_MATRIX:
          {
            RigidBodyInfo rbInfo = bodyInfo as RigidBodyInfo;
            goKeyframe.SetBodyKeyframe( isVisible, isGhost, rbInfo.position_, rbInfo.orientation_ );
            break;
          }

        case BD_TYPE.BODYMESH_ANIMATED_BY_VERTEX:
          {
            BodyMeshInfo bodymeshInfo = bodyInfo as BodyMeshInfo;
            goKeyframe.SetBodyKeyframe( isVisible, isGhost, bodymeshInfo.position_, bodymeshInfo.orientation_ );
            
            if ( goKeyframe.HasFrameData() )
            {
              Vector3[] arrNormal;
              Vector4[] arrTangent;
              CalculateMeshData( bodymeshInfo.idBody_, bodymeshInfo.arrVertices_, out arrNormal, out arrTangent );
              goKeyframe.SetVertexKeyframe( bodymeshInfo.arrVertices_, arrNormal, arrTangent );
            }
            break;
          }

        case BD_TYPE.SOFTBODY:
          {
            SoftBodyInfo sbInfo = (SoftBodyInfo)bodyInfo;

            if ( setBoneBodies_.Contains(idBody) )
            {
              SkelState skelState;
              RopeManager.QuerySkelState(idBody, out skelState);

              goKeyframe.SetBodyKeyframe( isVisible, isGhost, skelState.root_translation_MODEL_to_WORLD_, skelState.root_rotation_MODEL_to_WORLD_ );
              if ( goKeyframe.HasFrameData() )
              {
                goKeyframe.SetBonesKeyframe( skelState.arrTranslation_BONE_to_MODEL_, skelState.arrRotation_BONE_to_MODEL_, skelState.arrScale_BONE_to_MODEL_ );
              }
            }
            else
            { 
              goKeyframe.SetBodyKeyframe( isVisible, isGhost, sbInfo.center_, Quaternion.identity );
              if ( goKeyframe.HasFrameData() )
              {
                Vector3[] arrNormal;
                Vector4[] arrTangent;

                CalculateMeshData( sbInfo.idBody_, sbInfo.arrVerticesRender_, out arrNormal, out arrTangent );    
                goKeyframe.SetVertexKeyframe( sbInfo.arrVerticesRender_, arrNormal, arrTangent );
              }    
            }

            break;
          }

        case BD_TYPE.CLOTH:
          {
            SoftBodyInfo sbInfo = (SoftBodyInfo)bodyInfo;

            if ( setBoneBodies_.Contains(idBody) )
            {
              SkelState skelState;
              ClothManager.Cl_QuerySkelState(idBody, out skelState);

              goKeyframe.SetBodyKeyframe( isVisible, isGhost, skelState.root_translation_MODEL_to_WORLD_, skelState.root_rotation_MODEL_to_WORLD_ );
              if ( goKeyframe.HasFrameData() )
              {
                goKeyframe.SetBonesKeyframe( skelState.arrTranslation_BONE_to_MODEL_, skelState.arrRotation_BONE_to_MODEL_, skelState.arrScale_BONE_to_MODEL_ );
              }
            }
            else
            { 
              goKeyframe.SetBodyKeyframe( isVisible, isGhost, sbInfo.center_, Quaternion.identity );
              if ( goKeyframe.HasFrameData() )
              {
                Vector3[] arrNormal;
                Vector4[] arrTangent;
                CalculateMeshData( sbInfo.idBody_, sbInfo.arrVerticesRender_, out arrNormal, out arrTangent );    
                goKeyframe.SetVertexKeyframe( sbInfo.arrVerticesRender_, arrNormal, arrTangent );
              }    
            }
            break;
          }
      }
    }
    //----------------------------------------------------------------------------------
    private void CheckBodyVisibilityEstimateFrameSize(BD_TYPE type, BodyInfo bodyInfo)
    {
      uint idBody = bodyInfo.idBody_;

      bool isVisible = ( bodyInfo.broadcastFlag_ & BROADCASTFLAG.VISIBLE ) == BROADCASTFLAG.VISIBLE;
      bool isGhost   = ( bodyInfo.broadcastFlag_ & BROADCASTFLAG.GHOST )   == BROADCASTFLAG.GHOST;
   
      if (isVisible || isGhost)
      {
        setVisibleBodies_.Add(idBody);

        if ( !idBodyToFirstAppearanceFrame_.ContainsKey(idBody) )
        {
          idBodyToFirstAppearanceFrame_.Add(idBody, currentBakeFrame_);
        }
      }

      if (!removeInvisibleBodiesFromBake_)
      {
        setVisibleBodies_.Add(idBody);
      }

      if (setBakedBodies_.Contains(idBody) )
      {
        if (isVisible || isGhost)
        {
          estimatedAnimationBytesSize_ += 1 + rqBytes; //visibility byte + translation + rotation.

          if ( type == BD_TYPE.BODYMESH_ANIMATED_BY_VERTEX ||
               type == BD_TYPE.SOFTBODY ||
               type == BD_TYPE.CLOTH )
          {
            estimatedAnimationBytesSize_ += EstimateVertexAnimatedFrameBytes(idBody);
          }
        }
        else
        {
          estimatedAnimationBytesSize_ += 1;
        }
      }
    }
    //----------------------------------------------------------------------------------
    private uint EstimateVertexAnimatedFrameBytes(uint idBody)
    {
      uint frameBytes = 0;

      uint vertexCount = 0;
      uint bonesCount  = 0;

      if (entityManager_.IsRope(idBody) && skinRopes_)
      { 
        bonesCount = entityManager_.GetSimulatingRopeBoneCount(idBody);
      }
      else if (entityManager_.IsCloth(idBody) && skinClothes_ )
      {
        bonesCount = entityManager_.GetSimulatingClothBoneCount(idBody);
      }
      else
      {
        vertexCount = entityManager_.GetSimulatingVertexCount(idBody);
      }

      if (bonesCount > 0)
      {
        frameBytes += bonesCount * (boneBytes);
      }

      if (vertexCount > 0)
      {
        if (vertexCompression_)
        {
          if (vertexCompressionMode_ == EVertexCompressionMode.Box)
          {
            frameBytes += boxSizeBytes;
            frameBytes += vertexCount * (boxPosVertexBytes + boxNorVertexBytes);
            if (vertexTangents_)
            {
              frameBytes += vertexCount * boxTanVertexBytes;
            }
          }
          else if (vertexCompressionMode_ == EVertexCompressionMode.Fiber)
          {
            frameBytes += boxSizeBytes;
            frameBytes += vertexCount * fiberPosVertexBytes;
          }
        }
        else
        {
          frameBytes += vertexCount * (posVertexBytes + norVertexBytes);
          if (vertexTangents_)
          {
            frameBytes += vertexCount * tanVertexBytes;
          }
        }
      }
      return frameBytes;
    }
    //----------------------------------------------------------------------------------
    private void SetStartData()
    {
      float startTime = frameStart_ * FrameTime;

      foreach( var pair in idBodyToVisibilityInterval_ )
      {
        uint idBody = pair.Key;
        Vector2 visibilityInterval = pair.Value;

        GameObject go = idBodyToBakedGO_[idBody];

        if (bakeVisibility_)
        {
          bool isVisible = startTime >= visibilityInterval.x && startTime <= visibilityInterval.y;

          if ( setBoneBodies_.Contains(idBody) )
          {     
            if (!isVisible)
            {
              go.transform.localScale = Vector3.zero;
            }
          }
          else
          {
            go.SetActive( isVisible );
          }
        }
        else
        {
          if ( setBoneBodies_.Contains(idBody) )
          {     
            go.transform.localScale = Vector3.one;
          }
          else
          {
            go.SetActive( true );
          }
        }
      }
    }
    //----------------------------------------------------------------------------------
    private void CalculateMeshData( uint idBody, Vector3[] arrVertices, out Vector3[] arrNormal, out Vector4[] arrTangent)
    {
      Tuple2<UnityEngine.Mesh, MeshUpdater> meshData = entityManager_.GetBodyMeshRenderUpdaterRef(idBody);

      UnityEngine.Mesh mesh   = meshData.First;
      MeshUpdater meshUpdater = meshData.Second;

      mesh.vertices = arrVertices;
      meshComplexForUpdate_.Set( mesh, null );

      CaronteSharp.Tools.UpdateVertexNormalsAndTangents( meshUpdater, meshComplexForUpdate_ );
      
      mesh.normals  = meshComplexForUpdate_.arrNormal_;
      mesh.tangents = meshComplexForUpdate_.arrTan_;

      arrNormal   = meshComplexForUpdate_.arrNormal_;
      arrTangent  = meshComplexForUpdate_.arrTan_;
    }
    //----------------------------------------------------------------------------------
    private void BakeAnimBinaryFile()
    {
      int nGameObjects = idBodyToGOKeyframe_.Count;

      CarMemoryStream ms = new CarMemoryStream((int)estimatedAnimationBytesSize_);
      if (ms != null)
      {
        BinaryWriter bw = new BinaryWriter(ms);
        if (bw != null)
        {
          // write header data:
          //_________________________________________________________________________________
          bw.Write(binaryVersion_);
          bw.Write(vertexCompression_);
          bw.Write(vertexTangents_);
          bw.Write(frameCount_);
          bw.Write(FPS);
          bw.Write(nGameObjects);

          EFileHeaderFlags fileheaderFlags = EFileHeaderFlags.SKINNEDBOXES;
          if (alignData_)
          {
            fileheaderFlags |= EFileHeaderFlags.ALIGNEDDATA;
          }

          if (vertexCompression_)
          {
            if (vertexCompressionMode_ == EVertexCompressionMode.Box)
            {
              fileheaderFlags |= EFileHeaderFlags.BOXCOMPRESSION;
            }
            else if (vertexCompressionMode_ == EVertexCompressionMode.Fiber)
            {
              fileheaderFlags |= EFileHeaderFlags.FIBERCOMPRESSION;
              fileheaderFlags |= EFileHeaderFlags.VERTEXLOCALSYSTEMS;
            }
          }

          bool isFiberCompression = fileheaderFlags.IsFlagSet(EFileHeaderFlags.FIBERCOMPRESSION);
          bw.Write((uint)fileheaderFlags);

          Dictionary<uint, int> idBodyToIdGameObjectInFile = new Dictionary<uint, int>();
          List<TGOFrameData> listGOFrameData = new List<TGOFrameData>();

          foreach (var element in idBodyToGOKeyframe_)
          {
            uint idBody = element.Key;
            CarGOKeyframe goKeyframe = element.Value;

            GameObject go = idBodyToBakedGO_[idBody];

            idBodyToIdGameObjectInFile.Add(idBody, listGOFrameData.Count);
            listGOFrameData.Add( new TGOFrameData(idBody, go.transform, goKeyframe) );
          }

          // setup fiber compression:
          //_________________________________________________________________________________
          int updateFramesDelta = Mathf.Min(Mathf.Max(frameCount_ / 5, 1), 5);
          int frame = 0;
          VerticesAnimationCompressor[] arrVerticesAnimationCompressor = new VerticesAnimationCompressor[nGameObjects];

          if (isFiberCompression)
          {
            for (int i = 0; i < listGOFrameData.Count; i++)
            {
              TGOFrameData goFrameData = listGOFrameData[i];
              uint idBody              = goFrameData.First;
              Transform tr             = goFrameData.Second;
              CarGOKeyframe goKeyframe = goFrameData.Third;

              arrVerticesAnimationCompressor[i] = null;
              if (goKeyframe.VertexCount > 0)
              {
                if (idBodyToFirstAppearanceFrame_.ContainsKey(idBody))
                {
                  uint firstAppearanceFrame = idBodyToFirstAppearanceFrame_[idBody];

                  if (currentBakeFrame_ != idBodyToFirstAppearanceFrame_[idBody])
                  {
                    BakeGOKeyFrames((int)firstAppearanceFrame);
                  }
                  CarFrameWriterUtils.InitVerticesAnimationCompressor(tr, goKeyframe, ref arrVerticesAnimationCompressor[i]);
                }   
              }
            }

            EditorUtility.DisplayProgressBar("CaronteFX - Bake animation", "Bake first pass. ", 0f);
            for (int bakingFrame_ = frameStart_; bakingFrame_ <= frameEnd_; bakingFrame_++)
            {
              if ((frame % updateFramesDelta) == 0)
              {
                EditorUtility.DisplayProgressBar("CaronteFX - Bake animation", "Bake first pass. Frame " + bakingFrame_ + ".", (float)bakingFrame_ / (float)frameEnd_);
              }

              BakeGOKeyFrames(bakingFrame_);

              for (int j = 0; j < listGOFrameData.Count; j++)
              {
                TGOFrameData goFrameData = listGOFrameData[j];
                CarGOKeyframe goKeyframe = goFrameData.Third;
                VerticesAnimationCompressor vac = arrVerticesAnimationCompressor[j];
                CarFrameWriterUtils.VerticesAnimationCompressorFirstPass(goKeyframe, vac);
              }

              frame++;
            }
          }

          // bake animated game objects header data:
          //_________________________________________________________________________________
          BakeGOKeyFrames(frameStart_);
          VertexNormalsFastUpdater vnfu = new VertexNormalsFastUpdater();

          foreach (var element in idBodyToGOKeyframe_)
          {
            uint idBody = element.Key;
            CarGOKeyframe goKeyframe = element.Value;

            int idx = idBodyToIdGameObjectInFile[idBody];

            string pathRelativeTo       = idBodyToRelativePath_[idBody];
            int vertexCount             = goKeyframe.VertexCount;
            int boneCount               = goKeyframe.BoneCount;
            Vector2 visibleTimeInterval = idBodyToVisibilityInterval_[idBody];

            bw.Write(pathRelativeTo);
            bw.Write(vertexCount);
            bw.Write(boneCount);

            bw.Write(visibleTimeInterval.x);
            bw.Write(visibleTimeInterval.y);

            VerticesAnimationCompressor vac = arrVerticesAnimationCompressor[idx];
            if (vac != null)
            {
              byte[] definitionBytes = vac.GetDefinitionAsBytes();
              bw.Write(definitionBytes);
            }


            // write vertex local systems:
            //_________________________________________________________________________________
            if (isFiberCompression && vertexCount > 0)
            {
              TGOFrameData goFrameData = listGOFrameData[idx];

              Transform tr = goFrameData.Second;
              GameObject go = tr.gameObject;

              Mesh mesh = go.GetMesh();
              if (mesh != null)
              {
                vnfu.Calculate(mesh.vertices, mesh.normals, mesh.triangles);
                byte[] vertexDataBytes = vnfu.VertexDataAsBytes();
                bw.Write(vertexDataBytes);
              }
            }


            // write bone relative paths:
            //_________________________________________________________________________________
            if (boneCount > 0)
            {
              string[] bonesRelativePaths = idBodyToBonesRelativePaths_[idBody];
              for (int i = 0; i < bonesRelativePaths.Length; i++)
              {
                bw.Write(bonesRelativePaths[i]);
              }
            }
          }

          vnfu.Dispose();

          // write skinned gameobject header data:
          //_________________________________________________________________________________
          int nSkinnedGameObjects = bakedSkinnedGOToListIdBody_.Count;
          bw.Write(nSkinnedGameObjects);
          foreach (var pair in bakedSkinnedGOToListIdBody_)
          {
            GameObject bakedSkinnedGO = pair.Key;
            string skinnedRelativePath = bakedSkinnedGO.GetRelativePathTo(rootGameObject_);
            bw.Write(skinnedRelativePath);
          }

          // write contact emmiter data:
          //_________________________________________________________________________________
          Dictionary<uint, CNContactEmitter> tableIdToEventEmitter = entityManager_.GetTableIdToContactEmitter();
          bw.Write(tableIdToEventEmitter.Count);

          Dictionary<uint, int> idContactEmitterToIdEmitterInFile = new Dictionary<uint, int>();
          List<CNContactEmitter> listContactEmitter = new List<CNContactEmitter>();

          foreach (var pair in tableIdToEventEmitter)
          {
            uint idContactEmitter = pair.Key;
            CNContactEmitter ceNode = pair.Value;

            idContactEmitterToIdEmitterInFile.Add(idContactEmitter, listContactEmitter.Count);
            listContactEmitter.Add(ceNode);
            string contactEmitterName = ceNode.Name;

            bw.Write(contactEmitterName);
          }

          // write space for frame offsets :
          //_________________________________________________________________________________
          long[] fOffsets = new long[frameCount_];
          long fOffsetsP = ms.Position;
          for (int i = 0; i < frameCount_; i++)
          {
            long val = 0;
            bw.Write(val);
          }

          // write frames:
          //_________________________________________________________________________________
          frame = 0;
          for (int bakingFrame_ = frameStart_; bakingFrame_ <= frameEnd_; bakingFrame_++)
          {
            if ((frame % updateFramesDelta) == 0)
            {
              float progress = (float)frame / (float)frameCount_;
              string progressInfo = "Baking to " + rootGameObject_.name + ". " + "Frame " + frame + " of " + frameCount_ + ".";
              EditorUtility.DisplayProgressBar("CaronteFx - Baking animation Clip (BinaryFile)", progressInfo, progress);
            }

            fOffsets[frame] = ms.Position;
            BakeGOKeyFrames(bakingFrame_);

            // write gameobjects:
            //_________________________________________________________________________________
            for (int j = 0; j < listGOFrameData.Count; j++)
            {
              CarGOKeyframe goKeyframe = listGOFrameData[j].Third;
              VerticesAnimationCompressor vac = arrVerticesAnimationCompressor[j];
              CarFrameWriterUtils.WriteGOKeyframe(goKeyframe, vac, vertexCompression_, vertexTangents_, fileheaderFlags, ms, bw);
            }

            // write skinned objects boxes:
            //_________________________________________________________________________________
            for (int j = 0; j < listBakedSkinnedGOLocalBounds_.Count; j++)
            {
              Bounds skinnedLocalBounds = listBakedSkinnedGOLocalBounds_[j];

              Vector3 center = skinnedLocalBounds.center;

              bw.Write(center.x);
              bw.Write(center.y);
              bw.Write(center.z);

              Vector3 size = skinnedLocalBounds.size;

              bw.Write(size.x);
              bw.Write(size.y);
              bw.Write(size.z);
            }

            // write frame contact events:
            //_________________________________________________________________________________
            WriteFrameContactEvents(idBodyToIdGameObjectInFile, idContactEmitterToIdEmitterInFile, bakeEvents_, ms, bw);
            frame++;
          }

          // write actual frame offsets
          //_________________________________________________________________________________
          ms.Position = fOffsetsP;
          for (int i = 0; i < frameCount_; i++)
          {
            bw.Write(fOffsets[i]);
          }

          EditorUtility.ClearProgressBar();

          CreateAnimationAsset(ms);

          bw.Close();
          ms.Close();
        }
      }
    }
    //----------------------------------------------------------------------------------
    public void WriteFrameContactEvents(Dictionary<uint, int> idBodyToIdGameObjectInFile, Dictionary<uint, int> idContactEmitterToIdEmitterInFile, bool bakeEvents, Stream stream, BinaryWriter bw)
    {
      int nFrameEvents = 0;
      long nFrameEventsPos = stream.Position;
      bw.Write(nFrameEvents);

      if (bakeEvents)
      {
        List<CaronteSharp.ContactEventInfo> listFrameEventInfo = SimulationManager.listContactEventInfo_;

        foreach (CaronteSharp.ContactEventInfo evInfo in listFrameEventInfo)
        {
          uint idBodyA = evInfo.a_bodyId_;
          uint idBodyB = evInfo.b_bodyId_;

          if (idBodyToIdGameObjectInFile.ContainsKey(idBodyA) &&
              idBodyToIdGameObjectInFile.ContainsKey(idBodyB))
          {
            uint idEntity = evInfo.idEntity_;
            int idEmitterInFile = idContactEmitterToIdEmitterInFile[idEntity];
            bw.Write(idEmitterInFile);

            bw.Write((int)CRAnimationEvData.EEventDataType.Contact);

            int idGameObjectInFileA = idBodyToIdGameObjectInFile[idBodyA];
            int idGameObjectInFileB = idBodyToIdGameObjectInFile[idBodyB];

            bw.Write(idGameObjectInFileA);
            bw.Write(idGameObjectInFileB);
          }
          else
          {
            continue;
          }
          bw.Write(evInfo.position_.x);
          bw.Write(evInfo.position_.y);
          bw.Write(evInfo.position_.z);

          bw.Write(evInfo.a_v_.x);
          bw.Write(evInfo.a_v_.y);
          bw.Write(evInfo.a_v_.z);

          bw.Write(evInfo.b_v_.x);
          bw.Write(evInfo.b_v_.y);
          bw.Write(evInfo.b_v_.z);

          bw.Write(evInfo.relativeSpeed_N_);
          bw.Write(evInfo.relativeSpeed_T_);

          bw.Write(evInfo.relativeP_N_);
          bw.Write(evInfo.relativeP_T_);

          nFrameEvents++;
        }
      }

      long currentPos = stream.Position;
      stream.Position = nFrameEventsPos;
      bw.Write(nFrameEvents);
      stream.Position = currentPos;
    }
    //----------------------------------------------------------------------------------
    private void CreateAnimationAsset(CarMemoryStream ms)
    {

      if (ms.Length > maxAnimationBytesSize)
      {
        EditorUtility.DisplayDialog("CaronteFX - Animation is too big", "Animation exceeds the maximum size (2GB), consider splitting the animation into parts", "Ok");
        return;
      }

      AssetDatabase.Refresh();

      CRAnimation crAnimation = rootGameObject_.GetComponent<CRAnimation>();
      if (animationFileType_ == EAnimationFileType.CRAnimationAsset)
      {
        CRAnimationAsset animationAsset = CRAnimationAsset.CreateInstance<CRAnimationAsset>();
        animationAsset.Bytes = ms.ToArray();

        string assetFilePath = assetsPath_ + "/" + animationName_ + ".asset";
        AssetDatabase.CreateAsset(animationAsset, assetFilePath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        crAnimation.AddAnimationAndSetActive(animationAsset);
      }
      else if (animationFileType_ == EAnimationFileType.TextAsset)
      {
        string assetFilePath = assetsPath_ + "/" + animationName_ + ".bytes";

        TextAsset existingAsset = (TextAsset)AssetDatabase.LoadAssetAtPath(assetFilePath, typeof(TextAsset) );
        if (existingAsset != null)
        {
          CRAnimationsManager instance = CRAnimationsManager.Instance;
          instance.RemoveCachedAnim(existingAsset);

          AssetDatabase.DeleteAsset(assetFilePath);
          AssetDatabase.SaveAssets();
          AssetDatabase.Refresh();
        }

        FileStream fs = new FileStream(assetFilePath, FileMode.Create);
        ms.WriteTo(fs);
        fs.Close();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        TextAsset crAnimationText = (TextAsset)AssetDatabase.LoadAssetAtPath(assetFilePath, typeof(TextAsset));
        crAnimation.AddAnimationAndSetActive(crAnimationText);
      }

      EditorUtility.SetDirty(crAnimation);
    }
    //----------------------------------------------------------------------------------
  }
}