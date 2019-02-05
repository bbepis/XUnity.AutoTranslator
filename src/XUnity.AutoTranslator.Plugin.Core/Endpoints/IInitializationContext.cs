namespace XUnity.AutoTranslator.Plugin.Core.Endpoints
{
   public interface IInitializationContext
   {
      string PluginDirectory { get; }

      T GetOrCreateSetting<T>( string section, string key, T defaultValue );
      T GetOrCreateSetting<T>( string section, string key );

      void EnableSslFor( params string[] hosts );

      string SourceLanguage { get; }
      string DestinationLanguage { get; }
   }
}
