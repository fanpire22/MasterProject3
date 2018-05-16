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

namespace CaronteFX
{
  //-----------------------------------------------------------------------------------
  public class CNFluidEditorState : CNCorpusclesEditorState
  {
    public float wall_stiffness_;
    public float wall_damping_;
    public float cohesion_acceleration_;
    public float viscosity_damping_;
  }
  //-----------------------------------------------------------------------------------
  public class CNFluidEditor : CNCorpusclesEditor
  {
    protected new CNFluidEditorState state_;
    protected new CNFluid Data { get; set; }

    public CNFluidEditor( CNFluid data, CNFluidEditorState state )
      : base( data, state )
    {
      Data   = (CNFluid)data;
      state_ = state;
    }
    //-----------------------------------------------------------------------------------
    public override void Init()
    {
      base.Init();

      FieldController.SetCalculatesDiff(true);
      FieldController.SetFieldContentType(CNFieldContentType.Geometry);  
    }
    //-----------------------------------------------------------------------------------
    public override void CreateCorpuscles()
    {
      GameObject[] arrGameObject = FieldController.GetUnityGameObjects();

      eManager.CreateCorpuscles(Data, arrGameObject);
    }
    //-----------------------------------------------------------------------------------
    public override void DestroyCorpuscles()
    {
      eManager.DestroyCorpuscles();
    }
    //-----------------------------------------------------------------------------------
    protected void DrawWallStiffness()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( "Wall stiffness", Data.Wall_Stiffness );
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(Data, "Change wall stiffnesss - " + Data.Name);
        Data.Wall_Stiffness = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    protected void DrawWallDamping()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( "Wall damping", Data.Wall_Damping );
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(Data, "Change wall damping - " + Data.Name);
        Data.Wall_Damping = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    protected void DrawCohesionAcceleration()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( "Cohesion acceleration", Data.Cohesion_Acceleration );
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(Data, "Change cohesion acceleration - " + Data.Name);
        Data.Cohesion_Acceleration = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    protected void DrawViscosityDamping()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( "Viscosity damping", Data.Viscosity_Damping );
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(Data, "Change viscosity damping - " + Data.Name);
        Data.Viscosity_Damping = value;
        EditorUtility.SetDirty(Data);
      }
    }
    //-----------------------------------------------------------------------------------
    public override void RenderGUI ( Rect area, bool isEditable )
    {
      GUILayout.BeginArea( area );

      RenderTitle(isEditable);

      EditorGUI.BeginDisabledGroup(!isEditable);
      RenderFieldObjects( "Objects", FieldController, true, true, CNFieldWindow.Type.normal );
      EditorGUI.EndDisabledGroup();

      EditorGUILayout.Space();
      EditorGUILayout.Space();

      CarGUIUtils.DrawSeparator();
      CarGUIUtils.Splitter();

      float originalLabelWidth = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = label_width;

      scroller_ = EditorGUILayout.BeginScrollView(scroller_);
      EditorGUI.BeginDisabledGroup(!isEditable);

      EditorGUILayout.Space(); 
      EditorGUILayout.Space();     

      EditorGUI.BeginDisabledGroup(true);
      DrawCorpusclesRadius();
      EditorGUI.EndDisabledGroup();

      EditorGUILayout.Space(); 

      DrawWallStiffness();
      DrawWallDamping();
      DrawCohesionAcceleration();
      DrawViscosityDamping();

      EditorGUI.EndDisabledGroup();
      EditorGUILayout.EndScrollView();

      EditorGUIUtility.labelWidth = originalLabelWidth;

      GUILayout.EndArea();
    }
    //-----------------------------------------------------------------------------------
  }
}

