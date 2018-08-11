using System;
using System.Collections;
using System.Net;
using System.Threading;
using XUnity.AutoTranslator.Plugin.Core.Configuration;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   public abstract class KnownHttpEndpoint : IKnownEndpoint
   {
      private static readonly TimeSpan MaxUnusedLifespan = TimeSpan.FromSeconds( 50 );

      private ServicePoint[] _servicePoints;
      private bool _isBusy = false;
      private UnityWebClient _client;
      private DateTime? _clientLastUse = null;

      public KnownHttpEndpoint()
      {
      }

      public bool IsBusy => _isBusy;

      public virtual bool SupportsLineSplitting
      {
         get
         {
            return false;
         }
      }

      protected void SetupServicePoints( params string[] endpoints )
      {
         _servicePoints = new ServicePoint[ endpoints.Length ];
         
         for( int i = 0 ; i < endpoints.Length ; i++ )
         {
            var endpoint = endpoints[ i ];
            var servicePoint = ServicePointManager.FindServicePoint( new Uri( endpoint ) );
            _servicePoints[ i ] = servicePoint;
         }
      }

      public IEnumerator Translate( string untranslatedText, string from, string to, Action<string> success, Action failure )
      {
         _isBusy = true;
         try
         {
            var setup = OnBeforeTranslate( Settings.TranslationCount );
            if( setup != null )
            {
               while( setup.MoveNext() )
               {
                  yield return setup.Current;
               }
            }
            Logger.Current.Debug( "Starting translation for: " + untranslatedText );
            DownloadResult result = null;
            try
            {
               var client = GetClient();
               var url = GetServiceUrl( untranslatedText, from, to );
               ApplyHeaders( client.Headers );
               result = client.GetDownloadResult( new Uri( url ) );
            }
            catch( Exception e )
            {
               Logger.Current.Error( e, "Error occurred while setting up translation request." );
            }

            if( result != null )
            {
               yield return result;

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
            else
            {
               failure();
            }
         }
         finally
         {
            _clientLastUse = DateTime.UtcNow;
            _isBusy = false;
         }
      }

      public virtual void OnUpdate()
      {
         if( !_isBusy && _clientLastUse.HasValue && DateTime.UtcNow - _clientLastUse > MaxUnusedLifespan && !_client.IsBusy
            && _servicePoints != null && _servicePoints.Length > 0 )
         {
            Logger.Current.Debug( $"Closing service points because they were not used for {(int)MaxUnusedLifespan.TotalSeconds} seconds." );

            _isBusy = true;
            _clientLastUse = null;

            ThreadPool.QueueUserWorkItem( delegate ( object state )
            {
               // never do a job like this on the game loop thread
               try
               {
                  foreach( var servicePoint in _servicePoints )
                  {
                     servicePoint.CloseConnectionGroup( MyWebClient.ConnectionGroupName );
                  }
               }
               finally
               {
                  _isBusy = false;
               }
            } );
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
