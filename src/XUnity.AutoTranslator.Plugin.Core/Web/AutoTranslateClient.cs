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
      //private static readonly object Sync = new object();
      //private static readonly HashSet<WebClientReference> AvailableClients = new HashSet<WebClientReference>();
      //private static readonly HashSet<WebClientReference> WorkingClients = new HashSet<WebClientReference>();

      private static KnownEndpoint _endpoint;
      private static int _runningTranslations = 0;

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

      //private static int CurrentClientCount => AvailableClients.Count + WorkingClients.Count;

      public static bool HasAvailableClients => _runningTranslations < Settings.MaxConcurrentTranslations;

      public static IEnumerator TranslateByWWW( string untranslated, string from, string to, Action<string> success, Action failure )
      {
         var url = _endpoint.GetServiceUrl( WWW.EscapeURL( untranslated ), from, to );
         var headers = new Dictionary<string, string>();
         _endpoint.ApplyHeaders( headers );
         using( var www = new WWW( url, null, headers ) )
         {
            _runningTranslations++;
            yield return www;
            _runningTranslations--;

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
               string translatedText = null;
               var text = www.text;
               if( text != null )
               {
                  try
                  {
                     translatedText = _endpoint.ExtractTranslated( text ) ?? string.Empty;
                  }
                  catch { }
               }

               success( translatedText );
            }
         }
      }

      //public static bool TranslateByWebClient( string untranslated, string from, string to, Action<string> success, Action failure )
      //{
      //   var url = _endpoint.GetServiceUrl( untranslated, from, to );
      //   var reference = GetOrCreateAvailableClient();
      //   if( reference == null )
      //   {
      //      return false;
      //   }
      //   var client = reference.Client;

      //   _endpoint.ApplyHeaders( client.Headers );
      //   client.Encoding = Encoding.UTF8;

      //   Interlocked.Increment( ref _runningTranslations );
      //   DownloadStringCompletedEventHandler callback = null;
      //   callback = ( s, e ) =>
      //   {
      //      try
      //      {
      //         client.DownloadStringCompleted -= callback;

      //         string translatedText = null;
      //         bool failed = false;

      //         if( e.Error == null )
      //         {
      //            if( e.Result != null )
      //            {
      //               try
      //               {
      //                  translatedText = _endpoint.ExtractTranslated( e.Result ) ?? string.Empty;
      //               }
      //               catch { }
      //            }
      //         }
      //         else
      //         {
      //            failed = true;
      //         }

      //         if( failed )
      //         {
      //            failure();
      //         }
      //         else
      //         {
      //            success( translatedText );
      //         }
      //      }
      //      finally
      //      {
      //         ReleaseClientAfterUse( reference );
      //         Interlocked.Decrement( ref _runningTranslations );
      //      }
      //   };
      //   client.DownloadStringCompleted += callback;

      //   client.DownloadStringAsync( new Uri( url ) );

      //   return true;
      //}

      //public static void ReleaseClientAfterUse( WebClientReference client )
      //{
      //   lock( Sync )
      //   {
      //      var removed = WorkingClients.Remove( client );
      //      if( removed )
      //      {
      //         client.LastTimestamp = DateTime.UtcNow;
      //         AvailableClients.Add( client );
      //      }
      //   }
      //}

      //public static WebClientReference GetOrCreateAvailableClient()
      //{
      //   lock( Sync )
      //   {
      //      if( AvailableClients.Count > 0 )
      //      {
      //         // take a already configured client...
      //         var client = AvailableClients.First();
      //         AvailableClients.Remove( client );
      //         WorkingClients.Add( client );
      //         return client;
      //      }
      //      else if( CurrentClientCount < Settings.MaxConcurrentTranslations )
      //      {
      //         var client = new WebClient();
      //         var reference = new WebClientReference( client );
      //         WorkingClients.Add( reference );
      //         return reference;
      //      }
      //      else
      //      {
      //         return null;
      //      }
      //   }
      //}

      //public static void RemoveUnusedClients()
      //{
      //   lock( Sync )
      //   {
      //      var now = DateTime.UtcNow;
      //      var references = AvailableClients.ToList();
      //      foreach( var reference in references )
      //      {
      //         var livedFor = now - reference.LastTimestamp;
      //         if( livedFor > Settings.WebClientLifetime )
      //         {
      //            AvailableClients.Remove( reference );
      //            reference.Client.Dispose();
      //         }
      //      }
      //   }
      //}
   }
}
