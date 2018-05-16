// ***********************************************************
//	Copyright 2016 Next Limit Technologies, http://www.nextlimit.com
//	All rights reserved.
//
//	THIS SOFTWARE IS PROVIDED 'AS IS' AND WITHOUT ANY EXPRESS OR
//	IMPLIED WARRANTIES, INCLUDING, WITHOUT LIMITATION, THE IMPLIED
//	WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.
//
// ***********************************************************

using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEditor;
using CaronteSharp;

using Object = UnityEngine.Object;


namespace CaronteFX
{
  /// <summary>
  /// Main class for the plugin management.
  /// </summary>
  public class CarManager
  {
    /**********************************************************************************************/
    /* Public types and data                                                                      */
    /**********************************************************************************************/
    #region Public types and data

    public Caronte_Fx FxData
    {
      get { return fxData_; }
      set
      {
        fxData_ = value;
        if (fxData_ != null)
        {
          if (fxData_.CheckEffect())
          {
            EditorUtility.SetDirty(fxData_);
          }
          Init();
        }
      }
    }

    public int  CurrentFxIdx
    {
      get { return currentFxIdx_; }
      set { currentFxIdx_ = value; }
    }

    public CREffectData EffectData
    {
      get { return FxData.effect; }
    }

    public CNGroup RootNode
    {
      get { return EffectData.rootNode_; }
    }

    public CNGroup SubeffectsNode
    {
      get { return EffectData.subeffectsNode_;}
    }
    
    public bool BlockEdition
    {
      get { return Hierarchy.BlockEdition; }
    }

    public bool IsInited
    {
      get { return isInited_; }
    }

    public static bool IsInitedStatic
    {
      get { return (instance_ != null && instance_.isInited_);}
    }

    public CarEntityManager EntityManager
    {
      get { return entityManager_; }
    }

    public CarGOManager GOManager
    {
      get { return goManager_; }
    }

    public CNHierarchy Hierarchy
    {
      get { return hierarchy_; }     
    }

    public CarAnimationBaker SimulationBaker
    {
      get { return simulationBaker_; }
    }

    public  CarPlayer Player
    {
      get { return simulationPlayer_; }
    }

    #endregion // Public types and data

    /**********************************************************************************************/
    /* Private fields                                                                             */
    /**********************************************************************************************/

    #region Private fields

    private CarManagerEditor        managerEditor_;
    private CarListBox              nodesListBox_;
    private CarEntityManager        entityManager_; 
    private CarGOManager            goManager_;
    private CNHierarchy             hierarchy_;
    private CarSimulationDisplayer  simulationDisplayer_;
    private CarPlayer               simulationPlayer_;
    private CarAnimationBaker       simulationBaker_;
    
    private Caronte_Fx       fxData_;
    private List<Caronte_Fx> listFxData_;
    private int              currentFxIdx_;              

    private bool isInited_;

    private int selectedButton_ ;
    private string[] sceneGUIButtonNames_ = new string[5] { "Bodies", "Joints", "Explosions", "Events", "Visualization" };

    private double lastUpdateTimeDisplay_;
    private double lastUpdateTimeCheckForChanges_;

    private const float timeForUpdateDisplay_ = 1.5f;
    private const float timeForCheckChanges_  = 0.5f;

    private bool hierarchyChangeRequested_;
    private bool undoRedoChecksRequested_;

    #endregion // Private fields

    /**********************************************************************************************/
    /* Static init                                                                                */
    /**********************************************************************************************/

    #region Static initialization
    private static CarManager instance_ = null;
    public  static CarManager Instance
    {
      get
      {
        if (instance_ == null)
        {
          CarDebug.Log("x64 Editor");
     
          bool dllUpToDate = Caronte.IsDllUpToDate();   

          if (dllUpToDate)
          {
            CarDebug.Log("Dll version up to date.");
            instance_ = new CarManager();
          }
          else
          {
            CarDebug.Log("Dll version not up to date.");
          }       
        }
        return instance_;
      }
    }
    #endregion
    //-----------------------------------------------------------------------------------
    private CarManager()
    {
      managerEditor_    = CarManagerEditor.Instance;
      entityManager_    = new CarEntityManager(this);
      simulationPlayer_ = new CarPlayer(this, entityManager_);
      goManager_        = new CarGOManager();

      simulationDisplayer_ = new CarSimulationDisplayer(entityManager_, simulationPlayer_);
      simulationBaker_     = new CarAnimationBaker(this, entityManager_, simulationPlayer_);

      hierarchy_ = new CNHierarchy(this, managerEditor_, entityManager_, goManager_);

      entityManager_.SetHierarchy(hierarchy_);

      listFxData_ = new List<Caronte_Fx>();

      selectedButton_           = 0;
      hierarchyChangeRequested_ = false;
      undoRedoChecksRequested_  = false;
      isInited_                 = false;

      AppDomain.CurrentDomain.DomainUnload -= OnDomainUnload;
      AppDomain.CurrentDomain.DomainUnload += OnDomainUnload;
    }
    //-----------------------------------------------------------------------------------
    public void Init()
    {
      if (isInited_)
      {
        return;
      }

      StartCaronte();
      ResetIfSimulatingState();
      SetEditingStateBegin();

      CollisionEditingBlockedBegin();

      InitFxData();

      EditorUtility.SetDirty(fxData_);

      managerEditor_ = CarManagerEditor.Instance;

      hierarchy_.SetManagerEditor(managerEditor_);
      hierarchy_.Init();

      nodesListBox_ = new CarListBox(Hierarchy, "managerLB", true);
      managerEditor_.NodesListBox = nodesListBox_;

      simulationDisplayer_.Init(FxData);

      CaronteFXInitCreateAll();
      EditorApplication.delayCall += CaronteFXStartDelayedTask;

      hierarchyChangeRequested_ = true;

      lastUpdateTimeDisplay_         = 0f;
      lastUpdateTimeCheckForChanges_ = 0f;

      ShowInvisibleBodies(FxData.ShowInvisibles);

      isInited_ = true;
    }
    //-----------------------------------------------------------------------------------
    public void Deinit()
    { 
      if ( !isInited_ )
      {
        return;
      }

      CloseWindows();
      CaronteFXFinishTask();

      hierarchy_.Deinit();
      simulationPlayer_.Deinit();

      isInited_ = false;

      entityManager_.Clear();
      goManager_    .Clear();

      if (FxData != null)
      {
        FxData.ClearBodyMeshes();
        FxData.ClearJointMeshes();
        FxData.ClearSphereMeshes();
      }

      FinishCaronte();
      SceneView.RepaintAll();
    }
    //-----------------------------------------------------------------------------------
    private void StartCaronte()
    {
      Caronte.StarterEditor( Mathf.Max(1, SystemInfo.processorCount - 1) );
    }
    //-----------------------------------------------------------------------------------
    private void FinishCaronte()
    {
      Caronte.FinisherEditor();
    }
    //-----------------------------------------------------------------------------------
    private void CaronteFXInitCreateAll()
    {
      CreateBodies();
      CreateJoints();
      CreateServos();
      CreateEntities();
      ValidateEditorsState();
    }
    //-----------------------------------------------------------------------------------
    private void CaronteFXStartDelayedTask()
    {
      EditorApplication.update += CaronteEditorUpdate;
      EditorApplication.update += ShowCaronteLog;
      
      SceneView.onSceneGUIDelegate             += OnSceneGUI;
      EditorApplication.playmodeStateChanged   += ChangeState;
      EditorApplication.hierarchyWindowChanged += CustomHierarchyChange; 
      Undo.undoRedoPerformed                   += OnUndoRedo;
    }
    //-----------------------------------------------------------------------------------
    private void CaronteFXFinishTask()
    {
      EditorApplication.update -= CaronteEditorUpdate;
      EditorApplication.update -= ShowCaronteLog;
      
      SceneView.onSceneGUIDelegate             -= OnSceneGUI;
      EditorApplication.playmodeStateChanged   -= ChangeState;
      EditorApplication.hierarchyWindowChanged -= CustomHierarchyChange; 
      Undo.undoRedoPerformed                   -= OnUndoRedo;
    }
    //-----------------------------------------------------------------------------------
    private void InitFxData()
    {
      FxData.gameObject.SetActive( true );
      FxData.e_purgeIsolatedNodes();
      FxData.UpdateRootNodeName();
      FxData.OpenRootNodeIfNotOpened();
      FxData.SetStateEditing();

      EditorUtility.SetDirty(FxData);
    }
    //-----------------------------------------------------------------------------------
    private void SetEditingStateBegin()
    {
      if ( !SimulationManager.IsEditing() )
      {
        SimulationManager.EditingBegin();
      }

      SimulationManager.PauseOnWaitDone();
      SimulationManager.ClearBroadcast(UN_BROADCAST_MODE.EDITING);
    }
    //-----------------------------------------------------------------------------------
    private void SetEditingStateEnd()
    {
      SimulationManager.ResetTimeSlider();
      SimulationManager.EditingEnd();
    }
    //-----------------------------------------------------------------------------------
    private void ResetIfSimulatingState()
    {
      if ( SimulationManager.IsSimulating() || SimulationManager.IsReplaying() )
      {
        SimulationManager.ResetSimulation();
      }
    }
    //-----------------------------------------------------------------------------------
    private void CollisionEditingBlockedBegin()
    {
      SimulationManager.CollisionEditingBlockedBegin();
    }
    //-----------------------------------------------------------------------------------
    private void CollisionEditingBlockedEnd()
    {
      SimulationManager.CollisionEditingBlockedEnd();
    }
    //-----------------------------------------------------------------------------------
    public void OnUndoRedo()
    {
      if ( IsInited )
      {
        undoRedoChecksRequested_ = true;
      }
    }
    //-----------------------------------------------------------------------------------
    private void CloseWindows()
    {
      CarPlayerWindow      .SetForcedExit();
      CarPlayerWindow      .CloseIfOpen();
      CNFieldWindow        .CloseIfOpen();
      CarBakeSimulationMenu.CloseIfOpen();
      CarBakeFrameMenu     .CloseIfOpen();
      CarAddFXMenu         .CloseIfOpen();

      CarRebakeAnimationWindow.CloseIfOpen();
      CarFbxExporterWindow .CloseIfOpen();
      CarAddDefaultParticleSystemWindow.CloseIfOpen();
    }
    //-----------------------------------------------------------------------------------
    public void CustomHierarchyChange()
    {
      if (IsInited)
      {
        if ( !Hierarchy.AreCurrentEffectsValid() )
        {
          //something happened to the current FXs
          Deinit();
          LoadEffectsArray();
        }
        else if (  SimulationManager.IsEditing() &&
                  !EditorApplication.isPlayingOrWillChangePlaymode && !AnimationMode.InAnimationMode() )
        {
          bool wasUpdated = FxData.UpdateRootNodeName();
          if (wasUpdated)
          {
            EditorUtility.SetDirty(FxData);
          }
          hierarchyChangeRequested_ = true;
        }
      }
    }
    //-----------------------------------------------------------------------------------
    private void DoHierarchyChange()
    {
      if ( hierarchyChangeRequested_ && SimulationManager.IsEditing() && Hierarchy.AreCurrentEffectsValid() )
      {
        goManager_.HierarchyChange();
        if (IsInited)
        {
          Hierarchy.RecalculateFieldsAutomatic();
        }
        hierarchyChangeRequested_ = false;
      }
      CarManagerEditor.RepaintIfOpen();
    }
    //-----------------------------------------------------------------------------------
    private void DoUndoRedoChecks()
    {
      if (undoRedoChecksRequested_)
      {
        bool isSimulatingOrReplaying = simulationPlayer_.IsSimulating || simulationPlayer_.IsReplaying;

        if ( isSimulatingOrReplaying && fxData_.IsEditingState() )
        {
          Undo.PerformRedo();
          EditorUtility.DisplayDialog("CaronteFX - Info", "To undo more actions first go back to Edit mode.", "Ok");
        }
        else if ( SimulationManager.IsEditing() )
        {
          CloseWindows();
          Hierarchy.CheckHierarchyUndoRedo();
        }

        undoRedoChecksRequested_ = false;
      }
    }
    //-----------------------------------------------------------------------------------
    private void ChangeState()
    {
      if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isPlaying)
      {
        Deinit();
      }
    }
    //-----------------------------------------------------------------------------------
    private void CaronteEditorUpdate()
    {
      if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
      {
        Deinit();
      }

      if ( isInited_ )
      {
        if ( CarManagerEditor.IsFocused || CNFieldWindow.IsFocused )
        {
          DoHierarchyChange();
        }

        DoUndoRedoChecks();

        float elapsedTimeDisplay = (float)(EditorApplication.timeSinceStartup - lastUpdateTimeDisplay_);
   
        if ( elapsedTimeDisplay > timeForUpdateDisplay_ )
        {
          lastUpdateTimeDisplay_ = EditorApplication.timeSinceStartup;

          if ( SimulationManager.IsEditing() )
          {
            simulationDisplayer_.UpdateListsBodyGORequested    = true;
            simulationDisplayer_.UpdateClothCollidersRequested = true;
          }     

          simulationPlayer_.CycleStatus();      
          managerEditor_.Repaint();     
        }

        if ( SimulationManager.IsEditing() )
        {
          float elapsedTimeCheckForChanges = (float)(EditorApplication.timeSinceStartup - lastUpdateTimeCheckForChanges_);
          if ( elapsedTimeCheckForChanges > timeForCheckChanges_ )
          {
            lastUpdateTimeCheckForChanges_ = EditorApplication.timeSinceStartup;  
            CheckBodiesForChanges(false);
          }
        }

        simulationDisplayer_.Update();
      }
    }
    //-----------------------------------------------------------------------------------
    public void SetManagerEditor(CarManagerEditor managerEditor)
    {
      managerEditor_ = managerEditor;
      if (nodesListBox_ != null )
      {
        managerEditor_.NodesListBox = nodesListBox_;
        Hierarchy.SetManagerEditor(managerEditor);
      }
    }
    //-----------------------------------------------------------------------------------
    public IView GetFocusedNodeView()
    {
      return Hierarchy.GetFocusedNodeView();
    }
    //-----------------------------------------------------------------------------------
    void OnDomainUnload(object sender, EventArgs e) 
    {
      AppDomain.CurrentDomain.DomainUnload -= OnDomainUnload; 
      Deinit();
    }
    //-----------------------------------------------------------------------------------
    public static void ShowCaronteLog()
    {
      string[] errors;
      SimulationManager.GetLastErrors(out errors);
      if (errors.Length > 0)
      {
        CarDebug.Log("Caronte Log: ");
        for (int i = 0; i < errors.Length; ++i)
        {
          CarDebug.Log(errors[i]);
        }
      }
    }
    //----------------------------------------------------------------------------------
    public void ShowInvisibleBodies(bool active)
    {
      SimulationManager.SetBroadcastInvisibleBodies(active);
    }
    //----------------------------------------------------------------------------------
    private void CreateBodies()
    {
      List<CNBodyEditor> listBodyEditor = Hierarchy.ListBodyEditor;
      foreach(CNBodyEditor bodyEditor in listBodyEditor)
      {
        bodyEditor.CreateBodies();
      }
      EditorUtility.ClearProgressBar();
      hierarchy_.UpdateFieldLists();
    }
    //----------------------------------------------------------------------------------
    private void DestroyBodies()
    {
      List<CNBodyEditor> listBodyEditor = Hierarchy.ListBodyEditor;
      foreach(CNBodyEditor bodyEditor in listBodyEditor)
      {
        bodyEditor.DestroyBodies();
      }
      EditorUtility.ClearProgressBar();
    }
    //----------------------------------------------------------------------------------
    private void RecreateBodies()
    {
      List<CNBodyEditor> listBodyEditor = Hierarchy.ListBodyEditor;
      foreach(CNBodyEditor bodyEditor in listBodyEditor)
      {
        bodyEditor.RecreateBodies();
      }
      EditorUtility.ClearProgressBar();
    }
    //----------------------------------------------------------------------------------
    private void CreateJoints()
    {
      List<CNJointGroupsEditor> listMultiJointEditor = Hierarchy.ListMultiJointEditor;
      foreach (CNJointGroupsEditor multijointEditor in listMultiJointEditor)
      {
        multijointEditor.CreateEntities();
      }
    }
    //----------------------------------------------------------------------------------
    private void DestroyJoints()
    {
      List<CNJointGroupsEditor> listMultiJointEditor = Hierarchy.ListMultiJointEditor;
      foreach (CNJointGroupsEditor multijointEditor in listMultiJointEditor)
      {
        multijointEditor.DestroyEntities();
      }
    }
    //----------------------------------------------------------------------------------
    private void RecreateJoints()
    {
      List<CNJointGroupsEditor> listMultiJointEditor = Hierarchy.ListMultiJointEditor;

      int nJointsGroups = listMultiJointEditor.Count;

      for (int i = 0; i < nJointsGroups; i++)
      {
        CNJointGroupsEditor multijointEditor = listMultiJointEditor[i];
        EditorUtility.DisplayProgressBar("CaronteFX - Setting up joints","Setting up joints in " +  multijointEditor.Name, (float)i / (float)nJointsGroups);

        CNRigidGlueEditor rgEditor = multijointEditor as CNRigidGlueEditor;
        if (rgEditor != null)
        {
          rgEditor.RecreateEntitiesAsServos();
        }
        else
        {
          multijointEditor.RecreateEntities();
        }   
      }

      EditorUtility.ClearProgressBar();
    }
    //----------------------------------------------------------------------------------
    private void CreateServos()
    {
      List<CNServosEditor> listServosEditor = Hierarchy.ListServosEditor;
      foreach (CNServosEditor servosEditor in listServosEditor)
      {
        servosEditor.CreateEntities();
      }
    }
    //----------------------------------------------------------------------------------
    private void DestroyServos()
    {
      List<CNServosEditor> listServosEditor = Hierarchy.ListServosEditor;
      foreach (CNServosEditor servosEditor in listServosEditor)
      {
        servosEditor.DestroyEntities();
      }
    }
    //----------------------------------------------------------------------------------
    private void RecreateServos()
    {
      List<CNServosEditor> listServosEditor = Hierarchy.ListServosEditor;
      int nServos = listServosEditor.Count;

      for (int i = 0; i < nServos; i++)
      {
        CNServosEditor servosEditor = listServosEditor[i];
        EditorUtility.DisplayProgressBar("CaronteFX - Setting up motos/servos", "Setting up motors/servos in " +  servosEditor.Name, (float)i / (float)nServos);
        
        servosEditor.RecreateEntities();
      }

      EditorUtility.ClearProgressBar();
    }
    //----------------------------------------------------------------------------------
    private void RecreateRopes()
    {
      List<CNBodyEditor> listBodyEditor = Hierarchy.ListBodyEditor;
      foreach (CNBodyEditor bodyEditor in listBodyEditor)
      {
        CNRopeEditor rpEditor = bodyEditor as CNRopeEditor;
        if (rpEditor != null)
        {
          rpEditor.RecreateBodies();
        }
      }
    }
    //----------------------------------------------------------------------------------
    private void CreateCorpuscles()
    {
      List<CNCorpusclesEditor> listCorpusclesEditor = Hierarchy.ListCorpusclesEditor;
      foreach (CNCorpusclesEditor cpEditor in listCorpusclesEditor)
      {
        cpEditor.CreateCorpuscles();     
      }
    }
    //----------------------------------------------------------------------------------
    private void DestroyCorpuscles()
    {
      List<CNCorpusclesEditor> listCorpusclesEditor = Hierarchy.ListCorpusclesEditor;
      foreach (CNCorpusclesEditor cpEditor in listCorpusclesEditor)
      {
        cpEditor.DestroyCorpuscles();     
      }
    }
    //----------------------------------------------------------------------------------
    private void CheckBodiesForChanges(bool recreateIfInvalid)
    {
      List<CNBodyEditor> listBodyEditor = Hierarchy.ListBodyEditor;
      int nBodyNodes = listBodyEditor.Count;

      for (int i = 0; i < nBodyNodes; i++)
      {
        CNBodyEditor bodyEditor = listBodyEditor[i];

        if (recreateIfInvalid)
        {
          string displayString = "Checking body nodes..." + (i + 1) + " of " + nBodyNodes + ".";
          float progress = (float)i / (float)nBodyNodes;
          EditorUtility.DisplayProgressBar("CaronteFX - Checking bodies for updated geometry.", displayString, progress);
        }

        bodyEditor.CheckBodiesForChanges(recreateIfInvalid);
      }
      EditorUtility.ClearProgressBar();
    }
    //----------------------------------------------------------------------------------
    private void SetBodiesActivityVisibilityState()
    {
      List<CNBodyEditor> listBodyEditor = Hierarchy.ListBodyEditor;

      foreach ( CNBodyEditor bodyEditor in listBodyEditor )
      {
        bodyEditor.SetVisibilityState();
        bodyEditor.SetActivityIfDisabled();
      }
    }
    //----------------------------------------------------------------------------------
    private void EditBodiesCollisionFlags()
    {
      CollisionEditingBlockedEnd();

      List<CNBodyEditor> listBodyEditor = Hierarchy.ListBodyEditor;
      foreach ( CNBodyEditor bodyEditor in listBodyEditor )
      {
        bodyEditor.EditCollisionFlags();
      }
    }
    //----------------------------------------------------------------------------------
    public void ValidateEditorsState()
    {
      if ( SimulationManager.IsEditing() )
      {
        Hierarchy.ValidateEditors();
      }
    }
    //----------------------------------------------------------------------------------
    private void CreateEntities()
    {
      List<CNEntityEditor> listEntityEditor = Hierarchy.ListEntityEditor;

      foreach (CNEntityEditor entityEditor in listEntityEditor)
      {
        entityEditor.CreateEntity();  
      }
    }
    //----------------------------------------------------------------------------------
    private void ApplyEntities()
    {
      List<CNEntityEditor> listEditorNoTrigger = new List<CNEntityEditor>();
      List<CNEntityEditor> listEditorTrigger   = new List<CNEntityEditor>();

      foreach (CNEntityEditor entityEditor in Hierarchy.ListEntityEditor)
      {
        CNTriggerEditor triggerEditor = entityEditor as CNTriggerEditor;
        if ( triggerEditor == null )
        {
          listEditorNoTrigger.Add( entityEditor );
        }
        else
        {
          listEditorTrigger.Add( entityEditor );
        }
      }

      foreach (CNEntityEditor entityEditor in listEditorNoTrigger)
      {
        entityEditor.ApplyEntity();
      }

      foreach (CNEntityEditor entityEditor in listEditorTrigger)
      {
        entityEditor.ApplyEntity();
      }
    }
    //----------------------------------------------------------------------------------
    private void AddAnimations()
    {
      foreach (CNAnimatedbodyEditor animNodeEditor in Hierarchy.ListAnimatedBodyEditor)
      {
        if ( !animNodeEditor.IsExcludedInHierarchy )
        {
          Player.AddAnimation(animNodeEditor);
        }
      }
    }
    //----------------------------------------------------------------------------------
    void ConsoleSimulationStart()
    {
      Debug.Log("**********************SIMULATION START**********************");
    }
    //----------------------------------------------------------------------------------
    public void StartSimulating()
    {
      if ( entityManager_.NumberOfBodies == 0 )
      {
        EditorUtility.DisplayDialog("CaronteFX - Info", "In order to Simulate, you must first define some object bodies.", "Ok" );
        return;
      }

      bool canSimulate = CarDataUtils.CheckDisplayEvaluationMessage(FxData);
      if (canSimulate)
      {
        if (FxData.DeterministicMode)
        {
          ResetSimulation();
        }

        DoPreSimulationOperations();
        SimulationStart();
      }
    }
    //----------------------------------------------------------------------------------
    private void DoPreSimulationOperations()
    {
      ConsoleSimulationStart();

      ValidateEditorsState();

      SimulationManager.ForceReadjust();

      DestroyJoints();
      DestroyServos();

      CheckBodiesForChanges(true);

      EditorUtility.DisplayProgressBar("CaronteFX - Saving state of bodies", "Saving state...", 1.0f);
      entityManager_.SaveStateOfBodies();
      EditorUtility.ClearProgressBar();

      RecreateJoints();
      RecreateServos();

      EditorUtility.DisplayProgressBar("CaronteFX - Creating corpuscles", "Creating corpuscles...", 1.0f);
      CreateCorpuscles();
      EditorUtility.ClearProgressBar();

      EditorUtility.DisplayProgressBar("CaronteFX - Applying entities", "Applying entities...", 1.0f);
      ApplyEntities();
      EditorUtility.ClearProgressBar();

      EditorUtility.DisplayProgressBar("CaronteFX - Setting body state properties", "Setting body state properties...", 1.0f);
      SetBodiesActivityVisibilityState();
      EditorUtility.ClearProgressBar();

      EditorUtility.DisplayProgressBar("CaronteFX - Setting body collision state properties", "Setting body collision state properties...", 1.0f);
      EditBodiesCollisionFlags();
      EditorUtility.ClearProgressBar();

      Undo.RecordObject(fxData_, "CaronteFX - Start simulating");

      FxData.SetStateSimulating();
      EditorUtility.SetDirty(fxData_);
    }
    //----------------------------------------------------------------------------------
    private void SimulationStart()
    {
      if ( SimulationManager.IsEditing() )
      { 
        SimulationParams simParams = CarDataUtils.GetSimulationParams(EffectData);
  
        EditorUtility.DisplayProgressBar("CaronteFX - Adding animations", "Adding animations...", 1.0f);
        AddAnimations();
        EditorUtility.ClearProgressBar();

        EditorUtility.DisplayProgressBar("CaronteFX - Creating simulation meshes", "Creating simulation meshes...", 1.0f);
        entityManager_.CreateTmpSimulationGameObjects();
        EditorUtility.ClearProgressBar();

        EditorUtility.DisplayProgressBar("CaronteFX - Init callback manager", "Creating callback objects...", 1.0f);
        InitCallbackManager();
        EditorUtility.ClearProgressBar();

        Debug.Log("[CaronteFX] Starting simulation with " + simParams.nThreads_ + " threads");
        SetEditingStateEnd();
        UN_SimulationProperties un_simProperties = Player.SimulatingBeginFirst( simParams, EffectData.GetAnimationSamplingRate() );
        Debug.Log("[CaronteFX] Delta time: " + un_simProperties.deltaTime_);

        EffectData.SetLastUsedProperties( un_simProperties.deltaTime_, 2 * un_simProperties.distThick_, 2 * un_simProperties.distNarrow_ );
        EditorUtility.SetDirty( FxData );

        Selection.activeGameObject = null;
        Hierarchy.BlockEdition = true;

        simulationDisplayer_.Update();
        EditorApplication.delayCall += CNFieldWindow.CloseIfOpen;    
      }
    }
    //----------------------------------------------------------------------------------
    public void PrepareToRestartSimulation()
    {
      SetEditingStateBegin();

      SimulationManager.PrepareToRestartSimulation();   
      SimulationManager.RequestDisplay();

      CollisionEditingBlockedBegin();

      entityManager_.DestroyBodiesTmpGameObjects();

      DestroyJoints();
      DestroyServos();
      DestroyCorpuscles();

      entityManager_.LoadStateOfBodies();

      CreateJoints();
      CreateServos();

      CustomHierarchyChange();
      Hierarchy.BlockEdition = false;
      
      fxData_.SetStateEditing();
      EditorUtility.SetDirty(fxData_);

      simulationDisplayer_.Init(fxData_);
    }
    //----------------------------------------------------------------------------------
    private void InitCallbackManager()
    {
      var listCarObject = entityManager_.GetListCarObjectFromArrNode( Hierarchy.ListCommandNode.ToArray() );
      Scripting.CarGlobals.Init(listCarObject);
      var listParamterModifierAction = entityManager_.BuildListParameterModifierAction( Hierarchy.ListCommandNode.ToArray() );

      CarCallbacksManager.Init(Hierarchy.ListScriptPlayerEditor, listParamterModifierAction);
    }
    //----------------------------------------------------------------------------------
    public void ShowPlayerWindow()
    {
      CarPlayerWindow.ShowWindow( simulationPlayer_ );
    }
    //----------------------------------------------------------------------------------
    public void ResetSimulation()
    {
      if (IsInited)
      { 
        if (CarPlayerWindow.IsOpen)
        {
          bool userConfirmation = simulationPlayer_.ChangeToEditModeRequest();
          if (!userConfirmation)
          {
            return;
          }
        }

        Deinit();
        LoadEffectsArray();
        Init();
      }
    }
    //----------------------------------------------------------------------------------
    public void SceneSelection()
    {
      Hierarchy.SceneSelection();
    }
    //----------------------------------------------------------------------------------
    public void GetListBodyNodesForBake( List<CNBody> listBodyNode )
    {
      listBodyNode.Clear();

      foreach( CommandNodeEditor nodeEditor in Hierarchy.ListCommandNodeEditor )
      {
        CNBodyEditor  bodyEditor   = nodeEditor as CNBodyEditor;
        if ( bodyEditor != null && !bodyEditor.IsExcludedInHierarchy )
        {
          listBodyNode.Add( (CNBody)nodeEditor.Data );
        }
      }
    }
    //----------------------------------------------------------------------------------
    public bool IsEffectIncluded(Caronte_Fx fx)
    {
      return Hierarchy.IsEffectIncluded(fx);
    }
    //----------------------------------------------------------------------------------
    public bool RootNodeAlreadyContainedInNode(Caronte_Fx fx)
    {
      return Hierarchy.RootNodeAlreadyContainedInNode(fx);
    }
    //----------------------------------------------------------------------------------
    public void SetEnableStateSelection(bool enabled)
    {
      Hierarchy.SetEnableStateSelection(enabled);
    }
    //----------------------------------------------------------------------------------
    public void DuplicateSelection()
    {
      Hierarchy.DuplicateSelection();
    }
    //----------------------------------------------------------------------------------
    public void ContextClickExternal()
    {
      Hierarchy.ContextClick();
    }
    //----------------------------------------------------------------------------------
    public void DoDeferredActions()
    {
      Hierarchy.RemoveNodesDefeerred();
    }
    //----------------------------------------------------------------------------------
    public void RefreshEffectsArray()
    {    
      Caronte_Fx[] arrFxData = CarEditorUtils.GetAllSceneComponentsOfType<Caronte_Fx>();
      Array.Sort(arrFxData, delegate(Caronte_Fx fx1, Caronte_Fx fx2)
      {
        return fx1.gameObject.name.CompareTo(fx2.gameObject.name);
      });

      int indexOfCurrent = -1;
      for (int i = 0; i < arrFxData.Length; i++)
      {
        if ( arrFxData[i] == FxData )
        {
          indexOfCurrent = i;
        }
      }

      if (indexOfCurrent != -1)
      {
        CurrentFxIdx = indexOfCurrent;
      }
  
      listFxData_.Clear();
      listFxData_.AddRange(arrFxData);
    }
    //----------------------------------------------------------------------------------
    public void LoadEffectsArray()
    {
      Caronte_Fx[] arrFxData = CarEditorUtils.GetAllSceneComponentsOfType<Caronte_Fx>();
      int arrFxData_size = arrFxData.Length;

      UpdateArrCaronteFXData(arrFxData);

      Array.Sort(arrFxData, delegate(Caronte_Fx fx1, Caronte_Fx fx2)
      {
        return fx1.gameObject.name.CompareTo(fx2.gameObject.name);
      });

      bool foundActive = false;
      for (int i = 0; i < arrFxData_size; i++)
      {
        Caronte_Fx data = arrFxData[i];
        if (data.ActiveInEditor)
        {
          foundActive   = true;
          FxData        = data;
          currentFxIdx_ = i;
          break;
        }
      }

      if (!foundActive && arrFxData_size > 0)
      {
        Caronte_Fx data = arrFxData[0];
        data.ActiveInEditor = true;
        EditorUtility.SetDirty(data);
        FxData = data;
      }

      listFxData_.Clear();
      listFxData_.AddRange(arrFxData);
    }
    //-----------------------------------------------------------------------------------
    private void UpdateArrCaronteFXData(Caronte_Fx[] arrCaronteFX)
    {
      foreach (Caronte_Fx fxData in arrCaronteFX)
      {
        if (fxData != null && fxData.DataVersion < 5)
        {
          CarDataUtils.UpdateFxDataVersionIfNeeded(fxData);
        }
      }
    }
    //----------------------------------------------------------------------------------
    private void DeactivateEffects()
    {
      Caronte_Fx[] arrFxData = CarEditorUtils.GetAllSceneComponentsOfType<Caronte_Fx>();
      int arrFxData_size = arrFxData.Length;

      for (int i = 0; i < arrFxData_size; i++)
      {
        Caronte_Fx data = arrFxData[i];
        data.ActiveInEditor = false;
        EditorUtility.SetDirty(data);
      }

      listFxData_.Clear();
      listFxData_.AddRange(arrFxData);
    }
    //----------------------------------------------------------------------------------
    public void CreateNewFx()
    {
      Deinit();

      // create 
      DeactivateEffects();

      GameObject go = new GameObject("CaronteFx_0");
      Undo.RegisterCreatedObjectUndo (go, "Created CaronteFX GameObject");

      go.tag = "EditorOnly";
      Caronte_Fx data = go.AddComponent<Caronte_Fx>();

      MakeFxNameUnique(data);
      data.ActiveInEditor = true;
      EditorUtility.SetDirty(data);

      EditorGUIUtility.PingObject(data);
      Selection.activeGameObject = data.gameObject;

      LoadEffectsArray();
    }
    //----------------------------------------------------------------------------------
    public void ChangeToFx(int fxIdx)
    {
      Caronte_Fx data = listFxData_[fxIdx];

      DeactivateEffects();
      
      Undo.RecordObject(data, "CaronteFX - change active fx");
      data.ActiveInEditor = true;
      EditorUtility.SetDirty(data);

      Deinit();

      EditorGUIUtility.PingObject(data);
      Selection.activeGameObject = data.gameObject;

      LoadEffectsArray();
    }
    //----------------------------------------------------------------------------------
    public void SetFxDataActive(Caronte_Fx fxDataToActivate)
    {
      if (fxDataToActivate == FxData)
      {
        return;
      }

      Caronte_Fx[] arrFxData = GameObject.FindObjectsOfType<Caronte_Fx>();

      int arrFxData_size = arrFxData.Length;
      for (int i = 0; i < arrFxData_size; i++)
      {
        Caronte_Fx _fxData = arrFxData[i];
        _fxData.ActiveInEditor = false;
        if (_fxData == fxDataToActivate)
        {
          fxDataToActivate.ActiveInEditor = true;
          FxData = fxDataToActivate;
          currentFxIdx_ = i;
        }
      }
    }
    //----------------------------------------------------------------------------------
    public void MakeFxNameUnique(Caronte_Fx __fxData)
    {
      bool loop = false;
      int count = 0;
      GameObject __go = __fxData.gameObject;
      do
      {
        if (loop) loop = false;
        foreach (Caronte_Fx _fxData in listFxData_)
        {
          if (_fxData != null)
          {
            GameObject _go = _fxData.gameObject;
            if (_fxData != __fxData && _go.name == __go.name)
            {
              int index = __go.name.IndexOf("_");
              __go.name = __go.name.Substring(0, index);
              count++;
              __go.name += "_" + count;
              loop = true;
              break;
            }
          }
        }
      } while (loop);
    }
    //----------------------------------------------------------------------------------
    public string[] GetFxNames(out string longestName)
    {
      Caronte_Fx[] arrFxData = listFxData_.ToArray();
      int arrFxData_size = arrFxData.Length;
      List<string> names = new List<String>(arrFxData_size + 2);
      longestName = string.Empty;
      for (int i = 0; i < arrFxData_size; i++)
      {
        if (arrFxData[i] != null)
        {
          string effectName = arrFxData[i].name;
          string auxName = effectName;
          int id = 2;
          while (names.Contains(auxName))
          {
            auxName = effectName + " (" + id + ")";
            id++;
          }
          names.Add(auxName);
          if (auxName.Length > longestName.Length)
          {
            longestName = auxName;
          }
        }
      }
      longestName += "........";

      return names.ToArray();
    }
    //----------------------------------------------------------------------------------
    public void GetBodiesData(List<CRBodyData> listBodyData)
    {
      listBodyData.Clear();

      Transform[] selection = Selection.GetTransforms(SelectionMode.ExcludePrefab);

      foreach (Transform tr in selection)
      {
        GameObject go = tr.gameObject;
        Caronte_Fx_Body fxBody = go.GetComponent<Caronte_Fx_Body>();
        CRBodyData bodyData;
        if (fxBody != null)
        {
          GetBodyData( go, out bodyData );
          listBodyData.Add(bodyData);
        }    
      }

      if (listBodyData.Count == 0)
      {
        listBodyData.Add( new CRBodyData() );
      }
    }
    //-----------------------------------------------------------------------------------
    public void GetBodyData(GameObject gameObject, out CRBodyData bodyData)
    {
      bodyData = new CRBodyData();

      uint idBody = uint.MaxValue;
      if (simulationPlayer_.IsEditing)
      {
        idBody = entityManager_.GetIdBodyFromGo( gameObject );
      }
      else
      {
        idBody = entityManager_.GetIdBodyFromGOForSimulatingOrReplaying( gameObject );
      }
   
      if (idBody != uint.MaxValue)
      {
        List<CommandNode> listNode = entityManager_.GetListNodeReferences(idBody);

        BodyType bodyType  = entityManager_.GetBodyType(idBody);
        bodyData.idBody_   = idBody;
        bodyData.bodyType_ = bodyType;
        bodyData.listNode_ = listNode;
      }
    }
    //----------------------------------------------------------------------------------
    public void ModifyEffectsIncluded(List<GameObject> listCaronteFxGameObjectToDeinclude, List<GameObject> listCaronteFXGameObjectToInclude)
    {
      Hierarchy.ModifyEffectsIncluded(listCaronteFxGameObjectToDeinclude, listCaronteFXGameObjectToInclude);
    }
    //----------------------------------------------------------------------------------
    public void CreateNodeUnique<T>(string name)
      where T : CommandNode
    {
      Hierarchy.CreateNodeUnique<T>(name);
    }
    //----------------------------------------------------------------------------------
    public void CreateIrresponsiveBodiesNode()
    {
      Hierarchy.CreateIrresponsiveBodiesNode();
    }
    //----------------------------------------------------------------------------------
    public void CreateRopeBodiesNode()
    {
      Hierarchy.CreateRopeBodiesNode();
    }
    //----------------------------------------------------------------------------------
    public void CreateMultiJointNodeArea()
    {
      Hierarchy.CreateMultiJointAreaNode(); 
    }
    //----------------------------------------------------------------------------------
    public void CreateMultiJointNodeVertices()
    {
      Hierarchy.CreateMultiJointVerticesNode(); 
    }
    //----------------------------------------------------------------------------------
    public void CreateMultiJointNodeLeaves()
    {
      Hierarchy.CreateMultiJointLeavesNode(); 
    }
    //----------------------------------------------------------------------------------
    public void CreateMultiJointNodeLocators()
    {
      Hierarchy.CreateMultiJointLocatorsNode(); 
    }
    //----------------------------------------------------------------------------------
    public void CreateRigidglueNode()
    {
      Hierarchy.CreateRigidGlueNode();
    }
    //----------------------------------------------------------------------------------
    public void CreateServosLinearNode()
    {
      Hierarchy.CreateServosLinearNode();
    }
    //----------------------------------------------------------------------------------
    public void CreateServosAngularNode()
    {
      Hierarchy.CreateServosAngularNode();
    }
    //----------------------------------------------------------------------------------
    public void CreateMotorsLinearNode()
    {
      Hierarchy.CreateMotorsLinearNode();
    }
    //----------------------------------------------------------------------------------
    public void CreateMotorsAngularNode()
    {
      Hierarchy.CreateMotorsAngularNode();
    }
    //----------------------------------------------------------------------------------
    public void CreateFracturerNodeUniform()
    {
      Hierarchy.CreateFracturerUniformNode();
    }
    //----------------------------------------------------------------------------------
    public void CreateFracturerNodeGeometry()
    {
      Hierarchy.CreateFracturerGeometryNode();
    }
    //----------------------------------------------------------------------------------
    public void CreateFracturerNodeRadial()
    {
      Hierarchy.CreateFracturerRadialNode();
    }
    //----------------------------------------------------------------------------------
    public void CreateExplosionNode()
    {
      Hierarchy.CreateExplosionNode();
    }
    //----------------------------------------------------------------------------------
    public void CreateClothBodiesNode()
    {
      Hierarchy.CreateClothBodiesNode();
    }
    //----------------------------------------------------------------------------------
    public CommandNodeEditor GetNodeEditor(CommandNode node)
    {
      return Hierarchy.GetNodeEditor(node);
    }
    //----------------------------------------------------------------------------------
    private void OnSceneGUI(SceneView sceneView)
    {
      if (FxData != null && isInited_)
      {
        if (FxData.DrawExplosions)
        {
          simulationDisplayer_.RenderExplosions( Mathf.Log(FxData.ExplosionsOpacity) / 10f );
        }

        if (FxData.DrawContactEvents)
        {
          simulationDisplayer_.RenderContactEvents( Mathf.Log(FxData.ContactEventSize) / 10f );
        }
    
        if (FxData.ShowOverlay)
        {
          DrawOverlay(sceneView);
        }

        DrawBalltrees();
      }
    }
    //----------------------------------------------------------------------------------
    public void DrawOverlay(SceneView sceneView)
    {
      if (isInited_)
      {
        Handles.BeginGUI();

        simulationDisplayer_.RenderStatistics();

        Rect svRect = sceneView.position;
        Rect windowRect = new Rect(svRect.size.x - 215f, svRect.size.y - 150f, 210f, 145f);

        string title = "CaronteFX";
        windowRect = GUILayout.Window(0, windowRect, DoSceneWindow, title);

        Handles.EndGUI();
      }
    }
    //-----------------------------------------------------------------------------------
    private void SetSelectionGridButtonNames()
    {

      if (FxData.DrawBodyBoxes ||
          FxData.DrawClothColliders ||
          FxData.ShowInvisibles )
      {
        sceneGUIButtonNames_[0] = "*Bodies*";
      }
      else
      {
        sceneGUIButtonNames_[0] = "Bodies";
      }

      if (FxData.DrawJoints)
      {
        sceneGUIButtonNames_[1] = "*Joints*";
      }
      else
      {
        sceneGUIButtonNames_[1] = "Joints";
      }

      if (FxData.DrawExplosions)
      {
        sceneGUIButtonNames_[2] = "*Explosions*";
      }
      else
      {
        sceneGUIButtonNames_[2] = "Explosions";
      }

      if (FxData.DrawContactEvents)
      {
        sceneGUIButtonNames_[3] = "*Events*";
      }
      else
      {
        sceneGUIButtonNames_[3] = "Events";
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawBodiesSection()
    {
      EditorGUILayout.BeginHorizontal(); 
      EditorGUI.BeginChangeCheck();
      FxData.DrawBodyBoxes = GUILayout.Toggle(FxData.DrawBodyBoxes, "Draw boxes");
      if (EditorGUI.EndChangeCheck())
      {
        simulationDisplayer_.UpdateListsBodyGORequested = true;
        EditorUtility.SetDirty(FxData);
      }
      EditorGUI.BeginChangeCheck();
      FxData.DrawClothColliders = GUILayout.Toggle( FxData.DrawClothColliders, "Cloth balls");
      if (EditorGUI.EndChangeCheck())
      {
        simulationDisplayer_.UpdateClothCollidersRequested = true;
        EditorUtility.SetDirty(FxData);
      }
      EditorGUILayout.EndHorizontal();
      GUILayout.Space(2f);
      EditorGUI.BeginChangeCheck();
      Color current = GUI.contentColor;
      if (FxData.ShowInvisibles)
      {
        GUI.contentColor = Color.red;
      }
      FxData.ShowInvisibles = GUILayout.Toggle(FxData.ShowInvisibles, "Draw invisibles");
      GUI.contentColor = current;
      if (EditorGUI.EndChangeCheck())
      {
        ShowInvisibleBodies(FxData.ShowInvisibles);
        simulationDisplayer_.UpdateListsBodyGORequested = true;
        EditorUtility.SetDirty(FxData);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawJointsSection()
    {
      EditorGUI.BeginChangeCheck();
      EditorGUILayout.BeginHorizontal();  
      FxData.DrawJoints = GUILayout.Toggle(FxData.DrawJoints, "Draw joints");
      EditorGUI.BeginDisabledGroup(!FxData.DrawJoints);
      FxData.DrawOnlySelected = GUILayout.Toggle(FxData.DrawOnlySelected, "Only selected");
      EditorGUILayout.EndHorizontal();
      if (EditorGUI.EndChangeCheck())
      {
        Hierarchy.SceneSelection();
        simulationDisplayer_.UpdateListJointsRequested = true;
        EditorUtility.SetDirty(FxData);
      }

      EditorGUI.BeginChangeCheck();
      GUILayout.BeginHorizontal();
      GUILayout.Label( "Render size", GUILayout.MaxWidth(100) );
      FxData.JointsSize = GUILayout.HorizontalSlider(FxData.JointsSize, 1.02f, 5f);
      GUILayout.EndHorizontal();
      EditorGUI.EndDisabledGroup();
      if (EditorGUI.EndChangeCheck())
      {
        simulationDisplayer_.UpdateListJointsRequested = true;
        EditorUtility.SetDirty(FxData);
        }
    }
    //-----------------------------------------------------------------------------------
    private void DrawExplosionsSection()
    {
      EditorGUI.BeginChangeCheck();
      FxData.DrawExplosions = GUILayout.Toggle(FxData.DrawExplosions, "Draw explosions");
      EditorGUI.BeginDisabledGroup(!FxData.DrawExplosions);
      GUILayout.BeginHorizontal();
      GUILayout.Label("Render opacity", GUILayout.MaxWidth(100));
      FxData.ExplosionsOpacity = GUILayout.HorizontalSlider(FxData.ExplosionsOpacity, 1.5f, 10f);
      GUILayout.EndHorizontal();
      EditorGUI.EndDisabledGroup();
      if (EditorGUI.EndChangeCheck())
      {
        EditorUtility.SetDirty(FxData);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawEventsSection()
    {
      EditorGUI.BeginChangeCheck();
      FxData.DrawContactEvents = GUILayout.Toggle(FxData.DrawContactEvents, "Draw contacts");
      EditorGUI.BeginDisabledGroup(!FxData.DrawContactEvents);
      GUILayout.BeginHorizontal();
      GUILayout.Label("Render size", GUILayout.MaxWidth(100));
      FxData.ContactEventSize = GUILayout.HorizontalSlider(FxData.ContactEventSize, 1.5f, 10f);
      GUILayout.EndHorizontal();
      EditorGUI.EndDisabledGroup();
      if (EditorGUI.EndChangeCheck())
      {
        EditorUtility.SetDirty(FxData);
      }
    }
    //-----------------------------------------------------------------------------------
    private void DrawVisualizationSection()
    {
      bool isSimulatingOrReplaying = simulationPlayer_.IsSimulating || simulationPlayer_.IsReplaying;

      List<Renderer>        listBodyRenderer;
      List<Caronte_Fx_Body> listBodyComponent;
      entityManager_.GetListBodyRendererAndBodyComponent( !isSimulatingOrReplaying, out listBodyRenderer, out listBodyComponent );

      CarToggleUtils.DrawToggleMixedRenderers("Draw render meshes", listBodyRenderer, 150f);
      GUILayout.Space(2f);
      CarToggleUtils.DrawToggleMixedBodyComponents("Draw collider meshes", listBodyComponent, 190f);
    }
    //-----------------------------------------------------------------------------------
    private void DrawBalltrees()
    {
      bool isSimulatingOrReplaying = simulationPlayer_.IsSimulating || simulationPlayer_.IsReplaying;

      List<Renderer> listBodyRenderer;
      List<Caronte_Fx_Body> listBodyComponent;
      entityManager_.GetListBodyRendererAndBodyComponent( !isSimulatingOrReplaying, out listBodyRenderer, out listBodyComponent);

      CarBalltreeMeshesManager bmm = CarBalltreeMeshesManager.Instance;
      Material btMaterial = bmm.GetBalltreeMaterial();

      bool repaintRequest = false;
      foreach(Caronte_Fx_Body cfxBody in listBodyComponent)
      { 
        if (cfxBody != null && cfxBody.IsUsingBalltree() && cfxBody.IsDrawRequested())
        {
          CRBalltreeAsset btAsset = cfxBody.GetBalltreeAsset();

          if (!bmm.HasBalltreeMeshes(btAsset) )
          {
            EditorUtility.DisplayProgressBar("CaronteFX - Building balltree meshes", "Baltree mesh representation is being built, please wait...", 1.0f);
          }
          CarBalltreeMeshes btMeshes = bmm.GetBalltreeMeshes(btAsset);

          Transform tr = cfxBody.transform;
          Matrix4x4 m_MODEL_to_WORLD = tr.localToWorldMatrix;
          Vector3 scale = m_MODEL_to_WORLD.GetScalePre();
          Vector3 uniformScaleX = new Vector3(scale.x, scale.x, scale.x);

          Matrix4x4 m_MODEL_to_WORLD_PRIM = Matrix4x4.TRS(tr.position, tr.rotation, uniformScaleX);
          btMeshes.DrawMeshesSolid(m_MODEL_to_WORLD_PRIM, btMaterial);
          repaintRequest = true;
        }  
      }

      if (repaintRequest)
      {
        SceneView.RepaintAll();
        EditorUtility.ClearProgressBar();
      }
    }
    //-----------------------------------------------------------------------------------
    private void DoSceneWindow(int windowID)
    {
      SetSelectionGridButtonNames();

      GUIStyle styleTabButton = new GUIStyle(EditorStyles.toolbarButton);
      styleTabButton.fontSize    = 10;
      styleTabButton.fixedHeight = 19f;
      styleTabButton.alignment = TextAnchor.MiddleCenter;
      selectedButton_ = GUILayout.SelectionGrid(selectedButton_, sceneGUIButtonNames_, 2, styleTabButton );
      CarGUIUtils.Splitter(Color.gray);
 
      if ( selectedButton_ == 0)
      {
        DrawBodiesSection();
      }
      else if(selectedButton_ == 1)
      {
        DrawJointsSection();
      }
      else if (selectedButton_ == 2)
      {
        DrawExplosionsSection();
      }
      else if (selectedButton_ == 3)
      {
        DrawEventsSection();
      }
      else if (selectedButton_ == 4)
      {
        DrawVisualizationSection();
      }
      
      if ( SimulationManager.IsEditing() )
      {
        string editorString = CarManagerEditor.IsOpen ? "Focus Editor" : "Open Editor";
        if ( GUILayout.Button(editorString) )
        {
          CarManagerEditor.Init();
        }
      }
      else
      {
        string playerString = CarPlayerWindow.IsOpen ? "Focus Player" : "Open Player";
        if ( GUILayout.Button(playerString) )
        {
          CarPlayerWindow.ShowWindow( Player );
        }
      } 
    }
    //-----------------------------------------------------------------------------------
    public void BuildBakerData()
    {
      simulationBaker_.BuildBakerInitData();
    }
    //-----------------------------------------------------------------------------------
    public void BuildDisplayerVisibilityIntervals()
    {
      simulationDisplayer_.BuildVisibilityIntervals();
    }
  } 
}
