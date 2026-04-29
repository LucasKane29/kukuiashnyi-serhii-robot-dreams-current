using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    [SerializeField] 
    private PlayerHealthController _playerHealthController;

    public void OnDeathAnimationFinished()
    {
        if(_playerHealthController != null)
            _playerHealthController.OnDeathAnimationFinished();
    }
}
