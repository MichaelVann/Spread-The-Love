using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeNodeReadout : MonoBehaviour
{
    UpgradeItem m_upgradeItemRef;
    UpgradeTreeUIHandler m_upgradeTreeUIHandlerRef;
    [SerializeField] TextMeshProUGUI m_titleTextRef;
    [SerializeField] TextMeshProUGUI m_descriptionTextRef;
    [SerializeField] TextMeshProUGUI m_costTextRef;
    [SerializeField] GameObject m_levelDisplayRef;
    [SerializeField] TextMeshProUGUI m_levelTextRef;

    [SerializeField] Button m_purchaseButtonRef;
    [SerializeField] Image m_purchaseButtonImageRef;
    [SerializeField] TextMeshProUGUI m_purchaseButtonTextRef;
    [SerializeField] GameObject m_coinIconRef;

    [SerializeField] GameObject m_toggleButtonRef;
    [SerializeField] UICheckBox m_checkBoxRef;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void SetUp(UpgradeItem a_upgradeItemRef, UpgradeTreeUIHandler a_upgradeTreeUIHandler)
    {
        m_upgradeItemRef = a_upgradeItemRef;
        m_upgradeTreeUIHandlerRef = a_upgradeTreeUIHandler;
        Refresh();
    }

    void UpdatePurchaseButton()
    {
        bool cappedOut = false;
        if (m_upgradeItemRef.m_hasLevels)
        {
            if (m_upgradeItemRef.m_level >= m_upgradeItemRef.m_maxLevel)
            {
                cappedOut = true;
            }
        }
        else if (m_upgradeItemRef.m_owned)
        {
            cappedOut = true;
        }

        if (!cappedOut)
        {
            bool enoughCash = GameHandler._score >= m_upgradeItemRef.m_cost;
            if (enoughCash)
            {
                m_purchaseButtonRef.interactable = true;
                m_purchaseButtonImageRef.color = Color.white;
                m_purchaseButtonTextRef.color = Color.red;
            }
            else
            {
                m_purchaseButtonRef.interactable = false;
                m_purchaseButtonImageRef.color = Color.grey;
                m_purchaseButtonTextRef.color = Color.white;

            }
            m_purchaseButtonTextRef.text = "Purchase";
            m_costTextRef.text = m_upgradeItemRef.m_cost.ToString();
            m_purchaseButtonTextRef.alignment = TextAlignmentOptions.Left;
            m_coinIconRef.SetActive(true);
        }
        else
        {
            m_purchaseButtonRef.interactable = false;
            m_costTextRef.text = "";
            m_purchaseButtonTextRef.alignment = TextAlignmentOptions.Center;
            m_purchaseButtonTextRef.text = "Owned";
            m_purchaseButtonTextRef.color = Color.white;
            m_purchaseButtonImageRef.color = Color.red;
            m_coinIconRef.SetActive(false);
        }
    }

    internal void Refresh()
    {
        m_titleTextRef.text = m_upgradeItemRef.m_name;
        m_descriptionTextRef.text = m_upgradeItemRef.m_description;
        if (m_upgradeItemRef.m_hasLevels)
        {
            m_levelTextRef.text = "LVL " + m_upgradeItemRef.m_level.ToString() + "/" + m_upgradeItemRef.m_maxLevel.ToString();
            m_levelDisplayRef.SetActive(true);
        }
        else
        {
            m_levelDisplayRef.SetActive(false);
        }
        UpdatePurchaseButton();

        m_toggleButtonRef.SetActive(m_upgradeItemRef.m_owned);
        m_checkBoxRef.SetToggled(m_upgradeItemRef.m_toggled);
    }

    public void Toggle()
    {
        m_upgradeItemRef.m_toggled = !m_upgradeItemRef.m_toggled;
        m_upgradeTreeUIHandlerRef.Refresh();
        Refresh();
        GameHandler.AutoSaveCheck();
    }

    public void Purchase()
    {
        m_upgradeTreeUIHandlerRef.AttemptToPurchaseUpgrade(m_upgradeItemRef);
        Refresh();
        GameHandler.AutoSaveCheck();
    }
}
