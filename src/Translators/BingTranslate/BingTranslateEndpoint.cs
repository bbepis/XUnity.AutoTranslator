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
using XUnity.Common.Logging;

namespace BingTranslate
{
   public class BingTranslateEndpoint : HttpEndpoint
   {
      private static readonly HashSet<string> SupportedLanguages = new HashSet<string>
      {
         "auto-detect","af","ar","bn","bs","bg","yue","ca","zh-Hans","zh-Hant","hr","cs","da","nl","en","et","fj","fil","fi","fr","de","el","ht","he","hi","mww","hu","is","id","it","ja","sw","tlh","tlh-Qaak","ko","lv","lt","mg","ms","mt","nb","fa","pl","pt","otq","ro","ru","sm","sr-Cyrl","sr-Latn","sk","sl","es","sv","ty","ta","te","th","to","tr","uk","ur","vi","cy","yua"
      };

      private static readonly string HttpsServicePointTemplateUrl = "https://www.bing.com/ttranslatev3?isVertical=1&&IG={0}&IID={1}.{2}"; // "https://www.bing.com/ttranslate?&category=&IG={0}&IID={1}.{2}";
      private static readonly string HttpsServicePointTemplateUrlWithoutIG = "https://www.bing.com/ttranslatev3?isVertical=1"; // "https://www.bing.com/ttranslate?&category="; // FIX THIS
      private static readonly string HttpsTranslateUserSite = "https://www.bing.com/translator";
      private static readonly string RequestTemplate = "&fromLang={1}&text={0}&to={2}"; //"&text={0}&from={1}&to={2}";
      private static readonly Random RandomNumbers = new Random();

      private static readonly string[] Accepts = new string[] { "*/*" };
      private static readonly string[] AcceptLanguages = new string[] { null, "en-US,en;q=0.9", "en-US", "en" };
      private static readonly string[] Referers = new string[] { "https://bing.com/translator" };
      private static readonly string[] Origins = new string[] { "https://www.bing.com" };
      private static readonly string[] AcceptCharsets = new string[] { null, Encoding.UTF8.WebName };
      private static readonly string[] ContentTypes = new string[] { "application/x-www-form-urlencoded" };

      private static readonly string Accept = Accepts[ RandomNumbers.Next( Accepts.Length ) ];
      private static readonly string AcceptLanguage = AcceptLanguages[ RandomNumbers.Next( AcceptLanguages.Length ) ];
      private static readonly string Referer = Referers[ RandomNumbers.Next( Referers.Length ) ];
      private static readonly string Origin = Origins[ RandomNumbers.Next( Origins.Length ) ];
      private static readonly string AcceptCharset = AcceptCharsets[ RandomNumbers.Next( AcceptCharsets.Length ) ];
      private static readonly string ContentType = ContentTypes[ RandomNumbers.Next( ContentTypes.Length ) ];

      private CookieContainer _cookieContainer;
      private bool _hasSetup = false;
      private string _ig;
      private string _iid;
      private int _translationCount = 0;
      private int _resetAfter = RandomNumbers.Next( 75, 125 );

      public BingTranslateEndpoint()
      {
         _cookieContainer = new CookieContainer();
      }

      public override string Id => "BingTranslate";

      public override string FriendlyName => "Bing Translator";

      private string FixLanguage( string lang )
      {
         switch( lang )
         {
            case "zh-CN":
            case "zh":
               return "zh-Hans";
            case "zh-TW":
               return "zh-Hant";
            case "auto":
               return "auto-detect";
            default:
               return lang;
         }
      }

      public override void Initialize( IInitializationContext context )
      {
         // Configure service points / service point manager
         context.DisableCertificateChecksFor( "www.bing.com" );

         if( !SupportedLanguages.Contains( FixLanguage( context.SourceLanguage ) ) ) throw new EndpointInitializationException( $"The source language '{context.SourceLanguage}' is not supported." );
         if( !SupportedLanguages.Contains( FixLanguage( context.DestinationLanguage ) ) ) throw new EndpointInitializationException( $"The destination language '{context.DestinationLanguage}' is not supported." );
      }

      public override IEnumerator OnBeforeTranslate( IHttpTranslationContext context )
      {
         if( !_hasSetup || _translationCount % _resetAfter == 0 )
         {
            _resetAfter = RandomNumbers.Next( 75, 125 );
            _translationCount = 0;

            _hasSetup = true;

            // Setup TKK and cookies
            var enumerator = SetupIGAndIID();
            while( enumerator.MoveNext() ) yield return enumerator.Current;
         }
      }

      public override void OnCreateRequest( IHttpRequestCreationContext context )
      {
         _translationCount++;

         string address = null;
         if( _ig == null || _iid == null )
         {
            address = HttpsServicePointTemplateUrlWithoutIG;
         }
         else
         {
            address = string.Format( HttpsServicePointTemplateUrl, _ig, _iid, _translationCount );
         }

         var data = string.Format(
            RequestTemplate,
            Uri.EscapeDataString( context.UntranslatedText ),
            FixLanguage( context.SourceLanguage ),
            FixLanguage( context.DestinationLanguage ) );

         var request = new XUnityWebRequest( "POST", address, data );

         request.Cookies = _cookieContainer;
         AddHeaders( request, true );

         context.Complete( request );
      }

      public override void OnInspectResponse( IHttpResponseInspectionContext context )
      {
         InspectResponse( context.Response );
      }

      public override void OnExtractTranslation( IHttpTranslationExtractionContext context )
      {
         var arr = JSON.Parse( context.Response.Data );

         var obj = arr.AsArray[ 0 ];

         var translations = obj[ "translations" ];
         var translation = translations.AsArray[ 0 ];

         var token = translation[ "text" ].ToString();
         var translatedText = JsonHelper.Unescape( token.Substring( 1, token.Length - 2 ) );

         context.Complete( translatedText );
      }

      private XUnityWebRequest CreateWebSiteRequest()
      {
         var request = new XUnityWebRequest( HttpsTranslateUserSite );

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
         if( Accept != null )
         {
            request.Headers[ HttpRequestHeader.Accept ] = Accept;
         }
         if( Referer != null && isTranslationRequest )
         {
            request.Headers[ HttpRequestHeader.Referer ] = Referer;
         }
         if( Origin != null && isTranslationRequest )
         {
            request.Headers[ "Origin" ] = Origin;
         }
         if( AcceptCharset != null )
         {
            request.Headers[ HttpRequestHeader.AcceptCharset ] = AcceptCharset;
         }
         if( ContentType != null && isTranslationRequest )
         {
            request.Headers[ HttpRequestHeader.ContentType ] = ContentType;
         }
      }

      private void InspectResponse( XUnityWebResponse response )
      {
         CookieCollection cookies = response.NewCookies;

         // FIXME: Is this needed? Should already be added
         _cookieContainer.Add( cookies );
      }

      public IEnumerator SetupIGAndIID()
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
            XuaLogger.AutoTranslator.Warn( e, "An error occurred while setting up BingTranslate IG. Proceeding without..." );
            yield break;
         }

         var iterator = response.GetSupportedEnumerator();
         while( iterator.MoveNext() ) yield return iterator.Current;

         if( response.IsTimedOut )
         {
            XuaLogger.AutoTranslator.Warn( "A timeout error occurred while setting up BingTranslate IG. Proceeding without..." );
            yield break;
         }

         // failure
         if( response.Error != null )
         {
            XuaLogger.AutoTranslator.Warn( response.Error, "An error occurred while setting up BingTranslate IG. Proceeding without..." );
            yield break;
         }

         // failure
         if( response.Data == null )
         {
            XuaLogger.AutoTranslator.Warn( null, "An error occurred while setting up BingTranslate IG. Proceeding without..." );
            yield break;
         }

         InspectResponse( response );

         try
         {
            var html = response.Data;

            _ig = Lookup( "\",IG:\"", html );
            _iid = Lookup( "data-iid=\"", html );

            if( _ig == null || _iid == null )
            {
               XuaLogger.AutoTranslator.Warn( "An error occurred while setting up BingTranslate IG/IID. Proceeding without..." );
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Warn( e, "An error occurred while setting up BingTranslate IG. Proceeding without..." );
         }
      }

      private string Lookup( string lookFor, string html )
      {
         var index = html.IndexOf( lookFor );
         if( index > -1 ) // simple string approach
         {
            var startIndex = index + lookFor.Length;
            var endIndex = html.IndexOf( "\"", startIndex );

            if( startIndex > -1 && endIndex > -1 )
            {
               var result = html.Substring( startIndex, endIndex - startIndex );

               return result;
            }
         }
         return null;
      }
   }
}
