using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.VFX;
using static Unity.Burst.Intrinsics.X86;

public class BattleHandler : MonoBehaviour
{
    internal GameHandler m_gameHandlerRef;

    [SerializeField] Camera m_mainCameraRef;
    [SerializeField] CameraHandler m_mainCameraHandlerRef;
    [SerializeField] GameObject m_vesselPrefab;
    [SerializeField] PlayerHandler m_playerHandlerRef;
    [SerializeField] GameObject m_buildingPrefab;
    [SerializeField] GameObject m_loveVibePrefab;
    [SerializeField] GameObject m_buildingContainer;
    [SerializeField] GameObject m_vesselContainer;

    //UI
    [SerializeField] GameObject m_uiContainerRef;
    [SerializeField] GameObject m_optionsMenuPrefab;
    [SerializeField] TextMeshProUGUI m_tierText;
    [SerializeField] TextMeshProUGUI m_timeText;
    [SerializeField] TextMeshProUGUI m_speedText;
    [SerializeField] SpreadPercentageBar m_spreadPercentageBarRef;
    [SerializeField] Image m_whiteOutImageRef;
    [SerializeField] internal GameObject m_worldTextCanvasRef;
    [SerializeField] ClockRadialCircle m_clockRadialCircle;

    //Abilities UI
    [SerializeField] AbilityReadout[] m_abilityReadouts;
    [SerializeField] AbilityReadout m_shootAbilityReadout;
    [SerializeField] AbilityReadout m_brakeAbilityReadout;
    [SerializeField] AbilityReadout m_aquaplaneAbilityReadout;

    //Minimap
    [SerializeField] GameObject m_miniMapRef;
    [SerializeField] Camera m_miniMapCameraRef;

    //Score
    [SerializeField] TextMeshProUGUI m_scoreText;
    [SerializeField] ParticleSystem m_lootBagPoppedParticleSystem;
    int m_score = 0;

    //Vessels
    int starterSouls = 20;
    float m_spawnDistance = 2f;
    internal List<Vessel> m_vesselList;
    int m_vesselsPerIntersection = 6;
    int m_vesselsLoved = 0;
    int m_vesselsLovedDelta = 0;
    int m_vesselsFearful = 0;
    int m_vesselsFearfulDelta = 0;
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
    [SerializeField] GameObject[] m_roadDecals;

    //Cutscenes
    [SerializeField] GameObject m_cutscenePrefab;
    [SerializeField] Transform m_cutsceneCanvasTransformRef;
    bool cutsceneActive = false;
    bool m_startingCutsceneseChecked = false;
    float m_newEnemyTypeCameraZoom = 3f;

    //Background
    [SerializeField] SpriteRenderer m_backgroundRef;
    [SerializeField] ScrollingRawImage m_scrollingBackgroundRef;

    //Time
    float m_gameTime = 45f;
    vTimer m_battleTimer;
    vTimer m_battleExplosionTimer;
    vTimer m_secondPassedTimer;
    bool m_gameEnding = false;
    bool m_gameEnded = false;
    float m_bulletTimeFactor = 1f;
    float m_pauseTimeFactor = 1f;
    float m_gameEndingTimeFactor = 1f;
    float m_cutsceneTimeFactor = 1f;

    //Entities
    [SerializeField] GameObject m_lootBagPrefab;
    int m_lootBagsToSpawn;
    int m_minimumLivesLivedToSpawnLootBags = 1;
    float m_lootBagBonus = 0f;
    const float m_lootBagBonusIncrease = 0.1f;
    [SerializeField] TextMeshProUGUI m_lootBagBonusText;

    internal bool m_paused = false;

    internal Vector2 GetMapHalfSize() { return new Vector2(m_streetSize / 2f + (m_buildingSize + m_streetSize) * m_buildingColumns / 2f, m_streetSize / 2f + (m_buildingSize + m_streetSize) * m_buildingRows / 2f); }

    internal void SetPaused(bool a_paused) { m_paused = a_paused; m_pauseTimeFactor = m_paused ? 0f : 1f; Cursor.visible = m_paused || GameHandler._upgradeTree.HasUpgrade(UpgradeItem.UpgradeId.MouseAim); }

    internal void SetBulletTimeFactor(float a_factor) { m_bulletTimeFactor = a_factor; } 

    internal float GetBuildingGap() { return m_buildingSize + m_streetSize; }

    internal void IncreaseLootBagBonus() { m_lootBagBonus += m_lootBagBonusIncrease; UpdateLootBagBonusText(); m_lootBagPoppedParticleSystem.Play(); }

    void CutsceneEndedCallback() { m_cutsceneTimeFactor = 1f; m_mainCameraHandlerRef.EndTargetedZoom(); }

    Vector3 GetBuildingPosition(int a_x, int a_y)
    {
        float buildingGap = GetBuildingGap();
        float posX = a_x * buildingGap;
        posX -= buildingGap * m_buildingColumns / 2f;
        posX += buildingGap / 2f;
        float posY = a_y * buildingGap;
        posY -= buildingGap * m_buildingRows / 2f;
        posY += buildingGap / 2f;

        return new Vector3(posX, posY);
    }

    void UpdateLootBagBonusText()
    {
        m_lootBagBonusText.text = "+" + (m_lootBagBonus * 100f).ToString("f0") + "%";
    }
    
    int[] GetCentreInteresection()
    {
        return new int[2] { m_buildingColumns / 2, m_buildingRows / 2 };
    }

    bool IsCentreIntersection(int a_x, int a_y)
    {
        return (a_x == m_buildingColumns / 2 && a_y == m_buildingRows / 2);
    }

    internal Vector2 GetIntersectionPos(int a_x, int a_y)
    {
        float buildingGap = GetBuildingGap();
        Vector2 returnPos =  Vector2.zero;
        returnPos.x = a_x * buildingGap;
        returnPos.x -= buildingGap * m_buildingColumns / 2f;
        returnPos.y = a_y * buildingGap;
        returnPos.y -= buildingGap * m_buildingRows / 2f;
        return returnPos;
    }

    private void Awake()
    {
        m_gameHandlerRef = FindObjectOfType<GameHandler>();
        Time.timeScale = 1f;
        m_vesselList = new List<Vessel>();
        m_buildingColumns = m_buildingRows = GameHandler._mapSize * 2;
        m_miniMapCameraRef.GetComponent<Camera>().orthographicSize = GetMapHalfSize().x;
        ChangeScore(0);
        Cursor.visible = GameHandler._upgradeTree.HasUpgrade(UpgradeItem.UpgradeId.MouseAim);
        UpdateLootBagBonusText();
    }

    void CheckForStartingCutscenes()
    {
        if (GameHandler._highestMapSizeSeen < GameHandler._mapSize)
        {
            switch (GameHandler._mapSize)
            {
                case 2:
                    Cutscene cutscene = Instantiate(m_cutscenePrefab, m_cutsceneCanvasTransformRef).GetComponent<Cutscene>();
                    cutscene.Init(false, CutsceneEndedCallback);
                    cutscene.AddDialog("This is a <color=#FFFF00>Coward</color> type, they will flee from you if you get too close. They are also immune to your vibes, the little fuckers.");
                    Vector3 zoomPoint =  Vector3.zero;
                    for (int i = 0; i < m_vesselList.Count; i++)
                    {
                        if (m_vesselList[i].GetEmotion() == -2)
                        {
                            zoomPoint = m_vesselList[i].transform.position;
                            i = m_vesselList.Count;
                        }
                    }
                    m_mainCameraHandlerRef.SetTargetedZoom(zoomPoint, m_newEnemyTypeCameraZoom);
                    m_cutsceneTimeFactor = 0f;
                    break;
                default:
                    break;
            }
        }
        GameHandler.UpdateHighestMapSeen();

        m_startingCutsceneseChecked = true;
    }

    void InitialiseUpgrades()
    {
        m_gameTime += GameHandler._upgradeTree.GetUpgradeLeveledStrength(UpgradeItem.UpgradeId.AdditionalTime);

        //for (int i = 0; i < m_abilityReadouts.Length; i++)
        //{
        //    m_abilityReadouts[i].Init()
        //}

        m_shootAbilityReadout.SetUnlocked(GameHandler._upgradeTree.HasUpgrade(UpgradeItem.UpgradeId.Shooting));
        m_brakeAbilityReadout.SetUnlocked(GameHandler._upgradeTree.HasUpgrade(UpgradeItem.UpgradeId.Braking));

        m_aquaplaneAbilityReadout.SetUnlocked(GameHandler._upgradeTree.HasUpgrade(UpgradeItem.UpgradeId.Aquaplane));
    }

    // Start is called before the first frame update
    void Start()
    {
        SetupMap();
        InitialiseUpgrades();
        m_battleTimer = new vTimer(m_gameTime, true, true, false);
        m_secondPassedTimer = new vTimer(1f);
        UpdateSpreadPercentageBar();
        m_tierText.text = "Tier " + GameHandler._mapSize;
        if (GameHandler._mapSize <= 3)
        {
            m_tierText.text += "/3";
        }
        m_backgroundRef.size = GetMapHalfSize() * new Vector2(2f/m_backgroundRef.transform.localScale.x,2f/ m_backgroundRef.transform.localScale.y);
    }

    void ChangeScore(int a_change)
    {
        m_score += a_change;
        m_scoreText.text = m_score.ToString();
    }

    internal void CrementLovedVessels(int a_change)
    { 
        m_vesselsLoved += a_change; 
        m_vesselsLovedDelta += a_change;
        if (m_vesselsLoved >= m_vesselList.Count)
        {
            Enlighten();
        }
        UpdateSpreadPercentageBar();
        if (a_change > 0)
        {
            ChangeScore(a_change);
        }
    }

    internal void CrementFearfulVessels(int a_change)
    {
        m_vesselsFearful += a_change;
        m_vesselsFearfulDelta += a_change;
        UpdateSpreadPercentageBar();
    }

    void UpdateSpreadPercentageBar()
    {
        m_spreadPercentageBarRef.SetSegmentValues(m_vesselsLoved, m_vesselList.Count - m_vesselsLoved - m_vesselsFearful, m_vesselsFearful);
        m_spreadPercentageBarRef.UpdateBar();
    }

    Vessel SpawnVessel(Vector3 a_position, int a_emotion = 0)
    {
        Vessel vessel = Instantiate(m_vesselPrefab, a_position, Quaternion.identity, m_vesselContainer.transform).GetComponent<Vessel>();
        vessel.Init(this, m_playerHandlerRef, a_emotion, m_miniMapCameraRef);
        m_vesselList.Add(vessel);
        if (a_emotion < 0)
        {
            CrementFearfulVessels(1);
        }
        else if (a_emotion > 0)
        {
            CrementLovedVessels(1);
        }
        return vessel;
    }

    void SpawnVessels()
    {
        for (int i = 0; i < m_buildingColumns + 1; i++)
        {
            for (int j = 0; j < m_buildingRows + 1; j++)
            {
                if (IsCentreIntersection(i, j))
                {
                    continue;
                }

                int[] centreIntersection = GetCentreInteresection();

                Vector3 spawnPos = GetIntersectionPos(i, j);

                //bool inACorner = (i == 0 || i == m_buildingColumns);
                //inACorner &= (j == 0 || j == m_buildingRows);

                int xDist = Mathf.Abs(centreIntersection[0] - i);
                int yDist = Mathf.Abs(centreIntersection[1] - j);

                int vesselStrength = 0;
                if (xDist == yDist)
                {
                    vesselStrength = Mathf.Clamp(-xDist, Soul.GetMinPossibleLove(), 0);
                }

                for (int k = 0; k < m_vesselsPerIntersection; k++)
                {
                    SpawnVessel(spawnPos, vesselStrength);
                }
            }
        }
    }

    void SpawnOuterWalls()
    {
        float buildingGap = GetBuildingGap();
        float xOffset = m_streetSize / 2f + buildingGap * m_buildingColumns / 2f;
        float yOffset = m_streetSize / 2f + buildingGap * m_buildingRows / 2f;
        for (int i = 0; i < 4; i++)
        {
            float posX = i < 2 ? (i % 2 == 0 ? -xOffset : xOffset) : 0f;
            float posY = i >= 2 ? (i % 2 == 0 ? -yOffset : yOffset) : 0f;

            bool isVerticalWall = i < 2;

            int wallCount = 6 * (isVerticalWall ? m_buildingRows / 2 : m_buildingColumns / 2);

            float wallSize = 4.22f; // Pulled from editor

            for (int j = 0; j < wallCount; j++)
            {
                Vector3 spawnPos = Vector3.zero;
                float offset = (2f * (isVerticalWall ? yOffset : xOffset)) * j / wallCount;
                offset -= isVerticalWall ? yOffset : xOffset;
                offset += wallSize / 2f;
                if (isVerticalWall)
                {
                    spawnPos = new Vector3(posX, posY + offset, 0f);
                }
                else
                {
                    spawnPos = new Vector3(posX + offset, posY, 0f);
                }
                GameObject outerWall = Instantiate(m_outerWallPrefab, spawnPos, Quaternion.identity);
                outerWall.transform.localEulerAngles = new Vector3(0f, 0f, VLib.Vector2ToEulerAngle(new Vector2(posX, posY)) - 90f);
            }
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
        GameObject prefab = m_buildingPrefabs[VLib.vRandom(0, m_buildingPrefabs.Length - 1)];
        Instantiate(prefab, a_position, Quaternion.identity, m_buildingContainer.transform);
        SpawnBuildingDecals(a_position);
    }

    void SpawnBuildings()
    {
        float buildingGap = GetBuildingGap();
        for (int i = 0; i < m_buildingColumns; i++)
        {
            for (int j = 0; j < m_buildingRows; j++)
            {
                SpawnBuilding(GetBuildingPosition(i,j));
            }
        }
    }

    void SpawnLinesOfDecals(bool a_horizontal)
    {
        int lines = a_horizontal ? m_buildingRows : m_buildingColumns;
        for (int i = 0; i < lines + 1; i++)
        {
            int decalsToSpawn = VLib.vRandom(2, 8);
            for (int j = 0; j < decalsToSpawn; j++)
            {
                int decalRoll = VLib.vRandom(0, m_roadDecals.Length - 1);

                float scale = GetMapHalfSize().x - 0.75f;
                float posA =  VLib.vRandom(-scale, scale);

                float buildingGap = GetBuildingGap();
                float posB = i * buildingGap;
                posB -= buildingGap * lines / 2f;
                float posBDeviation = m_streetSize / 3.5f;
                posB += VLib.vRandom(-posBDeviation, posBDeviation);

                Vector3 spawnPos = a_horizontal ? new Vector3(posA, posB) : new Vector3(posB, posA);

                Instantiate(m_roadDecals[decalRoll], spawnPos, Quaternion.identity);
            }
        }
    }

    void SpawnRoadDecals()
    {
        //for (int i = 0; i < m_buildingColumns+1; i++)
        //{
        //    int decalRoll = VLib.vRandom(0, m_roadDecals.Length-1);
        //    float buildingGap = GetBuildingGap();
        //    float posX = i * buildingGap;
        //    posX -= buildingGap * m_buildingColumns / 2f;

        //    float yScale = GetMapHalfSize().y;
        //    float posY = VLib.vRandom(-yScale, yScale);

        //    Vector3 spawnPos = new Vector3(posX, posY);

        //    Instantiate(m_roadDecals[decalRoll], spawnPos, Quaternion.identity);
        //}

        SpawnLinesOfDecals(true);
        SpawnLinesOfDecals(false);
    }

    void SpawnEntities()
    {
        if (GameHandler._livesLived > 0)
        {
            while (VLib.vRandom(0, 1) == 0)
            {
                m_lootBagsToSpawn++;
            }
            int[][] spawnPositions = new int[m_lootBagsToSpawn][];
            for (int i = 0; i < m_lootBagsToSpawn; i++)
            {
                spawnPositions[i] = new int[2];
                do
                {
                    spawnPositions[i][0] = VLib.vRandom(0, m_buildingColumns);
                    spawnPositions[i][1] = VLib.vRandom(0, m_buildingRows);
                } while (IsCentreIntersection(spawnPositions[i][0], spawnPositions[i][1]));


                LootBag lootBag = Instantiate(m_lootBagPrefab, GetIntersectionPos(spawnPositions[i][0], spawnPositions[i][1]), Quaternion.identity).GetComponent<LootBag>();
                lootBag.SetBattleHandlerRef(this);
            }
        }
    }

    void SetupMap()
    {
        SpawnBuildings();
        SpawnRoadDecals();
        SpawnOuterWalls();
        SpawnVessels();
        SpawnEntities();
    }

    void MoveToSamsara()
    {
        GameHandler.ChangeScore(m_vesselsLoved + (int)(m_score* (1f + m_lootBagBonus)));
        m_gameEnded = true;
        FindObjectOfType<GameHandler>().TransitionScene(GameHandler.eScene.Samsara);
        GameHandler.IncrementLivesLived();
    }

    void UpdateUI()
    {
        m_speedText.text = m_playerHandlerRef.GetSpeed().ToString("f1") + " m/s";
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
        //ResetBattleExplosionTimer();
    }

    internal void Perish()
    {
        FinishEarly();
    }

    void UpdateBattleTimer()
    {
        if (!m_gameEnding && m_battleTimer.Update())
        {
            m_gameEnding = true;
            m_battleExplosionTimer = new vTimer(2);
            m_battleExplosionTimer.SetUsingUnscaledDeltaTime(true);
        }
        else
        {
            m_clockRadialCircle.SetPieFillAmount(m_battleTimer.GetCompletionPercentage());
        }
    }

    void ResetBattleExplosionTimer()
    {
        m_battleExplosionTimer.Reset();
    }

    void UpdateTimeScale()
    {
        Time.timeScale = Mathf.Min(m_bulletTimeFactor, m_gameEndingTimeFactor, m_cutsceneTimeFactor) * m_pauseTimeFactor;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimeScale();
        if (!m_paused)
        {
            UpdateUI();

            UpdateBattleTimer();

            //Game ending
            if (!m_gameEnded && m_gameEnding)
            {
                if (m_battleExplosionTimer.Update())
                {
                    MoveToSamsara();
                }
                else
                {
                    float percentageFinished = m_battleExplosionTimer.GetCompletionPercentage();
                    m_gameEndingTimeFactor = 1f - percentageFinished;
                    Color whiteoutColor = m_whiteOutImageRef.color;
                    m_whiteOutImageRef.color = new Color(whiteoutColor.r, whiteoutColor.g, whiteoutColor.b, percentageFinished);
                }
            }

            if (Application.isEditor && Input.GetKeyDown(KeyCode.Alpha8))
            {
                Perish();
            }
            if (Application.isEditor && Input.GetKeyDown(KeyCode.Alpha9))
            {
                Enlighten();
            }

            if (Input.GetButtonDown("Start"))
            {
                OpenPauseMenu();
            }
        }

        if (!m_startingCutsceneseChecked && GameHandler._autoRef.IsSceneFadeFinished())
        {
            CheckForStartingCutscenes();
        }

        Cursor.visible = m_paused || GameHandler._upgradeTree.HasUpgrade(UpgradeItem.UpgradeId.MouseAim);
    }

    void OpenPauseMenu()
    {
        Instantiate(m_optionsMenuPrefab, m_uiContainerRef.transform).GetComponent<OptionsMenu>().Init(this);
        SetPaused(true);
    }

    internal void DestroyVessel(Vessel a_vessel)
    {
        m_vesselList.Remove(a_vessel);
        Destroy(a_vessel.gameObject);
    }
}
