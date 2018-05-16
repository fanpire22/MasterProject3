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
  public class CarBakeFrameMenu : CarWindow<CarBakeFrameMenu>
  {
    CarAnimationBaker simulationBaker_;

    float  width  = 350f;
    float  height = 460f;

    Vector2 scroller_;

    List<CNBody> listBodyNode_;
    BitArray bitArrNeedsBaking_;

    CarManager Controller { get; set; }

    int StartFrame
    {
      get
      {
        return simulationBaker_.FrameStart;
      }
    }
   
    int EndFrame
    {
      get
      {
        return simulationBaker_.FrameEnd;
      }
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

      Controller = CarManager.Instance;
      Controller.Player.pause();

      simulationBaker_ = Controller.SimulationBaker;

      listBodyNode_      = simulationBaker_.ListBodyNode;
      bitArrNeedsBaking_ = simulationBaker_.BitArrNeedsBaking;
    }

    void SetStartEndFrames(int startFrame, int endFrame)
    {
      simulationBaker_.FrameStart = Mathf.Min(startFrame, endFrame - 1 );
      simulationBaker_.FrameEnd   = Mathf.Max(endFrame,   startFrame + 1 );
    }

    void OnGUI()
    {
      Rect nodesArea    = new Rect( 5, 5, width - 10, ( (height - 10) * 0.70f ) ); 
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
      GUILayout.Space( 65f );
      EditorGUILayout.EndHorizontal();
      GUILayout.FlexibleSpace();

      GUILayout.EndHorizontal();
      
      Rect boxRect = new Rect( nodesAreaBox.xMin - 5f, rect.yMax, nodesAreaBox.width, (nodesAreaBox.yMax - rect.yMax) + 1f );

      GUI.Box(boxRect, "");

      scroller_ = GUILayout.BeginScrollView(scroller_);
      
      for (int i = 0; i < bodyNodeCount; i++)
      {
        CNBody bodyNode = listBodyNode_[i];
        string name = bodyNode.Name;
        bitArrNeedsBaking_[i] = EditorGUILayout.ToggleLeft(name, bitArrNeedsBaking_[i], GUILayout.Width(250f) );    
      }

      EditorGUILayout.EndScrollView();

      GUILayout.EndArea();
      GUILayout.FlexibleSpace();

      Rect optionsArea = new Rect( 5f, nodesArea.yMax + 15f, width - 10f, ( (height - 10f) * 0.30f ) );
      GUILayout.BeginArea(optionsArea);
      
      EditorGUILayout.Space();
      EditorGUILayout.Space();

      GUILayout.BeginHorizontal();
      simulationBaker_.combineMeshesInFrame_ = EditorGUILayout.Toggle("Combine meshes", simulationBaker_.combineMeshesInFrame_ );
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      simulationBaker_.BakeObjectName = EditorGUILayout.TextField("Bake object name", simulationBaker_.BakeObjectName );    
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      simulationBaker_.BakeObjectPrefix = EditorGUILayout.TextField("Bake object prefix", simulationBaker_.BakeObjectPrefix );    
      GUILayout.EndHorizontal();

      EditorGUI.BeginDisabledGroup(simulationBaker_.combineMeshesInFrame_);
      GUILayout.BeginHorizontal();
      simulationBaker_.PreserveCFXComponentsInFrameBake = EditorGUILayout.Toggle("Keep body components", simulationBaker_.PreserveCFXComponentsInFrameBake);
      GUILayout.EndHorizontal();
      EditorGUI.EndDisabledGroup();

      EditorGUILayout.Space();
      EditorGUILayout.Space();
      if (GUILayout.Button("Bake!"))
      {
        EditorApplication.delayCall += () => 
        { 
          simulationBaker_.BakeCurrentFrame(); 
          Close();
        };
      }


      GUILayout.EndArea();
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

