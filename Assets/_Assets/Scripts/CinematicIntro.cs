using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;

public class CinematicIntro : MonoBehaviour {

    [SerializeField] CinemachineVirtualCamera CMIntro;

    PlayableDirector _dir;
    NavMeshAgent _agent;

    private void Awake()
    {
        _dir = GetComponent<PlayableDirector>();
        _agent = GetComponent<NavMeshAgent>();
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(_dir.time >= _dir.duration-0.3f)
        {
            //Ha acabado la animación. Desactivamos la cámara, desactivamos el director, y activamos el NavMeshAgent del personaje
            _agent.enabled = true;
            _dir.enabled = false;
            CMIntro.enabled = false;
        }
	}
}
