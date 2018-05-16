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

namespace CaronteFX
{

  public class CarAboutWindow: CarWindow<CarAboutWindow>
  {   
    Texture companyIcon_;
    string versionString_;

    private static readonly string stFbxSdkLicense_ = CarStringManager.GetString("AboutFbxSdkLicense");

    DateTime expirationDateTime_;

    public static CarAboutWindow ShowWindow()
    {
      if (Instance == null)
      {
        Instance = (CarAboutWindow)EditorWindow.GetWindow(typeof(CarAboutWindow), true, "About CaronteFX");
      }

      float width  = 450f;
      float height = 400f;

      Instance.minSize = new Vector2(width, height);
      Instance.maxSize = new Vector2(width, height);
    
      Instance.Focus();    
      return Instance;
    }

    void OnEnable()
    {
      string version = CaronteSharp.Caronte.GetNativeDllVersion();
      string versionTypeName;

      if (CarVersionChecker.IsEvaluationVersion() )
      {
        versionTypeName = " PRO Evaluation";

      }
      else
      {
        versionTypeName = " PRO";
      }

      if (CarVersionChecker.DoVersionExpires())
      {
        expirationDateTime_ = CarVersionChecker.GetExpirationDateDateInSeconds();
      }

      companyIcon_ = CarEditorResource.LoadEditorTexture(CarVersionChecker.CompanyIconName);
      versionString_ = "Version: " + version + versionTypeName;
    }

    void OnLostFocus()
    {
      Close();
    }

    public void OnGUI()
    {
      GUI.DrawTexture(new Rect(-30, 0, 260f, 260f), CarManagerEditor.ic_logoCaronte_);
      GUILayout.FlexibleSpace();
      GUILayout.BeginArea( new Rect( 10f, 220f, 230f, 180f ) );

      EditorGUILayout.Space();

      GUILayout.Label( new GUIContent("Powered by Caronte physics engine."), EditorStyles.miniLabel );
      GUILayout.Label( new GUIContent("(c) 2017 Next Limit Technologies."), EditorStyles.miniLabel );
      GUILayout.Label( new GUIContent( versionString_ ), EditorStyles.miniLabel );

      if (CarVersionChecker.CompanyName != string.Empty)
      {
        GUILayout.Label(new GUIContent("This version is exclusive for " + CarVersionChecker.CompanyName + "\ninternal use.\n"), EditorStyles.miniLabel);
        GUILayout.Label(new GUIContent(companyIcon_), GUILayout.MaxWidth(69.7f), GUILayout.MaxHeight(32f));
      }

      GUILayout.EndArea();

      GUILayout.BeginArea( new Rect( 200f, 0f, 245f, 400f ) ); 
      
      if (CarVersionChecker.DoVersionExpires())
      {
        EditorGUILayout.Space();
        GUILayout.Label(new GUIContent("Expiration date of this version is:\n\n" + expirationDateTime_.ToShortDateString() + " (month/day/year).\n\nUse of this software is forbidden\nafter the expiration date."), EditorStyles.miniLabel);
      }

      GUILayout.FlexibleSpace();
      EditorGUILayout.Space();
      GUILayout.Label(stFbxSdkLicense_, EditorStyles.textArea);
      EditorGUILayout.Space();
      GUILayout.FlexibleSpace();
      GUILayout.EndArea();
    }

  }
}
