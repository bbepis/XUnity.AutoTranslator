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
using XUnity.AutoTranslator.Plugin.Core.Support;
using XUnity.AutoTranslator.Plugin.MelonMod;
using XUnity.AutoTranslator.Plugin.MelonMod.Support;
using XUnity.Common.Support;

[assembly: MelonModInfo( typeof( AutoTranslatorPlugin ), PluginData.Name, PluginData.Version, PluginData.Author )]
[assembly: MelonModGame( null, null )]

namespace XUnity.AutoTranslator.Plugin.MelonMod
{
   public class AutoTranslatorPlugin : MelonLoader.MelonMod
   {
      private IMonoBehaviour _monoBehaviour;

      public override void OnApplicationStart()
      {
         CoroutineHelper.Instance = new MelonModCoroutineHelper();

         _monoBehaviour = PluginLoader.Load();

         _monoBehaviour.Start();
      }

      public override void OnUpdate()
      {
         _monoBehaviour.Update();
      }
   }
}
