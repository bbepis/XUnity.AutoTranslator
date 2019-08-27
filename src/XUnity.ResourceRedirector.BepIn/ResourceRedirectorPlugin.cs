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

      void Awake()
      {
         LogAllLoadedResources = new ConfigWrapper<bool>( "LogAllLoadedResources", this, false );
         ResourceRedirection.LogAllLoadedResources = LogAllLoadedResources.Value;
         LogAllLoadedResources.SettingChanged += ( s, e ) => ResourceRedirection.LogAllLoadedResources = LogAllLoadedResources.Value;

         Config.ConfigReloaded += Config_ConfigReloaded;
      }

      private void Config_ConfigReloaded()
      {
         ResourceRedirection.LogAllLoadedResources = LogAllLoadedResources.Value;
      }
   }
}
