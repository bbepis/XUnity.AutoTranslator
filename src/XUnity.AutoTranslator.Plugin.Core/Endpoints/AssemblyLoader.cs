using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints
{
   internal static class AssemblyLoader
   {
      internal static List<Type> GetAllTypesOf<TService>( string directory )
      {
         if( !Directory.Exists( directory ) ) return new List<Type>();

         var files = Directory.GetFiles( directory, "*.dll" );
         var allTypes = new HashSet<Type>();

         foreach( var file in files )
         {
            try
            {
               var assemblyName = AssemblyName.GetAssemblyName( file );
               var assembly = Assembly.Load( assemblyName );
               var types = GetAllTypesOf<TService>( assembly );

               foreach( var type in types )
               {
                  allTypes.Add( type );
               }
            }
            catch( Exception e )
            {
               XuaLogger.Current.Error( e, "An error occurred while loading types in assembly: " + file );
            }
         }

         return allTypes.ToList();
      }

      internal static List<Type> GetAllTypesOf<TService>( Assembly assembly )
      {
         return assembly
            .GetTypes()
            .Where( x => typeof( TService ).IsAssignableFrom( x ) && !x.IsAbstract && !x.IsInterface )
            .ToList();
      }
   }
}
