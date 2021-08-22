﻿#if IL2CPP
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.Common.Constants
{
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

      // NGUI
      public static readonly Il2CppTypeWrapper UILabel = FindType( "UILabel" );
      public static readonly Il2CppTypeWrapper UIInput = FindType( "UIInput" );
      public static readonly Il2CppTypeWrapper UIRect = FindType( "UIRect" );

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
      public static readonly Il2CppTypeWrapper Texture2D = FindType( "UnityEngine.Texture2D" );
      public static readonly Il2CppTypeWrapper Texture = FindType( "UnityEngine.Texture" );
      public static readonly Il2CppTypeWrapper SpriteRenderer = FindType( "UnityEngine.SpriteRenderer" );
      public static readonly Il2CppTypeWrapper Sprite = FindType( "UnityEngine.Sprite" );
      public static readonly Il2CppTypeWrapper Object = FindType( "UnityEngine.Object" );
      public static readonly Il2CppTypeWrapper GameObject = FindType( "UnityEngine.GameObject" );
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

      public static class GameObject_Methods
      {
         public static readonly IntPtr SetActive = Il2CppUtilities.GetIl2CppMethod( UnityTypes.GameObject?.ClassPointer, "SetActive", typeof( void ), typeof( bool ) );

         public static readonly Func<GameObject, Il2CppSystem.Type, Component> GetComponent =
            (Func<GameObject, Il2CppSystem.Type, Component>)ExpressionHelper.CreateTypedFastInvokeUnchecked(
               typeof( GameObject ).GetMethod(
                  "GetComponent",
                  BindingFlags.Public | BindingFlags.Instance,
                  null,
                  new Type[] { typeof( Il2CppSystem.Type ) },
                  null
               ) );
      }

      public static class TextMesh_Methods
      {
         public static readonly IntPtr set_text = Il2CppUtilities.GetIl2CppMethod( UnityTypes.TextMesh?.ClassPointer, "set_text", typeof( void ), typeof( string ) );
         public static readonly IntPtr get_text = Il2CppUtilities.GetIl2CppMethod( UnityTypes.TextMesh?.ClassPointer, "get_text", typeof( string ) );
         public static readonly IntPtr get_richText = Il2CppUtilities.GetIl2CppMethod( UnityTypes.TextMesh?.ClassPointer, "get_richText", typeof( bool ) );
      }

      public static class Text_Methods
      {
         public static readonly IntPtr set_text = Il2CppUtilities.GetIl2CppMethod( UnityTypes.Text?.ClassPointer, "set_text", typeof( void ), typeof( string ) );
         public static readonly IntPtr get_text = Il2CppUtilities.GetIl2CppMethod( UnityTypes.Text?.ClassPointer, "get_text", typeof( string ) );
         public static readonly IntPtr get_supportRichText = Il2CppUtilities.GetIl2CppMethod( UnityTypes.Text?.ClassPointer, "get_supportRichText", typeof( bool ) );
         public static readonly IntPtr OnEnable = Il2CppUtilities.GetIl2CppMethod( UnityTypes.Text?.ClassPointer, "OnEnable", typeof( void ) );
      }

      public static class InputField_Methods
      {
         public static readonly IntPtr get_placeholder = Il2CppUtilities.GetIl2CppMethod( UnityTypes.InputField?.ClassPointer, "get_placeholder", "UnityEngine.UI.Graphic" );
      }

      public static class TMP_Text_Methods
      {
         public static readonly IntPtr set_text = Il2CppUtilities.GetIl2CppMethod( UnityTypes.TMP_Text?.ClassPointer, "set_text", typeof( void ), typeof( string ) );
         public static readonly IntPtr get_text = Il2CppUtilities.GetIl2CppMethod( UnityTypes.TMP_Text?.ClassPointer, "get_text", typeof( string ) );
         public static readonly IntPtr get_richText = Il2CppUtilities.GetIl2CppMethod( UnityTypes.TMP_Text?.ClassPointer, "get_richText", typeof( bool ) );
      }

      public static class TMP_InputField_Methods
      {
         public static readonly IntPtr get_placeholder = Il2CppUtilities.GetIl2CppMethod( UnityTypes.TMP_InputField?.ClassPointer, "get_placeholder", "UnityEngine.UI.Graphic" );
      }

      public static class TextMeshPro_Methods
      {
         public static readonly IntPtr OnEnable = Il2CppUtilities.GetIl2CppMethod( UnityTypes.TextMeshPro?.ClassPointer, "OnEnable", typeof( void ) );
      }

      public static class TextMeshProUGUI_Methods
      {
         public static readonly IntPtr OnEnable = Il2CppUtilities.GetIl2CppMethod( UnityTypes.TextMeshProUGUI?.ClassPointer, "OnEnable", typeof( void ) );
      }

      public static class UILabel_Methods
      {
         public static readonly IntPtr set_text = Il2CppUtilities.GetIl2CppMethod( UnityTypes.UILabel?.ClassPointer, "set_text", typeof( void ), typeof( string ) );
         public static readonly IntPtr get_text = Il2CppUtilities.GetIl2CppMethod( UnityTypes.UILabel?.ClassPointer, "get_text", typeof( string ) );
      }

      public static class UIRect_Methods
      {
         public static readonly IntPtr OnEnable = Il2CppUtilities.GetIl2CppMethod( UnityTypes.UIRect?.ClassPointer, "OnEnable", typeof( void ) );
      }

      public static class GUIUtility_Methods
      {
         public static readonly Func<Il2CppSystem.Type, int, object> GetStateObject = (Func<Il2CppSystem.Type, int, object>)ExpressionHelper.CreateTypedFastInvokeUnchecked(
            typeof( GUIUtility ).GetMethod(
               "GetStateObject",
               BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic,
               null,
               new Type[] { typeof( Il2CppSystem.Type ), typeof( int ) },
               null
            ) );
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
         public static readonly Action<Texture2D, Il2CppStructArray<byte>> LoadImage =
            (Action<Texture2D, Il2CppStructArray<byte>>)ExpressionHelper.CreateTypedFastInvokeUnchecked(
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
      }

      public static class ImageConversions_Methods
      {
         public static readonly Action<Texture2D, Il2CppStructArray<byte>, bool> LoadImage =
            (Action<Texture2D, Il2CppStructArray<byte>, bool>)ExpressionHelper.CreateTypedFastInvokeUnchecked(
               UnityTypes.ImageConversion?.ProxyType?.GetMethod(
                  "LoadImage",
                  BindingFlags.Public | BindingFlags.Static,
                  null,
                  new Type[] { typeof( Texture2D ), typeof( Il2CppStructArray<byte> ), typeof( bool ) },
                  null
               ) );

         public static readonly Func<Texture2D, Il2CppStructArray<byte>> EncodeToPNG =
            (Func<Texture2D, Il2CppStructArray<byte>>)ExpressionHelper.CreateTypedFastInvokeUnchecked(
               UnityTypes.ImageConversion?.ProxyType.GetMethod(
                  "EncodeToPNG",
                  BindingFlags.Public | BindingFlags.Static,
                  null,
                  new Type[] { typeof( Texture2D ) },
                  null
               ) );
      }

      private static Il2CppTypeWrapper FindType( string name )
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
            if( ptr == IntPtr.Zero ) return null;

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

#endif