using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ExIni;
using MelonLoader;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.MelonMod;
using XUnity.Common.Constants;
using XUnity.Common.Logging;

[assembly: MelonInfo( typeof( AutoTranslatorPlugin ), PluginData.Name, PluginData.Version, PluginData.Author )]
[assembly: MelonGame( null, null )]

namespace XUnity.AutoTranslator.Plugin.MelonMod
{
   public class AutoTranslatorPlugin : MelonLoader.MelonMod
   {
      public override void OnApplicationLateStart()
      {
#if IL2CPP
         var modFi = new FileInfo( Location );
         var gameDir = modFi.Directory.Parent;
         var unhollowedPath = Path.Combine( gameDir.FullName, @"MelonLoader\Il2CppAssemblies" );
         Il2CppProxyAssemblies.Location = unhollowedPath;
#endif

         PluginLoader.Load( false );
      }
   }
}
