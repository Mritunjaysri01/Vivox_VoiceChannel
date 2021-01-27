using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VivoxUnity;
using System.ComponentModel;
using System;
using UnityEngine.UI;
using TMPro;

public class LoginCredentials : MonoBehaviour
{
    VivoxUnity.Client client;
    
    // Add vivox server credntials for your project
    
    private Uri server = new Uri("************");
    private string issuer = "***********";
    private string  domain = "********";
    private string tokenKey = "********";
    private TimeSpan timeSpan = new TimeSpan(90);


    private ILoginSession loginSession;
    private IChannelSession channelSession;


    #region UI Variables
    // Updated UI variables not shown In my first video
    // The rest of the code is fairly the same
    [SerializeField] Text txt_UserName;
    [SerializeField] Text txt_ChannelName;
   
    [SerializeField] TMP_InputField tmp_Input_Username;
    [SerializeField] TMP_InputField tmp_Input_ChannelName;
 
   

    #endregion



    private void Awake()
    {
        client = new Client();
        client.Uninitialize();
        client.Initialize();
        DontDestroyOnLoad(this);
    }

    private void OnApplicationQuit()
    {
        client.Uninitialize();
    }

    void Start()
    {
     

    }

    public void Bind_Login_Callback_Listeners(bool bind, ILoginSession loginSesh)
    {
        if (bind)
        {
             loginSesh.PropertyChanged += Login_Status;
        }
        else
        {
            loginSesh.PropertyChanged -= Login_Status;
        }
    }

    public void Bind_Channel_Callback_Listeners(bool bind, IChannelSession channelSesh)
    {
        if (bind)
        {
            channelSesh.PropertyChanged += On_Channel_Status_Changed;
        }
        else
        {
            channelSesh.PropertyChanged -= On_Channel_Status_Changed;
        }
    }


    #region Login Methods

    public void LoginUser()
    {
        Login(tmp_Input_Username.text);
    }

    public void Login(string userName)
    {
        AccountId accountId = new AccountId(issuer, userName, domain);
        loginSession = client.GetLoginSession(accountId);

        Bind_Login_Callback_Listeners(true, loginSession);
        loginSession.BeginLogin(server, loginSession.GetLoginToken(tokenKey, timeSpan), ar =>
        {
            try
            {
                loginSession.EndLogin(ar);
            }
            catch (Exception e)
            {
                Bind_Login_Callback_Listeners(false, loginSession);
                Debug.Log(e.Message);
            }
            // run more code here 
        });
    }

    public void Logout()
    {
        loginSession.Logout();
        Bind_Login_Callback_Listeners(false, loginSession);
    }

    public void Login_Status(object sender, PropertyChangedEventArgs loginArgs)
    {
        var source = (ILoginSession)sender;
        
        switch (source.State)
        {
            case LoginState.LoggingIn:
                Debug.Log("Logging In");
                break;
            
            case LoginState.LoggedIn:
                Debug.Log($"Logged In {loginSession.LoginSessionId.Name}");
                txt_UserName.text = loginSession.LoginSessionId.Name;
                break;
        }
    }



    #endregion


    #region Join Channel Methods

    public void JoinChannel(string channelName, bool IsAudio, bool IsText, bool switchTransmission,ChannelType channelType)
    {

        ChannelId channelId = new ChannelId(issuer, channelName, domain, channelType);
        channelSession = loginSession.GetChannelSession(channelId);
        Bind_Channel_Callback_Listeners(true, channelSession);

        channelSession.BeginConnect(IsAudio, IsText, switchTransmission, channelSession.GetConnectToken(tokenKey, timeSpan), ar => 
        {
            try
            {
                channelSession.EndConnect(ar);
            }
            catch(Exception e)
            {
                Bind_Channel_Callback_Listeners(false, channelSession);
                Debug.Log(e.Message);
            }
        });
    }


    public void Btn_Join_Channel()
    {
        JoinChannel(tmp_Input_ChannelName.text, true, true, true, ChannelType.NonPositional);
    }

    public void Leave_Channel(IChannelSession channelToDiconnect, string channelName)
    {
        channelToDiconnect.Disconnect();
        loginSession.DeleteChannelSession(new ChannelId(issuer, channelName, domain));
    }

    public void Btn_Leave_Channel_Clicked()
    {
        Leave_Channel(channelSession, tmp_Input_ChannelName.text);
    }

    public void On_Channel_Status_Changed(object sender, PropertyChangedEventArgs channelArgs)
    {
        IChannelSession source = (IChannelSession)sender;

        switch (source.ChannelState)
        {
            case ConnectionState.Connecting:
                Debug.Log("Channel Connecting");
                break;    
            case ConnectionState.Connected:
                Debug.Log($"{source.Channel.Name} Connected");
                txt_ChannelName.text = source.Channel.Name;
                break;       
            case ConnectionState.Disconnecting:
                Debug.Log($"{source.Channel.Name} disconnecting");
                break;    
            case ConnectionState.Disconnected:
                Debug.Log($"{source.Channel.Name} disconnected");
                break;
        }
    }

    #endregion



}
