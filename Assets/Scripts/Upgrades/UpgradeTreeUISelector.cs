using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeTreeUISelector : MonoBehaviour
{
    [SerializeField] GameObject m_selectionContainer;
    [SerializeField] GameObject m_selectionPrefab;
    [SerializeField] float m_exponent = 0.75f;
    [SerializeField] float m_selectorSize = 1f;
    [SerializeField] float m_selectorGap = 0f;
    [SerializeField] AudioClip m_selectorChangeAudio;
    List<GameObject> m_selections;
    int m_selectedIndex = 0;
    float m_selectorPosition = 0;
    [SerializeField] float m_selectorSpeed = 7f;
    // Start is called before the first frame update
    void Awake()
    {
        UpgradeTree upgradeTreeRef = GameHandler._upgradeTree;
        List<UpgradeItem> initialUpgrades = upgradeTreeRef.GetInitialUpgradeItems();
        m_selections = new List<GameObject>();
        UpgradeTreeUIHandler upgradeTreeUIHandler =  FindObjectOfType<UpgradeTreeUIHandler>();
        for (int i = 0; i < initialUpgrades.Count; i++)
        {
            m_selections.Add(Instantiate(m_selectionPrefab, m_selectionContainer.transform));
            m_selections[i].GetComponent<UpgradeTreeSelection>().Init(upgradeTreeUIHandler.SetTreeSelection, i, GameHandler.GetUpgradeSprite(initialUpgrades[i].m_ID), initialUpgrades[i]);
            m_selections[i].gameObject.name = i.ToString();
        }
    }

    internal void SetSelected(int a_selected)
    {
        if (m_selectedIndex != a_selected)
        {
            GameHandler._audioManager.PlaySFX(m_selectorChangeAudio);
        }
        m_selectedIndex = a_selected;
    }

    float GetSelectorSize(float a_deltaIndex)
    {
        float size = m_selectorSize;
        float exponent = Mathf.Pow(m_exponent, Mathf.Abs(a_deltaIndex));
        size *= exponent;
        return size;
    }

    float GetSelectorPosition(float a_deltaIndex)
    {
        float pos;

        float a = m_selectorSize;
        float b = m_exponent;
        float bToTheX = Mathf.Pow(m_exponent, Mathf.Abs(a_deltaIndex));

        float integral = (a * bToTheX / Mathf.Log(b)) - (a / Mathf.Log(b));
        pos = integral;


        pos *= a_deltaIndex > 0 ? -1f : 1f;

        pos -= a_deltaIndex * m_selectorGap;
        return pos;
    }

    void PositionSelectors()
    {
        float totalHeight = GetComponent<RectTransform>().rect.height;
        float segmentHeight = 1f / (m_selections.Count);
        for (int i = 0; i < m_selections.Count; i++)
        {
            float deltaIndex = m_selectorPosition - i;

            Vector3 pos = new Vector3(0f, segmentHeight * (i), 0f);
            pos.y = GetSelectorPosition(deltaIndex);
            pos *= segmentHeight * totalHeight;
            m_selections[i].transform.localPosition = pos;

            //Size
            float size = GetSelectorSize(deltaIndex);
            size *= segmentHeight * totalHeight;
            RectTransform rectTransform = m_selections[i].GetComponent<RectTransform>();
            rectTransform.localScale = new Vector2(size, size) / rectTransform.sizeDelta;
        }
    }

    internal void RefreshUpgradeAvailabilities()
    {
        int cash = GameHandler._score;
        for (int i = 0; i < m_selections.Count; i++)
        {
            m_selections[i].GetComponent<UpgradeTreeSelection>().Refresh(cash);
        }
    }

    // Update is called once per frame
    void Update()
    {
        m_selectorPosition = Mathf.Lerp(m_selectorPosition, m_selectedIndex, Time.deltaTime * m_selectorSpeed);
        PositionSelectors();
    }
}
