using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUINode : MonoBehaviour
{
    GameHandler m_gameHandlerRef;

    internal UpgradeItem m_upgradeItemRef;
    [SerializeField] TextMeshProUGUI m_nameText;
    internal float m_availableSpace = -1f;
    UpgradeTreeUIHandler m_upgradeTreeUIHandler;
    [SerializeField] Image m_backdropRef;
    [SerializeField] Image m_iconRef;
    [SerializeField] Sprite[] m_possibleIconRefs;
    [SerializeField] Sprite m_lockIconRef;
    [SerializeField] Button m_buttonRef;
    [SerializeField] Image m_tickCrossRef;
    [SerializeField] Sprite m_tickSpriteRef;
    [SerializeField] Sprite m_crossSpriteRef;

    [SerializeField] GameObject m_levelIndicator;
    [SerializeField] TextMeshProUGUI m_levelText;

    internal void SetNameText(string a_name) { m_nameText.text = a_name;}
    internal void SetAvailableSpace(float a_space) { m_availableSpace = a_space;}

    // Start is called before the first frame update
    void Awake()
    {
        m_gameHandlerRef = FindObjectOfType<GameHandler>();
    }

    internal void SetUp(UpgradeItem a_upgradeItem, UpgradeTreeUIHandler a_upgradeTreeUIHandler)
    {
        m_upgradeItemRef = a_upgradeItem;
        m_upgradeTreeUIHandler = a_upgradeTreeUIHandler;
        Refresh();
    }

    internal void Refresh()
    {
        m_iconRef.sprite = m_possibleIconRefs[(int)m_upgradeItemRef.m_ID];

        Color nodeColor = Color.white;
        bool interactable = true;

        if (m_upgradeItemRef.m_owned)
        {
            nodeColor = m_gameHandlerRef.m_loveColor;

            if (m_upgradeItemRef.m_toggleable)
            {
                m_tickCrossRef.sprite = m_upgradeItemRef.m_toggled ? m_tickSpriteRef : m_crossSpriteRef;
            }
            m_tickCrossRef.gameObject.SetActive(m_upgradeItemRef.m_toggleable);

            m_tickCrossRef.color = m_upgradeItemRef.m_toggled ? Color.green : Color.red;
        }
        else
        {
            if (!m_upgradeItemRef.m_unlocked)
            {
                m_iconRef.sprite = m_lockIconRef;
                interactable = false;
                nodeColor = Color.gray;
            }
            else if (m_upgradeItemRef.m_cost > GameHandler._score)
            {
                nodeColor = Color.gray;
            }
            else
            {
                nodeColor = Color.white;
            }
            m_tickCrossRef.gameObject.SetActive(false);

        }

        if (m_upgradeItemRef.m_hasLevels)
        {
            m_levelIndicator.SetActive(true);
            m_levelText.text = m_upgradeItemRef.m_level + "/" + m_upgradeItemRef.m_maxLevel;
        }
        else
        {
            m_levelIndicator.SetActive(false);
        }

        m_backdropRef.color = nodeColor;
        m_buttonRef.interactable = interactable;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ButtonPressed()
    {
        m_upgradeTreeUIHandler.SelectUpgrade(this);
    }
}
