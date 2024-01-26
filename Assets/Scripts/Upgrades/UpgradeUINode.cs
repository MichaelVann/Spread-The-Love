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
    [SerializeField] TextMeshProUGUI m_costText;
    [SerializeField] GameObject m_costPlateRef;
    [SerializeField] ParticleSystem m_upgradeEffectParticleSystemRef;

    [SerializeField] GameObject m_availableUpgradeIndicatorRef;

    [SerializeField] Color m_baseColor;
    [SerializeField] Color m_notPurchaseableColor;

    //Selection
    [SerializeField] GameObject m_selectionRing;
    bool m_selected = false;
    float m_selectionRotationSpeed = 60f;

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

    void RefreshLevelIndicator()
    {
        if (m_upgradeItemRef.m_hasLevels)
        {
            m_levelIndicator.SetActive(true);
            m_levelText.text = m_upgradeItemRef.m_level + "/" + m_upgradeItemRef.m_maxLevel;
        }
        else
        {
            m_levelIndicator.SetActive(false);
        }
    }

    void RefreshAvailableUpgradeIndicator()
    {
        m_availableUpgradeIndicatorRef.SetActive(m_upgradeItemRef.IsReadyToUpgrade(GameHandler._score));
    }

    internal void Refresh()
    {
        m_iconRef.sprite = m_possibleIconRefs[(int)m_upgradeItemRef.m_ID];

        Color nodeColor = Color.white;
        bool interactable = true;

        if (m_upgradeItemRef.m_owned)
        {
            bool purchaseable = GameHandler._score >= m_upgradeItemRef.m_cost;
            purchaseable |= m_upgradeItemRef.m_level >= m_upgradeItemRef.m_maxLevel;
            purchaseable |= !m_upgradeItemRef.m_hasLevels;
            nodeColor = purchaseable ? m_baseColor : m_notPurchaseableColor;

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

        RefreshLevelIndicator();

        RefreshAvailableUpgradeIndicator();

        m_backdropRef.color = nodeColor;
        m_buttonRef.interactable = interactable;
        if (m_upgradeItemRef.m_level < m_upgradeItemRef.m_maxLevel)
        {
            m_costText.text = m_upgradeItemRef.m_cost.ToString();
        }
        else
        {
            m_costPlateRef.SetActive(false);
        }
    }

    internal void SetSelectedStatus(bool a_selected)
    {
        m_selected = a_selected;
        m_selectionRing.SetActive(m_selected);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_selected)
        {
            m_selectionRing.transform.eulerAngles += new Vector3(0f, 0f, -Time.deltaTime * m_selectionRotationSpeed);
        }
    }

    public void ButtonPressed()
    {
        m_upgradeTreeUIHandler.SelectUpgrade(this);
    }

    internal void RunUpgradeEffect()
    {
        m_upgradeEffectParticleSystemRef.Play();
    }
}
