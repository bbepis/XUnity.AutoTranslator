using System;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core
{
   /// <summary>
   /// Class representing publicly available state of the plugin.
   /// </summary>
   public static class AutoTranslatorState
   {
      /// <summary>
      /// Gets the number of translation requests that has been completed.
      /// </summary>
      public static int TranslationCount => Settings.TranslationCount;

      /// <summary>
      /// Indicates whether the plugin has been successfully fully initialized.
      /// </summary>
      public static bool PluginInitialized { get; private set; }

      /// <summary>
      /// Occurs when the plugin initialization process has completed.
      /// Use this to perform very early actions that depend on the plugin being fully initialized.
      /// </summary>
      public static event Action PluginInitializationCompleted;

      internal static void OnPluginInitializationCompleted()
      {
         if( PluginInitialized ) return;

         PluginInitialized = true;

         if( PluginInitializationCompleted != null )
         {
            try
            {
               PluginInitializationCompleted();
            }
            catch( Exception e )
            {
               XuaLogger.AutoTranslator.Error( "Subscriber crash in PluginInitializationCompleted event: " + e );
            }
         }
      }
   }
}
