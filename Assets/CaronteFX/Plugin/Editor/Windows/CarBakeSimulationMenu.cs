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
using System.Collections.Generic;

namespace CaronteFX
{
  public class CarBakeSimulationMenu : CarWindow<CarBakeSimulationMenu>
  {
    CarAnimationBaker simulationBaker_;

    float width  = 400f;
    float height = 610f;

    List<CNBody> listBodyNode_;
    Vector2 scroller_;

    BitArray bitArrNeedsBaking_;
    BitArray bitArrNeedsCollapsing_;

    private string[] arrVertexCompressionModes_;
    private readonly string[] arrCreationModes_ = new string[] { "New bake GameObject", "Bake into existing GameObject" };

    CarManager Controller { get; set; }

    int StartFrame
    {
      get { return simulationBaker_.FrameStart;  }
    }
   
    int EndFrame
    {
      get { return simulationBaker_.FrameEnd; }
    }

    int MaxFrames
    {
      get { return simulationBaker_.MaxFrames; }
    }

    void OnEnable()
    {
      Instance = this;

      this.minSize = new Vector2(width, height);
      this.maxSize = new Vector2(width, height);

      arrVertexCompressionModes_ = new string[2] { "Box (medium compression)", "Fiber (high compression)" };

      Controller = CarManager.Instance;
      Controller.Player.pause();

      simulationBaker_ = Controller.SimulationBaker;

      listBodyNode_          = simulationBaker_.ListBodyNode;

      bitArrNeedsBaking_     = simulationBaker_.BitArrNeedsBaking;
      bitArrNeedsCollapsing_ = simulationBaker_.BitArrNeedsCollapsing;
    }

    void SetStartEndFrames(int startFrame, int endFrame)
    {
      simulationBaker_.FrameStart = Mathf.Clamp(startFrame, 0, MaxFrames);
      simulationBaker_.FrameEnd   = Mathf.Clamp(endFrame,  0, MaxFrames);
    }

    void OnGUI()
    {
      Rect nodesArea    = new Rect( 5, 5, width - 10, Mathf.CeilToInt( (height - 10f) / 3 ) ); 
      Rect nodesAreaBox = new Rect( nodesArea.xMin, nodesArea.yMin, nodesArea.width + 1, nodesArea.height + 1 );
      GUI.Box(nodesAreaBox, "");

      GUILayout.BeginArea(nodesArea);
      GUILayout.BeginHorizontal();

      GUIStyle styleTitle = new GUIStyle(EditorStyles.label);
      styleTitle.fontStyle = FontStyle.Bold;

      GUILayout.Label( "Nodes to bake:", styleTitle);
      GUILayout.EndHorizontal();

      CarGUIUtils.DrawSeparator();

      GUILayout.BeginHorizontal();

      int bodyNodeCount = listBodyNode_.Count;
      EditorGUILayout.BeginHorizontal();
      DrawToggleMixed( bodyNodeCount );
      Rect rect = GUILayoutUtility.GetLastRect();
      GUILayout.Space( 90f );
      EditorGUILayout.LabelField("Collapse/Skin");
      EditorGUILayout.EndHorizontal();
      GUILayout.FlexibleSpace();

      GUILayout.EndHorizontal();
      
      Rect boxRect      = new Rect( nodesAreaBox.xMin - 5, rect.yMax, nodesAreaBox.width - 90f, nodesAreaBox.yMax - rect.yMax );
      Rect collapseRect = new Rect( boxRect.xMax, boxRect.yMin, 90f, boxRect.height );

      GUI.Box(boxRect, "");
      GUI.Box(collapseRect, "");

      scroller_ = GUILayout.BeginScrollView(scroller_);
      
      for (int i = 0; i < bodyNodeCount; i++)
      {
        GUILayout.BeginHorizontal();
        CNBody bodyNode = listBodyNode_[i];
        string name = bodyNode.Name;

        bitArrNeedsBaking_[i] = EditorGUILayout.ToggleLeft(name, bitArrNeedsBaking_[i], GUILayout.Width(250f) );
        GUILayout.Space(85f);

        bool isRigid    = bodyNode is CNRigidbody;
        bool isAnimated = bodyNode is CNAnimatedbody;

        if (isRigid && !isAnimated)
        {
          bitArrNeedsCollapsing_[i] = EditorGUILayout.Toggle(bitArrNeedsCollapsing_[i]);
        }
        
        GUILayout.EndHorizontal();
      }

      EditorGUILayout.EndScrollView();

      GUILayout.EndArea();
      GUILayout.Space(boxRect.height);
      //GUILayout.FlexibleSpace();
      
      EditorGUILayout.Space();
      EditorGUILayout.Space();

      float start = EditorGUILayout.IntField("Frame Start : ", StartFrame );
      float end   = EditorGUILayout.IntField("Frame End   : ", EndFrame );

      EditorGUILayout.MinMaxSlider( new GUIContent("Frames:"), ref start, ref end, 0, MaxFrames );

      SetStartEndFrames( (int)start, (int)end );

      EditorGUILayout.Space();

      GUILayout.BeginHorizontal();
      simulationBaker_.BakeAnimationFileType = (CarAnimationBaker.EAnimationFileType)EditorGUILayout.EnumPopup("File type", simulationBaker_.BakeAnimationFileType);
      GUILayout.EndHorizontal();

      EditorGUILayout.Space();
  
      GUILayout.BeginHorizontal();
      simulationBaker_.BakeMode = (CarAnimationBaker.EBakeMode) EditorGUILayout.Popup( "Bake mode", (int)simulationBaker_.BakeMode, arrCreationModes_);
      GUILayout.EndHorizontal();

      if (simulationBaker_.BakeMode == CarAnimationBaker.EBakeMode.BakeToNew)
      {
        GUILayout.BeginHorizontal();
        simulationBaker_.BakeObjectName = EditorGUILayout.TextField("Bake object name", simulationBaker_.BakeObjectName );
        GUILayout.EndHorizontal();

        /*
        GUILayout.BeginHorizontal();
        simulationBaker_.OptionalRootTransform = (Transform) EditorGUILayout.ObjectField("Root transform to copy", simulationBaker_.OptionalRootTransform, typeof(Transform), true );
        GUILayout.EndHorizontal();
        */
      }
      else if (simulationBaker_.BakeMode == CarAnimationBaker.EBakeMode.BakeToExisting)
      {
        EditorGUILayout.HelpBox("Bake into existing GameObject requires baking the same bodies as the existing bake GameObject. Please, make sure you are baking the same bodies, otherwise use the new bake GameObject mode.", MessageType.Info);

        GUILayout.BeginHorizontal();
        simulationBaker_.ExistingBake = (CRAnimation)EditorGUILayout.ObjectField("Bake object", simulationBaker_.ExistingBake, typeof(CRAnimation), true );
        GUILayout.EndHorizontal();
      }

      GUILayout.BeginHorizontal();
      simulationBaker_.BakeObjectPrefix = EditorGUILayout.TextField("Bake objects prefix", simulationBaker_.BakeObjectPrefix );
      GUILayout.EndHorizontal();

      EditorGUILayout.Space();

      GUILayout.BeginHorizontal();
      simulationBaker_.RemoveInvisibleBodiesFromBake = EditorGUILayout.Toggle("Remove invisible bodies", simulationBaker_.RemoveInvisibleBodiesFromBake);
      GUILayout.EndHorizontal();

      EditorGUILayout.Space();

      GUILayout.BeginHorizontal();
      simulationBaker_.SkinRopes = EditorGUILayout.Toggle("Skin ropes", simulationBaker_.SkinRopes);
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      simulationBaker_.SkinClothes = EditorGUILayout.Toggle("Skin clothes", simulationBaker_.SkinClothes);
      GUILayout.EndHorizontal();

      EditorGUILayout.Space();

      GUILayout.BeginHorizontal();
      simulationBaker_.VertexCompression = EditorGUILayout.Toggle("Vertex compression", simulationBaker_.VertexCompression);
      GUILayout.EndHorizontal();

      EditorGUI.BeginDisabledGroup(!simulationBaker_.VertexCompression);
      simulationBaker_.VertexCompressionMode = (CarAnimationBaker.EVertexCompressionMode)EditorGUILayout.Popup( "Compression mode", (int)simulationBaker_.VertexCompressionMode, arrVertexCompressionModes_);
      EditorGUI.EndDisabledGroup();

      bool isFiberCompression = simulationBaker_.VertexCompression && (simulationBaker_.VertexCompressionMode == (CarAnimationBaker.EVertexCompressionMode.Fiber));
      EditorGUI.BeginDisabledGroup(isFiberCompression);
      GUILayout.BeginHorizontal();
      simulationBaker_.VertexTangents = EditorGUILayout.Toggle("Save tangents", simulationBaker_.VertexTangents);
      GUILayout.EndHorizontal();
      EditorGUI.EndDisabledGroup();

      GUILayout.BeginHorizontal();
      simulationBaker_.AlignData = EditorGUILayout.Toggle("Align data", simulationBaker_.AlignData);
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      simulationBaker_.BakeEvents = EditorGUILayout.Toggle("Save events", simulationBaker_.BakeEvents);
      GUILayout.EndHorizontal();

      //simulationBaker_.bakeVisibility_ = EditorGUILayout.Toggle("Save visibility", simulationBaker_.bakeVisibility_);
      
      EditorGUILayout.Space();

      GUILayout.FlexibleSpace();
      if (GUILayout.Button("Bake!", GUILayout.Height(20f)))
      {   
        if (simulationBaker_.FrameEnd <= simulationBaker_.FrameStart)
        {
          EditorUtility.DisplayDialog("CaronteFX - Bake Menu", "Frame End must be above Frame Start", "Ok");
          return;
        }

        if (simulationBaker_.BakeMode == CarAnimationBaker.EBakeMode.BakeToExisting)
        {
          if (simulationBaker_.ExistingBake == null)
          {
            EditorUtility.DisplayDialog("CaronteFX - Bake Menu", "When baking to existing GameObject, specifying a bake gameobject is mandatory", "Ok");
            return;
          }
          else
          {
            CarAnimationPersistence persistence = simulationBaker_.ExistingBake.AnimationPersistence;
            if (persistence != null && persistence.HasOptimizedGameObjects())
            {
              EditorUtility.DisplayDialog("CaronteFX - Bake Menu", "A CR Animation with an optimized hierarchy of bones cannot be baked into. In order to bake, first unoptimize its hierarchy of bones.", "Ok");
              return;
            } 
          }
        }

        EditorApplication.delayCall += () => 
        { 
          simulationBaker_.BakeSimulationAsAnim(); 
          Close();
        };
      }

      GUILayout.BeginHorizontal();
      GUILayout.EndHorizontal();
      GUILayout.Space(5f);
    }

   void DrawToggleMixed( int bodyNodeCount )
   {
     EditorGUI.BeginChangeCheck();
     if (bodyNodeCount > 0)
     {
       bool value = bitArrNeedsBaking_[0];
       for (int i = 1; i < bodyNodeCount; ++i)
       {
         if ( value != bitArrNeedsBaking_[i] )
         {
           EditorGUI.showMixedValue = true;
           break;
         }
       }
       simulationBaker_.BakeAllNodes = value;
     }

     simulationBaker_.BakeAllNodes = EditorGUILayout.ToggleLeft("All", simulationBaker_.BakeAllNodes);
     EditorGUI.showMixedValue = false;
     if (EditorGUI.EndChangeCheck())
     {
       for (int i = 0; i < bodyNodeCount; ++i)
       {
         bitArrNeedsBaking_[i] = simulationBaker_.BakeAllNodes;
       }
     }
     EditorGUI.showMixedValue = false;
   }
  }
}

