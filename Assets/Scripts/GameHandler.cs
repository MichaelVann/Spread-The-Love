using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class GameHandler : MonoBehaviour
{
    internal static GameHandler _autoRef;
    internal static UpgradeTree _upgradeTree;
    internal static AudioManager _audioManager;
    internal static int _score;
    internal static int _lastSeenScore;
    internal static int _mapSize;

    [SerializeField] internal Color m_loveColorMax;
    [SerializeField] internal Color m_neutralColor;
    [SerializeField] internal Color m_fearColor1;
    [SerializeField] internal Color m_fearColor2;
    // Start is called before the first frame update

    internal static void ChangeScore(int a_change) { _score += a_change; }
    internal static void UpdateLastSeenScore() { _lastSeenScore = _score; }
    internal static void IncrementMapSize() { _mapSize++; }

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
        
    }

    static internal void AutoSaveCheck()
    {

    }

    static internal void Quit()
    {
        Application.Quit();
    }
}
