using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour, IService
{
    private LogService _logService;
    [SerializeField]
    private Transform[] _spawnPoints;
    [SerializeField]
    private GameObject _enemyPrefab;
    [SerializeField]
    private Transform _enemiesContainer;
    [SerializeField]
    private List<Enemy> _enemiesPool;

    [SerializeField]
    private float _respawnEnemiesCooldown = 5f;

    [SerializeField]
    private float _respawnEnemyCooldown = 10f;

    private Coroutine _spawnCoroutine;
    private HashSet<Enemy> _pendingRespawn = new HashSet<Enemy>();

    private Transform _player;

    private List<Enemy> GenerateEnemiesPool(int enemiesAmount)
    {
        _logService.Log($"Generating {enemiesAmount} enemies pool");
        for (int i = 0; i < enemiesAmount; i++)
        {
            _logService.Log($"Generating enemy {i + 1} at spawn point {i}");
            int spawnPointIndex = i;
            var enemy = Instantiate(_enemyPrefab, _spawnPoints[spawnPointIndex].position, Quaternion.identity, _enemiesContainer);
            Enemy _enemy = enemy.GetComponent<Enemy>();
            _enemy.SetPlayer(_player);
            _enemiesPool.Add(_enemy);
            enemy.SetActive(false);
        }
        return null;
    }

    private void RespawnEnemies()
    {
        for(int i = 0; i < _enemiesPool.Count; i++)
        {
            if (!_enemiesPool[i].gameObject.activeInHierarchy)
            {
                if (_pendingRespawn.Contains(_enemiesPool[i]))
                    continue;

                if (_enemiesPool[i].IsDead())
                {
                    _pendingRespawn.Add(_enemiesPool[i]);
                    StartCoroutine(RespawnEnemyRoutine(_enemiesPool[i], _spawnPoints[i].position));
                }
                else
                {
                    ActivateUnit(_enemiesPool[i], _spawnPoints[i].position);
                }
            }
        }
    }
    void Awake()
    {
        _logService = IServiceLocator.Instance.GetService<LogService>();
        _player = IServiceLocator.Instance.GetService<PlayerMoveController>().GetPlayer();
        RunGenerateEnemies();
    }

    public void RunGenerateEnemies()
    {
        GenerateEnemiesPool(_spawnPoints.Length);
    }

    void Start()
    {
        _spawnCoroutine = StartCoroutine(SpawnEnemiesRoutine());
    }

    IEnumerator SpawnEnemiesRoutine()
    {
        yield return null;
        while (true)
        {
            RespawnEnemies();
            yield return new WaitForSeconds(_respawnEnemiesCooldown);
        }
    }

    IEnumerator RespawnEnemyRoutine(Enemy enemy, Vector3 spawnPosition)
    {
        yield return new WaitForSeconds(_respawnEnemyCooldown);
        _pendingRespawn.Remove(enemy);
        ActivateUnit(enemy, spawnPosition);
    }

    private void ActivateUnit(Enemy enemy, Vector3 spawnPosition)
    {
        enemy.transform.position = spawnPosition;
        enemy.gameObject.SetActive(true);
        enemy.ResetStatus();
    }
}
