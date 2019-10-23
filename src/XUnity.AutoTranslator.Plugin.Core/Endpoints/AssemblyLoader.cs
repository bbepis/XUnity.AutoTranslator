using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints
{
   internal static class AssemblyLoader
   {
      internal static List<Type> GetAllTypesOf<TService>( string directory )
      {
         Directory.CreateDirectory( directory );

         var files = Directory.GetFiles( directory, "*.dll" );
         var allTypes = new HashSet<Type>();
         
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
            if( assembly != null )
            {
               var types = GetAllTypesOf<TService>( assembly );

               foreach( var type in types )
               {
                  allTypes.Add( type );
               }

               return true;
            }
         }
         catch( Exception e )
         {
            XuaLogger.AutoTranslator.Error( e, "An error occurred while loading types in assembly: " + file );
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
         catch( BadImageFormatException )
         {
            // unmanaged
         }
         catch
         {
            // fallback to legacy API
            try
            {
               var assembly = Assembly.LoadFrom( file );
               return assembly;
            }
            catch( BadImageFormatException )
            {
               // unmanaged
            }
         }

         return null;
      }

      internal static List<Type> GetAllTypesOf<TService>( Assembly assembly )
      {
         try
         {
            return assembly
               .GetTypes()
               .Where( x => typeof( TService ).IsAssignableFrom( x ) && !x.IsAbstract && !x.IsInterface )
               .ToList();
         }
         catch( ReflectionTypeLoadException )
         {
            return new List<Type>();
         }
      }
   }
}
