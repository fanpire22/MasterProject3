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
  /// Holds the data of a trigger by contact node.
  /// </summary>
  [AddComponentMenu("")]
  public class CNTriggerByForce : CNTrigger
  {
    [SerializeField]
    CNField fieldA_;
    public CNField FieldA
    {
      get
      {
        if (fieldA_ == null)
        {
          CNFieldContentType allowedTypes =     CNFieldContentType.Geometry
                                              | CNFieldContentType.BodyNode;
                      
          fieldA_ = new CNField( false, allowedTypes, false );
        }
        return fieldA_;
      }
    }

    [SerializeField]
    float forceMin_ = 30f;
    public float ForceMin
    {
      get { return forceMin_; }
      set { forceMin_ = value; }
    }

    [SerializeField]
    bool isForcePerMass_ = true;
    public bool IsForcePerMass
    {
      get { return isForcePerMass_; }
      set { isForcePerMass_ = value; }
    }

    [SerializeField]
    bool triggerForInvolvedBodies_ = false;
    public bool TriggerForInvolvedBodies
    {
      get { return triggerForInvolvedBodies_; }
      set { triggerForInvolvedBodies_ = value; }
    }

    public override CNFieldContentType FieldContentType { get { return CNFieldContentType.TriggerByPressureNode; } }

    public override void CloneData(CommandNode original)
    {
      base.CloneData(original);

      CNTriggerByForce originalTbf = (CNTriggerByForce) original;

      fieldA_ = originalTbf.FieldA.DeepClone();

      forceMin_                 = originalTbf.forceMin_;
      isForcePerMass_           = originalTbf.isForcePerMass_;
      triggerForInvolvedBodies_ = originalTbf.TriggerForInvolvedBodies;
    }

    public override bool UpdateNodeReferences(Dictionary<CommandNode, CommandNode> dictNodeToClonedNode)
    {
      bool updateEntityField = field_.UpdateNodeReferences(dictNodeToClonedNode);
      bool updateA = fieldA_.UpdateNodeReferences(dictNodeToClonedNode);

      return (updateEntityField || updateA);
    }

  }


}


