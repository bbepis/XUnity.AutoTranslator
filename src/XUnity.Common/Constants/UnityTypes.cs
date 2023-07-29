#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

#if IL2CPP
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
#endif

namespace XUnity.Common.Constants
{
   public static class UnityTypes
   {
#if IL2CPP
      private static bool _initialized;
      private static readonly HashSet<string> Blacklist = new HashSet<string>( StringComparer.OrdinalIgnoreCase )
      {
         "netstandard.dll"
      };

      private static void Initialize()
      {
         // we need to force load ALL assemblies because we do not know which ones are relevant and which ones are not!!
         // (types we are hooking may be stored in assemblies whose names are not known at compile time)
         if( !_initialized )
         {
            _initialized = true;

            try
            {
               XuaLogger.AutoTranslator.Info( "Force loading ALL proxy assemblies." );

               var dir = string.IsNullOrEmpty( Il2CppProxyAssemblies.Location ) ? new FileInfo( typeof( UnityTypes ).Assembly.Location ).Directory : new DirectoryInfo( Il2CppProxyAssemblies.Location );
               var files = dir.GetFiles();
               foreach( var file in files )
               {
                  if( Blacklist.Contains( file.Name ) )
                     continue;

                  if( file.Extension.Equals( ".dll", StringComparison.OrdinalIgnoreCase ) )
                  {
                     try
                     {
                        XuaLogger.AutoTranslator.Debug( "Force loading assembly: " + file.FullName );

                        var assemblyName = AssemblyName.GetAssemblyName( file.FullName );
                        var assembly = Assembly.Load( assemblyName );
                     }
                     catch( Exception e )
                     {
                        XuaLogger.AutoTranslator.Error( e, "An error occurred while force loading assembly: " + file.FullName );
                     }
                  }
               }
            }
            catch( Exception e )
            {
               XuaLogger.AutoTranslator.Error( e, "An error occurred while attempting to locate proxy assemblies." );
            }
         }
      }
#endif

      // NGUI
      public static readonly TypeContainer UILabel = FindType( "UILabel" );
      public static readonly TypeContainer UIWidget = FindType( "UIWidget" );
      public static readonly TypeContainer UIAtlas = FindType( "UIAtlas" );
      public static readonly TypeContainer UISprite = FindType( "UISprite" );
      public static readonly TypeContainer UITexture = FindType( "UITexture" );
      public static readonly TypeContainer UI2DSprite = FindType( "UI2DSprite" );
      public static readonly TypeContainer UIFont = FindType( "UIFont" );
      public static readonly TypeContainer UIPanel = FindType( "UIPanel" );
      public static readonly TypeContainer UIRect = FindType( "UIRect" );
      public static readonly TypeContainer UIInput = FindType( "UIInput" );

      // FairyGUI
      public static readonly TypeContainer TextField = FindType( "FairyGUI.TextField" );

      // TextMeshPro
      public static readonly TypeContainer TMP_InputField = FindType( "TMPro.TMP_InputField" );
      public static readonly TypeContainer TMP_Text = FindType( "TMPro.TMP_Text" );
      public static readonly TypeContainer TextMeshProUGUI = FindType( "TMPro.TextMeshProUGUI" );
      public static readonly TypeContainer TextMeshPro = FindType( "TMPro.TextMeshPro" );
      public static readonly TypeContainer TMP_FontAsset = FindType( "TMPro.TMP_FontAsset" );
      public static readonly TypeContainer TMP_Settings = FindType( "TMPro.TMP_Settings" );

      // Unity
      public static readonly TypeContainer GameObject = FindType( "UnityEngine.GameObject" );
      public static readonly TypeContainer Transform = FindType( "UnityEngine.Transform" );
      public static readonly TypeContainer TextMesh = FindType( "UnityEngine.TextMesh" );
      public static readonly TypeContainer Text = FindType( "UnityEngine.UI.Text" );
      public static readonly TypeContainer Image = FindType( "UnityEngine.UI.Image" );
      public static readonly TypeContainer RawImage = FindType( "UnityEngine.UI.RawImage" );
      public static readonly TypeContainer MaskableGraphic = FindType( "UnityEngine.UI.MaskableGraphic" );
      public static readonly TypeContainer Graphic = FindType( "UnityEngine.UI.Graphic" );
      public static readonly TypeContainer GUIContent = FindType( "UnityEngine.GUIContent" );
      public static readonly TypeContainer WWW = FindType( "UnityEngine.WWW" );
      public static readonly TypeContainer InputField = FindType( "UnityEngine.UI.InputField" );
      public static readonly TypeContainer GUI = FindType( "UnityEngine.GUI" );
      public static readonly TypeContainer GUI_ToolbarButtonSize = FindType( "UnityEngine.GUI+ToolbarButtonSize" );
      public static readonly TypeContainer GUIStyle = FindType( "UnityEngine.GUIStyle" );
      public static readonly TypeContainer ImageConversion = FindType( "UnityEngine.ImageConversion" );
      public static readonly TypeContainer Texture2D = FindType( "UnityEngine.Texture2D" );
      public static readonly TypeContainer Texture = FindType( "UnityEngine.Texture" );
      public static readonly TypeContainer SpriteRenderer = FindType( "UnityEngine.SpriteRenderer" );
      public static readonly TypeContainer Sprite = FindType( "UnityEngine.Sprite" );
      public static readonly TypeContainer Object = FindType( "UnityEngine.Object" );
      public static readonly TypeContainer TextEditor = FindType( "UnityEngine.TextEditor" );
      public static readonly TypeContainer CustomYieldInstruction = FindType( "UnityEngine.CustomYieldInstruction" );
      public static readonly TypeContainer SceneManager = FindType( "UnityEngine.SceneManagement.SceneManager" );
      public static readonly TypeContainer Scene = FindType( "UnityEngine.SceneManagement.Scene" );
      public static readonly TypeContainer UnityEventBase = FindType( "UnityEngine.Events.UnityEventBase" );
      public static readonly TypeContainer BaseInvokableCall = FindType( "UnityEngine.Events.BaseInvokableCall" );
      public static readonly TypeContainer Font = FindType( "UnityEngine.Font" );
      public static readonly TypeContainer WaitForSecondsRealtime = FindType( "UnityEngine.WaitForSecondsRealtime" );
      public static readonly TypeContainer Input = FindType( "UnityEngine.Input" );
      public static readonly TypeContainer AssetBundleCreateRequest = FindType( "UnityEngine.AssetBundleCreateRequest" );
      public static readonly TypeContainer AssetBundle = FindType( "UnityEngine.AssetBundle" );
      public static readonly TypeContainer AssetBundleRequest = FindType( "UnityEngine.AssetBundleRequest" );
      public static readonly TypeContainer Resources = FindType( "UnityEngine.Resources" );
      public static readonly TypeContainer AsyncOperation = FindType( "UnityEngine.AsyncOperation" );
      public static readonly TypeContainer TextAsset = FindType( "UnityEngine.TextAsset" );
      public static readonly Type HorizontalWrapMode = FindClrType( "UnityEngine.HorizontalWrapMode" );
      public static readonly Type TextOverflowModes = FindClrType( "TMPro.TextOverflowModes" );
      public static readonly Type TextAlignmentOptions = FindClrType( "TMPro.TextAlignmentOptions" );
      public static readonly Type VerticalWrapMode = FindClrType( "UnityEngine.VerticalWrapMode" );

      // Shimeji Engine
      public static readonly TypeContainer TextExpansion = FindType( "UnityEngine.UI.TextExpansion" );

      // Something...
      public static readonly TypeContainer Typewriter = FindType( "Typewriter" );

      // Utage
      public static readonly TypeContainer UguiNovelText = FindType( "Utage.UguiNovelText" );
      public static readonly TypeContainer UguiNovelTextGenerator = FindType( "Utage.UguiNovelTextGenerator" );
      public static readonly TypeContainer AdvEngine = FindType( "Utage.AdvEngine" );
      public static readonly TypeContainer AdvPage = FindType( "Utage.AdvPage" );
      public static readonly TypeContainer TextData = FindType( "Utage.TextData" );
      public static readonly TypeContainer AdvUguiMessageWindow = FindType( "Utage.AdvUguiMessageWindow" ) ?? FindType( "AdvUguiMessageWindow" );
      public static readonly TypeContainer AdvUiMessageWindow = FindType( "AdvUiMessageWindow" );
      public static readonly TypeContainer AdvDataManager = FindType( "Utage.AdvDataManager" );
      public static readonly TypeContainer AdvScenarioData = FindType( "Utage.AdvScenarioData" );
      public static readonly TypeContainer AdvScenarioLabelData = FindType( "Utage.AdvScenarioLabelData" );
      public static readonly TypeContainer DicingTextures = FindType( "Utage.DicingTextures" );
      public static readonly TypeContainer DicingImage = FindType( "Utage.DicingImage" );
      public static readonly TypeContainer TextArea2D = FindType( "Utage.TextArea2D" );

      // Live2D
      public static readonly TypeContainer CubismRenderer = FindType( "Live2D.Cubism.Rendering.CubismRenderer" );

      // Assets.System (what engine is this?)
      public static readonly TypeContainer TextWindow = FindType( "Assets.System.Text.TextWindow" );

      public static class TMP_Settings_Properties
      {
         public static CachedProperty Version = UnityTypes.TMP_Settings?.ClrType.CachedProperty( "version" );
         public static CachedProperty FallbackFontAssets = UnityTypes.TMP_Settings?.ClrType.CachedProperty( "fallbackFontAssets" );
      }

      public static class TMP_FontAsset_Properties
      {
         public static CachedProperty Version = UnityTypes.TMP_FontAsset?.ClrType.CachedProperty( "version" );
      }

      public static class AdvScenarioData_Properties
      {
         public static CachedProperty ScenarioLabels = UnityTypes.AdvScenarioData?.ClrType.CachedProperty( "ScenarioLabels" );
      }

      public static class UguiNovelText_Properties
      {
         public static CachedProperty TextGenerator = UnityTypes.UguiNovelText?.ClrType.CachedProperty( "TextGenerator" );
      }

      public static class UguiNovelText_Methods
      {
         public static CachedMethod SetAllDirty = UnityTypes.UguiNovelText?.ClrType.CachedMethod( "SetAllDirty" );
      }

      public static class UguiNovelTextGenerator_Methods
      {
         public static CachedMethod Refresh = UnityTypes.UguiNovelTextGenerator?.ClrType.CachedMethod( "Refresh" );
      }

      public static class AdvUguiMessageWindow_Properties
      {
         public static CachedProperty Text = UnityTypes.AdvUguiMessageWindow?.ClrType.CachedProperty( "Text" );
         public static CachedProperty Engine = UnityTypes.AdvUguiMessageWindow?.ClrType.CachedProperty( "Engine" );
      }

      public static class AdvUiMessageWindow_Fields
      {
         public static CachedField text = UnityTypes.AdvUiMessageWindow?.ClrType.CachedField( "text" );
         public static CachedField nameText = UnityTypes.AdvUiMessageWindow?.ClrType.CachedField( "nameText" );
      }

      public static class AdvUguiMessageWindow_Fields
      {
         public static FieldInfo text = UnityTypes.AdvUguiMessageWindow?.ClrType.GetField( "text", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance );
         public static FieldInfo nameText = UnityTypes.AdvUguiMessageWindow?.ClrType.GetField( "nameText", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance );
         public static FieldInfo engine = UnityTypes.AdvUguiMessageWindow?.ClrType.GetField( "engine", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance );
      }

      public static class AdvEngine_Properties
      {
         public static CachedProperty Page = UnityTypes.AdvEngine?.ClrType.CachedProperty( "Page" );
      }

      public static class AdvPage_Methods
      {
         public static CachedMethod RemakeTextData = UnityTypes.AdvPage?.ClrType.CachedMethod( "RemakeTextData" );
         public static CachedMethod RemakeText = UnityTypes.AdvPage?.ClrType.CachedMethod( "RemakeText" );
         public static CachedMethod ChangeMessageWindowText = UnityTypes.AdvPage?.ClrType.CachedMethod( "ChangeMessageWindowText", new Type[] { typeof( string ), typeof( string ), typeof( string ), typeof( string ) } );
      }

      public static class UILabel_Properties
      {
         public static CachedProperty MultiLine = UnityTypes.UILabel?.ClrType.CachedProperty( "multiLine" );
         public static CachedProperty OverflowMethod = UnityTypes.UILabel?.ClrType.CachedProperty( "overflowMethod" );
         public static CachedProperty SpacingX = UnityTypes.UILabel?.ClrType.CachedProperty( "spacingX" );
         public static CachedProperty UseFloatSpacing = UnityTypes.UILabel?.ClrType.CachedProperty( "useFloatSpacing" );
      }

      public static class Text_Properties
      {
         public static CachedProperty Font = UnityTypes.Text?.ClrType.CachedProperty( "font" );
         public static CachedProperty FontSize = UnityTypes.Text?.ClrType.CachedProperty( "fontSize" );

         public static CachedProperty HorizontalOverflow = UnityTypes.Text?.ClrType.CachedProperty( "horizontalOverflow" );
         public static CachedProperty VerticalOverflow = UnityTypes.Text?.ClrType.CachedProperty( "verticalOverflow" );
         public static CachedProperty LineSpacing = UnityTypes.Text?.ClrType.CachedProperty( "lineSpacing" );
         public static CachedProperty ResizeTextForBestFit = UnityTypes.Text?.ClrType.CachedProperty( "resizeTextForBestFit" );
         public static CachedProperty ResizeTextMinSize = UnityTypes.Text?.ClrType.CachedProperty( "resizeTextMinSize" );
         public static CachedProperty ResizeTextMaxSize = UnityTypes.Text?.ClrType.CachedProperty( "resizeTextMaxSize" );
      }

      public static class InputField_Properties
      {
         public static CachedProperty Placeholder = UnityTypes.InputField?.ClrType.CachedProperty( "placeholder" );
      }

      public static class TMP_InputField_Properties
      {
         public static CachedProperty Placeholder = UnityTypes.TMP_InputField?.ClrType.CachedProperty( "placeholder" );
      }

      public static class Font_Properties
      {
         public static CachedProperty FontSize = UnityTypes.Font?.ClrType.CachedProperty( "fontSize" );
      }

      public static class AssetBundle_Methods
      {
#if MANAGED
         public static CachedMethod LoadAll = UnityTypes.AssetBundle?.ClrType.CachedMethod( "LoadAll", typeof( Type ) );
         public static CachedMethod LoadAllAssets = UnityTypes.AssetBundle?.ClrType.CachedMethod( "LoadAllAssets", typeof( Type ) );
#else
         public static CachedMethod LoadAll = UnityTypes.AssetBundle?.ClrType.CachedMethod( "LoadAll", typeof( Il2CppSystem.Type ) );
         public static CachedMethod LoadAllAssets = UnityTypes.AssetBundle?.ClrType.CachedMethod( "LoadAllAssets", typeof( Il2CppSystem.Type ) );
#endif

         public static CachedMethod LoadFromFile = UnityTypes.AssetBundle?.ClrType.CachedMethod( "LoadFromFile", typeof( string ) );
         public static CachedMethod CreateFromFile = UnityTypes.AssetBundle?.ClrType.CachedMethod( "CreateFromFile", typeof( string ) );
      }

      public static class TextExpansion_Methods
      {
         public static CachedMethod SetMessageType = UnityTypes.TextExpansion?.ClrType.CachedMethod( "SetMessageType" );
         public static CachedMethod SkipTypeWriter = UnityTypes.TextExpansion?.ClrType.CachedMethod( "SkipTypeWriter" );
      }

      public static class GameObject_Methods
      {
#if IL2CPP
         public static class IL2CPP
         {
            public static readonly IntPtr SetActive = Il2CppUtilities.GetIl2CppMethod( UnityTypes.GameObject?.ClassPointer, "SetActive", typeof( void ), typeof( bool ) );
         }
#endif
      }

      public static class TextMesh_Methods
      {
#if IL2CPP
         public static class IL2CPP
         {
            public static readonly IntPtr set_text = Il2CppUtilities.GetIl2CppMethod( UnityTypes.TextMesh?.ClassPointer, "set_text", typeof( void ), typeof( string ) );
            public static readonly IntPtr get_text = Il2CppUtilities.GetIl2CppMethod( UnityTypes.TextMesh?.ClassPointer, "get_text", typeof( string ) );
         }
#endif
      }

      public static class Text_Methods
      {
#if IL2CPP
         public static class IL2CPP
         {
            public static readonly IntPtr set_text = Il2CppUtilities.GetIl2CppMethod( UnityTypes.Text?.ClassPointer, "set_text", typeof( void ), typeof( string ) );
            public static readonly IntPtr get_text = Il2CppUtilities.GetIl2CppMethod( UnityTypes.Text?.ClassPointer, "get_text", typeof( string ) );
            public static readonly IntPtr get_supportRichText = Il2CppUtilities.GetIl2CppMethod( UnityTypes.Text?.ClassPointer, "get_supportRichText", typeof( bool ) );
            public static readonly IntPtr OnEnable = Il2CppUtilities.GetIl2CppMethod( UnityTypes.Text?.ClassPointer, "OnEnable", typeof( void ) );
         }
#endif
      }

      public static class InputField_Methods
      {
#if IL2CPP
         public static class IL2CPP
         {
            public static readonly IntPtr get_placeholder = Il2CppUtilities.GetIl2CppMethod( UnityTypes.InputField?.ClassPointer, "get_placeholder", "UnityEngine.UI.Graphic" );
         }
#endif
      }

      public static class TMP_Text_Methods
      {
#if IL2CPP
         public static class IL2CPP
         {
            public static readonly IntPtr set_text = Il2CppUtilities.GetIl2CppMethod( UnityTypes.TMP_Text?.ClassPointer, "set_text", typeof( void ), typeof( string ) );
            public static readonly IntPtr get_text = Il2CppUtilities.GetIl2CppMethod( UnityTypes.TMP_Text?.ClassPointer, "get_text", typeof( string ) );
            public static readonly IntPtr get_richText = Il2CppUtilities.GetIl2CppMethod( UnityTypes.TMP_Text?.ClassPointer, "get_richText", typeof( bool ) );
         }
#endif
      }

      public static class TMP_InputField_Methods
      {
#if IL2CPP
         public static class IL2CPP
         {
            public static readonly IntPtr get_placeholder = Il2CppUtilities.GetIl2CppMethod( UnityTypes.TMP_InputField?.ClassPointer, "get_placeholder", "UnityEngine.UI.Graphic" );
         }
#endif
      }

      public static class TextMeshPro_Methods
      {
#if IL2CPP
         public static class IL2CPP
         {
            public static readonly IntPtr OnEnable = Il2CppUtilities.GetIl2CppMethod( UnityTypes.TextMeshPro?.ClassPointer, "OnEnable", typeof( void ) );
         }
#endif
      }

      public static class TextMeshProUGUI_Methods
      {
#if IL2CPP
         public static class IL2CPP
         {
            public static readonly IntPtr OnEnable = Il2CppUtilities.GetIl2CppMethod( UnityTypes.TextMeshProUGUI?.ClassPointer, "OnEnable", typeof( void ) );
         }
#endif
      }

      public static class UILabel_Methods
      {
#if IL2CPP
         public static class IL2CPP
         {
            public static readonly IntPtr set_text = Il2CppUtilities.GetIl2CppMethod( UnityTypes.UILabel?.ClassPointer, "set_text", typeof( void ), typeof( string ) );
            public static readonly IntPtr get_text = Il2CppUtilities.GetIl2CppMethod( UnityTypes.UILabel?.ClassPointer, "get_text", typeof( string ) );
         }
#endif
      }

      public static class UIRect_Methods
      {
#if IL2CPP
         public static class IL2CPP
         {
            public static readonly IntPtr OnEnable = Il2CppUtilities.GetIl2CppMethod( UnityTypes.UIRect?.ClassPointer, "OnEnable", typeof( void ) );
         }
#endif
      }

      public static class SceneManager_Methods
      {
         public static readonly Action<UnityAction<Scene, LoadSceneMode>> add_sceneLoaded =
            (Action<UnityAction<Scene, LoadSceneMode>>)ExpressionHelper.CreateTypedFastInvokeUnchecked(
               typeof( SceneManager ).GetMethod(
                  "add_sceneLoaded",
                  BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic,
                  null,
                  new Type[] { typeof( UnityAction<Scene, LoadSceneMode> ) },
                  null
               ) );
      }

      public static class Texture2D_Methods
      {
#if IL2CPP
         public static readonly Func<Texture2D, Il2CppStructArray<byte>, bool> LoadImage =
            (Func<Texture2D, Il2CppStructArray<byte>, bool>)ExpressionHelper.CreateTypedFastInvokeUnchecked(
               typeof( Texture2D ).GetMethod(
                  "LoadImage",
                  BindingFlags.Public | BindingFlags.Instance,
                  null,
                  new Type[] { typeof( Il2CppStructArray<byte> ) },
                  null
               ) );

         public static readonly Func<Texture2D, Il2CppStructArray<byte>> EncodeToPNG =
            (Func<Texture2D, Il2CppStructArray<byte>>)ExpressionHelper.CreateTypedFastInvokeUnchecked(
               typeof( Texture2D ).GetMethod(
                  "EncodeToPNG",
                  BindingFlags.Public | BindingFlags.Instance,
                  null,
                  new Type[] { },
                  null
               ) );
#else
         public static readonly Func<Texture2D, byte[], bool> LoadImage =
            (Func<Texture2D, byte[], bool>)ExpressionHelper.CreateTypedFastInvokeUnchecked(
               typeof( Texture2D ).GetMethod(
                  "LoadImage",
                  BindingFlags.Public | BindingFlags.Instance,
                  null,
                  new Type[] { typeof( byte[] ) },
                  null
               ) );

         public static readonly Func<Texture2D, byte[]> EncodeToPNG =
            (Func<Texture2D, byte[]>)ExpressionHelper.CreateTypedFastInvokeUnchecked(
               typeof( Texture2D ).GetMethod(
                  "EncodeToPNG",
                  BindingFlags.Public | BindingFlags.Instance,
                  null,
                  new Type[] { },
                  null
               ) );
#endif
      }

      public static class ImageConversion_Methods
      {
#if IL2CPP
         public static readonly Func<Texture2D, Il2CppStructArray<byte>, bool, bool> LoadImage =
            (Func<Texture2D, Il2CppStructArray<byte>, bool, bool>)ExpressionHelper.CreateTypedFastInvokeUnchecked(
               UnityTypes.ImageConversion?.ClrType.GetMethod(
                  "LoadImage",
                  BindingFlags.Public | BindingFlags.Static,
                  null,
                  new Type[] { typeof( Texture2D ), typeof( Il2CppStructArray<byte> ), typeof( bool ) },
                  null
               ) );

         public static readonly Func<Texture2D, Il2CppStructArray<byte>> EncodeToPNG =
            (Func<Texture2D, Il2CppStructArray<byte>>)ExpressionHelper.CreateTypedFastInvokeUnchecked(
               UnityTypes.ImageConversion?.ClrType.GetMethod(
                  "EncodeToPNG",
                  BindingFlags.Public | BindingFlags.Static,
                  null,
                  new Type[] { typeof( Texture2D ) },
                  null
               ) );
#else
         public static readonly Func<Texture2D, byte[], bool, bool> LoadImage =
            (Func<Texture2D, byte[], bool, bool>)ExpressionHelper.CreateTypedFastInvokeUnchecked(
               UnityTypes.ImageConversion?.ClrType.GetMethod(
                  "LoadImage",
                  BindingFlags.Public | BindingFlags.Static,
                  null,
                  new Type[] { typeof( Texture2D ), typeof( byte[] ), typeof( bool ) },
                  null
               ) );

         public static readonly Func<Texture2D, byte[]> EncodeToPNG =
            (Func<Texture2D, byte[]>)ExpressionHelper.CreateTypedFastInvokeUnchecked(
               UnityTypes.ImageConversion?.ClrType.GetMethod(
                  "EncodeToPNG",
                  BindingFlags.Public | BindingFlags.Static,
                  null,
                  new Type[] { typeof( Texture2D ) },
                  null
               ) );
#endif
      }

      private static Type FindClrType( string name )
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

#if IL2CPP
      private static TypeContainer FindType( string name )
      {
         Initialize();

         try
         {
            string @namespace = string.Empty;
            string typeName = null;

            var lastDot = name.LastIndexOf( "." );
            if( lastDot == -1 )
            {
               typeName = name;
            }
            else
            {
               @namespace = name.Substring( 0, lastDot );
               typeName = name.Substring( lastDot + 1 );
            }

            var ptr = Il2CppUtilities.GetIl2CppClass( @namespace, typeName );

            Type wrapperType = null;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach( var assembly in assemblies )
            {
               try
               {
                  var type = assembly.GetType( name, false );
                  if( type != null )
                  {
                     wrapperType = type;
                  }
               }
               catch
               {
                  // don't care!
               }
            }

            if( wrapperType != null && ptr == IntPtr.Zero )
            {
               XuaLogger.AutoTranslator.Warn( "Could not find '" + name + "' in IL2CPP domain even though it could be found in the managed domain." );
            }

            if( wrapperType != null )
            {
               return new TypeContainer( ptr != IntPtr.Zero ? Il2CppType.TypeFromPointer( ptr ) : null, wrapperType, ptr );
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Warn( e, "An error occurred while resolving type: " + name );
         }

         return null;
      }
#else
      private static TypeContainer FindType( string name )
      {
         var assemblies = AppDomain.CurrentDomain.GetAssemblies();
         foreach( var assembly in assemblies )
         {
            try
            {
               var type = assembly.GetType( name, false );
               if(type != null)
               {
                  return new TypeContainer( type );
               }
            }
            catch
            {
               // don't care!
            }
         }

         return null;
      }
#endif
   }
}
