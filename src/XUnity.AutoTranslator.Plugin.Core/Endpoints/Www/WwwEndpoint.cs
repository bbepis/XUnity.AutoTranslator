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

      public int MaxConcurrency => 1;

      public abstract void Initialize( IInitializationContext context );

      public abstract void OnCreateRequest( IWwwRequestCreationContext context );

      public abstract void OnExtractTranslation( IWwwTranslationExtractionContext context );

      public virtual IEnumerator OnBeforeTranslate( IWwwTranslationContext context ) => null;

      public IEnumerator Translate( ITranslationContext context )
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
            OnCreateRequest( wwwContext );
            if( wwwContext.RequestInfo == null )
            {
               wwwContext.Fail( "No request object was provided by the translator.", null );
               yield break;
            }

            var request = wwwContext.RequestInfo;
            var url = request.Address;
            var data = request.Data;
            var headers = request.Headers;

            // execute request
            www = WwwConstructor.Invoke( new object[] { request.Address, data != null ? Encoding.UTF8.GetBytes( data ) : null, headers } );
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

            wwwContext.ResponseData = text;

            OnExtractTranslation( wwwContext );
         }
         catch( Exception e )
         {
            wwwContext.Fail( "Error occurred while retrieving translation.", e );
         }
      }
   }
}
