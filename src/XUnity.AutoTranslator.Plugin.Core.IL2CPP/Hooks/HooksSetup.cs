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
using XUnity.AutoTranslator.Plugin.Core.Hooks.TextMeshPro;
using XUnity.AutoTranslator.Plugin.Core.Hooks.UGUI;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks
{
   internal static class HooksSetup
   {
      public static void InstallTextHooks()
      {

         try
         {
            if( Settings.EnableUGUI )
            {
               MLHookingHelper.PatchAll( UGUIHooks.All );
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
               MLHookingHelper.PatchAll( TextMeshProHooks.All );
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while setting up hooks for TextMeshPro." );
         }

         //try
         //{
         //   if( Settings.EnableNGUI )
         //   {
         //      MLHookingHelper.PatchAll( NGUIHooks.All, Settings.ForceMonoModHooks );
         //   }
         //}
         //catch( Exception e )
         //{
         //   XuaLogger.AutoTranslator.Error( e, "An error occurred while setting up hooks for NGUI." );
         //}

         //try
         //{
         //   if( Settings.EnableIMGUI )
         //   {
         //      MLHookingHelper.PatchAll( IMGUIHooks.All, Settings.ForceMonoModHooks );
         //   }
         //}
         //catch( Exception e )
         //{
         //   XuaLogger.AutoTranslator.Error( e, "An error occurred while setting up hooks for IMGUI." );
         //}

         //try
         //{
         //   MLHookingHelper.PatchAll( UtageHooks.All, Settings.ForceMonoModHooks );
         //}
         //catch( Exception e )
         //{
         //   XuaLogger.AutoTranslator.Error( e, "An error occurred while setting up hooks for Utage." );
         //}

         try
         {
            if( Settings.EnableTextMesh )
            {
               MLHookingHelper.PatchAll( TextMeshHooks.All );
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while setting up hooks for TextMesh." );
         }

         //try
         //{
         //   if( Settings.EnableFairyGUI )
         //   {
         //      MLHookingHelper.PatchAll( FairyGUIHooks.All, Settings.ForceMonoModHooks );
         //   }
         //}
         //catch( Exception e )
         //{
         //   XuaLogger.AutoTranslator.Error( e, "An error occurred while setting up hooks for FairyGUI." );
         //}
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
