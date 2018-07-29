using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using Harmony;
using UnityEngine.Networking;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   public static class AutoTranslateClient
   {
      public static KnownEndpoint Endpoint;
      private static int _runningTranslations = 0;
      private static int _translationCount;
      private static int _maxConcurrency;
      private static bool _isSettingUp = false;

      public static void Configure()
      {
         Endpoint = KnownEndpoints.FindEndpoint( Settings.ServiceEndpoint );
         _maxConcurrency = 1; // WebClient does not support concurrency

         if( Endpoint != null )
         {
            Endpoint.ConfigureServicePointManager();
         }
      }

      public static bool IsConfigured
      {
         get
         {
            return Endpoint != null;
         }
      }

      public static bool HasAvailableClients => !_isSettingUp && _runningTranslations < _maxConcurrency;

      public static bool Fallback()
      {
         return Endpoint.Fallback();
      }

      public static IEnumerator TranslateByWWW( string untranslated, string from, string to, Action<string> success, Action failure )
      {
         UnityWebClient.TouchedDefault();

         try
         {
            _isSettingUp = true;

            var setup = Endpoint.Setup( _translationCount );
            if( setup != null )
            {
               while( setup.MoveNext() )
               {
                  yield return setup.Current;
               }
            }
         }
         finally
         {
            _isSettingUp = false;
         }

         var url = Endpoint.GetServiceUrl( untranslated, from, to );
         Endpoint.ApplyHeaders( UnityWebClient.Default.Headers );
         var result = UnityWebClient.Default.GetDownloadResult( new Uri( url ) );
         try
         {
            _runningTranslations++;
            yield return result;
            _runningTranslations--;

            _translationCount++;
            if( !Settings.IsSlowdown )
            {
               if( _translationCount > Settings.MaxTranslationsBeforeSlowdown )
               {
                  Settings.IsSlowdown = true;
                  _maxConcurrency = 1;

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


            if( result.Succeeded )
            {
               if( Endpoint.TryExtractTranslated( result.Result, out var translatedText ) )
               {
                  translatedText = translatedText ?? string.Empty;
                  success( translatedText );
               }
               else
               {
                  Console.WriteLine( "[XUnity.AutoTranslator][ERROR]: Error occurred while extracting translation." );
                  failure();
               }
            }
            else
            {
               Console.WriteLine( "[XUnity.AutoTranslator][ERROR]: Error occurred while retrieving translation." + Environment.NewLine + result.Error );
               failure();
            }
         }
         finally
         {
            UnityWebClient.TouchedDefault();
         }
      }
   }
}
