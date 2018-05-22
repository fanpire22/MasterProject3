using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Enemy : Soldier, IPointerClickHandler {

    #region Editor properties

    [SerializeField] Transform _crosshairTarget;

    [SerializeField] float _viewAngle = 45;
    [SerializeField] float _viewDistance = 4;

    [SerializeField] Color _relaxColor = Color.green;
    [SerializeField] Color _warnColor = Color.yellow;
    [SerializeField] Color _attackColor = Color.red;

    [SerializeField] Transform _pathRoot;

    [SerializeField] ParticleSystem _selectionPS;

    #endregion

    #region Component references

    Projector _viewConeProjector;

    #endregion

    #region State variables

    float _alertPreviousSpeed;

    bool _isIdle;
    Quaternion _idleRotation;

    bool _isAlerted;
    Vector3 _alertPoint;
    float _alertEndTime;

    bool _isFollowingPath;
    Transform[] _followPathPoints;
    int _followPathCurrentPoint = 0;

    bool _isAttacking;

    bool _isSelected;

    #endregion

    #region Public properties

    public Transform crosshairTarget
    {
        get { return _crosshairTarget; }
    }

    #endregion

    #region Awake

    protected override void Awake()
    {
        base.Awake();

        _viewConeProjector = GetComponentInChildren<Projector>();

        AwakePathPoints();
    }

    private void AwakePathPoints()
    {
        if (_pathRoot != null && _pathRoot.childCount > 0)
        {
            _followPathPoints = new Transform[_pathRoot.childCount];
            for (int i = 0; i < _followPathPoints.Length; i++)
            {
                _followPathPoints[i] = _pathRoot.GetChild(i);
            }
        }
    }

    #endregion

    #region Start

    protected override void Start()
    {
        base.Start();

        _idleRotation = this.transform.rotation;
        _selectionPS.Stop();

        StartViewConeProjector();

        BeginIdle();
	}
    
    void StartViewConeProjector()
    {
        _viewConeProjector.material = new Material(_viewConeProjector.material);
        _viewConeProjector.orthographicSize = _viewDistance;
        _viewConeProjector.material.SetFloat("_Angle", _viewAngle / 2);
    }

    #endregion

    #region Update

    protected override void Update()
    {
        if(_isDead) { return; }

        base.Update();        

        UpdateCanSeePlayer();

        UpdateIdle();
        UpdateFollowingPath();
        UpdateAlerted();
        UpdateAttack();
    }

    private void UpdateCanSeePlayer()
    {
        Player player = GameManager.instance.player;
        Vector3 playerPosition = player.transform.position;
        Vector3 directionToPlayer = playerPosition - this.transform.position;
        Vector3 myForward = this.transform.forward;

        bool canSeePlayer = false;
        float angle = Vector3.Angle(myForward, directionToPlayer);
        if (angle < _viewAngle / 2)
        {
            float distance = directionToPlayer.magnitude;
            if (distance < _viewDistance)
            {
                if (HasLineOfSightToSoldier(player))
                {
                    canSeePlayer = true;
                }
            }
        }

        if (canSeePlayer)
        {
            BeginAttack();
        }
        else
        {
            EndAttack();
        }
    }

    #endregion

    #region States

    #region State: Idle

    void BeginIdle()
    {
        if(_followPathPoints != null)
        {
            BeginFollowingPath();
        }
        else
        {
            if (!_isIdle)
            {
                _isIdle = true;

                SetViewConeColor(_relaxColor);
            }
        }        
    }

    void UpdateIdle()
    {
        if(_isIdle)
        {
            this.transform.rotation = Quaternion.RotateTowards(
                this.transform.rotation, _idleRotation, _agent.angularSpeed * Time.deltaTime);
        }
    }

    void EndIdle()
    {
        if(_isIdle)
        {
            _isIdle = false;
        }
    }

    #endregion

    #region State: Following path

    void BeginFollowingPath()
    {
        if(!_isFollowingPath)
        { 
            _isFollowingPath = true;
            _agent.SetDestination(_followPathPoints[_followPathCurrentPoint].position);
            SetViewConeColor(_relaxColor);
        }
    }

    void UpdateFollowingPath()
    {
        if (_isFollowingPath)
        {
            if (_followPathPoints != null && !_agent.pathPending && _agent.remainingDistance < 0.5f)
            {
                _followPathCurrentPoint = (_followPathCurrentPoint + 1) % _followPathPoints.Length;
                _agent.SetDestination(_followPathPoints[_followPathCurrentPoint].position);
            }
        }
    }

    void EndFollowingPath()
    {
        if (_isFollowingPath)
        {
            _isFollowingPath = false;
        }
    }

    #endregion

    #region State: Alerted

    void BeginAlerted()
    {
        if(!_isAlerted && !_isAttacking)
        {   
            _isAlerted = true;
            Debug.Log("Begin alerted");

            EndIdle();
            EndFollowingPath();

            SetViewConeColor(_warnColor);
            _alertPreviousSpeed = _agent.speed;
            _agent.speed = 0;
        }
    }

    private void UpdateAlerted()
    {
        if(_isAlerted)
        { 
            if (Time.time < _alertEndTime)
            {            
                RotateTowardsPosition(_alertPoint);
            }
            else
            {
                EndAlerted();
            }
        }
    }

    void EndAlerted()
    {
        if(_isAlerted)
        {
            Debug.Log("End alerted");

            _isAlerted = false;
            _agent.speed = _alertPreviousSpeed;

            if (!_isAttacking && !_isDead)
            {
                BeginIdle();
            }            
        }
    }

    public void SetAlertPoint(Vector3 position)
    {
        _alertPoint = position;
        _alertEndTime = Time.time + 5;
    }

    #endregion

    #region State: Attack

    void BeginAttack()
    {
        if(!_isAttacking)
        {
            _isAttacking = true;
            Debug.Log("Begin attack");

            EndIdle();
            EndFollowingPath();
            EndAlerted();

            _animator.SetBool("aiming", true);

            _alertPreviousSpeed = _agent.speed;
            _agent.speed = 0;

            SetViewConeColor(_attackColor);
            SetAlertPoint(GameManager.instance.player.transform.position);

                        
            Invoke("AttackPlayer", 3);            
        }
    }

    void UpdateAttack()
    {
        if(_isAttacking)
        {
            Debug.Log("Update attack");

            SetAlertPoint(GameManager.instance.player.transform.position);
            RotateTowardsPosition(_alertPoint);
        }
    }

    void EndAttack()
    {
        if(_isAttacking)
        {
            Debug.Log("End attack");

            _isAttacking = false;

            _agent.speed = _alertPreviousSpeed;
            _animator.SetBool("aiming", false);
            CancelInvoke("AttackPlayer");

            if (!_isDead)
            {
                BeginAlerted();
            }
        }
    }

    void AttackPlayer()
    {
        GameManager.instance.player.Die();
    }

    #endregion

    #endregion

    #region Triggers

    private void OnTriggerStay(Collider other)
    {
        if (_isDead) { return; }

        SetAlertPoint(other.bounds.center);
        BeginAlerted();
    }

    #endregion

    #region Common methods

    public void ActivateSelection()
    {
        _isSelected = true;
        _selectionPS.Play();
    }

    public void DeactivateSelection()
    {
        _isSelected = false;
        _selectionPS.Stop();
    }

    public override void Die()
    {
        base.Die();

        EndIdle();
        EndFollowingPath();
        EndAlerted();
        EndAttack();

        _viewConeProjector.gameObject.SetActive(false);
    }

    void RotateTowardsPosition(Vector3 position)
    {
        Vector3 directionToPosition = position - this.transform.position;
        Quaternion desiredRotation = Quaternion.LookRotation(directionToPosition);
        this.transform.rotation = Quaternion.RotateTowards(
            this.transform.rotation, 
            desiredRotation, 
            _agent.angularSpeed * Time.deltaTime);
    }

    void SetViewConeColor(Color color)
    {
        _viewConeProjector.material.SetColor("_Color", color);
    }

    #endregion

    #region Input events

    public void OnPointerClick(PointerEventData eventData)
    {
        if(_isDead) { return; }

        if(!_isSelected)
        {
            GameManager.instance.player.AimAtTargetEnemy(this);
        }
        else
        {
            GameManager.instance.player.ShootAtTarget();
        }
        
    }

    #endregion
    
}
