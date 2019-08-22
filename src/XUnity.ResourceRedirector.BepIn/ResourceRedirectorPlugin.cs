using BepInEx;
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

         LogAllLoadedResources = new ConfigWrapper<bool>( "LogAllLoadedResources", this, false );
         ResourceRedirection.LogAllLoadedResources = LogAllLoadedResources.Value;
         LogAllLoadedResources.SettingChanged += ( s, e ) => ResourceRedirection.LogAllLoadedResources = LogAllLoadedResources.Value;


         EmulateAssetBundles = new ConfigWrapper<bool>( "EmulateAssetBundles", this, false );
         ToggleEmulateAssetBundles( null, null );
         EmulateAssetBundles.SettingChanged += ToggleEmulateAssetBundles;

         RedirectMissingAssetBundles = new ConfigWrapper<bool>( "RedirectMissingAssetBundles", this, false );
         ToggleDisableRedirectMissingAssetBundles( null, null );
         RedirectMissingAssetBundles.SettingChanged += ToggleEmulateAssetBundles;

         Config.ConfigReloaded += Config_ConfigReloaded;
      }

      private void Config_ConfigReloaded()
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
