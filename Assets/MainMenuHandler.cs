using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuHandler : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MoveToSamsara()
    {
        SceneManager.LoadScene("Samsara");
    }

    public void Exit()
    {
        Application.Quit();
    }
}
