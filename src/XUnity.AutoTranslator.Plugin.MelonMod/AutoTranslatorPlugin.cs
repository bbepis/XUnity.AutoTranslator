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
using XUnity.AutoTranslator.Plugin.Core.Support;
using XUnity.AutoTranslator.Plugin.MelonMod;
using XUnity.Common.Constants;
using XUnity.Common.Logging;
using XUnity.Common.Support;

#if IL2CPP
using UnhollowerRuntimeLib;
#endif

[assembly: MelonInfo( typeof( AutoTranslatorPlugin ), PluginData.Name, PluginData.Version, PluginData.Author )]
[assembly: MelonGame( null, null )]

namespace XUnity.AutoTranslator.Plugin.MelonMod
{
   public class AutoTranslatorPlugin : MelonLoader.MelonMod
   {
      public static IMonoBehaviour _monoBehaviour;


      public override void OnApplicationStart()
      {
#if MANAGED
         // specify location because the mod dll is loaded by byte[]
         PluginLoader.Load( false, Location );
#else
         // do not specify location, because the Core DLL is actually loaded properly from the file system
         _monoBehaviour = PluginLoader.Load( false, null );

         AutoTranslatorBehaviour.Create();
#endif
      }
   }

#if IL2CPP
   internal class AutoTranslatorBehaviour : MonoBehaviour
   {
      static AutoTranslatorBehaviour()
      {
         ClassInjector.RegisterTypeInIl2Cpp<AutoTranslatorBehaviour>();
      }

      private static GameObject _obj;
      private static AutoTranslatorBehaviour _comp;
      private static bool _destroying;

      internal static void Create()
      {
         _obj = new GameObject();
         GameObject.DontDestroyOnLoad( _obj );
         _comp = _obj.AddComponent<AutoTranslatorBehaviour>();
      }

      public AutoTranslatorBehaviour( IntPtr value ) : base( value )
      {

      }

      void Update()
      {
         AutoTranslatorPlugin._monoBehaviour.Update();
      }

      void OnGUI()
      {
         AutoTranslatorPlugin._monoBehaviour.OnGUI();
      }

      void Start()
      {
         AutoTranslatorPlugin._monoBehaviour.Start();
      }

      void OnDestroy()
      {
         if( !_destroying )
         {
            XuaLogger.AutoTranslator.Warn( "Recreating plugin behaviour because it was destroyed..." );
            Create();
         }
      }

      void OnApplicationQuit()
      {
         _destroying = true;
      }
   }
#endif
}
