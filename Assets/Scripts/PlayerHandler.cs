using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandler : MonoBehaviour
{
    [SerializeField] GameObject m_loveVibePrefab;
    [SerializeField] Camera m_cameraRef;

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
            LoveVibe loveVibe = Instantiate(m_loveVibePrefab, transform.position, Quaternion.identity).GetComponent<LoveVibe>();
            loveVibe.Init(null, deltaMousePos.normalized);

        }
        UpdateShootTimer();
    }
}
