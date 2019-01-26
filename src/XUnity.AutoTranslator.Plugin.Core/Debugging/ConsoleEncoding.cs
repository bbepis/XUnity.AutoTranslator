using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XUnity.AutoTranslator.Plugin.Core.Debugging
{
   internal class ConsoleEncoding : Encoding
   {
      private byte[] _byteBuffer = new byte[ 256 ];
      private char[] _charBuffer = new char[ 256 ];
      private byte[] _zeroByte = new byte[ 0 ];
      private char[] _zeroChar = new char[ 0 ];

      private readonly uint _codePage;
      public override int CodePage => (int)_codePage;

      private ConsoleEncoding( uint codePage )
      {
         _codePage = codePage;
      }

      public static ConsoleEncoding GetEncoding( uint codePage )
      {
         return new ConsoleEncoding( codePage );
      }

      public override int GetByteCount( char[] chars, int index, int count )
      {
         WriteCharBuffer( chars, index, count );
         int result = Kernel32.WideCharToMultiByte( _codePage, 0, _charBuffer, count, _zeroByte, 0, IntPtr.Zero, IntPtr.Zero );
         return result;
      }

      public override int GetBytes( char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex )
      {
         var byteCount = GetMaxByteCount( charCount );

         WriteCharBuffer( chars, charIndex, charCount );

         ExpandByteBuffer( byteCount );
         int result = Kernel32.WideCharToMultiByte( _codePage, 0, chars, charCount, _byteBuffer, byteCount, IntPtr.Zero, IntPtr.Zero );
         ReadByteBuffer( bytes, byteIndex, byteCount );

         return result;
      }

      public override int GetCharCount( byte[] bytes, int index, int count )
      {
         WriteByteBuffer( bytes, index, count );
         int result = Kernel32.MultiByteToWideChar( _codePage, 0, bytes, count, _zeroChar, 0 );
         return result;
      }

      public override int GetChars( byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex )
      {
         var charCount = GetMaxCharCount( byteCount );

         WriteByteBuffer( bytes, byteIndex, byteCount );

         ExpandCharBuffer( charCount );
         int result = Kernel32.MultiByteToWideChar( _codePage, 0, bytes, byteCount, _charBuffer, charCount );
         ReadCharBuffer( chars, charIndex, charCount );

         return result;
      }

      public override int GetMaxByteCount( int charCount ) => charCount * 2;
      public override int GetMaxCharCount( int byteCount ) => byteCount;

      private void ExpandByteBuffer( int count )
      {
         if( _byteBuffer.Length < count )
            _byteBuffer = new byte[ count ];
      }

      private void ExpandCharBuffer( int count )
      {
         if( _charBuffer.Length < count )
            _charBuffer = new char[ count ];
      }

      private void ReadByteBuffer( byte[] bytes, int index, int count )
      {
         for( int i = 0 ; i < count ; i++ )
            bytes[ index + i ] = _byteBuffer[ i ];
      }

      private void ReadCharBuffer( char[] chars, int index, int count )
      {
         for( int i = 0 ; i < count ; i++ )
            chars[ index + i ] = _charBuffer[ i ];
      }

      private void WriteByteBuffer( byte[] bytes, int index, int count )
      {
         ExpandByteBuffer( count );
         for( int i = 0 ; i < count ; i++ )
            _byteBuffer[ i ] = bytes[ index + i ];
      }

      private void WriteCharBuffer( char[] chars, int index, int count )
      {
         ExpandCharBuffer( count );
         for( int i = 0 ; i < count ; i++ )
            _charBuffer[ i ] = chars[ index + i ];
      }
   }
}
