using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CinematicCheckPoint : MonoBehaviour {

    [SerializeField] CinemachineVirtualCamera CMIntro;
    [SerializeField] CinemachineVirtualCamera CMMainCamera;
    public Conversacion[] Conversaciones;

    PlayableDirector _dir;
    UnityEngine.AI.NavMeshAgent _agent;
    float waitConvoTime;
    int _iConvo;
    AI2Animator _anim;
    bool _bConvoStart = false;
    Player _player;
    InteractionIK _interaction;
}
