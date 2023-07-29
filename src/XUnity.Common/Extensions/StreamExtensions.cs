using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XUnity.Common.Extensions
{
   /// <summary>
   /// Simple extensions to the Stream class.
   /// </summary>
   public static class StreamExtensions
   {
      /// <summary>
      /// Reads all data in the stream into a buffer. If possible specify the length of the stream as the initialLength.
      /// </summary>
      /// <param name="stream">The stream to read from.</param>
      /// <param name="initialLength">The initial length of the buffer.</param>
      /// <returns>All the data in the stream.</returns>
      public static byte[] ReadFully( this Stream stream, int initialLength )
      {
         // https://jonskeet.uk/csharp/readbinary.html

         // If we've been passed an unhelpful initial length, just
         // use 32K.
         if( initialLength < 1 )
         {
            initialLength = 32768;
         }

         byte[] buffer = new byte[ initialLength ];
         int read = 0;

         int chunk;
         while( ( chunk = stream.Read( buffer, read, buffer.Length - read ) ) > 0 )
         {
            read += chunk;

            // If we've reached the end of our buffer, check to see if there's
            // any more information
            if( read == buffer.Length )
            {
               int nextByte = stream.ReadByte();

               // End of stream? If so, we're done
               if( nextByte == -1 )
               {
                  return buffer;
               }

               // Nope. Resize the buffer, put in the byte we've just
               // read, and continue
               byte[] newBuffer = new byte[ buffer.Length * 2 ];
               Array.Copy( buffer, newBuffer, buffer.Length );
               newBuffer[ read ] = (byte)nextByte;
               buffer = newBuffer;
               read++;
            }
         }
         // Buffer is now too big. Shrink it.
         byte[] ret = new byte[ read ];
         Array.Copy( buffer, ret, read );
         return ret;
      }

#if IL2CPP
      /// <summary>
      /// Reads all data in the stream into a buffer. If possible specify the length of the stream as the initialLength.
      /// </summary>
      /// <param name="stream">The stream to read from.</param>
      /// <param name="initialLength">The initial length of the buffer.</param>
      /// <returns>All the data in the stream.</returns>
      public static byte[] ReadFully( this Il2CppSystem.IO.Stream stream, int initialLength )
      {
         // https://jonskeet.uk/csharp/readbinary.html

         // If we've been passed an unhelpful initial length, just
         // use 32K.
         if( initialLength < 1 )
         {
            initialLength = 32768;
         }

         var buffer = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>( initialLength );
         int read = 0;

         int chunk;
         while( ( chunk = stream.Read( buffer, read, buffer.Length - read ) ) > 0 )
         {
            read += chunk;

            // If we've reached the end of our buffer, check to see if there's
            // any more information
            if( read == buffer.Length )
            {
               int nextByte = stream.ReadByte();

               // End of stream? If so, we're done
               if( nextByte == -1 )
               {
                  return buffer;
               }

               // Nope. Resize the buffer, put in the byte we've just
               // read, and continue
               byte[] newBuffer = new byte[ buffer.Length * 2 ];
               Array.Copy( buffer, newBuffer, buffer.Length );
               newBuffer[ read ] = (byte)nextByte;
               buffer = newBuffer;
               read++;
            }
         }
         // Buffer is now too big. Shrink it.
         byte[] ret = new byte[ read ];
         Array.Copy( buffer, ret, read );
         return ret;
      }
#endif
   }
}
