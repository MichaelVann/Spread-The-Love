using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConfirmationBox : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI m_dialogueText;

    internal delegate void DecisionCallbackDelegate(bool a_decision);
    DecisionCallbackDelegate m_decisionCallBackDelegate;
    // Start is called before the first frame update

    internal void SetDecisionCallbackDelegate(DecisionCallbackDelegate a_decisionCallBackDelegate) { m_decisionCallBackDelegate = a_decisionCallBackDelegate; }
    internal void SetDialogueText(string a_text) { m_dialogueText.text = a_text; }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Decide(bool a_decision)
    {
        if (m_decisionCallBackDelegate != null) 
        {
            m_decisionCallBackDelegate.Invoke(a_decision);
        }
        Destroy(gameObject);
    }
}
