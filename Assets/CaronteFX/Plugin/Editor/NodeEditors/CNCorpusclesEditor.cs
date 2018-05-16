using UnityEngine;
using UnityEditor;

namespace CaronteFX
{
  public class CNCorpusclesEditorState : CommandNodeEditorState
  {
    public float corpusclesRadius_;
  }
  //-----------------------------------------------------------------------------------
  public abstract class  CNCorpusclesEditor : CNMonoFieldEditor
  {
    public static Texture icon_;
    public override Texture TexIcon 
    { 
      get { return icon_; }
    }

    protected new CNCorpusclesEditorState state_;
    protected new CNCorpuscles Data { get; set; }

    public CNCorpusclesEditor( CNCorpuscles data, CNCorpusclesEditorState state )
      : base( data, state )
    {
      Data   = (CNCorpuscles)data;
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
    protected override void LoadState()
    {
      base.LoadState();
      state_.corpusclesRadius_ = Data.CorpusclesRadius;
    }
    //-----------------------------------------------------------------------------------
    public override void ValidateState()
    {
      base.ValidateState();
      ValidateCorpusclesRadius();
    }
    //-----------------------------------------------------------------------------------
    private void ValidateCorpusclesRadius()
    {
      if ( state_.corpusclesRadius_ != Data.CorpusclesRadius )
      {
        DestroyCorpuscles();
        CreateCorpuscles();

        state_.corpusclesRadius_ = Data.CorpusclesRadius;
      }
    }
    //-----------------------------------------------------------------------------------
    public abstract void CreateCorpuscles();
    //-----------------------------------------------------------------------------------
    public abstract void DestroyCorpuscles();
    //-----------------------------------------------------------------------------------
    protected void DrawCorpusclesRadius()
    {
      EditorGUI.BeginChangeCheck();
      var value = EditorGUILayout.FloatField( "Corpuscles radius (m)", Data.CorpusclesRadius );
      if ( EditorGUI.EndChangeCheck() )
      {
        Undo.RecordObject(Data, "Change corpuscles radius - " + Data.Name);
        Data.CorpusclesRadius = value;
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

      DrawCorpusclesRadius();

      EditorGUI.EndDisabledGroup();
      EditorGUILayout.EndScrollView();

      EditorGUIUtility.labelWidth = originalLabelWidth;

      GUILayout.EndArea();
    }
  }
}

