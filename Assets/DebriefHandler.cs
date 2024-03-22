using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebriefHandler : MonoBehaviour
{
    [SerializeField] BattleHandler m_battleHandlerRef;
    [SerializeField] TextMeshProUGUI m_loveEarnedText;
    [SerializeField] GameObject m_lootBagBonusRef;
    [SerializeField] GameObject m_continueButtonRef;

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

        //Setup object active states
        m_loveEarnedText.gameObject.SetActive(true);
        m_lootBagBonusRef.SetActive(false);
        m_continueButtonRef.gameObject.SetActive(false);

        m_delayTimer = new vTimer(0.5f, true, true, true, true);
        m_onDelayEndDelegate = SetupStartingScore;
        m_score = m_battleHandlerRef.GetScore();
    }

    void StartDelayTimer()
    {
        m_delayTimer.Reset();
    }

    void SetupStartingScore()
    {
        m_loveEarnedText.text = m_score.ToString();
        RollingText loveEarnedText = m_loveEarnedText.GetComponent<RollingText>();
        loveEarnedText.SetCurrentValue(0);
        loveEarnedText.SetDesiredValue(m_score);
        loveEarnedText.SetRollTime(1f);
        loveEarnedText.SetOnRollFinishDelegate(StartDelayTimer);
        m_onDelayEndDelegate = ScoreEarnedTextRollFinished;
    }

    void ScoreEarnedTextRollFinished()
    {
        m_lootBagBonusRef.SetActive(true);
        ZoomExpandComponent lootBagZoomExpandComponent = m_lootBagBonusRef.GetComponent<ZoomExpandComponent>();
        lootBagZoomExpandComponent.Init();
        lootBagZoomExpandComponent.SetFinishDelegate(LootBagBonusAppeared);
        //m_onDelayEndDelegate = LootBagBonusAppeared;
    }

    void LootBagBonusAppeared()
    {
        RollingText loveEarnedText = m_loveEarnedText.GetComponent<RollingText>();
        m_lootBagBonusRef.GetComponentInChildren<TextMeshProUGUI>().text = "+" + (m_battleHandlerRef.GetLootBagBonus() * 100f).ToString("f0") + "%";
        if (m_battleHandlerRef.GetLootBagBonus() > 0f)
        {
            loveEarnedText.Refresh(m_score, m_battleHandlerRef.GetScorePlusLootBagBonus());
            loveEarnedText.SetOnRollFinishDelegate(StartDelayTimer);
            m_onDelayEndDelegate = ShowContinueButton;
        }
        else
        {
            StartDelayTimer();
            m_onDelayEndDelegate = ShowContinueButton;
        }
    }

    void ShowContinueButton()
    {
        m_continueButtonRef.SetActive(true);
        m_continueButtonRef.GetComponent<ZoomExpandComponent>().Init();
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_delayTimer.m_finished && m_delayTimer.Update())
        {
            m_onDelayEndDelegate.Invoke();
        }
    }
}
