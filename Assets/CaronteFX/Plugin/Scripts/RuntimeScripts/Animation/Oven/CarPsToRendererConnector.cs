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

namespace CaronteFX
{
  [RequireComponent(typeof (ParticleSystem), typeof(CarCorpusclesRenderer) )]
  [AddComponentMenu("")]
  public class CarPsToRendererConnector : MonoBehaviour
  {
    public int currentNumberOfParticles;
    
    private CarCorpusclesRenderer corpusclesRenderer_;
    private ParticleSystem pSystem_;
    private ParticleSystem.Particle[] arrParticle_;
    private Vector3[] arrParticlePosition_;

	  void Start ()
    {
      pSystem_ = GetComponent<ParticleSystem>();
      corpusclesRenderer_ = GetComponent<CarCorpusclesRenderer>();

		  if (pSystem_ != null && corpusclesRenderer_ != null)
      {
#if UNITY_5_5_OR_NEWER
        int maxParticles = pSystem_.main.maxParticles;
#else
        int maxParticles = pSystem_.maxParticles;
#endif
        arrParticle_ = new ParticleSystem.Particle[maxParticles];
        arrParticlePosition_ = new Vector3[maxParticles];

        corpusclesRenderer_.Init(maxParticles, 1.0f);
      }
      else
      {
        enabled = false;
      }

	  }
	
	  void OnRenderObject ()
    {
		  currentNumberOfParticles = pSystem_.GetParticles(arrParticle_);

      for (int i = 0; i < currentNumberOfParticles; i++)
      {
          arrParticlePosition_[i] = arrParticle_[i].position;
      }

      corpusclesRenderer_.RebindShaderAttributes();
      corpusclesRenderer_.SetCorpusclesPositions( currentNumberOfParticles, arrParticlePosition_ );
	  }
  }

}

