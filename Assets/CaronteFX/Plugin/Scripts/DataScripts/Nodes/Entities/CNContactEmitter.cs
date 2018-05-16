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
using System.Collections;
using System.Collections.Generic;


namespace CaronteFX
{
  /// <summary>
  /// Contacts the data of a contact emitter node. (Entity which emit contact events)
  /// </summary>
  [AddComponentMenu("")] 
  public class CNContactEmitter : CNEntity 
  {

    [SerializeField]
           CNField fieldA_;
    public CNField FieldA
    {
      get
      {
        if (fieldA_ == null)
        {
          CNFieldContentType allowedTypes =   CNFieldContentType.Geometry
                                            | CNFieldContentType.RigidBodyNode;
                      
          fieldA_ = new CNField( false, allowedTypes, false );
        }
        return fieldA_;
      }
    }

    [SerializeField]
           CNField fieldB_;
    public CNField FieldB
    {
      get
      {
        if (fieldB_ == null)
        {
          CNFieldContentType allowedTypes =  CNFieldContentType.Geometry
                                           | CNFieldContentType.RigidBodyNode;
                      
          fieldB_ = new CNField( false, allowedTypes, false );
        }
        return fieldB_;
      }
    }


    public enum EmitModeOption
    {
      OnlyFirst,
      All
    }

    [SerializeField]
    private EmitModeOption emitMode_ = EmitModeOption.All;
    public EmitModeOption EmitMode
    {
      get { return emitMode_; }
      set { emitMode_ = value; }
    }

    [SerializeField]
    private int maxEventsPerSecond_ = 100;
    public int MaxEventsPerSecond
    {
      get { return maxEventsPerSecond_; }
      set { maxEventsPerSecond_ = value; }
    }

    [SerializeField]
    private float relativeSpeedMin_N_ = 0.1f;
    public float RelativeSpeedMin_N
    {
      get { return relativeSpeedMin_N_; }
      set { relativeSpeedMin_N_ = value; }
    }

    [SerializeField]
    private float relativeSpeedMin_T_ = 0.0f;
    public float RelativeSpeedMin_T
    {
      get { return relativeSpeedMin_T_; }
      set { relativeSpeedMin_T_ = value; }
    }

    [SerializeField]
    private float relativeMomentum_N_ = 0f;
    public float RelativeMomentum_N
    {
      get { return relativeMomentum_N_; }
      set { relativeMomentum_N_ = value;}
    }

    [SerializeField]
    private float relativeMomentum_T_ = 0f;
    public float RelativeMomentum_T
    {
      get { return relativeMomentum_T_; }
      set { relativeMomentum_T_ = value; }
    }

    [SerializeField]
    private float lifeTimMinInSecs_ = 0.1f;
    public float LifeTimeMin
    {
      get { return lifeTimMinInSecs_; }
      set { lifeTimMinInSecs_ = value; }
    }

    [SerializeField]
    private float collapseRadius_ = 0f;
    public float CollapseRadius
    {
      get { return collapseRadius_; }
      set { collapseRadius_ = value; }
    }

    public override CNFieldContentType FieldContentType { get { return CNFieldContentType.ContactEmitterNode; } }

    //----------------------------------------------------------------------------------
    public override void CloneData(CommandNode original)
    {
      base.CloneData(original);
      CNContactEmitter originalCE = (CNContactEmitter)original;

      fieldA_ = originalCE.FieldA.DeepClone();
      fieldB_ = originalCE.FieldB.DeepClone();

      emitMode_           = originalCE.emitMode_;
      maxEventsPerSecond_ = originalCE.maxEventsPerSecond_;
      relativeSpeedMin_N_ = originalCE.relativeSpeedMin_N_;
      relativeSpeedMin_T_ = originalCE.relativeSpeedMin_T_;
      relativeMomentum_N_ = originalCE.relativeMomentum_N_;
      relativeMomentum_T_ = originalCE.relativeMomentum_T_;
      lifeTimMinInSecs_   = originalCE.lifeTimMinInSecs_;
      collapseRadius_     = originalCE.collapseRadius_;
    }
    //----------------------------------------------------------------------------------
    public override bool UpdateNodeReferences(Dictionary<CommandNode, CommandNode> dictNodeToClonedNode)
    {
      bool wasAnyUpdatedA = fieldA_.UpdateNodeReferences(dictNodeToClonedNode);
      bool wasAnyUpdatedB = fieldB_.UpdateNodeReferences(dictNodeToClonedNode);

      return (wasAnyUpdatedA || wasAnyUpdatedB);
    }
    //----------------------------------------------------------------------------------
  }
}
