 using System;
   
namespace CaronteFX
{ 
    [System.Serializable]
    [Flags]
    public enum CNFieldContentType
    {
      None                   = 0,
      Locator                = 1 << 1,
      Geometry               = 1 << 2,
      Animator               = 1 << 3,
      GroupNode              = 1 << 4,
      RigidBodyNode          = 1 << 5,
      IrresponsiveNode       = 1 << 6,
      SoftBodyNode           = 1 << 7,
      ClothBodyNode          = 1 << 8,
      RopeBodyNode           = 1 << 9,
      AnimatedBodyNode       = 1 << 10,
      MultiJointNode         = 1 << 11,
      ServosNode             = 1 << 12,
      GravityNode            = 1 << 13,
      ExplosionNode          = 1 << 14,
      ParameterModifierNode  = 1 << 15,
      SubstituterNode        = 1 << 16,
      ContactEmitterNode     = 1 << 17,
      TriggerByTimeNode      = 1 << 18,
      TriggerByContactNode   = 1 << 19,
      TriggerByPressureNode  = 1 << 20,
      TriggerByDetectorNode  = 1 << 21,
      TriggerByExplosionNode = 1 << 22,
      AimedForceNode         = 1 << 23,
      AimedFallNode          = 1 << 24,
      WindNode               = 1 << 25,
      SpeedLimiterNode       = 1 << 26,
      JetNode                = 1 << 27,
      AttractorNode          = 1 << 28,
      Corpuscles             = 1 << 29,

      GameObject            = Locator | Geometry | Animator,
      BodyNode              = RigidBodyNode | IrresponsiveNode | AnimatedBodyNode | SoftBodyNode | ClothBodyNode | RopeBodyNode,
      Bodies                = BodyNode | Geometry,
      JointServosNode       = MultiJointNode | ServosNode,
      DaemonNode            = GravityNode | ExplosionNode | AimedFallNode | WindNode | SpeedLimiterNode | JetNode | AttractorNode,
      TriggerNode           = TriggerByTimeNode | TriggerByContactNode | TriggerByPressureNode | TriggerByDetectorNode | TriggerByExplosionNode,
      UtilityNode           = ParameterModifierNode | SubstituterNode | ContactEmitterNode,
      EntityNode            = DaemonNode | TriggerNode | UtilityNode,
      All                   = ~None
    }
}