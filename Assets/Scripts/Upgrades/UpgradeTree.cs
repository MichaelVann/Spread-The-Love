using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UpgradeItem;

public class UpgradeTree
{
    internal List<UpgradeItem> m_upgradeItemList;

    // Start is called before the first frame update

    internal UpgradeItem GetUpgrade(UpgradeId a_upgradeId) { return m_upgradeItemList[(int)a_upgradeId]; }

    internal bool HasUpgrade(UpgradeId a_upgradeId) { return GetUpgrade(a_upgradeId).IsEnabled(); }

    internal int GetUpgradeLevel(UpgradeId a_upgradeId) { return GetUpgrade(a_upgradeId).m_level; }

    internal float GetUpgradeLeveledStrength(UpgradeId a_upgradeId) { return GetUpgrade(a_upgradeId).GetLeveledStrength(); }

    internal UpgradeTree()
    {
        m_upgradeItemList = new List<UpgradeItem>();
        SetupUpgrades();
    }

    UpgradeItem NewUpgrade(UpgradeItem.UpgradeId a_ID, string a_name, int a_cost, int a_maxLevel, float a_effectStrength, UpgradeItem a_precursorUpgrade, string a_description)
    {
        UpgradeItem upgrade = new UpgradeItem(a_ID,a_name, a_cost, a_maxLevel, a_effectStrength, a_precursorUpgrade, a_description);
        m_upgradeItemList.Add(upgrade);
        return upgrade;
    }

    void SetupUpgrades()
    {
        UpgradeItem mass = NewUpgrade(UpgradeItem.UpgradeId.Mass, "Mass", 30, 10, 0.25f, null, "Increases mass by 25% of base for each level.");
        UpgradeItem acceleration = NewUpgrade(UpgradeItem.UpgradeId.Acceleration, "Acceleration", 30, 10, 0.25f, null, "Increases acceleration by 25% each level.");
        UpgradeItem topSpeed = NewUpgrade(UpgradeItem.UpgradeId.TopSpeed, "Top Speed", 30, 10, 1f, null, "Increases top speed by 1 m/s each level.");
        UpgradeItem turnSpeed = NewUpgrade(UpgradeItem.UpgradeId.TurnSpeed, "Turn Speed", 30, 10, 0.25f, topSpeed, "Increases turn speed by 25% each level.");
        UpgradeItem fireRate = NewUpgrade(UpgradeItem.UpgradeId.FireRate, "Fire Rate", 30, 10, 0.25f, null, "Increases fire rate by 25% each level.");
    }

    internal List<UpgradeItem> GetInitialUpgradeItems()
    {
        List<UpgradeItem> returnList = new List<UpgradeItem>();
        for (int i = 0; i < m_upgradeItemList.Count; i++)
        {
            if (m_upgradeItemList[i].m_precursorUpgrade == null)
            {
                returnList.Add(m_upgradeItemList[i]);
            }
        }
        return returnList;
    }

    void RefreshUpgrades()
    {
        for (int i = 0; i < m_upgradeItemList.Count; i++)
        {
            m_upgradeItemList[i].Refresh();
        }
    }

    internal void AttemptToBuyUpgrade(UpgradeItem a_upgrade)
    {
        float cash = GameHandler._score;
        if (a_upgrade.m_cost <= cash)
        {
            GameHandler.UpdateLastSeenScore(); 
            GameHandler.ChangeScore(-a_upgrade.m_cost);
            a_upgrade.m_level++;
            a_upgrade.m_cost = (int)(a_upgrade.m_cost * a_upgrade.m_costScaling);
            if (!a_upgrade.m_owned)
            {
                a_upgrade.SetOwned(true);
            }
            RefreshUpgrades();
        }
    }
}
