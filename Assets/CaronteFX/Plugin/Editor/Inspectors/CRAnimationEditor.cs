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
using System.Collections;
using System.IO;
using System.Collections.Generic;

namespace CaronteFX
{
  [CanEditMultipleObjects]
  [CustomEditor(typeof(CRAnimation))]
  public class CRAnimationEditor : Editor
  {
    CRAnimation ac_;

    float  editorFrame_;
    double lastPreviewTime_ = 0;

    SerializedProperty activeAnimationTextProp_;
    SerializedProperty activeAnimationAssetProp_;

    SerializedProperty gpuVertexAnimationProp_;
    SerializedProperty gpuSkinnedAnimationProp_;

    SerializedProperty bufferAllFramesProp_;
    SerializedProperty gpuFrameBufferSizeProp_;
    SerializedProperty overrideShaderForGPUProp_;
    SerializedProperty useDoubleSidedShaderProp_;
    SerializedProperty recomputeNormalsProp_;

    static Texture ic_logoCaronte_ = null;
    bool slowMode_ = false;

    void OnEnable()
    {
      activeAnimationTextProp_  = serializedObject.FindProperty("activeAnimationText");
      activeAnimationAssetProp_ = serializedObject.FindProperty("activeAnimation");

      gpuVertexAnimationProp_  = serializedObject.FindProperty("decodeInGPU_");
      gpuSkinnedAnimationProp_ = serializedObject.FindProperty("skinningInGPU_");

      bufferAllFramesProp_      = serializedObject.FindProperty("bufferAllFrames_");
      gpuFrameBufferSizeProp_   = serializedObject.FindProperty("gpuFrameBufferSize_");
      overrideShaderForGPUProp_ = serializedObject.FindProperty("overrideShaderForVertexAnimation_");
      useDoubleSidedShaderProp_ = serializedObject.FindProperty("useDoubleSidedShader_");
      recomputeNormalsProp_     = serializedObject.FindProperty("recomputeNormals_");

      ac_ = (CRAnimation)target;
      editorFrame_ = Mathf.Max(ac_.LastReadFrame, 0);
      slowMode_ = ac_.IsFiberCompression() && !ac_.GPUVertexAnimation;

      SetAnimationPreviewMode();
      LoadCaronteIcon();
    }

    void OnDisable()
    {

    }

    public override void OnInspectorGUI()
    {
      serializedObject.Update();
      bool isPlayingOrWillChangePlaymode = EditorApplication.isPlayingOrWillChangePlaymode;

      Rect rect = GUILayoutUtility.GetRect(80f, 80f);
      GUI.DrawTexture(rect, ic_logoCaronte_, ScaleMode.ScaleToFit );
      CarGUIUtils.Splitter();

      bool isUsingGPU =    gpuVertexAnimationProp_.boolValue 
                        || gpuSkinnedAnimationProp_.boolValue;

      if (Selection.gameObjects.Length > 1)
      {
        DrawMultiInspector(isUsingGPU, isPlayingOrWillChangePlaymode);
        return;
      }

      DrawSingleInspector(isUsingGPU, isPlayingOrWillChangePlaymode);

      if (isPlayingOrWillChangePlaymode)
      {
        Repaint();
      }
    }

    private void DrawMultiInspector(bool isUsingGPU, bool isPlayingOrWillChangePlaymode)
    {
      EditorGUILayout.PropertyField(activeAnimationTextProp_,  new GUIContent("Active animation (TextAsset)") );
      EditorGUILayout.PropertyField(activeAnimationAssetProp_, new GUIContent("Active animation (CRAnimationAsset)") );
      serializedObject.ApplyModifiedProperties();
      DrawDefaultInspector();
      EditorGUILayout.Space();

      bool disabledGPU = isPlayingOrWillChangePlaymode || ac_.PreviewInEditor;
      EditorGUI.BeginDisabledGroup(disabledGPU);

      DrawGPUVertexAnimation();
      DrawGPUSkinnedAnimation();
      EditorGUI.BeginDisabledGroup(isUsingGPU);
      DrawBufferAllFrames();
      EditorGUI.EndDisabledGroup();
      EditorGUI.BeginDisabledGroup(bufferAllFramesProp_.boolValue);
      DrawGPUBufferSize();
      EditorGUI.EndDisabledGroup();
      DrawOverrideShaderForGPU();
      DrawUseDoubleSidedShader();
      EditorGUI.EndDisabledGroup();
      EditorGUILayout.Space();

      DrawRecomputeNormals(isPlayingOrWillChangePlaymode);
      DrawCPUSlowMode();

      serializedObject.ApplyModifiedProperties();
    }

    private void DrawSingleInspector(bool isUsingGPU, bool isPlayingOrWillChangePlaymode)
    {
      EditorGUILayout.Space();
      EditorGUILayout.Space();   
      DrawAnimationFileType();
      DrawAnimationFiles();
      CarGUIUtils.Splitter();
      EditorGUILayout.Space();
      EditorGUILayout.Space();
      DrawDefaultInspector();
      EditorGUILayout.Space();

      EditorGUI.BeginDisabledGroup(isPlayingOrWillChangePlaymode || ac_.PreviewInEditor );
      DrawGPUVertexAnimation();
      DrawGPUSkinnedAnimation();

      bool gpuModeRequested = ac_.GPUVertexAnimation || ac_.GPUSkinnedAnimation;
      if (gpuModeRequested)
      {
        EditorGUILayout.HelpBox("GPU modes require compute shaders. Make sure that compute shaders are available in your target platform.", MessageType.Info);
      }

      EditorGUI.BeginDisabledGroup(!isUsingGPU);
      DrawBufferAllFrames();

      EditorGUI.BeginDisabledGroup(bufferAllFramesProp_.boolValue);
      DrawGPUBufferSize();
      EditorGUI.EndDisabledGroup();
      DrawOverrideShaderForGPU();
      DrawUseDoubleSidedShader();

      EditorGUI.EndDisabledGroup();
      EditorGUI.EndDisabledGroup();

      EditorGUILayout.Space();
      DrawRecomputeNormals(isPlayingOrWillChangePlaymode);
      DrawCPUSlowMode();

      serializedObject.ApplyModifiedProperties();
      EditorGUILayout.Space();

      GameObject go = ac_.gameObject;

      PrefabType pType = PrefabUtility.GetPrefabType(go);

      bool isPrefab = pType == PrefabType.Prefab;
      bool isPrefabInstance = pType ==PrefabType.PrefabInstance;

      bool isPrefabOrPrefabInstance = isPrefab || isPrefabInstance;

      EditorGUI.BeginDisabledGroup(isPlayingOrWillChangePlaymode || isPrefabOrPrefabInstance);
      DrawIsPreviewInEditor();
      if ( ac_.PreviewInEditor && ac_.GPUVertexAnimation )
      {
        EditorGUILayout.HelpBox("GPU Vertex Animation decoding is not available in editor preview mode. Standard CPU decoding will be used.", MessageType.Info);
      }

      if (isPrefabOrPrefabInstance)
      {
        EditorGUILayout.HelpBox("Preview in editor is disabled on prefab and prefab instances due to performance reasons. Use play mode or break the prefab connection through the GameObject menu.", MessageType.Info);
      }

      EditorGUI.EndDisabledGroup();
        
      bool isPlayingOrPreviewInEditor = isPlayingOrWillChangePlaymode || ac_.PreviewInEditor;
      EditorGUI.BeginDisabledGroup( !isPlayingOrPreviewInEditor );
      EditorGUI.BeginChangeCheck();

      editorFrame_ = Mathf.Clamp(ac_.LastReadFrame, 0, ac_.LastFrame);
      if (ac_.interpolate)
      {
        editorFrame_ = EditorGUILayout.Slider(new GUIContent("Frame"), editorFrame_, 0, ac_.LastFrame);
      }
      else
      {
        editorFrame_ = EditorGUILayout.IntSlider(new GUIContent("Frame"), (int)editorFrame_, 0, ac_.LastFrame);
      }  
      if (EditorGUI.EndChangeCheck() && isPlayingOrPreviewInEditor )
      {   
        ac_.SetFrame(editorFrame_);      
        SceneView.RepaintAll();
      }

      ac_.LoadActiveAnimationInfoMinimal();

      EditorGUI.EndDisabledGroup();
      EditorGUILayout.LabelField(new GUIContent("Time"),             new GUIContent(ac_.AnimationTime.ToString("F3")) );
      EditorGUILayout.LabelField(new GUIContent("Frame Count"),      new GUIContent(ac_.FrameCount.ToString()) );
      EditorGUILayout.LabelField(new GUIContent("FPS"),              new GUIContent(ac_.Fps.ToString()) );
      EditorGUILayout.LabelField(new GUIContent("Animation Length"), new GUIContent(ac_.AnimationLength.ToString()) );
      EditorGUILayout.LabelField(new GUIContent("Compression type"), new GUIContent(GetCompressionTypeString()) );

      EditorGUILayout.Space();
      EditorGUI.BeginDisabledGroup(ac_.AnimatorSync != null);
      DrawStartTimeOffset();
      EditorGUI.EndDisabledGroup();
      DrawSyncWithAnimator();
      CarGUIUtils.Splitter();

      EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode || isPrefab || ac_.IsPreviewing);

      CarGUIUtils.Splitter();

      EditorGUILayout.Space();

      DrawRebakeAnimation();

      DrawOptimizeTransformHierarchy();
      DrawUnoptimizeTransformHierarchy();

      DrawAddDefaultParticleSystem();

#if UNITY_EDITOR_WIN
      DrawExportToFbx();
#endif
      DrawRecordScreenshots();

      EditorGUI.EndDisabledGroup();
    }

    private void LoadCaronteIcon()
    {
      if (ic_logoCaronte_ == null)
      { 
        bool isUnityFree = !UnityEditorInternal.InternalEditorUtility.HasPro();
        if ( isUnityFree )
        {
          ic_logoCaronte_ = CarEditorResource.LoadEditorTexture("cr_caronte_logo_free");
        }
        else
        {
          ic_logoCaronte_ = CarEditorResource.LoadEditorTexture("cr_caronte_logo_pro");
        }
      }
    }

    private void DrawAnimationFileType()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.EnumPopup("Animation file type", ac_.animationFileType);
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(ac_, "Change animation file type");
        ac_.animationFileType = (CRAnimation.AnimationFileType)value;
        EditorUtility.SetDirty( ac_ );
      }
    }

    private void ConvertCRAnimationAssetsToTextAssets()
    {  
      CRAnimationAsset activeCRAnimationAsset = ac_.activeAnimation;

      if (activeCRAnimationAsset != null)
      {
        string oldAssetPath;
        TextAsset textAsset = ConvertCRAnimationAssetToTextAsset(activeCRAnimationAsset, out oldAssetPath );

        ac_.activeAnimation = null;
        ac_.RemoveAnimation(activeCRAnimationAsset);
        AssetDatabase.DeleteAsset(oldAssetPath);

        ac_.AddAnimationAndSetActive(textAsset);
      }

      List<CRAnimationAsset> listCRAnimationAsset = ac_.listAnimations;
      List<TextAsset>        listTextAsset        = ac_.listAnimationsText;

      int lastAnimationAssets = listCRAnimationAsset.Count - 1;

      for (int i = lastAnimationAssets; i >= 0; i--)
      {
        CRAnimationAsset crAnimationAsset = listCRAnimationAsset[i];

        string oldAssetPath;
        TextAsset textAsset = ConvertCRAnimationAssetToTextAsset( crAnimationAsset, out oldAssetPath );
        
        listTextAsset.Add(textAsset);
        listCRAnimationAsset.RemoveAt(i);
        AssetDatabase.DeleteAsset(oldAssetPath);
      }

      listTextAsset.Reverse();

      AssetDatabase.Refresh();
      AssetDatabase.SaveAssets();
        
      EditorUtility.SetDirty(ac_);
    }

    private TextAsset ConvertCRAnimationAssetToTextAsset(CRAnimationAsset crAnimationAsset, out string oldAssetPath)
    {
      oldAssetPath = AssetDatabase.GetAssetPath(crAnimationAsset.GetInstanceID());
      int index = oldAssetPath.IndexOf(crAnimationAsset.name + ".asset");
      string assetDirectiory = oldAssetPath.Substring(0, index);

      string cacheFilePath = AssetDatabase.GenerateUniqueAssetPath(assetDirectiory + crAnimationAsset.name + ".bytes");

      FileStream fs = new FileStream(cacheFilePath, FileMode.Create);
      byte[] arrByte = crAnimationAsset.Bytes;
      fs.Write(arrByte, 0, arrByte.Length);
      fs.Close();

      AssetDatabase.Refresh();
      AssetDatabase.SaveAssets();

      TextAsset crAnimationText = (TextAsset)AssetDatabase.LoadAssetAtPath( cacheFilePath, typeof(TextAsset) );
      return crAnimationText;
    }

    private void ConvertTextAssetsToCRAnimationAssets()
    {
      TextAsset textAnimation = ac_.activeAnimationText;

      if (textAnimation != null)
      {
        string oldAssetPath;
        CRAnimationAsset crAnimationAsset = ConvertTextAssetToCRAnimationAsset(textAnimation, out oldAssetPath);

        ac_.activeAnimationText = null;
        ac_.RemoveAnimation(textAnimation);
        AssetDatabase.DeleteAsset(oldAssetPath);

        ac_.AddAnimationAndSetActive(crAnimationAsset);
      }

      List<CRAnimationAsset> listCRAnimationAsset = ac_.listAnimations;
      List<TextAsset>        listTextAsset        = ac_.listAnimationsText;

      int lastAnimationAssets = listTextAsset.Count - 1;

      for (int i = lastAnimationAssets; i >= 0; i--)
      {
        TextAsset textAsset = listTextAsset[i];

        string oldAssetPath;
        CRAnimationAsset crAnimationAsset = ConvertTextAssetToCRAnimationAsset( textAsset, out oldAssetPath );
        
        listTextAsset.RemoveAt(i);
        listCRAnimationAsset.Add(crAnimationAsset);

        AssetDatabase.DeleteAsset(oldAssetPath);
      }

      listCRAnimationAsset.Reverse();

      AssetDatabase.Refresh();
      AssetDatabase.SaveAssets();
      
      EditorUtility.SetDirty(ac_);
    }

    private CRAnimationAsset ConvertTextAssetToCRAnimationAsset(TextAsset textAsset, out string oldAssetPath)
    {
      oldAssetPath = AssetDatabase.GetAssetPath(textAsset.GetInstanceID());
      int index = oldAssetPath.IndexOf(textAsset.name + ".bytes");
      string assetDirectiory = oldAssetPath.Substring(0, index);

      string crAnimationFilePath = AssetDatabase.GenerateUniqueAssetPath(assetDirectiory + textAsset.name + ".asset");
      
      CRAnimationAsset crAnimationAsset = CRAnimationAsset.CreateInstance<CRAnimationAsset>();
      crAnimationAsset.Bytes = textAsset.bytes;
      AssetDatabase.CreateAsset( crAnimationAsset, crAnimationFilePath );

      return crAnimationAsset;
    }

    private void DrawAnimationFiles()
    {
      if (ac_.animationFileType == CRAnimation.AnimationFileType.CRAnimationAsset)
      {
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        var value = EditorGUILayout.ObjectField("Active animation", ac_.activeAnimation, typeof(CRAnimationAsset), false );
        if (EditorGUI.EndChangeCheck())
        {
          Undo.RecordObject(ac_, "Change active animation");
          ac_.activeAnimation = (CRAnimationAsset)value;
          EditorUtility.SetDirty( ac_ );
        }
        EditorGUILayout.EndHorizontal();

        CarEditorUtils.DrawInspectorList("Animation tracks", ac_.listAnimations, ac_, "Set Active", ChangeToAnimationTrack );
        
        EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
        if ( GUILayout.Button("Convert animations to TextAssets") )
        {
          bool ok = EditorUtility.DisplayDialog("CaronteFX - Convert animations to TextAssets", "Proceed to conversion?", "Yes", "No" );
          if (ok)
          {
            ConvertCRAnimationAssetsToTextAssets();
          }

        }
        EditorGUI.EndDisabledGroup();

      }
      else if (ac_.animationFileType == CRAnimation.AnimationFileType.TextAsset)
      {
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        var value = EditorGUILayout.ObjectField("Active animation", ac_.activeAnimationText, typeof(TextAsset), false );
        if (EditorGUI.EndChangeCheck())
        {
          Undo.RecordObject(ac_, "Change active animation");
          ac_.activeAnimationText = (TextAsset)value;
          EditorUtility.SetDirty( ac_ );
        }
        EditorGUILayout.EndHorizontal();

        CarEditorUtils.DrawInspectorList("Animation tracks", ac_.listAnimationsText, ac_, "Set Active", ChangeToAnimationTrack );
    
        if ( GUILayout.Button("Convert animations to CRAnimationAssets") )
        {
          bool ok = EditorUtility.DisplayDialog("CaronteFX - Convert animations to CRAnimationAssets", "Proceed to conversion?", "Yes", "No" );
          if (ok)
          {
            ConvertTextAssetsToCRAnimationAssets();
          }     
        }
      }    
    }

    private void ChangeToAnimationTrack(int trackIdx)
    {
      ac_.ChangeToAnimationTrack(trackIdx);
      EditorUtility.SetDirty(ac_);
    }

    private void DrawSpeed()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField("Speed", ac_.speed);
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(ac_, "Change animation file type");
        ac_.speed = value;
        EditorUtility.SetDirty( ac_ );
      }
    }

    private void DrawRepeatMode()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.EnumPopup("Repeat Mode", ac_.repeatMode);
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(ac_, "Change animation file type");
        ac_.repeatMode = (CRAnimation.RepeatMode)value;
        EditorUtility.SetDirty( ac_ );
      }
    }

    private void DrawIsPreviewInEditor()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Toggle("Preview in editor", ac_.PreviewInEditor);
      if ( EditorGUI.EndChangeCheck() )
      {
        ac_.PreviewInEditor = value;
        SetAnimationPreviewMode();
      }
  }

    private void DrawSyncWithAnimator()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.ObjectField(new GUIContent("Sync with animator"), ac_.AnimatorSync, typeof(Animator), true);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(ac_, "Change sync with animator");
        ac_.AnimatorSync = (Animator)value;
        EditorUtility.SetDirty(ac_);
      }
    }

    private void DrawStartTimeOffset()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField(new GUIContent("Start time offset"), ac_.StartTimeOffset );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(ac_, "Change start time offset");
        ac_.StartTimeOffset = value;
        EditorUtility.SetDirty(ac_);
      }
    }

    private void DrawGPUVertexAnimation()
    {
      EditorGUILayout.PropertyField(gpuVertexAnimationProp_, new GUIContent("GPU vertex animation", "When this option is enabled vertex animations will be procesed in the GPU, this option requieres DX11 hardware level and a Vertex Animation Material."));
    }

    private void DrawGPUSkinnedAnimation()
    {
      EditorGUILayout.PropertyField(gpuSkinnedAnimationProp_, new GUIContent("GPU skinned animation", "When this option is enabled bone animations will be procesed in the GPU, this option requieres DX11 hardware level, an optimized hierarchy of bones, and a Vertex Animation Material."));
    }

    private void DrawBufferAllFrames()
    {
      EditorGUILayout.PropertyField(bufferAllFramesProp_, new GUIContent("Buffer all frames"));
    }

    private void DrawGPUBufferSize()
    {
      EditorGUILayout.PropertyField(gpuFrameBufferSizeProp_, new GUIContent("GPU buffer size","Number of frames that will be prebuffered to GPU for Vertex Animations") );
      gpuFrameBufferSizeProp_.intValue = Mathf.Clamp(gpuFrameBufferSizeProp_.intValue , 1, int.MaxValue);
    }

    private void DrawOverrideShaderForGPU()
    {
      EditorGUILayout.PropertyField(overrideShaderForGPUProp_, new GUIContent("Override shader", "When this option is enabled vertex animated or skinned animated objects will use the default CaronteFX VA shader, if it's disabled you must provide a material which support vertex animation when using GPU mode") );
    }

    private void DrawUseDoubleSidedShader()
    {
      EditorGUI.BeginDisabledGroup(!overrideShaderForGPUProp_.boolValue);
      EditorGUILayout.PropertyField(useDoubleSidedShaderProp_, new GUIContent("Use double sided shader", "When this option is enabled a double sided version of the standard material will be use for vertex animation.") );
      EditorGUI.EndDisabledGroup();
    }

    private void DrawCPUSlowMode()
    {
      if (Event.current.type == EventType.Layout)
      {
        slowMode_ = ac_.IsFiberCompression() && !ac_.GPUVertexAnimation;
      }
    
      if (slowMode_)
      {
        EditorGUILayout.HelpBox("Fiber decompression in CPU is very slow. It's strongly recommended to use GPU vertex animation mode.", MessageType.Warning);
      }
    }

    private void DrawRecomputeNormals(bool isPlayingOrWillChangePlaymode)
    {
      EditorGUILayout.PropertyField(recomputeNormalsProp_, new GUIContent("Recompute normals", "When this option is enabled, if the animation was baked with the option 'Save vertex systems', normals will be recomputed in each frame. \n\nThis option is very slow on CPU, use only in GPU mode.") );
    }

    private string GetCompressionTypeString()
    {
      if (ac_.IsBoxCompression())
      {
        return "Box Compression";
      }
      else if (ac_.IsFiberCompression())
      {
        return "Fiber Compression";
      }
      else
      {
        return "None";
      }
    }

    private void SetAnimationPreviewMode()
    {
      if (!EditorApplication.isPlayingOrWillChangePlaymode)
      {
        if (ac_.PreviewInEditor && !ac_.IsPreviewing)
        {
          ac_.LoadActiveAnimation(true);
          ac_.SetFrame(editorFrame_);
          EditorUtility.SetDirty(ac_);

          lastPreviewTime_ = EditorApplication.timeSinceStartup;
          EditorApplication.update -= UpdatePreview;
          EditorApplication.update += UpdatePreview;

          ac_.IsPreviewing = true;
        }
        else if ( !ac_.PreviewInEditor)
        {
          ClosePreviewMode();
        }
      }
    }

    private void ClosePreviewMode()
    {
      EditorApplication.update -= UpdatePreview;

      if (ac_ != null)
      {      
        ac_.PreviewInEditor = false;
        ac_.IsPreviewing = false;
        ac_.SetFrame(0.0f);
        ac_.CloseActiveAnimation();
        EditorUtility.SetDirty(ac_);
      }
    }

    private void UpdatePreview()
    {
      if (ac_ == null)
      {
        ClosePreviewMode();
        return;
      }

      if (!EditorApplication.isPlayingOrWillChangePlaymode)
      {
        if (ac_.PreviewInEditor && ac_.IsPreviewing && ac_.isActiveAndEnabled)
        {
          double currentTime = EditorApplication.timeSinceStartup;
          float deltaTime = (float)(currentTime - lastPreviewTime_);

          lastPreviewTime_ = currentTime;
          ac_.Update(deltaTime);

          Repaint();

        }
        else if (!ac_.PreviewInEditor)
        {
          ClosePreviewMode();
        }
      }
      else
      {
        ClosePreviewMode();       
      }   
    }

    private void DrawRebakeAnimation()
    {
      EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);

      bool openRebakeWindow = true;
      if ( GUILayout.Button("Rebake active animation...") )
      {
        CarAnimationPersistence animationPersistence = ac_.AnimationPersistence;
        if (animationPersistence != null && animationPersistence.HasOptimizedGameObjects())
        {
          EditorUtility.DisplayDialog("CaronteFX - Animation rebake", "CRAnimations with optimized hierarchy of bones cannot be rebaked. In order to rebake, first unoptimize the hierarchy.", "Ok");
          openRebakeWindow = false;
        }

        if (openRebakeWindow)
        {
          ac_.PreviewInEditor = false;
          CarRebakeAnimationWindow.ShowWindow(ac_);
        }
      }

      EditorGUI.EndDisabledGroup();
    }

    private void DrawAddDefaultParticleSystem()
    {
      EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
      if ( GUILayout.Button("Add default particle system...") )
      {
        ac_.PreviewInEditor = false;
        SetAnimationPreviewMode();
        CarAddDefaultParticleSystemWindow.ShowWindow(ac_);     
      }
      EditorGUI.EndDisabledGroup();
    }

    private void DrawExportToFbx()
    {
      EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
      if ( GUILayout.Button("Export to FBX...") )
      {
        ac_.PreviewInEditor = false;
        SetAnimationPreviewMode();
        CarFbxExporterWindow.ShowWindow(ac_);     
      }
      EditorGUI.EndDisabledGroup();
    }

    private void DrawRecordScreenshots()
    {
      EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
      if ( GUILayout.Button("Make screenshots...") )
      {
        ac_.PreviewInEditor = false;
        SetAnimationPreviewMode();
        CarMakeScreenshotsWindow.ShowWindow(ac_);     
      }

      EditorGUI.EndDisabledGroup();
    }

    private void DrawOptimizeTransformHierarchy()
    {
      if ( GUILayout.Button("Optimize hierarchy of bones") )
      {

        bool doOptimize = true;
        if (ac_.AnimationPersistence == null )
        {
           doOptimize = EditorUtility.DisplayDialog("CaronteFX - Optimize hierarchy of bones", 
                        "This bake was created with an old version of CaronteFX.\n\nOptimizing it may make it unusable. "+
                        "Please, make a back up of the GameObject first.\n\n" +
                        "It's strongly recommended to bake the simulation again with this version of CaronteFX " +
                        "if you want to be able to optimize it properly.", "Ok, proceed anyway", "Cancel");
        }
        if (doOptimize)
        {
         CarAnimationUtils.OptimizeTransformHierarchy(ac_);
        }       
      }
    }

    private void DrawUnoptimizeTransformHierarchy()
    {
      if ( GUILayout.Button("Unoptimize hierarchy of bones") )
      {
        CarAnimationUtils.UnoptimizeTransformHierarchy(ac_);
      }
    }

  }
}

