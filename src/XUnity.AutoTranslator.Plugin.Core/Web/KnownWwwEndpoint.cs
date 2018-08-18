using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using Harmony;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;

namespace XUnity.AutoTranslator.Plugin.Core.Web
{
   public abstract class KnownWwwEndpoint : IKnownEndpoint
   {
      protected static readonly ConstructorInfo WwwConstructor = Constants.Types.WWW.GetConstructor( new[] { typeof( string ), typeof( byte[] ), typeof( Dictionary<string, string> ) } );

      private bool _isBusy = false;

      public KnownWwwEndpoint()
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
            object www = null;
            try
            {
               var headers = new Dictionary<string, string>();
               ApplyHeaders( headers );
               var url = GetServiceUrl( untranslatedText, from, to );
               www = WwwConstructor.Invoke( new object[] { url, null, headers } );
            }
            catch( Exception e )
            {
               Logger.Current.Error( e, "Error occurred while setting up translation request." );
            }

            if( www != null )
            {
               yield return www;

               try
               {
                  string error = null;
                  try
                  {
                     error = (string)AccessTools.Property( Constants.Types.WWW, "error" ).GetValue( www, null );
                  }
                  catch( Exception e )
                  {
                     error = e.ToString();
                  }

                  if( error != null )
                  {
                     Logger.Current.Error( "Error occurred while retrieving translation." + Environment.NewLine + error );
                     failure();
                  }
                  else
                  {
                     var text = (string)AccessTools.Property( Constants.Types.WWW, "text" ).GetValue( www, null );

                     if( text != null )
                     {
                        if( TryExtractTranslated( text, out var translatedText ) )
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
                        Logger.Current.Error( "Error occurred while extracting text from response." );
                        failure();
                     }
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
            _isBusy = false;
         }
      }

      public virtual void OnUpdate()
      {
      }

      public virtual bool ShouldGetSecondChanceAfterFailure()
      {
         return false;
      }

      public abstract string GetServiceUrl( string untranslatedText, string from, string to );

      public abstract void ApplyHeaders( Dictionary<string, string> headers );

      public abstract bool TryExtractTranslated( string result, out string translated );

      public virtual IEnumerator OnBeforeTranslate( int translationCount )
      {
         return null;
      }
   }
}
