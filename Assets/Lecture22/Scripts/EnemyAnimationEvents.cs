using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    [SerializeField] 
    private Enemy _enemy;

    public void OnDeathAnimationFinished()
    {
        if(_enemy != null)
            _enemy.OnDeathAnimationFinished();
    }
}
