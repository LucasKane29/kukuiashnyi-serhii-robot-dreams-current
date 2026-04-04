using BehaviourTrees;
using UnityEngine;
using UnityEngine.AI;

public class PatrolStrategy : IStrategy
{
    readonly NavMeshAgent navMeshAgent;
    readonly Transform transform;
    readonly float patrolRadius;
    readonly int patrolPointLimit;
    private int patrolPointsVisited = 0;
    private bool isPathCalculated = false;

    public PatrolStrategy(NavMeshAgent navMeshAgent, Transform transform, float patrolRadius, int patrolPointLimit = 5)
    {
        this.navMeshAgent = navMeshAgent;
        this.transform = transform;
        this.patrolRadius = patrolRadius;
        this.patrolPointLimit = patrolPointLimit;
    }

    public Node.Status Process()
    {
        if (patrolPointsVisited >= patrolPointLimit) 
        { 
            return Node.Status.Success; 
        
        }
        if (!isPathCalculated && !navMeshAgent.pathPending)
        {
            Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
            randomDirection += transform.position;
            NavMeshHit hit;

            if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
            {
                navMeshAgent.SetDestination(hit.position);
            }
        }

        if(isPathCalculated && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            patrolPointsVisited++;
            isPathCalculated = false;

        }

        if(navMeshAgent.pathPending)
        {
            isPathCalculated = true;
        }
        return Node.Status.Running;
    }

    public void Reset()
    {
        patrolPointsVisited = 0;
        isPathCalculated = false;
    }
}
