using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Soldier : MonoBehaviour {

    [SerializeField] Transform _viewRaycastOrigin;
    [SerializeField] Transform _viewRaycastDestination;
    [SerializeField] LayerMask _viewLayerMask;
    [SerializeField] GameObject _minimapIcon;

    protected NavMeshAgent _agent;
    protected Animator _animator;
    private Ragdoll _ragdoll;
    private Collider _rootCollider;

    protected bool _isDead = false;

    protected virtual void Awake()
    {
        _ragdoll = GetComponent<Ragdoll>();
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _rootCollider = GetComponent<Collider>();
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {

    }

    protected bool HasLineOfSightToSoldier(Soldier target)
    {
        Vector3 raycastOrigin = _viewRaycastOrigin.position;
        Vector3 raycastDestination = target._viewRaycastDestination.position;
        Vector3 raycastDirection = raycastDestination - raycastOrigin;

        RaycastHit hitInfo;
        Debug.DrawLine(raycastOrigin, raycastDestination, Color.red);

        if(Physics.Raycast(raycastOrigin, raycastDirection, out hitInfo, Mathf.Infinity, _viewLayerMask))
        {
            // Debug.Log(hitInfo.collider.name + " of "+ hitInfo.collider.transform.root.name);
            Soldier hittedSoldier = hitInfo.collider.GetComponentInParent<Soldier>();
            if(hittedSoldier != null && hittedSoldier == target)
            {
                return true;
            }
        }
        return false;
    }

    protected bool RaycastToSoldier(Soldier target, out RaycastHit hitInfo)
    {
        Vector3 raycastOrigin = _viewRaycastOrigin.position;
        Vector3 raycastDestination = target._viewRaycastDestination.position;
        Vector3 raycastDirection = raycastDestination - raycastOrigin;
        
        if (Physics.Raycast(raycastOrigin, raycastDirection, out hitInfo, Mathf.Infinity, _viewLayerMask))
        {
            return true;
        }
        return false;
    }

    public virtual void Die()
    {
        _isDead = true;
        _agent.enabled = false;
        _ragdoll.ActivateRagdoll();
        _rootCollider.enabled = false;
        _minimapIcon.SetActive(false);
    }

}
