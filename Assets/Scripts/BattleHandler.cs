using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.VFX;

public class BattleHandler : MonoBehaviour
{
    [SerializeField] GameObject m_vesselPrefab;
    [SerializeField] PlayerHandler m_playerHandlerRef;
    [SerializeField] GameObject m_buildingPrefab;
    [SerializeField] GameObject m_loveVibePrefab;

    //UI
    [SerializeField] TextMeshProUGUI m_timeText;
    [SerializeField] TextMeshProUGUI m_scoreText;
    [SerializeField] TextMeshProUGUI m_speedText;
    [SerializeField] TextMeshProUGUI m_vesselsConvertedText;
    [SerializeField] Image m_whiteOutImageRef;

    float m_score = 0f;

    //Vessels
    int starterSouls = 20;
    float m_spawnDistance = 2f;
    internal List<Vessel> m_vesselList;
    int m_vesselsConverted = 0;
    const float m_starterDeviance = 0.25f;
    static float m_scaredSoul = 0.5f - m_starterDeviance;
    static float m_normalSoul = 0.5f;
    static float m_peacefulSoul = 0.5f + m_starterDeviance;

    //Building Grid
    int m_buildingColumns = 16;
    int m_buildingRows = 16;
    float m_buildingSize = 5f;
    float m_streetSize = 5f;

    //Timer
    const float m_gameTime = 60f;
    vTimer m_battleTimer;
    vTimer m_battleExplosionTimer;
    bool m_gameEnding = false;


    internal void IncrementConvertedVessels(int a_change) { m_vesselsConverted += a_change; }

    internal void ChangeScore(float a_score) { m_score += a_score; RefreshScoreText(); }

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1f;
        m_vesselList = new List<Vessel>();
        SpawnBuildings();
        SpawnVessels();
        m_battleTimer = new vTimer(m_gameTime, true, true, false);
        RefreshScoreText();
    }

    void RefreshScoreText()
    {
        m_scoreText.text = VLib.RoundToDecimalPlaces(m_score, 1).ToString();
    }

    // Update is called once per frame
    void Update()
    {
        m_speedText.text = m_playerHandlerRef.GetSpeed().ToString("f2") + " m/s";
        m_vesselsConvertedText.text = m_vesselsConverted + "/" + m_vesselList.Count;
        m_timeText.text = (m_gameTime - m_battleTimer.GetTimer()).ToString("f1");
        if (m_battleTimer.Update())
        {
            m_gameEnding = true;
            m_battleExplosionTimer = new vTimer(2);
            m_battleExplosionTimer.SetUsingUnscaledDeltaTime(true);
        }

        if (m_gameEnding)
        {
            if (m_battleExplosionTimer.Update())
            {
                SceneManager.LoadScene("Samsara");
            }
            else
            {
                float percentageFinished = m_battleExplosionTimer.GetCompletionPercentage();
                Time.timeScale = 1f - percentageFinished;
                m_whiteOutImageRef.color = new Color(1f, 1f, 1f, percentageFinished);
            }
        }
    }

    void SpawnBuilding(Vector3 a_position)
    {
        Instantiate<GameObject>(m_buildingPrefab, a_position, Quaternion.identity);
    }

    void SpawnVessel(Vector3 a_position)
    {
        Vessel vessel = Instantiate(m_vesselPrefab, a_position, Quaternion.identity).GetComponent<Vessel>();
        vessel.Init(this, m_playerHandlerRef, 0f);
        m_vesselList.Add(vessel);
    }

    void SpawnVessels()
    {
        float buildingGap = m_buildingSize + m_streetSize;

        for (int i = 0; i < m_buildingColumns; i++)
        {
            for (int j = 0; j < m_buildingRows; j++)
            {
                if (i == m_buildingColumns/2 && j == m_buildingRows/2)
                {
                    continue;
                }
                float posX = i * buildingGap;
                posX -= buildingGap * m_buildingColumns / 2f;
                float posY = j * buildingGap;
                posY -= buildingGap * m_buildingRows / 2f;

                SpawnVessel(new Vector3(posX, posY));
                SpawnVessel(new Vector3(posX, posY));
                SpawnVessel(new Vector3(posX, posY));
            }
        }
    }

    void SpawnBuildings()
    {
        float buildingGap = m_buildingSize + m_streetSize;
        for (int i = 0; i < m_buildingColumns; i++)
        {
            for (int j = 0; j < m_buildingRows; j++)
            {
                float posX = i * buildingGap;
                posX -= buildingGap * m_buildingColumns / 2f;
                posX += buildingGap / 2f;
                float posY = j * buildingGap;
                posY -= buildingGap * m_buildingRows / 2f ;
                posY += buildingGap / 2f;

                SpawnBuilding(new Vector3(posX, posY));
            }
        }
    }

    internal void DestroyVessel(Vessel a_vessel)
    {
        m_vesselList.Remove(a_vessel);
        Destroy(a_vessel.gameObject);
    }
}
