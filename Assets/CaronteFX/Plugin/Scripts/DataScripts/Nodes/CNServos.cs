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
using System.Linq;

namespace CaronteFX
{ 
  /// <summary>
  /// Holds the data of a node with contains sevos/motors definition
  /// </summary>
  [AddComponentMenu("")]
  public class CNServos : CommandNode
  {

    #region createParams
    [SerializeField]
    CNField objectsA_;
    public CNField ObjectsA
    {
      get
      {
        if ( objectsA_ == null )
        {
          objectsA_ = new CNField( false, CNFieldContentType.Geometry | CNFieldContentType.RigidBodyNode, 
                                   CNField.ScopeFlag.Inherited, false );
        }
        return objectsA_;
      }
    }

    [SerializeField]
    CNField objectsB_;
    public CNField ObjectsB
    {
      get
      {
        if ( objectsB_ == null )
        {
          objectsB_ = new CNField(false, CNFieldContentType.Geometry | CNFieldContentType.RigidBodyNode, 
                                   CNField.ScopeFlag.Inherited, false );
        }
        return objectsB_;
      }
    }

    [SerializeField]
    bool isLinearOrAngular_;
    public bool IsLinearOrAngular
    {
      get { return isLinearOrAngular_; }
      set { isLinearOrAngular_ = value; }
    }

    [SerializeField]
    bool isPositionOrVelocity_;
    public bool IsPositionOrVelocity
    {
      get { return isPositionOrVelocity_; }
      set { isPositionOrVelocity_ = value; }
    }
    
    [SerializeField]
    bool isCreateModeNearest_ = true;
    public bool IsCreateModeNearest
    {
      get { return isCreateModeNearest_; }
      set { isCreateModeNearest_ = value; }
    }

    [SerializeField]
    bool isCreateModeChain_ = false;
    public bool IsCreateModeChain
    {
      get { return isCreateModeChain_; }
      set { isCreateModeChain_ = value; }
    }

    public enum SV_LOCAL_SYSTEM
    {
      SV_LOCAL_SYSTEM_MYSELF_A,
      SV_LOCAL_SYSTEM_MYSELF_B
    };

    [SerializeField]
    SV_LOCAL_SYSTEM localSystem_ = SV_LOCAL_SYSTEM.SV_LOCAL_SYSTEM_MYSELF_B;
    public SV_LOCAL_SYSTEM LocalSystem
    {
      get { return localSystem_; }
      set { localSystem_ = value; }
    }

    [SerializeField]
    bool isFreeX_ = false;
    public bool IsFreeX
    {
      get { return isFreeX_; }
      set { isFreeX_ = value; }
    }

    [SerializeField]
    bool isFreeY_ = false;
    public bool IsFreeY
    {
      get { return isFreeY_; }
      set { isFreeY_ = value; }
    }

    [SerializeField]
    bool isFreeZ_ = false;
    public bool IsFreeZ
    {
      get { return isFreeZ_;}
      set { isFreeZ_ = value; }
    }

    [SerializeField]
    bool isBlockedX_ = false;
    public bool IsBlockedX
    {
      get { return isBlockedX_; }
      set { isBlockedX_ = value; }
    }

    [SerializeField]
    bool isBlockedY_ = false;
    public bool IsBlockedY
    {
      get { return isBlockedY_; }
      set { isBlockedY_ = value; }
    }

    [SerializeField]
    bool isBlockedZ_ = false;
    public bool IsBlockedZ
    {
      get { return isBlockedZ_; }
      set { isBlockedZ_ = value; }
    }

    [SerializeField]
    bool disableCollisionByPairs_ = false;
    public bool DisableCollisionByPairs
    {
      get { return disableCollisionByPairs_; }
      set { disableCollisionByPairs_ = value; }
    }

    [SerializeField]
    bool multiplierFoldOut_ = false;
    public bool MultiplierFoldOut
    {
      get { return multiplierFoldOut_; }
      set { multiplierFoldOut_ = value; }
    }

    [SerializeField]
    bool multiplierTimeFoldOut_ = true;
    public bool MultiplierTimeFoldOut
    {
      get { return multiplierTimeFoldOut_; }
      set { multiplierTimeFoldOut_ = value; }
    }

    [SerializeField]
    bool breakFoldOut_ = false;
    public bool BreakFoldout
    {
      get { return breakFoldOut_; }
      set { breakFoldOut_ = value; }
    }
    #endregion

    #region editParams

    [SerializeField]
    Vector3 targetExternal_LOCAL_ = Vector3.zero;
    public Vector3 TargetExternal_LOCAL
    {
      get { return targetExternal_LOCAL_; }
      set { targetExternal_LOCAL_ = value; }
    }

    [SerializeField]
    CarVector3Curve targetExternal_LOCAL_NEW_ = null;
    public CarVector3Curve TargetExternal_LOCAL_NEW
    {
      get { return targetExternal_LOCAL_NEW_; }
      set {targetExternal_LOCAL_NEW_ = value; }
    }

    [SerializeField]
    float reactionTime_ = 0.01f;
    public float ReactionTime
    {
      get { return reactionTime_; }
      set { reactionTime_ = Mathf.Clamp(value, 0f, float.MaxValue); }
    }

    [SerializeField]
    float overreactionDelta_ = 0.01f;
    public float OverreactionDelta
    {
      get { return overreactionDelta_; }
      set { overreactionDelta_ = Mathf.Clamp(value, 0f, float.MaxValue); }
    }

    [SerializeField]
    float speedMax_ = 20.0f;
    public float SpeedMax
    {
      get { return speedMax_; }
      set { speedMax_ = value; }
    }

    [SerializeField]
    bool maximumSpeed_ = true;
    public bool MaximumSpeed
    {
      get { return maximumSpeed_; }
      set { maximumSpeed_ = value; }
    }

    [SerializeField]
    float powerMax_ = 1000.0f;
    public float PowerMax
    {
      get { return powerMax_; }
      set { powerMax_ = value; }
    }

    [SerializeField]
    bool maximumPower_ = true;
    public bool MaximumPower
    {
      get { return maximumPower_; }
      set { maximumPower_ = value; }
    }

    [SerializeField]
    float forceMax_ = 1000.0f;
    public float ForceMax
    {
      get { return forceMax_; }
      set { forceMax_ = value; }
    }

    [SerializeField]
    bool maximumForce_ = true;
    public bool MaximumForce
    {
      get { return maximumForce_; }
      set { maximumForce_ = value; }
    }

    [SerializeField]
    float brakePowerMax_ = 5000.0f;
    public float BrakePowerMax
    {
      get { return brakePowerMax_; }
      set { brakePowerMax_ = value; }
    }

    [SerializeField]
    bool maximumBrakePowerMax_ = true;
    public bool MaximumBrakePowerMax
    {
      get { return maximumBrakePowerMax_; }
      set { maximumBrakePowerMax_ = value; }
    }

    [SerializeField]
    float brakeForceMax_ = 5000.0f;
    public float BrakeForceMax
    {
      get { return brakeForceMax_; }
      set { brakeForceMax_ = value; }
    }

    [SerializeField]
    bool maximumBrakeForceMax_ = true;
    public bool MaximumBrakeForceMax
    {
      get { return maximumBrakeForceMax_; }
      set { maximumBrakeForceMax_ = value; }
    }

    [SerializeField]
    bool isBreakIfDist_ = false;
    public bool IsBreakIfDist
    {
      get { return isBreakIfDist_; }
      set { isBreakIfDist_ = value; }
    }

    [SerializeField]
    bool isBreakIfAng_ = false;
    public bool IsBreakIfAng
    {
      get { return isBreakIfAng_; }
      set { isBreakIfAng_ = value; }
    }

    [SerializeField]
    float breakDistance_ = 1.0f;
    public float BreakDistance
    {
      get { return breakDistance_; }
      set { breakDistance_ = Mathf.Clamp(value, 0f, float.MaxValue); }
    }

    [SerializeField]
    float breakAngleInDegrees_ = 1.0f;
    public float BreakAngleInDegrees
    {
      get { return breakAngleInDegrees_; }
      set { breakAngleInDegrees_ = Mathf.Clamp(value, 0f, float.MaxValue); }
    }

    [SerializeField]
    float dampingForce_ = 0.0f;
    public float DampingForce
    {
      get { return dampingForce_; }
      set { dampingForce_ = Mathf.Clamp(value, 0f, float.MaxValue); }
    }

    [SerializeField]
    float distStepToDefineMultiplierDependingOnDist_ = 1.0f;
    public float DistStepToDefineMultiplierDependingOnDist
    {
      get { return distStepToDefineMultiplierDependingOnDist_; }
      set { distStepToDefineMultiplierDependingOnDist_ = value; }
    }

    [SerializeField]
    float multiplierRange_ = 1.0f;
    public float MultiplierRange
    {
      get { return multiplierRange_; }
      set { multiplierRange_ = value; }
    }

    [SerializeField]
    AnimationCurve functionMultiplierDependingOnDist_ = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    public AnimationCurve FunctionMultiplierDependingOnDist
    {
      get { return functionMultiplierDependingOnDist_; }
      set { functionMultiplierDependingOnDist_ = value; }
    }

    [SerializeField]
    float multiplier_ = 1.0f;
    public float Multiplier
    {
      get { return multiplier_; }
      set { multiplier_ = value; }
    }

    [SerializeField]
    CarFloatCurve multiplier_Curve_ = null;
    public CarFloatCurve MultiplierCurve
    {
      get { return multiplier_Curve_; }
      set { multiplier_Curve_ = value; }
    } 

    [SerializeField]
    float angularLimit_ = 180.0f;
    public float AngularLimit
    {
      get { return angularLimit_; }
      set { angularLimit_ = value; }
    }

    [SerializeField]
    bool maximumAngle_ = true;
    public bool MaximumAngle
    {
      get { return maximumAngle_; }
      set { maximumAngle_ = value; }
    }
    #endregion

    public override CNFieldContentType FieldContentType { get { return CNFieldContentType.ServosNode; } }

    public override void CloneData(CommandNode original)
    {
      base.CloneData(original);

      CNServos originalSv = (CNServos)original;

      objectsA_  = originalSv.ObjectsA.DeepClone();
      objectsB_  = originalSv.ObjectsB.DeepClone();

      isLinearOrAngular_ = originalSv.isLinearOrAngular_;

      isPositionOrVelocity_ = originalSv.isPositionOrVelocity_ ;
      isCreateModeNearest_  = originalSv.isCreateModeNearest_;
      isCreateModeChain_    = originalSv.isCreateModeChain_;

      localSystem_ = originalSv.localSystem_;

      isFreeX_ = originalSv.isFreeX_;
      isFreeY_ = originalSv.isFreeY_;
      isFreeZ_ = originalSv.isFreeZ_;

      isBlockedX_ = originalSv.isBlockedX_;
      isBlockedY_ = originalSv.isBlockedY_;
      isBlockedZ_ = originalSv.isBlockedZ_;

      disableCollisionByPairs_ = originalSv.disableCollisionByPairs_;

      targetExternal_LOCAL_ = originalSv.targetExternal_LOCAL_;

      if (originalSv.targetExternal_LOCAL_NEW_ != null)
      {
        targetExternal_LOCAL_NEW_ = originalSv.targetExternal_LOCAL_NEW_.DeepClone();
      }

      reactionTime_ = originalSv.reactionTime_;
      overreactionDelta_ = originalSv.overreactionDelta_;;

      angularLimit_ = originalSv.angularLimit_;
      maximumAngle_ = originalSv.maximumAngle_;
      speedMax_     = originalSv.speedMax_;
      maximumSpeed_ = originalSv.maximumSpeed_;
      powerMax_     = originalSv.powerMax_;
      maximumPower_ = originalSv.maximumPower_;
      forceMax_     = originalSv.forceMax_;
      maximumForce_ = originalSv.maximumForce_;

      brakePowerMax_        = originalSv.brakePowerMax_;
      maximumBrakePowerMax_ = originalSv.maximumBrakePowerMax_;
      brakeForceMax_        = originalSv.brakeForceMax_;
      maximumBrakeForceMax_ = originalSv.maximumBrakeForceMax_;

      isBreakIfDist_ = originalSv.isBreakIfDist_;
      isBreakIfAng_  = originalSv.isBreakIfAng_;

      breakDistance_       = originalSv.breakDistance_;
      breakAngleInDegrees_ = originalSv.breakAngleInDegrees_;

      dampingForce_ = originalSv.dampingForce_;

      distStepToDefineMultiplierDependingOnDist_ = originalSv.distStepToDefineMultiplierDependingOnDist_;
      functionMultiplierDependingOnDist_ = originalSv.functionMultiplierDependingOnDist_.DeepClone();

      multiplier_ = originalSv.multiplier_;

      if (originalSv.multiplier_Curve_ != null)
      {
        multiplier_Curve_ = originalSv.multiplier_Curve_.DeepClone();
      }
    }

    public override bool UpdateNodeReferences(Dictionary<CommandNode, CommandNode> dictNodeToClonedNode)
    {
      bool updatedA = objectsA_.UpdateNodeReferences(dictNodeToClonedNode);
      bool updatedB = objectsB_.UpdateNodeReferences(dictNodeToClonedNode);

      return (updatedA || updatedB);
    }

  }
}
