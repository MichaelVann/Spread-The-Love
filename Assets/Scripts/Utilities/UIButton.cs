using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButton : MonoBehaviour
{
    [SerializeField] Button m_buttonRef;
    [SerializeField] GameObject m_shineRef;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Refresh()
    {
        m_shineRef.SetActive(m_buttonRef.interactable);
    }

    // Update is called once per frame
    void Update()
    {
        Refresh();
    }
}
