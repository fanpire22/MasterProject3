using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class VCamTrigger : MonoBehaviour {

    CinemachineVirtualCamera _vCam;

    private void Awake()
    {
        _vCam = GetComponentInChildren<CinemachineVirtualCamera>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            _vCam.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _vCam.enabled = false;
        }
    }

    public void DisableMe()
    {
        _vCam.enabled = false;
        Destroy(this);
    }
    
}
