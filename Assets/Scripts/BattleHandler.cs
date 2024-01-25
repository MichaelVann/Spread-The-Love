using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.VFX;

public class BattleHandler : MonoBehaviour
{
    internal GameHandler m_gameHandlerRef;

    [SerializeField] Camera m_mainCameraRef;
    [SerializeField] GameObject m_vesselPrefab;
    [SerializeField] PlayerHandler m_playerHandlerRef;
    [SerializeField] GameObject m_buildingPrefab;
    [SerializeField] GameObject m_loveVibePrefab;
    [SerializeField] GameObject m_buildingContainer;
    [SerializeField] GameObject m_vesselContainer;

    //UI
    [SerializeField] GameObject m_uiContainerRef;
    [SerializeField] GameObject m_optionsMenuPrefab;
    [SerializeField] TextMeshProUGUI m_timeText;
    [SerializeField] TextMeshProUGUI m_speedText;
    [SerializeField] TextMeshProUGUI m_vesselsConvertedText;
    [SerializeField] TextMeshProUGUI m_vesselCountText;
    [SerializeField] TextMeshProUGUI m_vesselsConvertedDeltaText;
    [SerializeField] Image m_whiteOutImageRef;
    [SerializeField] internal GameObject m_worldTextCanvasRef;

    //Minimap
    [SerializeField] GameObject m_miniMapRef;
    [SerializeField] Camera m_miniMapCameraRef;

    //Vessels
    int starterSouls = 20;
    float m_spawnDistance = 2f;
    internal List<Vessel> m_vesselList;
    int m_vesselsConverted = 0;
    int m_vesselsConvertedDelta = 0;
    const float m_starterDeviance = 0.25f;
    static float m_scaredSoul = 0.5f - m_starterDeviance;
    static float m_normalSoul = 0.5f;
    static float m_peacefulSoul = 0.5f + m_starterDeviance;

    //Building Grid
    [SerializeField] GameObject m_outerWallPrefab;
    [SerializeField] GameObject[] m_buildingPrefabs;
    int m_buildingColumns = 2;
    int m_buildingRows = 2;
    float m_buildingSize = 5f;
    float m_streetSize = 5f;
    [SerializeField] GameObject[] m_trashDecalPrefabs;
    [SerializeField] GameObject[] m_cornerDecals;

    //Background
    [SerializeField] SpriteRenderer m_backgroundRef;
    [SerializeField] ScrollingRawImage m_scrollingBackgroundRef;

    //Timer
    float m_gameTime = 30f;
    vTimer m_battleTimer;
    vTimer m_battleExplosionTimer;
    vTimer m_secondPassedTimer;
    bool m_gameEnding = false;

    internal bool m_paused = false;

    internal Vector2 GetMapSize() { return new Vector2(m_streetSize / 2f + (m_buildingSize + m_streetSize) * m_buildingColumns / 2f, m_streetSize / 2f + (m_buildingSize + m_streetSize) * m_buildingRows / 2f); }

    internal void SetPaused(bool a_paused) { m_paused = a_paused; Time.timeScale = m_paused ? 0f : 1f; }

    private void Awake()
    {
        m_gameHandlerRef = FindObjectOfType<GameHandler>();
        Time.timeScale = 1f;
        m_vesselList = new List<Vessel>();
        m_buildingColumns = m_buildingRows = GameHandler._mapSize * 2;
        m_miniMapCameraRef.GetComponent<Camera>().orthographicSize = GetMapSize().x;
    }

    // Start is called before the first frame update
    void Start()
    {
        SpawnBuildings();
        SpawnOuterWalls();
        SpawnVessels();
        m_gameTime += GameHandler._upgradeTree.GetUpgradeLeveledStrength(UpgradeItem.UpgradeId.AdditionalTime);
        m_battleTimer = new vTimer(m_gameTime, true, true, false);
        m_secondPassedTimer = new vTimer(1f);
        m_vesselCountText.text = "/" + m_vesselList.Count;
        m_backgroundRef.size = GetMapSize() * new Vector2(2f/m_backgroundRef.transform.localScale.x,2f/ m_backgroundRef.transform.localScale.y);
    }

    internal void CrementConvertedVessels(int a_change)
    { 
        m_vesselsConverted += a_change; 
        m_vesselsConvertedDelta += a_change;
        if (m_vesselsConverted >= m_vesselList.Count)
        {
            Enlighten();
        }
    }

    void SecondPassedTimerUpdate()
    {
        if (m_secondPassedTimer.Update())
        {
            m_vesselsConvertedDeltaText.text = "+" + m_vesselsConvertedDelta.ToString() + "/s";
            m_vesselsConvertedDelta = 0;
        }
    }

    void MoveToSamsara()
    {
        GameHandler.ChangeScore(m_vesselsConverted);
        SceneManager.LoadScene("Samsara");
    }

    void UpdateUI()
    {
        SecondPassedTimerUpdate();
        m_speedText.text = m_playerHandlerRef.GetSpeed().ToString("f1") + " m/s";
        m_vesselsConvertedText.text = m_vesselsConverted.ToString();
        m_timeText.text = (m_gameTime - m_battleTimer.GetTimer()).ToString("f1");

        m_scrollingBackgroundRef.SetAdditionalOffset(m_mainCameraRef.transform.position);
    }

    internal void FinishEarly()
    {
        m_battleTimer.SetTimer(m_battleTimer.GetTimerMax());
    }

    void Enlighten()
    {
        m_whiteOutImageRef.color = m_gameHandlerRef.m_loveColorMax;
        GameHandler.IncrementMapSize();
        FinishEarly();
    }

    internal void Perish()
    {
        FinishEarly();
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_paused)
        {
            UpdateUI();

            //Battle Timer
            if (!m_gameEnding && m_battleTimer.Update())
            {
                m_gameEnding = true;
                m_battleExplosionTimer = new vTimer(2);
                m_battleExplosionTimer.SetUsingUnscaledDeltaTime(true);
            }

            //Game ending
            if (m_gameEnding)
            {
                if (m_battleExplosionTimer.Update())
                {
                    MoveToSamsara();
                }
                else
                {
                    float percentageFinished = m_battleExplosionTimer.GetCompletionPercentage();
                    Time.timeScale = 1f - percentageFinished;
                    Color whiteoutColor = m_whiteOutImageRef.color;
                    m_whiteOutImageRef.color = new Color(whiteoutColor.r, whiteoutColor.g, whiteoutColor.b, percentageFinished);
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                Perish();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OpenPauseMenu();
            }
        }
    }

    void OpenPauseMenu()
    {
        Instantiate(m_optionsMenuPrefab, m_uiContainerRef.transform).GetComponent<OptionsMenu>().Init(this);
        SetPaused(true);
    }

    Vessel SpawnVessel(Vector3 a_position, int a_emotion = 0)
    {
        Vessel vessel = Instantiate(m_vesselPrefab, a_position, Quaternion.identity, m_vesselContainer.transform).GetComponent<Vessel>();
        vessel.Init(this, m_playerHandlerRef, a_emotion, m_miniMapCameraRef);
        m_vesselList.Add(vessel);
        return vessel;
    }

    void SpawnVessels()
    {
        float buildingGap = m_buildingSize + m_streetSize;

        for (int i = 0; i < m_buildingColumns+1; i++)
        {
            for (int j = 0; j < m_buildingRows+1; j++)
            {
                if (i == m_buildingColumns/2 && j == m_buildingRows/2)
                {
                    continue;
                }
                float posX = i * buildingGap;
                posX -= buildingGap * m_buildingColumns / 2f;
                float posY = j * buildingGap;
                posY -= buildingGap * m_buildingRows / 2f;

                bool inACorner = (i == 0 || i == m_buildingColumns);
                inACorner &= (j == 0 || j == m_buildingRows);

                if (inACorner)
                {
                    int vesselStrength = Mathf.Clamp(-GameHandler._mapSize, Soul.GetMinPossibleLove(), -1);
                    SpawnVessel(new Vector3(posX, posY), vesselStrength);
                    SpawnVessel(new Vector3(posX, posY), vesselStrength);
                    SpawnVessel(new Vector3(posX, posY), vesselStrength);
                }
                else
                {
                    SpawnVessel(new Vector3(posX, posY));
                    SpawnVessel(new Vector3(posX, posY));
                    SpawnVessel(new Vector3(posX, posY));
                }
            }
        }
    }

    void SpawnOuterWalls()
    {
        float buildingGap = m_buildingSize + m_streetSize;
        float xOffset = m_streetSize/2f + buildingGap * m_buildingColumns / 2f;
        float yOffset = m_streetSize/2f + buildingGap * m_buildingRows / 2f;
        for (int i = 0; i < 4; i++)
        {
            float posX = i < 2 ? (i % 2 == 0 ? -xOffset : xOffset): 0f;
            float posY = i >= 2 ? (i % 2 == 0 ? -yOffset : yOffset): 0f;
            float angle = i < 2 ? 0f : 90f;
            SpriteRenderer spriteRenderer = Instantiate(m_outerWallPrefab, new Vector3(posX, posY, 0f), Quaternion.identity).GetComponent<SpriteRenderer>();
            if (i < 2)
            {
                spriteRenderer.size = spriteRenderer.GetComponent<BoxCollider2D>().size = new Vector2(1f, Mathf.Abs(posX) * 2f);
            }
            else
            {
                spriteRenderer.size = spriteRenderer.GetComponent<BoxCollider2D>().size = new Vector2(Mathf.Abs(posY) * 2f, 1f);
            }
            //spriteRenderer.gameObject.transform.eulerAngles = new Vector3 (0f, 0f, angle);
        }
    }

    void SpawnBuildingDecals(Vector3 a_position)
    {
        float spreadRange = 0.6f + m_buildingSize / 2f;

        int trashToSpawn = VLib.vRandom(3, 6);
        for (int i = 0; i < trashToSpawn; i++)
        {
            int trashRoll = VLib.vRandom(0, m_trashDecalPrefabs.Length - 1);
            Vector3 spawnPos = new Vector3(VLib.vRandom(-spreadRange, spreadRange), spreadRange, 0f);
            spawnPos += a_position;
            int wallToSpawnBy = VLib.vRandom(0, 3);
            spawnPos = VLib.RotateVector3In2D(spawnPos, wallToSpawnBy * 90f);
            Instantiate(m_trashDecalPrefabs[trashRoll], spawnPos, Quaternion.identity);
        }

        int cornerDecalsToSpawn = VLib.vRandom(2, 4);
        for (int i = 0; i < cornerDecalsToSpawn; i++)
        {
            int decalRoll = VLib.vRandom(0, m_cornerDecals.Length - 1);
            Vector3 spawnPos = new Vector3(spreadRange, spreadRange, 0f);
            spawnPos += a_position;
            int corner = VLib.vRandom(0, 3);
            spawnPos = VLib.RotateVector3In2D(spawnPos, corner * 90f);
            Instantiate(m_cornerDecals[decalRoll], spawnPos, Quaternion.identity);
        }
    }

    void SpawnBuilding(Vector3 a_position)
    {
        GameObject prefab = m_buildingPrefabs[VLib.vRandom(0,m_buildingPrefabs.Length-1)];
        Instantiate(prefab, a_position, Quaternion.identity, m_buildingContainer.transform);
        SpawnBuildingDecals(a_position);
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
