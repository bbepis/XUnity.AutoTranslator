using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XUnity.Common.Utilities;

namespace XUnity.Common.Constants
{
#pragma warning disable CS1591 // Really could not care less..

   public static class ClrTypes
   {
      // TextMeshPro
      public static readonly Type TMP_InputField = FindType( "TMPro.TMP_InputField" );
      public static readonly Type TMP_Text = FindType( "TMPro.TMP_Text" );
      public static readonly Type TextMeshProUGUI = FindType( "TMPro.TextMeshProUGUI" );
      public static readonly Type TextMeshPro = FindType( "TMPro.TextMeshPro" );
      public static readonly Type FontAsset = FindType( "TMPro.TMP_FontAsset" );
      public static readonly Type AssetBundle = FindType( "UnityEngine.AssetBundle" );

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
      public static readonly Type UIInput = FindType( "UIInput" );

      // FairyGUI
      public static readonly Type TextField = FindType( "FairyGUI.TextField" );

      // Unity
      public static readonly Type GameObject = FindType( "UnityEngine.GameObject" );
      public static readonly Type Transform = FindType( "UnityEngine.Transform" );
      public static readonly Type TextMesh = FindType( "UnityEngine.TextMesh" );
      public static readonly Type Text = FindType( "UnityEngine.UI.Text" );
      public static readonly Type Image = FindType( "UnityEngine.UI.Image" );
      public static readonly Type RawImage = FindType( "UnityEngine.UI.RawImage" );
      public static readonly Type MaskableGraphic = FindType( "UnityEngine.UI.MaskableGraphic" );
      public static readonly Type Graphic = FindType( "UnityEngine.UI.Graphic" );
      public static readonly Type GUIContent = FindType( "UnityEngine.GUIContent" );
      public static readonly Type WWW = FindType( "UnityEngine.WWW" );
      public static readonly Type InputField = FindType( "UnityEngine.UI.InputField" );
      public static readonly Type GUI = FindType( "UnityEngine.GUI" );
      public static readonly Type GUIStyle = FindType( "UnityEngine.GUIStyle" );
      public static readonly Type GUI_ToolbarButtonSize = FindType( "UnityEngine.GUI+ToolbarButtonSize" );
      public static readonly Type ImageConversion = FindType( "UnityEngine.ImageConversion" );
      public static readonly Type Texture = FindType( "UnityEngine.Texture" );
      public static readonly Type SpriteRenderer = FindType( "UnityEngine.SpriteRenderer" );
      public static readonly Type Sprite = FindType( "UnityEngine.Sprite" );
      public static readonly Type Object = FindType( "UnityEngine.Object" );
      public static readonly Type TextEditor = FindType( "UnityEngine.TextEditor" );
      public static readonly Type CustomYieldInstruction = FindType( "UnityEngine.CustomYieldInstruction" );
      public static readonly Type SceneManager = FindType( "UnityEngine.SceneManagement.SceneManager" );
      public static readonly Type Scene = FindType( "UnityEngine.SceneManagement.Scene" );
      public static readonly Type UnityEventBase = FindType( "UnityEngine.Events.UnityEventBase" );
      public static readonly Type BaseInvokableCall = FindType( "UnityEngine.Events.BaseInvokableCall" );
      public static readonly Type HorizontalWrapMode = FindType( "UnityEngine.HorizontalWrapMode" );
      public static readonly Type VerticalWrapMode = FindType( "UnityEngine.VerticalWrapMode" );
      public static readonly Type Font = FindType( "UnityEngine.Font" );
      public static readonly Type WaitForSecondsRealtime = FindType( "UnityEngine.WaitForSecondsRealtime" );

      // Shimeji Engine
      public static readonly Type TextExpansion = FindType( "UnityEngine.UI.TextExpansion" );

      // Something...
      public static readonly Type Typewriter = FindType( "Typewriter" );

      // Utage
      public static readonly Type UguiNovelText = FindType( "Utage.UguiNovelText" );
      public static readonly Type AdvEngine = FindType( "Utage.AdvEngine" );
      public static readonly Type AdvPage = FindType( "Utage.AdvPage" );
      public static readonly Type TextData = FindType( "Utage.TextData" );
      public static readonly Type AdvUguiMessageWindow = FindType( "Utage.AdvUguiMessageWindow" ) ?? FindType( "AdvUguiMessageWindow" );
      public static readonly Type AdvUiMessageWindow = FindType( "AdvUiMessageWindow" );
      public static readonly Type AdvDataManager = FindType( "Utage.AdvDataManager" );
      public static readonly Type AdvScenarioData = FindType( "Utage.AdvScenarioData" );
      public static readonly Type AdvScenarioLabelData = FindType( "Utage.AdvScenarioLabelData" );
      public static readonly Type DicingTextures = FindType( "Utage.DicingTextures" );
      public static readonly Type DicingImage = FindType( "Utage.DicingImage" );
      public static readonly Type TextArea2D = FindType( "Utage.TextArea2D" );

      // Live2D
      public static readonly Type CubismRenderer = FindType( "Live2D.Cubism.Rendering.CubismRenderer" );

      // Harmony
      public static readonly Type AccessTools = FindTypeStrict( "Harmony.AccessTools, 0Harmony" ) ?? FindTypeStrict( "HarmonyLib.AccessTools, 0Harmony" );
      public static readonly Type HarmonyInstance = FindTypeStrict( "Harmony.HarmonyInstance, 0Harmony" );
      public static readonly Type HarmonyMethod = FindTypeStrict( "Harmony.HarmonyMethod, 0Harmony" ) ?? FindTypeStrict( "HarmonyLib.HarmonyMethod, 0Harmony" );
      public static readonly Type Harmony = FindTypeStrict( "HarmonyLib.Harmony, 0Harmony" );

      // MonoMod
      public static readonly Type Hook = FindTypeStrict( "MonoMod.RuntimeDetour.Hook, MonoMod.RuntimeDetour" );
      public static readonly Type Detour = FindTypeStrict( "MonoMod.RuntimeDetour.Detour, MonoMod.RuntimeDetour" );
      public static readonly Type NativeDetour = FindTypeStrict( "MonoMod.RuntimeDetour.NativeDetour, MonoMod.RuntimeDetour" );
      public static readonly Type DynamicMethodDefinition = FindTypeStrict( "MonoMod.Utils.DynamicMethodDefinition, MonoMod.Utils" );

      // Mono / .NET
      public static readonly Type MethodBase = FindType( "System.Reflection.MethodBase" );
      public static readonly Type Task = FindType( "System.Threading.Tasks.Task" );

      // Assets.System (what engine is this?)
      public static readonly Type TextWindow = FindType( "Assets.System.Text.TextWindow" );

      public static class AdvUguiMessageWindow_Properties
      {
         public static CachedProperty Text = ClrTypes.AdvUguiMessageWindow?.CachedProperty( "Text" );
         public static CachedProperty Engine = ClrTypes.AdvUguiMessageWindow?.CachedProperty( "Engine" );
      }

      public static class AdvUiMessageWindow_Fields
      {
         public static CachedField text = ClrTypes.AdvUiMessageWindow?.CachedField( "text" );
         public static CachedField nameText = ClrTypes.AdvUiMessageWindow?.CachedField( "nameText" );
      }

      public static class AdvUguiMessageWindow_Fields
      {
         public static FieldInfo text = ClrTypes.AdvUguiMessageWindow?.GetField( "text", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance );
         public static FieldInfo nameText = ClrTypes.AdvUguiMessageWindow?.GetField( "nameText", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance );
         public static FieldInfo engine = ClrTypes.AdvUguiMessageWindow?.GetField( "engine", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance );
      }

      public static class AdvEngine_Properties
      {
         public static CachedProperty Page = ClrTypes.AdvEngine?.CachedProperty( "Page" );
      }

      public static class AdvPage_Methods
      {
         public static CachedMethod RemakeTextData = ClrTypes.AdvPage?.CachedMethod( "RemakeTextData" );
         public static CachedMethod RemakeText = ClrTypes.AdvPage?.CachedMethod( "RemakeText" );
         public static CachedMethod ChangeMessageWindowText = ClrTypes.AdvPage?.CachedMethod( "ChangeMessageWindowText", new Type[] { typeof( string ), typeof( string ), typeof( string ), typeof( string ) } );
      }

      public static class UILabel_Properties
      {
         public static CachedProperty MultiLine = ClrTypes.UILabel?.CachedProperty( "multiLine" );
         public static CachedProperty OverflowMethod = ClrTypes.UILabel?.CachedProperty( "overflowMethod" );
         public static CachedProperty SpacingX = ClrTypes.UILabel?.CachedProperty( "spacingX" );
         public static CachedProperty UseFloatSpacing = ClrTypes.UILabel?.CachedProperty( "useFloatSpacing" );
      }

      public static class Text_Properties
      {
         public static CachedProperty Font = ClrTypes.Text?.CachedProperty( "font" );
         public static CachedProperty FontSize = ClrTypes.Text?.CachedProperty( "fontSize" );

         public static CachedProperty HorizontalOverflow = ClrTypes.Text?.CachedProperty( "horizontalOverflow" );
         public static CachedProperty VerticalOverflow = ClrTypes.Text?.CachedProperty( "verticalOverflow" );
         public static CachedProperty LineSpacing = ClrTypes.Text?.CachedProperty( "lineSpacing" );
         public static CachedProperty ResizeTextForBestFit = ClrTypes.Text?.CachedProperty( "resizeTextForBestFit" );
         public static CachedProperty ResizeTextMinSize = ClrTypes.Text?.CachedProperty( "resizeTextMinSize" );
         public static CachedProperty ResizeTextMaxSize = ClrTypes.Text?.CachedProperty( "resizeTextMaxSize" );
      }

      public static class InputField_Properties
      {
         public static CachedProperty Placeholder = ClrTypes.InputField?.CachedProperty( "placeholder" );
      }

      public static class TMP_InputField_Properties
      {
         public static CachedProperty Placeholder = ClrTypes.TMP_InputField?.CachedProperty( "placeholder" );
      }

      public static class Font_Properties
      {
         public static CachedProperty FontSize = ClrTypes.Font?.CachedProperty( "fontSize" );
      }

      public static class AssetBundle_Methods
      {
         public static CachedMethod LoadAll = ClrTypes.AssetBundle?.CachedMethod( "LoadAll", typeof( Type ) );
         public static CachedMethod LoadAllAssets = ClrTypes.AssetBundle?.CachedMethod( "LoadAllAssets", typeof( Type ) );

         public static CachedMethod LoadFromFile = ClrTypes.AssetBundle?.CachedMethod( "LoadFromFile", typeof( string ) );
         public static CachedMethod CreateFromFile = ClrTypes.AssetBundle?.CachedMethod( "CreateFromFile", typeof( string ) );
      }

      public static class TextExpansion_Methods
      {
         public static CachedMethod SetMessageType = ClrTypes.TextExpansion?.CachedMethod( "SetMessageType" );
         public static CachedMethod SkipTypeWriter = ClrTypes.TextExpansion?.CachedMethod( "SkipTypeWriter" );
      }

      private static Type FindType( string name )
      {
         var assemblies = AppDomain.CurrentDomain.GetAssemblies();
         foreach( var assembly in assemblies )
         {
            try
            {
               var type = assembly.GetType( name, false );
               if( type != null )
               {
                  return type;
               }
            }
            catch
            {
               // don't care!
            }
         }

         return null;
      }

      private static Type FindTypeStrict( string name )
      {
         return Type.GetType( name, false );
      }
   }
}
