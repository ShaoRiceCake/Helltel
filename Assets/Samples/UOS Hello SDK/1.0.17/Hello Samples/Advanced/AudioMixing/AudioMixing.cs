using System;
using System.Threading.Tasks;
using Agora.Rtc;
using Unity.UOS.Auth;
using Unity.UOS.Hello.Exception;
using Unity.UOS.Hello.HelloSdk.Samples.Tools;
using Unity.UOS.Hello.Model;
using UnityEngine;
using UnityEngine.UI;
using Logger = Unity.UOS.Hello.HelloSdk.Samples.Tools.Logger;
using Random = System.Random;
using TokenInfo = Unity.UOS.Hello.Model.TokenInfo;

// using UnityEngine.Networking.Types;

namespace Unity.UOS.Hello.HelloSdk.Samples.Advanced.AudioMixing
{
    public class AudioMixing : MonoBehaviour
    {
        [SerializeField] public string Sound_URL = "";

        private string _localPath = "";

        public string userId;
        public Text LogText;
        internal Logger Log;
        internal IRtcEngine RtcEngine = null;
        internal TokenInfo TokenInfo;
        internal readonly string channelName = "channel-1";
        
        private Toggle _urlToggle;
        private Toggle _loopbackToggle;

        // Start is called before the first frame update
        private async void Awake()
        {
            // 初始化 sdk instance
            try
            {
                // 如果安装了Uos Launcher 使用Uos Launcher关联的Uos App初始化SDK
                HelloSDK.Initialize();
                // 如果需要使用其他 UOS APP，可传入一对AppId, AppSecret初始化SDK
                // await HelloSDK.InitializeAsync(appId, appSecret);
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

            // 默认生成不重复的唯一数字字符串作为userId。用户也可自行填写或生成userId，但也需注意userId不可重复
            if (string.IsNullOrEmpty(userId))
            {
                userId = GenerateRandomUserId();
            }

            // 初始化AuthTokenManager中的user token
            await AuthTokenManager.ExternalLogin(userId);

            var handler = new UserEventHandler(this);
            var context = new RtcEngineContext
            {
                channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT,
                areaCode = AREA_CODE.AREA_CODE_GLOB
            };

            // 初始化语音引擎RtcEngine
            RtcEngine = await HelloSDK.InitRtcEngine(context, handler);
            Debug.Log($"successfully init agora rtc engine");

            // 生成声网项目鉴权AccessToken
            await GenerateAccessToken(channelName, new HelloOptions { role = Role.Publisher });
            Debug.Log($"access token: {TokenInfo.AccessToken}");

            // 配置语音引擎
            RtcEngine.EnableAudio();
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            
            Log = new Logger(LogText);
            SetupUI();
            // enable it after joining
            EnableUI(false);
            JoinChannel();
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
                Debug.LogErrorFormat("failed to generate token, serverEx: {0}",  e.Message);
                throw;
            }
        }

        private void SetupUI()
        {
            _mixingButton = GameObject.Find("MixButton").GetComponent<Button>();
            _mixingButton.onClick.AddListener(HandleAudioMixingButton);
            _effectButton = GameObject.Find("EffectButton").GetComponent<Button>();
            _effectButton.onClick.AddListener(HandleEffectButton);
            _urlToggle = GameObject.Find("Toggle").GetComponent<Toggle>();
            _loopbackToggle = GameObject.Find("Loopback").GetComponent<Toggle>();


#if UNITY_ANDROID && !UNITY_EDITOR
            // On Android, the StreamingAssetPath is just accessed by /assets instead of Application.streamingAssetPath
            _localPath = "/assets/audio/Agora.io-Interactions.mp3";
#else
            _localPath = Application.streamingAssetsPath + "/audio/" + "Agora.io-Interactions.mp3";
#endif
            Log.UpdateLog($"the audio file path: {_localPath}");
        }

        internal void EnableUI(bool enable)
        {
            _mixingButton.enabled = enable;
            _effectButton.enabled = enable;
        }

        private void JoinChannel()
        {
            RtcEngine.JoinChannelWithUserAccount(TokenInfo.AccessToken, TokenInfo.ChannelName, TokenInfo.UserId);
        }

        private static string GenerateRandomUserId()
        {
            // 默认生成随机8位数字字符串作为userId
            var random = new Random();
            return random.Next(10000000, 20000000).ToString();
        }
        
        #region -- Test Control logic ---

        private void StartAudioMixing()
        {
            Debug.Log("Playing with " + (_urlToggle.isOn ? "URL" : "local file"));

            var ret = RtcEngine.StartAudioMixing(_urlToggle.isOn ? Sound_URL : _localPath, _loopbackToggle.isOn, 1);
            Debug.Log("StartAudioMixing returns: " + ret);
        }


        private void PlayEffectTest()
        {
            Debug.Log("Playing with " + (_urlToggle.isOn ? "URL" : "local file"));
            RtcEngine.PlayEffect(1, _urlToggle.isOn ? Sound_URL : _localPath, 1, 1.0, 0, 100, !_loopbackToggle.isOn, 0);
        }

        private void StopEffectTest()
        {
            RtcEngine.StopAllEffects();
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


        #region -- Application UI Logic ---

        private bool _isMixing = false;
        private Button _mixingButton { get; set; }

        private void HandleAudioMixingButton()
        {
            if (_effectOn)
            {
                Log.UpdateLog("Testing Effect right now, can't play effect...");
                return;
            }

            if (_isMixing)
            {
                RtcEngine.StopAudioMixing();
            }
            else
            {
                StartAudioMixing();
            }

            _isMixing = !_isMixing;
            _mixingButton.GetComponentInChildren<Text>().text = (_isMixing ? "Stop Mixing" : "Start Mixing");
        }


        private bool _effectOn = false;
        private Button _effectButton { get; set; }

        private void HandleEffectButton()
        {
            if (_isMixing)
            {
                Log.UpdateLog("Testing Mixing right now, can't play effect...");
                return;
            }

            if (_effectOn)
            {
                StopEffectTest();
            }
            else
            {
                PlayEffectTest();
            }

            _effectOn = !_effectOn;
            _effectButton.GetComponentInChildren<Text>().text = (_effectOn ? "Stop Effect" : "Play Effect");
        }




        #endregion
    }

    #region -- Agora Event ---

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly AudioMixing _audioMixing;

        internal UserEventHandler(AudioMixing audioMixing)
        {
            _audioMixing = audioMixing;
        }

        public override void OnError(int err, string msg)
        {
            _audioMixing.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            _audioMixing.Log.UpdateLog(string.Format("sdk version: ${0}",
                _audioMixing.RtcEngine.GetVersion(ref build)));
            _audioMixing.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                    connection.channelId, connection.localUid, elapsed));
            _audioMixing.EnableUI(true);
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _audioMixing.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _audioMixing.Log.UpdateLog("OnLeaveChannel");
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
        {
            _audioMixing.Log.UpdateLog("OnClientRoleChanged");
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _audioMixing.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _audioMixing.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
        }

        public override void OnAudioMixingStateChanged(AUDIO_MIXING_STATE_TYPE state, AUDIO_MIXING_REASON_TYPE errorCode)
        {
            _audioMixing.Log.UpdateLog(string.Format("AUDIO_MIXING_STATE_TYPE: ${0}, AUDIO_MIXING_REASON_TYPE: ${1}",
                state, errorCode));
        }
    }

    #endregion
}