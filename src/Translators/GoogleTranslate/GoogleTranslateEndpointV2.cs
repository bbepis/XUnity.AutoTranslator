using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using SimpleJSON;
using XUnity.AutoTranslator.Plugin.Core;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Endpoints.Http;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Core.Web;
using XUnity.Common.Extensions;
using XUnity.Common.Logging;

namespace GoogleTranslate
{
   public class GoogleTranslateEndpointV2 : HttpEndpoint
   {
      private static readonly char[] WordSplitters = new char[] { ' ', '\r', '\n' };
      private static readonly HashSet<string> SupportedLanguages = new HashSet<string>
      {
         "auto","af","sq","am","ar","hy","az","eu","be","bn","bs","bg","ca","ceb","zh-CN","zh-TW","co","hr","cs","da","nl","en","eo","et","fi","fr","fy","gl","ka","de","el","gu","ht","ha","haw","he","hi","hmn","hu","is","ig","id","ga","it","ja","jw","kn","kk","km","ko","ku","ky","lo","la","lv","lt","lb","mk","mg","ms","ml","mt","mi","mr","mn","my","ne","no","ny","ps","fa","pl","pt","pa","ro","ru","sm","gd","sr","st","sn","sd","si","sk","sl","so","es","su","sw","sv","tl","tg","ta","te","th","tr","uk","ur","uz","vi","cy","xh","yi","yo","zu"
      };

      private static readonly string DefaultUserBackend = "https://translate.google.com";
      private static readonly string TranslationPostTemplate = "[[[\"{0}\",\"[[\\\"{1}\\\",\\\"{2}\\\",\\\"{3}\\\",true],[null]]\",null,\"generic\"]]]";

      private static readonly string HttpsServicePointTranslateTemplateUrl = "/_/TranslateWebserverUi/data/batchexecute";
      private static readonly Random RandomNumbers = new Random();

      private static readonly string[] AcceptLanguages = new string[] { null, "en-US,en;q=0.9", "en-US", "en" };

      private static readonly string AcceptLanguage = AcceptLanguages[ RandomNumbers.Next( AcceptLanguages.Length ) ];

      private string _selectedUserBackend;
      private string _httpsServicePointTranslateTemplateUrl;

      private CookieContainer _cookieContainer;
      private bool _hasSetup = false;
      private long _FSID = LongRandom( long.MinValue, long.MaxValue, RandomNumbers );
      private int _translationCount = 0;
      private int _resetAfter = RandomNumbers.Next( 75, 125 );

      private string _translateRpcId;
      private string _version;
      private bool _useSimplestSuggestion;
      private long _reqId;

      public GoogleTranslateEndpointV2()
      {
         _cookieContainer = new CookieContainer();
      }

      private static long LongRandom( long min, long max, Random rand )
      {
         byte[] buf = new byte[ 8 ];
         rand.NextBytes( buf );
         long longRand = BitConverter.ToInt64( buf, 0 );

         return ( Math.Abs( longRand % ( max - min ) ) + min );
      }

      public override string Id => KnownTranslateEndpointNames.GoogleTranslateV2;

      public override string FriendlyName => "Google! Translate (v2)";

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
         var backendOverride = context.GetOrCreateSetting<string>( "GoogleV2", "ServiceUrl" );
         if( !backendOverride.IsNullOrWhiteSpace() )
         {
            _selectedUserBackend = backendOverride;

            _httpsServicePointTranslateTemplateUrl = _selectedUserBackend + HttpsServicePointTranslateTemplateUrl;

            XuaLogger.AutoTranslator.Info( "The default backend for google translate was overwritten." );
         }
         else
         {
            _selectedUserBackend = DefaultUserBackend;

            _httpsServicePointTranslateTemplateUrl = _selectedUserBackend + HttpsServicePointTranslateTemplateUrl;
         }

         _translateRpcId = context.GetOrCreateSetting( "GoogleV2", "RPCID", "MkEWBc" );
         _version = context.GetOrCreateSetting( "GoogleV2", "VERSION", "boq_translate-webserver_20210323.10_p0" );
         _useSimplestSuggestion = context.GetOrCreateSetting( "GoogleV2", "UseSimplest", false );


         context.DisableCertificateChecksFor( new Uri( _selectedUserBackend ).Host, new Uri( _selectedUserBackend ).Host );

         if( !SupportedLanguages.Contains( FixLanguage( context.SourceLanguage ) ) ) throw new EndpointInitializationException( $"The source language '{context.SourceLanguage}' is not supported." );
         if( !SupportedLanguages.Contains( FixLanguage( context.DestinationLanguage ) ) ) throw new EndpointInitializationException( $"The destination language '{context.DestinationLanguage}' is not supported." );
      }

      public override IEnumerator OnBeforeTranslate( IHttpTranslationContext context )
      {
         if( !_hasSetup || _translationCount % _resetAfter == 0 )
         {
            _resetAfter = RandomNumbers.Next( 75, 125 );
            _translationCount = 1;
            _reqId = RandomNumbers.Next( 0, 100000 ); //maybe divide by 10 and multiply?

            _hasSetup = true;

            // Setup TKK and cookies
            var enumerator = SetupFSID();
            while( enumerator.MoveNext() )
            {
               yield return enumerator.Current;
            }
         }
      }

      public override void OnCreateRequest( IHttpRequestCreationContext context )
      {
         _translationCount++;

         var allUntranslatedText =
            JsonHelper.Escape( JsonHelper.Escape( string.Join( "\n", context.UntranslatedTexts ) ) );

         var query = string.Join( "&", new[]
         {
            "rpcids=" + _translateRpcId,
            "f.sid=" + _FSID.ToString(CultureInfo.InvariantCulture),
            "bl=" + Uri.EscapeDataString(_version),
            "hl=en-US",
            "soc-app=1",
            "soc-platform=1",
            "soc-device=1",
            "_reqid=" + _reqId.ToString(CultureInfo.InvariantCulture),
            "rt=c"
         } );

         var data = "f.req=" + Uri.EscapeDataString( string.Format( TranslationPostTemplate, _translateRpcId, allUntranslatedText, FixLanguage( context.SourceLanguage ), FixLanguage( context.DestinationLanguage ) ) ) + "&";

         var url = _httpsServicePointTranslateTemplateUrl + "?" + query;

         var request = new XUnityWebRequest( "POST", url, data );

         request.Cookies = _cookieContainer;
         AddHeaders( request, true );

         _reqId += 100000;

         context.Complete( request );
      }

      public override void OnExtractTranslation( IHttpTranslationExtractionContext context )
      {
         var data = context.Response.Data;

         // output is some odd form of chunked-encoding. We just look at the first chunk
         data = data.Substring( 6 );
         var chars = data.Substring( 0, data.IndexOf( "\n" ) );
         var num = int.Parse( chars, CultureInfo.InvariantCulture );
         data = data.Substring( chars.Length, num );

         var outerArr = JSON.Parse( data );
         var innerJsonString = outerArr.AsArray[ 0 ].AsArray[ 2 ].ToString();
         var innerJson = innerJsonString.Substring( 1, innerJsonString.Length - 2 );
         var escapedJson = JsonHelper.Unescape( innerJson );

         var arr = JSON.Parse( escapedJson );
         arr = arr.AsArray[ 1 ].AsArray[ 0 ].AsArray[ 0 ].AsArray[ 5 ];
         var lineBuilder = new StringBuilder( escapedJson.Length );

         foreach( JSONNode entry in arr.AsArray )
         {
            var translationArray = entry.AsArray;
            var token = translationArray[ 0 ].ToString();
            token = JsonHelper.Unescape( token.Substring( 1, token.Length - 2 ) );
            if( token.IsNullOrWhiteSpace() )
               continue;

            if( _useSimplestSuggestion && translationArray.Count > 1 )
            {
               var alternativeTranslationsArray = translationArray[ 1 ]?.AsArray;
               if( alternativeTranslationsArray != null )
               {
                  var translations = new HashSet<string>();
                  translations.Add( token );

                  for( int i = 0; i < alternativeTranslationsArray.Count; i++ )
                  {
                     var alternativeToken = alternativeTranslationsArray[ i ].ToString();
                     alternativeToken = JsonHelper.Unescape( alternativeToken.Substring( 1, alternativeToken.Length - 2 ) );
                     translations.Add( alternativeToken );
                  }

                  if( translations.Count > 1 )
                  {
                     XuaLogger.AutoTranslator.Debug( "[GoogleTranslateV2]: Primary translation is '" + token + "', but found multiple suggestion:" );
                     foreach( var translation in translations )
                     {
                        XuaLogger.AutoTranslator.Debug( "[GoogleTranslateV2]: " + translation );
                     }

                     var wordsInPrimary = token.Split( WordSplitters ).Length;
                     token = translations
                        .Where( x => x.Split( WordSplitters ).Length < wordsInPrimary )
                        .OrderBy( x => x.Split( WordSplitters ).Length )
                        .FirstOrDefault() ?? token;

                     XuaLogger.AutoTranslator.Debug( "[GoogleTranslateV2]: Selecting translation: " + token );
                  }
               }
            }


            if( !lineBuilder.EndsWithWhitespaceOrNewline() ) lineBuilder.Append( '\n' );

            lineBuilder.Append( token );
         }

         var allTranslation = lineBuilder.ToString();
         if( context.UntranslatedTexts.Length == 1 )
         {
            context.Complete( allTranslation );
         }
         else
         {
            var translatedLines = allTranslation.Split( '\n' );
            var translatedTexts = new List<string>();

            int current = 0;
            foreach( var untranslatedText in context.UntranslatedTexts )
            {
               var untranslatedLines = untranslatedText.Split( '\n' );
               var untranslatedLinesCount = untranslatedLines.Length;
               var translatedText = string.Empty;

               for( int i = 0; i < untranslatedLinesCount; i++ )
               {
                  if( current >= translatedLines.Length ) context.Fail( "Batch operation received incorrect number of translations." );

                  var translatedLine = translatedLines[ current++ ];
                  translatedText += translatedLine;

                  if( i != untranslatedLinesCount - 1 ) translatedText += '\n';
               }

               translatedTexts.Add( translatedText );
            }

            if( current != translatedLines.Length ) context.Fail( "Batch operation received incorrect number of translations." );

            context.Complete( translatedTexts.ToArray() );
         }
      }

      private XUnityWebRequest CreateWebSiteRequest()
      {
         var request = new XUnityWebRequest( _selectedUserBackend );

         request.Cookies = _cookieContainer;
         AddHeaders( request, false );

         return request;
      }

      private void AddHeaders( XUnityWebRequest request, bool isTranslationRequest )
      {
         request.Headers[ HttpRequestHeader.UserAgent ] = string.IsNullOrEmpty( AutoTranslatorSettings.UserAgent ) ? UserAgents.Chrome_Win10_Latest : AutoTranslatorSettings.UserAgent;
         if( AcceptLanguage != null )
         {
            request.Headers[ HttpRequestHeader.AcceptLanguage ] = AcceptLanguage;
         }

         if( isTranslationRequest )
         {
            request.Headers[ HttpRequestHeader.Referer ] = _selectedUserBackend + "/";
            request.Headers[ "X-Same-Domain" ] = "1";
            request.Headers[ "DNT" ] = "1";
            request.Headers[ HttpRequestHeader.ContentType ] = "application/x-www-form-urlencoded;charset=UTF-8";
            request.Headers[ HttpRequestHeader.Accept ] = "*/*";
            request.Headers[ "Origin" ] = _selectedUserBackend;
         }
         else
         {
            request.Headers[ "Upgrade-Insecure-Requests" ] = "1";
         }
      }

      public IEnumerator SetupFSID()
      {
         XUnityWebResponse response = null;

         _cookieContainer = new CookieContainer();

         try
         {
            var client = new XUnityWebClient();
            var request = CreateWebSiteRequest();
            response = client.Send( request );
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Warn( e, "An error occurred while setting up GoogleTranslate FSID. Using random instead." );
            yield break;
         }

         // wait for response completion
         var iterator = response.GetSupportedEnumerator();
         while( iterator.MoveNext() ) yield return iterator.Current;

         if( response.IsTimedOut )
         {
            XuaLogger.AutoTranslator.Warn( "A timeout error occurred while setting up GoogleTranslate FSID. Using random instead." );
            yield break;
         }

         // failure
         if( response.Error != null )
         {
            XuaLogger.AutoTranslator.Warn( response.Error, "An error occurred while setting up GoogleTranslate FSID. Using random instead." );
            yield break;
         }

         // failure
         if( response.Data == null )
         {
            XuaLogger.AutoTranslator.Warn( null, "An error occurred while setting up GoogleTranslate FSID. Using random instead." );
            yield break;
         }

         try
         {
            var html = response.Data;

            bool found = false;
            string[] lookups = new[] { "FdrFJe\":\"" };
            foreach( var lookup in lookups )
            {
               var index = html.IndexOf( lookup );
               if( index > -1 ) // simple string approach
               {
                  var startIndex = index + lookup.Length;
                  var endIndex = html.IndexOf( "\"", startIndex );
                  var result = html.Substring( startIndex, endIndex - startIndex );

                  _FSID = long.Parse( result, CultureInfo.InvariantCulture );
                  found = true;
                  break;
               }
            }

            if( !found )
            {
               XuaLogger.AutoTranslator.Warn( "An error occurred while setting up GoogleTranslate FSID. Could not locate FSID value. Using random instead." );
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Warn( e, "An error occurred while setting up GoogleTranslate FSID. Using random instead." );
         }
      }
   }
}
