using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflectiveSheen : MonoBehaviour
{
    [SerializeField] GameObject m_topLeftTransform;
    [SerializeField] GameObject m_bottomRightTransform;
    [SerializeField] GameObject m_sheenRef;
    vTimer m_timer;
    [SerializeField] float m_pauseTime = 2.6f;
    [SerializeField] float m_runTime = 0.4f;
    [SerializeField] bool m_stayIndependentOfParentRotation = true;
    Quaternion m_originalRotation;
    // Start is called before the first frame update
    void Awake()
    {
        float totalRunTime = m_pauseTime + m_runTime;
        m_timer = new vTimer(totalRunTime);
        m_timer.SetUsingUnscaledDeltaTime(true);
        m_timer.SetTimer(VLib.vRandom(0f, totalRunTime));
        m_originalRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        m_timer.Update();
        float timer = m_timer.GetTimer();
        if (timer >= m_pauseTime)
        {
            float lerp = (timer - m_pauseTime) / m_runTime;
            m_sheenRef.transform.position = Vector3.Lerp(m_topLeftTransform.transform.position, m_bottomRightTransform.transform.position, Mathf.Pow(lerp,2f));
            m_sheenRef.transform.eulerAngles = new Vector3(0f,0f,90f + VLib.Vector2ToEulerAngle(m_bottomRightTransform.transform.position - m_topLeftTransform.transform.position));
        }
        else
        {
            m_sheenRef.transform.localPosition = 2f * m_topLeftTransform.transform.localPosition;
        }

        if (m_stayIndependentOfParentRotation)
        {
            transform.rotation = m_originalRotation;
            //m_topLeftTransform.transform.localPosition = m_topLeftPosition;
            //m_bottomRightTransform.transform.localPosition = m_bottomRightPosition;
        }
    }
}
