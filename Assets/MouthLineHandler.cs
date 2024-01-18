using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouthLineHandler : MonoBehaviour
{
    [SerializeField] LineRenderer m_lineRenderer;
    const float m_smileWidth = 0.3f;
    const float m_smileHeight = 0.10f;
    const int m_linePositionCount = 30;
    // Start is called before the first frame update
    void Awake()
    {
        SetupLineRenderer();
    }

    void SetupLineRenderer()
    {
        m_lineRenderer.positionCount = m_linePositionCount;
    }

    internal void Refresh(float a_loveScale)
    {
        Vector3[] linePositions = new Vector3[m_linePositionCount];
        for (int i = 0; i < m_linePositionCount; i++)
        {
            float indexRatio = (i / (float)(m_linePositionCount - 1));
            Vector3 linePos = new Vector3(-m_smileWidth / 2f, 0f, 0f);

            linePos.x += indexRatio * m_smileWidth;

            const float eccentricity = 0.5f;
            const float offset = 10f;

            linePos.y = Mathf.Pow((indexRatio - 0.5f), 2f) * eccentricity - (eccentricity / offset);
            linePos.y *= a_loveScale;
            linePositions[i] = linePos;
        }
        m_lineRenderer.SetPositions(linePositions);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
