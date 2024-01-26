using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class RollingText : MonoBehaviour
{
    TextMeshProUGUI m_localTextRef;

    float m_desiredValue = 0;
    float m_currentValue = 0;

    float m_rollTime= 1.5f;
    float m_elapsedTime = 0f;
    int m_shownDecimals = 0;

    bool m_inBrackets = false;
    bool m_hasPlusOrMinusSign = false;

    //Destruction
    bool m_destroyGameObjectOnFinish = false;
    float m_destructionTime = 1f;
    vTimer m_destructionTimer;

    internal void SetShownDecimals(int a_decimals) { m_shownDecimals = a_decimals; }

    public void SetDesiredValue(float a_value) { m_desiredValue = a_value; }

    internal void SetRollTime(float a_time) { m_rollTime = a_time; }

    public void SetCurrentValue(float a_value) { m_currentValue = a_value; }

    internal void SetBrackets(bool a_value) { m_inBrackets = a_value; }
    internal void SetPlusMinusSign(bool a_value) { m_hasPlusOrMinusSign = a_value; }

    internal void SetDestroyGameObjectOnFinish(bool a_value) { m_destroyGameObjectOnFinish = a_value; }

    // Start is called before the first frame update
    void Start()
    {
        m_localTextRef = GetComponent<TextMeshProUGUI>();
        m_localTextRef.text = "" + m_currentValue;
    }

    internal void Refresh(float a_currentValue, float a_desiredValue)
    {
        SetCurrentValue(a_currentValue);
        SetDesiredValue(a_desiredValue);
        m_elapsedTime = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        //If roll is not completed
        if (m_elapsedTime < m_rollTime )//m_desiredValue != m_currentValue && m_desiredValue > m_currentValue)
        {
            m_elapsedTime += Time.deltaTime;
            float value = VLib.Eerp(m_currentValue, m_desiredValue, m_elapsedTime / m_rollTime, 3f);

            //m_currentValue = value;
            //m_currentValue = Mathf.Clamp(m_currentValue, 0f, m_desiredValue);
            string text = m_inBrackets ? "(" : "";
            text += m_hasPlusOrMinusSign ? (value >= 0 ? "+" : "-") : "";
            text += VLib.RoundToDecimalPlaces(value, m_shownDecimals);
            text += m_inBrackets ? ")" : ""; 
            m_localTextRef.text = text;
        }
        else if (m_destroyGameObjectOnFinish)
        {
            if (m_destructionTimer == null)
            {
                m_destructionTimer = new vTimer(m_destructionTime);
            }
            if (m_destructionTimer.Update())
            {
                Destroy(gameObject);
            }
            Color currentColor = m_localTextRef.color;
            m_localTextRef.color = new Color(currentColor.r, currentColor.g, currentColor.b, 1f-m_destructionTimer.GetCompletionPercentage());
        }
    }
}