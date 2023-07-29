using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using XUnity.ResourceRedirector.Constants;

namespace XUnity.ResourceRedirector.BepInEx
{
   [BepInPlugin( PluginData.Identifier, PluginData.Name, PluginData.Version )]
   public class ResourceRedirectorPlugin : BasePlugin
   {
      public static ConfigEntry<bool> LogAllLoadedResources { get; set; }
      public static ConfigEntry<bool> LogCallbackOrder { get; set; }

      public override void Load()
      {
         LogAllLoadedResources = Config.Bind( new ConfigDefinition( "Diagnostics", "Log all loaded resources" ), false );
         ResourceRedirection.LogAllLoadedResources = LogAllLoadedResources.Value;
         LogAllLoadedResources.SettingChanged += ( s, e ) => ResourceRedirection.LogAllLoadedResources = LogAllLoadedResources.Value;

         LogCallbackOrder = Config.Bind( new ConfigDefinition( "Diagnostics", "Log callback order" ), false );
         ResourceRedirection.LogCallbackOrder = LogCallbackOrder.Value;
         LogCallbackOrder.SettingChanged += ( s, e ) => ResourceRedirection.LogCallbackOrder = LogCallbackOrder.Value;

         Config.ConfigReloaded += Config_ConfigReloaded;
      }

      private static void Config_ConfigReloaded( object sender, EventArgs e )
      {
         ResourceRedirection.LogAllLoadedResources = LogAllLoadedResources.Value;
         ResourceRedirection.LogCallbackOrder = LogCallbackOrder.Value;
      }
   }
}
