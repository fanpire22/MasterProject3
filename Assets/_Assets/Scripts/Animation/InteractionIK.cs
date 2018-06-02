using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionIK : MonoBehaviour
{
    Transform _targetRightHand;
    Transform _targetLeftHand;

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

    public void ActivateIK(Transform TargetLeft, Transform TargetRight)
    {
        _isIKActive = true;
        _targetLeftHand = TargetLeft;
        _targetRightHand = TargetRight;
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
        if (_targetLeftHand != null)
        {

            _ani.SetIKPosition(AvatarIKGoal.LeftHand, _targetLeftHand.transform.position);
            _ani.SetIKPositionWeight(AvatarIKGoal.LeftHand, _IKWeight);

            _ani.SetIKRotation(AvatarIKGoal.LeftHand, _targetLeftHand.rotation);
            _ani.SetIKRotationWeight(AvatarIKGoal.LeftHand, _IKWeight);

            _ani.SetLookAtPosition(_targetLeftHand.transform.position);
            _ani.SetLookAtWeight(_IKWeight);
        }

        if (_targetRightHand != null)
        {

            _ani.SetIKPosition(AvatarIKGoal.RightHand, _targetRightHand.transform.position);
            _ani.SetIKPositionWeight(AvatarIKGoal.RightHand, _IKWeight);

            _ani.SetIKRotation(AvatarIKGoal.RightHand, _targetLeftHand.rotation);
            _ani.SetIKRotationWeight(AvatarIKGoal.RightHand, _IKWeight);

            _ani.SetLookAtPosition(_targetRightHand.transform.position);
            _ani.SetLookAtWeight(_IKWeight);
        }
    }
}
