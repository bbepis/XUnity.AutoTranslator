using Harmony;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.Common.Constants
{
#pragma warning disable CS1591 // Really could not care less..

   public static class UnityTypes
   {
      private static bool _initialized;
      private static readonly HashSet<string> Blacklist = new HashSet<string>( StringComparer.OrdinalIgnoreCase )
      {
         "netstandard.dll"
      };

      private static void Initialize()
      {
         

         // we need to force load ALL assemblies because we do not know which ones are relevant and which ones are not!!
         // (types we are hooking may be stored in assemblies whose names are not known at runtime)
         if( !_initialized )
         {
            _initialized = true;

            try
            {
               XuaLogger.AutoTranslator.Info( "Force loading ALL proxy assemblies." );

               var dir = new FileInfo( typeof( UnityTypes ).Assembly.Location ).Directory;
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


      // TextMeshPro
      public static readonly Il2CppTypeWrapper TMP_InputField = FindType( "TMPro.TMP_InputField" );
      public static readonly Il2CppTypeWrapper TMP_Text = FindType( "TMPro.TMP_Text" );
      public static readonly Il2CppTypeWrapper TextMeshProUGUI = FindType( "TMPro.TextMeshProUGUI" );
      public static readonly Il2CppTypeWrapper TextMeshPro = FindType( "TMPro.TextMeshPro" );
      public static readonly Il2CppTypeWrapper FontAsset = FindType( "TMPro.TMP_FontAsset" );
      public static readonly Il2CppTypeWrapper AssetBundle = FindType( "UnityEngine.AssetBundle" );

      // Unity
      public static readonly Il2CppTypeWrapper TextMesh = FindType( "UnityEngine.TextMesh" );
      public static readonly Il2CppTypeWrapper Text = FindType( "UnityEngine.UI.Text" );
      public static readonly Il2CppTypeWrapper Image = FindType( "UnityEngine.UI.Image" );
      public static readonly Il2CppTypeWrapper RawImage = FindType( "UnityEngine.UI.RawImage" );
      public static readonly Il2CppTypeWrapper MaskableGraphic = FindType( "UnityEngine.UI.MaskableGraphic" );
      public static readonly Il2CppTypeWrapper Graphic = FindType( "UnityEngine.UI.Graphic" );
      public static readonly Il2CppTypeWrapper GUIContent = FindType( "UnityEngine.GUIContent" );
      public static readonly Il2CppTypeWrapper WWW = FindType( "UnityEngine.WWW" );
      public static readonly Il2CppTypeWrapper InputField = FindType( "UnityEngine.UI.InputField" );
      public static readonly Il2CppTypeWrapper GUI = FindType( "UnityEngine.GUI" );
      public static readonly Il2CppTypeWrapper ImageConversion = FindType( "UnityEngine.ImageConversion" );
      public static readonly Il2CppTypeWrapper Texture = FindType( "UnityEngine.Texture" );
      public static readonly Il2CppTypeWrapper SpriteRenderer = FindType( "UnityEngine.SpriteRenderer" );
      public static readonly Il2CppTypeWrapper Sprite = FindType( "UnityEngine.Sprite" );
      public static readonly Il2CppTypeWrapper Object = FindType( "UnityEngine.Object" );
      public static readonly Il2CppTypeWrapper TextEditor = FindType( "UnityEngine.TextEditor" );
      public static readonly Il2CppTypeWrapper CustomYieldInstruction = FindType( "UnityEngine.CustomYieldInstruction" );
      public static readonly Il2CppTypeWrapper SceneManager = FindType( "UnityEngine.SceneManagement.SceneManager" );
      public static readonly Il2CppTypeWrapper Scene = FindType( "UnityEngine.SceneManagement.Scene" );
      public static readonly Il2CppTypeWrapper UnityEventBase = FindType( "UnityEngine.Events.UnityEventBase" );
      public static readonly Il2CppTypeWrapper BaseInvokableCall = FindType( "UnityEngine.Events.BaseInvokableCall" );
      public static readonly Il2CppTypeWrapper HorizontalWrapMode = FindType( "UnityEngine.HorizontalWrapMode" );
      public static readonly Il2CppTypeWrapper VerticalWrapMode = FindType( "UnityEngine.VerticalWrapMode" );
      public static readonly Il2CppTypeWrapper Font = FindType( "UnityEngine.Font" );
      public static readonly Il2CppTypeWrapper WaitForSecondsRealtime = FindType( "UnityEngine.WaitForSecondsRealtime" );
      public static readonly Il2CppTypeWrapper Input = FindType( "UnityEngine.Input" );

      //public static class AdvUguiMessageWindow_Properties
      //{
      //   public static CachedProperty Text = UnityTypes.AdvUguiMessageWindow?.CachedProperty( "Text" );
      //   public static CachedProperty Engine = UnityTypes.AdvUguiMessageWindow?.CachedProperty( "Engine" );
      //}

      //public static class AdvUguiMessageWindow_Fields
      //{
      //   public static FieldInfo text = UnityTypes.AdvUguiMessageWindow?.GetField( "text", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance );
      //   public static FieldInfo nameText = UnityTypes.AdvUguiMessageWindow?.GetField( "nameText", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance );
      //   public static FieldInfo engine = UnityTypes.AdvUguiMessageWindow?.GetField( "engine", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance );
      //}

      //public static class AdvEngine_Properties
      //{
      //   public static CachedProperty Page = UnityTypes.AdvEngine?.CachedProperty( "Page" );
      //}

      //public static class AdvPage_Methods
      //{
      //   public static CachedMethod RemakeTextData = UnityTypes.AdvPage?.CachedMethod( "RemakeTextData" );
      //   public static CachedMethod RemakeText = UnityTypes.AdvPage?.CachedMethod( "RemakeText" );
      //   public static CachedMethod ChangeMessageWindowText = UnityTypes.AdvPage?.CachedMethod( "ChangeMessageWindowText", new Type[] { typeof( string ), typeof( string ), typeof( string ), typeof( string ) } );
      //}

      //public static class UILabel_Properties
      //{
      //   public static CachedProperty MultiLine = UnityTypes.UILabel?.CachedProperty( "multiLine" );
      //   public static CachedProperty OverflowMethod = UnityTypes.UILabel?.CachedProperty( "overflowMethod" );
      //   public static CachedProperty SpacingX = UnityTypes.UILabel?.CachedProperty( "spacingX" );
      //   public static CachedProperty UseFloatSpacing = UnityTypes.UILabel?.CachedProperty( "useFloatSpacing" );
      //}

      //public static class Text_Properties
      //{
      //   public static CachedProperty Font = UnityTypes.Text?.CachedProperty( "font" );
      //   public static CachedProperty FontSize = UnityTypes.Text?.CachedProperty( "fontSize" );

      //   public static CachedProperty HorizontalOverflow = UnityTypes.Text?.CachedProperty( "horizontalOverflow" );
      //   public static CachedProperty VerticalOverflow = UnityTypes.Text?.CachedProperty( "verticalOverflow" );
      //   public static CachedProperty LineSpacing = UnityTypes.Text?.CachedProperty( "lineSpacing" );
      //   public static CachedProperty ResizeTextForBestFit = UnityTypes.Text?.CachedProperty( "resizeTextForBestFit" );
      //   public static CachedProperty ResizeTextMinSize = UnityTypes.Text?.CachedProperty( "resizeTextMinSize" );
      //}

      //public static class InputField_Properties
      //{
      //   public static CachedProperty Placeholder = UnityTypes.InputField?.CachedProperty( "placeholder" );
      //}

      //public static class TMP_InputField_Properties
      //{
      //   public static CachedProperty Placeholder = UnityTypes.TMP_InputField?.CachedProperty( "placeholder" );
      //}

      //public static class Font_Properties
      //{
      //   public static CachedProperty FontSize = UnityTypes.Font?.CachedProperty( "fontSize" );
      //}

      //public static class AssetBundle_Methods
      //{
      //   public static CachedMethod LoadAll = UnityTypes.AssetBundle?.CachedMethod( "LoadAll", typeof( Type ) );
      //   public static CachedMethod LoadAllAssets = UnityTypes.AssetBundle?.CachedMethod( "LoadAllAssets", typeof( Type ) );

      //   public static CachedMethod LoadFromFile = UnityTypes.AssetBundle?.CachedMethod( "LoadFromFile", typeof( string ) );
      //   public static CachedMethod CreateFromFile = UnityTypes.AssetBundle?.CachedMethod( "CreateFromFile", typeof( string ) );
      //}

      //public static class TextExpansion_Methods
      //{
      //   public static CachedMethod SetMessageType = UnityTypes.TextExpansion?.CachedMethod( "SetMessageType" );
      //   public static CachedMethod SkipTypeWriter = UnityTypes.TextExpansion?.CachedMethod( "SkipTypeWriter" );
      //}

      private static Il2CppTypeWrapper FindType( string name )
      {
         Initialize();

         try
         {
            string @namespace = null;
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
            if( ptr == System.IntPtr.Zero ) return null;

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

            return new Il2CppTypeWrapper( Il2CppType.TypeFromPointer( ptr ), wrapperType, ptr );
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Warn( e, "An error occurred while resolving type: " + name );

            return null;
         }
      }
   }
}
