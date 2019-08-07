using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Hooks.IMGUI;
using XUnity.AutoTranslator.Plugin.Core.Hooks.NGUI;
using XUnity.AutoTranslator.Plugin.Core.Hooks.TextGetterCompat;
using XUnity.AutoTranslator.Plugin.Core.Hooks.TextMeshPro;
using XUnity.AutoTranslator.Plugin.Core.Hooks.UGUI;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks
{

   internal static class HooksSetup
   {
      private static object _harmony;

      public static void InitializeHarmony()
      {
         try
         {
            if( ClrTypes.HarmonyInstance != null )
            {
               _harmony = ClrTypes.HarmonyInstance.GetMethod( "Create", BindingFlags.Static | BindingFlags.Public )
                  .Invoke( null, new object[] { PluginData.Identifier } );
            }
            else if( ClrTypes.Harmony != null )
            {
               _harmony = ClrTypes.Harmony.GetConstructor( new Type[] { typeof( string ) } )
                  .Invoke( new object[] { PluginData.Identifier } );
            }
            else
            {
               XuaLogger.Current.Error( "An unexpected exception occurred during harmony initialization, likely caused by unknown Harmony version. Harmony hooks will be unavailable!" );
            }
         }
         catch( Exception e )
         {
            XuaLogger.Current.Error( e, "An unexpected exception occurred during harmony initialization. Harmony hooks will be unavailable!" );
         }
      }

      public static void InstallOverrideTextHooks()
      {
         if( Settings.EnableUGUI )
         {
            UGUIHooks.HooksOverriden = SetupHook( KnownEvents.OnUnableToTranslateUGUI, AutoTranslationPlugin.Current.ExternalHook_TextChanged_WithResult );
         }
         if( Settings.EnableTextMeshPro )
         {
            TextMeshProHooks.HooksOverriden = SetupHook( KnownEvents.OnUnableToTranslateTextMeshPro, AutoTranslationPlugin.Current.ExternalHook_TextChanged_WithResult );
         }
         if( Settings.EnableNGUI )
         {
            NGUIHooks.HooksOverriden = SetupHook( KnownEvents.OnUnableToTranslateNGUI, AutoTranslationPlugin.Current.ExternalHook_TextChanged_WithResult );
         }
         if( Settings.EnableIMGUI )
         {
            IMGUIHooks.HooksOverriden = SetupHook( KnownEvents.OnUnableToTranslateIMGUI, AutoTranslationPlugin.Current.ExternalHook_TextChanged_WithResult );
         }
         if( Settings.EnableTextMesh )
         {
            TextMeshProHooks.HooksOverriden = SetupHook( KnownEvents.OnUnableToTranslateTextMesh, AutoTranslationPlugin.Current.ExternalHook_TextChanged_WithResult );
         }
      }

      public static void InstallTextGetterCompatHooks()
      {
         try
         {
            if( Settings.TextGetterCompatibilityMode )
            {
               _harmony.PatchAll( TextGetterCompatHooks.All );
            }
         }
         catch( Exception e )
         {
            XuaLogger.Current.Error( e, "An error occurred while setting up text getter compat hooks." );
         }
      }

      public static void InstallImageHooks()
      {
         try
         {
            if( Settings.EnableTextureTranslation || Settings.EnableTextureDumping )
            {
               _harmony.PatchAll( ImageHooks.All );
            }
         }
         catch( Exception e )
         {
            XuaLogger.Current.Error( e, "An error occurred while setting up image hooks." );
         }
      }

      public static void InstallTextHooks()
      {

         try
         {
            if( Settings.EnableUGUI )
            {
               _harmony.PatchAll( UGUIHooks.All );
            }
         }
         catch( Exception e )
         {
            XuaLogger.Current.Error( e, "An error occurred while setting up hooks for UGUI." );
         }

         try
         {
            if( Settings.EnableTextMeshPro )
            {
               _harmony.PatchAll( TextMeshProHooks.All );
            }
         }
         catch( Exception e )
         {
            XuaLogger.Current.Error( e, "An error occurred while setting up hooks for TextMeshPro." );
         }

         try
         {
            if( Settings.EnableNGUI )
            {
               _harmony.PatchAll( NGUIHooks.All );
            }
         }
         catch( Exception e )
         {
            XuaLogger.Current.Error( e, "An error occurred while setting up hooks for NGUI." );
         }

         try
         {
            if( Settings.EnableIMGUI )
            {
               _harmony.PatchAll( IMGUIHooks.All );
            }
         }
         catch( Exception e )
         {
            XuaLogger.Current.Error( e, "An error occurred while setting up hooks for IMGUI." );
         }

         try
         {
            _harmony.PatchAll( UtageHooks.All );
         }
         catch( Exception e )
         {
            XuaLogger.Current.Error( e, "An error occurred while setting up hooks for Utage." );
         }

         try
         {
            if( Settings.EnableTextMesh )
            {
               _harmony.PatchAll( TextMeshHooks.All );
            }
         }
         catch( Exception e )
         {
            XuaLogger.Current.Error( e, "An error occurred while setting up hooks for TextMesh." );
         }
      }

      public static bool SetupHook( string eventName, Func<object, string, string> callback )
      {
         if( !Settings.AllowPluginHookOverride ) return false;

         try
         {
            var objects = GameObject.FindObjectsOfType<GameObject>();
            foreach( var gameObject in objects )
            {
               if( gameObject != null )
               {
                  var components = gameObject.GetComponents<Component>();
                  foreach( var component in components )
                  {
                     if( component != null )
                     {
                        var e = component.GetType().GetEvent( eventName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
                        if( e != null )
                        {
                           var addMethod = e.GetAddMethod();
                           if( addMethod != null )
                           {
                              try
                              {
                                 if( addMethod.IsStatic )
                                 {
                                    addMethod.Invoke( null, new object[] { callback } );
                                 }
                                 else
                                 {
                                    addMethod.Invoke( component, new object[] { callback } );
                                 }

                                 XuaLogger.Current.Info( eventName + " was hooked by external plugin." );
                                 return true;
                              }
                              catch { }
                           }
                        }
                     }
                  }
               }
            }
         }
         catch( Exception e )
         {
            XuaLogger.Current.Error( e, $"An error occurred while setting up override hooks for '{eventName}'." );
         }

         return false;
      }
   }
}
