using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints
{
   internal static class KnownEndpoints
   {
      public static List<ConfiguredEndpoint> CreateEndpoints( GameObject go, ServiceEndpointConfiguration servicePoints )
      {
         var endpoints = new List<ConfiguredEndpoint>();

         // could dynamically load types from other assemblies...
         var types = typeof( KnownEndpoints ).Assembly
            .GetTypes()
            .Where( x => typeof( ITranslateEndpoint ).IsAssignableFrom( x ) && !x.IsAbstract && !x.IsInterface )
            .ToList();

         foreach( var type in types )
         {
            AddEndpoint( go, servicePoints, endpoints, type );
         }

         return endpoints;
      }

      private static void AddEndpoint( GameObject go, ServiceEndpointConfiguration servicePoints, List<ConfiguredEndpoint> endpoints, Type type )
      {
         ITranslateEndpoint endpoint;
         try
         {
            if( typeof( MonoBehaviour ).IsAssignableFrom( type ) )
            {
               // allow implementing plugins to hook into Unity lifecycle
               endpoint = (ITranslateEndpoint)go.AddComponent( type );
            }
            else
            {
               // or... just use any old object
               endpoint = (ITranslateEndpoint)Activator.CreateInstance( type );
            }
         }
         catch( Exception e )
         {
            Logger.Current.Error( e, "Could not instantiate class: " + type.Name );
            return;
         }

         try
         {
            endpoint.Initialize( Config.Current, servicePoints );
            endpoints.Add( new ConfiguredEndpoint( endpoint, null ) );
         }
         catch( Exception e )
         {
            endpoints.Add( new ConfiguredEndpoint( endpoint, e ) );
         }
      }
   }
}
