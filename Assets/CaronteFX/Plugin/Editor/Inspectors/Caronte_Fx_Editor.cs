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

namespace CaronteFX
{
  [CustomEditor(typeof(Caronte_Fx))]
  public class Caronte_Fx_Editor : Editor
  {
    Caronte_Fx fxData_;
    static Texture ic_logoCaronte_ = null;

    private void OnEnable()
    {
      fxData_ = (Caronte_Fx)target;
      LoadCaronteIcon();
    }

    void LoadCaronteIcon()
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

    public override void OnInspectorGUI()
    {
      Rect rect = GUILayoutUtility.GetRect(80f, 80f);
      GUI.DrawTexture(rect, ic_logoCaronte_, ScaleMode.ScaleToFit );
      CarGUIUtils.Splitter();
      
      EditorGUILayout.Space();
      EditorGUILayout.Space();
      GUILayout.BeginHorizontal();  
      GUILayout.FlexibleSpace();

      if (GUILayout.Button("Open in CaronteFX Editor", GUILayout.Height(30f) ) )
      { 
        CarManagerEditor editor = (CarManagerEditor)CarManagerEditor.Init();
        editor.Controller.SetFxDataActive(fxData_);
      }
      GUILayout.FlexibleSpace();
      GUILayout.EndHorizontal();
      EditorGUILayout.Space();
      EditorGUILayout.Space();
    }
  }
}