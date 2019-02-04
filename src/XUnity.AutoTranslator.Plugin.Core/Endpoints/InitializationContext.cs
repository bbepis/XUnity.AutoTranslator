using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints
{
   public class InitializationContext
   {
      internal InitializationContext(
         IConfiguration config,
         HttpSecurity httpSecurity,
         string sourceLanguage,
         string destinationLanguage )
      {
         Config = config;
         HttpSecurity = httpSecurity;
         SourceLanguage = sourceLanguage;
         DestinationLanguage = destinationLanguage;
      }

      /// <summary>
      /// Gets the configuration of the plugin, including the directory where
      /// the configuration file is placed.
      /// </summary>
      public IConfiguration Config { get; }

      /// <summary>
      /// Gets the HttpSecurity class which enables setting up SSL for endpoints.
      /// </summary>
      public HttpSecurity HttpSecurity { get; }

      /// <summary>
      /// Gets the source language.
      /// </summary>
      public string SourceLanguage { get; }

      /// <summary>
      /// Gets the destination language.
      /// </summary>
      public string DestinationLanguage { get; }
   }
}
