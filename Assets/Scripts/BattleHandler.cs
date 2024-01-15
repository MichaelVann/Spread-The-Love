using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleHandler : MonoBehaviour
{
    [SerializeField] GameObject m_soulPrefab;
    [SerializeField] PlayerHandler m_playerHandlerRef;
    [SerializeField] TextMeshProUGUI m_scoreText;

    float m_score = 0f;

    int starterSouls = 20;
    float m_spawnDistance = 2f;
    internal List<Vessel> m_soulList;
    const float m_starterDeviance = 0.25f;
    static Vector2 m_scaredSoul = new Vector2 (0.5f - m_starterDeviance, 0.5f - m_starterDeviance);
    static Vector2 m_manicSoul = new Vector2 (0.5f - m_starterDeviance, 0.5f + m_starterDeviance);
    static Vector2 m_sadSoul = new Vector2(0.5f + m_starterDeviance, 0.5f - m_starterDeviance);
    static Vector2 m_peacefulSoul = new Vector2(0.5f + m_starterDeviance, 0.5f + m_starterDeviance);

    internal void ChangeScore(float a_score) { m_score += a_score; RefreshScoreText(); }

    // Start is called before the first frame update
    void Start()
    {
        m_soulList = new List<Vessel>();
        //SpawnStarterSouls(5, 2f, new Vector2[] { m_peacefulSoul });
        SpawnStarterSouls(10, 3f, new Vector2[] { m_manicSoul });
        SpawnStarterSouls(10, 4f, new Vector2[] { m_manicSoul });
        RefreshScoreText();
        //SpawnStarterSouls(6, 4f, new Vector2[] { m_manicSoul, m_scaredSoul });
    }

    void SpawnStarterSouls(int a_soulCount, float a_distance, Vector2[] a_possibleEmotions)
    {
        for (int i = 0; i < a_soulCount; i++)
        {
            Vector3 spawnPos = VLib.RotateVector3In2D(Vector3.up, i * 360f/ a_soulCount) * a_distance;
            Vessel soul = Instantiate(m_soulPrefab, spawnPos, Quaternion.identity).GetComponent<Vessel>();
            Vector2 emotion = a_possibleEmotions[VLib.vRandom(0, a_possibleEmotions.Length-1)];// VLib.vRandom(0f, 1f) > 0.5f ? new Vector2(1f, 0f) : new Vector2(0f, 1f);
            soul.Init(this, m_playerHandlerRef, emotion);
            m_soulList.Add(soul);
        }
    }

    void RefreshScoreText()
    {
        m_scoreText.text = VLib.RoundToDecimalPlaces(m_score*10f, 1).ToString();
    }

    // Update is called once per frame
    void Update()
    {
    }
}
