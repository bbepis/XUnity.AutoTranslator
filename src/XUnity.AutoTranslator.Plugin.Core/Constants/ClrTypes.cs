using System;
using System.Linq;
using System.Reflection;

namespace XUnity.AutoTranslator.Plugin.Core.Constants
{
   internal static class ClrTypes
   {
      // TextMeshPro
      public static readonly Type TMP_InputField = FindType( "TMPro.TMP_InputField" );
      public static readonly Type TMP_Text = FindType( "TMPro.TMP_Text" );
      public static readonly Type TextMeshProUGUI = FindType( "TMPro.TextMeshProUGUI" );
      public static readonly Type TextMeshPro = FindType( "TMPro.TextMeshPro" );

      // NGUI
      public static readonly Type UILabel = FindType( "UILabel" );
      public static readonly Type UIWidget = FindType( "UIWidget" );
      public static readonly Type UIAtlas = FindType( "UIAtlas" );
      public static readonly Type UISprite = FindType( "UISprite" );
      public static readonly Type UITexture = FindType( "UITexture" );
      public static readonly Type UI2DSprite = FindType( "UI2DSprite" );
      public static readonly Type UIFont = FindType( "UIFont" );
      public static readonly Type UIPanel = FindType( "UIPanel" );
      public static readonly Type UIRect = FindType( "UIRect" );

      // Unity
      public static readonly Type WWW = FindType( "UnityEngine.WWW" );
      public static readonly Type InputField = FindType( "UnityEngine.UI.InputField" );
      public static readonly Type Text = FindType( "UnityEngine.UI.Text" );
      public static readonly Type GUI = FindType( "UnityEngine.GUI" );
      public static readonly Type ImageConversion = FindType( "UnityEngine.ImageConversion" );
      public static readonly Type Texture = FindType( "UnityEngine.Texture" );
      public static readonly Type SpriteRenderer = FindType( "UnityEngine.SpriteRenderer" );
      public static readonly Type Object = FindType( "UnityEngine.Object" );
      public static readonly Type TextEditor = FindType( "UnityEngine.TextEditor" );
      public static readonly Type CustomYieldInstruction = FindType( "UnityEngine.CustomYieldInstruction" );
      public static readonly Type SceneManager = FindType( "UnityEngine.SceneManagement.SceneManager" );
      public static readonly Type Scene = FindType( "UnityEngine.SceneManagement.Scene" );
      //public static readonly Type GraphicRaycaster = FindType( "UnityEngine.UI.GraphicRaycaster" );

      // Something...
      public static readonly Type Typewriter = FindType( "Typewriter" );

      // Utage
      public static readonly Type UguiNovelText = FindType( "Utage.UguiNovelText" );
      public static readonly Type AdvCommand = FindType( "Utage.AdvCommand" );
      public static readonly Type AdvEngine = FindType( "Utage.AdvEngine" );
      public static readonly Type AdvDataManager = FindType( "Utage.AdvDataManager" );
      public static readonly Type AdvScenarioData = FindType( "Utage.AdvScenarioData" );
      public static readonly Type AdvScenarioLabelData = FindType( "Utage.AdvScenarioLabelData" );

      // Live2D
      public static readonly Type CubismRenderer = FindType( "Live2D.Cubism.Rendering.CubismRenderer" );

      // Harmony
      public static readonly Type HarmonyInstance = FindType( "Harmony.HarmonyInstance" );
      public static readonly Type HarmonyMethod = FindType( "Harmony.HarmonyMethod" );

      // Mono / .NET
      public static readonly Type MethodBase = FindType( "System.Reflection.MethodBase" );
      public static readonly Type Task = FindType( "System.Threading.Tasks.Task" );

      private static Type FindType( string name )
      {
         return AppDomain.CurrentDomain.GetAssemblies()
            .Select( x => x.GetType( name, false ) )
            .Where( x => x != null )
            .FirstOrDefault();
      }
   }
}
