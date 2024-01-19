using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILine : MonoBehaviour
{
    [SerializeField] private RectTransform m_rectTransformRef;
    float m_lineWidth;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void SetUp(Vector3 a_start, Vector3 a_end, float a_lineWidth)
    {
        float mag = (a_start - a_end).magnitude;
        Vector2 directionVector = (a_end - a_start).ToVector2();
        float angle = VLib.Vector2ToEulerAngle(directionVector);

        m_rectTransformRef.localPosition = a_start;
        m_rectTransformRef.sizeDelta = new Vector2(a_lineWidth, mag);
        m_rectTransformRef.eulerAngles = new Vector3(0, 0, angle);
    }

    internal void SetUp(Vector3[] a_positions, float a_lineWidth)
    {
        SetUp(a_positions[0], a_positions[1], a_lineWidth);
    }
}
