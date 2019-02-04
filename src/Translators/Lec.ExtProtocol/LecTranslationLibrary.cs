using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Lec.ExtProtocol
{
   class LecTranslationLibrary : UnmanagedTranslationLibrary
   {
      public const int JapaneseCodepage = 932;

      [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
      private delegate int eg_init( string path );
      [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
      private delegate int eg_init2( string path, int i );
      [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
      private delegate int eg_end();
      [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
      private delegate int eg_translate_multi( int i, IntPtr in_str, int out_size, StringBuilder out_str );

      private eg_end _end;
      private eg_translate_multi _translate;
      private eg_init _init;
      private eg_init2 _init2;

      public LecTranslationLibrary( string libraryPath )
         : base( libraryPath )
      {

      }

      private string PreprocessString( string str )
      {
         var builder = new StringBuilder( str.Length );

         foreach( var c in str )
         {
            char o;

            switch( c )
            {
               case '『':
               case '｢':
               case '「':
                  o = '['; break;
               case '』':
               case '｣':
               case '」':
                  o = ']'; break;
               case '≪':
               case '（':
                  o = '('; break;
               case '≫':
               case '）':
                  o = ')'; break;
               case '…':
                  o = ' '; break;
               case '：':
                  o = '￤'; break;
               case '・':
                  o = '.'; break;
               default:
                  o = c;
                  break;
            }
            builder.Append( c );
         }

         return builder.ToString();
      }

      public override string Translate( string toTranslate )
      {
         toTranslate = PreprocessString( toTranslate );
         var size = toTranslate.Length * 3;
         var builder = new StringBuilder();
         int translatedSize;
         // we can't know the output size, so just try until we have a big enough buffer
         do
         {
            size = size * 2;
            // give up when we reach 10 MB string
            if( size > 10 * 1024 * 1024 )
            {
               return null;
            }

            builder.Capacity = size;
            var str = ConvertStringToNative( toTranslate, JapaneseCodepage );
            translatedSize = _translate( 0, str, size, builder );
            Marshal.FreeHGlobal( str );

         } while( translatedSize > size );

         return builder.ToString();
      }
      
      protected override bool OnInitialize( string libraryPath )
      {
         try
         {
            _end = Loader.LoadFunction<eg_end>( "eg_end" );
            _translate = Loader.LoadFunction<eg_translate_multi>( "eg_translate_multi" );
         }
         catch( Exception )
         {
            return false;
         }

         try
         {
            _init2 = Loader.LoadFunction<eg_init2>( "eg_init2" );
         }
         catch
         {
            try
            {
               _init = Loader.LoadFunction<eg_init>( "eg_init" );
            }
            catch
            {
               return false;
            }
         }

         var directory = Path.GetDirectoryName( libraryPath ) + '\\';

         var succeded = _init2?.Invoke( directory, 0 );
         var initialized = succeded ?? _init( directory );

         return initialized == 0;
      }

      protected override void Dispose( bool disposing )
      {
         if( disposing ) _end?.Invoke();
         base.Dispose( disposing );
      }
   }
}
