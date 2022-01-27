using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public enum typeChat
{
    Static,
    Bubble
}

public class TwitchChat : MonoBehaviour
{
    public bool highlightMessage; //message triggers
    public string message;
    public TextMeshProUGUI chatBox;
    public float chatFadeTime = 150;
    public float chatFadeTimer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (chatFadeTimer <= chatFadeTime)
        {
            chatFadeTimer += Time.deltaTime;
        }
        
        if (chatFadeTimer >= chatFadeTime)
        {
            chatBox.gameObject.SetActive(false);
        }
    }

    public void DisplayChat(string chatName, string chatColor, string chatText, bool isSub, bool isReward)
    {
        //if user has no chatcolor... make em a random color...
        if (chatColor == "")
        {
            int randomColor = Random.Range(0, 6);
            switch (randomColor)
            {
                case 0:
                    chatColor = "#F699CD";
                    break;
                case 1:
                    chatColor = "#C724B1";
                    break;
                case 2:
                    chatColor = "#4D4DFF";
                    break;
                case 3:
                    chatColor = "#E0E722";
                    break;
                case 4:
                    chatColor = "#FFAD00";
                    break;
                case 5:
                    chatColor = "#D22730";
                    break;
            }
        } 

        if (!isReward)
        {
            if (highlightMessage == true)
            {
                HighlightMessage(chatColor, chatName, chatText);
            }
            else
            {
                NormalMessage(chatColor, chatName, chatText);
            }
            
        }

        chatBox.text += message;
    }

    //message templates
    public void NormalMessage(string chatColor, string chatName, string chatText)
    {
        message = "<color=" + chatColor +">" + chatName + "</color>: " + chatText + "\n";
    }

    public void HighlightMessage(string chatColor, string chatName, string chatText)
    {
        message = "<size=50%> <color=#808080>" + chatName + " redeemed  \"HIGHLIGHTED MESSAGE\" </color></size>" + "\n" + "<color=" + chatColor +">" + chatName + "</color>: " + "<mark=#9146FF40 >" + chatText + "</mark> \n";
    }

    public void onChatMessage(string pChatter, string pMessage)
    {
        //mod commands
        if (pMessage.Contains("badges=broadcaster") || pMessage.Contains("badges=moderator"))
        {
            
        }

        highlightMessage = pMessage.Contains ("msg-id=highlighted-message");
    }
}
