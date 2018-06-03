using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;

public class LastCinematic : MonoBehaviour
{
    [SerializeField] Floor _floor;
    Vector3 AnimationLocation = new Vector3(99.8f, 0, 156f);
    bool _onAnimate = false;
    CinemachineVirtualCamera _cam;

    private void Awake()
    {
        _cam = GetComponentInChildren<CinemachineVirtualCamera>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Movemos al Jugador al punto de inicio de la cinemática, y desactivamos la opción de que el jugador pueda cambiar de dirección
            _floor.enabled = false;
            GameManager.instance.player.GetComponent<NavMeshAgent>().SetDestination(AnimationLocation);
            _onAnimate = true;
            _cam.enabled = true;
        }
    }

    void Update()
    {
        if (_onAnimate)
        {
            //Ahora sí, empezamos la animación
            _floor.enabled = false;
            GameManager.instance.player.transform.rotation = Quaternion.Euler(0, 90, 0);
        }
    }
}
