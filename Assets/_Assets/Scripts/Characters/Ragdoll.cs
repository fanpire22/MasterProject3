using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ragdoll : MonoBehaviour {

    [SerializeField] Transform _ragdollRootBone;

    Animator _animator;
    Rigidbody[] _bodyPartsRigidbodies;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _bodyPartsRigidbodies = _ragdollRootBone.GetComponentsInChildren<Rigidbody>();
    }
    
    public void ActivateRagdoll()
    {
        _animator.enabled = false;
        for(int i=0; i<_bodyPartsRigidbodies.Length; i++)
        {
            _bodyPartsRigidbodies[i].isKinematic = false;
        }        
    }

}
