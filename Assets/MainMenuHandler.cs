using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] GameObject m_optionsMenuPrefab;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OpenOptionsMenu()
    {
        Instantiate(m_optionsMenuPrefab);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OpenOptionsMenu();
        }
    }

    public void MoveToSamsara()
    {
        FindObjectOfType<GameHandler>().TransitionScene(GameHandler.eScene.Samsara);
    }

    public void Exit()
    {
        GameHandler.Quit();
    }
}
