using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CaronteFX
{
  
  public abstract class CNTriggerEditor : CNEntityEditor
  {
    public CNTriggerEditor( CNTrigger data, CommandNodeEditorState state )
      : base( data, state )
    {

    }

    public override void Init()
    {
      base.Init();                                   
      FieldController.SetFieldContentType( CNFieldContentType.EntityNode );
    }   
  }

}
