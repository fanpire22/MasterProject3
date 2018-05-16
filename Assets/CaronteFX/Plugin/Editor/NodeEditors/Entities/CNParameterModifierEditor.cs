using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CaronteFX
{

  public class CNParameterModifierEditor : CNEntityEditor
  {
    public static Texture icon_;

    public override Texture TexIcon { get{ return icon_; } }

    protected string[] properties = new string[] { "Velocity Linear", "Velocity Angular", "Enabled", "Plasticity", "Freeze", "Visible", "Force Multiplier", "External Damping", "Servos Target (linear)", "Servos Target (angular)", "Collide with all", "Collide with bodies b", "Autocollision" };
    protected string[] onOffFlip  = new string[] { "Off", "On", "Flip" };
    protected string[] onOff      = new string[] { "Off", "On" };

    List<CNFieldController> listCommandFieldController = new List<CNFieldController>();

    new CNParameterModifier Data { get; set; }

    public CNParameterModifierEditor( CNParameterModifier data, CommandNodeEditorState state )
      : base(data, state)
    {
      Data = (CNParameterModifier)data;
    }

    public override void Init()
    {
      base.Init();
  
      CNFieldContentType allowedTypes =   CNFieldContentType.Bodies 
                                        | CNFieldContentType.JointServosNode
                                        | CNFieldContentType.DaemonNode
                                        | CNFieldContentType.TriggerNode;

      FieldController.SetFieldContentType(allowedTypes);

      List<ParameterModifierCommand> listPmCommand = Data.ListPmCommand;
      if (listPmCommand.Count == 0)
      {
        listPmCommand.Add( new ParameterModifierCommand() );
      }

      foreach(ParameterModifierCommand pmCommand in listPmCommand)
      {
        if (pmCommand.RequieresFieldController)
        {
          CNFieldController fc = new CNFieldController(Data, pmCommand.FieldBodies_B, eManager, goManager);
          fc.IsBodyField = true;
          listCommandFieldController.Add(fc);
        }
        else
        {
          listCommandFieldController.Add(null);
        }
      }
    }

    public override void LoadInfo()
    {
      foreach(CNFieldController fc in listCommandFieldController)
      {
        if (fc != null)
        {
          fc.RestoreFieldInfo();
        }
      }
      base.LoadInfo();
    }

    public override void StoreInfo()
    {
      foreach(CNFieldController fc in listCommandFieldController)
      {
        if (fc != null)
        {
          fc.StoreFieldInfo();
        }
      }
      base.StoreInfo();
    }

    public override void BuildListItems()
    {
      foreach(CNFieldController fc in listCommandFieldController)
      {
        if (fc != null)
        {
          fc.BuildListItems();
        }
      }
      base.BuildListItems();
    }

    public override bool RemoveNodeFromFields(CommandNode node)
    {
      Undo.RecordObject(Data, "CaronteFX - Remove node from fields");
      List<ParameterModifierCommand> listPmCommnad = Data.ListPmCommand;

      bool removed = false;
      foreach( ParameterModifierCommand pmCommand in listPmCommnad)
      {
        removed |= pmCommand.FieldBodies_B.RemoveNode(node);
      }

      return (removed || base.RemoveNodeFromFields(node));
    }

    public override void SetScopeId(uint scopeId)
    {
      foreach(CNFieldController fc in listCommandFieldController)
      {
        if (fc != null)
        {
          fc.SetScopeId(scopeId);
        }
      }
      base.SetScopeId(scopeId);
    }

    public override void FreeResources()
    {
      foreach(CNFieldController fc in listCommandFieldController)
      {
        if (fc != null)
        {
          fc.DestroyField();
        }
      }   
      base.FreeResources();
    }

    public override void CreateEntitySpec()
    {
      eManager.CreateParameterModifier( Data );
    }

    public override void ApplyEntitySpec()
    {
      GameObject[] arrGameObject   = FieldController.GetUnityGameObjects();
      CommandNode[] arrCommandNode = FieldController.GetCommandNodes();

      List<GameObject[]> listCommandGameObject = new List<GameObject[]>();

      foreach(CNFieldController fc in listCommandFieldController)
      {
        GameObject[] arrCommandGameObject;
        if (fc != null)
        {
          arrCommandGameObject = fc.GetUnityGameObjects();
        }
        else
        {
          arrCommandGameObject = new GameObject[0];
        }
        listCommandGameObject.Add(arrCommandGameObject);
      }

      eManager.RecreateParameterModifier( Data, arrGameObject, arrCommandNode, listCommandGameObject);
    }

    public override void RenderGUI(Rect area, bool isEditable)
    {
      GUILayout.BeginArea(area);

      RenderTitle(isEditable, true, false);

      EditorGUI.BeginDisabledGroup(!isEditable);
      RenderFieldObjects( "Objects", FieldController, true, true, CNFieldWindow.Type.extended );

      EditorGUILayout.Space();
      EditorGUILayout.Space();

      EditorGUI.EndDisabledGroup();
      
      CarGUIUtils.DrawSeparator();
      CarGUIUtils.Splitter();

      EditorGUILayout.Space();

      EditorGUI.BeginDisabledGroup( !isEditable );
      
      EditorGUILayout.Space();

      DrawTimer();
      
      EditorGUILayout.Space();
      EditorGUILayout.Space();
      EditorGUILayout.Space();

      EditorGUILayout.LabelField("Object parameters to modify: ");

      CarGUIUtils.Splitter();
      scroller_ = EditorGUILayout.BeginScrollView(scroller_);

      DrawCommandList();

      EditorGUI.EndDisabledGroup();
      EditorGUILayout.EndScrollView();

      GUILayout.EndArea();
    }

    private void DrawCommandList()
    {
      List<ParameterModifierCommand> listPmCommand = Data.ListPmCommand;

      ParameterModifierCommand pmCommandToRemove = null;
      ParameterModifierCommand pmCommandToAdd = null;

      int addPosition = 0;

      int nPmCommand = listPmCommand.Count;

      for( int i = 0; i < nPmCommand; i++ )
      {
        ParameterModifierCommand pmCommand = listPmCommand[i];
        DrawPmCommand(i, pmCommand, ref pmCommandToRemove, ref pmCommandToAdd, ref addPosition );
      }

      if (pmCommandToRemove != null && listPmCommand.Count > 1)
      {
        int index = listPmCommand.IndexOf(pmCommandToRemove);
        listPmCommand.RemoveAt( index );


        bool recalculateFields = false;
        CNFieldController fc = listCommandFieldController[index];
        if (fc != null)
        {
          fc.DestroyField();
          recalculateFields = true;
        }

        listCommandFieldController.RemoveAt( index );
        pmCommandToRemove = null;
        
        if (recalculateFields)
        {
          cnHierarchy.RecalculateFieldsDueToUserAction();
        }
        EditorUtility.SetDirty(Data);
      }

      if (pmCommandToAdd != null )
      {
        listPmCommand.Insert(addPosition, pmCommandToAdd);

        if (pmCommandToAdd.RequieresFieldController)
        {
          CNFieldController fc = new CNFieldController(Data, pmCommandToAdd.FieldBodies_B, eManager, goManager);
          fc.IsBodyField = true;
          listCommandFieldController.Insert(addPosition, fc);
          cnHierarchy.RecalculateFieldsDueToUserAction();
        }
        else
        {
          listCommandFieldController.Insert(addPosition, null);
        }
   

        pmCommandToAdd = null;   
        EditorUtility.SetDirty(Data);
      }
    }

    private void DrawPmCommand(int commandIdx, ParameterModifierCommand pmCommand, ref ParameterModifierCommand pmCommandToRemove, ref ParameterModifierCommand pmCommandToAdd, ref int addPosition)
    {
      GUILayout.Space(6f);
      EditorGUILayout.BeginHorizontal();
      
      bool prevRequieredFieldController = pmCommand.RequieresFieldController;
      EditorGUI.BeginChangeCheck();
      var valueProperty = (PARAMETER_MODIFIER_PROPERTY)EditorGUILayout.Popup( (int)pmCommand.Target, properties, GUILayout.Width( 150f ) );
      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(Data, "Change parameter type - " + Data.Name);
        pmCommand.Target = valueProperty;

        bool currentRequieresFieldController = pmCommand.RequieresFieldController;

        if ( !prevRequieredFieldController &&
             currentRequieresFieldController )
        {
          CNFieldController fc = new CNFieldController(Data, pmCommand.FieldBodies_B, eManager, goManager);
          fc.IsBodyField = true;
          listCommandFieldController[commandIdx] = fc;
        }
        else if ( prevRequieredFieldController &&
                  !currentRequieresFieldController )
        {
          CNFieldController fc = listCommandFieldController[commandIdx];
          fc.DestroyField();
          listCommandFieldController[commandIdx] = null;
        }
        EditorUtility.SetDirty(Data);
      }
      GUILayout.Space(30f);

      CNFieldController fcCommand = listCommandFieldController[commandIdx];
      switch (pmCommand.Target)
      {
        case PARAMETER_MODIFIER_PROPERTY.VELOCITY_LINEAL:
        case PARAMETER_MODIFIER_PROPERTY.VELOCITY_ANGULAR:
        case PARAMETER_MODIFIER_PROPERTY.TARGET_ANGLE:
        case PARAMETER_MODIFIER_PROPERTY.TARGET_POSITION:
          {
            EditorGUI.BeginChangeCheck();
            var value = EditorGUILayout.Vector3Field( "", pmCommand.ValueVector3, GUILayout.Width( 150f ) );
            if (EditorGUI.EndChangeCheck())
            {
              Undo.RecordObject(Data, "Change parameter value - " + Data.Name);
              pmCommand.ValueVector3  = value;
              EditorUtility.SetDirty(Data);
            }
            break;
          }

        case PARAMETER_MODIFIER_PROPERTY.ACTIVITY:
        case PARAMETER_MODIFIER_PROPERTY.PLASTICITY:
          {
            EditorGUI.BeginChangeCheck();
            var value = EditorGUILayout.Popup( pmCommand.ValueInt, onOffFlip, GUILayout.Width( 150f ) );
            if (EditorGUI.EndChangeCheck())
            {
              Undo.RecordObject(Data, "Change parameter value - " + Data.Name);
              pmCommand.ValueInt = (int)value;
              EditorUtility.SetDirty(Data);
            }
          
            break;
          }

        case PARAMETER_MODIFIER_PROPERTY.FREEZE:
        case PARAMETER_MODIFIER_PROPERTY.VISIBILITY:
        case PARAMETER_MODIFIER_PROPERTY.COLLIDE_WITH_ALL:
        case PARAMETER_MODIFIER_PROPERTY.COLLIDE_AUTOCOLLISION:
          {
            EditorGUI.BeginChangeCheck();
            var value = EditorGUILayout.Popup( pmCommand.ValueInt, onOff, GUILayout.Width( 150f ) );
            if (EditorGUI.EndChangeCheck())
            {
              Undo.RecordObject(Data, "Change parameter value - " + Data.Name);
              pmCommand.ValueInt = (int)value;
              EditorUtility.SetDirty(Data);
            }
          
            break;
          }

        case PARAMETER_MODIFIER_PROPERTY.FORCE_MULTIPLIER:
        case PARAMETER_MODIFIER_PROPERTY.EXTERNAL_DAMPING:
          {
            EditorGUI.BeginChangeCheck();
            var value = EditorGUILayout.FloatField( pmCommand.ValueFloat, GUILayout.Width( 150f ) );
            if (EditorGUI.EndChangeCheck())
            {
              Undo.RecordObject(Data, "Change parameter value - " + Data.Name);
              pmCommand.ValueFloat = value;
              EditorUtility.SetDirty(Data);
            }
            break;
          }

        case PARAMETER_MODIFIER_PROPERTY.COLLIDE_BY_PAIRS:
          {   
            GUILayout.BeginVertical();   
            EditorGUI.BeginChangeCheck();
            var value = EditorGUILayout.Popup( pmCommand.ValueInt, onOff, GUILayout.Width( 150f ) );
            if (EditorGUI.EndChangeCheck())
            {
              Undo.RecordObject(Data, "Change parameter value - " + Data.Name);
              pmCommand.ValueInt = (int)value;
              EditorUtility.SetDirty(Data);
            }
            GUILayout.Space(3f);
       
            RenderFieldObjects("Bodies B", 60f, fcCommand, true, false, CNFieldWindow.Type.extended );
            GUILayout.EndVertical();
            break;     
          }


        default:
          throw new NotImplementedException();
      }

      if (pmCommand.Target != PARAMETER_MODIFIER_PROPERTY.COLLIDE_BY_PAIRS)
      {
        GUILayout.FlexibleSpace();
      }

      if ( GUILayout.Button( new GUIContent("-", "delete"), EditorStyles.miniButtonLeft, GUILayout.Width(25f) ) )
      {
        pmCommandToRemove = pmCommand;
      }
      if ( GUILayout.Button( new GUIContent("+", "Add"), EditorStyles.miniButtonRight, GUILayout.Width(25f) ) )
      {
        pmCommandToAdd = new ParameterModifierCommand();
        addPosition = commandIdx + 1;
      }
      EditorGUILayout.EndHorizontal();
      CarGUIUtils.Splitter();
    }
  }

 }
