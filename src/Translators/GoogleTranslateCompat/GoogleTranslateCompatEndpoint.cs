using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using SimpleJSON;
using XUnity.AutoTranslator.Plugin.Core;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Endpoints.ExtProtocol;
using XUnity.AutoTranslator.Plugin.Core.Endpoints.Http;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Core.Web;
using XUnity.Common.Logging;

namespace GoogleTranslateCompat
{
   internal class GoogleTranslateCompatEndpoint : ExtProtocolEndpoint
   {
      private static readonly HashSet<string> SupportedLanguages = new HashSet<string>
      {
         "auto","af","sq","am","ar","hy","az","eu","be","bn","bs","bg","ca","ceb","zh-CN","zh-TW","co","hr","cs","da","nl","en","eo","et","fi","fr","fy","gl","ka","de","el","gu","ht","ha","haw","he","hi","hmn","hu","is","ig","id","ga","it","ja","jw","kn","kk","km","ko","ku","ky","lo","la","lv","lt","lb","mk","mg","ms","ml","mt","mi","mr","mn","my","ne","no","ny","ps","fa","pl","pt","pa","ro","ru","sm","gd","sr","st","sn","sd","si","sk","sl","so","es","su","sw","sv","tl","tg","ta","te","th","tr","uk","ur","uz","vi","cy","xh","yi","yo","zu"
      };

      public override string Id => "GoogleTranslateCompat";

      public override string FriendlyName => "Google! Translate (Compat)";

      public override int MaxConcurrency => 1;

      public override int MaxTranslationsPerRequest => 10;

      private string FixLanguage( string lang )
      {
         switch( lang )
         {
            case "zh-Hans":
            case "zh":
               return "zh-CN";
            case "zh-Hant":
               return "zh-TW";
            default:
               return lang;
         }
      }

      public override void Initialize( IInitializationContext context )
      {
         base.Initialize( context );

         if( !SupportedLanguages.Contains( FixLanguage( context.SourceLanguage ) ) ) throw new EndpointInitializationException( $"The source language '{context.SourceLanguage}' is not supported." );
         if( !SupportedLanguages.Contains( FixLanguage( context.DestinationLanguage ) ) ) throw new EndpointInitializationException( $"The destination language '{context.DestinationLanguage}' is not supported." );

         Arguments = Convert.ToBase64String( Encoding.UTF8.GetBytes( "GoogleTranslateCompat.ExtProtocol.GoogleTranslateCompatTranslate, GoogleTranslateCompat.ExtProtocol" ) );
      }
   }
}
