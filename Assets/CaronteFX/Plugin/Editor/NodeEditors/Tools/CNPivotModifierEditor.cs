using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CaronteFX
{
  public class CNPivotModifierEditor : CNMonoFieldEditor
  {
    public static Texture icon_;
    public override Texture TexIcon { get{ return icon_; } }

    List<GameObject> listGameObjectTmp_   = new List<GameObject>();
    List<Mesh>       listOriginalMeshTmp_ = new List<Mesh>();
    List<Mesh>       listModifiedMeshTmp_ = new List<Mesh>();
    List<Vector3>    listMeshMoveTmp_     = new List<Vector3>();

    new CNPivotModifier Data { get; set; }

    public CNPivotModifierEditor( CNPivotModifier data, CommandNodeEditorState state )
      : base(data, state)
    {
      Data = (CNPivotModifier)data;
    }

    public void ModifyPivots()
    {
      GameObject[] arrGOtoModifyPivot = FieldController.GetUnityGameObjects();
      if (arrGOtoModifyPivot.Length == 0)
      {
        EditorUtility.DisplayDialog("CaronteFX", "Please, first add some objects to modify to the field.", "Ok");
        return;
      }

      EditorUtility.DisplayProgressBar( Data.Name, "Modifying pivot...", 1.0f);

      listGameObjectTmp_  .Clear();
      listOriginalMeshTmp_.Clear();
      listModifiedMeshTmp_.Clear();
      listMeshMoveTmp_    .Clear();

      int nGameObject = arrGOtoModifyPivot.Length;
      for (int i = 0; i < nGameObject; i++)
      {
        GameObject go = arrGOtoModifyPivot[i];
        if (go != null)
        {
          Mesh oldMesh = go.GetMeshFromMeshFilterOnly();
          if (oldMesh != null)
          {
            Mesh newMesh;
            Vector3 meshMove = CarPivotModifier.ModifyGameObjectPivot(go, (CarPivotModifier.EPivotLocationMode)Data.PivotLocationMode, Data.LocalPivotOffset, out newMesh);

            listGameObjectTmp_.Add(go);
            listOriginalMeshTmp_.Add(oldMesh);
            listModifiedMeshTmp_.Add(newMesh);
            listMeshMoveTmp_.Add(meshMove);
          }
        }
      }

      if (listGameObjectTmp_.Count > 0)
      {
        Data.ArrModifiedGO   = listGameObjectTmp_.ToArray();
        Data.ArrOriginalMesh = listOriginalMeshTmp_.ToArray();
        Data.ArrModifiedMesh = listModifiedMeshTmp_.ToArray();
        Data.ArrMeshMove     = listMeshMoveTmp_.ToArray();

        EditorUtility.SetDirty(Data);
      }

      EditorUtility.ClearProgressBar();

    }

    public void RestoreOriginalPivots()
    { 
      GameObject[] arrModifiedGO    = Data.ArrModifiedGO;
      Mesh[]       arrOriginalMesh  = Data.ArrOriginalMesh;
      Mesh[]       arrModifiedMesh  = Data.ArrModifiedMesh;
      Vector3[]    arrMeshMove      = Data.ArrMeshMove;

      if (arrModifiedGO != null)
      {
        int nGameObject = arrModifiedGO.Length;
        for (int i = 0; i < nGameObject; i++)
        {
          GameObject modifiedGO = arrModifiedGO[i]; 
          if (modifiedGO != null)
          {
            Mesh originalMesh = arrOriginalMesh[i];
            Mesh modifiedMesh = arrModifiedMesh[i];
            Vector3 meshMove = arrMeshMove[i];

            MeshFilter mf = modifiedGO.GetComponent<MeshFilter>();
            if (mf != null)
            {
              mf.sharedMesh = originalMesh;
              modifiedGO.transform.position += meshMove;

              EditorUtility.SetDirty(mf);
              EditorUtility.SetDirty(modifiedGO.transform);

              if (modifiedMesh != null && !AssetDatabase.Contains(modifiedMesh.GetInstanceID()))
              {
                Object.DestroyImmediate(modifiedMesh);
              }
            }
          }
        }

        Data.ArrModifiedGO   = null;
        Data.ArrOriginalMesh = null;
        Data.ArrModifiedMesh = null;        
        Data.ArrMeshMove     = null;

        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawPivotLocationMode()
    {
      EditorGUI.BeginChangeCheck();
      var value = (CNPivotModifier.EPivotLocationMode) EditorGUILayout.EnumPopup("Pivot location mode", Data.PivotLocationMode);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Undo - Change pivot location mode");
        Data.PivotLocationMode = value;
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawPivotLocalOffset()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.Vector3Field("Pivot local offset", Data.LocalPivotOffset);
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Undo - Change pivot location offset");
        Data.LocalPivotOffset = value;
        EditorUtility.SetDirty(Data);
      }
    }

 
    private void DrawOutput()
    {
      EditorGUI.BeginDisabledGroup(Data.ArrModifiedGO == null);
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField("", GUILayout.Width(50f));
      if ( GUILayout.Button("Select modified objects in hierarchy", GUILayout.Height(22f)) )
      {
        Selection.objects = Data.ArrModifiedGO;
      }
      EditorGUILayout.LabelField("", GUILayout.Width(50f));
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField("", GUILayout.Width(50f));
      if ( GUILayout.Button("Restore original pivots", GUILayout.Height(22f)) )
      {
        RestoreOriginalPivots();
      }
      EditorGUILayout.LabelField("", GUILayout.Width(50f));
      EditorGUILayout.EndHorizontal();

      EditorGUI.EndDisabledGroup();
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
      if ( GUILayout.Button("Modify Pivots", GUILayout.Height(30f) ) )
      {
        ModifyPivots();
      }

      if ( GUILayout.Button("Save assets...", GUILayout.Height(30f), GUILayout.Width(100f) ) )
      {
        SaveAssets();
      }
      EditorGUILayout.EndHorizontal();

      EditorGUI.EndDisabledGroup();
      CarGUIUtils.DrawSeparator();

      scroller_ = EditorGUILayout.BeginScrollView(scroller_);

      EditorGUILayout.Space();
      EditorGUILayout.Space();

      float originalLabelwidth = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = 200f;

      DrawPivotLocationMode();
      EditorGUILayout.Space();
      DrawPivotLocalOffset();

      EditorGUILayout.Space();

      CarGUIUtils.Splitter();

      GUIStyle centerLabel = new GUIStyle(EditorStyles.largeLabel);
      centerLabel.alignment = TextAnchor.MiddleCenter;
      centerLabel.fontStyle = FontStyle.Bold;
      EditorGUILayout.LabelField("Output", centerLabel);

      
      EditorGUILayout.Space();

      DrawOutput();

      EditorGUILayout.Space();


      EditorGUIUtility.labelWidth = originalLabelwidth;

      EditorGUILayout.EndScrollView();

      GUILayout.EndArea();
    } // RenderGUI

    private bool HasUnsavedMeshReferences()
    {
      GameObject[] arrModifiedObject = Data.ArrModifiedGO;

      if (Data.ArrModifiedGO == null)
      {
        return false;
      }

      return CarEditorUtils.IsAnyUnsavedMeshInArrGameObject(arrModifiedObject);
    }

    //SaveAssets
    private void SaveAssets()
    {
      if ( !HasUnsavedMeshReferences() )
      {
        EditorUtility.DisplayDialog("CaronteFX - Info", "There is not any mesh to save in assets.", "ok" );
        return;
      }

      GameObject[] arrModifiedObject = Data.ArrModifiedGO;
      CarEditorUtils.SaveAnyUnsavedMeshInArrGameObject(Data.Name, arrModifiedObject, false);

    } //Save tessellator result
  }
}

