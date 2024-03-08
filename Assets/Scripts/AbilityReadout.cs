using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityReadout : MonoBehaviour
{
    [SerializeField] GameObject m_lockRef;
    [SerializeField] Image m_upgradeIconRef;
    [SerializeField] UpgradeItem.UpgradeId m_upgradeType;
    [SerializeField] Image m_cooldownRadialIndicator;
    [SerializeField] Image m_borderImageRef;
    [SerializeField] Color m_abilityReadyBorderColor;
    [SerializeField] Image m_resourceIndicatorRef;
    Material m_cooldownMaterialRef;
    UpgradeItem m_upgradeRef;
    [SerializeField] TextMeshProUGUI m_keyNameText;
    
    float m_cooldownAngle = 90f;
    Ability m_abilityRef;

    // Start is called before the first frame update
    void Start()
    {
        m_upgradeIconRef.sprite = GameHandler._autoRef.m_upgradeImages[(int)m_upgradeType];
        bool active = GameHandler._upgradeTree.HasUpgrade(m_upgradeType);
        m_upgradeIconRef.gameObject.SetActive(active);
        m_lockRef.SetActive(!active);
        m_keyNameText.gameObject.SetActive(active);
        m_upgradeRef = GameHandler._upgradeTree.GetUpgrade(m_upgradeType);
        m_keyNameText.text = m_upgradeRef.m_key;
        m_abilityRef = FindObjectOfType<PlayerHandler>().GetAbility(m_upgradeType);
        if (m_abilityRef != null && active)
        {
            if (m_abilityRef.m_usingResource)
            {
                SetupResourceIndicator();
            }
            else
            {
                SetupCooldownIndicator();
            }
        }
        else
        {
            m_resourceIndicatorRef.gameObject.SetActive(false);
            m_cooldownRadialIndicator.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (m_cooldownMaterialRef != null)
        {
            Destroy(m_cooldownMaterialRef);
        }
    }

    void SetupCooldownIndicator()
    {
        m_cooldownRadialIndicator.gameObject.SetActive(true);
        m_cooldownMaterialRef = Instantiate(m_cooldownRadialIndicator.material);
        m_cooldownRadialIndicator.material = m_cooldownMaterialRef;
        if (m_cooldownRadialIndicator.sprite != null)
        {
            m_cooldownMaterialRef.SetTexture("_MainTex", m_cooldownRadialIndicator.sprite.texture);
        }
        m_cooldownMaterialRef.SetFloat("_Angle", 0f);
        m_resourceIndicatorRef.gameObject.SetActive(false);
    }

    void SetupResourceIndicator()
    {
        m_resourceIndicatorRef.gameObject.SetActive(true);
        m_cooldownRadialIndicator.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_abilityRef != null && m_abilityRef.m_enabled)
        {
            if (m_abilityRef.m_usingResource)
            {
                m_resourceIndicatorRef.rectTransform.localScale = new Vector2(1f, m_abilityRef.m_resource);
            }
            else
            {
                float angle = m_abilityRef.m_cooldownTimer.GetCompletionPercentage() * 360f;
                m_cooldownMaterialRef.SetFloat("_Angle", angle);
            }
            m_borderImageRef.color = m_abilityRef.m_ready ? m_abilityReadyBorderColor : Color.black;
        }
    }

    internal void SetUnlocked(bool a_unlocked)
    {
        m_lockRef.SetActive(!a_unlocked);
    }
}
