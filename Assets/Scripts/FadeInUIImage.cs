using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeInUIImage : MonoBehaviour
{
    Image m_imageRef;
    vTimer m_timer;
    // Start is called before the first frame update
    void Start()
    {
        Color color = m_imageRef.color;
        color.a = 0f;
        m_imageRef.color = color;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_timer.Update())
        {
            Destroy(this);
        }
        UpdateColor();
    }

    internal void Init(float a_fadeTime)
    {
        m_timer = new vTimer(a_fadeTime);
    }

    void UpdateColor()
    {
        Color color = m_imageRef.color;
        color.a = m_timer.GetCompletionPercentage();
        m_imageRef.color = color;
    }
}
