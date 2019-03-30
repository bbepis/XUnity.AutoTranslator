using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XUnity.RuntimeHooker.Core;
using XUnity.RuntimeHooker.Core.Utilities;

namespace XUnity.RuntimeHooker.Trampolines
{
   internal static class TrampolineInitializer
   {
      public static readonly object Sync = new object();
      public static TrampolineData Data;

      public static void SetupHook( MethodBase originalMethod )
      {
         // just skip if already initialized
         if( Data != null ) return;

         // determine if we have a supported return type
         var returnType = ( originalMethod as MethodInfo )?.ReturnType;
         if( !( returnType == typeof( void ) || !returnType.IsValueType ) )
         {
            throw new InvalidOperationException( "Current implementation only supports hooking method that returns reference types or void." );
         }

         // obtain parameters to method, and check they are not generic
         var parameters = originalMethod.GetParameters();
         if( originalMethod.IsGenericMethod || originalMethod.IsGenericMethodDefinition )
         {
            throw new InvalidOperationException( "Current implementation does not support generic methods." );
         }

         // check the are not ref or out parameters
         foreach( var parameter in parameters )
         {
            if( parameter.ParameterType.IsByRef || parameter.IsOut )
            {
               throw new InvalidOperationException( "Current implementation does not support out or ref parameters." );
            }
         }

         // determine which type to use as a trampoline
         string type = null;
         bool hasReturn = returnType != typeof( void );
         bool isStatic = originalMethod.IsStatic;
         if( hasReturn && isStatic )
         {
            type = "XUnity.RuntimeHooker.Trampolines.StaticTrampolineWithReturn";
         }
         else if( !hasReturn && isStatic )
         {
            type = "XUnity.RuntimeHooker.Trampolines.StaticTrampoline";
         }
         else if( hasReturn && !isStatic )
         {
            type = "XUnity.RuntimeHooker.Trampolines.InstanceTrampolineWithReturn";
         }
         else if( !hasReturn && !isStatic )
         {
            type = "XUnity.RuntimeHooker.Trampolines.InstanceTrampoline";
         }

         // determine if and how many generic arguments the trampoline type has
         var genericArguments = new List<Type>();
         if( !originalMethod.IsStatic )
         {
            genericArguments.Add( originalMethod.DeclaringType );
         }
         genericArguments.AddRange( parameters.Select( x => x.ParameterType ) );
         if( genericArguments.Count > 0 )
         {
            // and apply those generic arguments to the type name so it can be found
            type += "`" + genericArguments.Count;
         }

         // find the trampoline type
         var trampolineType = typeof( TrampolineInitializer ).Assembly.GetType( type );
         if( trampolineType == null )
         {
            throw new InvalidOperationException( "Current implementation only supports methods with up to 5 arguments (including the instance)." );
         }

         // genericify the trampoline type
         if( genericArguments.Count > 0 )
         {
            trampolineType = trampolineType.MakeGenericType( genericArguments.ToArray() );
         }

         // obtain the method that we will jump to when the original method is called
         var flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
         var overrideMethod = trampolineType.GetMethod( "Override", flags );

         // get the location of the override method
         long replacementMethodLocation = MemoryHelper.GetMethodStartLocation( overrideMethod );

         // get the location and initial code of the original method
         long originalMethodLocation = MemoryHelper.GetMethodStartLocation( originalMethod );
         var originalCode = MemoryHelper.GetInstructionsAtLocationRequiredToWriteJump( originalMethodLocation );

         // create an (optional) delegate that enables invoking the function MUCH faster than through reflection
         Func<object, object[], object> fastInvoke = null;
         if( originalMethod is MethodInfo methodInfo )
         {
            fastInvoke = ExpressionHelper.CreateFastInvoke( methodInfo );
         }

         // store all data required by the trampoline function
         Data = new TrampolineData
         {
            Method = originalMethod,
            FastInvoke = fastInvoke,
            JumpedMethodInfo = new JumpedMethodInfo( originalMethodLocation, replacementMethodLocation, originalCode ),
            Parameters = new object[ parameters.Length ],
            Postfixes = new List<HookCallbackInvoker>(),
            Prefixes = new List<HookCallbackInvoker>()
         };

         // create a JMP instruction at the start of the original method so we jump to the override method
         // (create the hook)
         MemoryHelper.WriteJump( true, originalMethodLocation, replacementMethodLocation );
      }

      public static void AddPrefix( HookMethod prefix )
      {
         if( Data == null ) throw new InvalidOperationException( "Please call SetupHook before adding prefixes." );

         var invoker = new HookCallbackInvoker( Data.Method, prefix );
         Data.Prefixes.Add( invoker );
         Data.Prefixes.Sort();
      }

      public static void AddPostfix( HookMethod postfix )
      {
         if( Data == null ) throw new InvalidOperationException( "Please call SetupHook before adding postfixes." );

         var invoker = new HookCallbackInvoker( Data.Method, postfix );
         Data.Postfixes.Add( invoker );
         Data.Postfixes.Sort();
      }
   }
}
