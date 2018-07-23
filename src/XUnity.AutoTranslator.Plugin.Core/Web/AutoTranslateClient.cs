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
      private static readonly ConstructorInfo WwwConstructor = Types.WWW.GetConstructor( new[] { typeof( string ), typeof( byte[] ), typeof( Dictionary<string, string> ) } );

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
         while( true )
         {
            var url = _endpoint.GetServiceUrl( untranslated, from, to );
            var headers = new Dictionary<string, string>();
            _endpoint.ApplyHeaders( headers );

            var failed = false;
            object www = WwwConstructor.Invoke( new object[] { url, null, headers } );
            try
            {
               _runningTranslations++;
               yield return www;
               _runningTranslations--;

               _translationCount++;
               //if( Settings.MaxConcurrentTranslations == Settings.DefaultMaxConcurrentTranslations )
               //{
               //   if( _translationCount > Settings.MaxTranslationsBeforeSlowdown )
               //   {
               //      Settings.MaxConcurrentTranslations = 1;
               //      Console.WriteLine( "[XUnity.AutoTranslator][WARN]: Maximum translations per session reached. Entering slowdown mode." );
               //   }
               //}

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
                  error = (string)AccessTools.Property( Types.WWW, "error" ).GetValue( www, null );
               }
               catch( Exception e )
               {
                  error = e.ToString();
               }

               if( error != null )
               {
                  failed = true;
               }
               else
               {
                  var text = (string)AccessTools.Property( Types.WWW, "text" ).GetValue( www, null ); ;
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
                           failed = true;
                        }
                     }
                     catch
                     {
                        failed = true;
                     }
                  }
                  else
                  {
                     failed = true;
                  }
               }
            }
            finally
            {
               var disposable = www as IDisposable;
               if( disposable != null )
               {
                  disposable.Dispose();
               }
            }

            if( failed )
            {
               if( ( _endpoint as ISupportFallback )?.Fallback() == true )
               {
                  // we can attempt with fallback strategy, simply retry
               }
               else
               {
                  failure();
                  break;
               }
            }
            else
            {
               break;
            }
         }
      }
   }
}
