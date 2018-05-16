using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using CaronteSharp;


namespace CaronteFX
{

  public class CNWelderEditor : CNMeshToolEditor
  {

    private static readonly GUIContent wdSelectWeldedObjectCt_ = new GUIContent( CarStringManager.GetString("WdSelectWeldedObject"), CarStringManager.GetString("WdSelectWeldedObjectTooltip") );
    public static Texture icon_;

    public override Texture TexIcon { get{ return icon_; } }

    new CNWelder Data { get; set; }

    public CNWelderEditor( CNWelder data, CommandNodeEditorState state )
      : base(data, state)
    {
      Data = (CNWelder)data;
    }

    public void Weld()
    {
      GameObject[] arrGOtoWeld = FieldController.GetUnityGameObjects();

      int arrGOtoWeld_size = arrGOtoWeld.Length;
      if (arrGOtoWeld_size == 0)
      {
        EditorUtility.DisplayDialog( "CaronteFX", "Input objects are mandatory", "Ok");
        return;
      }

      GameObject[] arrWeldedObject;
      Mesh[]       arrWeldedMesh;

      EditorUtility.DisplayProgressBar(Data.Name, "Welding...", 1.0f);
      CarGeometryUtils.WeldObjects( arrGOtoWeld, Data.Name, out arrWeldedObject, out arrWeldedMesh );
      EditorUtility.ClearProgressBar();
  
      if (arrWeldedObject != null)
      {
        DeleteOldObjects();

        List<GameObject> listWeldedObjects = new List<GameObject>();
        listWeldedObjects.AddRange(arrWeldedObject);

        Bounds bounds = CarEditorUtils.GetGlobalBoundsWorld(listWeldedObjects);

        GameObject go = new GameObject( Data.Name + "_output" );
        go.transform.position = bounds.center;

        foreach( GameObject weldedGO in listWeldedObjects )
        {
          weldedGO.transform.parent = go.transform;
        }

        if (Data.WeldGameObject != null)
        {
          go.transform.parent = Data.WeldGameObject.transform.parent;
          Object.DestroyImmediate( Data.WeldGameObject );
        }

        Data.WeldGameObject = go;
        Data.ArrWeldedGO    = arrWeldedObject;
        Data.ArrWeldedMesh  = arrWeldedMesh;
        EditorUtility.SetDirty(Data);

        UnityEditor.Selection.activeGameObject = Data.WeldGameObject;
      }
    }

    public void DeleteOldObjects()
    { 
      GameObject[] arrGOWelded = Data.ArrWeldedGO;
      
      if (arrGOWelded != null)
      {
        foreach( GameObject go in arrGOWelded )
        {
          if (go != null)
          {
            Object.DestroyImmediate(go);
          }
        }
      }

      Mesh[] arrMeshWelded = Data.ArrWeldedMesh;

      if (arrMeshWelded != null)
      {
        foreach( Mesh mesh in arrMeshWelded )
        {
          if (mesh != null)
          {
            Object.DestroyImmediate(mesh);
          }
        }
      }
    }

    private void DrawOutputButton()
    {
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField("", GUILayout.Width(50f));
      if ( GUILayout.Button(wdSelectWeldedObjectCt_, GUILayout.Height(22f)) )
      {
        Selection.activeGameObject = Data.WeldGameObject;
      }
      EditorGUILayout.LabelField("", GUILayout.Width(50f));
      EditorGUILayout.EndHorizontal();
    }


    public override void RenderGUI(Rect area, bool isEditable)
    {
      GUILayout.BeginArea(area);

      RenderTitle(isEditable, false, false);

      EditorGUI.BeginDisabledGroup(!isEditable);
      RenderFieldObjects( "Objects", FieldController, true, true, CNFieldWindow.Type.normal );

      EditorGUILayout.Space();
      EditorGUILayout.Space();

      EditorGUILayout.BeginHorizontal();
      if ( GUILayout.Button("Weld", GUILayout.Height(30f) ) )
      {
        Weld();
      }
      if ( GUILayout.Button("Save asset...", GUILayout.Height(30f), GUILayout.Width(100f) ) )
      {
        SaveWeldResult();
      }
      EditorGUILayout.EndHorizontal();

      EditorGUI.EndDisabledGroup();
      CarGUIUtils.DrawSeparator();

      scroller_ = EditorGUILayout.BeginScrollView(scroller_);

      EditorGUILayout.Space();
      EditorGUILayout.Space();

      float originalLabelwidth = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = 200f;
    
      CarGUIUtils.Splitter();
      GUIStyle centerLabel = new GUIStyle(EditorStyles.largeLabel);
      centerLabel.alignment = TextAnchor.MiddleCenter;
      centerLabel.fontStyle = FontStyle.Bold;
      EditorGUILayout.LabelField("Output", centerLabel);

      EditorGUI.BeginDisabledGroup(Data.WeldGameObject == null);

      EditorGUILayout.Space();
      
      DrawOutputButton();

      EditorGUILayout.Space();

      EditorGUI.EndDisabledGroup();

      EditorGUIUtility.labelWidth = originalLabelwidth;

      EditorGUILayout.EndScrollView();

      GUILayout.EndArea();
    } // RenderGUI


    public bool HasUnsavedWeldReferences()
    {
      if (Data.WeldGameObject == null)
      {
        return false;
      }

      if (CarEditorUtils.IsAnyUnsavedMeshInHierarchy(Data.WeldGameObject) )
      {   
        return true;
      }
      else
      {
        return false;
      }    
    }

    public void SaveWeldResult()
    {
      if (!HasUnsavedWeldReferences())
      {
        EditorUtility.DisplayDialog("CaronteFX - Info", "There is not any mesh to save in assets.", "ok" );
        return;
      }

      CarEditorUtils.SaveAnyUnsavedMeshInHierarchy(Data.WeldGameObject, false);

    } //Save weld result


  } // class CNWelderView

} //namespace CaronteFX
