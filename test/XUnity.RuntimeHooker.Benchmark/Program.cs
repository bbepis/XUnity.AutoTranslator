using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using XUnity.RuntimeHooker.Core;

namespace XUnity.RuntimeHooker.Benchmark
{
   class Program
   {
      static void Main( string[] args )
      {
         int a = 12;
         int b = 23;

         MyMethod( a, b );

         var myMethod = typeof( Program ).GetMethod( "MyMethod" );
         var myHook = typeof( Program ).GetMethod( "MyHook" );

         int iterations = 10000;
         var sw = Stopwatch.StartNew();

         for( int i = 0 ; i < iterations ; i++ )
         {
            MyMethod( a, b );
         }

         Console.WriteLine( sw.ElapsedMilliseconds );

         RuntimeMethodPatcher.Patch( myMethod, new HookMethod( myHook ), null );
         MyMethod( a, b );

         sw.Reset();
         sw.Start();

         for( int i = 0 ; i < iterations ; i++ )
         {
            MyMethod( a, b );
         }

         Console.WriteLine( sw.ElapsedMilliseconds );
      }

      [MethodImpl( MethodImplOptions.NoInlining )]
      public static void MyMethod( int a, int b )
      {
      }

      public static void MyHook( int b, int a )
      {
      }
   }
}
