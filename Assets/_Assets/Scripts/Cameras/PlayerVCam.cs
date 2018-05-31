using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public enum eVCamBehaviour
{
    Follow,
    Aim,
    Both
}

public class PlayerVCam : MonoBehaviour
{
    [SerializeField] eVCamBehaviour _behaviour=eVCamBehaviour.Follow;
    CinemachineVirtualCamera _vCam;

    private void Awake()
    {
        _vCam = GetComponent<CinemachineVirtualCamera>();
    }

    // Use this for initialization
    void Start()
    {
        switch (_behaviour)
        {
            case eVCamBehaviour.Follow:
                _vCam.Follow = GameManager.instance.player.transform;
                break;
            case eVCamBehaviour.Aim:
                _vCam.LookAt = GameManager.instance.player.transform;
                break;
            case eVCamBehaviour.Both:
                _vCam.LookAt = GameManager.instance.player.transform;
                _vCam.Follow = GameManager.instance.player.transform;
                break;
            default:
                _vCam.LookAt = GameManager.instance.player.transform;
                _vCam.Follow = GameManager.instance.player.transform;
                break;
        }
    }
}
