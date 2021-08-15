using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace UnityEngine
{
   public class Application
   {

      [Obsolete( "This property is deprecated, please use LoadLevelAsync to detect if a specific scene is currently loading." )]
      public static bool isLoadingLevel
      {


         get;
      }

      public static int streamedBytes
      {


         get;
      }

      public static bool isPlaying
      {


         get;
      }

      public static bool isFocused
      {


         get;
      }

      public static bool isEditor
      {


         get;
      }

      public static bool isWebPlayer
      {


         get;
      }

      public static RuntimePlatform platform
      {


         get;
      }

      public static string buildGUID
      {


         get;
      }

      public static bool isMobilePlatform
      {
         get
         {
            switch( platform )
            {
               case RuntimePlatform.IPhonePlayer:
               case RuntimePlatform.Android:
               case RuntimePlatform.MetroPlayerX86:
               case RuntimePlatform.MetroPlayerX64:
               case RuntimePlatform.MetroPlayerARM:
               case RuntimePlatform.TizenPlayer:
                  return true;
               default:
                  return false;
            }
         }
      }

      public static bool isConsolePlatform
      {
         get
         {
            RuntimePlatform platform = Application.platform;
            return platform == RuntimePlatform.PS4 || platform == RuntimePlatform.XboxOne;
         }
      }

      public static bool runInBackground
      {


         get;


         set;
      }

      [Obsolete( "use Application.isEditor instead" )]
      public static bool isPlayer => !isEditor;

      internal static bool isBatchmode
      {


         get;
      }

      internal static bool isTestRun
      {


         get;
      }

      internal static bool isHumanControllingUs
      {


         get;
      }

      public static string dataPath
      {


         get;
      }

      public static string streamingAssetsPath
      {


         get;
      }

      public static string persistentDataPath
      {


         get;
      }

      public static string temporaryCachePath
      {


         get;
      }

      public static string srcValue
      {


         get;
      }

      public static string absoluteURL
      {


         get;
      }

      public static string unityVersion
      {


         get;
      }

      public static string version
      {


         get;
      }

      public static string installerName
      {


         get;
      }

      public static string identifier
      {


         get;
      }

      public static ApplicationInstallMode installMode
      {


         get;
      }

      public static ApplicationSandboxType sandboxType
      {


         get;
      }

      public static string productName
      {


         get;
      }

      public static string companyName
      {


         get;
      }

      public static string cloudProjectId
      {


         get;
      }

      [Obsolete( "Application.webSecurityEnabled is no longer supported, since the Unity Web Player is no longer supported by Unity." )]
      public static bool webSecurityEnabled
      {


         get;
      }

      [Obsolete( "Application.webSecurityHostUrl is no longer supported, since the Unity Web Player is no longer supported by Unity." )]
      public static string webSecurityHostUrl
      {


         get;
      }

      public static int targetFrameRate
      {


         get;


         set;
      }

      public static SystemLanguage systemLanguage
      {


         get;
      }

      [Obsolete( "Use SetStackTraceLogType/GetStackTraceLogType instead" )]
      public static StackTraceLogType stackTraceLogType
      {


         get;


         set;
      }

      public static ThreadPriority backgroundLoadingPriority
      {


         get;


         set;
      }

      public static NetworkReachability internetReachability
      {


         get;
      }

      public static bool genuine
      {


         get;
      }

      public static bool genuineCheckAvailable
      {


         get;
      }

      internal static bool submitAnalytics
      {


         get;
      }

      [Obsolete( "This property is deprecated, please use SplashScreen.isFinished instead" )]
      public static bool isShowingSplashScreen => throw new NotImplementedException();

      [Obsolete( "Use SceneManager.sceneCountInBuildSettings" )]
      public static int levelCount => SceneManager.sceneCountInBuildSettings;

      [Obsolete( "Use SceneManager to determine what scenes have been loaded" )]
      public static int loadedLevel => SceneManager.GetActiveScene().buildIndex;

      [Obsolete( "Use SceneManager to determine what scenes have been loaded" )]
      public static string loadedLevelName => SceneManager.GetActiveScene().name;

      public static extern void Quit();

   }
}
