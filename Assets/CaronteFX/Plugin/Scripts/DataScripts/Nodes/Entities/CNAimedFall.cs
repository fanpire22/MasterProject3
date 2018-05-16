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
  /// Contains the data of an aimed fall node.
  /// </summary>
  [AddComponentMenu("")] 
  public class CNAimedFall : CNEntity
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
    float speedRate_ = 0.5f;
    public float SpeedRate
    {
      get { return speedRate_; }
      set { speedRate_ = Mathf.Clamp(value, 0f, 1f); }
    }

    [SerializeField]
    float release_threshold_ = 0.5f;
    public float ReleaseThreshold
    {
      get { return release_threshold_; }
      set { release_threshold_ = Mathf.Clamp(value, 0f, 1f); }
    }

    public override CNFieldContentType FieldContentType { get { return CNFieldContentType.AimedFallNode; } }

    public override void CloneData(CommandNode original)
    {
      base.CloneData(original);

      CNAimedFall originalAf = (CNAimedFall)original;

      fieldAimGameObjects_ = originalAf.FieldAimGameObjects.DeepClone();

      speedRate_ = originalAf.speedRate_;
      release_threshold_ = originalAf.release_threshold_;
    }


    public override bool UpdateNodeReferences(Dictionary<CommandNode, CommandNode> dictNodeToClonedNode)
    {
      bool wasAnyUpdatedA = field_.UpdateNodeReferences(dictNodeToClonedNode);
      bool wasAnyUpdatedB = fieldAimGameObjects_.UpdateNodeReferences(dictNodeToClonedNode);

      return (wasAnyUpdatedA || wasAnyUpdatedB);
    }
  }

}//namespace CaronteFX