using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

[Serializable]
public class UpgradeItem
{
    public string m_name;
    public string m_description;

    public int m_cost;
    public float m_costScaling = 1.2f;
    public bool m_owned = false;
    public bool m_hasLevels = false;
    public int m_level = 0;
    public int m_maxLevel = 0;

    public bool m_unlocked;

    //Toggle
    internal bool m_toggled = true;
    internal bool m_toggleable = false;

    public float m_effectStrength;

    internal UpgradeItem m_precursorUpgrade;
    internal List<UpgradeItem> m_upgradeChildren;

    public enum UpgradeId
    {
        Mass,
        Braking,
        Acceleration,
        TopSpeed,
        TurnSpeed,
        Aquaplane,
        //DriftSpread,
        Shooting,
        FireRate,
        ShootSpread,
        AutoShoot,
        MouseAim,
        //Radar,
        //Minimap,
        AdditionalTime
    }
    public UpgradeId m_ID;

    public void SetName(string a_string) { m_name = a_string; }
    public void SetDescription(string a_string) { m_description = a_string; }
    public void SetCost(int a_cost) { m_cost = a_cost; }
    public void SetCostScaling(float a_cost) { m_costScaling = a_cost; }

    public void SetOwned(bool a_value) { m_owned = a_value; }

    public void SetPrecursorUpgrade(UpgradeItem a_upgradeItem) { m_precursorUpgrade = a_upgradeItem;}
    public void SetHasLevels(bool a_value) { m_hasLevels = a_value; }

    public void SetLevel(int a_value) { m_level = a_value; }
    public void SetMaxLevel(int a_value) { m_maxLevel = a_value; m_hasLevels = true; }

    internal void SetID(UpgradeId a_ID) { m_ID = a_ID; }

    internal void SetToggled(bool a_value) { m_toggled = a_value; }

    internal void SetToggleable(bool a_value) { m_toggleable = a_value; }

    internal bool IsEnabled() { return m_toggled && m_owned; }

    internal float GetLeveledStrength() { return m_effectStrength * m_level; }
    internal float GetMaxLeveledStrength() { return m_effectStrength * m_maxLevel; }

    internal bool IsReadyToUpgrade(float a_cash) { return m_unlocked && a_cash >= m_cost && (m_hasLevels ? (m_level < m_maxLevel) : true); }

    internal void AddChildUpgrade(UpgradeItem a_child)
    {
        m_upgradeChildren.Add(a_child);
    }

    public UpgradeItem()
    {
        m_upgradeChildren = new List<UpgradeItem>();
    }

    public UpgradeItem(UpgradeId a_ID, string a_name, int a_cost, int a_maxLevel, float a_effectStrength, UpgradeItem a_precursorUpgrade, string a_description, bool a_toggleable)
    {
        m_upgradeChildren = new List<UpgradeItem>();
        SetID(a_ID);
        SetName(a_name);
        SetDescription(a_description);
        SetCost(a_cost);
        SetPrecursorUpgrade(a_precursorUpgrade);
        if (a_precursorUpgrade != null)
        {
            a_precursorUpgrade.AddChildUpgrade(this);
        }
        if (a_maxLevel > 0)
        {
            SetMaxLevel(a_maxLevel);
            m_hasLevels = true;
        }
        m_effectStrength = a_effectStrength;
        m_toggleable = a_toggleable;
        Refresh();
    }

    public void Copy(UpgradeItem a_upgradeItem)
    {
        m_name = a_upgradeItem.m_name;
        m_description = a_upgradeItem.m_description;

        m_cost = a_upgradeItem.m_cost;
        m_costScaling = a_upgradeItem.m_costScaling;
        m_owned = a_upgradeItem.m_owned;
        m_hasLevels = a_upgradeItem.m_hasLevels;
        m_level = a_upgradeItem.m_level;
        m_maxLevel = a_upgradeItem.m_maxLevel;
    }

    internal void Refresh()
    {
        m_unlocked = true;
        if (m_precursorUpgrade != null)
        {
            m_unlocked = m_precursorUpgrade.m_owned;
        }
    }
}
