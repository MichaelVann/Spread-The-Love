using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRamp : MonoBehaviour
{
    [SerializeField] GameObject m_rampRef;
    [SerializeField] Image m_rampImageRef;
    // Start is called before the first frame update
    void Start()
    {
        SetRampPercent(0f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void SetColor(Color a_color)
    {
        m_rampImageRef.color = a_color;
    }

    internal void SetRampPercent(float a_percent)
    {
        m_rampRef.transform.localScale = new Vector3(a_percent, a_percent, 1f);
    }
}
