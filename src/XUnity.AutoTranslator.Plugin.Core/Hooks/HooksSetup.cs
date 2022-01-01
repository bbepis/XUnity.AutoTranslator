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
using XUnity.AutoTranslator.Plugin.Core.Hooks.NGUI;
using XUnity.AutoTranslator.Plugin.Core.Hooks.TextMeshPro;
using XUnity.AutoTranslator.Plugin.Core.Hooks.UGUI;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks
{
   internal static class HooksSetup
   {
      public static void InstallTextGetterCompatHooks()
      {
         try
         {
            if( Settings.TextGetterCompatibilityMode )
            {
#if MANAGED
               HookingHelper.PatchAll( TextGetterCompatHooks.All, Settings.ForceMonoModHooks );
#else
               throw new NotImplementedException("TextGetterCompatibilityMode is not supported in IL2CPP.");
#endif
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

      public static void InstallSpriteRendererHooks()
      {
         try
         {
            if( Settings.EnableSpriteRendererHooking && ( Settings.EnableTextureTranslation || Settings.EnableTextureDumping ) )
            {
               HookingHelper.PatchAll( ImageHooks.SpriteRenderer, Settings.ForceMonoModHooks );
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

               if( Settings.DisableTextMeshProScrollInEffects )
               {
                  HookingHelper.PatchAll( TextMeshProHooks.DisableScrollInTmp, Settings.ForceMonoModHooks );
               }
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
#if MANAGED
               HookingHelper.PatchAll( IMGUIHooks.All, Settings.ForceMonoModHooks );
#else
               throw new NotImplementedException("EnableIMGUI is not supported in IL2CPP.");
#endif
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

         try
         {
            if( Settings.EnableFairyGUI )
            {
               HookingHelper.PatchAll( FairyGUIHooks.All, Settings.ForceMonoModHooks );
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while setting up hooks for FairyGUI." );
         }
      }

      private static bool _installedPluginTranslationHooks = false;
      public static void InstallComponentBasedPluginTranslationHooks()
      {
         if( AutoTranslationPlugin.Current.PluginTextCaches.Count > 0 )
         {
            if( !_installedPluginTranslationHooks )
            {
               _installedPluginTranslationHooks = true;
               try
               {
#if MANAGED
                  HookingHelper.PatchAll( PluginTranslationHooks.All, Settings.ForceMonoModHooks );
#else
                  throw new NotImplementedException("Plugin specific hooks are not supported in IL2CPP.");
#endif
               }
               catch( Exception e )
               {
                  XuaLogger.AutoTranslator.Error( e, "An error occurred while setting up hooks for Plugin translations." );
               }
            }
         }
      }

      private static HashSet<Assembly> _installedAssemblies = new HashSet<Assembly>();
      public static void InstallIMGUIBasedPluginTranslationHooks( Assembly assembly, bool final )
      {
         if( Settings.EnableIMGUI && !_installedAssemblies.Contains( assembly ) )
         {
#if MANAGED
            if( final )
            {
               IMGUIPluginTranslationHooks.ResetHandledForAllInAssembly( assembly );
            }

            var types = assembly.GetTypes();
            foreach( var type in types )
            {
               try
               {
                  if( typeof( MonoBehaviour ).IsAssignableFrom( type ) && !type.IsAbstract )
                  {
                     var method = type.GetMethod( "OnGUI", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy );
                     if( method != null )
                     {
                        IMGUIPluginTranslationHooks.HookIfConfigured( method );
                     }
                  }
               }
               catch( Exception e2 )
               {
                  XuaLogger.AutoTranslator.Warn( e2, "An error occurred while hooking type: " + type.FullName );
               }
            }

            if( final )
            {
               _installedAssemblies.Add( assembly );
            }
#else
            throw new NotImplementedException("Plugin specific hooks are not supported in IL2CPP.");
#endif
         }
      }
   }
}
