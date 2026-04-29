using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : Enemy
{
    private readonly int _takeDamageHash = Animator.StringToHash("IsTakingDamage");
    /*
    public override void runDieAnimation()
    {
        //throw new System.NotImplementedException();
    }

    public override void runTakeDamageAnimation()
    {
        _animator.SetTrigger(_takeDamageHash);
    }
    */
}
