using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.ResourceRedirector.Constants;

namespace XUnity.ResourceRedirector.BepIn_5x
{
   [BepInPlugin( PluginData.Identifier, PluginData.Name, PluginData.Version )]
   public class ResourceRedirectorPlugin : BaseUnityPlugin
   {
      public ConfigWrapper<bool> LogAllLoadedResources;
      public ConfigWrapper<bool> EmulateAssetBundles;
      public ConfigWrapper<bool> RedirectMissingAssetBundles;

      void Awake()
      {
         ResourceRedirection.Initialize();

         LogAllLoadedResources = Config.Wrap( new ConfigDefinition( "General", "LogAllLoadedResources", "Indicates whether or not unhandled assets/bundles should be logged to the console." ), false );
         ResourceRedirection.LogAllLoadedResources = LogAllLoadedResources.Value;
         LogAllLoadedResources.SettingChanged += ( s, e ) => ResourceRedirection.LogAllLoadedResources = LogAllLoadedResources.Value;


         EmulateAssetBundles = Config.Wrap( new ConfigDefinition( "General", "EmulateAssetBundles", "Indicates whether or not to enable emulation of asset bundles being loaded." ), false );
         ToggleEmulateAssetBundles( null, null );
         EmulateAssetBundles.SettingChanged += ToggleEmulateAssetBundles;

         RedirectMissingAssetBundles = Config.Wrap( new ConfigDefinition( "General", "RedirectMissingAssetBundles", "Indicates whether or not to redirect missing asset bundles to a dynamically generated empty asset bundle." ), false );
         ToggleDisableRedirectMissingAssetBundles( null, null );
         RedirectMissingAssetBundles.SettingChanged += ToggleEmulateAssetBundles;

         Config.ConfigReloaded += Config_ConfigReloaded;
      }

      private void Config_ConfigReloaded( object sender, EventArgs e )
      {
         ToggleEmulateAssetBundles( null, null );
         ToggleDisableRedirectMissingAssetBundles( null, null );
      }

      private void ToggleEmulateAssetBundles( object sender, EventArgs args )
      {
         if( EmulateAssetBundles.Value )
         {
            ResourceRedirection.EnableEmulateAssetBundles( 10000, "emulations" );
         }
         else
         {
            ResourceRedirection.DisableEmulateAssetBundles();
         }
      }

      private void ToggleDisableRedirectMissingAssetBundles( object sender, EventArgs args )
      {
         if( EmulateAssetBundles.Value )
         {
            ResourceRedirection.EnableRedirectMissingAssetBundlesToEmptyAssetBundle( -1000000 );
         }
         else
         {
            ResourceRedirection.DisableRedirectMissingAssetBundlesToEmptyAssetBundle();
         }
      }
   }
}
