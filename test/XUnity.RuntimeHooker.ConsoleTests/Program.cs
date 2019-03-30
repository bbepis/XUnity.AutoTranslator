using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using XUnity.RuntimeHooker.Core;

namespace XUnity.RuntimeHooker.ConsoleTests
{
   class Program
   {
      static void Main( string[] args )
      {
         var sayHello = typeof( Program ).GetMethod( "SayHello" );
         var sayGoodbye = typeof( Program ).GetMethod( "SayGoodbye" );
         RuntimeMethodPatcher.Patch( sayHello, new HookMethod( sayGoodbye ), null );
         SayHello();

         var textComponent = new TextComponent();
         textComponent.Text = "こんにちは";

         var textGetter = typeof( TextComponent ).GetProperty( "Text" ).GetGetMethod();
         var textGetterHook = typeof( Program ).GetMethod( "TextGetterHook" );
         RuntimeMethodPatcher.Patch( textGetter, null, new HookMethod( textGetterHook ) );

         Console.WriteLine( textComponent.Text );

         var printValue = typeof( Program ).GetMethod( "PrintValue" );
         var printValueHook = typeof( Program ).GetMethod( "PrintValueHook" );
         RuntimeMethodPatcher.Patch( printValue, new HookMethod( printValueHook ), null );
         PrintValue( 7331 );
      }

      [MethodImpl( MethodImplOptions.NoInlining )]
      public static void SayHello()
      {
         Console.WriteLine( "Hello" );
      }

      public static bool SayGoodbye()
      {
         Console.WriteLine( "Goodbye" );

         return false;
      }

      public static void TextGetterHook( ref string __result, TextComponent __instance )
      {
         Console.WriteLine( "Instance: " + __instance );

         __result = "Hello there!";
      }

      [MethodImpl( MethodImplOptions.NoInlining )]
      public static void PrintValue( int i )
      {
         Console.WriteLine( i );
      }

      public static void PrintValueHook( ref int i )
      {
         i = 1337;
      }
   }

   public class TextComponent
   {
      public string Text { [MethodImpl( MethodImplOptions.NoInlining )]get; set; }
   }
}
