using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepHandler : MonoBehaviour
{
    [SerializeField] 
    private Transform _footTransform;

    [SerializeField]
    private float _maxAudibleDistance = 25f;

    private Transform _playerTransform;

    private AudioManager _audioManager;



    private void Awake()
    {
        _audioManager = IServiceLocator.Instance.GetService<AudioManager>();
    }

    private void Start()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    public void OnFootstep()
    {
        if (_playerTransform == null) 
            return;
        float distance = Vector3.Distance(_footTransform.position, _playerTransform.position);

        if (distance > _maxAudibleDistance) 
            return;

        _audioManager.PlaySoundAtPosition("Footstep", _footTransform.position);
    }

    public void OnRun()
    {
        if (_playerTransform == null)
            return;
        float distance = Vector3.Distance(_footTransform.position, _playerTransform.position);

        if (distance > _maxAudibleDistance)
            return;

        _audioManager.PlaySoundAtPosition("Running", _footTransform.position);
    }
}
