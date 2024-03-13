using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpreadPercentageBar : MonoBehaviour
{
    [SerializeField] Image[] m_segments;
    [SerializeField] TextMeshProUGUI[] m_valueTexts;
    [SerializeField] RectTransform m_barContainerRef;
    RectTransform m_rectTransform;

    internal enum eBarTypes
    {
        Love = 0,
        Neutral = 1,
        Fear = 2,
    }

    float[] m_values;

    internal void SetSegmentValues(float a_loved, float a_neutral, float a_fearful)
    {
        SetSegmentValue(eBarTypes.Love, a_loved);
        SetSegmentValue(eBarTypes.Neutral, a_neutral);
        SetSegmentValue(eBarTypes.Fear, a_fearful);
    }

    internal void SetSegmentValue(eBarTypes a_type, float a_value) { m_values[(int)a_type] = a_value; }

    // Start is called before the first frame update
    void Awake()
    {
        m_rectTransform = m_barContainerRef;
        m_values = new float[m_segments.Length];
        SetSegmentValue(eBarTypes.Love, 1f);
        SetSegmentValue(eBarTypes.Neutral, 1f);
        SetSegmentValue(eBarTypes.Fear, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        //UpdateBar();
    }

    internal void UpdateBar()
    {
        float totalWidth = m_rectTransform.sizeDelta.x;
        float totalHeight = m_rectTransform.sizeDelta.y;
        float totalValue = 0f;
        for (int i = 0; i < m_values.Length; i++)
        {
            totalValue += m_values[i];
        }

        float widthUsed = 0f;
        for (int i = 0; i < m_segments.Length; i++)
        {
            m_segments[i].rectTransform.sizeDelta = new Vector2(totalWidth * m_values[i] / totalValue, totalHeight);
            m_segments[i].transform.localPosition = new Vector2(-totalWidth / 2f + m_segments[i].rectTransform.sizeDelta.x / 2f + widthUsed, m_segments[i].transform.localPosition.y);
            widthUsed += m_segments[i].rectTransform.sizeDelta.x;
            m_valueTexts[i].text = m_values[i].ToString(/*"f0"*/); ;
        }
    }
}
