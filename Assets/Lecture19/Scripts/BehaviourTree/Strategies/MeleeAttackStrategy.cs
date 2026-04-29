using BehaviourTrees;
using System;
using UnityEngine;
using UnityEngine.AI;

public class MeleeAttackStrategy : IStrategy
{
    private readonly NavMeshAgent _navMeshAgent;
    private readonly Func<Vector3> _target;

    private readonly Func<bool> _ConditionChecker;

    private Enemy _enemy;

    public MeleeAttackStrategy(NavMeshAgent navMeshAgent, Func<Vector3> target, Enemy enemy, Func<bool> ConditionChecker)
    {
        _navMeshAgent = navMeshAgent;
        _target = target;
        _ConditionChecker = ConditionChecker;
        _enemy = enemy;
    }

    public Node.Status Process()
    {
        if (_ConditionChecker())
        {
            _navMeshAgent.autoBraking = false;
            //_enemy.meleeAttack(_target());
            return Node.Status.Running;
        }
        else
        {
            return Node.Status.Failure;
        }
    }

    public void Reset()
    {
        _navMeshAgent.autoBraking = true;
    }
}
