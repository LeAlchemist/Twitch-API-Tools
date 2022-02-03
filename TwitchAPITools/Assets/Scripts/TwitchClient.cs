using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class TwitchClient : MonoBehaviour
{
    [Tooltip("Chatbox Object Here")]
    public GameObject chat;
    [Tooltip("Emote Prefab Object Goes Here")]
    public GameObject emote;
    [Tooltip("Emote Spawn Location")]
    public Transform emoteStartPoint;
    [Tooltip("Module Events Go Here")]
    public UnityEvent<string, string> onChatMessage;
    TcpClient twitch;
    StreamReader reader;
    StreamWriter writer;
    const string URL = "irc.chat.twitch.tv";
    const int PORT = 6667;
    [Tooltip("User Name of the Bot Here")]
    public string user;
    [Tooltip("OAuth Key Here")]
    public string Oauth;
    [Tooltip("Streamer Channel Name Here")]
    public string channel;
    float pingCounter = 0;
    public string[] seperateEmotes;

    // Start is called before the first frame update
    void Start()
    {
        ConnectToTwitch();
    }

    void Awake() 
    {
        ConnectToTwitch();
    }

    // Update is called once per frame
    void Update()
    {
        pingCounter += Time.deltaTime;
        if (pingCounter > 60)
        {
            writer.WriteLine("PING " + URL);
            writer.Flush();
            pingCounter = 0;
        }

        if(!twitch.Connected)
        {
            ConnectToTwitch();
        }

        ProcessChat();
    }

    private void ProcessChat()
    {
        if(twitch.Available > 0)
        {
            string message = reader.ReadLine();

            if (message.Contains("PRIVMSG"))
            {
                // if the emotes list is not empty, get the emote texture
                if (!message.Contains("emotes=;"))
                {
                    string[] stringSeparatorsE = new string[] { "emotes=" };
                    string[] resultE = message.Split(stringSeparatorsE, StringSplitOptions.None);
                    // split the emote string in case of multiple emotes
                    var splitPointallEmotes = resultE[1].IndexOf(";", 0);
                    var allemotes = resultE[1].Substring(0, splitPointallEmotes);
                    seperateEmotes = allemotes.Split('/');
                    // grab all emote textures
                    for (int i = 0; i < seperateEmotes.Length; i++)
                    {
                        var id = seperateEmotes[i].IndexOf(":", 0);
                        var emoteID = seperateEmotes[i].Substring(0, id);
                        // 1.0 / 2.0 / 3.0 is texture sizes
                        StartCoroutine(GetTexture("https://static-cdn.jtvnw.net/emoticons/v1/" + emoteID + "/3.0"));
                    }
                }

                //get chat name
                string[] stringSepChatName = new string[] {"display-name="};
                string[] resultChatName = message.Split(stringSepChatName, StringSplitOptions.None);
                var splitPointChatName = resultChatName[1].IndexOf(";", 0);
                var chatName = resultChatName[1].Substring(0, splitPointChatName);

                //get chat color
                string[] stringSepChatColor = new string[] {"color="};
                string[] resultChatColor = message.Split(stringSepChatColor, System.StringSplitOptions.None);
                var splitPointChatColor = resultChatColor[1].IndexOf(";", 0);
                var chatColor = resultChatColor[1].Substring(0, splitPointChatColor);                

                //get chat message
                string[] stringSepChatMsg = new string[] { "PRIVMSG" };
                string[] resultChatMsg = message.Split(stringSepChatMsg, StringSplitOptions.None);
                var splitPointChatMsg = resultChatMsg[1].Split(':');
                string chatText = splitPointChatMsg[1];
                
                // check if chatter is a subscriber
                // this is where you can add sub specific stuff...
                if (message.Contains("@badge-info=subscriber"))
                {
                    print("this person is a subscriber");
                }

                //if message is a command or redemption
                int splitPointCommand = message.IndexOf("!");
                string chatter = message.Substring(1, splitPointCommand -1);

                splitPointCommand = message.IndexOf("", 1);
                string msg = message.Substring(splitPointCommand + 1);

                //custom reward (template)
                //You place this inside your sub scripts using:
                //if (pMessage.Contains("custom-reward-id=" + custom_rewards_id))
                //{
                //    print("Redeemed a custom reward");
                //}                
                onChatMessage?.Invoke(chatter, msg);
                
                //chat message for display                
                chat.GetComponent<TwitchChat>().DisplayChat(chatName, chatColor, chatText, message.Contains("@badge-info=subscriber"), message.Contains("custom-reward-id="));                   
            }

            //reply to ping to stay connected
            if (message.Contains("PING :tmi.twitch.tv"))
            {
                writer.WriteLine("PONG " + "tmi.twitch.tv" + "\r\n");
                writer.Flush();
            }

            if (message.Contains(":tmi.twitch.tv CLEARCHAT"))
            {
                //destroy all chat objects
            }

            print(message);
        }
    }

    IEnumerator GetTexture(string url)
    {
        // find the emote texture
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();
 
        // set it to an image, and spawn a particle with that image
        Texture2D img = DownloadHandlerTexture.GetContent(www);
        GameObject emotePart = Instantiate(emote, emoteStartPoint.position, emoteStartPoint.rotation, emoteStartPoint.transform); 
        emotePart.GetComponent<ParticleSystem>().GetComponent<Renderer>().material.mainTexture = img;
        //emotePart.GetComponent<ParticleSystem>().GetComponent<Renderer>().material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        //emotePart.GetComponent<ParticleSystem>().GetComponent<Renderer>().material.SetInt ("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        //emotePart.GetComponent<ParticleSystem>().GetComponent<Renderer>().material.SetInt ("_ZWrite", 0);
        //emotePart.GetComponent<ParticleSystem>().GetComponent<Renderer>().material.DisableKeyword ("_ALPHATEST_ON");
        //emotePart.GetComponent<ParticleSystem>().GetComponent<Renderer>().material.DisableKeyword ("_ALPHABLEND_ON");
        //emotePart.GetComponent<ParticleSystem>().GetComponent<Renderer>().material.EnableKeyword ("_ALPHAPREMULTIPLY_ON");
       
    }

    private void ConnectToTwitch()
    {
        twitch = new TcpClient(URL, PORT);
        reader = new StreamReader(twitch.GetStream());
        writer = new StreamWriter(twitch.GetStream());

        writer.WriteLine("PASS " + Oauth);
        writer.WriteLine("NICK " + user);
        writer.WriteLine("User " + user + " 8 * :" + user);
        writer.WriteLine("JOIN #" + channel.ToLower());
        writer.WriteLine("CAP REQ :twitch.tv/tags");
        writer.WriteLine("CAP REQ :twitch.tv/commands");
        writer.WriteLine("CAP REQ :twitch.tv/membership");
        writer.Flush();
    }
}