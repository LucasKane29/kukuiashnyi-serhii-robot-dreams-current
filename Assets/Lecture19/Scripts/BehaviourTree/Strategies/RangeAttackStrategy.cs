using BehaviourTrees;
using System;
using UnityEngine;
using UnityEngine.AI;

public class RangeAttackStrategy : IStrategy
{
    private readonly NavMeshAgent _navMeshAgent;
    private readonly Func<Vector3> _target;

    private readonly Func<bool> _ConditionChecker;
    private readonly Enemy _enemy;

    private bool _wasActive = false;

    public RangeAttackStrategy(NavMeshAgent navMeshAgent, Func<Vector3> target, Enemy enemy, Func<bool> ConditionChecker)
    {
        _navMeshAgent = navMeshAgent;
        _target = target;
        _ConditionChecker = ConditionChecker;
        _enemy = enemy;
    }
    public Node.Status Process()
    {
        if (!_wasActive)
        {
            _navMeshAgent.ResetPath();
            _wasActive = true;
            _navMeshAgent.autoBraking = false;
        }

        if (_ConditionChecker())
        {
            //_enemy.rangeAttack(_target());

            return Node.Status.Running;
        }
        else
        {
            return Node.Status.Failure;
        }
    }

    public void Reset()
    {
        if (_wasActive)
        {
            _navMeshAgent.ResetPath();
            _wasActive = false;
            _navMeshAgent.autoBraking = true;
        }
    }
}
