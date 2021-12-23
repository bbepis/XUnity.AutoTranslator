using System;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;

namespace XUnity.AutoTranslator.Plugin.Core.Tests
{
   public partial class TestInitializationContext : IInitializationContext
   {
      private object _config;

      public TestInitializationContext( string sourceLanguage, string destinationLanguage )
      {
         SourceLanguage = sourceLanguage;
         DestinationLanguage = destinationLanguage;

         _config = new
         {
            DeepLLegitimate = new
            {
               ExecutableLocation = "Common.ExtProtocol.Executor.exe"
            },
            DeepL = new
            {
               ExecutableLocation = "Common.ExtProtocol.Executor.exe"
            }
         };
      }

      public string PluginDirectory => throw new NotImplementedException();
      public string TranslatorDirectory => Environment.CurrentDirectory;
      public string SourceLanguage { get; }
      public string DestinationLanguage { get; }

      public void DisableCertificateChecksFor( params string[] hosts )
      {
         // we are disabling all checks in tests
      }

      public void DisableSpamChecks()
      {

      }

      public T GetOrCreateSetting<T>( string section, string key, T defaultValue )
      {
         var sectionPropertyInfo = _config.GetType().GetProperty( section );
         if( sectionPropertyInfo == null ) return defaultValue;

         var sectionValue = sectionPropertyInfo.GetValue( _config );
         if( sectionValue == null ) return defaultValue;

         var keyPropertyInfo = sectionValue.GetType().GetProperty( key );
         if( keyPropertyInfo == null ) return defaultValue;

         var value = keyPropertyInfo.GetValue( sectionValue );

         return (T)value;
      }

      public T GetOrCreateSetting<T>( string section, string key )
      {
         return GetOrCreateSetting<T>( section, key, default );
      }

      public void SetSetting<T>( string section, string key, T value )
      {
         // nothing
      }

      public void SetTranslationDelay( float delayInSeconds )
      {

      }
   }
}
