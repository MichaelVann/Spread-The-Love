using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Totem : MonoBehaviour
{
    [SerializeField] GameObject m_waveRef;
    [SerializeField] ObjectShadow m_shadowRef;
    vTimer m_activeTimer;
    vTimer m_cooldownTimer;

    //Jump
    Vector3 m_startingPos;
    vTimer m_jumpTimer;
    float m_jumpHeight = 1f;

    bool m_functioning = false;
    bool m_coolingDown = false;
    float m_maxWaveScale = 1f;

    // Start is called before the first frame update
    void Start()
    {
        m_jumpTimer = new vTimer(1f, true);
        m_jumpTimer.SetClampingTimer(true);
        m_activeTimer = new vTimer(0.4f);
        m_cooldownTimer = new vTimer(1f/ GameHandler._upgradeTree.GetUpgradeLeveledStrength(UpgradeItem.UpgradeId.TotemFireRate));
        m_startingPos = transform.position;
        m_maxWaveScale *= GameHandler._upgradeTree.GetUpgradeLeveledStrength(UpgradeItem.UpgradeId.TotemRadius);
    }


    // Update is called once per frame
    void Update()
    {
        if (m_functioning)
        {
            if (!m_coolingDown)
            {
                if (m_activeTimer.Update())
                {
                    m_coolingDown = true;
                    m_waveRef.SetActive(false);
                }
                float scale = VLib.Eerp(0f, m_maxWaveScale, m_activeTimer.GetCompletionPercentage(), 1f);
                m_waveRef.transform.localScale = new Vector3(scale, scale, scale);
            }
            else
            {
                if (m_cooldownTimer.Update())
                {
                    m_waveRef.SetActive(true);
                    m_coolingDown = false;
                }
            }
        }
        else //Jump update
        {
            if (m_jumpTimer.Update())
            {
                m_functioning = true;
                m_waveRef.SetActive(true);
                m_waveRef.transform.localScale = new Vector3(0f, 0f, 1f);
            }

            Vector3 jumpVector = new Vector3(0f, Mathf.Sin(m_jumpTimer.GetCompletionPercentage() * Mathf.PI) * m_jumpHeight, 0f);
            transform.position = m_startingPos + jumpVector;
            m_shadowRef.m_height = jumpVector.y;
            transform.localScale = new Vector3(1f,1f,1f) * (1f + (jumpVector.y / 1f));
        }
    }

    private void OnTriggerEnter2D(Collider2D a_collision)
    {
        Vessel vessel = a_collision.GetComponent<Vessel>();
        vessel.AddEmotion(1);
    }
}
