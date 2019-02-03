using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using Harmony;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints.Www
{
   public abstract class WwwEndpoint : ITranslateEndpoint
   {
      protected static readonly ConstructorInfo WwwConstructor = Constants.ClrTypes.WWW.GetConstructor( new[] { typeof( string ), typeof( byte[] ), typeof( Dictionary<string, string> ) } );

      public abstract string Id { get; }

      public abstract string FriendlyName { get; }

      public abstract void Initialize( InitializationContext context );

      public abstract string GetServiceUrl( string untranslatedText, string from, string to );

      public abstract void ApplyHeaders( Dictionary<string, string> headers );

      public abstract bool TryExtractTranslated( string result, out string translated );

      public virtual IEnumerator OnBeforeTranslate() => null;

      public int MaxConcurrency => 1;

      public IEnumerator Translate( string untranslatedText, string from, string to, Action<string> success, Action<string, Exception> failure )
      {
         // allow implementer of HttpEndpoint to do anything before starting translation
         var setup = OnBeforeTranslate();
         if( setup != null )
         {
            while( setup.MoveNext() )
            {
               yield return setup.Current;
            }
         }

         object www = null;
         try
         {
            // prepare request
            var headers = new Dictionary<string, string>();
            ApplyHeaders( headers );
            var url = GetServiceUrl( untranslatedText, from, to );

            // execute request
            www = WwwConstructor.Invoke( new object[] { url, null, headers } );
         }
         catch( Exception e )
         {
            failure( "Error occurred while setting up translation request.", e );
            yield break;
         }

         if( www == null )
         {
            failure( "Unexpected error occurred while retrieving translation.", null );
            yield break;
         }

         // wait for completion
         yield return www;

         try
         {
            // extract error
            string error = null;
            try
            {
               error = (string)AccessTools.Property( Constants.ClrTypes.WWW, "error" ).GetValue( www, null );
            }
            catch( Exception e )
            {
               error = e.ToString();
            }

            if( error != null )
            {
               failure( "Error occurred while retrieving translation." + Environment.NewLine + error, null );
               yield break;
            }

            // extract text
            var text = (string)AccessTools.Property( Constants.ClrTypes.WWW, "text" ).GetValue( www, null );
            if( text == null )
            {
               failure( "Error occurred while extracting text from response.", null );
               yield break;
            }

            // attempt to extract translation from data
            if( TryExtractTranslated( text, out var translatedText ) )
            {
               translatedText = translatedText ?? string.Empty;
               success( translatedText );
            }
            else
            {
               failure( "Error occurred while extracting translation.", null );
            }
         }
         catch( Exception e )
         {
            failure( "Error occurred while retrieving translation.", e );
         }
      }
   }
}
