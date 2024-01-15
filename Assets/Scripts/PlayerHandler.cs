using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandler : Soul
{
    [SerializeField] GameObject m_loveVibePrefab;
    [SerializeField] Camera m_cameraRef;

    BattleHandler m_battleHandlerRef;

    bool m_readyToShoot = false;
    [SerializeField] float m_fireRate;
    vTimer m_shootTimer;

    void Awake()
    {
        m_shootTimer = new vTimer(m_fireRate);
    }

    // Start is called before the first frame update
    void Start()
    {
        m_emotion = new Vector2(1f, 1f);
        m_battleHandlerRef = FindObjectOfType<BattleHandler>();
        CalculateEmotionColor();
    }

    void UpdateShootTimer()
    {
        if (!m_readyToShoot && m_shootTimer.Update())
        {
            m_readyToShoot = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 worldMousePoint = m_cameraRef.ScreenToWorldPoint(Input.mousePosition);

        Vector2 deltaMousePos =  worldMousePoint - transform.position;
        //deltaMousePos = new Vector2(1f,0f);
        if (Input.GetMouseButton(0) && m_readyToShoot)
        {
            m_readyToShoot = false;
            Vibe loveVibe = Instantiate(m_loveVibePrefab, transform.position, Quaternion.identity).GetComponent<Vibe>();
            loveVibe.Init(m_battleHandlerRef, null, deltaMousePos.normalized, m_emotion, 10f);
        }
        else if (Input.GetMouseButton(1) && m_readyToShoot)
        {
            m_readyToShoot = false;
            Vibe loveVibe = Instantiate(m_loveVibePrefab, transform.position, Quaternion.identity).GetComponent<Vibe>();
            loveVibe.Init(m_battleHandlerRef, null, deltaMousePos.normalized, new Vector2(0f,0f), 10f);
        }
        UpdateShootTimer();
    }
}
