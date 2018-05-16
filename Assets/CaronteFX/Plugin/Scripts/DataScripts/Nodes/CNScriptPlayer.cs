// ***********************************************************
//	Copyright 2016 Next Limit Technologies, http://www.nextlimit.com
//	All rights reserved.
//
//	THIS SOFTWARE IS PROVIDED 'AS IS' AND WITHOUT ANY EXPRESS OR
//	IMPLIED WARRANTIES, INCLUDING, WITHOUT LIMITATION, THE IMPLIED
//	WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.
//
// ***********************************************************

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaronteFX
{
  /// <summary>
  /// Holds the data of a node with contains a simulation script
  /// </summary>
  [AddComponentMenu("")]
  public class CNScriptPlayer : CNMonoField
  { 
    public override CNField Field
    {
      get
      {
        if (field_ == null)
        {
          field_ = new CNField(true, false);
        }
        return field_;
      }
    }

    [SerializeField]
    UnityEngine.ScriptableObject simulationScript_;
    public UnityEngine.ScriptableObject SimulationScript
    {
      get { return simulationScript_; }
      set { simulationScript_ = value; }
    }

    public override CNFieldContentType FieldContentType { get { return CNFieldContentType.None; } }

    //-----------------------------------------------------------------------------------
    public override void CloneData(CommandNode node)
    {
      base.CloneData(node);

      CNScriptPlayer originalSp = (CNScriptPlayer)node;
      simulationScript_ = originalSp.simulationScript_;
    }
    //-----------------------------------------------------------------------------------
  }
}