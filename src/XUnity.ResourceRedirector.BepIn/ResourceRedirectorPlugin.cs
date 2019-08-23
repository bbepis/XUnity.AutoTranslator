using BepInEx;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using XUnity.ResourceRedirector.Constants;

namespace XUnity.ResourceRedirector.BepIn_5x
{
   [BepInPlugin( PluginData.Identifier, PluginData.Name, PluginData.Version )]
   public class ResourceRedirectorPlugin : BaseUnityPlugin
   {
      [Category( "Settings" )]
      [DisplayName( "Log all loaded resources" )]
      [Description( "Indicates whether or not all loaded resources should be logged to the console. Prepare for spam if enabled." )]
      public ConfigWrapper<bool> LogAllLoadedResources { get; set; }

      [Category( "Settings" )]
      [DisplayName( "Emulate asset bundles" )]
      [Description( "Indicates whether or not asset bundle emulation should be enabled. This will allow the plugin to look for asset bundles in the 'emulations' directory before looking in the default location." )]
      public ConfigWrapper<bool> EmulateAssetBundles { get; set; }

      void Awake()
      {
         LogAllLoadedResources = new ConfigWrapper<bool>( "LogAllLoadedResources", this, false );
         ResourceRedirection.LogAllLoadedResources = LogAllLoadedResources.Value;
         LogAllLoadedResources.SettingChanged += ( s, e ) => ResourceRedirection.LogAllLoadedResources = LogAllLoadedResources.Value;

         EmulateAssetBundles = new ConfigWrapper<bool>( "EmulateAssetBundles", this, false );
         ToggleEmulateAssetBundles( null, null );
         EmulateAssetBundles.SettingChanged += ToggleEmulateAssetBundles;

         Config.ConfigReloaded += Config_ConfigReloaded;
      }

      private void Config_ConfigReloaded()
      {
         ToggleEmulateAssetBundles( null, null );
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
   }
}
