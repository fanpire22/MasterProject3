using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI2Animator : MonoBehaviour {

    NavMeshAgent _agent;
    Animator _animator;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    void Update () {
        Vector3 agentVelocity = _agent.velocity;
        Vector3 localAgentVelocity = this.transform.InverseTransformVector(agentVelocity);
        _animator.SetFloat("speedX", localAgentVelocity.x);
        _animator.SetFloat("speedZ", localAgentVelocity.z);
	}
}
