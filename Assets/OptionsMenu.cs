using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
    BattleHandler m_battleHandlerRef;
    [SerializeField] GameObject m_resumeButtonRef;
    [SerializeField] GameObject m_perishButtonRef;
    [SerializeField] GameObject m_quitButtonRef;
    // Start is called before the first frame update
    void Awake()
    {
        m_perishButtonRef.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Resume();
        }
    }

    internal void Init(BattleHandler a_battleHandler)
    {
        m_battleHandlerRef = a_battleHandler;
        m_perishButtonRef.SetActive(true);
    }

    public void Perish()
    {
        m_battleHandlerRef.Perish();
        Resume();
    }

    public void Resume()
    {
        Destroy(gameObject);
        if (m_battleHandlerRef != null)
        {
            m_battleHandlerRef.SetPaused(false);
        }
    }

    public void QuitGame()
    {
        GameHandler.Quit();
    }
}
