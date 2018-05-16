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
  /// Holds the data of a trigger by explosion node.
  /// </summary>
  [AddComponentMenu("")]
  public class CNTriggerByExplosion : CNTrigger
  {
    [SerializeField]
           CNField fieldExplosions_;
    public CNField FieldExplosions
    {
      get
      {
        if (fieldExplosions_ == null)
        {
          CNFieldContentType allowedTypes = CNFieldContentType.ExplosionNode;
                      
          fieldExplosions_ = new CNField( false, allowedTypes, false );
        }
        return fieldExplosions_;
      }
    }

    [SerializeField]
           CNField fieldBodies_;
    public CNField FieldBodies
    {
      get
      {
        if (fieldBodies_ == null)
        {
          CNFieldContentType allowedTypes =   CNFieldContentType.Geometry
                                            | CNFieldContentType.BodyNode;
                      
          fieldBodies_ = new CNField( false, allowedTypes, false );
        }
        return fieldBodies_;
      }
    }

    public override CNFieldContentType FieldContentType { get { return CNFieldContentType.TriggerByExplosionNode; } }

    public override void CloneData(CommandNode original)
    {
      base.CloneData(original);

      CNTriggerByExplosion originalTbe = (CNTriggerByExplosion)original;

      fieldExplosions_ = originalTbe.FieldExplosions.DeepClone();
      fieldBodies_     = originalTbe.FieldBodies.DeepClone();
    }

    public override bool UpdateNodeReferences(Dictionary<CommandNode, CommandNode> dictNodeToClonedNode)
    {
      bool updateEntities  = Field.UpdateNodeReferences(dictNodeToClonedNode);
      bool updateExplosion = FieldExplosions.UpdateNodeReferences(dictNodeToClonedNode);
      bool updateBodies    = FieldBodies.UpdateNodeReferences(dictNodeToClonedNode);

      return (updateEntities || updateExplosion || updateBodies);
    }
  }
}//namespace CaronteFX


