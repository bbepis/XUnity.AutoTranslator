using System.Linq;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.Common.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core
{
   /// <summary>
   /// Class representing publicly available settings of the plugin.
   /// </summary>
   public static class AutoTranslatorSettings
   {
      /// <summary>
      /// Gets the user-supplied user agent.
      /// </summary>
      public static string UserAgent => Settings.UserAgent;

      /// <summary>
      /// Gets the configured source language.
      /// </summary>
      public static string SourceLanguage => Settings.FromLanguage;

      /// <summary>
      /// Gets the configured destination language.
      /// </summary>
      public static string DestinationLanguage => Settings.Language;

      /// <summary>
      /// Gets a bool indicating if redirected resource dumping is enabled.
      /// </summary>
      public static bool IsDumpingRedirectedResourcesEnabled => Settings.EnableDumping;

      /// <summary>
      /// Gets the root of the default path to output redirected  resources to.
      /// </summary>
      public static string DefaultRedirectedResourcePath => Settings.RedirectedResourcesPath;
   }
}
