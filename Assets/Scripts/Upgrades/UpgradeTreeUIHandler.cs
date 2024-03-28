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
    [SerializeField] private UpgradeTreeUISelector m_upgradeTreeUISelectorRef;

    UpgradeTree m_upgradeTreeRef;
    List<UpgradeItem> m_initialUpgrades;
    int m_viewedInitialUpgradeIndex = 0;
    //UpgradeItem m_viewedIntitalUpgrade;
    List<UpgradeUINode> m_upgradeNodes;

    //Nodes
    UpgradeUINode m_selectedUpgradeNode = null;
    const float m_firstNodeRowPadding = -0;
    const float m_nodeRowPadding = 145f;

    //Lines
    List<GameObject> m_lines;

    //Zoom
    float m_zoom = 1f;
    float m_minZoom = 0.1f;
    float m_maxZoom = 10f;
    bool m_wasPinchingLastFrame = false;
    float m_lastPinchDistance = 0f;

    bool m_inited = false;

    bool m_dPadUpWasDown = false;
    bool m_dPadDownWasDown = false;

    // Start is called before the first frame update
    void Start()
    {
        m_upgradeTreeRef = GameHandler._upgradeTree;
        m_upgradeNodes = new List<UpgradeUINode>();
        m_lines = new List<GameObject>();

        PositionUpgrades();
        SetUpgradeNodePanelStatus(false);
        m_inited = true;
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
        m_selectedUpgradeNode = null;
        RefreshNodesSelectedStatus();
    }

    public void AttemptToPurchaseUpgrade(UpgradeItem a_upgradeItemRef)
    {
        m_upgradeTreeRef.AttemptToBuyUpgrade(a_upgradeItemRef);
        m_selectedUpgradeNode.RunUpgradeEffect();
        Refresh();
    }

    public void AttemptToRefundUpgrade(UpgradeItem a_upgradeItemRef)
    {
        m_upgradeTreeRef.AttemptToRefundUpgrade(a_upgradeItemRef);
        m_selectedUpgradeNode.RunUpgradeEffect();
        Refresh();
    }

    void SpawnNode(UpgradeItem a_upgrade, Vector3 a_parentPos, int a_index, float a_parentHeight, float a_height, bool a_drawingConnection)
    {
        UpgradeUINode node = Instantiate(m_upgradeNodePrefab, m_contentTransform).GetComponent<UpgradeUINode>();

        if (a_upgrade.m_key != string.Empty)
        {
            node.SetKeyIndicator(a_upgrade.m_key);
        }


        m_upgradeNodes.Add(node);
        node.SetUp(a_upgrade, this);
        node.SetNameText(a_upgrade.m_name);
        node.SetAvailableSpace(a_height);
        Vector3 pos = new Vector3(m_nodeRowPadding, (a_index + 0.5f) * a_height, 0);
        pos += a_parentPos;
        //Account for 0,0 being in the middle
        pos -= new Vector3(0f, a_parentHeight / 2f, 0f);
        //node.GetComponent<RectTransform>().sizeDelta = new Vector2(a_width, 250f);

        node.transform.localPosition = pos;
        for (int i = 0; i < a_upgrade.m_upgradeChildren.Count; i++)
        {
            SpawnNode(a_upgrade.m_upgradeChildren[i], pos, i, a_height, a_height/ (a_upgrade.m_upgradeChildren.Count), true);
        }

        if (a_drawingConnection)
        {
            UILine[] lines = new UILine[3];
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = Instantiate(m_linePrefab, m_lineContainer.transform).GetComponent<UILine>();
                lines[i].gameObject.name = "Line " + i;
                m_lines.Add(lines[i].gameObject);
            }

            float xGap = (pos.x - a_parentPos.x);
            float lineWidth = 10f;
            Vector3 offsetVector = new Vector3(xGap / 2f, 0f);

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

    static internal int UpgradeNodeComparison(UpgradeUINode a_first, UpgradeUINode a_second)
    {
        int returnVal = a_first.m_upgradeItemRef.m_ID - a_second.m_upgradeItemRef.m_ID;

        return returnVal;
    }

    void PositionUpgrades()
    {
        float totalHeight = m_contentTransform.GetComponent<RectTransform>().rect.height;
        m_initialUpgrades = m_upgradeTreeRef.GetInitialUpgradeItems();
        UpgradeItem viewedIntitalUpgrade = m_initialUpgrades[m_viewedInitialUpgradeIndex];

        float xPos = m_firstNodeRowPadding;// + m_nodeRowPadding;
        SpawnNode(viewedIntitalUpgrade, new Vector3(xPos, 0f, 0f), 0, totalHeight, totalHeight, true);

        //for (int i = 0; i < m_initialUpgrades.Count; i++)
        //{
        //    float yPos = m_firstNodeRowPadding + m_nodeVerticalPadding;
        //    SpawnNode(m_initialUpgrades[i], new Vector3(0f, yPos, 0f), i, totalWidth, totalWidth, true);
        //}
        m_upgradeNodes.Sort(UpgradeNodeComparison);
        Refresh();
    }

    public void Refresh()
    {
        for (int i = 0; i < m_upgradeNodes.Count; i++)
        {
            m_upgradeNodes[i].Refresh();
        }
        RefreshNodesSelectedStatus();
        m_upgradeTreeUISelectorRef.RefreshUpgradeAvailabilities();
    }

    void ClearTree()
    {
        for (int i = 0; i < m_lines.Count; i++)
        {
            Destroy(m_lines[i]);
        }
        m_lines.Clear();
        for (int i = 0; i < m_upgradeNodes.Count; i++)
        {
            Destroy(m_upgradeNodes[i].gameObject);
        }
        m_upgradeNodes.Clear();
    }

    void RotateTreeSelection(bool a_up)
    {
        SetTreeSelection(m_viewedInitialUpgradeIndex + (a_up ? 1 : -1));
    }

    internal void SetTreeSelection(int a_id)
    {
        m_viewedInitialUpgradeIndex = a_id;
        m_viewedInitialUpgradeIndex = Mathf.Clamp(m_viewedInitialUpgradeIndex, 0, m_initialUpgrades.Count - 1);
        m_upgradeTreeUISelectorRef.SetSelected(m_viewedInitialUpgradeIndex);
        CloseUpgradeNodePanel();
        ClearTree();
        PositionUpgrades();
    }

    // Update is called once per frame
    void Update()
    {
        //RescaleContentContainer();

        if (Input.GetKeyDown(KeyCode.W) || (Input.GetAxis("D Pad Y") < 1f && m_dPadUpWasDown))
        {
            RotateTreeSelection(true);
        }
        if (Input.GetKeyDown(KeyCode.S) || (Input.GetAxis("D Pad Y") > -1f && m_dPadDownWasDown))
        {
            RotateTreeSelection(false);
        }

        m_dPadUpWasDown = Input.GetAxis("D Pad Y") >= 1f;
        m_dPadDownWasDown = Input.GetAxis("D Pad Y") <= -1f;

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
