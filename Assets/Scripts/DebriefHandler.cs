using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class DebriefHandler : MonoBehaviour
{
    [SerializeField] BattleHandler m_battleHandlerRef;
    [SerializeField] TextMeshProUGUI m_loveEarnedText;
    [SerializeField] GameObject m_lootBagBonusIconRef;
    [SerializeField] GameObject m_continueButtonRef;
    [SerializeField] ParticleSystem m_lootBagParticleSystem;
    [SerializeField] GameObject m_spreadLoveTextRef;
    RollingText m_loveEarnedRollingText;
    int m_vesselsLoved = 0;
    int m_score = 0;

    bool m_delaying = false;
    vTimer m_delayTimer;
    delegate void Delegate_OnDelayEnd();
    Delegate_OnDelayEnd m_onDelayEndDelegate;
    // Start is called before the first frame update
    void Start()
    {
        m_battleHandlerRef.GetComponent<BattleHandler>();

        ////Setup object active states
        //m_lootBagBonusRef.SetActive(false);
        m_continueButtonRef.gameObject.SetActive(false);
        m_delayTimer = new vTimer(0.5f, true, true, true, true);
        m_delayTimer.m_finished = true;
        m_onDelayEndDelegate = LootBagBonusAppeared;
        m_score = m_battleHandlerRef.GetScore();
    }

    internal void StartDelayTimer()
    {
        m_delayTimer.Reset();
    }

    void LootBagBonusAppeared()
    { 
        if (m_battleHandlerRef.GetLootBagBonus() > 0f)
        {
            m_lootBagParticleSystem.Play();
            m_lootBagBonusIconRef.transform.localScale = new Vector3(1f, 1f, 1f);
            m_loveEarnedRollingText = m_loveEarnedText.AddComponent<RollingText>();
            m_loveEarnedRollingText.Refresh(m_score, m_battleHandlerRef.GetScorePlusLootBagBonus());
            m_loveEarnedRollingText.SetOnRollFinishDelegate(StartDelayTimer);
        }
        else
        {
            StartDelayTimer();
        }
        m_onDelayEndDelegate = ProcessSpreadBonus;
    }

    void ProcessSpreadBonus()
    {
        m_lootBagBonusIconRef.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        m_spreadLoveTextRef.transform.localScale = new Vector3(2f, 2f, 2f);
        m_score = m_battleHandlerRef.GetScorePlusLootBagBonus();
        if (m_loveEarnedRollingText == null)
        {
            m_loveEarnedRollingText = m_loveEarnedText.AddComponent<RollingText>();
        }
        m_loveEarnedRollingText.Refresh(m_score, m_battleHandlerRef.GetTotalScoreEarnedThisBattle());
        m_loveEarnedRollingText.SetOnRollFinishDelegate(StartDelayTimer);
        m_onDelayEndDelegate = ShowContinueButton;
    }

    void ShowContinueButton()
    {
        m_spreadLoveTextRef.transform.localScale = new Vector3(1f, 1f, 1f);
        m_continueButtonRef.SetActive(true);
        m_continueButtonRef.GetComponent<ZoomExpandComponent>().Init();
        Cursor.visible = true;
    }

    public void ContinuePressed()
    {
        m_battleHandlerRef.MoveToSamsara();
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_delayTimer.m_finished && m_delayTimer.Update())
        {
            m_onDelayEndDelegate.Invoke();
        }

        if (m_continueButtonRef.activeInHierarchy && Input.GetButton("Submit"))
        {
            ContinuePressed();
        }
    }
}
