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
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;


using CaronteSharp;
using CaronteFX.Scripting;


namespace CaronteFX
{
  using Tuple_ParamaterModifierAction =  Tuple4< UN_BD_GENERIC_FAMILY, uint, ICarParameterEvaluator, ParameterModifierCommand>;

  public static class CarCallbacksManager
  {  
    static List<CarSimulationScript> listSimulationScript_ = new List<CarSimulationScript>(); 
    static bool skipDueToExcepction_ = false;

    static CaronteSharp.SimulationCallback startCB_; 
    static CaronteSharp.SimulationCallback updateCB_;

    public static CaronteSharp.PmCommand commandTmp_ = new CaronteSharp.PmCommand();
    public static List< Tuple_ParamaterModifierAction > listParameterModifierAction_ = new List< Tuple_ParamaterModifierAction >();

    public static void Init( List<CNScriptPlayerEditor> listSpEditor, List<Tuple_ParamaterModifierAction> listParameterModifierAction  )
    {
      ResetToDefaults();
      RegisterSimulationScripts(listSpEditor);
      AddScriptingCallbacks();

      RegisterParameterModiferActions(listParameterModifierAction);
      AddParameterCallbacks();

      RegisterCallbacksInCaronte();   
    }

    private static void RegisterCallbacksInCaronte()
    {
      if (startCB_ != null && updateCB_ != null)
      {
        SimulationManager.SetSimulationCallbacks(startCB_, updateCB_);
      }
    }

    private static void ResetToDefaults()
    {
      startCB_  = null;
      updateCB_ = null;

      skipDueToExcepction_ = false;

      listSimulationScript_.Clear();
      listParameterModifierAction_.Clear();
    }

    private static void AddCallbacks( CaronteSharp.SimulationCallback startCallback, CaronteSharp.SimulationCallback updateCallback )
    {
      if (startCB_ ==  null)
      {
        startCB_ =  startCallback;
      }
      else
      {
        startCB_ += startCallback;
      }

      if (updateCB_ == null)
      {
        updateCB_ = updateCallback;
      }
      else
      {
        updateCB_ += updateCB_;
      }
    }

    private static void RegisterSimulationScripts( List<CNScriptPlayerEditor> listSpEditor )
    {
      foreach( CNScriptPlayerEditor spEditor in listSpEditor )
      {
        if (spEditor.IsEnabledInHierarchy && !spEditor.IsExcludedInHierarchy)
        {
          spEditor.InitSimulationScriptObject();
        
          CarSimulationScript simScript = spEditor.GetSimulationScript();
          if (simScript != null)
          {
            listSimulationScript_.Add(simScript);
          }
        }
      }
    }

    private static void RegisterParameterModiferActions( List<Tuple_ParamaterModifierAction> listParameterModifierAction )
    {
      listParameterModifierAction_.AddRange(listParameterModifierAction);
    }

    private static void AddScriptingCallbacks()
    {
      if (listSimulationScript_.Count > 0)
      {
        AddCallbacks(SimulationStartScripts, SimulationUpdateScripts);
      }
    }

    private static void AddParameterCallbacks()
    {
      if (listParameterModifierAction_.Count > 0)
      { 
        AddCallbacks(SimulationStartParameters, SimulationUpdateParameters);
      }
    }


    private static void SimulationStartScripts()
    {
      if ( startCB_ != null && !skipDueToExcepction_ )
      {
        try
        {
          foreach(CarSimulationScript simulationScript in listSimulationScript_)
          {
            simulationScript.SimulationStart();
          }
        }
        catch (Exception e)
        {
          Debug.LogError("Exception happened in simulation start. Check simulation scripts. Details: ");
          Debug.LogError(e.Message);
          Debug.LogError(e.StackTrace);
          skipDueToExcepction_ = true;
          return;
        }
      }
    }

    private static void SimulationUpdateScripts()
    {
      if ( updateCB_ != null && !skipDueToExcepction_ )
      {
        try
        {
          foreach(CarSimulationScript simulationScript in listSimulationScript_)
          {
            simulationScript.SimulationUpdate();
          }
        }
        catch (Exception e)
        {
          Debug.LogError("Exception happened in simulation update. Check simulation scripts. Details: ");
          Debug.LogError(e.Message);
          Debug.LogError(e.StackTrace);
          skipDueToExcepction_ = true;
          return;
        }
      }
    }

    private static void SimulationStartParameters()
    {
      EvaluateParams( 0.0f );
    }

    private static void SimulationUpdateParameters()
    {
      EvaluateParams( CarGlobals.GetTimeSimulated() );
    }

    private static void EvaluateParams(float time)
    {
      int nParameter = listParameterModifierAction_.Count;

      for (int i = 0; i < nParameter; i++)
      {
        Tuple_ParamaterModifierAction pmAction = listParameterModifierAction_[i];

        ICarParameterEvaluator paramEvaluator = pmAction.Third;
        ParameterModifierCommand pmCommand    = pmAction.Fourth;

        paramEvaluator.Evaluate(time, pmCommand);

        CarDataUtils.SetParameterModifierCommand(ref commandTmp_, pmCommand, null);
        ParameterModifierManager.ModifyGenericParameter(pmAction.First, pmAction.Second, ref commandTmp_);
      }
    }
  }
}

