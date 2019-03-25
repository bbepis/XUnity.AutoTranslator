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

         var currentDirectory = Environment.CurrentDirectory;
         foreach( var relativeFilePath in files )
         {
            LoadAssembliesInFile<TService>( relativeFilePath, allTypes );
         }

         return allTypes.ToList();
      }

      private static bool LoadAssembliesInFile<TService>( string file, HashSet<Type> allTypes )
      {
         try
         {
            var assembly = LoadAssembly( file );
            var types = GetAllTypesOf<TService>( assembly );

            foreach( var type in types )
            {
               allTypes.Add( type );
            }

            return true;
         }
         catch( Exception e )
         {
            XuaLogger.Current.Error( e, "An error occurred while loading types in assembly: " + file );
         }

         return false;
      }

      private static Assembly LoadAssembly( string file )
      {
         try
         {
            var assemblyName = AssemblyName.GetAssemblyName( file );
            var assembly = Assembly.Load( assemblyName );
            return assembly;
         }
         catch
         {
            // fallback to legacy API
            var assembly = Assembly.LoadFrom( file );
            return assembly;
         }
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
