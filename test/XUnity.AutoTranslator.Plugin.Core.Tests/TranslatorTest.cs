using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.Common.Constants;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Tests
{
   public abstract class TranslatorTest
   {
      private static List<IMonoBehaviour_Update> _behavioursRequiringUpdates;

      static TranslatorTest()
      {
         _behavioursRequiringUpdates = new List<IMonoBehaviour_Update>();

         ServicePointManager.ServerCertificateValidationCallback += ( a1, a2, a3, a4 ) => true;
         TimeSupport.Time = new DotNetTime();
         UnityFeatures.SupportsWaitForSecondsRealtime = false;
         Paths.GameRoot = Environment.CurrentDirectory;

         ThreadPool.QueueUserWorkItem( Run );
      }

      public static void Run( object state )
      {
         try
         {
            while( true )
            {
               lock( _behavioursRequiringUpdates )
               {
                  foreach( var behaviour in _behavioursRequiringUpdates )
                  {
                     behaviour.Update();
                  }
               }

               Thread.Sleep( 20 );
            }
         }
         catch( System.Exception )
         {

         }
      }

      public static void Register( ITranslateEndpoint endpoint )
      {
         if(endpoint is IMonoBehaviour_Update requiresUpdates )
         {
            lock(_behavioursRequiringUpdates)
            {
               _behavioursRequiringUpdates.Add( requiresUpdates );
            }
         }
      }
   }

   public abstract class TranslatorTest<TTranslateEndpoint> : TranslatorTest
      where TTranslateEndpoint : ITranslateEndpoint, new()
   {
      private static TTranslateEndpoint _translator;

      public TTranslateEndpoint GetInitializedTranslator( string from, string to )
      {
         lock( this )
         {
            if( _translator == null )
            {
               _translator = new TTranslateEndpoint();
               var initializationContext = new TestInitializationContext( from, to );
               _translator.Initialize( initializationContext );

               Register( _translator );
            }
            return _translator;
         }
      }

      [Theory( DisplayName = "TestEndpoint" )]
      [InlineData( "ja", "en", "カラスの使い魔" )]
      [InlineData( "ja", "en", "ゲーム開始" )]
      public async Task TestEndpointAsync( string from, string to, string text )
      {
         var endpoint = GetInitializedTranslator( from, to );

         bool? succeeded = null;
         bool? failed = null;

         var translationContext = new TranslationContext(
            untranslatedTextInfos: new UntranslatedTextInfo[]
            {
               new UntranslatedTextInfo(text)
            },
            sourceLanguage: from,
            destinationLanguage: to,
            complete: translations =>
            {
               succeeded = true;
            },
            fail: ( msg, e ) =>
            {
               failed = true;
            } );

         await CoroutineSimulator.StartAsync( endpoint.Translate( translationContext ) );

         Assert.Null( failed );
         Assert.True( succeeded );
      }
   }
}
