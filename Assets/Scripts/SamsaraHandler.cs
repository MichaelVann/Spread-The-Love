using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SamsaraHandler : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI m_loveText;

    void UpdateLoveText() { m_loveText.text = GameHandler._score.ToString(); }

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1.0f;
        UpdateLoveText();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Reincarnate()
    {
        SceneManager.LoadScene("Battle");
        GameHandler.UpdateLastSeenScore();
    }
}
