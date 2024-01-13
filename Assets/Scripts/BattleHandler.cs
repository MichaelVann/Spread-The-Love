using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleHandler : MonoBehaviour
{
    [SerializeField] GameObject m_soulPrefab;
    [SerializeField] PlayerHandler m_playerHandlerRef;

    int starterSouls = 20;
    float m_spawnDistance = 2f;
    internal List<Soul> m_soulList;

    // Start is called before the first frame update
    void Start()
    {
        m_soulList = new List<Soul>();
        SpawnStarterSouls();
    }

    void SpawnStarterSouls()
    {
        for (int i = 0; i < starterSouls; i++)
        {
            Vector3 spawnPos = VLib.RotateVector3In2D(Vector3.up, i * 360f/starterSouls) * m_spawnDistance;
            Soul soul = Instantiate(m_soulPrefab, spawnPos, Quaternion.identity).GetComponent<Soul>();
            soul.Init(this, m_playerHandlerRef);
            m_soulList.Add(soul);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
