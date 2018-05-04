using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Hooks.NGUI;
using XUnity.AutoTranslator.Plugin.Core.Hooks.TextMeshPro;
using XUnity.AutoTranslator.Plugin.Core.Hooks.UGUI;
using XUnity.AutoTranslator.Plugin.Core.IMGUI;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks
{

   public static class HooksSetup
   {
      public static void InstallHooks( Func<object, string, string> defaultHook )
      {
         try
         {
            var harmony = HarmonyInstance.Create( "gravydevsupreme.xunity.autotranslator" );

            bool success = false;
            if( Settings.EnableUGUI )
            {
               success = SetupHook( KnownEvents.OnUnableToTranslateUGUI, defaultHook );
               if( !success )
               {
                  harmony.PatchAll( UGUIHooks.All );
               }
            }

            if( Settings.EnableTextMeshPro )
            {
               success = SetupHook( KnownEvents.OnUnableToTranslateTextMeshPro, defaultHook );
               if( !success )
               {
                  harmony.PatchAll( TextMeshProHooks.All );
               }
            }

            if( Settings.EnableNGUI )
            {
               success = SetupHook( KnownEvents.OnUnableToTranslateNGUI, defaultHook );
               if( !success )
               {
                  harmony.PatchAll( NGUIHooks.All );
               }
            }

            if( Settings.EnableIMGUI )
            {
               harmony.PatchAll( IMGUIHooks.All );
            }
         }
         catch( Exception e )
         {
            Console.WriteLine( "ERROR WHILE INITIALIZING AUTO TRANSLATOR: " + Environment.NewLine + e );
         }
      }

      public static bool SetupHook( string eventName, Func<object, string, string> callback )
      {
         if( !Settings.AllowPluginHookOverride ) return false;

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

                              return true;
                           }
                           catch { }
                        }
                     }
                  }
               }
            }
         }

         return false;
      }
   }
}
