using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneHandler : MonoBehaviour
{
    [SerializeField] DialogBox m_dialogBoxRef;
    [SerializeField] Sprite[] m_sprites;

    // Start is called before the first frame update
    void Start()
    {
        Sprite concernedSprite = m_sprites[0];
        Sprite enlightenedSprite = m_sprites[1];
        List<string> dialogs = new List<string>();
        List<Sprite> sprites = new List<Sprite>();
        dialogs.Add("Everyone says the world is collapsing.");
        sprites.Add(concernedSprite);
        dialogs.Add("The urge to lash out or give in feels strong, however something occurs to you. A path.");
        sprites.Add(concernedSprite);

        dialogs.Add("Letting go. And spreading <color=#ff004c>love</color>.");
        sprites.Add(enlightenedSprite);

        m_dialogBoxRef.Init(MoveToNextScene);
        m_dialogBoxRef.AddDialogs(dialogs);
        m_dialogBoxRef.AddImages(sprites);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void MoveToNextScene()
    {
        FindObjectOfType<GameHandler>().TransitionScene(GameHandler.eScene.Battle);
    }
}
