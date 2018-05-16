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
using System;
using System.Collections;
using System.Collections.Generic;

namespace CaronteFX
{
  public enum PARAMETER_MODIFIER_PROPERTY
  {
    VELOCITY_LINEAL       = 0,
    VELOCITY_ANGULAR      = 1,
    ACTIVITY              = 2,
    PLASTICITY            = 3,
    FREEZE                = 4,
    VISIBILITY            = 5,
    FORCE_MULTIPLIER      = 6,
    EXTERNAL_DAMPING      = 7,
    TARGET_POSITION       = 8,
    TARGET_ANGLE          = 9,
    COLLIDE_WITH_ALL      = 10,
    COLLIDE_BY_PAIRS      = 11,
    COLLIDE_AUTOCOLLISION = 12,

    UNKNOWN
  };

  [Serializable]
  public class ParameterModifierCommand : IDeepClonable<ParameterModifierCommand>, ICarParameter
  {
    [SerializeField]
    private PARAMETER_MODIFIER_PROPERTY target_;
    [SerializeField]
    private Vector3 valueVector3_;
    [SerializeField]
    private int valueInt_;
    [SerializeField]
    private CNField fieldBodies_B_;

    public PARAMETER_MODIFIER_PROPERTY Target
    {
      get { return target_; }
      set { target_ = value; }
    }

    public float ValueFloat
    {
      get { return valueVector3_.x; }
      set { valueVector3_.x = value; }
    }

    public Vector3 ValueVector3
    {
      get { return valueVector3_; }
      set { valueVector3_ = value; }
    }

    public int ValueInt
    {
      get { return valueInt_; }
      set { valueInt_ = value; }
    }

    public CNField FieldBodies_B
    {
      get
      {
        if (fieldBodies_B_ == null)
        {
          fieldBodies_B_ = new CNField(false, CNFieldContentType.Bodies, false);
        }
        return fieldBodies_B_;
      }
    }

    public bool RequieresFieldController
    {
      get
      {
        return target_ == PARAMETER_MODIFIER_PROPERTY.COLLIDE_BY_PAIRS;
      }
    }

    public ParameterModifierCommand()
    {
      target_        = PARAMETER_MODIFIER_PROPERTY.ACTIVITY;
      valueVector3_  = new Vector3( 0.0f, 0.0f, 0.0f );
      valueInt_      = 0;
      fieldBodies_B_ = new CNField(false, CNFieldContentType.Bodies, false);
    }

    public ParameterModifierCommand(PARAMETER_MODIFIER_PROPERTY target)
    {
      target_        = target;
      valueVector3_  = new Vector3( 0.0f, 0.0f, 0.0f );
      valueInt_      = 0;
      fieldBodies_B_ = new CNField(false, CNFieldContentType.Bodies, false);
    }

    public ParameterModifierCommand DeepClone()
    {
      ParameterModifierCommand clone = new ParameterModifierCommand();

      clone.target_        = target_;
      clone.valueVector3_  = valueVector3_;
      clone.valueInt_      = valueInt_;                        
      clone.fieldBodies_B_ = FieldBodies_B.DeepClone();     
      return clone;
    }
  }

  /// <summary>
  /// Holds the data of a parameter modifier node.
  /// </summary>
  [AddComponentMenu("")]
  public class CNParameterModifier : CNEntity
  {   
 
    public override CNField Field
    {
      get
      {
        if (field_ == null)
        {
          CNFieldContentType allowedTypes =    CNFieldContentType.Bodies
                                             | CNFieldContentType.JointServosNode
                                             | CNFieldContentType.DaemonNode
                                             | CNFieldContentType.TriggerNode;

          field_ = new CNField(false, allowedTypes, false);
        }
        return field_;
      }
    }

    [SerializeField]
    private List<ParameterModifierCommand> listPmCommand_ = new List<ParameterModifierCommand>();
    public List<ParameterModifierCommand> ListPmCommand
    {
      get { return listPmCommand_; }
    }

    public override CNFieldContentType FieldContentType { get { return CNFieldContentType.ParameterModifierNode; } }

    public override void CloneData(CommandNode original)
    {
      base.CloneData(original);
      CNParameterModifier originalPM = (CNParameterModifier)original;
      
      listPmCommand_ = new List<ParameterModifierCommand>();
      foreach( ParameterModifierCommand pmCommand in originalPM.listPmCommand_ )
      {
        listPmCommand_.Add( pmCommand.DeepClone() );
      }
    }

  }
}
