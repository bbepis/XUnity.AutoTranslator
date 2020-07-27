using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ExIni;
using MelonLoader;
using XUnity.AutoTranslator.Plugin.Core;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.MelonMod;
using XUnity.Common.Shims;

[assembly: MelonModInfo( typeof( AutoTranslatorPlugin ), PluginData.Name, PluginData.Version, PluginData.Author )]
[assembly: MelonModGame( null, null )]

namespace XUnity.AutoTranslator.Plugin.MelonMod
{
   public class AutoTranslatorPlugin : MelonLoader.MelonMod
   {
      private IMonoBehaviour _monoBehaviour;

      public override void OnApplicationStart()
      {
         _monoBehaviour = PluginLoader.Load();

         _monoBehaviour.Start();
      }

      public override void OnUpdate()
      {
         _monoBehaviour.Update();
      }
   }
}
