using BehaviourTrees;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestStrategy : IStrategy
{
    public readonly float restDuration;
    public readonly MonoBehaviour monoBehaviour;
    private bool isResting = false;
    private bool isDone = false;
    public RestStrategy(float restDuration, MonoBehaviour monoBehaviour)
    {
        this.restDuration = restDuration;
        this.monoBehaviour = monoBehaviour;
    }

    public Node.Status Process()
    {
        if (!isResting)
        {
            monoBehaviour.StartCoroutine(RestCoroutine());
            isResting = true;
        }
        if (isDone)
        {
            return Node.Status.Success;
        }
        else
        {
            return Node.Status.Running;
        }
    }

    public IEnumerator RestCoroutine()
    {
        Debug.Log("Resting...");
        yield return new WaitForSeconds(restDuration);
        isDone = true;
    }

    public void Reset()
    {
        isResting = false;
        isDone = false;
    }
}
