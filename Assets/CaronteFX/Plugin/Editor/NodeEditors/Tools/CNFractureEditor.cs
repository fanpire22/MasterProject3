using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using CaronteSharp;

namespace CaronteFX
{
  public class CNFractureEditor : CNMeshToolEditor
  {
    private static readonly GUIContent frGlobalPatternCt_                      = new GUIContent(CarStringManager.GetString("FrGlobalPattern"),                      CarStringManager.GetString("FrGlobalPatternTooltip"));
    private static readonly GUIContent frNDesiredPiecesCt_                     = new GUIContent(CarStringManager.GetString("FrRoughNumberOfPieces"),                CarStringManager.GetString("FrRoughNumberOfPiecesTooltip"));
    private static readonly GUIContent frSeedCt_                               = new GUIContent(CarStringManager.GetString("FrSeed"),                               CarStringManager.GetString("FrSeedTooltip"));
    private static readonly GUIContent frInteriorMaterialCt_                   = new GUIContent(CarStringManager.GetString("FrInteriorFacesMaterial"),              CarStringManager.GetString("FrInteriorFacesMaterialTooltip"));
    private static readonly GUIContent frInteriorFacesSmoothingAngleCt_        = new GUIContent(CarStringManager.GetString("FrInteriorFacesSmoothingAngle"),        CarStringManager.GetString("FrInteriorFacesSmoothingAngleTooltip"));
    private static readonly GUIContent frGenerateNewSubmeshForInteriorFacesCt_ = new GUIContent(CarStringManager.GetString("FrGenerateNewSubmeshForInteriorFaces"), CarStringManager.GetString("FrGenerateNewSubmeshForInteriorFacesTooltip"));
    private static readonly GUIContent frModifyInteriorFacesUVsCt_             = new GUIContent(CarStringManager.GetString("FrModifyInteriorFacesUVs"),             CarStringManager.GetString("FrModifyInteriorFacesUVsTooltip"));

    private static readonly GUIContent frSteeringGeometryCt_    = new GUIContent(CarStringManager.GetString("FrSteeringGeometry"), CarStringManager.GetString("FrSteeringGeometryTooltip"));
    private static readonly GUIContent frSteeringGeometriesCt_  = new GUIContent(CarStringManager.GetString("FrSteeringGeometries"), CarStringManager.GetString("FrSteeringGeometriesTooltip"));
    private static readonly GUIContent frGridResolutionCt_      = new GUIContent(CarStringManager.GetString("FrSteeringResolution"), CarStringManager.GetString("FrSteeringResolutionTooltip"));
    private static readonly GUIContent frFocusModeCt_           = new GUIContent(CarStringManager.GetString("FrFocusMode"),          CarStringManager.GetString("FrFocusModeTooltip"));  
    private static readonly GUIContent frDensityRateCt_         = new GUIContent(CarStringManager.GetString("FrDensityRate"),        CarStringManager.GetString("FrDensityRateTooltip"));
    private static readonly GUIContent frTransitionLengthCt_    = new GUIContent(CarStringManager.GetString("FrTransitionLength"),   CarStringManager.GetString("FrTransitionLengthTooltip"));

    private static readonly GUIContent frReferenceSystemCt_     = new GUIContent(CarStringManager.GetString("FrReferenceSystem"),     CarStringManager.GetString("FrReferenceSystemTooltip"));
    private static readonly GUIContent frReferenceSystemAxisCt_ = new GUIContent(CarStringManager.GetString("FrFrontDirection"),      CarStringManager.GetString("FrFrontDirectionTooltip"));
    private static readonly GUIContent frRaysNumberCt_          = new GUIContent(CarStringManager.GetString("FrRaysNumber"),          CarStringManager.GetString("FrRaysNumberTooltip"));
    private static readonly GUIContent frRaysRateRandCt_        = new GUIContent(CarStringManager.GetString("FrRaysAngleRandomness"), CarStringManager.GetString("FrRaysAngleRandomnessTooltip"));

    private static readonly GUIContent frUseInternalRadiusCt_        = new GUIContent(CarStringManager.GetString("FrUseInternalRadius"),       CarStringManager.GetString("FrUseInternalRadiusTooltip"));
    private static readonly GUIContent frRingsIntRadiusCt_           = new GUIContent(CarStringManager.GetString("FrRingIntRadius"),           CarStringManager.GetString("FrRingIntRadiusTooltip"));
    private static readonly GUIContent frRingsIntTransitionLengthCt_ = new GUIContent(CarStringManager.GetString("FrRingIntTransitionLength"), CarStringManager.GetString("FrRingIntTransitionLengthTooltip"));
    private static readonly GUIContent frRingsIntTransitionDecayCt_  = new GUIContent(CarStringManager.GetString("FrRingIntTransitionDecay"),  CarStringManager.GetString("FrRintIntTransitionDecayTooltip"));

    private static readonly GUIContent frRingsExtRadiusCt_           = new GUIContent(CarStringManager.GetString("FrRingExtRadius"),           CarStringManager.GetString("FrRingExtRadiusTooltip"));
    private static readonly GUIContent frRingsExtTransitionLengthCt_ = new GUIContent(CarStringManager.GetString("FrRingExtTransitionLength"), CarStringManager.GetString("FrRingExtTransitionLengthTooltip"));
    private static readonly GUIContent frRingsExtTransitionDecayCt_  = new GUIContent(CarStringManager.GetString("FrRingExtTransitionDecay"),  CarStringManager.GetString("FrRintExtTransitionDecayTooltip"));

    private static readonly GUIContent frRingsNumberInsideAnnulusCt_ = new GUIContent(CarStringManager.GetString("FrRingsNumberInsideAnnulus"), CarStringManager.GetString("FrRingsNumberInsideAnnulusTooltip"));
    private static readonly GUIContent frRingsRateRandCt_            = new GUIContent(CarStringManager.GetString("FrRingsRadiusRandomness"),    CarStringManager.GetString("FrRingsRadiusRandomnessTooltip"));
    private static readonly GUIContent frCreateCentralPieceCt_       = new GUIContent(CarStringManager.GetString("FrCreateCentralPiece"),       CarStringManager.GetString("FrCreateCentralPieceTooltip"));

    private static readonly GUIContent frNoiseRateCt_ = new GUIContent(CarStringManager.GetString("FrNoise"), CarStringManager.GetString("FrNoiseTooltip"));
    private static readonly GUIContent frTwistRateCt_ = new GUIContent(CarStringManager.GetString("FrTwist"), CarStringManager.GetString("FrTwistTooltip"));

    private static readonly GUIContent frDoExtrusionEffectCt_ = new GUIContent(CarStringManager.GetString("FrExtrusionEffect"), CarStringManager.GetString("FrExtrusionEffectTooltip"));
    private static readonly GUIContent frDoCoordinateCt_      = new GUIContent(CarStringManager.GetString("FrCoordinate"), CarStringManager.GetString("FrCoordinateTooltip"));

    private static readonly GUIContent frHideParentObjectsAutoCt_ = new GUIContent(CarStringManager.GetString("FrAutoHideOriginalObjects"), CarStringManager.GetString("FrAutoHideOriginalObjectsTooltip"));

    private static readonly GUIContent frRectrictFractureZoneCt_  = new GUIContent(CarStringManager.GetString("FrRestrictFractureZone"), CarStringManager.GetString("FrRestrictFractureZoneTooltip"));
    private static readonly GUIContent frResctrictGeometryCt_     = new GUIContent(CarStringManager.GetString("FrRestrictionGeometry"),  CarStringManager.GetString("FrRestrictionGeometryTooltip"));
    private static readonly GUIContent frResctrictGeometriesCt_   = new GUIContent(CarStringManager.GetString("FrRestrictionGeometries"),  CarStringManager.GetString("FrRestrictionGeometriesTooltip"));
    private static readonly GUIContent frFractureZoneCt_          = new GUIContent(CarStringManager.GetString("FrFractureZone"),         CarStringManager.GetString("FrFractureZoneTooltip"));
    private static readonly GUIContent frUnfracturedMeshCt_       = new GUIContent(CarStringManager.GetString("FrUnfracturedMesh"),      CarStringManager.GetString("FrUnfracturedMeshTooltip"));

    private static readonly GUIContent frInteriorFacesNoiseCt_           = new GUIContent(CarStringManager.GetString("FrInteriorFacesNoise"),          CarStringManager.GetString("FrInteriorFacesNoiseTooltip"));
    private static readonly GUIContent frNoiseResolutionRateCt_          = new GUIContent(CarStringManager.GetString("FrNoiseResolutionRate"),         CarStringManager.GetString("FrNoiseResolutionRateTooltip"));
    private static readonly GUIContent frNoiseReliefAmplitudeRateCt_     = new GUIContent(CarStringManager.GetString("FrNoiseReliefAmplitureRate"),    CarStringManager.GetString("FrNoiseReliefAmplitureRateTooltip"));
    private static readonly GUIContent frNoiseReliefSerrationRateCt_     = new GUIContent(CarStringManager.GetString("FrNoiseReliefSerrationRate"),    CarStringManager.GetString("FrNoiseReliefSerrationRateTooltip"));
    private static readonly GUIContent frNoiseSeedCt_                    = new GUIContent(CarStringManager.GetString("FrNoiseSeed"),                   CarStringManager.GetString("FrNoiseSeedTooltip") );
    private static readonly GUIContent frDoNoiseExtrusionEffectCt_       = new GUIContent(CarStringManager.GetString("FrDoNoiseExtrusionEffect"),     CarStringManager.GetString("FrDoNoiseExtrusionEffectTooltip"));

    private static readonly GUIContent frOutputSectionCt_    = new GUIContent(CarStringManager.GetString("FrOutputSection"));
    private static readonly GUIContent frSelectPiecesCt_     = new GUIContent(CarStringManager.GetString("FrSelectPieces"),       CarStringManager.GetString("FrSelectPiecesTooltip"));
    private static readonly GUIContent frActivateInputOnly_  = new GUIContent(CarStringManager.GetString("FrActivateInputOnly"),  CarStringManager.GetString("FrActivateInputOnlyTooltip"));
    private static readonly GUIContent frActivateOutputOnly_ = new GUIContent(CarStringManager.GetString("FrActivateOutputOnly"), CarStringManager.GetString("FrActivateOutputOnlyTooltip")); 
    private static readonly GUIContent frPushRateCt_         = new GUIContent(CarStringManager.GetString("FrPushRate"),           CarStringManager.GetString("FrPushRateTooltip"));
    private static readonly GUIContent frPushMultiplierCt_   = new GUIContent(CarStringManager.GetString("FrPushMultiplier"),     CarStringManager.GetString("FrPushMultiplierTooltip"));

    private static readonly GUIContent[] arrTabName_         = new GUIContent[] { new GUIContent( CarStringManager.GetString("FrTabParameters")),          new GUIContent(CarStringManager.GetString("FrTabStats")) };

    private static readonly GUIContent[] arrRestrictionMode_ = new GUIContent[] { new GUIContent( CarStringManager.GetString("FrStrictlyInside")),
                                                                                  new GUIContent( CarStringManager.GetString("FrInsidePlusBoundary") ),
                                                                                  new GUIContent( CarStringManager.GetString("FrStrictlyOutside") ),
                                                                                  new GUIContent( CarStringManager.GetString("FrOutsidePlusBoundary") )
                                                                                };

    private static readonly GUIContent[] arrUnfracturedZoneNames_ = new GUIContent[] { new GUIContent(CarStringManager.GetString("FrUniteDisconnectedParts")), new GUIContent(CarStringManager.GetString("FrSeparateDisconnectedParts"))  };

    public static Texture icon_uniform_;
    public static Texture icon_geometry_;
    public static Texture icon_radial_;

    public override Texture TexIcon 
    { 
      get
      { 
        switch (Data.ChopMode)
        {
        case CNFracture.CHOP_MODE.VORONOI_UNIFORM:
          return icon_uniform_;

        case CNFracture.CHOP_MODE.VORONOI_BY_GEOMETRY:
          return icon_geometry_;

        case CNFracture.CHOP_MODE.VORONOI_RADIAL:
          return icon_radial_;
        
        default:
          return null;
        }
      } 
    }
   
    int   tabIndex_;
    int   cropModeIdx_;
    float pushRate_;
    float pushMultiplier_;

    CNFieldController FieldControllerRestriction { get; set; }
    CNFieldController FieldControllerSteering { get; set; }

    new CNFracture Data { get; set; }

    public CNFractureEditor( CNFracture data, CommandNodeEditorState state )
      : base(data, state)
    {
      Data = (CNFracture)data;
    }

    public override void Init()
    {
      base.Init();
                                        
      FieldControllerRestriction = new CNFieldController( Data, Data.FieldRestrictionGeometry, eManager, goManager );
      FieldControllerRestriction.SetFieldContentType( CNFieldContentType.Geometry );
      FieldControllerRestriction.IsBodyField = false;
      FieldControllerRestriction.SetScopeType(CNField.ScopeFlag.Global);

      FieldControllerSteering = new CNFieldController( Data, Data.FieldSteeringGeometry, eManager, goManager );
      FieldControllerSteering.SetFieldContentType( CNFieldContentType.Geometry );
      FieldControllerSteering.IsBodyField = false;
      FieldControllerRestriction.SetScopeType(CNField.ScopeFlag.Global);
    }

    public override void LoadInfo()
    {
      base.LoadInfo();

      FieldControllerRestriction.RestoreFieldInfo();
      FieldControllerSteering.RestoreFieldInfo();
    }
    //----------------------------------------------------------------------------------
    public override void StoreInfo()
    {
      base.StoreInfo();

      FieldControllerRestriction.StoreFieldInfo();
      FieldControllerSteering.StoreFieldInfo();
    }
    //----------------------------------------------------------------------------------
    public override void BuildListItems()
    {
      base.BuildListItems();

      FieldControllerRestriction.BuildListItems();
      FieldControllerSteering.BuildListItems();
    }
    //----------------------------------------------------------------------------------
    public override bool RemoveNodeFromFields( CommandNode node )
    {
      Undo.RecordObject(Data, "CaronteFX - Remove node from fields");
      bool removed = Data.Field.RemoveNode(node);
      bool removedFromRestriction = Data.FieldRestrictionGeometry.RemoveNode(node);
      bool removedFromSteering    = Data.FieldSteeringGeometry.RemoveNode(node);
      return ( removed || removedFromRestriction || removedFromSteering );
    }
    //----------------------------------------------------------------------------------
    public override void SetScopeId(uint scopeId)
    {
      base.SetScopeId(scopeId);

      FieldControllerRestriction.SetScopeId(scopeId);
      FieldControllerSteering.SetScopeId(scopeId);
    }
    //----------------------------------------------------------------------------------
    public override void FreeResources()
    {
      base.FreeResources();

      FieldControllerRestriction.DestroyField();
      FieldControllerSteering.DestroyField();
    }
    //----------------------------------------------------------------------------------
    public void CheckUpdate(out int[] unityIdsAdded, out int[] unityIdsRemoved)
    {
      unityIdsAdded   = null;
      unityIdsRemoved = null;
      Data.NeedsUpdate = false;                
    }

    protected override void LoadState()
    {
      base.LoadState();

      pushRate_       = Data.PushRate;
      pushMultiplier_ = Data.PushMultiplier;
    }

    public override void ValidateState()
    {
      base.ValidateState();

      if ( (pushRate_ != Data.PushRate || pushMultiplier_ != Data.PushMultiplier) )
      {
        SeparatePieces();

        pushRate_       = Data.PushRate;
        pushMultiplier_ = Data.PushMultiplier;
      }  
    }

    public bool IsFracturableMesh(Mesh mesh)
    {
      Vector3[] arrNor = mesh.normals;
      Vector3[] arrPos = mesh.vertices;

      if ( arrPos == null || arrNor == null ||
           arrPos.Length == 0 || arrNor.Length == 0 ||
           arrPos.Length != arrNor.Length )
      {
        return false;
      }

      return true;
    }


    public void Chop()
    {
      GameObject[] goToChop = FieldController.GetUnityGameObjects();
      int numObjects = goToChop.Length;

      string errorMessage = string.Empty;

      if (!CarVersionChecker.IsVersionPeriodActive())
      {
        errorMessage = "The working period of this version has expired, you are not allowed to use this version of CaronteFX anymore.\n";
      }

      if ( numObjects == 0 )
      {
        errorMessage = "Objects field must contain at least one object with geometry";
      }

      if ( Data.RestrictFractureZone )
      {
        if ( !FieldControllerRestriction.HasGameObjects() ) 
        {
          errorMessage = "Restriction geometries field must contain at least a gameobject with mesh";
        }
      }

      if (Data.ChopMode == CNFracture.CHOP_MODE.VORONOI_BY_GEOMETRY)
      {
        if ( !FieldControllerSteering.HasGameObjects() ) 
        {
          errorMessage = "Specifying a steering geometry is mandatory";
        }
      }

      if (Data.ChopMode == CNFracture.CHOP_MODE.VORONOI_RADIAL)
      {
        if ( Data.ReferenceSystem == null )
        {
          errorMessage = "Specifying the reference system is mandatory";
        } 
      }

      for ( int i = 0; i < numObjects; i++ )
      {
        GameObject go = goToChop[i];
        Mesh mesh = go.GetMesh();

        if (mesh != null)
        {
          if (!IsFracturableMesh( mesh ))
          {
            errorMessage = "All input meshes mush have positions and normals";
            break;
          }
        }
      }
      
      if ( errorMessage != string.Empty )
      {
        EditorUtility.DisplayDialog("CaronteFX", errorMessage, "Ok");
        return;
      }

      Undo.RecordObject(Data, "Chop - " + Data.Name);

      List<GameObject>  listParentGO           = new List<GameObject>();
      List<MeshComplex> listParentMesh_car     = new List<MeshComplex>();
      List<Matrix4x4>   listMatrixModelToWorld = new List<Matrix4x4>();
      List<Material>    listMaterial           = new List<Material>();

      Dictionary<Material, int> dictMaterialMaterialIdx = new Dictionary<Material,int>();

      for (int i = 0; i < numObjects; i++)
      {
        GameObject go = goToChop[i];

        Mesh un_mesh;
        bool wasBaked = go.GetMeshOrBakedMesh( out un_mesh );

        if (un_mesh != null)
        {
          Renderer rn = go.GetComponent<Renderer>();
          Material[] arrMaterial = rn.sharedMaterials;

          List<int> listMeshMaterialIdx = new List<int>();
         
          for (int m = 0; m < arrMaterial.Length; m++)
          {
            Material material = arrMaterial[m];

            if (material == null)
            {
              listMeshMaterialIdx.Add(-2);
              continue;
            }

            if (!dictMaterialMaterialIdx.ContainsKey(material))
            {
              dictMaterialMaterialIdx[material] = listMaterial.Count;  
              listMaterial.Add(material);
            }

            int materialIdx = dictMaterialMaterialIdx[material];
            listMeshMaterialIdx.Add(materialIdx);
          }


          MeshComplex mc = new MeshComplex();
          mc.Set( un_mesh, listMeshMaterialIdx.ToArray() );

          if (wasBaked)
          {
            Object.DestroyImmediate(un_mesh);
          }

          listParentMesh_car.Add( mc );
          listParentGO      .Add( go );
          listMatrixModelToWorld.Add( go.transform.localToWorldMatrix );
        }
      }

      Bounds globalBounds = CarEditorUtils.GetGlobalBoundsWorld( listParentGO );
      Vector3 centerWorld = globalBounds.center;

      ChopRequest cr = new ChopRequest();

      cr.doKeepUVCoords_          = true;
      cr.doKeepVertexNormals_     = true;
      cr.arrMeshToChop_           = listParentMesh_car.ToArray();
      cr.arrMatrixModelToWorld_   = listMatrixModelToWorld.ToArray();
      
      bool isOnePieceInputOnly = listParentMesh_car.Count == 1;

      cr.doGlobalPattern_                           = Data.DoGlobalPattern && !isOnePieceInputOnly;
      cr.seed_                                      = (uint)Data.Seed;
      cr.smoothingAngle_internalFacesVertexNormals_ = Data.InternalFacesSmoothingAngle * Mathf.Deg2Rad;

      bool chopModeUniform  = false;
      bool chopModeGeometry = false;
      bool chopModeRadial   = false;

      cr.pProgressFunction_ = null;

      switch (Data.ChopMode)
      {
        case CNFracture.CHOP_MODE.VORONOI_UNIFORM:
          cr.chopMode_ = CaronteSharp.CP_CHOP_MODE.CP_CHOP_MODE_VORONOI_UNIFORM;
          chopModeUniform = ChopModeUniform( cr );
          break;

        case CNFracture.CHOP_MODE.VORONOI_BY_GEOMETRY:
          cr.chopMode_ = CaronteSharp.CP_CHOP_MODE.CP_CHOP_MODE_VORONOI_BY_STEERING_GEOMETRY;
          chopModeGeometry = ChopModeGeometry( cr, centerWorld );
          break;

        case CNFracture.CHOP_MODE.VORONOI_RADIAL:
          cr.chopMode_ = CaronteSharp.CP_CHOP_MODE.CP_CHOP_MODE_VORONOI_RADIAL;
          chopModeRadial = ChopModeRadial( cr, centerWorld );
          break;

      }

      if ( !chopModeUniform && !chopModeGeometry && !chopModeRadial )
      {
        return;
      }

      CaronteSharp.MeshComplex[] arrMeshPieceCaronte;

      uint[]     arrMeshPieceParentIdx;
      Vector3[]  arrMeshPiecePosition;
      ArrayIndex arrInsideOutsideIdx;

      ChopRestrictionRequest wr = GetRestrictRequest(centerWorld);

      EditorUtility.DisplayProgressBar( Data.Name, "Chopping...", 1.0f);

      ChopNoiseRequest cnr = GetNoiseRequest();
      CaronteSharp.Tools.FractureMeshes( cr, wr, cnr, centerWorld, out arrMeshPieceCaronte, out arrMeshPieceParentIdx, out arrMeshPiecePosition, out arrInsideOutsideIdx );
      
      bool thereIsOutput = arrMeshPieceCaronte.Length > 0;
      if (thereIsOutput)
      {
        List<GameObject> listChoppedParentGO;
        CreateListChoppedParentGO( listParentGO, out listChoppedParentGO );

        EditorUtility.DisplayProgressBar( Data.Name, "Creating pieces...", 1.0f);

        List< List<int> >        listListMaterialIdx;
        List< UnityEngine.Mesh > listMeshPieceUnity;
        List< int >              listInputMeshIdx;
        
        CarGeometryUtils.CreateMeshesFromCaronte( arrMeshPieceCaronte, out listMeshPieceUnity, out listInputMeshIdx, out listListMaterialIdx );

        Transform oldParent = DestroyOldObjects();

        Undo.RecordObject(Data, "Chop - " + Data.Name);
        CreateNewObjects( centerWorld, listParentGO, listChoppedParentGO, listMaterial, listMeshPieceUnity,
                          listListMaterialIdx, listInputMeshIdx, arrMeshPieceParentIdx, arrMeshPiecePosition, arrInsideOutsideIdx.arrIdx_ );

        if (oldParent != null)
        {
          Data.GameObjectChoppedRoot.transform.parent = oldParent;
        }

        SeparatePieces();
        ApplyUVsPostProcess();
        CalculateStatistics();

        Undo.SetCurrentGroupName("Chop - " + Data.Name);
        Undo.CollapseUndoOperations( Undo.GetCurrentGroup() );
        EditorUtility.SetDirty(Data);
      }
      EditorUtility.ClearProgressBar();
    }

    private void CalculateStatistics()
    {
      GameObject[] goToChop = FieldController.GetUnityGameObjects();

      int nInputObjects = 0;
      int inputVertices = 0;
      int inputIndices = 0;

      int nOutputPieces = 0;
      int outputVertices = 0;
      int outputIndices = 0;

      for (int i = 0; i < goToChop.Length; i++)
      {
        GameObject go = goToChop[i];
        Mesh mesh = go.GetMesh();

        if ( mesh != null )
        {
          nInputObjects++;
          inputVertices += mesh.vertexCount;
          inputIndices+= mesh.triangles.Length;
        }
      }

      Data.InputObjects    = nInputObjects;
      Data.InputVertices   = inputVertices;
      Data.InputTriangles  = inputIndices / 3;

      GameObject[] arrGOChopped = Data.ArrChoppedGameObject;
      for (int i = 0; i < arrGOChopped.Length; i++)
      {
        GameObject go = arrGOChopped[i];
        Mesh mesh = go.GetMesh();

        if ( mesh != null )
        {
          nOutputPieces++;
          outputVertices += mesh.vertexCount;
          outputIndices+= mesh.triangles.Length;
        }
      }

      Data.OutputPieces    = nOutputPieces;
      Data.OutputVertices  = outputVertices;
      Data.OutputTriangles = outputIndices / 3;
    }

    private void CreateListChoppedParentGO( List<GameObject> listParentGO, out List<GameObject> listChoppedParentGO )
    {
      listChoppedParentGO = new List<GameObject>();

      // Create ParentGO Pieces
      for ( int i = 0; i < listParentGO.Count; i++)
      {
        GameObject parentGO       = listParentGO[i];
        GameObject chopParentGO   = parentGO.CreateDummy(parentGO.name + "_chopped");
        listChoppedParentGO.Add(chopParentGO);
      }

    }

    private Transform DestroyOldObjects()
    {
      if ( Data.GameObjectChoppedRoot == null )
      {
        return null;
      }

      Transform oldParent = Data.GameObjectChoppedRoot.transform.parent;

      Undo.DestroyObjectImmediate(Data.GameObjectChoppedRoot);

      return oldParent;
    }

    private ChopRestrictionRequest GetRestrictRequest(Vector3 centerWorld)
    {
      ChopRestrictionRequest wr = new ChopRestrictionRequest();
      wr.doRestrictionZone_ = Data.RestrictFractureZone;

      if (Data.RestrictFractureZone)
      {
        MeshSimple restrictionMesh;
        GameObject[] arrGameObject = FieldControllerRestriction.GetUnityGameObjects();
        CarGeometryUtils.CreateCenteredAppendedMeshSimple(arrGameObject, centerWorld, out restrictionMesh);
        wr.meshCropGeometry_ = restrictionMesh;
      }
      else
      {
        wr.meshCropGeometry_ = null;
      }

      wr.cropMode_                     = (CROP_MODE)Data.CropMode;
      wr.weldAllRemainingsTogether_    = Data.WeldInOnePiece;
      wr.includeBoundary_              = Data.FrontierPieces;
      wr.isAbleToClassifyDisconnected_ = true;

      return wr;
    }

    private ChopNoiseRequest GetNoiseRequest()
    {
      ChopNoiseRequest cnr = new ChopNoiseRequest();

      cnr.doNoise_                  = Data.DoNoise;
      cnr.noiseResolutionRate_      = Data.NoiseResolutionRate;
      cnr.noiseReliefAmplitudeRate_ = Data.NoiseReliefAmplitudeRate;
      cnr.noiseReliefSerrationRate_ = Data.NoiseReliefSerrationRate;
      cnr.seed0_                    = Data.NoiseSeed0;
      cnr.doExtrusionEffect_        = Data.NoiseDoExtrusionEffect && Data.DoExtrusionEffect;

      return cnr;
    }

    public void ChangeInteriorMaterial(Material interiorMaterial)
    {
      if (Data.ArrChoppedGameObject != null && Data.ArrInteriorSubmeshIdx != null)
      {
        int nGameObjects = Data.ArrChoppedGameObject.Length;

        for (int i = 0; i < nGameObjects; i++)
        {
          GameObject go = Data.ArrChoppedGameObject[i];
          if ( go != null )
          {
            Renderer   renderer  = go.GetComponent<Renderer>();
            Material[] materials = renderer.sharedMaterials;

            int subMeshIdx = Data.ArrInteriorSubmeshIdx[i];
            if (subMeshIdx != -1)
            {
              materials[subMeshIdx] = interiorMaterial;  
              renderer.sharedMaterials = materials;
            }
          }
        }
      }
    }

    private void CreateOutsideInsideGOParent( List<GameObject> listParentGO, List<GameObject> listChoppedParentGO, bool hasBeenSplitted, uint[] arrOutsideInsideIdx,
                                              out List< Tuple2<GameObject, GameObject> > listOutsideInsideGOParent )
    {
      listOutsideInsideGOParent = new List< Tuple2<GameObject, GameObject> >();
      
      // Create InsideOutsidePieces
      for ( int i = 0; i < listParentGO.Count; i++)
      {
        GameObject parentGO     = listParentGO[i];
        GameObject chopParentGO = listChoppedParentGO[i];

        if ( hasBeenSplitted )
        {
          GameObject outsideGO = parentGO.CreateDummy(parentGO.name + "_outside");
          GameObject insideGO  = parentGO.CreateDummy(parentGO.name + "_inside");

          outsideGO.transform.parent = chopParentGO.transform;
          insideGO.transform.parent  = chopParentGO.transform;

          listOutsideInsideGOParent.Add( Tuple2.New( outsideGO, insideGO ) );
        }
        
        if ( Data.HideParentObjectAuto )
        {
          Undo.RecordObject( parentGO, "Change activate state - " + parentGO.name );
          parentGO.SetActive(false);
          EditorUtility.SetDirty( parentGO );
        }
      }
    }

    private void GeneratePieceMaterialList( GameObject parentGO, Mesh meshPiece, List<Material> listMaterial, List<int> listPieceMaterialIdx, List<Material> listPieceMaterial,
                                            out int idInteriorSubmesh, out int interiorTrianglesStartIdx )
    {
      listPieceMaterial.Clear();

      idInteriorSubmesh = -1;
      interiorTrianglesStartIdx = -1;
 
      for( int j = 0; j < listPieceMaterialIdx.Count; j++ ) 
      {
        int materialIdx = listPieceMaterialIdx[j];

        Material mat;
        if (materialIdx == -1)
        {
          if (Data.GenerateNewSubmeshForInteriorFaces)
          {
            mat = Data.InteriorMaterial;
            idInteriorSubmesh = j;
            listPieceMaterial.Add(mat);  
          }
          else
          {
            idInteriorSubmesh = 0;
            if ( j != 0)
            {
              CarGeometryUtils.AppendSubmeshFromTo(meshPiece, j, idInteriorSubmesh, out interiorTrianglesStartIdx);     
            }
            else
            {
              if (parentGO != null)
              {
                Renderer rn = parentGO.GetComponent<Renderer>();
                listPieceMaterial.Add(rn.sharedMaterial);
              }
              else
              {
                listPieceMaterial.Add(listMaterial[0]);
              }
            }
          }    
        }
        else if ( materialIdx == -2)
        {
          mat = null;
          listPieceMaterial.Add(mat);  
        }
        else
        {
          mat = listMaterial[materialIdx];  
          listPieceMaterial.Add(mat);    
        }               
      }
    }

    private void CreateNewObjects( Vector3 centerWorld, List<GameObject> listParentGO, List<GameObject> listChoppedParentGO, List<Material> listMaterial, List<UnityEngine.Mesh> listMeshPieceUnity,
                                   List< List< int > > listListMaterialIdx, List<int> listInputMeshIdx, uint[] arrMeshPieceParentIdx, Vector3[] arrMeshPiecePosition, uint[] arrOutsideInsideIdx )
    {  
      bool hasBeenSplitted = arrOutsideInsideIdx != null && arrOutsideInsideIdx.Length > 0;

      List< Tuple2<GameObject, GameObject> > listOutsideInsideGOParent;
      CreateOutsideInsideGOParent( listParentGO, listChoppedParentGO, hasBeenSplitted, arrOutsideInsideIdx,
                                   out listOutsideInsideGOParent );

      GameObject chopRoot = new GameObject( Data.Name + "_output" );
      chopRoot.transform.position = centerWorld;
      Undo.RegisterCreatedObjectUndo(chopRoot, "Created " + Data.Name + "_output");

      Data.GameObjectChoppedRoot = chopRoot;
      
      Dictionary<GameObject, int> dictChoppedGOInteriorSubmeshIdx = new Dictionary<GameObject,int>();
      Dictionary<GameObject, int> dictChoppedGOInteriorTriangleStartIdx = new Dictionary<GameObject, int>();
      List<Material> listPieceMaterial = new List<Material>();

      int nMeshPieces = listMeshPieceUnity.Count;
      for (int i = 0; i < nMeshPieces; i++)
      {
        UnityEngine.Mesh meshPiece      = listMeshPieceUnity[i];
        List<int> listPieceMaterialIdx  = listListMaterialIdx[i];

        int inputMeshIdx = listInputMeshIdx[i];

        Vector3 goPosition = arrMeshPiecePosition[inputMeshIdx];
        int parentIdx = (int)arrMeshPieceParentIdx[inputMeshIdx];

        GameObject parentGO;
        if (parentIdx == -1)
        {
          parentGO = null;
        }
        else
        {
          parentGO = listParentGO[parentIdx];
        }

        int idInteriorSubmesh;
        int interiorTrianglesStartIdx;
        GeneratePieceMaterialList( parentGO, meshPiece, listMaterial, listPieceMaterialIdx, listPieceMaterial, 
                                   out idInteriorSubmesh, out interiorTrianglesStartIdx );

        GameObject goPiece;
        // if does not have parent piece (unfractured zone)
        if (parentGO == null)
        {
          goPiece = CarGeometryUtils.CreatePiece( 0, "Unfracture_zone", listPieceMaterial, meshPiece, goPosition, chopRoot );    
        }
        else
        {
          GameObject chopParentGO = listChoppedParentGO[parentIdx];
          goPiece = CarGeometryUtils.CreatePiece( i, parentGO.name, listPieceMaterial, meshPiece, goPosition,  chopParentGO );
          
          if (hasBeenSplitted)
          {
            Tuple2<GameObject, GameObject> tupleOutsideInside = listOutsideInsideGOParent[parentIdx];

            GameObject outsideGO = tupleOutsideInside.First;
            GameObject insideGO  = tupleOutsideInside.Second;

            uint outsideInsideIdx = arrOutsideInsideIdx[inputMeshIdx];
            if( outsideInsideIdx == 0 )
            {
              goPiece.transform.parent = outsideGO.transform;
              goPiece.name = parentGO.name + "_out_" + i;
            }
            else if( outsideInsideIdx == 1 )
            {
              goPiece.transform.parent = insideGO.transform;
              goPiece.name = parentGO.name + "_in_" + i;
            }
          }
        }

        dictChoppedGOInteriorSubmeshIdx.Add(goPiece, idInteriorSubmesh);
        dictChoppedGOInteriorTriangleStartIdx.Add(goPiece, interiorTrianglesStartIdx);
      }
      
      foreach( Tuple2<GameObject, GameObject> tupleOutsideInside in listOutsideInsideGOParent )
      {
        GameObject outside = tupleOutsideInside.First;
        if (outside.transform.childCount == 0)
        {
          Object.DestroyImmediate(outside);
        }

        GameObject inside = tupleOutsideInside.Second;
        if (inside.transform.childCount == 0)
        {
          Object.DestroyImmediate(inside);
        }
      }

      for (int i = 0; i < listChoppedParentGO.Count; i++)
      {
        GameObject chopParentGO = listChoppedParentGO[i];

        chopParentGO.transform.parent = Data.GameObjectChoppedRoot.transform;
        chopParentGO.transform.SetAsFirstSibling();
      }

      Selection.activeGameObject = Data.GameObjectChoppedRoot ;
      SaveNewChopInfo(dictChoppedGOInteriorSubmeshIdx, dictChoppedGOInteriorTriangleStartIdx);
    }

    private bool ChopModeUniform( ChopRequest cr )
    {
      if ( Data.NDesiredPieces == 0 )
      {
        return false;
      }
      cr.nDesiredPieces_ = (uint)Data.NDesiredPieces;
      cr.doExtrusionEffect_ = Data.DoExtrusionEffect;
      cr.doCoordinate_      = Data.DoCoordinate;

      return true;
    }

    private bool ChopModeGeometry( ChopRequest cr, Vector3 centerWorld )
    {
      if ( Data.NDesiredPieces <= 0 )
      {
        return false;
      }

      MeshSimple steeringMesh;
      GameObject[] arrGameObject = FieldControllerSteering.GetUnityGameObjects();
      CarGeometryUtils.CreateCenteredAppendedMeshSimple(arrGameObject, centerWorld, out steeringMesh);

      if (steeringMesh == null)
      {
        return false;
      }

      cr.nDesiredPieces_    = (uint)Data.NDesiredPieces;  
      cr.meshFocusGeometry_ = steeringMesh;
      cr.focusMode_         = (PSBG_FOCUS_MODE)Data.FocusMode;
      cr.gridResolution_    = (uint)Data.GridResolution;
      cr.densityRate_       = Data.DensityRate;
      cr.transitionLength_  = Data.TransitionLength;
      cr.doExtrusionEffect_ = Data.DoExtrusionEffect;
      cr.doCoordinate_      = Data.DoCoordinate;

      return true;
    }

    private bool ChopModeRadial( ChopRequest cr, Vector3 centerWorld )
    {
      if (Data.ReferenceSystem == null)
      {
        Debug.Log("Radial mode requires specifying a reference system.");
        return false;      
      }

      Transform tr = Data.ReferenceSystem.transform;
      Matrix4x4 m_LOCAL_to_WORLD = tr.localToWorldMatrix;

      cr.focusSystem_center_ = Data.ReferenceSystem.position - centerWorld;
      

      switch (Data.ReferenceSystemAxis)
      {
        case CNFracture.AxisDir.x:
          cr.focusSystem_axisDir_ = m_LOCAL_to_WORLD.MultiplyVector(Data.ReferenceSystem.right);
          break;
        case CNFracture.AxisDir.y:
          cr.focusSystem_axisDir_ = m_LOCAL_to_WORLD.MultiplyVector(Data.ReferenceSystem.up);
          break;
        case CNFracture.AxisDir.z:
          cr.focusSystem_axisDir_ = m_LOCAL_to_WORLD.MultiplyVector(Data.ReferenceSystem.forward);
          break;
      }

      cr.rays_number_   = (uint)Data.RaysNumber;
      cr.rays_rateRand_ = Data.RaysRateRand;

      cr.rings_numberInsideAnnulus_ = (uint)Data.RingsNumberInsideAnnulus;

      cr.rings_extRadius_           = Data.RingsExtRadius;
      cr.rings_extTransitionLength_ = Data.RingsExtTransitionLength;
      cr.rings_extTransitionDecay_  = Data.RingsExtTransitionDecay;

      cr.rings_intRadius_           = Data.DoUseInternalRadius ? Data.RingsIntRadius : 0.0f;
      cr.rings_intTransitionLength_ = Data.DoUseInternalRadius ? Data.RingsIntTransitionLength : 0.0f;
      cr.rings_intTransitionDecay_  = Data.DoUseInternalRadius ? Data.RingsIntTransitionDecay : 0.0f;
 
      cr.rings_rateRand_ = Data.RingsRateRand;
      cr.doCentralPiece_ = Data.DoCentralPiece;

      cr.noiseRate_    = Data.NoiseRate;
      cr.twistRate_    = Data.TwistRate;
      cr.doCoordinate_ = Data.DoCoordinate;

      return true;   
    }

    private void SaveNewChopInfo( Dictionary<GameObject, int> dictChoppedGOInteriorSubmeshIdx,
                                  Dictionary<GameObject, int> dictChoppedGOInteriorTrianglesStartIdx )
    {     
      List<GameObject> listChoppedGameObject = new List<GameObject>();

      GameObject[] arrChoppedGO = CarEditorUtils.GetAllChildObjectsWithGeometry(Data.GameObjectChoppedRoot, true);
      listChoppedGameObject.AddRange(arrChoppedGO);
   
      int nTotalPieces = listChoppedGameObject.Count;

      Data.ArrChoppedGameObject = listChoppedGameObject.ToArray();

      Data.ArrInteriorSubmeshIdx           = new int[nTotalPieces];
      Data.ArrInteriorTrianglesRange       = new int[nTotalPieces];
      Data.ArrChoppedMesh                  = new Mesh[nTotalPieces];
      Data.ArrGameObject_Bounds_Chopped    = new Bounds[nTotalPieces];
      Data.ArrGameObject_Chopped_Positions = new Vector3[nTotalPieces];

      for (int i = 0; i < nTotalPieces; i++)
      {
        GameObject go = listChoppedGameObject[i];

        int interiorSubMeshIdx = dictChoppedGOInteriorSubmeshIdx[go];
        int interiorStartIdx   = dictChoppedGOInteriorTrianglesStartIdx[go];

        Data.ArrChoppedMesh[i]                  = go.GetMesh();
        Data.ArrInteriorSubmeshIdx[i]           = interiorSubMeshIdx;
        Data.ArrInteriorTrianglesRange[i]       = interiorStartIdx;
        Data.ArrGameObject_Bounds_Chopped[i]    = go.GetWorldBounds();
        Data.ArrGameObject_Chopped_Positions[i] = go.transform.localPosition;
      }

      EditorUtility.SetDirty( Data );
    }

    private void SeparatePieces()
    {
      int numPieces = Data.ArrGameObject_Bounds_Chopped.Length;
      Box[]  arrBox = new Box[numPieces];
      for (int i = 0; i < numPieces; i++)
      {
        Bounds bounds = Data.ArrGameObject_Bounds_Chopped[i];
        arrBox[i] = new Box(bounds.min, bounds.max);
      }
      Vector3[] arrDeltaPush;
      CaronteSharp.Tools.CalculateDeltasToPushAwayPieces(arrBox, Data.PushMultiplier, out arrDeltaPush);
      
      for (int i = 0; i < numPieces; i++)
      {
        GameObject go = Data.ArrChoppedGameObject[i];
        if (go != null)
        {
          Transform tr  = go.transform;
          tr.localPosition = Data.ArrGameObject_Chopped_Positions[i] + (tr.worldToLocalMatrix.MultiplyVector(arrDeltaPush[i]) * Data.PushRate);
        }
      }
    }

    public void ApplyUVsPostProcess()
    {
      if (Data.ArrChoppedGameObject != null && Data.ArrChoppedMesh == null)
      {
        int nGameObject = Data.ArrChoppedGameObject.Length;
        Data.ArrChoppedMesh = new Mesh[nGameObject];
        for (int i = 0; i < nGameObject; i++)
        {
          GameObject go = Data.ArrChoppedGameObject[i];
          Data.ArrChoppedMesh[i] = go.GetMesh();
        }
        EditorUtility.SetDirty(Data);
      }

      if (Data.ArrChoppedGameObject != null)
      {
        int nMesh = Data.ArrChoppedMesh.Length;
        
        for (int i = 0; i < nMesh; i++)
        {
          GameObject go = Data.ArrChoppedGameObject[i];
          Mesh originalMesh = Data.ArrChoppedMesh[i];
          if ( go != null && originalMesh != null )
          {
            Mesh newMesh = Object.Instantiate(originalMesh);
            newMesh.name = originalMesh.name;

            int submeshIdx      = Data.ArrInteriorSubmeshIdx[i];
            int submeshStartIdx = Data.ArrInteriorTrianglesRange[i];

            if (submeshIdx != -1)
            {
              bool wasModified = CarGeometryUtils.ModifyMeshUVs( Data.InteriorFacesTiling, Data.InteriorFacesOffset, newMesh, 
                                                                 submeshIdx,  submeshStartIdx );
              if (wasModified)
              {
                go.SetMesh(newMesh); 
              }         
            }
          }
        }
      }

    }

    private void DrawIsRestrictedFracture()
    {
      EditorGUILayout.BeginHorizontal();
      EditorGUI.BeginChangeCheck();
      var foldOutVal = EditorGUILayout.Foldout(Data.DoRestrictFractureZoneFoldout, frRectrictFractureZoneCt_);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + "internal radius foldout" + Data.Name);
        Data.DoRestrictFractureZoneFoldout = foldOutVal;
        EditorUtility.SetDirty(Data);
      }
      GUILayout.Space(145f);
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle("", Data.RestrictFractureZone );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frRectrictFractureZoneCt_.text + " - " + Data.Name);
        Data.RestrictFractureZone = value;
        EditorUtility.SetDirty(Data);
      }
      GUILayout.FlexibleSpace();

      EditorGUILayout.EndHorizontal();
    }

    private void DrawCropGeometry()
    {
      EditorGUI.BeginDisabledGroup(!Data.RestrictFractureZone);
      EditorGUI.BeginChangeCheck();
      var value = (GameObject) EditorGUILayout.ObjectField(frResctrictGeometryCt_, Data.CropGeometry, typeof(GameObject), true );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Restriction geometry - " + Data.Name);
        Data.CropGeometry = value;
        EditorUtility.SetDirty(Data);
      }
      EditorGUI.EndDisabledGroup();
    }

    private void DrawFieldRestrictionGeometries()
    {
      RenderFieldObjects( frResctrictGeometriesCt_, FieldControllerRestriction, Data.RestrictFractureZone, false, CNFieldWindow.Type.normal );
    }

    private void LoadGUICropMode()
    {
      if ( Data.CropMode == CNFracture.CROP_MODE.INSIDE )
      {
        if ( !Data.FrontierPieces )
        {
          cropModeIdx_ = 0;
        }
        else
        {
          cropModeIdx_ = 1;
        }
      }
      else if ( Data.CropMode == CNFracture.CROP_MODE.OUTSIDE )
      {
        if (!Data.FrontierPieces)
        {
          cropModeIdx_ = 2;
        }
        else
        {
          cropModeIdx_ = 3;
        }
      }
    }

    private void DrawCropMode()
    {
      EditorGUI.BeginDisabledGroup(!Data.RestrictFractureZone);
      LoadGUICropMode();
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Popup( frFractureZoneCt_, cropModeIdx_, arrRestrictionMode_ );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change crop mode - " + Data.Name);
        cropModeIdx_ = value;
        SetCropMode();
        EditorUtility.SetDirty( Data );
      }
      EditorGUI.EndDisabledGroup();
    }

    private void DrawUnfracturedZone()
    {
      EditorGUI.BeginDisabledGroup(!Data.RestrictFractureZone);
      EditorGUI.BeginChangeCheck();
      int unfractureZoneIdx = Data.WeldInOnePiece ? 0 : 1;
      var value = EditorGUILayout.Popup( frUnfracturedMeshCt_, unfractureZoneIdx, arrUnfracturedZoneNames_ );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change unfractured zone - " + Data.Name);
        Data.WeldInOnePiece = (value == 0);
        EditorUtility.SetDirty(Data);
      }
      EditorGUI.EndDisabledGroup();
    }

    private void DrawPushRate()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Slider(frPushRateCt_, Data.PushRate, 0.0f, 1.0f);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frPushRateCt_.text + " - " + Data.Name);
        Data.PushRate = value;
        EditorUtility.SetDirty(Data);
      } 
    }

    private void DrawPushMultiplier()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField(frPushMultiplierCt_, Data.PushMultiplier);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frPushMultiplierCt_.text + " - " + Data.Name);
        Data.PushMultiplier = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawOutputButton()
    {
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField("", GUILayout.Width(EditorGUIUtility.labelWidth));
      if ( GUILayout.Button(frSelectPiecesCt_, GUILayout.Height(22f)) )
      {
        Selection.activeGameObject = Data.GameObjectChoppedRoot;
      }
      EditorGUILayout.EndHorizontal();
    }

    private void DrawEnableOnlyOriginals()
    {
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField("", GUILayout.Width(EditorGUIUtility.labelWidth));
      if (GUILayout.Button(frActivateInputOnly_, GUILayout.Height(22f)))
      {
        GameObject[] arrGameObject = FieldController.GetUnityGameObjects();
        foreach( GameObject go in arrGameObject )
        {
          go.SetActive(true);
        }
        
        if (Data.GameObjectChoppedRoot != null)
        {
          Data.GameObjectChoppedRoot.SetActive(false);
        }
        Selection.objects = arrGameObject;

      }
      EditorGUILayout.EndHorizontal();
    }

    private void DrawEnableOnlyPieces()
    {
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField("", GUILayout.Width(EditorGUIUtility.labelWidth));
      if (GUILayout.Button(frActivateOutputOnly_, GUILayout.Height(22f)))
      {
        GameObject[] arrGameObject = FieldController.GetUnityGameObjects();
        foreach (GameObject go in arrGameObject)
        {
          go.SetActive(false);
        }

        if (Data.GameObjectChoppedRoot != null)
        {
          Data.GameObjectChoppedRoot.SetActive(true);
          Selection.activeGameObject = Data.GameObjectChoppedRoot;
        }
        
      }
      EditorGUILayout.EndHorizontal();
    }

    private void DrawIsNoiseFracture()
    {
      EditorGUILayout.BeginHorizontal();
      EditorGUI.BeginChangeCheck();
      var foldOutVal = EditorGUILayout.Foldout(Data.DoNoiseFoldout, frInteriorFacesNoiseCt_);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + "internal radius foldout" + Data.Name);
        Data.DoNoiseFoldout = foldOutVal;
        EditorUtility.SetDirty(Data);
      }
      GUILayout.Space(145f);
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle("", Data.DoNoise);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frInteriorFacesNoiseCt_.text + " - " + Data.Name);
        Data.DoNoise = value;
        EditorUtility.SetDirty(Data);
      }
      GUILayout.FlexibleSpace();
      EditorGUILayout.EndHorizontal();
    }

    private void DrawNoiseResolutionRate()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Slider(frNoiseResolutionRateCt_, Data.NoiseResolutionRate, 0f, 1f);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frNoiseResolutionRateCt_.text + " - " + Data.Name);
        Data.NoiseResolutionRate = Mathf.Clamp01(value);
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawNoiseAmplitudeRate()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Slider(frNoiseReliefAmplitudeRateCt_, Data.NoiseReliefAmplitudeRate, 0f, 1f);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frNoiseReliefAmplitudeRateCt_.text + " - " + Data.Name);
        Data.NoiseReliefAmplitudeRate = Mathf.Clamp01(value);
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawNoiseSerrationRate()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Slider(frNoiseReliefSerrationRateCt_, Data.NoiseReliefSerrationRate, 0f, 1f);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frNoiseReliefSerrationRateCt_.text + " - " + Data.Name);
        Data.NoiseReliefSerrationRate = Mathf.Clamp01(value);
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawNoiseSeed()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.IntField(frNoiseSeedCt_, (int)Data.NoiseSeed0);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frNoiseSeedCt_.text + Data.Name);
        Data.NoiseSeed0 = (uint)Mathf.Clamp(value, 0f, int.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawNoiseExtrussionEffect()
    {
      EditorGUI.BeginChangeCheck();
      EditorGUI.BeginDisabledGroup(!Data.DoExtrusionEffect);
      var value = EditorGUILayout.Toggle(frDoNoiseExtrusionEffectCt_, Data.NoiseDoExtrusionEffect);
      EditorGUI.EndDisabledGroup();
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frDoNoiseExtrusionEffectCt_.text + Data.Name);
        Data.NoiseDoExtrusionEffect = value;
        EditorUtility.SetDirty(Data);
      }
    }
  
    private void DrawInteriorFacesSmoothingAngle()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Slider(frInteriorFacesSmoothingAngleCt_, Data.InternalFacesSmoothingAngle, 0f, 180f);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frInteriorFacesSmoothingAngleCt_.text + Data.Name);
        Data.InternalFacesSmoothingAngle = Mathf.Clamp(value, 0.0f, 180.0f);
      }
    }

    private void DrawGenerateInteriorFacesInNewSubmesh()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle(frGenerateNewSubmeshForInteriorFacesCt_, Data.GenerateNewSubmeshForInteriorFaces);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frGenerateNewSubmeshForInteriorFacesCt_.text + Data.Name);
        Data.GenerateNewSubmeshForInteriorFaces = value;
      }
    }


    private void DrawModifyInteriorFaces()
    {
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField("", GUILayout.Width(EditorGUIUtility.labelWidth));
      if ( GUILayout.Button( frModifyInteriorFacesUVsCt_, GUILayout.Height(22f) ) )
      {
        CarModifyInteriorFacesWindow.ShowWindow(Data, this);
      }
      EditorGUILayout.EndHorizontal();
    }


    private void DrawStatisticsTab()
    {
      EditorGUILayout.LabelField("Last chop input geometry objects " + Data.InputObjects );
      EditorGUILayout.LabelField("Last chop input vertices: "  + Data.InputVertices );
      EditorGUILayout.LabelField("Last chop input triangles: " + Data.InputTriangles );

      EditorGUILayout.Space();

      EditorGUILayout.LabelField("Last chop output pieces: "    + Data.OutputPieces );
      EditorGUILayout.LabelField("Last chop output vertices: "  + Data.OutputVertices );
      EditorGUILayout.LabelField("Last chop output triangles: " + Data.OutputTriangles );
    }



    public override void RenderGUI(Rect area, bool isEditable)
    {
      GUILayout.BeginArea(area);
      
      RenderTitle(isEditable, false, false);

      EditorGUI.BeginDisabledGroup(!isEditable);
      RenderFieldObjects( "Objects", FieldController, true, true, CNFieldWindow.Type.normal );

      EditorGUILayout.Space();
      EditorGUILayout.Space();

      EditorGUILayout.BeginHorizontal();
      if ( GUILayout.Button("Chop", GUILayout.Height(30f)) )
      {
        Chop();
      }

      if ( GUILayout.Button("Save assets..", GUILayout.Height(30f), GUILayout.Width(100f) ) )
      {
        SaveChopResult();
      }

      EditorGUILayout.EndHorizontal();

      EditorGUI.EndDisabledGroup();
      CarGUIUtils.DrawSeparator();
      CarGUIUtils.Splitter();

      GUIStyle styleTabButton = new GUIStyle(EditorStyles.toolbarButton);
      styleTabButton.fontSize    = 10;
      styleTabButton.fixedHeight = 18f;
      styleTabButton.onNormal.background  = styleTabButton.onActive.background;

      tabIndex_ = GUILayout.SelectionGrid(tabIndex_, arrTabName_, 2, styleTabButton);
      CarGUIUtils.Splitter();

      scroller_ = EditorGUILayout.BeginScrollView(scroller_);

      if (tabIndex_ == 1)
      {
        DrawStatisticsTab();
      }
      else
      {
        EditorGUI.BeginDisabledGroup(!isEditable);

        float originalLabelwidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 200f;

        EditorGUILayout.Space();

        switch (Data.ChopMode)
        {
          case CNFracture.CHOP_MODE.VORONOI_UNIFORM:
            DrawGUICommon();
            break;
          case CNFracture.CHOP_MODE.VORONOI_BY_GEOMETRY: 
            DrawGUIGeometry();
            break;
          case CNFracture.CHOP_MODE.VORONOI_RADIAL:
            DrawGUIRadial();
            break;
        }

        EditorGUILayout.Space();
        CarGUIUtils.Splitter();
        EditorGUILayout.Space();

        DrawIsRestrictedFracture();
        if (Data.DoRestrictFractureZoneFoldout)
        {
         EditorGUI.BeginDisabledGroup(!Data.RestrictFractureZone);
         EditorGUILayout.Space();
         DrawFieldRestrictionGeometries();
         DrawCropMode();
         EditorGUILayout.Space();
         DrawUnfracturedZone();
         EditorGUI.EndDisabledGroup();
        }

        CarGUIUtils.Splitter();

        EditorGUILayout.Space();
        DrawIsNoiseFracture();
        if (Data.DoNoiseFoldout)
        {
          EditorGUI.BeginDisabledGroup(!Data.DoNoise);
          EditorGUILayout.Space();
          DrawNoiseResolutionRate();
          DrawNoiseAmplitudeRate();
          DrawNoiseSerrationRate();
          DrawNoiseSeed();
          DrawNoiseExtrussionEffect();
          EditorGUI.EndDisabledGroup();
        }

        CarGUIUtils.Splitter();

        GUIStyle centerLabel = new GUIStyle(EditorStyles.largeLabel);
        centerLabel.alignment = TextAnchor.MiddleCenter;
        centerLabel.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField(frOutputSectionCt_, centerLabel);

        EditorGUI.BeginDisabledGroup(Data.GameObjectChoppedRoot == null);
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.Space();

        DrawPushRate();
        DrawPushMultiplier();
        EditorGUILayout.Space();
        DrawOutputButton();
        DrawEnableOnlyOriginals();
        DrawEnableOnlyPieces();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        ValidateState();

        EditorGUI.EndDisabledGroup();

        EditorGUIUtility.labelWidth = originalLabelwidth;

        EditorGUI.EndDisabledGroup();
      }

      EditorGUILayout.EndScrollView();
      GUILayout.EndArea();  
    }

    void SetCropMode()
    {
      if ( cropModeIdx_ == 0 )
      {
        Data.CropMode       = CNFracture.CROP_MODE.INSIDE;
        Data.FrontierPieces = false;
      }
      else if (cropModeIdx_ == 1)
      {
        Data.CropMode       = CNFracture.CROP_MODE.INSIDE;
        Data.FrontierPieces = true;
      }
      else if ( cropModeIdx_ == 2 )
      {
        Data.CropMode       = CNFracture.CROP_MODE.OUTSIDE;
        Data.FrontierPieces = false;
      }
      else if ( cropModeIdx_ == 3 )
      {
        Data.CropMode       = CNFracture.CROP_MODE.OUTSIDE;
        Data.FrontierPieces = true;
      }
    }

    private bool HasUnsavedChopReferences()
    {
      if (Data.GameObjectChoppedRoot == null)
      {
        return false;
      }

      if ( CarEditorUtils.IsAnyUnsavedMeshInHierarchy(Data.GameObjectChoppedRoot) )
      {
        return true;
      }
      else
      {
        return false;
      }    
    }

    private void SaveChopResult()
    {
      if (!HasUnsavedChopReferences())
      {
        EditorUtility.DisplayDialog("CaronteFX - Info", "There is not any mesh to save in assets.", "ok" );
        return;
      }

      CarEditorUtils.SaveAnyUnsavedMeshInHierarchy(Data.GameObjectChoppedRoot, false);
    }

    private void DrawGlobalPattern()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle(frGlobalPatternCt_, Data.DoGlobalPattern);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frGlobalPatternCt_.text + " - " + Data.Name);
        Data.DoGlobalPattern = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawNDesiredPieces()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.IntField(frNDesiredPiecesCt_, Data.NDesiredPieces);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frNDesiredPiecesCt_.text + " - " + Data.Name);
        Data.NDesiredPieces = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawSeed()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.IntField(frSeedCt_, Data.Seed);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frSeedCt_.text + " - " + Data.Name);
        Data.Seed = Mathf.Clamp(value, 0, int.MaxValue);
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawInteriorMaterial()
    {
      EditorGUI.BeginChangeCheck();
      var value = (Material) EditorGUILayout.ObjectField(frInteriorMaterialCt_, Data.InteriorMaterial, typeof(Material), true );
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(Data, "Change " + frInteriorMaterialCt_.text + " - " + Data.Name);
        ChangeInteriorMaterial( value );
        Data.InteriorMaterial = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawDoExtrussionEffect()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle(frDoExtrusionEffectCt_, Data.DoExtrusionEffect);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frDoExtrusionEffectCt_.text + " - " + Data.Name);
        Data.DoExtrusionEffect = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawDoCoordinate()
    {
      EditorGUI.BeginDisabledGroup( Data.RestrictFractureZone );
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle(frDoCoordinateCt_, Data.DoCoordinate);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frDoCoordinateCt_.text + " - " + Data.Name);
        Data.DoCoordinate = value;
        EditorUtility.SetDirty(Data);
      }
      EditorGUI.EndDisabledGroup();
    }

    private void DrawHideParentObjectAuto()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle( frHideParentObjectsAutoCt_, Data.HideParentObjectAuto );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frHideParentObjectsAutoCt_.text + " - " + Data.Name);
        Data.HideParentObjectAuto = value;
        EditorUtility.SetDirty(Data);
      }
    }

    void DrawGUICommon()
    {
      DrawGlobalPattern(); 
      DrawNDesiredPieces();
      DrawSeed();
      EditorGUILayout.Space();
      DrawGenerateInteriorFacesInNewSubmesh();
      EditorGUI.BeginDisabledGroup(!Data.GenerateNewSubmeshForInteriorFaces);
      DrawInteriorMaterial();
      EditorGUI.EndDisabledGroup();
      DrawInteriorFacesSmoothingAngle();
      DrawModifyInteriorFaces();
      EditorGUILayout.Space();
      EditorGUILayout.Space();
      EditorGUILayout.Space();
      DrawDoExtrussionEffect();
      DrawDoCoordinate();
      EditorGUILayout.Space();
      DrawHideParentObjectAuto();
    }

    private void DrawChopGeometry()
    {
      EditorGUI.BeginChangeCheck();
      var value = (GameObject) EditorGUILayout.ObjectField(frSteeringGeometryCt_, Data.ChopGeometry, typeof(GameObject), true );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frSteeringGeometryCt_.text + " - " + Data.Name);
        Data.ChopGeometry = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawSteeringGeometries()
    {
      RenderFieldObjects( frSteeringGeometriesCt_, FieldControllerSteering, true, false, CNFieldWindow.Type.normal );
    }

    private void DrawGridResolution()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.IntField(frGridResolutionCt_, Data.GridResolution );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frGridResolutionCt_.text + " - " + Data.Name);
        Data.GridResolution = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawFocusMode()
    {
      EditorGUI.BeginChangeCheck();
      var value = (CNFracture.FOCUS_MODE) EditorGUILayout.EnumPopup(frFocusModeCt_, Data.FocusMode );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frFocusModeCt_.text + " - " + Data.Name);
        Data.FocusMode = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawDensityRate()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField(frDensityRateCt_, Data.DensityRate );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frDensityRateCt_.text + " - " + Data.Name);
        Data.DensityRate = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawTransitionLength()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField(frTransitionLengthCt_, Data.TransitionLength);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frTransitionLengthCt_.text + " - " + Data.Name);
        Data.TransitionLength = value;
        EditorUtility.SetDirty(Data);
      }
    }

    void DrawGUIGeometry()
    {
      DrawGUICommon();
      EditorGUILayout.Space();
      CarGUIUtils.Splitter();
      EditorGUILayout.Space();
      EditorGUILayout.Space();
      DrawSteeringGeometries();
      DrawGridResolution();
      DrawFocusMode();
      DrawDensityRate();
      DrawTransitionLength();
    }

    private void DrawReferenceSystem()
    {
      EditorGUI.BeginChangeCheck();
      var value = (Transform) EditorGUILayout.ObjectField(frReferenceSystemCt_, Data.ReferenceSystem, typeof(Transform), true );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frReferenceSystemCt_.text + " - " + Data.Name);
        Data.ReferenceSystem = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawReferenceSystemAxis()
    {
      EditorGUI.BeginChangeCheck();
      var value = (CNFracture.AxisDir) EditorGUILayout.EnumPopup(frReferenceSystemAxisCt_, Data.ReferenceSystemAxis);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frReferenceSystemAxisCt_.text + " - " + Data.Name);
        Data.ReferenceSystemAxis = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawRaysNumber()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.IntField(frRaysNumberCt_, Data.RaysNumber);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frRaysNumberCt_.text + " - " + Data.Name);
        Data.RaysNumber = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawRaysRateRand()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Slider(frRaysRateRandCt_, Data.RaysRateRand, 0.0f, 1.0f);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frRaysRateRandCt_.text + " - " + Data.Name);
        Data.RaysRateRand = value;
        EditorUtility.SetDirty(Data);
      }
    }


    private void DrawDoIntRadius()
    {
      EditorGUILayout.BeginHorizontal();
      EditorGUI.BeginChangeCheck();
      var foldOutVal = EditorGUILayout.Foldout(Data.DoUseInternalRadiusFoldout, frUseInternalRadiusCt_);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + Data.Name);
        Data.DoUseInternalRadiusFoldout = foldOutVal;
        EditorUtility.SetDirty(Data);
      }
      GUILayout.Space(145f);
      EditorGUI.BeginChangeCheck();
      var val = EditorGUILayout.Toggle("", Data.DoUseInternalRadius);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frUseInternalRadiusCt_.text + " " + Data.Name);
        Data.DoUseInternalRadius = val;
        EditorUtility.SetDirty(Data);
      }
      GUILayout.FlexibleSpace();
      EditorGUILayout.EndHorizontal();
    }

    private void DrawIntRadius()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField(frRingsIntRadiusCt_, Data.RingsIntRadius);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frRingsIntRadiusCt_.text + " - " + Data.Name);
        Data.RingsIntRadius = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawRingsIntTransitionLength()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField(frRingsIntTransitionLengthCt_, Data.RingsIntTransitionLength);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frRingsIntTransitionLengthCt_.text + " - " + Data.Name);
        Data.RingsIntTransitionLength = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawRingsIntTransitionDecay()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Slider(frRingsIntTransitionDecayCt_, Data.RingsIntTransitionDecay, 0.0f, 1.0f);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frRingsIntTransitionDecayCt_.text  + " - "+ Data.Name);
        Data.RingsIntTransitionDecay = value;
        EditorUtility.SetDirty(Data);
      }
    }


    private void DrawRingsExtRadius()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField(frRingsExtRadiusCt_, Data.RingsExtRadius);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frRingsExtRadiusCt_.text + " - " + Data.Name);
        Data.RingsExtRadius = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawRingsExtTransitionLength()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField(frRingsExtTransitionLengthCt_, Data.RingsExtTransitionLength);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frRingsExtTransitionLengthCt_.text + " - " + Data.Name);
        Data.RingsExtTransitionLength = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawRingsExtTransitionDecay()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Slider(frRingsExtTransitionDecayCt_, Data.RingsExtTransitionDecay, 0.0f, 1.0f);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frRingsExtTransitionDecayCt_.text + " - " + Data.Name);
        Data.RingsExtTransitionDecay = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawRingsNumberInsideAnnulus()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.IntField(frRingsNumberInsideAnnulusCt_, Data.RingsNumberInsideAnnulus);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frRingsNumberInsideAnnulusCt_.text + " - " + Data.Name);
        Data.RingsNumberInsideAnnulus = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawRingsRateRand()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Slider(frRingsRateRandCt_, Data.RingsRateRand, 0.0f, 1.0f);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frRingsRateRandCt_.text + " - " + Data.Name);
        Data.RingsRateRand = value;
        EditorUtility.SetDirty(Data);
      }
    }


    private void DrawDoCentralPiece()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle(frCreateCentralPieceCt_, Data.DoCentralPiece);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frCreateCentralPieceCt_.text + " - " + Data.Name);
        Data.DoCentralPiece = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawNoiseRate()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Slider(frNoiseRateCt_, Data.NoiseRate, 0.0f, 1.0f);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frNoiseRateCt_.text + " - " + Data.Name);
        Data.NoiseRate = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawTwistRate()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Slider(frTwistRateCt_, Data.TwistRate, -1.0f, 1.0f);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change " + frTwistRateCt_.text + " - " + Data.Name);
        Data.TwistRate = value;
        EditorUtility.SetDirty(Data);
      }
    }

    void DrawGUIRadial()
    {
      DrawGlobalPattern();
      DrawSeed();
      DrawInteriorMaterial();
      DrawInteriorFacesSmoothingAngle();
      DrawModifyInteriorFaces();
      EditorGUILayout.Space();
      DrawDoExtrussionEffect();
      DrawDoCoordinate();
      EditorGUILayout.Space();
      DrawHideParentObjectAuto();

      CarGUIUtils.Splitter();
      EditorGUILayout.Space();
      DrawReferenceSystem();
      DrawReferenceSystemAxis();
      
      CarGUIUtils.Splitter();
      EditorGUILayout.Space();
      DrawRaysNumber(); 
      DrawRaysRateRand();
      EditorGUILayout.Space();
      DrawRingsNumberInsideAnnulus();
      DrawRingsRateRand();

      CarGUIUtils.Splitter();
      EditorGUILayout.Space();
      DrawRingsExtRadius();
      DrawRingsExtTransitionLength();
      DrawRingsExtTransitionDecay();
      CarGUIUtils.Splitter();
      EditorGUILayout.Space();
      DrawDoIntRadius();   
      if (Data.DoUseInternalRadiusFoldout)
      {
        EditorGUILayout.Space();
        EditorGUI.BeginDisabledGroup(!Data.DoUseInternalRadius);
        DrawIntRadius();
        DrawRingsIntTransitionLength();
        DrawRingsIntTransitionDecay();
        EditorGUI.EndDisabledGroup();
      }

      CarGUIUtils.Splitter();
      EditorGUILayout.Space();
      DrawDoCentralPiece();
      DrawNoiseRate();
      DrawTwistRate();
    }




  } //class CNFractureView
} // namespace Caronte

