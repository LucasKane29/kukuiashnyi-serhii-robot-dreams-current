using System;
using UnityEngine;

public class ScoreManager: MonoBehaviour, IService
{
    public float _score { get; private set; }
    public int _headshots { get; private set; }
    public int _shots { get; private set; }

    private UIManager _uiManager;
    private GameManager _gameManager;

    public void AddScore(float value)
    {
        _score += value;
        RedrawScore();

        if(_score >= _gameManager.GetWinScore())
        {
            _gameManager.OnPlayerWin();
        }
    }

    void Awake()
    {
        _score = 0;
        _headshots = 0;
        _shots = 0;
        _uiManager = IServiceLocator.Instance.GetService<UIManager>();
        _gameManager = IServiceLocator.Instance.GetService<GameManager>();
    }

    void Start()
    {
        RedrawScore();
    }

    public void OnShotMade()
    {
        _shots += 1;
        RedrawShots();
        
    }

    public void OnHeadshotMade()
    {
        _headshots += 1;
        RedrawHeadshots();
    }

    void RedrawScore()
    {
        _uiManager.UpdateScore(_score);
    }

    void RedrawShots()
    {
        _uiManager.UpdateShots(_shots);
    }

    void RedrawHeadshots()
    {
        _uiManager.UpdateHeadshots(_headshots);
    }
}
