using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Lec.ExtProtocol
{
   class LecTranslationLibrary : UnmanagedTranslationLibrary
   {
      [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
      private delegate int eg_init( string path );

      [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
      private delegate int eg_init2( string path, int i );

      [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
      private delegate int eg_end();

      [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
      private delegate int eg_translate_multi( int i, IntPtr in_str, int out_size, IntPtr out_str );

      public const int ShiftJIS = 932;

      private static IntPtr ConvertStringToNative( string managedString, int codepage )
      {
         var encoding = Encoding.GetEncoding( codepage );
         var buffer = encoding.GetBytes( managedString );
         IntPtr nativeUtf8 = Marshal.AllocHGlobal( buffer.Length + 1 );
         Marshal.Copy( buffer, 0, nativeUtf8, buffer.Length );
         Marshal.WriteByte( nativeUtf8, buffer.Length, 0 );
         return nativeUtf8;
      }

      private static string ConvertNativeToString( IntPtr nativeUtf8, int codepage )
      {
         int len = 0;
         while( Marshal.ReadByte( nativeUtf8, len ) != 0 ) ++len;
         byte[] buffer = new byte[ len ];
         Marshal.Copy( nativeUtf8, buffer, 0, buffer.Length );
         return Encoding.GetEncoding( codepage ).GetString( buffer );
      }

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
            switch( c )
            {
               case '『':
               case '｢':
               case '「':
                  builder.Append( '[' );
                  break;
               case '』':
               case '｣':
               case '」':
                  builder.Append( ']' );
                  break;
               case '≪':
               case '（':
                  builder.Append( '(' );
                  break;
               case '≫':
               case '）':
                  builder.Append( ')' );
                  break;
               case '…':
                  builder.Append( "..." );
                  break;
               case '：':
                  builder.Append( '￤' );
                  break;
               case '・':
                  builder.Append( '.' );
                  break;
               default:
                  builder.Append( c );
                  break;
            }
         }

         return builder.ToString();
      }

      public override string Translate( string toTranslate )
      {
         toTranslate = PreprocessString( toTranslate );

         // allocate three times the size of the untranslated text for the result (initial)
         var size = toTranslate.Length * 3;
         IntPtr ptr = IntPtr.Zero;
         IntPtr str = IntPtr.Zero;

         // we can't know the output size, so just try until we have a big enough buffer
         try
         {
            // get an unmanaged version of the untranslated text encoded as ShiftJIS
            str = ConvertStringToNative( toTranslate, ShiftJIS );

            int translatedSize;
            do
            {
               // double the size of the allocated unmanaged memory
               size = size * 2;

               // give up when we reach 10 MB
               if( size > 10 * 1024 * 1024 )
               {
                  return null;
               }

               // free up previous allocated memory for the result
               if( ptr != IntPtr.Zero )
               {
                  Marshal.FreeHGlobal( ptr );
               }

               // allocate memory for result
               ptr = Marshal.AllocHGlobal( size );

               // perform translation
               translatedSize = _translate( 0, str, size, ptr );

            } while( translatedSize > size );

            // convert the unamanged ShiftJIS string to a managed C# string
            var result = ConvertNativeToString( ptr, ShiftJIS );

            return result;
         }
         finally
         {
            if( str != null )
            {
               Marshal.FreeHGlobal( str );
            }

            if( ptr != IntPtr.Zero )
            {
               Marshal.FreeHGlobal( ptr );
            }
         }
      }

      protected override void Initialize( string libraryPath )
      {
         try
         {
            _end = Loader.LoadFunction<eg_end>( "eg_end" );
            _translate = Loader.LoadFunction<eg_translate_multi>( "eg_translate_multi" );
         }
         catch( Exception e )
         {
            throw new Exception( $"Could not load functions from LEC library '{libraryPath}'.", e );
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
            catch( Exception e )
            {
               throw new Exception( $"Could not load functions from LEC library '{libraryPath}'.", e );
            }
         }

         var directory = Path.GetDirectoryName( libraryPath ) + Path.DirectorySeparatorChar;

         var initializationCode = _init2?.Invoke( directory, 0 );
         if( initializationCode == null )
         {
            initializationCode = _init( directory );
         }

         if( initializationCode != 0 )
         {
            throw new Exception( $"Could not initialize LEC library. Received code '{initializationCode}'." );
         }
      }

      protected override void Dispose( bool disposing )
      {
         if( disposing ) _end?.Invoke();
         base.Dispose( disposing );
      }
   }
}
