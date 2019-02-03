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
      public InitializationContext( IConfiguration config, HttpSecurity httpSecurity )
      {
         Config = config;
         HttpSecurity = httpSecurity;
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
   }
}
