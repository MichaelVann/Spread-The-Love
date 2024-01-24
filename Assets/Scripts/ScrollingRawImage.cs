using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollingRawImage : MonoBehaviour
{
    [SerializeField] RawImage m_imageRef;
    [SerializeField] float m_scrollDirectionX, m_scrollDirectionY;
    [SerializeField] float m_speed;
    Vector2 m_additionalOffset;
    Vector2 m_timeOffset;
    internal void SetAdditionalOffset(Vector2 a_offset) { m_additionalOffset = a_offset; }

    // Start is called before the first frame update
    void Start()
    {
        m_additionalOffset = Vector2.zero;
        m_timeOffset = Vector2.zero;
    }

    // Update is called once per frame
    void Update()
    {
        m_timeOffset += new Vector2(m_scrollDirectionX, m_scrollDirectionY).normalized * m_speed * Time.deltaTime;
        Vector2 uvPos = m_timeOffset + m_additionalOffset * 0.1f;

        m_imageRef.uvRect = new Rect(uvPos, m_imageRef.uvRect.size) ;
    }
}
