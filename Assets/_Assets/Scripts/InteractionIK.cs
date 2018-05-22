using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionIK : MonoBehaviour
{
    Transform _target;

    Animator _ani;
    float _IKWeight;
    float _IKSpeed = 2f;
    bool _isIKActive = false;

    #region Events

    public Action OnShoot;
    public Action OnDie;

    #endregion

    private void Awake()
    {
        _ani = GetComponent<Animator>();
    }

    public void ActivateIK(Transform Target)
    {
        _isIKActive = true;
        _target = Target;
    }

    public void DisableIK()
    {
        _isIKActive = false;
    }

    private void Update()
    {

        if (_isIKActive)
        {
            _IKWeight = Mathf.Clamp01(_IKWeight + Time.deltaTime * _IKSpeed);
        }
        else
        {

            _IKWeight = Mathf.Clamp01(_IKWeight - Time.deltaTime * _IKSpeed);
        }
    }


    private void OnAnimatorIK(int layerIndex)
    {
        if (_target != null)
        {

            _ani.SetIKPosition(AvatarIKGoal.LeftHand, _target.transform.position);
            _ani.SetIKPositionWeight(AvatarIKGoal.LeftHand, _IKWeight);

            _ani.SetIKRotation(AvatarIKGoal.LeftHand, _target.rotation);
            _ani.SetIKRotationWeight(AvatarIKGoal.LeftHand, _IKWeight);

            _ani.SetLookAtPosition(_target.transform.position);
            _ani.SetLookAtWeight(_IKWeight);
        }
    }
}
