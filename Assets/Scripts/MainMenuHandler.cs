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
        Cursor.visible = true;
    }

    public void OpenOptionsMenu()
    {
        Instantiate(m_optionsMenuPrefab);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Start"))
        {
            OpenOptionsMenu();
        }
    }

    public void MoveToSamsara()
    {
        if (!GameHandler._firstTimeCutscenePlayed)
        {
            FindObjectOfType<GameHandler>().TransitionScene(GameHandler.eScene.FirstTimeCutScene);
        }
        else
        {
            FindObjectOfType<GameHandler>().TransitionScene(GameHandler.eScene.Samsara);
        }
    }

    public void Exit()
    {
        GameHandler.Quit();
    }
}
