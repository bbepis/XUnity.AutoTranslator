using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using Harmony;
using SimpleJSON;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Extensions;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   public class BingTranslateEndpoint : KnownHttpEndpoint
   {
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

         // Configure service points / service point manager
         ServicePointManager.ServerCertificateValidationCallback += Security.AlwaysAllowByHosts( "www.bing.com" );
         SetupServicePoints( "https://www.bing.com" );
      }

      public override bool SupportsLineSplitting => false;

      public override void ApplyHeaders( WebHeaderCollection headers )
      {
         headers[ HttpRequestHeader.UserAgent ] = Settings.GetUserAgent( DefaultUserAgent );

         if( AcceptLanguage != null )
         {
            headers[ HttpRequestHeader.AcceptLanguage ] = AcceptLanguage;
         }
         if( Accept != null )
         {
            headers[ HttpRequestHeader.Accept ] = Accept;
         }
         if( Referer != null )
         {
            headers[ HttpRequestHeader.Referer ] = Referer;
         }
         if( Origin != null )
         {
            headers[ "Origin" ] = Origin;
         }
         if( AcceptCharset != null )
         {
            headers[ HttpRequestHeader.AcceptCharset ] = AcceptCharset;
         }
         if( ContentType != null )
         {
            headers[ HttpRequestHeader.ContentType ] = ContentType;
         }
      }

      public override IEnumerator OnBeforeTranslate( int translationCount )
      {
         if( !_hasSetup || translationCount % _resetAfter == 0 )
         {
            _resetAfter = RandomNumbers.Next( 75, 125 );
            _hasSetup = true;

            // Setup TKK and cookies
            var enumerator = SetupIGAndIID();
            while( enumerator.MoveNext() )
            {
               yield return enumerator.Current;
            }
         }
      }

      public IEnumerator SetupIGAndIID()
      {
         string error = null;
         DownloadResult downloadResult = null;

         _cookieContainer = new CookieContainer();
         _translationCount = 0;

         var client = GetClient();
         try
         {
            ApplyHeaders( client.Headers );
            client.Headers.Remove( HttpRequestHeader.Referer );
            client.Headers.Remove( "Origin" );
            client.Headers.Remove( HttpRequestHeader.ContentType );
            downloadResult = client.GetDownloadResult( new Uri( HttpsTranslateUserSite ) );
         }
         catch( Exception e )
         {
            error = e.ToString();
         }

         if( downloadResult != null )
         {
            if( Features.SupportsCustomYieldInstruction )
            {
               yield return downloadResult;
            }
            else
            {
               while( !downloadResult.IsCompleted )
               {
                  yield return new WaitForSeconds( 0.2f );
               }
            }

            error = downloadResult.Error;
            if( downloadResult.Succeeded && downloadResult.Result != null )
            {
               try
               {
                  var html = downloadResult.Result;

                  _ig = Lookup( "ig\":\"", html );
                  _iid = Lookup( ".init(\"/feedback/submission?\",\"", html );

                  if( _ig == null || _iid == null )
                  {
                     Logger.Current.Warn( "An error occurred while setting up BingTranslate IG/IID. Proceeding without..." );
                  }
               }
               catch( Exception e )
               {
                  error = e.ToString();
               }
            }
         }

         if( error != null )
         {
            Logger.Current.Warn( "An error occurred while setting up BingTranslate IG. Proceeding without..." + Environment.NewLine + error );
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

      public override bool TryExtractTranslated( string result, out string translated )
      {
         try
         {
            var obj = JSON.Parse( result );

            var code = obj[ "statusCode" ].AsInt;
            if( code != 200 )
            {
               translated = null;
               return false;
            }

            var token = obj[ "translationResponse" ].ToString();
            token = token.Substring( 1, token.Length - 2 ).UnescapeJson();

            translated = token;

            var success = !string.IsNullOrEmpty( translated );
            return success;
         }
         catch
         {
            translated = null;
            return false;
         }
      }

      public override string GetRequestObject( string untranslatedText, string from, string to )
      {
         return string.Format( RequestTemplate, WWW.EscapeURL( untranslatedText ), from, to );

      }

      public override string GetServiceUrl( string untranslatedText, string from, string to )
      {
         _translationCount++;

         if( _ig == null || _iid == null )
         {
            return HttpsServicePointTemplateUrlWithoutIG;
         }
         else
         {
            return string.Format( HttpsServicePointTemplateUrl, _ig, _iid, _translationCount );
         }
      }

      public override void WriteCookies( HttpWebResponse response )
      {
         _cookieContainer.Add( response.Cookies );
      }

      public override CookieContainer ReadCookies()
      {
         return _cookieContainer;
      }
   }
}
