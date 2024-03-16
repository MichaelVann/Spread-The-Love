using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeTreeSelection : MonoBehaviour
{
    [SerializeField] Image m_iconRef;
    [SerializeField] GameObject m_availableUpgradesNotifierRef;
    [SerializeField] TextMeshProUGUI m_availableUpgradesText;
    UpgradeItem m_upgradeItem;
    internal delegate void OnPressedDelegate(int a_index);
    OnPressedDelegate m_pressedDelegate;
    int m_index;

    // Start is called before the first frame update
    void Start()
    {

    }

    internal void Init(OnPressedDelegate a_delegate, int a_index, Sprite a_sprite, UpgradeItem a_upgradeItem)
    {
        m_pressedDelegate = a_delegate;
        m_index = a_index;
        m_iconRef.sprite = a_sprite;
        m_upgradeItem = a_upgradeItem;
        Refresh(GameHandler._score);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void Refresh(int a_cash)
    {
        int upgradesAvailable = m_upgradeItem.GetRecursiveReadyToUpgradeCount(a_cash);
        m_availableUpgradesNotifierRef.SetActive(upgradesAvailable > 0);
        if (upgradesAvailable > 0) 
        {
            m_availableUpgradesText.text = upgradesAvailable.ToString();
        }
    }

    public void ButtonPressed()
    {
        m_pressedDelegate.Invoke(m_index);
    }
}
