using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UpgradeItem;

public class UpgradeTree
{
    internal List<UpgradeItem> m_upgradeItemList;
    const int m_baseUpgradePrice = 10;
    // Start is called before the first frame update

    internal UpgradeItem GetUpgrade(UpgradeId a_upgradeId) { return m_upgradeItemList[(int)a_upgradeId]; }

    internal bool HasUpgrade(UpgradeId a_upgradeId) { return GetUpgrade(a_upgradeId).IsEnabled(); }

    internal int GetUpgradeLevel(UpgradeId a_upgradeId) { return GetUpgrade(a_upgradeId).m_level; }

    internal float GetUpgradeLeveledStrength(UpgradeId a_upgradeId) { return GetUpgrade(a_upgradeId).GetLeveledStrength(); }
    internal float GetUpgradeMaxLeveledStrength(UpgradeId a_upgradeId) { return GetUpgrade(a_upgradeId).GetMaxLeveledStrength(); }

    internal UpgradeTree()
    {
        m_upgradeItemList = new List<UpgradeItem>();
        SetupUpgrades();
    }

    UpgradeItem NewUpgrade(UpgradeItem.UpgradeId a_ID, string a_name, float a_cost, int a_maxLevel, float a_effectStrength, UpgradeItem a_precursorUpgrade, string a_description, bool a_toggleable = false)
    {
        UpgradeItem upgrade = new UpgradeItem(a_ID,a_name, (int)(a_cost * m_baseUpgradePrice), a_maxLevel, a_effectStrength, a_precursorUpgrade, a_description, a_toggleable);
        m_upgradeItemList.Add(upgrade);
        return upgrade;
    }

    void SetupUpgrades()
    {
        /**/UpgradeItem mass = NewUpgrade(UpgradeId.Mass, "Density", 1.5f, 10, 0.25f, null, "Increases desnity by 25% for each level. This helps maintain speed when colliding with the souls in the world.");
        UpgradeItem braking = NewUpgrade(UpgradeId.Braking, "Braking", 2f, 10, 0.1f, mass, "Unlocks the ability to brake and increases braking strength by 10% each level.");
        /**/UpgradeItem acceleration = NewUpgrade(UpgradeId.Acceleration, "Acceleration", 1.5f, 10, 0.25f, null, "Increases acceleration by 25% each level.");
        /**/UpgradeItem topSpeed = NewUpgrade(UpgradeId.TopSpeed, "Top Speed", 1.5f, 10, 1f, acceleration, "Increases top speed by 1 m/s each level.");
        UpgradeItem turnSpeed = NewUpgrade(UpgradeId.TurnSpeed, "Turn Speed", 1f, 10, 0.1f, null, "Increases turn speed by 10% each level.");
        UpgradeItem aquaplane = NewUpgrade(UpgradeId.Aquaplane, "Aquaplane", 3f, 1, 1f, turnSpeed, "Allows you to aquaplane by pressing <color=#ff004c>space</color>, removing all friction against the ground.");
        //UpgradeItem driftSpread = NewUpgrade(UpgradeId.DriftSpread, "Drift Spread", 20f, 1, 0.25f, aquaplane, "Spreads you out when drifting. This is a gimmick ability I wouldn't recommend actually buying this.", true);
        UpgradeItem shooting = NewUpgrade(UpgradeId.Shooting, "Spread Kindness", 2f, 1, 0.25f, null, "Unlocks the ability to spread good vibes remotely.");
        UpgradeItem fireRate = NewUpgrade(UpgradeId.FireRate, "Fire Rate", 1.5f, 10, 0.25f, shooting, "Increases fire rate by 25% each level.");
        UpgradeItem shootSpread = NewUpgrade(UpgradeId.ShootSpread, "Blast Spread", 5f, 10, 1f, fireRate, "Increases amount of love sent with each spread of kindness by 1.");
        UpgradeItem autoShoot = NewUpgrade(UpgradeId.AutoShoot, "Auto Shoot", 3.5f, 1, 1f, fireRate, "Now love blast repeats automatically.");
        UpgradeItem mouseAim = NewUpgrade(UpgradeId.MouseAim, "Cursor Aim", 15f, 1, 1f, autoShoot, "Love blast heads towards the cursor.", true);

        //UpgradeItem vesselRadar = NewUpgrade(UpgradeItem.UpgradeId.Radar, "Radar", 30, 1, 1f, null, "Points towards the nearest lost soul.");
        //UpgradeItem minimap = NewUpgrade(UpgradeItem.UpgradeId.Minimap, "Minimap", 60, 1, 1f, vesselRadar, "Gives you an overview of the world.");
        UpgradeItem additionalTime = NewUpgrade(UpgradeItem.UpgradeId.AdditionalTime, "Time Extension", 5f, 10, 10f, null, "Gives you an additional 10 seconds of time before rebirth per Level.");
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
