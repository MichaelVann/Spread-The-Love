using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability
{
    internal bool m_enabled;

    internal bool m_hasDuration;

    internal struct AbilityDuration
    {
        internal bool active;
        internal vTimer activeTimer;
        internal float duration;
    }
    AbilityDuration m_duration;

    internal vTimer m_cooldownTimer;
    internal float m_cooldownTime;
    internal float m_effectStrength;
    internal bool m_ready;

    internal bool IsActive() { return m_duration.active; }

    internal bool DurationUpdate()
    {
        bool result = m_duration.activeTimer.Update();
        if (result)
        {
            m_duration.active = false;
        }
        return result;
    }

    internal bool AttemptToActivate()
    {
        bool result = false;
        if (m_ready && m_enabled)
        {
            if (m_hasDuration)
            {
                m_duration.active = true;
            }
            m_ready = false;
            result = true;
        }
        return result;
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

    void Init(bool a_enabled, float a_cooldownTime, float a_effectStrength = 0f, float a_duration = 0f)
    {
        m_enabled = a_enabled;
        m_cooldownTimer = new vTimer(a_cooldownTime);
        m_cooldownTime = a_cooldownTime;
        m_effectStrength = a_effectStrength;
        m_ready = true;
        m_hasDuration = a_duration != 0f;

        if (m_hasDuration)
        {
            m_duration = new AbilityDuration();
            m_duration.active = false;
            m_duration.activeTimer = new vTimer(a_duration);
            m_duration.duration = a_duration;
        }
    }

    internal Ability(bool a_enabled, float a_cooldownTime, float a_effectStrength = 0f, float a_duration = 0f)
    {
        Init(a_enabled, a_cooldownTime, a_effectStrength, a_duration);
    }
}