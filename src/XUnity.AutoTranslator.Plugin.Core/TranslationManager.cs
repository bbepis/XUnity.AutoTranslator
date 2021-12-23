using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Web;
using XUnity.Common.Constants;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core
{
   class TranslationManager
   {
      public event Action<TranslationJob> JobCompleted;
      public event Action<TranslationJob> JobFailed;

      private readonly List<IMonoBehaviour_Update> _updateCallbacks;
      private readonly List<TranslationEndpointManager> _endpointsWithUnstartedJobs;

      public TranslationManager()
      {
         _updateCallbacks = new List<IMonoBehaviour_Update>();
         _endpointsWithUnstartedJobs = new List<TranslationEndpointManager>();
         ConfiguredEndpoints = new List<TranslationEndpointManager>();
         AllEndpoints = new List<TranslationEndpointManager>();
      }

      public int OngoingTranslations { get; set; }

      public int UnstartedTranslations { get; set; }

      public List<TranslationEndpointManager> ConfiguredEndpoints { get; private set; }

      public List<TranslationEndpointManager> AllEndpoints { get; private set; }

      public TranslationEndpointManager CurrentEndpoint { get; set; }

      public TranslationEndpointManager FallbackEndpoint { get; set; }

      public TranslationEndpointManager PassthroughEndpoint { get; private set; }

      public bool IsFallbackAvailableFor(TranslationEndpointManager endpoint)
      {
         return endpoint != null && FallbackEndpoint != null
            && endpoint == CurrentEndpoint
            && FallbackEndpoint != endpoint;
      }

      public void InitializeEndpoints()
      {
         try
         {
            var httpSecurity = new HttpSecurity();

            CreateEndpoints( httpSecurity );

            AllEndpoints = AllEndpoints
               .OrderBy( x => x.Error != null )
               .ThenBy( x => x.Endpoint.FriendlyName )
               .ToList();

            PassthroughEndpoint = AllEndpoints.FirstOrDefault( x => x.Endpoint is PassthroughTranslateEndpoint );

            var fallbackEndpoint = AllEndpoints.FirstOrDefault( x => x.Endpoint.Id == Settings.FallbackServiceEndpoint );
            if( fallbackEndpoint != null )
            {
               if( fallbackEndpoint.Error != null )
               {
                  XuaLogger.AutoTranslator.Error( fallbackEndpoint.Error, "Error occurred during the initialization of the fallback translate endpoint." );
               }
               else
               {
                  FallbackEndpoint = fallbackEndpoint;
               }
            }

            var primaryEndpoint = AllEndpoints.FirstOrDefault( x => x.Endpoint.Id == Settings.ServiceEndpoint );
            if( primaryEndpoint != null )
            {
               if( primaryEndpoint.Error != null )
               {
                  XuaLogger.AutoTranslator.Error( primaryEndpoint.Error, "Error occurred during the initialization of the selected translate endpoint." );
               }
               else
               {
                  if( fallbackEndpoint == primaryEndpoint )
                  {
                     XuaLogger.AutoTranslator.Warn( "Cannot use same fallback endpoint as primary." );
                  }
                  CurrentEndpoint = primaryEndpoint;
               }
            }
            else if( !string.IsNullOrEmpty( Settings.ServiceEndpoint ) )
            {
               XuaLogger.AutoTranslator.Error( $"Could not find the configured endpoint '{Settings.ServiceEndpoint}'." );
            }


            if( Settings.DisableCertificateValidation )
            {
               XuaLogger.AutoTranslator.Debug( $"Disabling certificate checks for endpoints because of configuration." );

               ServicePointManager.ServerCertificateValidationCallback += ( a1, a2, a3, a4 ) => true;
            }
            else
            {
               var callback = httpSecurity.GetCertificateValidationCheck();
               if( callback != null && !ClrFeatures.SupportsNet4x )
               {
                  XuaLogger.AutoTranslator.Debug( $"Disabling certificate checks for endpoints because a .NET 3.x runtime is used." );

                  ServicePointManager.ServerCertificateValidationCallback += callback;
               }
               else
               {
                  XuaLogger.AutoTranslator.Debug( $"Not disabling certificate checks for endpoints because a .NET 4.x runtime is used." );
               }
            }

            // save config because the initialization phase of plugins may have changed the config
            Settings.Save();
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while constructing endpoints. Shutting plugin down." );

            Settings.IsShutdown = true;
         }
      }

      public void CreateEndpoints( HttpSecurity httpSecurity )
      {
         if( Settings.FromLanguage != Settings.Language )
         {
            var dynamicTypes = AssemblyLoader.GetAllTypesOf<ITranslateEndpoint>( Settings.TranslatorsPath );

            // add built-in endpoint
            dynamicTypes.Add( typeof( PassthroughTranslateEndpoint ) );

            foreach( var type in dynamicTypes )
            {
               AddEndpoint( httpSecurity, type );
            }
         }
         else
         {
            XuaLogger.AutoTranslator.Warn( "AutoTranslator has been configured to use same destination language as source language. All translators will be disabled!" );

            //// add built-in endpoint
            //AddEndpoint( go, context, typeof( PassthroughTranslateEndpoint ) );
         }
      }

      private void AddEndpoint( HttpSecurity httpSecurity, Type type )
      {
         ITranslateEndpoint endpoint;
         try
         {
            endpoint = (ITranslateEndpoint)Activator.CreateInstance( type );
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "Could not instantiate class: " + type.Name );
            return;
         }

         var context = new InitializationContext( httpSecurity, Settings.FromLanguage, Settings.Language );
         try
         {
            endpoint.Initialize( context );
            var manager = new TranslationEndpointManager( endpoint, null, context );
            RegisterEndpoint( manager );
         }
         catch( Exception e )
         {
            var manager = new TranslationEndpointManager( endpoint, e, context );
            RegisterEndpoint( manager );
         }
      }

      public void Update()
      {
         var len = _updateCallbacks.Count;
         for( int i = 0; i < len; i++ )
         {
            try
            {
               _updateCallbacks[ i ].Update();
            }
            catch( Exception e )
            {
               XuaLogger.AutoTranslator.Error( e, "An error occurred while calling update on " + _updateCallbacks[ i ].GetType().Name + "." );
            }
         }
      }

      public void KickoffTranslations()
      {
         // iterate in reverse order so we can remove from list while iterating
         var endpoints = _endpointsWithUnstartedJobs;

         for( int i = endpoints.Count - 1; i >= 0; i-- )
         {
            var endpoint = endpoints[ i ];

            if( Settings.EnableBatching && endpoint.CanBatch )
            {
               while( endpoint.HasUnstartedBatch )
               {
                  if( endpoint.IsBusy ) break;

                  endpoint.HandleNextBatch();
               }
            }
            else
            {
               while( endpoint.HasUnstartedJob )
               {
                  if( endpoint.IsBusy ) break;

                  endpoint.HandleNextJob();
               }
            }
         }
      }

      public void ScheduleUnstartedJobs( TranslationEndpointManager endpoint )
      {
         _endpointsWithUnstartedJobs.Add( endpoint );
      }

      public void UnscheduleUnstartedJobs( TranslationEndpointManager endpoint )
      {
         _endpointsWithUnstartedJobs.Remove( endpoint );
      }

      public void ClearAllJobs()
      {
         foreach( var endpoint in ConfiguredEndpoints )
         {
            endpoint.ClearAllJobs();
         }
      }

      public void RebootAllEndpoints()
      {
         foreach( var endpoint in ConfiguredEndpoints )
         {
            endpoint.ConsecutiveErrors = 0;
         }
      }

      public void RegisterEndpoint( TranslationEndpointManager translationEndpointManager )
      {
         translationEndpointManager.Manager = this;
         AllEndpoints.Add( translationEndpointManager );
         if( translationEndpointManager.Error == null )
         {
            ConfiguredEndpoints.Add( translationEndpointManager );
         }

         if( translationEndpointManager.Endpoint is IMonoBehaviour_Update updatable )
         {
            _updateCallbacks.Add( updatable );
         }
      }

      public void InvokeJobCompleted( TranslationJob job )
      {
         JobCompleted?.Invoke( job );
      }

      public void InvokeJobFailed( TranslationJob job )
      {
         JobFailed?.Invoke( job );
      }
   }
}
