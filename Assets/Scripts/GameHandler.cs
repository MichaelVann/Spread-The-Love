using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class GameHandler : MonoBehaviour
{
    internal static GameHandler _autoRef;
    internal static int _score;
    internal static int _lastSeenScore;
    internal static UpgradeTree _upgradeTree;
    internal static AudioManager _audioManager;

    [SerializeField] internal Color m_loveColor;
    [SerializeField] internal Color m_neutralColor;
    [SerializeField] internal Color m_fearColor;
    // Start is called before the first frame update

    internal static void ChangeScore(int a_change) { _score += a_change; }
    internal static void UpdateLastSeenScore() { _lastSeenScore = _score; }

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
        _score = 0;
        _upgradeTree = new UpgradeTree();
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
