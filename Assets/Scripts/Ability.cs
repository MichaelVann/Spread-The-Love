using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability
{
    internal bool m_enabled;

    internal bool m_hasDuration;
    internal bool m_active;

    internal struct AbilityDuration
    {
        internal vTimer activeTimer;
        internal float duration;
    }
    AbilityDuration m_duration;

    internal bool m_usingResource;
    internal float m_resource;
    internal float m_resourceDrain;
    internal float m_resourceRegen;
    internal float m_minResourceNeededToActivate;
    bool m_wasActiveLastFrame;

    internal vTimer m_cooldownTimer;
    internal float m_cooldownTime;
    internal float m_effectStrength;
    internal bool m_ready;

    internal bool IsActive() { return m_active; }

    internal void SetActive(bool a_active) { m_active = a_active; }

    internal void DurationUpdate()
    {
        if (m_active && m_duration.activeTimer.Update())
        {
            m_active = false;
        }
    }

    internal bool AttemptToActivate()
    {
        bool result = false;
        if (m_ready && m_enabled)
        {
            if (m_hasDuration || m_usingResource)
            {
                m_active = true;
            }
            m_ready = false;
            result = true;
        }
        return result;
    }

    internal bool SetResourceBasedActivateInput(bool a_input)
    {
        m_active = false;

        if (a_input && m_ready)
        {
            m_active = true;
        }
        return m_active;
    }

    internal bool UpdateCooldown()
    {
        bool result = false;
        if (!m_ready)
        {
            if (m_cooldownTimer.Update(ref m_cooldownTime))
            {
                m_ready = true;
                result = true;
            }
        }
        return result;
    }

    internal void ResourceUpdate()
    {
        if (m_active)
        {
            m_resource -= m_resourceDrain * Time.deltaTime;
            //m_active = false;
            m_ready = m_resource > 0;
            if (m_resource <= 0f)
            {
                m_active = false;
            }
        }
        else
        {
            m_resource += m_resourceRegen * Time.deltaTime;
            m_ready = m_resource >= m_minResourceNeededToActivate;
        }

        m_wasActiveLastFrame = m_active;

        m_resource = Mathf.Clamp(m_resource, 0f, 1f);
    }

    void Init(bool a_enabled, float a_effectStrength = 0f, float a_duration = 0f)
    {
        m_enabled = a_enabled;

        m_effectStrength = a_effectStrength;
        m_ready = true;
        m_hasDuration = a_duration != 0f;
        m_active = false;
        m_wasActiveLastFrame = false;

        if (m_hasDuration)
        {
            m_duration = new AbilityDuration();
            m_duration.activeTimer = new vTimer(a_duration);
            m_duration.duration = a_duration;
        }
    }

    internal void SetUpCooldown(float a_cooldownTime)
    {
        m_cooldownTimer = new vTimer(a_cooldownTime);
        m_cooldownTime = a_cooldownTime;
    }

    internal void SetUpResource(float a_resourceDrainRate, float a_resourceRegenRate, float a_minResourceNeededToActivate = 0.2f)
    {
        m_resource = 1f;
        m_usingResource = true;
        m_resourceDrain = a_resourceDrainRate;
        m_resourceRegen = a_resourceRegenRate;
        m_minResourceNeededToActivate = a_minResourceNeededToActivate;
    }

    internal Ability(bool a_enabled, float a_effectStrength = 0f, float a_duration = 0f)
    {
        Init(a_enabled, a_effectStrength, a_duration);
    }
}