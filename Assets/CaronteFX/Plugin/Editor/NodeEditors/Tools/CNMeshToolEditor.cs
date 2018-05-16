using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CaronteFX
{
  
  public abstract class CNMeshToolEditor : CNMonoFieldEditor
  {
    public CNMeshToolEditor( CNMonoField data, CommandNodeEditorState state )
      : base( data, state )
    {

    }

    public override void Init()
    {
      base.Init();                                    
      FieldController.SetFieldContentType( CNFieldContentType.Geometry );
    }   
  }

}
