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
      private static HarmonyInstance _harmony;

      static HooksSetup()
      {
         _harmony = HarmonyInstance.Create( "gravydevsupreme.xunity.autotranslator" );
      }

      public static void InstallOverrideTextHooks()
      {
         if( Settings.EnableUGUI || Settings.EnableUtage )
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
      }

      public static void InstallImageHooks()
      {
         try
         {
            if( Settings.EnableTextureTranslation || Settings.EnableTextureDumping )
            {
               _harmony.PatchAll( UGUIImageHooks.All );
            }
         }
         catch( Exception e )
         {
            Logger.Current.Error( e, "An error occurred while setting up image hooks for UnityEngine." );
         }

         try
         {
            if( Settings.EnableTextureTranslation || Settings.EnableTextureDumping )
            {
               _harmony.PatchAll( NGUIImageHooks.All );
            }
         }
         catch( Exception e )
         {
            Logger.Current.Error( e, "An error occurred while setting up image hooks for NGUI." );
         }

         //var knownTypes = new HashSet<Type> { typeof( Texture ), typeof( Texture2D ), typeof( Sprite ), typeof( Material ) };
         //foreach( var assembly in AppDomain.CurrentDomain.GetAssemblies() )
         //{
         //   try
         //   {
         //      var types = assembly.GetTypes();
         //      foreach( var type in types )
         //      {
         //         try
         //         {
         //            var properties = type.GetProperties( BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
         //            foreach( var property in properties )
         //            {
         //               if( property.CanWrite && knownTypes.Contains( property.PropertyType ) )
         //               {
         //                  try
         //                  {
         //                     var original = property.GetSetMethod();
         //                     var prefix = typeof( GenericPrefix_Hook ).GetMethod( "Prefix" );
         //                     _harmony.Patch( original, new HarmonyMethod( prefix ), null, null );
         //                     Logger.Current.Warn( "Patched: " + type.Name + "." + property.Name );
         //                  }
         //                  catch( Exception e )
         //                  {
         //                     Logger.Current.Error( "Failed patching: " + type.Name + "." + property.Name );
         //                  }
         //               }
         //            }
         //         }
         //         catch( Exception e )
         //         {
         //            Logger.Current.Error( e, "Failed getting properties of type: " + type.Name );
         //         }
         //      }
         //   }
         //   catch( Exception )
         //   {
         //      Logger.Current.Error( "Failed getting types of assembly: " + assembly.FullName );
         //   }
         //}

      }

      public static void InstallTextHooks()
      {

         try
         {
            if( Settings.EnableUGUI || Settings.EnableUtage )
            {
               _harmony.PatchAll( UGUIHooks.All );
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
               _harmony.PatchAll( TextMeshProHooks.All );
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
               _harmony.PatchAll( NGUIHooks.All );
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
               _harmony.PatchAll( IMGUIHooks.All );

               // This wont work in "newer" unity versions!
               try
               {
                  _harmony.PatchType( typeof( DoButtonGridHook ) );
               }
               catch { }
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
               _harmony.PatchAll( UtageHooks.All );
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
         }
         catch( Exception e )
         {
            Logger.Current.Error( e, $"An error occurred while setting up override hooks for '{eventName}'." );
         }

         return false;
      }
   }
}
