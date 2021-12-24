using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core
{
   /// <summary>
   /// Entry point for manipulating translations that have been loaded by the plugin.
   ///
   /// Methods on this interface should be called during plugin initialization. Preferably during the Start callback.
   /// </summary>
   public static class TranslationRegistry
   {
      /// <summary>
      /// Obtains the translations registry instance.
      /// </summary>
      public static ITranslationRegistry Default => AutoTranslationPlugin.Current;
   }

   /// <summary>
   /// Interface for manipulating translation that have been loaded by the plugin.
   /// </summary>
   public interface ITranslationRegistry
   {
      /// <summary>
      /// Registers and loads the specified translation package.
      /// </summary>
      /// <param name="assembly">The assembly that the behaviour should be applied to.</param>
      /// <param name="package">Package containing translations.</param>
      void RegisterPluginSpecificTranslations( Assembly assembly, StreamTranslationPackage package );

      /// <summary>
      /// Registers and loads the specified translation package.
      /// </summary>
      /// <param name="assembly">The assembly that the behaviour should be applied to.</param>
      /// <param name="package">Package containing translations.</param>
      void RegisterPluginSpecificTranslations( Assembly assembly, KeyValuePairTranslationPackage package );

      /// <summary>
      /// Allow plugin-specific translation to fallback to generic translations.
      /// </summary>
      /// <param name="assembly">The assembly that the behaviour should be applied to.</param>
      void EnablePluginTranslationFallback( Assembly assembly );
   }

   /// <summary>
   /// Extension methods for the ITranslationRegistry interface.
   /// </summary>
   public static class TranslationRegistryExtensions
   {
      private static Assembly GetCallingPlugin()
      {
         var frame = new StackFrame( 2 );
         var method = frame.GetMethod();
         if( method != null )
         {
            var ass = method.DeclaringType.Assembly;
            return ass;
         }
         throw new ArgumentException( "Could not automatically determine the calling plugin. Consider calling the overload of this method taking an assembly name." );
      }

      /// <summary>
      /// Registers and loads the specified translation package. Inspects the callstack to determine
      /// the calling plugin that the translations should be associated with.
      /// </summary>
      /// <param name="registry">The translation registry that the package is being registered with.</param>
      /// <param name="package">Package containing translations.</param>
      public static void RegisterPluginSpecificTranslations( this ITranslationRegistry registry, StreamTranslationPackage package )
      {
         var assembly = GetCallingPlugin();
         registry.RegisterPluginSpecificTranslations( assembly, package );
      }

      /// <summary>
      /// Registers and loads the specified translation package. Inspects the callstack to determine
      /// the calling plugin that the translations should be associated with.
      /// </summary>
      /// <param name="registry">The translation registry that the package is being registered with.</param>
      /// <param name="package">Package containing translations.</param>
      public static void RegisterPluginSpecificTranslations( this ITranslationRegistry registry, KeyValuePairTranslationPackage package )
      {
         var assembly = GetCallingPlugin();
         registry.RegisterPluginSpecificTranslations( assembly, package );
      }

      /// <summary>
      /// Allow plugin-specific translation to fallback to generic translations. Inspects the callstack to determine
      /// the calling plugin that the translations should be associated with.
      /// </summary>
      /// <param name="registry">The translation registry that the package is being registered with.</param>
      public static void EnablePluginTranslationFallback( this ITranslationRegistry registry )
      {
         var assembly = GetCallingPlugin();
         registry.EnablePluginTranslationFallback( assembly );
      }
   }
}
