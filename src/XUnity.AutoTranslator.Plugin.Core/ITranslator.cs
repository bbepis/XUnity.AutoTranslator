using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core
{
   static class AutoTranslator
   {
      public static ITranslator Default => AutoTranslationPlugin.Current;
   }

   interface ITranslator
   {
      TranslationResult Translate( TranslationEndpointManager endpoint, string untranslatedText );
   }

   class TranslationResult : IEnumerator
   {
      public event Action<string> Completed;
      public event Action<string> Error;

      internal bool IsCompleted { get; private set; }

      public string TranslatedText { get; private set; }

      public string ErrorMessage { get; private set; }

      public void SetCompleted( string translatedText, bool delay )
      {
         if( !IsCompleted )
         {
            IsCompleted = true;

            if( delay )
            {
               CoroutineHelper.Start( SetCompletedAfterDelay( translatedText ) );
            }
            else
            {
               SetCompletedInternal( translatedText );
            }
         }
      }

      public void SetEmptyResponse( bool delay )
      {
         SetError( "Received empty response.", delay );
      }

      public void SetErrorWithMessage( string errorMessage, bool delay )
      {
         SetError( errorMessage, delay );
      }

      private void SetError( string errorMessage, bool delay )
      {
         if( !IsCompleted )
         {
            IsCompleted = true;

            if( delay )
            {
               CoroutineHelper.Start( SetErrorAfterDelay( errorMessage ) );
            }
            else
            {
               SetErrorInternal( errorMessage );
            }
         }
      }

      private IEnumerator SetErrorAfterDelay( string errorMessage )
      {
         yield return null;

         SetErrorInternal( errorMessage );
      }

      private void SetErrorInternal( string errorMessage )
      {
         ErrorMessage = errorMessage;

         try
         {
            Error?.Invoke( errorMessage );
         }
         catch( Exception e )
         {
            XuaLogger.Current.Error( e, "An error occurred while notifying of translation failure." );
         }
      }

      private IEnumerator SetCompletedAfterDelay( string translatedText )
      {
         yield return null;

         SetCompletedInternal( translatedText );
      }

      private void SetCompletedInternal( string translatedText )
      {
         TranslatedText = translatedText;

         try
         {
            Completed?.Invoke( translatedText );
         }
         catch( Exception e )
         {
            XuaLogger.Current.Error( e, "An error occurred while notifying of translation completion." );
         }
      }

      public object Current => null;

      public bool MoveNext()
      {
         return !IsCompleted;
      }

      public void Reset()
      {
      }
   }
}
