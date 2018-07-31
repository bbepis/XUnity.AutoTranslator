using System;
using System.Collections;
using System.Net;
using XUnity.AutoTranslator.Plugin.Core.Configuration;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   public abstract class KnownHttpEndpoint : IKnownEndpoint
   {
      private static readonly TimeSpan MaxUnusedLifespan = TimeSpan.FromSeconds( 20 );

      private static int _runningTranslations = 0;
      private static int _maxConcurrency = 1;
      private static bool _isSettingUp = false;
      private UnityWebClient _client;
      private DateTime _clientLastUse = DateTime.UtcNow;

      public KnownHttpEndpoint()
      {
      }

      public bool IsBusy => _isSettingUp || _runningTranslations >= _maxConcurrency;

      public IEnumerator Translate( string untranslatedText, string from, string to, Action<string> success, Action failure )
      {
         _clientLastUse = DateTime.UtcNow;

         try
         {
            _isSettingUp = true;

            var setup = OnBeforeTranslate( Settings.TranslationCount );
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

         var client = GetClient();
         var url = GetServiceUrl( untranslatedText, from, to );
         Logger.Current.Debug( "Starting translation for: " + untranslatedText );

         DownloadResult result = null;
         try
         {
            ApplyHeaders( client.Headers );
            result = client.GetDownloadResult( new Uri( url ) );
         }
         catch( Exception e )
         {
            Logger.Current.Error( e, "Error occurred while retrieving translation." );
         }

         if( result != null )
         {
            try
            {
               Logger.Current.Debug( "Yielding for translation." );
               _runningTranslations++;
               yield return result;
               _runningTranslations--;
               Logger.Current.Debug( "Yield completed." );

               try
               {
                  if( result.Succeeded )
                  {
                     if( TryExtractTranslated( result.Result, out var translatedText ) )
                     {
                        Logger.Current.Debug( $"Translation for '{untranslatedText}' succeded. Result: {translatedText}" );

                        translatedText = translatedText ?? string.Empty;
                        success( translatedText );
                     }
                     else
                     {
                        Logger.Current.Error( "Error occurred while extracting translation." );
                        failure();
                     }
                  }
                  else
                  {
                     Logger.Current.Error( "Error occurred while retrieving translation." + Environment.NewLine + result.Error );
                     failure();
                  }
               }
               catch( Exception e )
               {
                  Logger.Current.Error( e, "Error occurred while retrieving translation." );
                  failure();
               }
            }
            finally
            {
               _clientLastUse = DateTime.UtcNow;
            }
         }
      }

      public virtual void OnUpdate()
      {
         var client = _client;
         if( client != null && DateTime.UtcNow - _clientLastUse > MaxUnusedLifespan )
         {
            _client = null;
            client.Dispose();
         }
      }

      public virtual bool ShouldGetSecondChanceAfterFailure()
      {
         return false;
      }

      public abstract string GetServiceUrl( string untranslatedText, string from, string to );

      public abstract void ApplyHeaders( WebHeaderCollection headers );

      public abstract bool TryExtractTranslated( string result, out string translated );

      public virtual void WriteCookies( HttpWebResponse response )
      {

      }

      public virtual CookieContainer ReadCookies()
      {
         return null;
      }

      public virtual IEnumerator OnBeforeTranslate( int translationCount )
      {
         return null;
      }

      public UnityWebClient GetClient()
      {
         if( _client == null )
         {
            _client = new UnityWebClient( this );
            _clientLastUse = DateTime.UtcNow;
         }
         return _client;
      }
   }
}
