#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
using UnityEngine.Android;
#endif

namespace Unity.UOS.Hello.HelloSdk.Samples.Tools
{
    public class PermissionHelper
    {
        public static void RequestMicrophonePermission()
        {
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
		if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
		{                 
			Permission.RequestUserPermission(Permission.Microphone);
		}
#endif
        }
    }
}