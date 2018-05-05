using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using XUnity.AutoTranslator.Plugin.Core.Configuration;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   public static class AutoTranslateClient
   {
      private static KnownEndpoint _endpoint;
      private static int _runningTranslations = 0;
      private static int _translationCount;

      public static void Configure()
      {
         _endpoint = KnownEndpoints.FindEndpoint( Settings.ServiceEndpoint );

         if( _endpoint != null )
         {
            _endpoint.ConfigureServicePointManager();
         }
      }

      public static bool IsConfigured
      {
         get
         {
            return _endpoint != null;
         }
      }

      public static bool HasAvailableClients => _runningTranslations < Settings.MaxConcurrentTranslations;

      public static IEnumerator TranslateByWWW( string untranslated, string from, string to, Action<string> success, Action failure )
      {
         var url = _endpoint.GetServiceUrl( untranslated, from, to );
         var headers = new Dictionary<string, string>();
         _endpoint.ApplyHeaders( headers );

         using( var www = new WWW( url, null, headers ) )
         {
            _runningTranslations++;
            yield return www;
            _runningTranslations--;

            _translationCount++;
            if( Settings.MaxConcurrentTranslations == Settings.DefaultMaxConcurrentTranslations )
            {
               if( _translationCount > Settings.MaxTranslationsBeforeSlowdown )
               {
                  Settings.MaxConcurrentTranslations = 1;
                  Console.WriteLine( "[XUnity.AutoTranslator][WARN]: Maximum translations per session reached. Entering slowdown mode." );
               }
            }

            if( !Settings.IsShutdown )
            {
               if( _translationCount > Settings.MaxTranslationsBeforeShutdown )
               {
                  Settings.IsShutdown = true;
                  Console.WriteLine( "[XUnity.AutoTranslator][ERROR]: Maximum translations per session reached. Shutting plugin down." );
               }
            }


            string error = null;
            try
            {
               error = www.error;
            }
            catch( Exception e )
            {
               error = e.ToString();
            }

            if( error != null )
            {
               failure();
            }
            else
            {
               var text = www.text;
               if( text != null )
               {
                  try
                  {
                     if( _endpoint.TryExtractTranslated( text, out var translatedText ) )
                     {
                        translatedText = translatedText ?? string.Empty;
                        success( translatedText );
                     }
                     else
                     {
                        failure();
                     }
                  }
                  catch
                  {
                     failure();
                  }
               }
               else
               {
                  failure();
               }
            }
         }
      }
   }
}
