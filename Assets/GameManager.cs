using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance = null;
    private DayOfWeek _currentDayOfWeek = DayOfWeek.Monday;
    public static DayOfWeek CurrentDayOfWeek { get { return _instance._currentDayOfWeek; } set { _instance._currentDayOfWeek = value; } }

    [Header("Music")]
    [SerializeField] private MusicPlaylistSO MusicForGameplay;
    [SerializeField] private AudioClip MusicForMainMenu;
    [SerializeField] private AudioClip MusicForVictory;
    [SerializeField] private AudioClip MusicForDefeat;
    [SerializeField] private AudioClip MusicForLoading;
    public enum MusicType { Gameplay, Victory, Defeat, MainMenu };

    // player stats and settings
    private Dictionary<DayOfWeek, bool> _goldenCarrotPickState = new Dictionary<DayOfWeek, bool>();

    [Header("Debug")]
    [SerializeField] bool _debugResetGoldenCarrotState = false;

    void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private IEnumerator Start()
    {
        LoadPlayerPrefs();
        yield return null;

        if (_debugResetGoldenCarrotState)
        {
            foreach (var item in _goldenCarrotPickState)
            {
                _goldenCarrotPickState[item.Key] = false;
            }
        }
    }

    private void LoadPlayerPrefs()
    {
        _goldenCarrotPickState = GoldenCarrotDataManager.LoadGoldenCarrotStatus();
    }

    private void SavePlayerPrefs()
    {
        GoldenCarrotDataManager.SaveGoldenCarrotStatus(_goldenCarrotPickState);
    }

    public static void PlayMusic(MusicType type)
    {
        switch (type)
        {
            case MusicType.Gameplay:
                AudioManager.PlayMusicPlaylist(_instance.MusicForGameplay);
                break;
            case MusicType.Victory:
                AudioManager.PlayMusic(_instance.MusicForVictory);
                break;
            case MusicType.Defeat:
                AudioManager.PlayMusic(_instance.MusicForDefeat);
                break;
            case MusicType.MainMenu:
                AudioManager.PlayMusic(_instance.MusicForMainMenu);
                break;
            default:
                break;
        }
    }

    // --- GAMEPLAY --- //
    public static void GoldenCarrotPick()
    {
        _instance._goldenCarrotPickState[_instance._currentDayOfWeek] = true;
    }
}
