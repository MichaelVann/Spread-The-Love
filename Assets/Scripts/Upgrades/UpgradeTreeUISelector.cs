using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeTreeUISelector : MonoBehaviour
{
    [SerializeField] GameObject m_selectionContainer;
    [SerializeField] GameObject m_selectionPrefab;
    [SerializeField] float m_exponent = 0.75f;
    [SerializeField] float m_selectorSize = 2f;
    List<GameObject> m_selections;
    int m_selectedIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        UpgradeTree upgradeTreeRef = GameHandler._upgradeTree;
        List<UpgradeItem> initialUpgrades = upgradeTreeRef.GetInitialUpgradeItems();
        m_selections = new List<GameObject>();
        for (int i = 0; i < 10 /*initialUpgrades.Count*/; i++)
        {
            m_selections.Add(Instantiate(m_selectionPrefab, m_selectionContainer.transform));
            m_selections[i].gameObject.name = i.ToString();
        }
        PositionSelectors();
    }

    internal void SetSelected(int a_selected)
    {
        m_selectedIndex = a_selected;
        PositionSelectors();
    }

    float GetSelectorSize(float deltaIndex)
    {
        float size = m_selectorSize;
        float exponent = Mathf.Pow(m_exponent, Mathf.Abs(deltaIndex));
        size *= exponent;
        //size *= segmentHeight * totalHeight;
        return size;
    }

    float GetSelectorPosition(int a_deltaIndex)
    {
        float pos = 0f;
        int absDeltaIndex = Mathf.Abs(a_deltaIndex);
        for (int i = 0; i < absDeltaIndex; i++)
        {
            pos += GetSelectorSize(i);
        }
        pos *= a_deltaIndex > 0 ? -1f : 1f;
        Debug.Log(pos);
        return pos;
    }

    void PositionSelectors()
    {
        float totalHeight = GetComponent<RectTransform>().rect.height;
        float segmentHeight = 1f / (m_selections.Count);
        for (int i = 0; i < m_selections.Count; i++)
        {
            int deltaIndex = m_selectedIndex - i;

            Vector3 pos = new Vector3(0f, segmentHeight * (i), 0f);
            pos.y = GetSelectorPosition(deltaIndex);
            pos *= segmentHeight * totalHeight;
            m_selections[i].transform.localPosition = pos;

            //Size
            float size = GetSelectorSize(deltaIndex);
            size *= segmentHeight * totalHeight;
            RectTransform rectTransform = m_selections[i].GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(size, size);

            //Vector3 pos = new Vector3(0f, segmentHeight * (i), 0f);
            //pos.y -= (float)m_selectedIndex / m_selections.Count;

            //float exponent = Mathf.Pow(0.5f,1f + Mathf.Abs(pos.y * exponentMultiplier));
            ////Size
            //RectTransform rectTransform = m_selections[i].GetComponent<RectTransform>();
            //float size = 2f;
            //size *= exponent;// Mathf.Pow(size, 1f + Mathf.Abs(pos.y * 10f));
            //size *= segmentHeight * totalHeight;
            //rectTransform.sizeDelta = new Vector2(size, size);

            ////Apply scaling
            //pos.y *= exponent;
            //pos *= totalHeight;
            //m_selections[i].transform.localPosition = pos;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
