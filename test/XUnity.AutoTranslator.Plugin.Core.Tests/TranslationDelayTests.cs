using ExIni;
using Xunit;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;

namespace XUnity.AutoTranslator.Plugin.Core.Tests
{
   public class TranslationDelayTests
   {
      [Theory]
      [InlineData( null, 0.9f, "0.9" )]
      [InlineData( "0.1", 0.1f, "0.1" )]
      [InlineData( "0.25", 0.25f, "0.25" )]
      [InlineData( "0.9", 0.9f, "0.9" )]
      [InlineData( "2", 2f, "2" )]
      [InlineData( "", 0.9f, "0.9" )]
      [InlineData( "0", 0.9f, "0.9" )]
      [InlineData( "-1", 0.9f, "0.9" )]
      [InlineData( "0.09", 0.9f, "0.9" )]
      [InlineData( "NaN", 0.9f, "0.9" )]
      [InlineData( "Infinity", 0.9f, "0.9" )]
      [InlineData( "-Infinity", 0.9f, "0.9" )]
      [InlineData( "invalid", 0.9f, "0.9" )]
      public void Can_Read_Default_Translation_Delay( string configuredValue, float expectedDelay, string expectedValue )
      {
         var preferences = new IniFile();
         if( configuredValue != null )
         {
            preferences[ "Behaviour" ][ "DefaultTranslationDelay" ].Value = configuredValue;
         }

         var delay = Settings.ReadDefaultTranslationDelay( preferences );

         Assert.Equal( expectedDelay, delay );
         Assert.Equal( expectedValue, preferences[ "Behaviour" ][ "DefaultTranslationDelay" ].Value );
      }

      [Theory]
      [InlineData( "0.1", false, 0.1f, 600 )]
      [InlineData( "0.9", false, 0.9f, 66 )]
      [InlineData( "2", false, 2f, 30 )]
      [InlineData( "0.1", true, 0.5f, 120 )]
      public void Can_Initialize_Endpoint_With_Configured_Delay( string configuredValue, bool overrideDelay, float expectedDelay, int expectedRetries )
      {
         var previousDelay = Settings.DefaultTranslationDelay;
         try
         {
            var preferences = new IniFile();
            preferences[ "Behaviour" ][ "DefaultTranslationDelay" ].Value = configuredValue;
            Settings.DefaultTranslationDelay = Settings.ReadDefaultTranslationDelay( preferences );

            var context = new InitializationContext( null, "en", "ru" );
            if( overrideDelay )
            {
               context.SetTranslationDelay( 0.5f );
            }
            var manager = new TranslationEndpointManager( new PassthroughTranslateEndpoint(), null, context );

            Assert.Equal( expectedDelay, context.TranslationDelay );
            Assert.Equal( expectedDelay, manager.TranslationDelay );
            Assert.Equal( expectedRetries, manager.MaxRetries );
            Assert.True( manager.EnableSpamChecks );
         }
         finally
         {
            Settings.DefaultTranslationDelay = previousDelay;
         }
      }
   }
}
