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
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks
{
   internal static class HooksSetup
   {
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
               HookingHelper.PatchAll( TextGetterCompatHooks.All, Settings.ForceMonoModHooks );
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while setting up text getter compat hooks." );
         }
      }

      public static void InstallImageHooks()
      {
         try
         {
            if( Settings.EnableTextureTranslation || Settings.EnableTextureDumping )
            {
               HookingHelper.PatchAll( ImageHooks.All, Settings.ForceMonoModHooks );

               if( Settings.EnableLegacyTextureLoading || Settings.EnableSpriteHooking )
               {
                  HookingHelper.PatchAll( ImageHooks.Sprite, Settings.ForceMonoModHooks );
               }
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while setting up image hooks." );
         }
      }

      private static bool _textAssetHooksInstalled = false;
      public static void InstallTextAssetHooks()
      {
         try
         {
            if( !_textAssetHooksInstalled )
            {
               _textAssetHooksInstalled = true;
               HookingHelper.PatchAll( TextAssetHooks.All, Settings.ForceMonoModHooks );
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while setting up text asset hooks." );
         }
      }

      public static void InstallTextHooks()
      {

         try
         {
            if( Settings.EnableUGUI )
            {
               HookingHelper.PatchAll( UGUIHooks.All, Settings.ForceMonoModHooks );
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while setting up hooks for UGUI." );
         }

         try
         {
            if( Settings.EnableTextMeshPro )
            {
               HookingHelper.PatchAll( TextMeshProHooks.All, Settings.ForceMonoModHooks );
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while setting up hooks for TextMeshPro." );
         }

         try
         {
            if( Settings.EnableNGUI )
            {
               HookingHelper.PatchAll( NGUIHooks.All, Settings.ForceMonoModHooks );
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while setting up hooks for NGUI." );
         }

         try
         {
            if( Settings.EnableIMGUI )
            {
               HookingHelper.PatchAll( IMGUIHooks.All, Settings.ForceMonoModHooks );
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while setting up hooks for IMGUI." );
         }

         try
         {
            HookingHelper.PatchAll( UtageHooks.All, Settings.ForceMonoModHooks );
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while setting up hooks for Utage." );
         }

         try
         {
            if( Settings.EnableTextMesh )
            {
               HookingHelper.PatchAll( TextMeshHooks.All, Settings.ForceMonoModHooks );
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while setting up hooks for TextMesh." );
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

                                 XuaLogger.AutoTranslator.Info( eventName + " was hooked by external plugin." );
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
            XuaLogger.AutoTranslator.Error( e, $"An error occurred while setting up override hooks for '{eventName}'." );
         }

         return false;
      }
   }
}
