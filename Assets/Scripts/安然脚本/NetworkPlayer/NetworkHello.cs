using System;
using System.Linq;
using System.Threading.Tasks;
using Agora.Rtc;
using Unity.UOS.Auth;
using Unity.UOS.Hello;
using Unity.UOS.Hello.Exception;
using Unity.UOS.Hello.HelloSdk.Samples.Tools;
using Unity.UOS.Hello.Model;
using UnityEngine;
using UnityEngine.UI;
using Logger = Unity.UOS.Hello.HelloSdk.Samples.Tools.Logger;
using Random = System.Random;
using TokenInfo = Unity.UOS.Hello.Model.TokenInfo;

public class NetworkHello : MonoBehaviour
{
    private static bool HelloIns;
    public string appId;
    public string appSecret;
    public string userId;

    internal IRtcEngine RtcEngine = null;
    internal TokenInfo TokenInfo;
    internal string channelName = "Helltel";

    private IAudioDeviceManager _audioDeviceManager;
    private DeviceInfo[] _audioPlaybackDeviceInfos;

    private async void Awake()
    {
        if (HelloIns)
        {
            Destroy(gameObject);
        }
        else
        {
            HelloIns = true;
            DontDestroyOnLoad(gameObject);
        }

        // ��ʼ�� sdk instance
        try
        {
            // �����װ��Uos Launcher ʹ��Uos Launcher������Uos App��ʼ��SDK
            HelloSDK.Initialize();
            // �����Ҫʹ������ UOS APP���ɴ���һ��AppId, AppSecret��ʼ��SDK
            // HelloSDK.Initialize(appId, appSecret);
        }
        catch (HelloClientException e)
        {
            Debug.Log($"failed to initialize sdk, clientEx: {e.Message}");
            throw;
        }
        catch (HelloServerException e)
        {
            Debug.Log($"failed to initialize sdk, serverEx: {e.Message}");
            throw;
        }

        // Ĭ�����ɲ��ظ���Ψһ�����ַ�����ΪuserId���û�Ҳ��������д������userId����Ҳ��ע��userId�����ظ�
        if (string.IsNullOrEmpty(userId))
        {
            userId = GenerateRandomUserId();
        }

        // ��ʼ��AuthTokenManager�е�user token
        await AuthTokenManager.ExternalLogin(userId);

        var handler = new UserEventHandler(this);
        var context = new RtcEngineContext
        {
            channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
            audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT,
            areaCode = AREA_CODE.AREA_CODE_GLOB
        };

        // ��ʼ����������RtcEngine
        RtcEngine = await HelloSDK.InitRtcEngine(context, handler);
        Debug.Log($"successfully init agora rtc engine");

        // ����������Ŀ��ȨAccessToken
        await GenerateAccessToken(channelName, new HelloOptions { role = Role.Publisher });
        Debug.Log($"access token: {TokenInfo.AccessToken}");

        // ������������
        RtcEngine.EnableAudio();
        RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        JoinChannel();

    }

    public void SetOtherVolume(int value)
    {
        RtcEngine.AdjustPlaybackSignalVolume(value);
    }

    public void MyVolume(int value)
    {
        RtcEngine.AdjustRecordingSignalVolume(value);

    }

    private void Update()
    {
        PermissionHelper.RequestMicrophonePermission();
    }

    private async Task GenerateAccessToken(string channel, HelloOptions options = null)
    {
        try
        {
            TokenInfo = await HelloSDK.Instance.GenerateAccessToken(channel, options);
        }
        catch (HelloClientException e)
        {
            Debug.LogErrorFormat("failed to generate token, clientEx: {0}", e.Message);
            throw;
        }
        catch (HelloServerException e)
        {
            Debug.LogErrorFormat("failed to generate token, serverEx: {0}", e.Message);
            throw;
        }
    }

    private static string GenerateRandomUserId()
    {
        // Ĭ���������8λ�����ַ�����ΪuserId
        var random = new Random();
        return random.Next(10000000, 20000000).ToString();
    }

    #region -- Button Events ---

    public void JoinChannel()
    {
        // ����Ƶ��
        RtcEngine.JoinChannelWithUserAccount(TokenInfo.AccessToken, TokenInfo.ChannelName, TokenInfo.UserId);
    }

    public void LeaveChannel()
    {
        // �뿪Ƶ��
        RtcEngine.LeaveChannel();
    }

    public void StartEchoTest()
    {
        Debug.Log($"echo channelName {channelName} access token {TokenInfo.AccessToken}");
        var config = new EchoTestConfiguration
        {
            intervalInSeconds = 2,
            enableAudio = true,
            enableVideo = false,
            token = TokenInfo.AccessToken,
            channelId = channelName
        };
        RtcEngine.StartEchoTest(config);
        Debug.Log("StartEchoTest, speak now. You cannot conduct another echo test or join a channel before StopEchoTest");
    }

    public void StopEchoTest()
    {
        RtcEngine.StopEchoTest();
    }

    public void StopPublishAudio()
    {
        var options = new ChannelMediaOptions();
        options.publishMicrophoneTrack.SetValue(false);
        var nRet = RtcEngine.UpdateChannelMediaOptions(options);
        Debug.Log("UpdateChannelMediaOptions: " + nRet);
    }

    public void StartPublishAudio()
    {
        var options = new ChannelMediaOptions();
        options.publishMicrophoneTrack.SetValue(true);
        var nRet = RtcEngine.UpdateChannelMediaOptions(options);
        Debug.Log("UpdateChannelMediaOptions: " + nRet);
    }

    public void GetAudioPlaybackDevice()
    {
        _audioDeviceManager = RtcEngine.GetAudioDeviceManager();
        _audioPlaybackDeviceInfos = _audioDeviceManager.EnumeratePlaybackDevices();
        Debug.Log(string.Format("AudioPlaybackDevice count: {0}", _audioPlaybackDeviceInfos.Length));
        for (var i = 0; i < _audioPlaybackDeviceInfos.Length; i++)
        {
            Debug.Log(string.Format("AudioPlaybackDevice device index: {0}, name: {1}, id: {2}", i,
                _audioPlaybackDeviceInfos[i].deviceName, _audioPlaybackDeviceInfos[i].deviceId));
        }
    }

    #endregion

    private void OnDestroy()
    {
        Debug.Log("OnDestroy");
        if (RtcEngine == null) return;
        RtcEngine.InitEventHandler(null);
        RtcEngine.LeaveChannel();
        RtcEngine.Dispose();
    }
}
#region -- Agora Event ---

internal class UserEventHandler : IRtcEngineEventHandler
{
    private readonly NetworkHello _audioSample;

    internal UserEventHandler(NetworkHello audioSample)
    {
        _audioSample = audioSample;
    }

    public override void OnError(int err, string msg)
    {
        Debug.Log($"OnError err: {err}, msg: {msg}"); ;
    }

    public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
    {
        int build = 0;
        Debug.Log($"sdk version: ${_audioSample.RtcEngine.GetVersion(ref build)}");
        Debug.Log(
            $"OnJoinChannelSuccess channelName: {connection.channelId}, uid: {connection.localUid}, elapsed: {elapsed}");
    }

    public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
    {
        Debug.Log("OnRejoinChannelSuccess");
    }

    public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
    {
        Debug.Log("OnLeaveChannel");
    }

    public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
    {
        Debug.Log("OnClientRoleChanged");
    }

    public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
    {
        Debug.Log($"OnUserJoined uid: ${uid} elapsed: ${elapsed}");
    }

    public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
    {
        Debug.Log($"OnUserOffLine uid: ${uid}, reason: ${(int)reason}");
    }
}

#endregion
