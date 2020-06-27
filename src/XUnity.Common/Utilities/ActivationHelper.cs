using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace XUnity.Common.Utilities
{
   public static class ActivationHelper
   {
      public static TService Create<TService>( Assembly parentAssembly, params string[] assemblies )
      {
         foreach( var assembly in assemblies )
         {
            var dir = new FileInfo( parentAssembly.Location ).Directory.FullName;
            var path = Path.Combine( dir, assembly );

            var types = GetAllTypesOf<TService>( path );
            if( types == null )
               continue;

            var type = types.FirstOrDefault();
            if( type != null )
            {
               return (TService)Activator.CreateInstance( type );
            }
         }
         return default;
      }

      private static List<Type> GetAllTypesOf<TService>( string file )
      {
         try
         {
            var assembly = LoadAssembly( file );

            var types = GetAllTypesOf<TService>( assembly );

            return types;
         }
         catch( Exception )
         {

         }

         return null;
      }

      private static List<Type> GetAllTypesOf<TService>( Assembly assembly )
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
   }
}
