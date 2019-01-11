using System;
using System.Linq;
using System.Reflection;

namespace XUnity.AutoTranslator.Plugin.Core.Constants
{
   public static class ClrTypes
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

      // Something...
      public static readonly Type Typewriter = FindType( "Typewriter" );
      public static readonly Type TextEditor = FindType( "UnityEngine.TextEditor" );
      public static readonly Type CustomYieldInstruction = FindType( "UnityEngine.CustomYieldInstruction" );
      public static readonly Type SceneManager = FindType( "UnityEngine.SceneManager" );
      public static readonly Type Scene = FindType( "UnityEngine.SceneManagement.Scene" );

      // Utage
      public static readonly Type UguiNovelText = FindType( "Utage.UguiNovelText" );
      public static readonly Type AdvCommand = FindType( "Utage.AdvCommand" );
      public static readonly Type AdvEngine = FindType( "Utage.AdvEngine" );
      public static readonly Type AdvDataManager = FindType( "Utage.AdvDataManager" );
      public static readonly Type AdvScenarioData = FindType( "Utage.AdvScenarioData" );
      public static readonly Type AdvScenarioLabelData = FindType( "Utage.AdvScenarioLabelData" );

      private static Type FindType( string name )
      {
         return AppDomain.CurrentDomain.GetAssemblies()
            .Select( x => x.GetType( name, false ) )
            .Where( x => x != null )
            .FirstOrDefault();
      }
   }
}
