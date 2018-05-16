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
  [CustomEditor(typeof(CarCameraCapturer))]
  public class CRCameraCapturerEditor : Editor
  {
    CarCameraCapturer capturer_;

    void OnEnable()
    {
      capturer_ = (CarCameraCapturer)target;
    }

    public override void OnInspectorGUI()
    {
      DrawDefaultInspector();

      if ( GUILayout.Button("Open screen shots folder") )
      {
        EditorUtility.RevealInFinder(capturer_.folder);
      }
    }
  }
}

