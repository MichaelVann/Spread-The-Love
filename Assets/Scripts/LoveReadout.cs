using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoveReadout : MonoBehaviour
{
    [SerializeField] RollingText m_rollingTextRef;
    [SerializeField] RollingText m_deltaRollingTextRef;

    // Start is called before the first frame update
    void Start()
    {
        m_rollingTextRef.SetCurrentValue(GameHandler._lastSeenScore);
        m_rollingTextRef.SetDesiredValue(GameHandler._score);
        m_rollingTextRef.SetRollTime(1f);
        m_deltaRollingTextRef.SetCurrentValue(GameHandler._score - GameHandler._lastSeenScore);
        m_deltaRollingTextRef.SetDesiredValue(0f);
        m_deltaRollingTextRef.SetRollTime(1f);
        m_deltaRollingTextRef.SetPlusMinusSign(true);
        m_deltaRollingTextRef.SetBrackets(true);
        m_deltaRollingTextRef.SetDestroyGameObjectOnFinish(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RefreshRollingText()
    {
        m_rollingTextRef.Refresh(GameHandler._lastSeenScore, GameHandler._score);
    }
}
