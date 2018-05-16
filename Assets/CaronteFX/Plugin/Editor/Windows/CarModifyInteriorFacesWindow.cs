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
  public class CarModifyInteriorFacesWindow : CarWindow<CarModifyInteriorFacesWindow>
  {
    CNFracture fracturerData_;
    CNFractureEditor fracturerEditor_;

    private static readonly GUIContent frInteriorFacesTilingCt_          = new GUIContent(CarStringManager.GetString("FrInteriorFacesTiling"), CarStringManager.GetString("FrInteriorFacesTilingTooltip"));
    private static readonly GUIContent frInteriorFacesOffsetCt_          = new GUIContent(CarStringManager.GetString("FrInteriorFacesOffset"), CarStringManager.GetString("FrInteriorFacesOffsetTooltip"));

    public static CarModifyInteriorFacesWindow ShowWindow(CNFracture fracturerData, CNFractureEditor fracturerEditor)
    {
      if (Instance == null)
      {
        Instance = (CarModifyInteriorFacesWindow)EditorWindow.GetWindow( typeof(CarModifyInteriorFacesWindow), true, "CaronteFX - Modify interior faces UVs");
      }

      Instance.fracturerData_ = fracturerData;
      Instance.fracturerEditor_ = fracturerEditor;

      float width  = 300f;
      float height = 140f;

      Instance.minSize = new Vector2(width, height);
      Instance.maxSize = new Vector2(width, height);

      Instance.Focus();
      return Instance;
    }

    void OnLostFocus()
    {
      Close();
    }

    private void DrawInteriorFacesTiling()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Vector2Field(frInteriorFacesTilingCt_, fracturerData_.InteriorFacesTiling);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(fracturerData_, "Change " + frInteriorFacesTilingCt_.text + fracturerData_.Name);
        fracturerData_.InteriorFacesTiling = value;
      }
    }

    private void DrawInteriorFacesOffset()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Vector2Field(frInteriorFacesOffsetCt_, fracturerData_.InteriorFacesOffset);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(fracturerData_, "Change " + frInteriorFacesOffsetCt_.text + fracturerData_.Name);
        fracturerData_.InteriorFacesOffset = value;
      }
    }

    public void OnGUI()
    {
      EditorGUILayout.Space();
      EditorGUILayout.LabelField("UVs", EditorStyles.boldLabel);
      EditorGUILayout.Space();
      DrawInteriorFacesTiling();
      DrawInteriorFacesOffset();
      EditorGUILayout.Space();
      if (GUILayout.Button("Apply", GUILayout.Height(23f) ) )
      {
        fracturerEditor_.ApplyUVsPostProcess();
      }
      EditorGUILayout.Space();  
    }
  }

}