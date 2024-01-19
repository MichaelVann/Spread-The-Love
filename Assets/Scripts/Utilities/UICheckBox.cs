using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICheckBox : MonoBehaviour
{
    public GameObject m_tickRef;
    bool m_checked = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateTickVisibility()
    {
        m_tickRef.SetActive(m_checked);
    }

    public void Toggle()
    {
        m_checked = !m_checked;
        UpdateTickVisibility();
    }

    public void SetToggled(bool a_value)
    {
        m_checked = a_value;
        UpdateTickVisibility();
    }
}
