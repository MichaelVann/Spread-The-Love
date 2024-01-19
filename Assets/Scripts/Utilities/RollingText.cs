using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RollingText : MonoBehaviour
{
    TextMeshProUGUI m_localTextRef;

    float m_desiredValue = 0;
    float m_currentValue = 0;

    float m_rollTime= 1.5f;
    float m_elapsedTime = 0f;
    int m_shownDecimals = 0;

    internal void SetShownDecimals(int a_decimals) { m_shownDecimals = a_decimals; }

    public void SetDesiredValue(float a_value) { m_desiredValue = a_value; }

    public void SetCurrentValue(float a_value) { m_currentValue = a_value; }

    // Start is called before the first frame update
    void Start()
    {
        m_localTextRef = GetComponent<TextMeshProUGUI>();
        m_localTextRef.text = "" + m_currentValue;
    }

    // Update is called once per frame
    void Update()
    {
        //If roll is not completed
        if (m_desiredValue != m_currentValue && m_desiredValue > m_currentValue)
        {
            m_elapsedTime += Time.deltaTime;
            float value = VLib.Eerp(m_currentValue, m_desiredValue, m_elapsedTime / m_rollTime, 3f);

            //m_currentValue = value;
            //m_currentValue = Mathf.Clamp(m_currentValue, 0f, m_desiredValue);
            m_localTextRef.text = "" + VLib.TruncateFloatsDecimalPlaces(value, m_shownDecimals);
        }
    }
}