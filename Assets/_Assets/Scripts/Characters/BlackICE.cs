using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BlackICE : Avatar, IPointerClickHandler {

    #region Editor properties

    [SerializeField] Transform _crosshairTarget;

    [SerializeField] float _viewAngle = 45;
    [SerializeField] float _viewDistance = 4;

    [SerializeField] Color _relaxColor = Color.green;
    [SerializeField] Color _warnColor = Color.yellow;
    [SerializeField] Color _attackColor = Color.red;

    [SerializeField] Transform _pathRoot;
    [SerializeField] bool _isCurious;

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

    /// <summary>
    /// Inicialización de patrulla
    /// </summary>
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

    /// <summary>
    /// Inicio estándar de Unity
    /// </summary>
    protected override void Start()
    {
        base.Start();

        _idleRotation = this.transform.rotation;

        StartViewConeProjector();

        BeginIdle();
	}
    
    /// <summary>
    /// Generamos el cono dependiendo de nuestros parámetros
    /// </summary>
    void StartViewConeProjector()
    {
        _viewConeProjector.material = new Material(_viewConeProjector.material);
        _viewConeProjector.orthographicSize = _viewDistance;
        _viewConeProjector.material.SetFloat("_Angle", _viewAngle / 2);
    }

    #endregion

    #region Update

    /// <summary>
    /// El Update estándar de Unity
    /// </summary>
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

    /// <summary>
    /// Determinamos si podemos ver al jugador, y está dentro de nuestro área de visión
    /// </summary>
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
                if (HasLineOfSightToAvatar(player))
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

    /// <summary>
    /// Estamos en estado tranquilo o hemos vuelto a él
    /// </summary>
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

    /// <summary>
    /// No estamos alertados
    /// </summary>
    void UpdateIdle()
    {
        if(_isIdle)
        {
            this.transform.rotation = Quaternion.RotateTowards(
                this.transform.rotation, _idleRotation, _agent.angularSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Dejamos de estar tranquilos
    /// </summary>
    void EndIdle()
    {
        if(_isIdle)
        {
            _isIdle = false;
        }
    }

    #endregion

    #region State: Following path

    /// <summary>
    /// Iniciamos o retomamos la patrulla preestablecida
    /// </summary>
    void BeginFollowingPath()
    {
        if(!_isFollowingPath)
        { 
            _isFollowingPath = true;
            _agent.SetDestination(_followPathPoints[_followPathCurrentPoint].position);
            SetViewConeColor(_relaxColor);
        }
    }

    /// <summary>
    /// Comprobamos si hemos llegado a un punto de la ruta preestablecida
    /// </summary>
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

    /// <summary>
    /// Dejamos de seguir la patrulla preestablecida
    /// </summary>
    void EndFollowingPath()
    {
        if (_isFollowingPath)
        {
            _isFollowingPath = false;
        }
    }

    #endregion

    #region State: Alerted
    /// <summary>
    /// Función que salta cuando un Hielo normal detecta al jugador: TODOS los hielos negros convergen en el punto de alerta (incluso los que no son curiosos)
    /// </summary>
    /// <param name="position"></param>
    public void BeginAlerted(Vector3 position)
    {
        BeginAlerted();
        SetAlertPoint(position);
        if (!_isAttacking)
        {
            //No tiene aún al jugador dentro de su ángulo
            _agent.SetDestination(_alertPoint);
            _agent.speed = _alertPreviousSpeed;
        }
    }

    /// <summary>
    /// El hielo negro ha sido alertado. Dejamos de andar y comenzamos a girarnos
    /// </summary>
    void BeginAlerted()
    {
        if(!_isAlerted && !_isAttacking)
        {   
            _isAlerted = true;

            EndIdle();
            EndFollowingPath();

            SetViewConeColor(_warnColor);
            _alertPreviousSpeed = _agent.speed;
            _agent.speed = 0;
        }
    }

    /// <summary>
    /// Seguimos en alerta: Nos giramos hacia el punto de alerta
    /// </summary>
    private void UpdateAlerted()
    {
        if(_isAlerted)
        { 
            if (Time.time < _alertEndTime)
            {            
                RotateTowardsPosition(_alertPoint);
                if (_isCurious && !_isAttacking)
                {
                    //Como es de los que siguen, buscamos al jugador por todo el mapa
                    _agent.SetDestination(_alertPoint);
                    _agent.speed = _alertPreviousSpeed;
                }
                GameManager.instance.UI.TraceLvl += (0.0002f * Time.deltaTime);
            }
            else
            {
                EndAlerted();
            }
        }
    }

    /// <summary>
    /// Volvemos al estado Idle. Los que tienen patrulla vuelven a la patrulla, los que no, se quedan donde están
    /// </summary>
    void EndAlerted()
    {
        if(_isAlerted)
        {

            _isAlerted = false;
            _agent.speed = _alertPreviousSpeed;

            if (!_isAttacking && !_isDead)
            {
                BeginIdle();
            }            
        }
    }

    /// <summary>
    /// Determinamos el punto que nos ha alertado
    /// </summary>
    /// <param name="position"></param>
    public void SetAlertPoint(Vector3 position)
    {
        _alertPoint = position;
        _alertEndTime = Time.time + 5;
    }

    #endregion

    #region State: Attack

    /// <summary>
    /// El hielo tiene al jugador a la vista: Comenzamos a atacar
    /// </summary>
    void BeginAttack()
    {
        if(!_isAttacking)
        {
            _isAttacking = true;

            EndIdle();
            EndFollowingPath();
            EndAlerted();

            _animator.SetBool("aiming", true);

            _alertPreviousSpeed = _agent.speed;
            _agent.speed = 0;

            SetViewConeColor(_attackColor);
            SetAlertPoint(GameManager.instance.player.transform.position);

                        
            Invoke("AttackPlayer", 1);            
        }
    }

    /// <summary>
    /// Rotamos hacia el jugador, siempre que esté dentro de nuestro área
    /// </summary>
    void UpdateAttack()
    {
        if(_isAttacking)
        {
            _agent.speed = 0;

            GameManager.instance.UI.TraceLvl += (0.1f * Time.deltaTime);

            SetAlertPoint(GameManager.instance.player.transform.position);
            RotateTowardsPosition(_alertPoint);
        }
    }

    /// <summary>
    /// Se acabó el ataque, el jugador se ha ido.
    /// </summary>
    void EndAttack()
    {
        if(_isAttacking)
        {

            _isAttacking = false;

            _agent.speed = _alertPreviousSpeed;
            _animator.SetBool("aiming", false);
            _animator.SetTrigger("ResetAttack");

            CancelInvoke("AttackPlayer");
            CancelInvoke("KillPlayer");

            if (!_isDead)
            {
                BeginAlerted();
            }
        }
    }

    ///Disparamos al jugador
    void AttackPlayer()
    {
        _animator.SetTrigger("Shoot");
        Invoke("KillPlayer", 2.06f);
    }

    /// <summary>
    /// Acabamos de disparar al jugador: éste muere
    /// </summary>
    void KillPlayer()
    {

        GameManager.instance.player.Die();
    }

    #endregion

    #endregion

    #region Triggers

    /// <summary>
    /// Estamos dentro de un trigger: nos mantenemos en alerta
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerStay(Collider other)
    {
        if (_isDead) { return; }

        //Podríamos estar en el área de una zona de agachado automático, el punto de activación de una cámara o incluso la visión de otro enemigo
        if (other.CompareTag("Player")){
            SetAlertPoint(other.bounds.center);
            BeginAlerted();
        }
    }

    #endregion

    #region Common methods

    /// <summary>
    /// Nos ha seleccionado el jugador
    /// </summary>
    public void ActivateSelection()
    {
        _isSelected = true;
    }

    /// <summary>
    /// Nos ha deseleccionado el jugador
    /// </summary>
    public void DeactivateSelection()
    {
        _isSelected = false;
    }

    /// <summary>
    /// Nos ha matado el jugador
    /// </summary>
    public override void Die()
    {
        base.Die();

        EndIdle();
        EndFollowingPath();
        EndAlerted();
        EndAttack();

        _viewConeProjector.gameObject.SetActive(false);
    }

    /// <summary>
    /// Rotamos para mirar la dirección del parámetro
    /// </summary>
    /// <param name="position"></param>
    void RotateTowardsPosition(Vector3 position)
    {
        Vector3 directionToPosition = position - this.transform.position;
        Quaternion desiredRotation = Quaternion.LookRotation(directionToPosition);
        this.transform.rotation = Quaternion.RotateTowards(
            this.transform.rotation, 
            desiredRotation, 
            _agent.angularSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Cambiamos el color de nuestro cono de visión
    /// </summary>
    /// <param name="color"></param>
    void SetViewConeColor(Color color)
    {
        _viewConeProjector.material.SetColor("_Color", color);
    }

    #endregion

    #region Input events

    /// <summary>
    /// Nos han hecho clic. Si estamos vivos, mostramos la flecha o nos morimos, dependiendo de si ya estábamos seleccionados
    /// </summary>
    /// <param name="eventData"></param>
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
