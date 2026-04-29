using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour, IService, ISaveable
{
    public float _score { get; private set; }
    public int _headshots { get; private set; }
    public int _shots { get; private set; }

    private UIManager _uiManager;
    private GameManager _gameManager;
    private SaveSystemManager _saveSystemManager;

    public void AddScore(float value)
    {
        _score += value;
        RedrawScore();

        if (_score >= _gameManager.GetWinScore())
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
        _saveSystemManager = IServiceLocator.Instance.GetService<SaveSystemManager>();
        _saveSystemManager.RegisterSaveable(this);
    }

    void OnDestroy()
    {
        if (_saveSystemManager != null)
        {
            _saveSystemManager.UnregisterSaveable(this);
        }
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

    public ScoreData GetScoreData()
    {
        return new ScoreData
        {
            score = _score,
            headshots = _headshots,
            shots = _shots
        };
    }

    public void SetData(ScoreData data)
    {
        Debug.Log($"SetData BEFORE: _score={_score}, incoming score={data.score}");
        _score = data.score;
        _headshots = data.headshots;
        _shots = data.shots;
        Debug.Log($"SetData AFTER: _score={_score}");
        RedrawScore();
        RedrawHeadshots();
        RedrawShots();
    }

    public SaveData GetSaveData(SaveData data)
    {
        data.scoreData = GetScoreData();
        return data;
    }

    public void SetSaveData(SaveData data)
    {
        SetData(data.scoreData);
    }
}
