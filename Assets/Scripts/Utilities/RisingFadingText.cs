using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RisingFadingText : MonoBehaviour
{
    Image m_imageRef;
    float m_lifeTimer = 0f;
    float m_lifeTimerMax = 0.8f;
    float m_risingSpeed = 0.25f;
    float m_horizontalSpeed = 0.2f;
    const float m_fadeDelay = 0.2f;
    bool m_gravityAffected = true;

    Color m_firstColor;
    Color m_secondColor;
    Vector3 m_originalPosition;
    float m_originalScale = 1f;

    Vector3 m_slideDirection = Vector3.zero;

    bool m_newStyle = false;

    float m_firstColorChangeTime = 0.148f;
    float m_peakTime = 0.204f;
    float m_firstColorFadeTime = 0.241f;
    float m_landingTime = 1/3f;
    float m_fadeStartTime = 0.574f;

    public void SetGravityAffected(bool a_value) { m_gravityAffected = a_value; }
    public void SetHorizontalSpeed(float a_value) { m_horizontalSpeed = a_value; }
    public void SetLifeTimer(float a_value) { m_lifeTimer = a_value; }
    public void SetLifeTimerMax(float a_value) { m_lifeTimerMax = a_value; }

    public void SetImageEnabled(bool a_value) { m_imageRef.enabled = a_value; }
    public void SetOriginalScale(float a_value) { m_originalScale = a_value; }
    public void SetOriginalPosition(Vector3 a_value) { m_originalPosition = a_value; }

    void Awake()
    {
        m_firstColor = GetComponent<Text>().color;
        m_originalPosition = transform.position;
        m_horizontalSpeed = Random.Range(-0.3f, 0.3f);
        m_risingSpeed = Random.Range(0.25f, 0.3f);
        m_imageRef = GetComponentInChildren<Image>();
    }

    public void SetOriginalColor(Color a_color)
    {
        m_firstColor = a_color;
        GetComponent<Text>().color = a_color;
    }

    public void SetTextContent(string a_string)
    {
        GetComponent<Text>().text = a_string;
    }

    public void SetTextContent(float a_float)
    {
        GetComponent<Text>().text = ("" + VLib.TruncateFloatsDecimalPlaces(a_float, 2));
    }

    internal void SetUp(string a_string, Color a_firstColor, Color a_secondColor)
    {
        SetTextContent(a_string);
        m_lifeTimerMax = 0.74f;
        m_newStyle = true;
        m_slideDirection = new Vector3(VLib.vRandom(-1f, 1f), 1f, 0f).normalized;

        m_firstColor = a_firstColor;
        m_secondColor = a_secondColor;

        m_firstColorChangeTime *= m_lifeTimerMax;
        m_peakTime             *= m_lifeTimerMax;
        m_firstColorFadeTime   *= m_lifeTimerMax;
        m_landingTime          *= m_lifeTimerMax;
        m_fadeStartTime        *= m_lifeTimerMax;

        SetImageEnabled(false);
    }

    void StyleUpdate1()
    {
        float completionPerc = m_lifeTimer / m_lifeTimerMax;

        float verticalOffset = m_gravityAffected ? Mathf.Sin(completionPerc * Mathf.PI) * m_risingSpeed : completionPerc * m_risingSpeed / 2f;

        float scale = Mathf.Pow(Mathf.Clamp(Mathf.Sin(Mathf.Pow(completionPerc, 0.5f) * Mathf.PI), 0f, 1f), 0.2f);
        scale *= m_originalScale;
        transform.position = m_originalPosition + new Vector3(m_horizontalSpeed * completionPerc, verticalOffset);
        transform.localScale = new Vector3(scale, scale, 1f);
        if (m_lifeTimer >= m_fadeDelay)
        {
            float colorScale = 1f - Mathf.Pow((m_lifeTimer - m_fadeDelay) / (m_lifeTimerMax - m_fadeDelay), 2f);
            float exponentialColorScale = Mathf.Pow(colorScale, 3f);
            GetComponent<Text>().color = new Color(m_firstColor.r * exponentialColorScale, m_firstColor.g * exponentialColorScale, m_firstColor.b * exponentialColorScale, m_firstColor.a * colorScale);
        }
    }

    void StyleUpdate2()
    {

        float scale = -1f;
        float maxScale = 2f * m_originalScale;
        Color fadedColor = new Color(m_secondColor.r, m_secondColor.g, m_secondColor.b, 0f);

        Color color = Color.white;
        float lerpTime;

        float speed = 0.1f;

        switch (m_lifeTimer)
        {
            case float n when (n > 0 && n < m_firstColorChangeTime):
                scale = Mathf.Lerp(0f, maxScale, n / m_peakTime);
                break;

            case float n when (n > m_firstColorChangeTime && n < m_peakTime):
                scale = Mathf.Lerp(0f, maxScale, n / m_peakTime);
                lerpTime = (n- m_firstColorChangeTime) / (m_peakTime - m_firstColorChangeTime);
                color = Color.Lerp(Color.white, m_firstColor, lerpTime);
                break;

            case float n when (n > m_peakTime && n < m_firstColorFadeTime):
                lerpTime = (n - m_peakTime) / (m_landingTime - m_peakTime);
                scale = Mathf.Lerp(maxScale, m_originalScale, lerpTime);
                break;

            case float n when (n > m_firstColorFadeTime && n < m_landingTime):
                lerpTime = (n - m_peakTime) / (m_landingTime - m_peakTime);
                scale = Mathf.Lerp(maxScale, m_originalScale, lerpTime);
                lerpTime = (n - m_firstColorFadeTime) / (m_landingTime - m_firstColorFadeTime);
                color = Color.Lerp(m_firstColor, m_secondColor, lerpTime);
                break;

            case float n when (n > m_landingTime && n < m_fadeStartTime):
                color = m_secondColor;
                scale = m_originalScale;
                break;

            case float n when (n > m_fadeStartTime && n < m_lifeTimerMax):
                lerpTime = (n - m_fadeStartTime) / (m_lifeTimerMax - m_fadeStartTime);
                color = Color.Lerp(m_secondColor, fadedColor, lerpTime);
                scale = m_originalScale;
                break;
            default:
                break;
        }
        //scale *= m_originalScale;
        transform.localScale = new Vector3(scale, scale, 1f);
        GetComponent<Text>().color = color;

        transform.position = m_originalPosition + m_slideDirection * (m_lifeTimer/m_lifeTimerMax) * speed * scale;

    }

    void LifeTimerUpdate()
    {
        m_lifeTimer += Time.unscaledDeltaTime;
        if (m_lifeTimer >= m_lifeTimerMax)
        {
            Destroy(m_imageRef.gameObject);
            Destroy(gameObject);
        }

        if (m_newStyle)
        {
            StyleUpdate2();
        }
        else
        {
            StyleUpdate1();
        }

    }

    void Update()
    {
        LifeTimerUpdate();
    }
}
