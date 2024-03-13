using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvailableUpgradeNotifier : MonoBehaviour
{
    Vector3 m_originalLocalPosition;
    float m_bobSpeed = 7f;
    float m_bobHeight = 1.3f;
    // Start is called before the first frame update
    void Start()
    {
        m_originalLocalPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = m_originalLocalPosition + Vector3.up * Mathf.Sin(Time.time * m_bobSpeed) * m_bobHeight;
    }
}
