using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.VFX;

public class BattleHandler : MonoBehaviour
{
    [SerializeField] GameObject m_vesselPrefab;
    [SerializeField] PlayerHandler m_playerHandlerRef;
    [SerializeField] GameObject m_buildingPrefab;
    [SerializeField] GameObject m_loveVibePrefab;

    //UI
    [SerializeField] TextMeshProUGUI m_scoreText;
    [SerializeField] TextMeshProUGUI m_speedText;

    float m_score = 0f;

    int starterSouls = 20;
    float m_spawnDistance = 2f;
    internal List<Vessel> m_vesselList;
    const float m_starterDeviance = 0.25f;
    static float m_scaredSoul = 0.5f - m_starterDeviance;
    static float m_normalSoul = 0.5f;
    static float m_peacefulSoul = 0.5f + m_starterDeviance;

    //Building Grid
    int m_buildingColumns = 16;
    int m_buildingRows = 16;
    float m_buildingSize = 5f;
    float m_streetSize = 5f;

    internal void ChangeScore(float a_score) { m_score += a_score; RefreshScoreText(); }

    // Start is called before the first frame update
    void Start()
    {
        m_vesselList = new List<Vessel>();
        SpawnBuildings();
        SpawnVessels();
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
    }

    void SpawnBuilding(Vector3 a_position)
    {
        Instantiate<GameObject>(m_buildingPrefab, a_position, Quaternion.identity);
    }

    void SpawnVessel(Vector3 a_position)
    {
        Instantiate(m_vesselPrefab, a_position, Quaternion.identity).GetComponent<Vessel>().Init(this, m_playerHandlerRef, 0.5f);
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
