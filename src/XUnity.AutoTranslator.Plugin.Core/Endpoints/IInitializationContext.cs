namespace XUnity.AutoTranslator.Plugin.Core.Endpoints
{
   /// <summary>
   /// Interface used in context of initializing a translator plugin.
   /// </summary>
   public interface IInitializationContext
   {
      /// <summary>
      /// Gets the directory where the configuration file and translations are stored.
      /// </summary>
      string PluginDirectory { get; }

      /// <summary>
      /// Gets or creates the specified setting.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="section"></param>
      /// <param name="key"></param>
      /// <param name="defaultValue"></param>
      /// <returns></returns>
      T GetOrCreateSetting<T>( string section, string key, T defaultValue );

      /// <summary>
      /// Gets or creates the specified setting.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="section"></param>
      /// <param name="key"></param>
      /// <returns></returns>
      T GetOrCreateSetting<T>( string section, string key );

      /// <summary>
      /// Sets the specified setting.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="section"></param>
      /// <param name="key"></param>
      /// <param name="value"></param>
      void SetSetting<T>( string section, string key, T value );

      /// <summary>
      /// Disables the certificate check for the specified hostnames.
      /// </summary>
      /// <param name="hosts"></param>
      void DisableCertificateChecksFor( params string[] hosts );

      /// <summary>
      /// Gets the source language that the plugin is configured with.
      /// </summary>
      string SourceLanguage { get; }

      /// <summary>
      /// Gets the destination language that the plugin is configured with.
      /// </summary>
      string DestinationLanguage { get; }
   }
}
