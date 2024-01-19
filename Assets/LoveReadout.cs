using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoveReadout : MonoBehaviour
{
    [SerializeField] RollingText m_rollingTextRef;
    // Start is called before the first frame update
    void Start()
    {
        m_rollingTextRef.SetCurrentValue(GameHandler._lastSeenScore);
        m_rollingTextRef.SetDesiredValue(GameHandler._score);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
