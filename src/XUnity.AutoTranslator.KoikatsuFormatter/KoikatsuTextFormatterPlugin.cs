using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx.Harmony;
using UnityEngine;
using UnityEngine.UI;

namespace XUnity.AutoTranslator.KoikatsuFormatter
{
   [BepInPlugin( GUID: "gravydevsupreme.xunity.autotranslator.koikatsuformatter", Name: "KoikatsuTextFormatter", Version: "1.0.0" )]
   public class KoikatsuTextFormatterPlugin : BaseUnityPlugin
   {
      void Awake()
      {
         InstallHooks();

         // disable thy self!
         enabled = false;
      }

      public void InstallHooks()
      {
         try
         {
            var harmony = new Harmony( "gravydevsupreme.xunity.autotranslator.koikatsuformatter" );
            HarmonyWrapper.PatchAll( typeof( TextHooks ), harmony );
         }
         catch( System.Exception e )
         {
            Logger.Log( LogLevel.Error, e );
         }
      }
   }


   public static class TextHooks
   {
      [HarmonyPrefix, HarmonyPatch( typeof( HyphenationJpn ), "GetFormatedText" )]
      public static bool GetFormatedText( Text textComp, string msg, ref string __result )
      {
         if( string.IsNullOrEmpty( msg ) )
         {
            __result = string.Empty;
            return false;
         }

         textComp.horizontalOverflow = HorizontalWrapMode.Wrap;
         __result = msg.Replace( " \n ", " " )
            .Replace( " \n", " " )
            .Replace( "\n ", " " )
            .Replace( "\n", " " );

         return false;
      }
   }
}
