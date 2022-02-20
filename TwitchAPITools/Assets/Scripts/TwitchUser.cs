using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TwitchUser", menuName = "TwitchAPITools/TwitchUser", order = 0)]
public class TwitchUser : ScriptableObject
{
    [Tooltip("User Name of the Bot Here")]
    public string userName;
    [Tooltip("OAuth Key Here")]
    public string Oauth;
    [Tooltip("Streamer Channel Name Here")]
    public string channelName;
}

public class TwitchUserSave
{
    
}