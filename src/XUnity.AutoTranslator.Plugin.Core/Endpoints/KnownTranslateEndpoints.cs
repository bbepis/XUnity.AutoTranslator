using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Constants;
using XUnity.AutoTranslator.Plugin.Core.Web;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints
{
   internal static class KnownTranslateEndpoints
   {
      public static List<ConfiguredEndpoint> CreateEndpoints( GameObject go, InitializationContext context )
      {
         var endpoints = new List<ConfiguredEndpoint>();

         // could dynamically load types from other assemblies...
         //var integratedTypes = AssemblyLoader.GetAllTypesOf<ITranslateEndpoint>( typeof( KnownEndpoints ).Assembly );
         var pluginFolder = Path.Combine( PluginEnvironment.Current.DataPath, Settings.PluginFolder );
         var dynamicTypes = AssemblyLoader.GetAllTypesOf<ITranslateEndpoint>( pluginFolder );

         foreach( var type in dynamicTypes )
         {
            AddEndpoint( go, context, endpoints, type );
         }

         return endpoints;
      }

      private static void AddEndpoint( GameObject go, InitializationContext context, List<ConfiguredEndpoint> endpoints, Type type )
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
            XuaLogger.Current.Error( e, "Could not instantiate class: " + type.Name );
            return;
         }

         try
         {
            endpoint.Initialize( context );
            endpoints.Add( new ConfiguredEndpoint( endpoint, null ) );
         }
         catch( Exception e )
         {
            endpoints.Add( new ConfiguredEndpoint( endpoint, e ) );
         }
      }
   }
}
