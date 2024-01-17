using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler : MonoBehaviour
{
    internal static GameHandler m_autoRef;
    [SerializeField] internal Color m_loveColor;
    [SerializeField] internal Color m_neutralColor;
    [SerializeField] internal Color m_fearColor;
    // Start is called before the first frame update

    void Awake()
    {
        if (m_autoRef == null)
        {
            m_autoRef = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
