using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollingRawImage : MonoBehaviour
{
    [SerializeField] RawImage m_imageRef;
    [SerializeField] float m_scrollDirectionX, m_scrollDirectionY;
    [SerializeField] float m_speed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        m_imageRef.uvRect = new Rect(m_imageRef.uvRect.position + new Vector2(m_scrollDirectionX, m_scrollDirectionY).normalized * m_speed * Time.deltaTime, m_imageRef.uvRect.size);
    }
}
