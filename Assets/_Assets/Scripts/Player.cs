using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player : Avatar
{


    #region Editor properties

    [SerializeField] float _alertSpeedScale = 2;
    [SerializeField] float _walkSpeed = 3f;
    [SerializeField] float _runSpeed = 4.5f;
    [SerializeField] float _crouchSpeed = 2f;
    [SerializeField] float _maxAlertShootRadius = 6f;
    [SerializeField] GameObject _attackIcon;


    #endregion

    #region Component references

    Projector _alertAreaProjector;
    SphereCollider _alertAreaCollider;

    #endregion

    #region State variables

    bool _isIdle;
    bool _isAiming;
    public bool IsCrouching = false;
    Protector _targetEnemy;
    bool _isInteracting;
    bool _canInteractCancel = true;
    Interactive _targetInteract;
    Coroutine _updateInteractionCoroutine;
    InteractionIK _interactionIK;

    float _currentShootRadius;

    #endregion

    #region Events

    public Action OnShoot;
    public Action OnDie;

    #endregion

    protected override void Awake()
    {
        base.Awake();

        _alertAreaProjector = GetComponentInChildren<Projector>();
        _alertAreaCollider = _alertAreaProjector.GetComponent<SphereCollider>();
        _interactionIK = GetComponent<InteractionIK>();
    }

    protected override void Start()
    {
        base.Start();

        BeginIdle();
    }

    #region Update

    protected override void Update()
    {
        base.Update();

        UpdateIdle();
        UpdateAiming();

        UpdateAlertArea();
        UpdateInput();
    }

    public void FixedUpdate()
    {
        GameManager.instance.UI.TraceLvl += 0.0002f;
    }

    void UpdateAlertArea()
    {
        _currentShootRadius = Mathf.Max(0, _currentShootRadius - (Time.deltaTime * _maxAlertShootRadius));

        float usedRadius = Mathf.Max(_currentShootRadius, _agent.velocity.magnitude * _alertSpeedScale);

        _alertAreaProjector.orthographicSize = usedRadius;
        _alertAreaCollider.radius = usedRadius;
    }


    void UpdateInput()
    {

        _animator.SetBool("crouch", IsCrouching);

        //Determinamos primero si el jugador está corriendo
        _agent.speed = (IsCrouching ? _crouchSpeed : (Input.GetKey(KeyCode.LeftShift) ? _runSpeed : _walkSpeed));
    }

    #endregion

    #region State: Idle

    void BeginIdle()
    {
        if (!_isIdle)
        {
            _isIdle = true;

            EndAiming();
        }
    }

    void UpdateIdle()
    {
        if (_isIdle)
        {

        }
    }

    void EndIdle()
    {
        if (_isIdle)
        {
            _isIdle = false;
        }
    }

    #endregion

    #region State: Aiming

    void BeginAiming()
    {
        if (!_canInteractCancel) return;

        EndInteracting();
        if (!_isAiming)
        {
            _isAiming = true;

            EndIdle();

            _agent.SetDestination(this.transform.position);
            _animator.SetBool("aiming", true);

            if (_attackIcon)
            {
                _attackIcon.SetActive(true);
            }
        }
    }

    void UpdateAiming()
    {
        if (_isAiming && _targetEnemy != null)
        {
            Vector3 directionToEnemy = _targetEnemy.transform.position - this.transform.position;
            Quaternion desiredRotation = Quaternion.LookRotation(directionToEnemy);
            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, desiredRotation, _agent.angularSpeed * Time.deltaTime);

            float angle = Vector3.Angle(this.transform.forward, directionToEnemy);

            RaycastHit hitInfo;
            if (angle < 1 && RaycastToSoldier(_targetEnemy, out hitInfo))
            {


                Avatar hittedAvatar = hitInfo.collider.GetComponentInParent<Avatar>();
                if (hittedAvatar != null && hittedAvatar == _targetEnemy)
                {
                    Vector3 crosshairPosition = _targetEnemy.crosshairTarget.position;
                    
                    Vector3 screenPoint = Camera.main.WorldToScreenPoint(crosshairPosition);
                }
                else
                {
                    Vector3 hitPoint = hitInfo.point;
                    Vector3 screenPoint = Camera.main.WorldToScreenPoint(hitPoint);

                }
            }
            else
            {

            }


        }
    }

    void EndAiming()
    {
        if (_isAiming)
        {
            _isAiming = false;

            _targetEnemy.DeactivateSelection();

            _animator.SetBool("aiming", false);
            _animator.SetTrigger("ResetAttack");
            _attackIcon.SetActive(false);

            _targetEnemy = null;
        }
    }

    #endregion

    #region Input actions

    public void GoToDestination(Vector3 destination)
    {
        if (_isDead || !_canInteractCancel) { return; }

        EndInteracting();

        BeginIdle();

        _agent.SetDestination(destination);
    }

    public void AimAtTargetEnemy(Protector enemy)
    {
        if (_isDead || !_canInteractCancel) { return; }


        EndInteracting();

        if (_targetEnemy != null)
        {
            _targetEnemy.DeactivateSelection();
        }
        _targetEnemy = enemy;

        _attackIcon.transform.SetParent(_targetEnemy.crosshairTarget.transform);
        _attackIcon.transform.position = _targetEnemy.crosshairTarget.transform.position;

        _targetEnemy.ActivateSelection();

        BeginAiming();
    }

    public void AimAtTargetICE(ICE enemy)
    {
        if (_isDead || !_canInteractCancel) { return; }


        EndInteracting();
    }

    public void ShootAtTarget()
    {
        if (_isDead) return;

        if (HasLineOfSightToAvatar(_targetEnemy))
        {
            _animator.SetTrigger("Shoot");
            Invoke("KillTarget", 2.06f);

        }
        else
        {
            // Dar feedback sobre el hecho de no poder disparar
        }
    }

    public void KillTarget()
    {
        if (_isDead) return;

        if (HasLineOfSightToAvatar(_targetEnemy))
        {
            _currentShootRadius = _maxAlertShootRadius;
            _attackIcon.transform.SetParent(null);

            _targetEnemy.Die();

            BeginIdle();
        }
        else
        {
            // Dar feedback sobre el hecho de no poder disparar
        }
    }

    public override void Die()
    {
        EndAiming();
        base.Die();

        EndInteracting();
        //Si el evento está siendo escuchado, lo lanzamos
        if (OnDie != null)
        {
            OnDie.Invoke();
        }
    }

    #endregion

    #region Interactive

    public void InteractWithItem(Interactive item)
    {
        _targetInteract = item;

        BeginInteracting();
    }

    void BeginInteracting()
    {
        if (!_isInteracting)
        {
            _isInteracting = true;

            EndIdle();
            EndAiming();

            _updateInteractionCoroutine = StartCoroutine(UpdateInteractingCoroutine());
        }

    }

    IEnumerator UpdateInteractingCoroutine()
    {
        //Nos acercamos a la posición deseada
        _agent.SetDestination(_targetInteract.InteractionPoint.position);
        while (_agent.pathPending || _agent.remainingDistance > 0.1)
        {
            yield return null;
        }

        //Determinamos hacia dónde queremos girar y vamos rotando
        Vector3 desiredForward = Vector3.ProjectOnPlane(_targetInteract.InteractionPoint.forward, Vector3.up);
        Vector3 myForward = Vector3.ProjectOnPlane(this.transform.forward, Vector3.up);
        float angle = Vector3.Angle(myForward, desiredForward);
        while (angle > 0.1)
        {
            this.transform.forward = Vector3.RotateTowards(myForward, desiredForward, Mathf.Deg2Rad * _agent.angularSpeed * Time.deltaTime, 0);

            yield return null;

            myForward = Vector3.ProjectOnPlane(this.transform.forward, Vector3.up);
            angle = Vector3.Angle(myForward, desiredForward);
        }

        //Hemos llegado al objeto: Ya no podemos cancelar
        _canInteractCancel = false;

        if (IsCrouching)
        {
            //Incorporamos al personaje
            IsCrouching = false;
            _animator.SetBool("crouch", false);

            yield return new WaitForSeconds(0.5f);
        }

        _interactionIK.ActivateIK(_targetInteract.Handler);
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(_targetInteract.ActivateInteractionCoRoutine());
        EndInteracting();
    }

    void EndInteracting()
    {

        if (_isInteracting)
        {
            StopCoroutine(_updateInteractionCoroutine);

            _isInteracting = false;
            _interactionIK.DisableIK();

            _canInteractCancel = true;

            if (!_isAiming && !_isDead) BeginIdle();
        }
    }

    #endregion



}
