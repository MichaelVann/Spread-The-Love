using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityReadout : MonoBehaviour
{
    [SerializeField] GameObject m_lockRef;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void SetUnlocked(bool a_unlocked)
    {
        m_lockRef.SetActive(!a_unlocked);
    }
}
