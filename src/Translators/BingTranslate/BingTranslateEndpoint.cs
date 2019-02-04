using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using SimpleJSON;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Endpoints.Http;
using XUnity.AutoTranslator.Plugin.Core.Extensions;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace BingTranslate
{
   internal class BingTranslateEndpoint : HttpEndpoint
   {
      private static readonly HashSet<string> SupportedLanguages = new HashSet<string>
      {
         "af","ar","bn","bs","bg","yue","ca","zh-Hans","zh-Hant","hr","cs","da","nl","en","et","fj","fil","fi","fr","de","el","ht","he","hi","mww","hu","is","id","it","ja","sw","tlh","tlh-Qaak","ko","lv","lt","mg","ms","mt","nb","fa","pl","pt","otq","ro","ru","sm","sr-Cyrl","sr-Latn","sk","sl","es","sv","ty","ta","te","th","to","tr","uk","ur","vi","cy","yua"
      };

      private static readonly string HttpsServicePointTemplateUrl = "https://www.bing.com/ttranslate?&category=&IG={0}&IID={1}.{2}";
      private static readonly string HttpsServicePointTemplateUrlWithoutIG = "https://www.bing.com/ttranslate?&category=";
      private static readonly string HttpsTranslateUserSite = "https://www.bing.com/translator";
      private static readonly string RequestTemplate = "&text={0}&from={1}&to={2}";
      private static readonly string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36";
      private static readonly System.Random RandomNumbers = new System.Random();

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

      public override void Initialize( InitializationContext context )
      {
         // Configure service points / service point manager
         context.HttpSecurity.EnableSslFor( "www.bing.com" );

         if( !SupportedLanguages.Contains( context.DestinationLanguage ) ) throw new Exception( $"The destination language {context.DestinationLanguage} is not supported." );
      }

      public override XUnityWebRequest CreateTranslationRequest( HttpTranslationContext context )
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
            WWW.EscapeURL( context.UntranslatedText ),
            context.SourceLanguage,
            context.DestinationLanguage );

         var request = new XUnityWebRequest( "POST", address, data );

         request.Cookies = _cookieContainer;
         AddHeaders( request, true );

         return request;
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
         request.Headers[ HttpRequestHeader.UserAgent ] = string.IsNullOrEmpty( AutoTranslationState.UserAgent ) ? DefaultUserAgent : AutoTranslationState.UserAgent;

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

      public override void InspectTranslationResponse( HttpTranslationContext context, XUnityWebResponse response )
      {
         CookieCollection cookies = response.NewCookies;

         // FIXME: Is this needed? Should already be added
         _cookieContainer.Add( cookies );
      }

      public override IEnumerator OnBeforeTranslate( HttpTranslationContext context )
      {
         if( !_hasSetup || AutoTranslationState.TranslationCount % _resetAfter == 0 )
         {
            _resetAfter = RandomNumbers.Next( 75, 125 );
            _hasSetup = true;

            // Setup TKK and cookies
            var enumerator = SetupIGAndIID( context );
            while( enumerator.MoveNext() )
            {
               yield return enumerator.Current;
            }
         }
      }

      public IEnumerator SetupIGAndIID( HttpTranslationContext context )
      {
         XUnityWebResponse response = null;

         _cookieContainer = new CookieContainer();
         _translationCount = 0;

         try
         {
            var client = new XUnityWebClient();
            var request = CreateWebSiteRequest();
            response = client.Send( request );
         }
         catch( Exception e )
         {
            XuaLogger.Current.Warn( e, "An error occurred while setting up BingTranslate IG. Proceeding without..." );
            yield break;
         }

         if( Features.SupportsCustomYieldInstruction )
         {
            yield return response;
         }
         else
         {
            while( response.keepWaiting )
            {
               yield return new WaitForSeconds( 0.2f );
            }
         }

         InspectTranslationResponse( context, response );

         // failure
         if( response.Error != null )
         {
            XuaLogger.Current.Warn( response.Error, "An error occurred while setting up BingTranslate IG. Proceeding without..." );
            yield break;
         }

         // failure
         if( response.Result == null )
         {
            XuaLogger.Current.Warn( null, "An error occurred while setting up BingTranslate IG. Proceeding without..." );
            yield break;
         }

         try
         {
            var html = response.Result;

            _ig = Lookup( "ig\":\"", html );
            _iid = Lookup( ".init(\"/feedback/submission?\",\"", html );

            if( _ig == null || _iid == null )
            {
               XuaLogger.Current.Warn( "An error occurred while setting up BingTranslate IG/IID. Proceeding without..." );
            }
         }
         catch( Exception e )
         {
            XuaLogger.Current.Warn( e, "An error occurred while setting up BingTranslate IG. Proceeding without..." );
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

      public override void ExtractTranslatedText( HttpTranslationContext context )
      {
         var obj = JSON.Parse( context.ResultData );

         var code = obj[ "statusCode" ].AsInt;
         if( code != 200 )
         {
            return;
         }

         var token = obj[ "translationResponse" ].ToString();
         token = token.Substring( 1, token.Length - 2 ).UnescapeJson();

         var translated = token;

         context.Complete( translated );
      }
   }
}
