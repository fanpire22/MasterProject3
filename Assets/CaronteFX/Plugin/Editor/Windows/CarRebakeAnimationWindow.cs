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
using CaronteFX.AnimationFlags;
using CaronteSharp;


namespace CaronteFX
{
  using TEventsDictionary = Tuple2<Dictionary<string, int>, Dictionary<GameObject, int>>;

  public class CarRebakeAnimationWindow : CarWindow<CarRebakeAnimationWindow>
  {
    CRAnimation crAnimation_;
    CarAnimationRebaker animationRebaker_;

    int trimFrameStart_ = 0;
    int trimFrameEnd_   = 0;
    int trimFrameLast_  = 0;

    bool createLoopableBake_ = false;
    int  transitionFrames_ = 30;

    bool vertexCompression_  = true;
    bool saveTangents_       = false;
    bool alignData_          = true;
    bool bakeEvents_         = true;

    private int vertexCompressionIdx_ = 0;
    private readonly string[] arrVertexCompressionModes_ = new string[2] { "Box (medium compression)", "Fiber (high compression)" };

    private List<CRAnimationEvData> listAnimationEventDataTmp_ = new List<CRAnimationEvData>();

    const long maxAnimationBytesSize  = (long)(int.MaxValue - (1024*1024)); //2GB Aprox. - Max Animation size.

    public static CarRebakeAnimationWindow ShowWindow(CRAnimation crAnimation)
    {
      if (Instance == null)
      {
        Instance = (CarRebakeAnimationWindow)EditorWindow.GetWindow(typeof(CarRebakeAnimationWindow), true, "CaronteFX - Rebake active animation");
        Instance.crAnimation_ = crAnimation;
      }

      float width  = 320f;
      float height = 345f;

      Instance.minSize = new Vector2(width, height);
      Instance.maxSize = new Vector2(width, height);

      crAnimation.LoadActiveAnimation(true);

      Instance.trimFrameStart_ = 0;
      Instance.trimFrameEnd_   = crAnimation.LastFrame;
      Instance.trimFrameLast_  = crAnimation.LastFrame;

      Instance.animationRebaker_ = new CarAnimationRebaker(crAnimation, Instance.BakeEventsHeaderData, Instance.BakeEventsFrameData, Instance.CreateAsset);

      Instance.Focus();
      return Instance;
    }


    void OnLostFocus()
    {
      Close();
    }

    private void SetStartEndFrames(int startFrame, int endFrame)
    {
      trimFrameStart_ = Mathf.Clamp( startFrame, 0, trimFrameLast_);
      trimFrameEnd_   = Mathf.Clamp( endFrame, 0, trimFrameLast_);
    }

    public void DrawRebake()
    {
      EditorGUI.BeginDisabledGroup(true);
      if (crAnimation_.animationFileType == CRAnimation.AnimationFileType.CRAnimationAsset)
      {
        EditorGUILayout.ObjectField("Active animation", crAnimation_.activeAnimation, typeof(CRAnimationAsset), true);
      }
      else if ( crAnimation_.animationFileType == CRAnimation.AnimationFileType.TextAsset)
      {
        EditorGUILayout.ObjectField("Active animation", crAnimation_.activeAnimationText, typeof(TextAsset), true);
      }
      EditorGUI.EndDisabledGroup();

      EditorGUILayout.Space();

      float start = EditorGUILayout.IntField("Frame Start : ", trimFrameStart_ );
      float end   = EditorGUILayout.IntField("Frame End   : ", trimFrameEnd_ );

      EditorGUILayout.MinMaxSlider( new GUIContent("Frames:"), ref start, ref end, 0, trimFrameLast_ );
      SetStartEndFrames( (int)start, (int)end );

      EditorGUILayout.Space();

      EditorGUI.BeginDisabledGroup(true);      
      int actualFrameStart = createLoopableBake_ ? trimFrameStart_ + transitionFrames_ : trimFrameStart_;
      EditorGUILayout.IntField("Actual frame start", actualFrameStart);
      EditorGUILayout.IntField("Actual frame end",   trimFrameEnd_);
      EditorGUI.EndDisabledGroup();

      EditorGUI.BeginDisabledGroup(true);      
      int actualFrameCount = createLoopableBake_ ? trimFrameEnd_ - trimFrameStart_ - transitionFrames_ : trimFrameEnd_ - trimFrameStart_ ;
      EditorGUILayout.IntField("Actual frame count", actualFrameCount);
      EditorGUI.EndDisabledGroup();

      EditorGUI.BeginDisabledGroup(true);      
      EditorGUILayout.FloatField("Actual time", actualFrameCount * crAnimation_.FrameTime);
      EditorGUI.EndDisabledGroup();

      EditorGUILayout.Space();

      createLoopableBake_ = EditorGUILayout.Toggle("Create loop", createLoopableBake_);

      EditorGUI.BeginDisabledGroup(!createLoopableBake_);
      int frameCount = trimFrameEnd_ - trimFrameStart_ + 1;
      int maxLoopFrames = frameCount / 2;
      transitionFrames_ = Mathf.Clamp(Mathf.Min(EditorGUILayout.IntField("Loop frames", transitionFrames_), maxLoopFrames), 0, int.MaxValue);  
      EditorGUI.EndDisabledGroup();

      EditorGUILayout.Space();
      vertexCompression_ = EditorGUILayout.Toggle("Vertex compression", vertexCompression_);

      EditorGUI.BeginDisabledGroup(!vertexCompression_);
      vertexCompressionIdx_ = EditorGUILayout.Popup( "Compression mode", vertexCompressionIdx_, arrVertexCompressionModes_);
      EditorGUI.EndDisabledGroup();

      bool isFiberCompression = vertexCompression_ && vertexCompressionIdx_ == 1;

      EditorGUI.BeginDisabledGroup(isFiberCompression);
      saveTangents_ = EditorGUILayout.Toggle("Save tangents", saveTangents_);
      EditorGUI.EndDisabledGroup();

      alignData_ = EditorGUILayout.Toggle("Align data", alignData_);

      EditorGUI.BeginDisabledGroup(createLoopableBake_);
      bakeEvents_ = EditorGUILayout.Toggle("Bake events", bakeEvents_);
      EditorGUI.EndDisabledGroup();

      GUILayout.FlexibleSpace();
      CarGUIUtils.Splitter();

      if ( GUILayout.Button("Rebake animation") )
      {
        if (trimFrameEnd_ <= trimFrameStart_)
        {
          EditorUtility.DisplayDialog("CaronteFX - Invalid frames", "Frame End must be above Frame Start", "Ok");
          return;
        }

        bool isTextAsset = crAnimation_.animationFileType == CRAnimation.AnimationFileType.TextAsset;
        RebakeClip(isTextAsset);
      }
    }

    public void OnGUI()
    {
      EditorGUILayout.Space();
      DrawRebake();
      EditorGUILayout.Space();
    }

    private void RebakeClip(bool isTextAsset)
    {
      animationRebaker_.SetRebakeOptions(trimFrameStart_, trimFrameEnd_, createLoopableBake_, transitionFrames_, vertexCompression_, vertexCompressionIdx_, saveTangents_, alignData_, bakeEvents_);
      animationRebaker_.RebakeClip(isTextAsset);
    }

    private void BakeEventsHeaderData(BinaryWriter bw, Dictionary<string, int> dictionaryEmitterNameIdInBake)
    {
      List<string> listEventEmitterName = crAnimation_.GetEventEmitterNames();
      bw.Write( listEventEmitterName.Count );

      int emitterId = 0;
      foreach(string eventEmitterName in listEventEmitterName)
      {
        bw.Write( eventEmitterName );
        dictionaryEmitterNameIdInBake.Add( eventEmitterName, emitterId++ );
      }
    }

    private void BakeEventsFrameData(int frame, CarMemoryStream msW, BinaryWriter bw, TEventsDictionary eventsDictionaries)
    {
      listAnimationEventDataTmp_.Clear();
      crAnimation_.GetAnimationEventsData(frame, listAnimationEventDataTmp_);
      WriteAnimationEvents(listAnimationEventDataTmp_, msW, bw, eventsDictionaries.First, eventsDictionaries.Second);
    }

    private void WriteAnimationEvents(List<CRAnimationEvData> listAnimationEventData, CarMemoryStream ms, BinaryWriter bw, 
                                      Dictionary<string, int> dictionaryEmitterNameIdInBake, Dictionary<GameObject, int> dictionaryGameObjectIdInBake )
    {

      bw.Write(listAnimationEventData.Count);

      foreach( CRAnimationEvData evData in listAnimationEventData)
      {
        int idEmitterInBkae = dictionaryEmitterNameIdInBake[evData.emitterName_];
        bw.Write(idEmitterInBkae);

        if (evData.type_ == CRAnimationEvData.EEventDataType.Contact)
        {
          bw.Write( (int)CRAnimationEvData.EEventDataType.Contact );
          CRContactEvData ceData = (CRContactEvData)evData;

          int idGameObjectInFileA = dictionaryGameObjectIdInBake[ceData.GameObjectA];
          int idGameObjectInFileB = dictionaryGameObjectIdInBake[ceData.GameObjectB];

          bw.Write(idGameObjectInFileA);
          bw.Write(idGameObjectInFileB);

          bw.Write(ceData.position_.x);
          bw.Write(ceData.position_.y);
          bw.Write(ceData.position_.z);

          bw.Write(ceData.velocityA_.x);
          bw.Write(ceData.velocityA_.y);
          bw.Write(ceData.velocityA_.z);

          bw.Write(ceData.velocityB_.x);
          bw.Write(ceData.velocityB_.y);
          bw.Write(ceData.velocityB_.z);

          bw.Write(ceData.relativeSpeed_N_);
          bw.Write(ceData.relativeSpeed_T_);

          bw.Write(ceData.relativeP_N_);
          bw.Write(ceData.relativeP_T_);
        }
      }
    }

    private void CreateAsset( CarMemoryStream ms, bool isTextAsset )
    {
      AssetDatabase.Refresh();

      if (ms.Length > maxAnimationBytesSize)
      {
        EditorUtility.DisplayDialog("CaronteFX - Animation is too big", "Animation exceeds the maximum size (2GB), consider splitting the animation into parts", "Ok");
        return;
      }

      if (!isTextAsset)
      {
        string ext = "asset";
        CRAnimationAsset animationAsset = CRAnimationAsset.CreateInstance<CRAnimationAsset>();
        animationAsset.Bytes = ms.ToArray();

        CRAnimationAsset animationAssetOld = crAnimation_.activeAnimation;

        string path = AssetDatabase.GetAssetPath(animationAssetOld);
    
        string name = animationAssetOld.name;
        int index = path.IndexOf(name + "." + ext);
        string newFolder = path.Substring(0, index);

        string newPath = EditorUtility.SaveFilePanelInProject("CaronteFX - Rebake animation", name + "_rebake", ext, "Select destination file name...", newFolder);
        if (newPath != string.Empty)
        {
          AssetDatabase.CreateAsset( animationAsset, newPath );

          AssetDatabase.SaveAssets();
          AssetDatabase.Refresh(); 

          crAnimation_.AddAnimation(animationAsset);
          EditorGUIUtility.PingObject(animationAsset);
        }
      }
      else
      {
        string ext = "bytes";
        TextAsset textAsset = crAnimation_.activeAnimationText;

        string path = AssetDatabase.GetAssetPath(textAsset);
    
        string name = textAsset.name;
        int index = path.IndexOf(name + "." + ext);
        string newFolder = path.Substring(0, index);

        string newPath = EditorUtility.SaveFilePanelInProject("CaronteFX - Rebake animation", name + "_rebake", ext, "Select destination file name...", newFolder);
        if (newPath != string.Empty)
        {
          TextAsset existingAsset = (TextAsset)AssetDatabase.LoadAssetAtPath(newPath, typeof(TextAsset));
          if (existingAsset != null)
          {
            CRAnimationsManager instance = CRAnimationsManager.Instance;
            instance.RemoveCachedAnim(existingAsset);

            AssetDatabase.DeleteAsset(newPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
          }

          FileStream fs = new FileStream(newPath, FileMode.Create);
          ms.WriteTo(fs);
          fs.Close();

          AssetDatabase.SaveAssets();
          AssetDatabase.Refresh(); 

          TextAsset crAnimationText = (TextAsset)AssetDatabase.LoadAssetAtPath(newPath, typeof(TextAsset));
          crAnimation_.AddAnimation(crAnimationText);
          EditorGUIUtility.PingObject(crAnimationText);
        }
      }

      EditorUtility.SetDirty(crAnimation_);
    }
  }
}