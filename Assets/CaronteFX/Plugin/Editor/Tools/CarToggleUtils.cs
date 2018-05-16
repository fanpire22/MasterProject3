using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace CaronteFX
{
  public static class CarToggleUtils
  {
    public static void DrawToggleMixedMonoBehaviours(string toggleString, List<MonoBehaviour> listMonoBehaviour, float width)
    {
      EditorGUI.BeginChangeCheck();  
      int nMonoBehaviours = listMonoBehaviour.Count;

      EditorGUI.showMixedValue = false;
      bool value = false;
      
      if (nMonoBehaviours > 0)
      {
        value = listMonoBehaviour[0].enabled;
        for (int i = 1; i < nMonoBehaviours; ++i)
        {
          MonoBehaviour mbh = listMonoBehaviour[i];
          if ( value != mbh.enabled )
          {
            EditorGUI.showMixedValue = true;
            break;
          }
        }
      }
      EditorGUI.BeginDisabledGroup( nMonoBehaviours == 0 );
#if UNITY_PRO_LICENSE
      value = EditorGUILayout.ToggleLeft(toggleString, value, GUILayout.MaxWidth(width) );
#else
      value = GUILayout.Toggle(value, toggleString, GUILayout.MaxWidth(width));
#endif
      EditorGUI.showMixedValue = false;
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObjects(listMonoBehaviour.ToArray(), "CaronteFX - Change " + toggleString);
        for (int i = 0; i < nMonoBehaviours; ++i)
        {
          MonoBehaviour mbh = listMonoBehaviour[i];
          mbh.enabled = value;
          EditorUtility.SetDirty(mbh);
        }
      }
      EditorGUI.EndDisabledGroup();
    }
    //-----------------------------------------------------------------------------------
    public static void DrawToggleMixedRenderers(string toggleString, List<Renderer> listRenderer, float width)
    {
      EditorGUI.BeginChangeCheck();  
      int nRenderer = listRenderer.Count;

      EditorGUI.showMixedValue = false;
      bool value = false;
      
      if (nRenderer > 0)
      {
        int i;
        for (i = 0; i < nRenderer; i++)
        {
          if (listRenderer[i] != null)
          {
            value = listRenderer[i].enabled;
            break;
          }
        }

        for (i = i + 1; i < nRenderer; i++)
        {
          Renderer mbh = listRenderer[i];
          if ( mbh != null && value != mbh.enabled )
          {
            EditorGUI.showMixedValue = true;
            break;
          }
        }
      }
      EditorGUI.BeginDisabledGroup( nRenderer == 0 );

#if UNITY_PRO_LICENSE
      value = EditorGUILayout.ToggleLeft(toggleString, value, GUILayout.MaxWidth(width) );
#else
      value = GUILayout.Toggle(value, toggleString, GUILayout.MaxWidth(width));
#endif

      EditorGUI.showMixedValue = false;
      if (EditorGUI.EndChangeCheck())
      {
        int undoGroupIdx = Undo.GetCurrentGroup();
        bool wasSomeUndo = false;
        for (int i = 0; i < nRenderer; ++i)
        {
          Renderer rn = listRenderer[i];
          if (rn != null)
          {
            Undo.RecordObject(rn, "CaronteFX - Change " + toggleString);
            rn.enabled = value;
            EditorUtility.SetDirty(rn);
            wasSomeUndo = true;
          }
        }

        if (wasSomeUndo)
        {
          Undo.CollapseUndoOperations(undoGroupIdx);
        }
      }
      EditorGUI.EndDisabledGroup();
    }
    //-----------------------------------------------------------------------------------
    public static void DrawToggleMixedBodyComponents(string toggleString, List<Caronte_Fx_Body> listBodyComponent, float width)
    {
      EditorGUI.BeginChangeCheck();  
      int nMonoBehaviours = listBodyComponent.Count;

      EditorGUI.showMixedValue = false;
      bool value = false;
      
      if (nMonoBehaviours > 0)
      {
        int i;
        for (i = 0; i < nMonoBehaviours; i++)
        {
          if (listBodyComponent[i] != null)
          {
            value = listBodyComponent[i].RenderCollider;
            break;
          }
        }

        for (i = i + 1; i < nMonoBehaviours; i++)
        {
          Caronte_Fx_Body mbh = listBodyComponent[i];
          if ( mbh != null && value != mbh.RenderCollider )
          {
            EditorGUI.showMixedValue = true;
            break;
          }
        }
      }
      EditorGUI.BeginDisabledGroup( nMonoBehaviours == 0 );
#if UNITY_PRO_LICENSE
      value = EditorGUILayout.ToggleLeft(toggleString, value, GUILayout.MaxWidth(width) );
#else
      value = GUILayout.Toggle(value, toggleString, GUILayout.MaxWidth(width));
#endif
      EditorGUI.showMixedValue = false;
      if (EditorGUI.EndChangeCheck())
      {
        int undoGroupIdx = Undo.GetCurrentGroup();
        bool wasSomeUndo = false;
        for (int i = 0; i < nMonoBehaviours; i++)
        {
          Caronte_Fx_Body cfxBody = listBodyComponent[i];  
          if (cfxBody != null)
          {
            Undo.RecordObject(cfxBody, "CaronteFX - Change " + toggleString);
            cfxBody.RenderCollider = value;
            EditorUtility.SetDirty(cfxBody);
            wasSomeUndo = true;
          }
        }

        if (wasSomeUndo)
        {
          Undo.CollapseUndoOperations(undoGroupIdx);
        }
      }
      EditorGUI.EndDisabledGroup();
    }
    //-----------------------------------------------------------------------------------
  }

}

