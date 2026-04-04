using BehaviourTrees;
using System;
using UnityEngine;
using UnityEngine.AI;

public class ChaseStrategy : IStrategy
{
    private readonly NavMeshAgent _navMeshAgent;
    private readonly Func<Vector3> _target;

    private readonly Func<bool> _isPlayerVisible;
    private float timeSinceLastSeen = float.MaxValue;
    private float chaseDurationAfterLosingSight = 5f;
    private bool wasActive = false;
    private float _agentSpeedBeforeChase = 1f;

    public ChaseStrategy(NavMeshAgent navMeshAgent, Func<Vector3> target, Func<bool> isPlayerVisible)
    {
        _navMeshAgent = navMeshAgent;
        _target = target;
        _isPlayerVisible = isPlayerVisible;
        _agentSpeedBeforeChase = navMeshAgent.speed;
    }
    public Node.Status Process()
    {
        if (!wasActive)
        {
            _navMeshAgent.ResetPath();
            wasActive = true;
        }
        if (Vector3.Distance(_navMeshAgent.transform.position, _target()) > _navMeshAgent.stoppingDistance)
        {
            _navMeshAgent.speed = 1.75f;
            _navMeshAgent.SetDestination(_target());
        }


        if (_isPlayerVisible()) 
        {
            timeSinceLastSeen = Time.time;
        }
        
        if((Time.time - timeSinceLastSeen) > chaseDurationAfterLosingSight)
        {
            return Node.Status.Failure;
        }
        else
        {
            return Node.Status.Running;
        }
    }

    public void Reset()
    {
        timeSinceLastSeen = float.MaxValue;
        if (wasActive)
        {
            _navMeshAgent.speed = _agentSpeedBeforeChase;
            wasActive = false;
        }
    }
}
