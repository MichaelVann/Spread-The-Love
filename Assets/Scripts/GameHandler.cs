using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class GameHandler : MonoBehaviour
{
    internal static GameHandler _autoRef;
    internal static UpgradeTree _upgradeTree;
    internal static AudioManager _audioManager;
    internal static int _score;
    internal static int _lastSeenScore;
    internal static int _mapSize;
    internal static bool _firstTimeCutscenePlayed = false;
    internal static int _livesLived = 0;

    [SerializeField] internal Color m_loveColorMax;
    [SerializeField] internal Color m_loveColor1;
    [SerializeField] internal Color m_neutralColor;
    [SerializeField] internal Color[] m_fearColors;

    //Scenes
    internal enum eScene
    {
        MainMenu,
        Samsara,
        FirstTimeCutScene,
        Battle
    }
    eScene m_currentScene, m_queuedScene;
    [SerializeField] Image m_sceneFadeImageRef;
    [SerializeField] internal Canvas m_sceneFadeCanvasRef;
    vTimer m_sceneFadeTimer;
    bool m_sceneFadingOut;
    const float m_sceneFadeDuration = 0.35f;

    [SerializeField] internal Sprite[] m_upgradeImages;

    internal static void ChangeScore(int a_change) { _score += a_change; }
    internal static void ChangeScoreFromSamsara(int a_change) { _score += a_change; UpdateLastSeenScore(); }
    internal static void UpdateLastSeenScore() { _lastSeenScore = _score; }
    internal static void IncrementMapSize() { _mapSize++; }
    internal static void IncrementLivesLived() { _livesLived++; }

    internal static void SetFirstTimeCutscenePlayed(bool a_value) { _firstTimeCutscenePlayed = true; }

    void Awake()
    {
        if (_autoRef == null)
        {
            Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Init()
    {
        _autoRef = this;
        DontDestroyOnLoad(gameObject);
        _audioManager = GetComponent<AudioManager>();
        _upgradeTree = new UpgradeTree();
        _score = 0;
        _lastSeenScore = -1;
        _mapSize = 1;
    }

    // Update is called once per frame
    void Update()
    {
        SceneFadeUpdate();
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            LoadGame();
        }
        else if(Input.GetKeyDown(KeyCode.Equals))
        {
            SaveGame();
        }
    }

    static internal void AutoSaveCheck()
    {

    }

    static internal void Quit()
    {
        Application.Quit();
    }

    internal void TransitionScene(eScene a_scene)
    {
        m_queuedScene = a_scene;
        m_sceneFadeTimer = new vTimer(m_sceneFadeDuration, true, true, false, true);
        m_sceneFadingOut = true;
        m_sceneFadeCanvasRef.worldCamera = FindObjectOfType<Camera>();
        m_sceneFadeCanvasRef.sortingLayerName = "UI";
        m_sceneFadeCanvasRef.sortingOrder = 20;
    }

    internal void ChangeScene()
    {
        m_currentScene = m_queuedScene;

        switch (m_queuedScene)
        {
            case eScene.MainMenu:
                SceneManager.LoadScene("Main Menu");
                break;
            case eScene.Samsara:
                SceneManager.LoadScene("Samsara");
                break;
            case eScene.FirstTimeCutScene:
                SceneManager.LoadScene("FirstTimeCutScene");
                break;
            case eScene.Battle:
                SceneManager.LoadScene("Battle");
                break;
            default:
                break;
        }
        m_sceneFadeTimer = new vTimer(m_sceneFadeDuration, true, true, false, false);
        m_sceneFadingOut = false;
    }

    internal void SceneFadeUpdate()
    {
        if (m_sceneFadeTimer != null && !m_sceneFadeTimer.m_finished)
        {
            if (m_sceneFadeTimer.Update())
            {
                if (m_sceneFadingOut)
                {
                    ChangeScene();
                }
            }
            float compPerc = m_sceneFadeTimer.GetCompletionPercentage();
            float fade = m_sceneFadingOut ? compPerc : 1f - compPerc;
            fade = Mathf.Clamp(fade, 0f, 1f);
            m_sceneFadeImageRef.color = new Color(0f, 0f, 0f, fade);
            _audioManager.m_sceneFadeAmount = fade;
            _audioManager.Refresh();
        }
    }

    static void SaveGame()
    {
        SaveDataUtility saveDataUtility = new SaveDataUtility();
        saveDataUtility.Save();
    }

    static void LoadGame()
    {
        SaveDataUtility saveDataUtility = new SaveDataUtility();
        saveDataUtility.TryLoad();
    }
}
