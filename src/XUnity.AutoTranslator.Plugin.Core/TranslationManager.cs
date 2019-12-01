using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Web;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core
{
   class TranslationManager
   {
      public event Action<TranslationJob> JobCompleted;
      public event Action<TranslationJob> JobFailed;

      private readonly List<TranslationEndpointManager> _endpointsWithUnstartedJobs;

      public TranslationManager()
      {
         _endpointsWithUnstartedJobs = new List<TranslationEndpointManager>();
         ConfiguredEndpoints = new List<TranslationEndpointManager>();
         AllEndpoints = new List<TranslationEndpointManager>();
      }

      public int OngoingTranslations { get; set; }

      public int UnstartedTranslations { get; set; }

      public List<TranslationEndpointManager> ConfiguredEndpoints { get; private set; }

      public List<TranslationEndpointManager> AllEndpoints { get; private set; }

      public TranslationEndpointManager CurrentEndpoint { get; set; }

      public void InitializeEndpoints( GameObject go )
      {
         try
         {
            var httpSecurity = new HttpSecurity();
            var context = new InitializationContext( httpSecurity, Settings.FromLanguage, Settings.Language );

            CreateEndpoints( go, context );

            AllEndpoints = AllEndpoints
               .OrderBy( x => x.Error != null )
               .ThenBy( x => x.Endpoint.FriendlyName )
               .ToList();

            var primaryEndpoint = AllEndpoints.FirstOrDefault( x => x.Endpoint.Id == Settings.ServiceEndpoint );
            if( primaryEndpoint != null )
            {
               if( primaryEndpoint.Error != null )
               {
                  XuaLogger.AutoTranslator.Error( primaryEndpoint.Error, "Error occurred during the initialization of the selected translate endpoint." );
               }
               else
               {
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
               if( callback != null && !Features.SupportsNet4x )
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

      public void CreateEndpoints( GameObject go, InitializationContext context )
      {
         if( Settings.FromLanguage != Settings.Language )
         {
            var dynamicTypes = AssemblyLoader.GetAllTypesOf<ITranslateEndpoint>( Settings.TranslatorsPath );

            // add built-in endpoint
            dynamicTypes.Add( typeof( PassthroughTranslateEndpoint ) );

            foreach( var type in dynamicTypes )
            {
               AddEndpoint( go, context, type );
            }
         }
         else
         {
            XuaLogger.AutoTranslator.Warn( "AutoTranslator has been configured to use same destination language as source language. All translators will be disabled!" );

            //// add built-in endpoint
            //AddEndpoint( go, context, typeof( PassthroughTranslateEndpoint ) );
         }
      }

      private void AddEndpoint( GameObject go, InitializationContext context, Type type )
      {
         ITranslateEndpoint endpoint;
         try
         {
            if( typeof( MonoBehaviour ).IsAssignableFrom( type ) )
            {
               // allow implementing plugins to hook into Unity lifecycle
               endpoint = (ITranslateEndpoint)go.AddComponent( type );
               UnityEngine.Object.DontDestroyOnLoad( (UnityEngine.Object)endpoint );
            }
            else
            {
               // or... just use any old object
               endpoint = (ITranslateEndpoint)Activator.CreateInstance( type );
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "Could not instantiate class: " + type.Name );
            return;
         }

         try
         {
            endpoint.Initialize( context );
            var manager = new TranslationEndpointManager( endpoint, null );
            RegisterEndpoint( manager );
         }
         catch( Exception e )
         {
            var manager = new TranslationEndpointManager( endpoint, e );
            RegisterEndpoint( manager );
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
