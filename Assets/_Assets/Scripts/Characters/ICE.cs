using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class ICE : MonoBehaviour
{

    [SerializeField] Transform _pathRoot;
    [SerializeField] float _detectionRadius = 3;
    [SerializeField] Color _idleColor = Color.green;
    [SerializeField] Color _alertColor = Color.red;
    [SerializeField] ParticleSystem _deathParticles;


    Transform[] _followPathPoints;

    private NavMeshAgent _agent;
    private int _followPathCurrentPoint = 0;
    private bool _inAlertMode = false;
    private bool _followingPatrol = true;

    Projector _alertArea;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _alertArea = GetComponentInChildren<Projector>();
        _alertArea.material = new Material(_alertArea.material);

        _alertArea.orthographicSize = _detectionRadius;

        if (_pathRoot != null && _pathRoot.childCount > 0)
        {
            _followPathPoints = new Transform[_pathRoot.childCount];
            for (int i = 0; i < _followPathPoints.Length; i++)
            {
                _followPathPoints[i] = _pathRoot.GetChild(i);
                _agent.SetDestination(_followPathPoints[_followPathCurrentPoint].position);
            }
        }
    }

    void Update()
    {
        UpdateDetectPlayer();

        if (_inAlertMode)
        {
            //Hemos detectado al jugador, y alertamos a todos los ICE. Además, lo seguimos
            GameManager.instance.AlertICE();
            _agent.SetDestination(GameManager.instance.player.transform.position);
            _followingPatrol = false;
        }
        else
        {
            if (_followingPatrol)
            {
                if (_followPathPoints != null && !_agent.pathPending && _agent.remainingDistance < 0.5f)
                {
                    _followPathCurrentPoint = (_followPathCurrentPoint + 1) % _followPathPoints.Length;
                    _agent.SetDestination(_followPathPoints[_followPathCurrentPoint].position);
                }
            }
            else
            {
                _agent.SetDestination(_followPathPoints[_followPathCurrentPoint].position);
                _followingPatrol = true;
            }
        }
    }


    private void UpdateDetectPlayer()
    {
        _inAlertMode = false;
        _alertArea.material.SetColor("_Color", _idleColor);

        if (!GameManager.instance == null)
        {
            Vector3 playerPosition = GameManager.instance.player.transform.position;
            Vector3 directionToPlayer = playerPosition - this.transform.position;


            if (directionToPlayer.magnitude < _detectionRadius)
            {
                _inAlertMode = true;
                _alertArea.material.SetColor("_Color", _alertColor);
                GameManager.instance.UI.TraceLvl += (0.1f * Time.deltaTime);
            }
        }

    }

    public void Die()
    {
        Instantiate(_deathParticles,this.transform.position, new Quaternion());

        Destroy(gameObject);
    }
}
