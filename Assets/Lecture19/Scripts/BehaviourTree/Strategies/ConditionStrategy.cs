using BehaviourTrees;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionStrategy : IStrategy
{
    readonly System.Func<bool> condition;
    public ConditionStrategy(System.Func<bool> condition)
    {
        this.condition = condition;
    }
    public Node.Status Process()
    {
        return condition() ? Node.Status.Success : Node.Status.Failure;
    }
    public void Reset()
    {
        // No state to reset for a condition strategy
    }
}
