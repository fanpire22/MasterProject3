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
  /// Contains the data of and AimedForce node
  /// </summary>
  [AddComponentMenu("")] 
  public class CNAimedForce : CNEntity
  {
    public override CNField Field 
    { 
      get
      {
        if (field_ == null)
        {
          CNFieldContentType allowedTypes =   CNFieldContentType.Geometry 
                                            | CNFieldContentType.BodyNode;

          field_ = new CNField( false, allowedTypes, false );
        }
        return field_;
      }
    }

    [SerializeField]
           CNField fieldAimGameObjects_;
    public CNField FieldAimGameObjects
    {
      get
      {
        if (fieldAimGameObjects_ == null)
        {
          CNFieldContentType allowedTypes = CNFieldContentType.GameObject;
                      
          fieldAimGameObjects_ = new CNField( false, allowedTypes, false );
        }
        return fieldAimGameObjects_;
      }
    }

    [SerializeField]
    float timeDuration_ = 1.0f;
    public float TimeDuration
    {
      get { return timeDuration_; }
      set { timeDuration_ = Mathf.Clamp(value, 0, float.MaxValue); }
    }

    [SerializeField]
    float multiplier_r_ = 1.0f;
    public float Multiplier_r
    {
      get { return multiplier_r_; }
      set { multiplier_r_ = Mathf.Clamp(value, 0, float.MaxValue); }
    }

    [SerializeField]
    float multiplier_q_ = 1.0f;
    public float Multiplier_q
    {
      get { return multiplier_q_; }
      set { multiplier_q_ = Mathf.Clamp(value, 0, float.MaxValue); }
    }

    public override CNFieldContentType FieldContentType { get { return CNFieldContentType.AimedForceNode; } }

    public override void CloneData(CommandNode original)
    {
      base.CloneData(original);
      CNAimedForce originalAf = (CNAimedForce)original;

      fieldAimGameObjects_ = originalAf.fieldAimGameObjects_.DeepClone();

      timeDuration_ = originalAf.timeDuration_;
      multiplier_r_ = originalAf.multiplier_r_;
      multiplier_q_ = originalAf.multiplier_q_;
    }

    public override bool UpdateNodeReferences(Dictionary<CommandNode, CommandNode> dictNodeToClonedNode)
    {
      bool wasAnyUpdatedA = field_.UpdateNodeReferences(dictNodeToClonedNode);
      bool wasAnyUpdatedB = fieldAimGameObjects_.UpdateNodeReferences(dictNodeToClonedNode);

      return (wasAnyUpdatedA || wasAnyUpdatedB);
    }
  }

}//namespace CaronteFX...
