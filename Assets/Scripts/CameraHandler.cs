using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    [SerializeField] Camera m_cameraRef;
    [SerializeField] PlayerHandler m_playerRef;
    [SerializeField] float m_defaultZoom;
    [SerializeField] float m_startingZoom;
    [SerializeField] float m_maxSpeedZoom;
    [SerializeField] float m_zoomTime;
    Vector3 m_offset;
    vTimer m_zoomTimer;
    const float m_zoomChangeSpeed = 1f;
    float m_cameraZoomInertia = 0f;

    bool m_initialZooming = true;
    // Start is called before the first frame update
    void Start()
    {
        m_zoomTimer = new vTimer(m_zoomTime, true, true, false, false, true);
    }

    void UpdateOffset()
    {
        Vector3 desiredPosition = Vector3.zero;
        desiredPosition += m_playerRef.GetComponent<Rigidbody2D>().velocity.ToVector3() * 0.1f;
        m_offset = Vector3.Lerp(m_offset, desiredPosition, Time.deltaTime);
        transform.position = m_playerRef.transform.position + new Vector3(0f, 0f, -10f);
        transform.position += m_offset;
    }

    void UpdateZoom()
    {
        if (m_initialZooming)
        {
            m_cameraRef.orthographicSize = VLib.Eerp(m_startingZoom, m_defaultZoom, m_zoomTimer.GetCompletionPercentage(), 4f);
            if (m_zoomTimer.Update())
            {
                m_initialZooming = false;
            }
        }
        else
        {
            float desiredZoom = Mathf.Lerp( m_defaultZoom, m_maxSpeedZoom , m_playerRef.GetSpeed() / 20f);
            m_cameraRef.orthographicSize = Mathf.Lerp(m_cameraRef.orthographicSize, desiredZoom, Time.deltaTime); ;
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateOffset();
        UpdateZoom();
    }
}
