using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UpgradeItem;

public class UpgradeTree
{
    internal List<UpgradeItem> m_upgradeItemList;

    // Start is called before the first frame update

    internal UpgradeItem GetUpgrade(UpgradeItem.UpgradeId a_upgradeId) { return m_upgradeItemList[(int)a_upgradeId]; }

    internal bool HasUpgrade(UpgradeItem.UpgradeId a_upgradeId) { return GetUpgrade(a_upgradeId).IsEnabled(); }

    internal int GetUpgradeLevel(UpgradeItem.UpgradeId a_upgradeId) { return GetUpgrade(a_upgradeId).m_level; }

    internal UpgradeTree()
    {
        m_upgradeItemList = new List<UpgradeItem>();
        SetupUpgrades();
    }

    UpgradeItem NewUpgrade(UpgradeItem.UpgradeId a_ID, string a_name, int a_cost, int a_maxLevel, UpgradeItem a_precursorUpgrade, string a_description)
    {
        UpgradeItem upgrade = new UpgradeItem(a_ID,a_name, a_cost, a_maxLevel, a_precursorUpgrade, a_description);
        m_upgradeItemList.Add(upgrade);
        return upgrade;
    }

    void SetupUpgrades()
    {
        UpgradeItem mass = NewUpgrade(UpgradeItem.UpgradeId.Mass, "Mass", 20, 5, null, "Increases mass by 10% of base for each level.");
        UpgradeItem topSpeed = NewUpgrade(UpgradeItem.UpgradeId.TopSpeed, "Top Speed", 50, 5, null, "Increases top speed by 1 m/s each level.");
        UpgradeItem turnSpeed = NewUpgrade(UpgradeItem.UpgradeId.TurnSpeed, "Turn Speed", 30, 5, topSpeed, "Increases turn speed by 5% each level.");
        //UpgradeItem flingPrediction = NewUpgrade(UpgradeItem.UpgradeId.flingPredictor, "Fling Prediction", 50, 0, enemyVector, "Gives an indication of the fling direction.");
        //UpgradeItem comboHits = NewUpgrade(UpgradeItem.UpgradeId.comboHits, "Combo Hits", 100, 0, flingPrediction, "Adds 10% extra damage on each hit per turn.");
        //
        //
        //UpgradeItem nucleusHealthUpgrade = NewUpgrade(UpgradeId.nucleusHealthUpgrade, "Nucleus Health", 100, 5, null, "Increases the Nucleus health by 10% per level.");

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
