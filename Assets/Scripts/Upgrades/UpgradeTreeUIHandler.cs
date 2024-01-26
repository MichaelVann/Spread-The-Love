using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeTreeUIHandler : MonoBehaviour
{
    [SerializeField] private ScrollRect m_scrollRectRef;
    [SerializeField] private RectTransform m_contentTransform;
    [SerializeField] private GameObject m_upgradeNodePrefab;
    [SerializeField] private GameObject m_linePrefab;
    [SerializeField] private GameObject m_lineContainer;
    [SerializeField] private UpgradeNodeReadout m_upgradeNodePanelRef;
    [SerializeField] private RectTransform m_viewportTransform;

    UpgradeTree m_upgradeTreeRef;
    List<UpgradeUINode> m_upgradeNodes;

    //Nodes
    UpgradeUINode m_selectedUpgradeNode = null;
    const float m_firstNodeRowPadding = -145f;
    const float m_nodeVerticalPadding = 145f;

    //Zoom
    float m_zoom = 1f;
    float m_minZoom = 0.1f;
    float m_maxZoom = 10f;
    bool m_wasPinchingLastFrame = false;
    float m_lastPinchDistance = 0f;

    bool m_inited = false;

    // Start is called before the first frame update
    void Start()
    {
        m_upgradeTreeRef = GameHandler._upgradeTree;
        m_upgradeNodes = new List<UpgradeUINode>();

        PositionUpgrades();
        SetUpgradeNodePanelStatus(false);
        m_inited = true;
        Refresh();
    }

    private void OnEnable()
    {
        if (m_inited)
        {
            Refresh();
        }
    }

    void SelectNode()
    {

    }

    public void SetUpgradeNodePanelStatus(bool a_value)
    {
        m_upgradeNodePanelRef.gameObject.SetActive(a_value);
    }

    void OpenUpgradeNodePanel(UpgradeUINode a_node)
    {
        m_upgradeNodePanelRef.SetUp(a_node.m_upgradeItemRef, this);
        m_upgradeNodePanelRef.gameObject.SetActive(true);
    }

    public void CloseUpgradeNodePanel()
    {
        m_upgradeNodePanelRef.gameObject.SetActive(false);
    }

    public void AttemptToPurchaseUpgrade(UpgradeItem a_upgradeItemRef)
    {
        m_upgradeTreeRef.AttemptToBuyUpgrade(a_upgradeItemRef);
        m_upgradeNodes[(int)a_upgradeItemRef.m_ID].RunUpgradeEffect();
        Refresh();
    }

    void SpawnNode(UpgradeItem a_upgrade, Vector3 a_parentPos, int a_index, float a_parentWidth, float a_width, bool a_drawingConnection)
    {
        UpgradeUINode node = Instantiate(m_upgradeNodePrefab, m_contentTransform).GetComponent<UpgradeUINode>();
        m_upgradeNodes.Add(node);
        node.SetUp(a_upgrade, this);
        node.SetNameText(a_upgrade.m_name);
        node.SetAvailableSpace(a_width);
        Vector3 pos = new Vector3((a_index + 0.5f) * a_width, m_nodeVerticalPadding, 0);
        pos += a_parentPos;
        //Account for 0,0 being in the middle
        pos -= new Vector3(a_parentWidth / 2f, 0f, 0f);
        //node.GetComponent<RectTransform>().sizeDelta = new Vector2(a_width, 250f);

        node.transform.localPosition = pos;
        for (int i = 0; i < a_upgrade.m_upgradeChildren.Count; i++)
        {
            SpawnNode(a_upgrade.m_upgradeChildren[i], pos, i, a_width, a_width/ (a_upgrade.m_upgradeChildren.Count), true);
        }

        if (a_drawingConnection)
        {
            UILine[] lines = new UILine[3];
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = Instantiate(m_linePrefab, m_lineContainer.transform).GetComponent<UILine>();
                lines[i].gameObject.name = "Line " + i;
            }

            float yGap = (pos.y - a_parentPos.y);
            float lineWidth = 10f;
            Vector3 offsetVector = new Vector3(0f, yGap / 2f);

            Vector3 positionA = a_parentPos;
            Vector3 positionB = a_parentPos + offsetVector;
            Vector3 positionC = pos - offsetVector;
            Vector3 positionD = pos;

            lines[0].SetUp(positionA, positionB, lineWidth);
            lines[1].SetUp(positionB, positionC, lineWidth);
            lines[2].SetUp(positionC, positionD, lineWidth);

            //lines[0].localPosition = a_parentPos;
            //lines[0].sizeDelta = new Vector2(10f,yGap / 2f);
            //lines[1].localPosition = a_parentPos + new Vector3(0f,yGap/2f);
            //lines[1].eulerAngles = new Vector3(0f,0f,90);
        }
    }

    void SpawnChildNodes(UpgradeItem a_parentUpgrade, Vector3 a_parentPos, float a_parentWidth)
    {
        float itemWidth = a_parentWidth / (a_parentUpgrade.m_upgradeChildren.Count + 1);

        for (int i = 0; i < a_parentUpgrade.m_upgradeChildren.Count; i++)
        {
            UpgradeItem upgrade = a_parentUpgrade.m_upgradeChildren[i];
            UpgradeUINode node = Instantiate(m_upgradeNodePrefab, m_contentTransform).GetComponent<UpgradeUINode>();
            node.SetNameText(upgrade.m_name);
            node.SetAvailableSpace(itemWidth);
            Vector3 offset = new Vector3((i + 1) * itemWidth, 250f, 0);
            offset -= new Vector3(a_parentWidth / 2f, 0f, 0f);
            Vector3 spawnPos = a_parentPos + offset;
            node.transform.localPosition = spawnPos;
            SpawnChildNodes(upgrade, spawnPos, itemWidth);
        }
    }

    void PositionUpgrades()
    {
        float totalWidth = m_contentTransform.GetComponent<RectTransform>().rect.width;
        List<UpgradeItem> initialUpgrades = m_upgradeTreeRef.GetInitialUpgradeItems();
        float itemWidth = totalWidth / (initialUpgrades.Count);

        for (int i = 0; i < initialUpgrades.Count; i++)
        {
            float yPos = m_firstNodeRowPadding + m_nodeVerticalPadding;
            SpawnNode(initialUpgrades[i], new Vector3(0f, yPos, 0f), i, totalWidth, itemWidth, true);
        }
    }

    public void Refresh()
    {
        for (int i = 0; i < m_upgradeNodes.Count; i++)
        {
            m_upgradeNodes[i].Refresh();
        }
        RefreshNodesSelectedStatus();
    }

    // Update is called once per frame
    void Update()
    {
        RescaleContentContainer();
    }

    void RescaleContentContainer()
    {
        if (m_contentTransform == null || m_contentTransform.childCount == 0)
        {
            return;
        }

        float yPadding = 200f;
        float xPadding = 0f;// 200f;

        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        //Bounds bounds = new Bounds(m_contentTransform.GetChild(0).localPosition, Vector3.zero);
        foreach (Transform child in m_contentTransform)
        {
            bounds.Encapsulate(child.localPosition);
        }
        bounds.Encapsulate(transform.localPosition);

        float xSize = m_viewportTransform.rect.width;// bounds.size.x / 2f + xPadding;
        float ySize = bounds.size.y;
        ySize = Mathf.Max(m_contentTransform.sizeDelta.y, ySize);
        m_contentTransform.sizeDelta = new Vector2(xSize, ySize);

        //HandlePinchZoom();
    }

    void RefreshNodesSelectedStatus()
    {
        for (int i = 0; i < m_upgradeNodes.Count; i++)
        {
            m_upgradeNodes[i].SetSelectedStatus(m_selectedUpgradeNode == m_upgradeNodes[i]);
        }
    }

    internal void SelectUpgrade(UpgradeUINode a_node)
    {
        m_selectedUpgradeNode = a_node;
        OpenUpgradeNodePanel(a_node);
        RefreshNodesSelectedStatus();
    }
}
