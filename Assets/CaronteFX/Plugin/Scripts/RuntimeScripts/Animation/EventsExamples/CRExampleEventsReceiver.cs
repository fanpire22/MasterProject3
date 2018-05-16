using UnityEngine;


namespace CaronteFX
{
  [AddComponentMenu("")]
  public abstract class CRExampleEventsReceiver : MonoBehaviour
  { 
    public abstract void ProcessAnimationEvent(CRAnimationEvData aeData);
  }
}


