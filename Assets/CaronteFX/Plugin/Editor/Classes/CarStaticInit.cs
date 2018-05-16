using UnityEngine;
using UnityEditor;

namespace CaronteFX
{
  [InitializeOnLoad]
  static class CarStaticInit
  {   
    static Texture ic_CaronteFX_;
    static CarStaticInit()
    {
#if UNITY_5_5_OR_NEWER
      RegisterHierarchyCB();
#endif
    }

    private static void RegisterHierarchyCB()
    {
      ic_CaronteFX_ = CarEditorResource.LoadEditorTexture("cr_icon_carontefx");
      EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCB;
    }

    private static void HierarchyItemCB (int instanceID, Rect selectionRect)
    {
      Rect r = new Rect (selectionRect); 
      r.x = 1.0f;
      r.width = 18;
        
      GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
      if (go != null && go.GetComponent<Caronte_Fx>() != null)
      {
        GUI.Label (r, ic_CaronteFX_); 
      }
    }
 
  }

}

