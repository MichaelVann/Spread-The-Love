using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SamsaraHandler : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI m_turnRateText;
    [SerializeField] TextMeshProUGUI m_topSpeedText;
    [SerializeField] TextMeshProUGUI m_accelerationText;
    [SerializeField] TextMeshProUGUI m_massText;
    [SerializeField] TextMeshProUGUI m_fireRateText;
    [SerializeField] LoveReadout m_loveReadoutRef;
    [SerializeField] GameObject m_optionsMenuPrefab;
    [SerializeField] Button m_nextHintButton;

    //Hints
    [SerializeField] TextMeshProUGUI m_hintText;
    [SerializeField] TextMeshProUGUI m_hintCostText;
    int m_currentHint = -1;
    int m_hintCost = 3;

    List<string> m_hints = new List<string> {
        "I'd suggest getting your movement to a better place before trying to mess around with the shooting stuff",
        "Try to focus on loving the saddest and most fearful of the souls first, they cause the most pain.",
        "Time is all you have and all you need.",
        "The mohawk looking dudes are invulnerable to your shot love vibes, try giving them a hug.",
        "The guys chasing you down are not going to engage with you well physically, try zapping them with a good vibe first.",
    };

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1.0f;
        m_hintCostText.text = m_hintCost.ToString();
        RefreshUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor && Input.GetKey(KeyCode.Alpha7))
        {
            GameHandler.ChangeScoreFromSamsara(1);
            RefreshUI();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Instantiate(m_optionsMenuPrefab);
        }
    }

    void RefreshStats()
    {
        m_turnRateText.text = "Turn Speed: " + PlayerHandler.GetRotateSpeed() + "�/s";
        m_topSpeedText.text = "Top Speed: " + PlayerHandler.GetMaxSpeed() + "m/s";
        m_accelerationText.text = "Acceleration: " + PlayerHandler.GetAcceleration() + "m/s�";
        m_massText.text = "Mass: " + PlayerHandler.GetMass() *10f + "kg";
        m_fireRateText.text = "Fire Rate: " + PlayerHandler.GetFireRate() + "/s";
    }

    void RefreshHintButton()
    {
        m_nextHintButton.interactable =  GameHandler._score >= m_hintCost;
    }

    public void RefreshUI()
    {
        RefreshStats();
        RefreshHintButton();
        m_loveReadoutRef.RefreshRollingText();
    }

    public void Reincarnate()
    {
        FindObjectOfType<GameHandler>().TransitionScene(GameHandler.eScene.Battle);

        GameHandler.UpdateLastSeenScore();
    }

    public void ReturnToMainMenu()
    {
        FindObjectOfType<GameHandler>().TransitionScene(GameHandler.eScene.MainMenu);
    }

    public void PurchaseHint()
    {
        GameHandler.ChangeScoreFromSamsara(-m_hintCost);
        m_currentHint = (m_currentHint+1)%m_hints.Count;
        m_hintText.text = m_hints[m_currentHint];
        RefreshUI();
    }
}
