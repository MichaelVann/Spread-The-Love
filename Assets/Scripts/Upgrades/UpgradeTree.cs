using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UpgradeItem;

public class UpgradeTree
{
    internal List<UpgradeItem> m_upgradeItemList;
    const int m_baseUpgradePrice = 15;
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

    UpgradeItem NewUpgrade(UpgradeId a_ID, string a_name, float a_cost, int a_maxLevel, float a_strength, UpgradeItem a_precursorUpgrade, string a_description, bool a_toggleable = false, float a_costScaling = 1.2f)
    {
        UpgradeItem upgrade = new UpgradeItem(a_ID,a_name, (int)(a_cost * m_baseUpgradePrice), a_maxLevel, a_strength, a_precursorUpgrade, a_description, a_toggleable, a_costScaling);
        m_upgradeItemList.Add(upgrade);
        return upgrade;
    }

    void SetupUpgrades()
    {
        UpgradeItem acceleration = NewUpgrade(UpgradeId.Acceleration, "Acceleration", 1.5f, 10, 0.25f, null, "Increases acceleration by 25% each rank.");
        acceleration.SetStartingStrengthAndIfMultiplicative(1f, true);
        UpgradeItem mass = NewUpgrade(UpgradeId.Mass, "Density", 1.5f, 10, 0.25f, acceleration, "Increases desnity by 25% for each rank. This helps maintain speed when colliding with the souls in the world.");
        mass.SetStartingStrengthAndIfMultiplicative(25f, true);
        UpgradeItem braking = NewUpgrade(UpgradeId.Braking, "Braking", 2f, 10, 0.1f, mass, "Unlocks the ability to brake and increases braking strength by 10% each rank.");
        braking.SetStartingStrengthAndIfMultiplicative(1f, true);
        UpgradeItem grip = NewUpgrade(UpgradeId.Grip, "Grip", 1.5f, 12, 0.1f, acceleration, "Increases grip each rank.");
        grip.SetStartingStrengthAndIfMultiplicative(1f, true);
        UpgradeItem topSpeed = NewUpgrade(UpgradeId.TopSpeed, "Top Speed", 1.5f, 10, 1f, acceleration, "Increases top speed by 1 m/s each rank.");
        topSpeed.SetStartingStrengthAndIfMultiplicative(10f, false);
        UpgradeItem turnSpeed = NewUpgrade(UpgradeId.TurnSpeed, "Turn Speed", 1f, 10, 0.03f, acceleration, "Increases turn speed by 5% each rank.");
        turnSpeed.SetStartingStrengthAndIfMultiplicative(70f, true);
        UpgradeItem aquaplane = NewUpgrade(UpgradeId.Aquaplane, "Aquaplane", 3f, 1, 1f, grip, "Allows you to aquaplane, removing all friction against the ground.");
        //UpgradeItem driftSpread = NewUpgrade(UpgradeId.DriftSpread, "Drift Spread", 20f, 1, 0.25f, aquaplane, "Spreads you out when drifting. This is a gimmick ability I wouldn't recommend actually buying this.", true);
        UpgradeItem shooting = NewUpgrade(UpgradeId.Shooting, "Spread Kindness", 2f, 1, 0.25f, null, "Unlocks the ability to spread good vibes remotely.");
        shooting.SetStartingStrengthAndIfMultiplicative(1f, true);

        UpgradeItem projectileSpeed = NewUpgrade(UpgradeId.ProjectileSpeed, "Projectile Speed", 1.5f, 10, 0.25f, shooting, "Increases projectile speed.", false);
        projectileSpeed.SetStartingStrengthAndIfMultiplicative(5f, true);
        UpgradeItem shootSpread = NewUpgrade(UpgradeId.ShootSpread, "Blast Spread", 5f, 10, 1f, projectileSpeed, "Increases amount of love sent with each spread of kindness by 1.", false, 1.5f);
        UpgradeItem berserkShot = NewUpgrade(UpgradeId.BerserkShot, "Berserk Shot", 6f, 10, 1, shootSpread, "Gives the ability to send a vibe that sends a loved vessel into a love beserk. Each Rank increases beserk time by 1 second starting at 5 seconds.");
        berserkShot.SetStartingStrengthAndIfMultiplicative(5f, false);
        UpgradeItem fireRate = NewUpgrade(UpgradeId.FireRate, "Fire Rate", 1.5f, 10, 0.25f, shooting, "Increases fire rate by 25% each rank.", false, 1.5f);
        fireRate.SetStartingStrengthAndIfMultiplicative(1f, true);
        UpgradeItem autoShoot = NewUpgrade(UpgradeId.AutoShoot, "Auto Shoot", 3.5f, 1, 1f, fireRate, "Now love blast repeats automatically.", true);
        UpgradeItem mouseAim = NewUpgrade(UpgradeId.MouseAim, "Cursor Aim", 15f, 1, 1f, autoShoot, "Love blast heads towards the cursor.", true);

        //UpgradeItem vesselRadar = NewUpgrade(UpgradeItem.UpgradeId.Radar, "Radar", 30, 1, 1f, null, "Points towards the nearest lost soul.");
        //UpgradeItem minimap = NewUpgrade(UpgradeItem.UpgradeId.Minimap, "Minimap", 60, 1, 1f, vesselRadar, "Gives you an overview of the world.");
        UpgradeItem additionalTime = NewUpgrade(UpgradeId.AdditionalTime, "Time Extension", 3f, 10, 10f, null, "Gives you an additional 10 seconds of time before rebirth per rank.");
        UpgradeItem mindfulness = NewUpgrade(UpgradeId.Mindfulness, "Mindfulness", 3f, 10, 0.08f, additionalTime, "Slows time, allowing increased reaction time. Each rank further increases the amount time is slowed.");
        mindfulness.SetStartingStrengthAndIfMultiplicative(2f, true);

        UpgradeItem snowPlough = NewUpgrade(UpgradeId.SnowPlough, "Snow Plough", 3f, 1, 1f, null, "Gives the ability to spawn a directional forcefield in the direction of travel.");
        UpgradeItem snowPloughSize = NewUpgrade(UpgradeId.SnowPloughSize, "Size", 3f, 10, 0.1f, snowPlough, "Increases the size of the Snow Plough.");
        snowPloughSize.SetStartingStrengthAndIfMultiplicative(1f, true);
        UpgradeItem snowPloughCooldown = NewUpgrade(UpgradeId.SnowPloughCooldown, "Cooldown", 3f, 10, 0.05f, snowPlough, "Reduces the cooldown of the Snow Plough.");
        snowPloughCooldown.SetStartingStrengthAndIfMultiplicative(6f, true);
        snowPloughCooldown.SetReductive(true);
        UpgradeItem snowPloughDuration = NewUpgrade(UpgradeId.SnowPloughDuration, "Duration", 3f, 10, 0.05f, snowPlough, "Increases the duration of the Snow Plough.");
        snowPloughDuration.SetStartingStrengthAndIfMultiplicative(1.5f, true);

        //Keys
        braking.SetKey("S");
        shooting.SetKey("LMB");
        aquaplane.SetKey("Space");
        mindfulness.SetKey("L Shift");
        berserkShot.SetKey("Q");
        snowPlough.SetKey("W");
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
            a_upgrade.m_cost = a_upgrade.GetCostAtLevel(a_upgrade.m_level + 1);
            if (!a_upgrade.m_owned)
            {
                a_upgrade.SetOwned(true);
            }
            RefreshUpgrades();
        }
    }
}
