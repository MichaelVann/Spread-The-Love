using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    [SerializeField] Camera m_cameraRef;
    [SerializeField] Material m_cameraMaterial;
    [SerializeField] PlayerHandler m_playerRef;
    [SerializeField] BattleHandler m_battleHandlerRef;
    [SerializeField] float m_defaultZoom;
    [SerializeField] float m_startingZoom;
    [SerializeField] float m_maxSpeedZoom;
    [SerializeField] float m_zoomTime;
    Vector3 m_offset;
    vTimer m_zoomTimer;
    const float m_zoomChangeSpeed = 1f;
    float m_cameraZoomInertia = 0f;

    //TargetedZoom
    bool m_targetZoomTransitionActive = false;
    bool m_targetZoomTransitioningIn = true;
    Vector3 m_targetZoomTransitioningInStartPos;
    Vector3 m_targetedZoomPosition;
    float m_targetedZoomMagnification = 1f;
    float m_targetedZoomStartingMagnification = 1f;
    const float m_targetedZoomTransitionTime = 1.7f;
    vTimer m_targetToRegularZoomTransitionTimer;

    bool m_initialZooming = true;
    // Start is called before the first frame update
    void Start()
    {
        m_zoomTimer = new vTimer(m_zoomTime, true, true, false, false, true);
    }

    //private void OnRenderImage(RenderTexture a_source, RenderTexture a_destination)
    //{
    //    Graphics.Blit(a_source, a_destination, m_cameraMaterial);
    //}

    internal void SetTargetedZoom(Vector3 a_position, float a_magnification)
    {
        m_targetZoomTransitioningIn = true;
        m_targetToRegularZoomTransitionTimer = new vTimer(m_targetedZoomTransitionTime, true, true, false);
        m_targetedZoomPosition = a_position;
        m_targetedZoomMagnification = a_magnification;
        m_initialZooming = false;
        m_targetToRegularZoomTransitionTimer.SetUsingUnscaledDeltaTime(true);
        m_targetZoomTransitioningInStartPos = m_cameraRef.transform.position;
        m_targetedZoomStartingMagnification = m_cameraRef.orthographicSize;
    }

    internal void EndTargetedZoom()
    {
        m_targetZoomTransitioningIn = false;
        m_targetToRegularZoomTransitionTimer = new vTimer(m_targetedZoomTransitionTime, true, true, false);
        m_targetedZoomPosition = m_cameraRef.transform.position;
        m_targetedZoomMagnification = m_cameraRef.orthographicSize;
        m_targetToRegularZoomTransitionTimer.SetUsingUnscaledDeltaTime(true);
    }

    void ClampToBounds()
    {
        Vector2 mapSize = m_battleHandlerRef.GetMapHalfSize();
        Vector3 cameraPos = transform.position;

        Vector2 cameraSize = new Vector2(m_cameraRef.orthographicSize * m_cameraRef.aspect, m_cameraRef.orthographicSize);
        Vector2 minPoint = -mapSize + cameraSize;
        Vector2 maxPoint = mapSize - cameraSize;

        cameraPos.x = Mathf.Clamp(cameraPos.x, minPoint.x, maxPoint.x);
        cameraPos.y = Mathf.Clamp(cameraPos.y, minPoint.y, maxPoint.y);
        transform.position = cameraPos;
    }

    void UpdateOffset()
    {
        Vector3 desiredPosition = Vector3.zero;
        if (m_targetZoomTransitioningIn)
        {
            desiredPosition = m_targetedZoomPosition;
        }
        //else if (m_targetToRegularZoomTransitionTimer != null)
        //{

        //    float desiredZoom = Mathf.Lerp(m_defaultZoom, m_maxSpeedZoom, m_playerRef.GetSpeed() / 20f);
        //    m_cameraRef.orthographicSize = Mathf.Lerp(m_cameraRef.orthographicSize, desiredZoom, Time.unscaledDeltaTime);
        //}
        else
        {
            desiredPosition += m_playerRef.GetComponent<Rigidbody2D>().velocity.ToVector3() * 0.1f;
        }

        float lerp = Time.unscaledDeltaTime;
        Vector3 startingLerpPos = m_offset;
        if (m_targetToRegularZoomTransitionTimer != null)
        {
            lerp = m_targetToRegularZoomTransitionTimer.GetCompletionPercentage();
            Debug.Log(lerp);
            startingLerpPos = m_targetZoomTransitioningIn ? m_targetZoomTransitioningInStartPos : m_targetedZoomPosition;
        }

        m_offset = Vector3.Lerp(startingLerpPos, desiredPosition, lerp);
        transform.position = m_playerRef.transform.position + new Vector3(0f, 0f, -10f);
        transform.position += m_offset;
        //ClampToBounds();
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
        //else if (m_targetToRegularZoomTransitionTimer != null)
        //{
        //    float desiredZoom = Mathf.Lerp(m_defaultZoom, m_maxSpeedZoom, m_playerRef.GetSpeed() / 20f);
        //    m_cameraRef.orthographicSize = Mathf.Lerp(m_cameraRef.orthographicSize, desiredZoom, Time.unscaledDeltaTime);
        //}
        else
        {
            float lerp = Time.unscaledDeltaTime;
            float startingLerpZoom = m_cameraRef.orthographicSize;
            float desiredZoom = Mathf.Lerp( m_defaultZoom, m_maxSpeedZoom , m_playerRef.GetSpeed() / 20f);
            if (m_targetToRegularZoomTransitionTimer != null)
            {
                lerp = m_targetToRegularZoomTransitionTimer.GetCompletionPercentage();
                startingLerpZoom = m_targetZoomTransitioningIn ? m_targetedZoomStartingMagnification : m_targetedZoomMagnification;
                desiredZoom = m_targetZoomTransitioningIn ? m_targetedZoomMagnification : Mathf.Lerp(m_defaultZoom, m_maxSpeedZoom, m_playerRef.GetSpeed() / 20f);
            }
            m_cameraRef.orthographicSize = Mathf.Lerp(startingLerpZoom, desiredZoom, lerp);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_targetToRegularZoomTransitionTimer != null && m_targetToRegularZoomTransitionTimer.Update())
        {
            if (m_targetZoomTransitioningIn == false)
            {
                m_targetToRegularZoomTransitionTimer = null;
            }
        }
        UpdateOffset();
        UpdateZoom();
    }
}
