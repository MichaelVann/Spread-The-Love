using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockRadialCircle : MonoBehaviour
{
    [SerializeField] SpriteRenderer m_spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void SetPieFillAmount(float a_value)
    {
        m_spriteRenderer.material.SetFloat("_Arc2", (1f - a_value) * 360f);
        Color color = VLib.RatioToColorRGB(1f - a_value);
        color.a = a_value;
        m_spriteRenderer.color = color;
    }
}
