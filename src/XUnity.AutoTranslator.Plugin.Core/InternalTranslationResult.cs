using System;
using System.Collections;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core
{
   /// <summary>
   /// Represents an ongoing translation request.
   /// </summary>
   class InternalTranslationResult : IEnumerator
   {
      private readonly Action<TranslationResult> _onCompleted;

      internal InternalTranslationResult( bool isGlobal, Action<TranslationResult> onCompleted )
      {
         IsGlobal = isGlobal;
         _onCompleted = onCompleted;
      }

      internal bool IsGlobal { get; private set; }

      /// <summary>
      /// Gets a bool indicating if the translation is completed through
      /// either failure or success.
      /// </summary>
      public bool IsCompleted { get; private set; }

      /// <summary>
      /// Gets the translated text if the translation succeeded.
      /// </summary>
      public string TranslatedText { get; private set; }

      /// <summary>
      /// Gets the error message if the translation failed.
      /// </summary>
      public string ErrorMessage { get; private set; }

      /// <summary>
      /// Gets a bool indicating if the translation resulted in an error.
      /// </summary>
      public bool HasError => ErrorMessage != null;

      internal void SetCompleted( string translatedText )
      {
         if( !IsCompleted )
         {
            IsCompleted = true;
            SetCompletedInternal( translatedText );
         }
      }

      internal void SetEmptyResponse()
      {
         SetError( "Received empty response." );
      }

      internal void SetErrorWithMessage( string errorMessage )
      {
         SetError( errorMessage );
      }

      private void SetError( string errorMessage )
      {
         if( !IsCompleted )
         {
            IsCompleted = true;
            SetErrorInternal( errorMessage );
         }
      }

      private void SetErrorInternal( string errorMessage )
      {
         ErrorMessage = errorMessage ?? "Unknown error";

         try
         {
            _onCompleted?.Invoke( new TranslationResult( null, ErrorMessage ) );
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while notifying of translation failure." );
         }
      }

      private void SetCompletedInternal( string translatedText )
      {
         TranslatedText = translatedText;

         try
         {
            _onCompleted?.Invoke( new TranslationResult( TranslatedText, null ) );
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while notifying of translation completion." );
         }
      }

      /// <summary>
      /// Current of IEnumerator interface.
      /// </summary>
      public object Current => null;

      /// <summary>
      /// MoveNext of IEnumerator interface.
      /// </summary>
      /// <returns>Returns true if the translation has not completed.</returns>
      public bool MoveNext()
      {
         return !IsCompleted;
      }

      /// <summary>
      /// Reset of IEnumerator interface.
      /// </summary>
      public void Reset()
      {
      }
   }
}
