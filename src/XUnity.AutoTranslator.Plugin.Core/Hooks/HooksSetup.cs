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
      public static void InstallHooks()
      {
         var harmony = HarmonyInstance.Create( "gravydevsupreme.xunity.autotranslator" );

         bool success = false;
         try
         {
            if( Settings.EnableUGUI || Settings.EnableUtage )
            {
               success = SetupHook( KnownEvents.OnUnableToTranslateUGUI, AutoTranslationPlugin.Current.Hook_TextChanged_WithResult );
               if( !success )
               {
                  harmony.PatchAll( UGUIHooks.All );
               }
            }
         }
         catch( Exception e )
         {
            Logger.Current.Error( e, "An error occurred while setting up hooks for UGUI." );
         }

         try
         {
            if( Settings.EnableTextMeshPro )
            {
               success = SetupHook( KnownEvents.OnUnableToTranslateTextMeshPro, AutoTranslationPlugin.Current.Hook_TextChanged_WithResult );
               if( !success )
               {
                  harmony.PatchAll( TextMeshProHooks.All );
               }
            }
         }
         catch( Exception e )
         {
            Logger.Current.Error( e, "An error occurred while setting up hooks for TextMeshPro." );
         }

         try
         {
            if( Settings.EnableNGUI )
            {
               success = SetupHook( KnownEvents.OnUnableToTranslateNGUI, AutoTranslationPlugin.Current.Hook_TextChanged_WithResult );
               if( !success )
               {
                  harmony.PatchAll( NGUIHooks.All );
               }
            }
         }
         catch( Exception e )
         {
            Logger.Current.Error( e, "An error occurred while setting up hooks for NGUI." );
         }

         try
         {
            if( Settings.EnableIMGUI )
            {
               success = SetupHook( KnownEvents.OnUnableToTranslateNGUI, AutoTranslationPlugin.Current.Hook_TextChanged_WithResult );
               if( !success )
               {
                  harmony.PatchAll( IMGUIHooks.All );

                  // This wont work in "newer" unity versions!
                  try
                  {
                     harmony.PatchType( typeof( DoButtonGridHook ) );
                  }
                  catch { }
               }
            }
         }
         catch( Exception e )
         {
            Logger.Current.Error( e, "An error occurred while setting up hooks for IMGUI." );
         }

         try
         {
            if( Settings.EnableUtage )
            {
               harmony.PatchAll( UtageHooks.All );
            }
         }
         catch( Exception e )
         {
            Logger.Current.Error( e, "An error occurred while setting up hooks for Utage." );
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

                              Logger.Current.Info( eventName + " was hooked by external plugin." );
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
