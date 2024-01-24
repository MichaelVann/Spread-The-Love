using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SamsaraHandler : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI m_turnRateText;
    [SerializeField] TextMeshProUGUI m_topSpeedText;
    [SerializeField] TextMeshProUGUI m_accelerationText;
    [SerializeField] TextMeshProUGUI m_massText;
    [SerializeField] TextMeshProUGUI m_fireRateText;
    [SerializeField] LoveReadout m_loveReadoutRef;
    [SerializeField] GameObject m_optionsMenuPrefab;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1.0f;
        RefreshUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Alpha7))
        {
            GameHandler.ChangeScore(1);
            RefreshUI();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Instantiate(m_optionsMenuPrefab);
        }
    }

    void RefreshStats()
    {
        m_turnRateText.text = "Turn Speed: " + PlayerHandler.GetRotateSpeed() + "°/s";
        m_topSpeedText.text = "Top Speed: " + PlayerHandler.GetMaxSpeed() + "m/s";
        m_accelerationText.text = "Acceleration: " + PlayerHandler.GetAcceleration() + "m/s²";
        m_massText.text = "Mass: " + PlayerHandler.GetMass() *10f + "kg";
        m_fireRateText.text = "Fire Rate: " + PlayerHandler.GetFireRate() + "/s";
    }

    public void RefreshUI()
    {
        RefreshStats();
        m_loveReadoutRef.RefreshRollingText();
    }

    public void Reincarnate()
    {
        SceneManager.LoadScene("Battle");
        GameHandler.UpdateLastSeenScore();
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
