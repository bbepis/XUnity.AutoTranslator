using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints
{
   internal class InitializationContext : IInitializationContext
   {
      private HttpSecurity _security;

      internal InitializationContext(
         HttpSecurity httpSecurity,
         string sourceLanguage,
         string destinationLanguage )
      {
         _security = httpSecurity;

         SourceLanguage = sourceLanguage;
         DestinationLanguage = destinationLanguage;
      }

      /// <summary>
      /// Gets the source language.
      /// </summary>
      public string SourceLanguage { get; }

      /// <summary>
      /// Gets the destination language.
      /// </summary>
      public string DestinationLanguage { get; }

      public string PluginDirectory => PluginEnvironment.Current.PluginPath;

      public void DisableCertificateChecksFor( params string[] hosts )
      {
         _security.EnableSslFor( hosts );
      }

      public T GetOrCreateSetting<T>( string section, string key, T defaultValue )
      {
         return PluginEnvironment.Current.Preferences.GetOrDefault( section, key, defaultValue );
      }

      public T GetOrCreateSetting<T>( string section, string key )
      {
         return PluginEnvironment.Current.Preferences.GetOrDefault( section, key, default( T ) );
      }

      public void SetSetting<T>( string section, string key, T value )
      {
         PluginEnvironment.Current.Preferences.Set( section, key, value );
      }
   }
}
