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

namespace CaronteFX
{
  public static class CarDataUtils
  {
    
    public static void GetCaronteFxGameObjects(UnityEngine.Object[] arrObjectReference, out List<GameObject> listGameObject)
    {
      listGameObject = new List<GameObject>();
      int arrObjectReference_size = arrObjectReference.Length;

      for (int i = 0; i < arrObjectReference_size; i++)
      {
        GameObject go = arrObjectReference[i] as GameObject;
        if (go != null)
        {
          if (go.IsInScene())
          {
            if (go.GetComponent<Caronte_Fx>() != null)
            {
              listGameObject.Add(go);
            }
            GameObject[] arrGameObject = CarEditorUtils.GetAllChildObjects(go, true);
            foreach (GameObject childGo in arrGameObject)
            {
              if (childGo.GetComponent<Caronte_Fx>() != null)
              {
                listGameObject.Add(childGo);
              }
            }
          }
        }
      }
    }

    public static void GetCaronteFxsRelations(Caronte_Fx caronteFx, out List<Tuple2<Caronte_Fx, int>> listCaronteFx )
    {
      listCaronteFx = new List<Tuple2<Caronte_Fx, int>>();
      GameObject go = caronteFx.gameObject;
      if ( go.IsInScene() )
      {     
        GameObject[] arrChild = CarEditorUtils.GetAllGameObjectsInScene();
        AddRelations( go, arrChild, listCaronteFx );
      }
    }

    public static void AddRelations(GameObject parentFx, GameObject[] arrGameObject, List<Tuple2<Caronte_Fx, int>> listCaronteFx)
    {
      for (int i = 0; i < arrGameObject.Length; i++)
      {
        GameObject go = arrGameObject[i];
        Caronte_Fx fxChild = go.GetComponent<Caronte_Fx>();
        if (fxChild != null)
        {
          int depth = go.GetFxHierachyDepthFrom(parentFx);
          listCaronteFx.Add(Tuple2.New(fxChild, depth));
        }
      }
    }

    //----------------------------------------------------------------------------------
    public static bool CheckDisplayEvaluationMessage(Caronte_Fx fxData)
    {
      if ( CarVersionChecker.IsEvaluationVersion() )
      {
        string companyName = CarVersionChecker.CompanyName;

        if ( fxData.FirstUse )
        {
          fxData.FirstUse = false;
          EditorUtility.SetDirty(fxData);

          if (companyName == string.Empty)
          {
            EditorUtility.DisplayDialog("CaronteFX - Info", "Evaluation version.\n\nUse for evaluation purposes only. Any commercial use, copying, or redistribution of this plugin is strictly forbidden.", "Ok");
          }
          else
          {
            EditorUtility.DisplayDialog("CaronteFX - Info", "Evaluation version. Only for " +  companyName + " internal use." + " \n\nUse for evaluation purposes only. Any commercial use, copying, or redistribution of this plugin is strictly forbidden.", "Ok");
          }  
        }
      }

      if (!CarVersionChecker.IsVersionPeriodActive())
      {
        EditorUtility.DisplayDialog("CaronteFX - Info", "The working period of this version has expired, you are not allowed to use this version of CaronteFX anymore.\n", "Ok");
        return false;
      }

      return true;
    }
    //----------------------------------------------------------------------------------
    public static void UpdateFxDataVersionIfNeeded(Caronte_Fx fxData)
    {
      GameObject dataHolder = fxData.GetDataGameObject();
      if (fxData.DataVersion < 1)
      {
        CNBody[] arrBodyNode = dataHolder.GetComponents<CNBody>();
        
        foreach(CNBody bodyNode in arrBodyNode)
        {
          bodyNode.OmegaStart_inDegSeg *= Mathf.Rad2Deg;
          EditorUtility.SetDirty(bodyNode);
        }

        fxData.DataVersion = 1;
        EditorUtility.SetDirty(fxData);
        CarDebug.Log("Updated " + fxData.name + " definitions to version 1.");
      }
      if (fxData.DataVersion < 2)
      {
        CNJointGroups[] arrJgGroups = dataHolder.GetComponents<CNJointGroups>();
        foreach(CNJointGroups jgGroups in arrJgGroups)
        {
          jgGroups.ContactAngleMaxInDegrees *= Mathf.Rad2Deg;
          if (jgGroups.ContactAngleMaxInDegrees > 180f ||
              jgGroups.ContactAngleMaxInDegrees < 0f      )
          {
            jgGroups.ContactAngleMaxInDegrees -= (jgGroups.ContactAngleMaxInDegrees % 180f) * 180f;
            EditorUtility.SetDirty(jgGroups);
          }
        }
        fxData.DataVersion = 2;
        EditorUtility.SetDirty(fxData);
        CarDebug.Log("Updated " + fxData.name + " definitions to version 2.");
      }
      if (fxData.DataVersion < 3)
      {
        CNSoftbody[] arrSoftbodyNode = dataHolder.GetComponents<CNSoftbody>();
        foreach(CNSoftbody sbNode in arrSoftbodyNode)
        {
          sbNode.LengthStiffness = Mathf.Clamp(sbNode.LengthStiffness, 0f, 30f);
          EditorUtility.SetDirty(sbNode);
        }
        fxData.DataVersion = 3;
        EditorUtility.SetDirty(fxData);
        CarDebug.Log("Updated " + fxData.name + " definitions to version 3.");
      }
      if (fxData.DataVersion < 4)
      {
        CNFracture[] arrFractureNode = dataHolder.GetComponents<CNFracture>();
        foreach(CNFracture frNode in arrFractureNode)
        {
          if (frNode.ChopGeometry != null)
          {
            frNode.FieldSteeringGeometry.GameObjects.Add(frNode.ChopGeometry);
            EditorUtility.SetDirty(frNode);
          }
          if (frNode.CropGeometry != null)
          {
            frNode.FieldRestrictionGeometry.GameObjects.Add(frNode.CropGeometry);
            EditorUtility.SetDirty(frNode);
          }       
        }
        fxData.DataVersion = 4;
        EditorUtility.SetDirty(fxData);
        CarDebug.Log("Updated " + fxData.name + " definitions to version 4.");
      }
      if (fxData.DataVersion < 5)
      {
        CNServos[] arrServos = dataHolder.GetComponents<CNServos>();
        foreach(CNServos svNode in arrServos)
        {
          svNode.TargetExternal_LOCAL_NEW = new CarVector3Curve(svNode.TargetExternal_LOCAL, fxData.effect.totalTime_);
          EditorUtility.SetDirty(svNode);
        }
        fxData.DataVersion = 5;
        EditorUtility.SetDirty(fxData);
        CarDebug.Log("Updated " + fxData.name + " definitions to version 5.");
      }
    }

    public static void SetParameterModifierCommand(ref CaronteSharp.PmCommand pmCommand, ParameterModifierCommand parameterModifierCommand, List<uint> listCommandIdBody)
    {
      pmCommand.arrBodyId_B_ = null;

      switch ( parameterModifierCommand.Target )
      {
        case PARAMETER_MODIFIER_PROPERTY.VELOCITY_LINEAL:
          pmCommand.target_ = CaronteSharp.PARAMETER_MODIFIER_PROPERTY.VELOCITY_LINEAL;
          pmCommand.valueVector3_ = parameterModifierCommand.ValueVector3;
        break;

        case PARAMETER_MODIFIER_PROPERTY.VELOCITY_ANGULAR:
          pmCommand.target_ = CaronteSharp.PARAMETER_MODIFIER_PROPERTY.VELOCITY_ANGULAR;
          pmCommand.valueVector3_ = parameterModifierCommand.ValueVector3;
          break;

        case PARAMETER_MODIFIER_PROPERTY.ACTIVITY:
          pmCommand.target_ = CaronteSharp.PARAMETER_MODIFIER_PROPERTY.ACTIVITY;
          pmCommand.valueIndex_ = (uint)parameterModifierCommand.ValueInt;
          break;

        case PARAMETER_MODIFIER_PROPERTY.VISIBILITY:
          pmCommand.target_ = CaronteSharp.PARAMETER_MODIFIER_PROPERTY.VISIBILITY;
          pmCommand.valueIndex_ = (uint)parameterModifierCommand.ValueInt;
          break;

        case PARAMETER_MODIFIER_PROPERTY.FORCE_MULTIPLIER:
          pmCommand.target_ = CaronteSharp.PARAMETER_MODIFIER_PROPERTY.FORCE_MULTIPLIER;
          pmCommand.valueVector3_ = parameterModifierCommand.ValueVector3;
          break;

        case PARAMETER_MODIFIER_PROPERTY.EXTERNAL_DAMPING:
          pmCommand.target_ = CaronteSharp.PARAMETER_MODIFIER_PROPERTY.EXTERNAL_DAMPING;
          pmCommand.valueVector3_ = parameterModifierCommand.ValueVector3;
          break;

        case PARAMETER_MODIFIER_PROPERTY.FREEZE:
          pmCommand.target_ = CaronteSharp.PARAMETER_MODIFIER_PROPERTY.FREEZE;
          pmCommand.valueIndex_ = (uint)parameterModifierCommand.ValueInt;
          break;

        case PARAMETER_MODIFIER_PROPERTY.PLASTICITY:
          pmCommand.target_ = CaronteSharp.PARAMETER_MODIFIER_PROPERTY.PLASTICITY;
          pmCommand.valueIndex_ = (uint)parameterModifierCommand.ValueInt;
          break;

        case PARAMETER_MODIFIER_PROPERTY.TARGET_POSITION:
          pmCommand.target_ = CaronteSharp.PARAMETER_MODIFIER_PROPERTY.TARGET_POSITION;
          pmCommand.valueVector3_ = parameterModifierCommand.ValueVector3;
          break;

        case PARAMETER_MODIFIER_PROPERTY.TARGET_ANGLE:
          pmCommand.target_ = CaronteSharp.PARAMETER_MODIFIER_PROPERTY.TARGET_ANGLE;
          pmCommand.valueVector3_ = parameterModifierCommand.ValueVector3;
          break;

        case PARAMETER_MODIFIER_PROPERTY.COLLIDE_WITH_ALL:
          pmCommand.target_ = CaronteSharp.PARAMETER_MODIFIER_PROPERTY.COLLIDE_WITH_ALL;
          pmCommand.valueIndex_ = (uint)parameterModifierCommand.ValueInt;
          break;

        case PARAMETER_MODIFIER_PROPERTY.COLLIDE_BY_PAIRS:
          pmCommand.target_ = CaronteSharp.PARAMETER_MODIFIER_PROPERTY.COLLIDE_BY_PAIRS;
          pmCommand.valueIndex_ = (uint)parameterModifierCommand.ValueInt;
          pmCommand.arrBodyId_B_ = listCommandIdBody.ToArray();
          break;

        case PARAMETER_MODIFIER_PROPERTY.COLLIDE_AUTOCOLLISION:
          pmCommand.target_ = CaronteSharp.PARAMETER_MODIFIER_PROPERTY.COLLIDE_AUTOCOLLISION;
          pmCommand.valueIndex_ = (uint)parameterModifierCommand.ValueInt;
          break;

        default:
          throw new NotImplementedException();
      }
    }

    //----------------------------------------------------------------------------------
    public static SimulationParams GetSimulationParams(CREffectData effectData)
    {
      SimulationParams simParams = new SimulationParams();

      simParams.nThreads_ = (uint) SystemInfo.processorCount - 1;

      if (effectData.quality_ < 50 )
      {
        float oldMax = 50;
        float oldMin = 1;

        float newMax = 50;
        float newMin = 20;

        float q = effectData.quality_;

        float oldRange = (oldMax - oldMin);  
        float newRange = (newMax - newMin);  
        float newQ = (( (q - oldMin) * newRange) / oldRange) + newMin; 

        simParams.qualityRq_0_100_  = newQ;
      }
      else
      {
        simParams.qualityRq_0_100_  = effectData.quality_;
      }
      
      simParams.jitterZapperRq_0_100_ = effectData.antiJittering_;

      simParams.totalTime_ = (effectData.totalTime_ > 0.0f) ? effectData.totalTime_ : 3600.0f;
      simParams.fps_       = (effectData.frameRate_ > 0) ? (uint)effectData.frameRate_ : 30;

      simParams.isUnTimeStepFixed_ = false;
      simParams.unTimeStepFixed_   = Time.fixedDeltaTime;

      simParams.isNormalized_deltaTime_ = (!effectData.byUserDeltaTime_) || 
                                        (effectData.deltaTime_ == -1);

      simParams.deltaTime_ = (effectData.deltaTime_ > 0.0f) ? effectData.deltaTime_ : 0.000001f;

      simParams.isByUser_distCharacteristic_ = (effectData.byUserCharacteristicObjectProperties_)  && 
                                               (effectData.thickness_ != -1) &&
                                               (effectData.length_    != -1);

      simParams.byUser_distCharacteristicThickness_ = effectData.thickness_;
      simParams.byUser_distCharacteristicLength_    = effectData.length_;

      simParams.doSleeping_               = effectData.doSleeping_;
      simParams.doBullet_                 = effectData.doBullet_;
      simParams.doHit_                    = effectData.doHit_;
      simParams.doRgBinaryPLConservation_ = effectData.doRgBinaryPLConservation_;    

      return simParams;
    }
  }
}
