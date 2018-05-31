using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;

[System.Serializable]public struct Conversacion
{
    public string Texto;
    public float Duracion;
}

public class CinematicIntro : MonoBehaviour {

    [SerializeField] CinemachineVirtualCamera CMIntro;
	[SerializeField] CinemachineVirtualCamera CMICE;
	[SerializeField] CinemachineVirtualCamera CMBlackICE;
	public Conversacion[] Conversaciones;

    PlayableDirector _dir;
    NavMeshAgent _agent;
    float waitConvoTime;
    int _iConvo;
	AI2Animator _anim;
	bool _bConvoStart = false;

	private void Awake()
    {
        _dir = GetComponent<PlayableDirector>();
        _agent = GetComponent<NavMeshAgent>();
		_anim = GetComponent<AI2Animator>();
	}

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (!_bConvoStart)
		{
			if (_dir.time >= _dir.duration - 0.3f)
			{
				_bConvoStart = true;
			}
		}
		else
		{
			//Iniciamos la conversacion
			Conversacion();
		}
	}

    void Conversacion()
    {
        if(Time.time > waitConvoTime)
        {
            GameManager.instance.UI.Talk(Conversaciones[_iConvo].Texto, Conversaciones[_iConvo].Duracion);
            waitConvoTime = Time.time + Conversaciones[_iConvo].Duracion;

			if(_iConvo == 4)
			{
				//Estamos mirando a un ICE
				CMICE.Priority = 40;
			}

			if (_iConvo ==6)
			{
				//Estamos mirando a un BlackICE
				CMICE.Priority = 10;
				CMBlackICE.Priority = 40;
			}

			_iConvo++;
            if(_iConvo >= Conversaciones.Length)
            {
				//Ha acabado la intro. Desactivamos la cámara, desactivamos el director, y activamos el NavMeshAgent del personaje
				GameManager.instance.UI.ShutUp();
                _agent.enabled = true;
				_anim.enabled = true;
                _dir.enabled = false;
                CMIntro.enabled = false;
				CMBlackICE.enabled = false;
				CMICE.enabled = false;
				GameManager.instance.UI.IsTracing = true;
                this.enabled = false;
            }
        }
    }
}
