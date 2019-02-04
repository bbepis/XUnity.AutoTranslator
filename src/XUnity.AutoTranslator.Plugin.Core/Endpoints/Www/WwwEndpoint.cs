using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;
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

      public abstract void CreateTranslationRequest( WwwTranslationContext context );

      public abstract void ExtractTranslatedText( WwwTranslationContext context );

      public virtual IEnumerator OnBeforeTranslate( WwwTranslationContext context ) => null;

      public int MaxConcurrency => 1;

      public IEnumerator Translate( TranslationContext context )
      {
         var wwwContext = new WwwTranslationContext( context );

         // allow implementer of HttpEndpoint to do anything before starting translation
         var setup = OnBeforeTranslate( wwwContext );
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
            CreateTranslationRequest( wwwContext );
            var url = wwwContext.ServiceUrl;
            var data = wwwContext.Data;
            var headers = wwwContext.Headers ?? new Dictionary<string, string>();

            // execute request
            www = WwwConstructor.Invoke( new object[] { url, data != null ? Encoding.UTF8.GetBytes( data ) : null, headers } );
         }
         catch( Exception e )
         {
            wwwContext.Fail( "Error occurred while setting up translation request.", e );
            yield break;
         }

         if( www == null )
         {
            wwwContext.Fail( "Unexpected error occurred while retrieving translation.", null );
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
               wwwContext.Fail( "Error occurred while retrieving translation." + Environment.NewLine + error, null );
               yield break;
            }

            // extract text
            var text = (string)AccessTools.Property( Constants.ClrTypes.WWW, "text" ).GetValue( www, null );
            if( text == null )
            {
               wwwContext.Fail( "Error occurred while extracting text from response.", null );
               yield break;
            }

            ExtractTranslatedText( wwwContext );
         }
         catch( Exception e )
         {
            wwwContext.Fail( "Error occurred while retrieving translation.", e );
         }
      }
   }
}
