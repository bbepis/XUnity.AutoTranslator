using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Inject;
using ReiPatcher;
using ReiPatcher.Patch;

namespace XUnity.AutoTranslator.Patcher
{
   public class Patcher : PatchBase
   {
      private static readonly HashSet<string> EntryClasses = new HashSet<string> { "Display", "Input" };
      private AssemblyDefinition _hookAssembly;
      private string _assemblyName;

      public override string Name
      {
         get
         {
            return "Auto Translator";
         }
      }

      public override string Version
      {
         get
         {
            return "5.3.0";
         }
      }

      public override void PrePatch()
      {
         if( ManagedDllExists( "UnityEngine.CoreModule.dll" ) )
         {
            RPConfig.RequestAssembly( "UnityEngine.CoreModule.dll" );
            _assemblyName = "UnityEngine.CoreModule";
         }
         else if( ManagedDllExists( "UnityEngine.dll" ) )
         {
            RPConfig.RequestAssembly( "UnityEngine.dll" );
            _assemblyName = "UnityEngine";
         }

         _hookAssembly = LoadAssembly( "XUnity.AutoTranslator.Plugin.Core.dll" );
      }

      public override bool CanPatch( PatcherArguments args )
      {
         return ( args.Assembly.Name.Name == _assemblyName ) && !HasAttribute( this, args.Assembly, "XUnity.AutoTranslator.Plugin.Core" );
      }

      public override void Patch( PatcherArguments args )
      {
         var PluginLoader = _hookAssembly.MainModule.GetType( "XUnity.AutoTranslator.Plugin.Core.PluginLoader" );
         var LoadThroughBootstrapper = PluginLoader.GetMethod( "LoadThroughBootstrapper" );

         var entryClasses = args.Assembly.MainModule.GetTypes().Where( x => EntryClasses.Contains( x.Name ) ).ToList();
         foreach( var entryClass in entryClasses )
         {
            var staticCtor = entryClass.Methods.FirstOrDefault( x => x.IsStatic && x.IsConstructor );
            if( staticCtor != null )
            {
               var injecctor = new InjectionDefinition( staticCtor, LoadThroughBootstrapper, InjectFlags.None );
               injecctor.Inject(
                  startCode: 0,
                  direction: InjectDirection.Before );
            }
         }

         SetPatchedAttribute( args.Assembly, "XUnity.AutoTranslator.Plugin.Core" );
      }

      public bool ManagedDllExists( string name )
      {
         string path = Path.Combine( RPConfig.ConfigFile.GetSection( "ReiPatcher" ).GetKey( "AssembliesDir" ).Value, name );
         return File.Exists( path );
      }

      public static AssemblyDefinition LoadAssembly( string name )
      {
         string path = Path.Combine( RPConfig.ConfigFile.GetSection( "ReiPatcher" ).GetKey( "AssembliesDir" ).Value, name );
         if( !File.Exists( path ) ) throw new FileNotFoundException( "Missing DLL: " + path );
         using( Stream s = File.OpenRead( path ) )
         {
            return AssemblyDefinition.ReadAssembly( s );
         }
      }

      public static bool HasAttribute( PatchBase patch, AssemblyDefinition assembly, string attribute )
      {
         return patch.GetPatchedAttributes( assembly ).Any( a => a.Info == attribute );
      }
   }
}
