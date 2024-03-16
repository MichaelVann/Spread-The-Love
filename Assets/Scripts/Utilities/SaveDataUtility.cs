using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using static Unity.Collections.AllocatorManager;

public class SaveDataUtility
{
    internal const string m_saveFileName = "DataToo.txt";
    const int m_saveVersion = 1;

    [Serializable]
    struct UpgradeTreeData
    {
        [Serializable]
        public struct UpgradeData
        {
            public int cost;
            public bool owned;
            public int level;

            public bool unlocked;
            public bool toggled;
        }
        public List<UpgradeData> upgrades;
    }

    [Serializable]
    struct AudioData
    {
        [Serializable]
        public struct AudioChannelData
        {
            public bool enabled;
            public float volume;
        }
        public List<AudioChannelData> channels;
    }

    [Serializable]
    struct SaveData
    {
        public int version;
        public UpgradeTreeData upgradeTreeData;
        public AudioData audioData;

        public int score;
        public int lastSeenScore;
        public int mapSize;
        public bool firstTimeCutscenePlayed;
        public int highestMapSizeSeen;
    }
    [SerializeField]
    SaveData m_saveData;


    string GetSaveDataPath()
    {
        return Application.persistentDataPath + "/" + m_saveFileName;
    }

    internal SaveDataUtility()
    {
        m_saveData = new SaveData();
    }

    void SaveUpgradeTree()
    {
        m_saveData.upgradeTreeData.upgrades = new List<UpgradeTreeData.UpgradeData>();
        for (int i = 0; i < GameHandler._upgradeTree.m_upgradeItemList.Count; i++)
        {
            UpgradeTreeData.UpgradeData upgradeData = new UpgradeTreeData.UpgradeData();
            UpgradeItem upgradeItem = GameHandler._upgradeTree.m_upgradeItemList[i];
            upgradeData.cost = upgradeItem.m_cost;
            upgradeData.level = upgradeItem.m_level;
            upgradeData.unlocked = upgradeItem.m_unlocked;
            upgradeData.owned = upgradeItem.m_owned;
            upgradeData.toggled = upgradeItem.m_toggled;
            m_saveData.upgradeTreeData.upgrades.Add(upgradeData);
        }
    }

    void LoadUpgradeTree()
    {
        for (int i = 0; i < m_saveData.upgradeTreeData.upgrades.Count; i++)
        {
            UpgradeTreeData.UpgradeData upgradeData = m_saveData.upgradeTreeData.upgrades[i];
            UpgradeItem upgradeItem = GameHandler._upgradeTree.m_upgradeItemList[i];
            upgradeItem.m_cost = upgradeData.cost;
            upgradeItem.m_level = upgradeData.level;
            upgradeItem.m_unlocked = upgradeData.unlocked;
            upgradeItem.m_owned = upgradeData.owned;
        }
    }

    void SaveAudioData()
    {
        m_saveData.audioData.channels = new List<AudioData.AudioChannelData>();
        for (int i = 0; i < (int)AudioManager.eSoundChannel.Count; i++)
        {
            AudioData.AudioChannelData channelData = new AudioData.AudioChannelData();
            channelData.enabled = GameHandler._audioManager.GetChannelEnabled((AudioManager.eSoundChannel)i);
            channelData.volume = GameHandler._audioManager.GetChannelVolume((AudioManager.eSoundChannel)i);
            m_saveData.audioData.channels.Add(channelData);
        }
    }

    void LoadAudioData()
    {
        for (int i = 0; i < (int)AudioManager.eSoundChannel.Count; i++)
        {
            GameHandler._audioManager.SetChannelEnabled((AudioManager.eSoundChannel)i, m_saveData.audioData.channels[i].enabled);
            GameHandler._audioManager.SetChannelVolume((AudioManager.eSoundChannel)i, m_saveData.audioData.channels[i].volume);
        }
    }

    internal void Save()
    {
        SaveUpgradeTree();
        SaveAudioData();

        m_saveData.version = m_saveVersion;
        m_saveData.score = GameHandler._score;
        m_saveData.lastSeenScore = GameHandler._lastSeenScore;
        m_saveData.firstTimeCutscenePlayed = GameHandler._firstTimeCutscenePlayed;
        m_saveData.highestMapSizeSeen = GameHandler._highestMapSizeSeen;
        m_saveData.mapSize = GameHandler._mapSize;

        string path = GetSaveDataPath();
        string json = JsonUtility.ToJson(m_saveData);
        File.WriteAllText(path, json);
    }

    bool SaveFileVersionCheck()
    {
        bool result = m_saveData.version >= m_saveVersion;
        //result &= m_saveData.subVersion >= GameHandler.SUB_VERSION_NUMBER;

        return result;
    }

    void Load()
    {
        LoadUpgradeTree();
        LoadAudioData();
        GameHandler._score = m_saveData.score;
        GameHandler._lastSeenScore = m_saveData.lastSeenScore;
        GameHandler._firstTimeCutscenePlayed = m_saveData.firstTimeCutscenePlayed;
        GameHandler._highestMapSizeSeen = m_saveData.highestMapSizeSeen;
        GameHandler._mapSize = m_saveData.mapSize;
    }

    internal bool TryLoad()
    {
        bool retVal = false;
        string path = GetSaveDataPath();
        if (File.Exists(path))
        {
            string loadedString = File.ReadAllText(path);
            m_saveData = JsonUtility.FromJson<SaveData>(loadedString);
            if (SaveFileVersionCheck())
            {
                Load();
                retVal = true;
            }
        }
        else
        {
            //Throw up window saying no save found
        }
        return retVal;
    }
}
