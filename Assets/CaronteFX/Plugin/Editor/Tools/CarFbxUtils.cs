using UnityEditor;

namespace CaronteFX
{
  public static class CarFbxUtils
  {

#if UNITY_EDITOR_WIN
    [MenuItem("Assets/CaronteFX - Export Selection to FBX")]
    public static void ExportSelectionToFBX()
    {
      CarFbxExporter.StaticExportSelectionToFbx();
    }
#endif

  }
}




