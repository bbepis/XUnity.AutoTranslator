using BepInEx;
using BepInEx.Configuration;
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
         LogAllLoadedResources = Config.Wrap( new ConfigDefinition( "General", "LogAllLoadedResources", "Indicates whether or not unhandled assets/bundles should be logged to the console." ), false );
         ResourceRedirection.LogAllLoadedResources = LogAllLoadedResources.Value;
         LogAllLoadedResources.SettingChanged += ( s, e ) => ResourceRedirection.LogAllLoadedResources = LogAllLoadedResources.Value;


         EmulateAssetBundles = Config.Wrap( new ConfigDefinition( "General", "EmulateAssetBundles", "Indicates whether or not to enable emulation of asset bundles being loaded." ), false );
         ToggleEmulateAssetBundles( null, null );
         EmulateAssetBundles.SettingChanged += ToggleEmulateAssetBundles;

         Config.ConfigReloaded += Config_ConfigReloaded;
      }

      private void Config_ConfigReloaded( object sender, EventArgs e )
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
