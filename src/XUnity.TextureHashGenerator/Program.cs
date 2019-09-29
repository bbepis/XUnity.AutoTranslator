using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace XUnity.TextureHashGenerator
{
   class Program
   {
      static void Main( string[] args )
      {
         if( args.Length == 0 )
         {
            Console.WriteLine( "Please open this application with a file by dragging it ontop of this application." );
            Console.WriteLine( "Press any key to exit." );
            Console.ReadKey();
            return;
         }

         var regex = new Regex( @"^([\s\S]+) \[([0-9A-Za-z]{10})-?([0-9A-Za-z]{10})?\]$" );

         foreach( var path in args )
         {
            var file = new FileInfo( path );
            if( file.Exists )
            {
               var name = Path.GetFileNameWithoutExtension( file.Name );
               Console.WriteLine( "File: " + name );

               using( var stream = file.OpenRead() )
               {
                  var dataHash = HashHelper.Compute( stream );
                  string nameHash;

                  var regexResult = regex.Match( name );
                  if( regexResult.Success )
                  {
                     string existingDataHash = regexResult.Groups[ 2 ].Value;
                     string existingNameHash = existingDataHash;
                     name = regexResult.Groups[ 1 ].Value;
                     var nameData = Encoding.UTF8.GetBytes( name );
                     var nameStream = new MemoryStream( nameData );
                     nameHash = HashHelper.Compute( nameStream );

                     if( regexResult.Groups[ 3 ].Success )
                     {
                        existingDataHash = regexResult.Groups[ 3 ].Value;

                        var nameHashMatches = StringComparer.InvariantCultureIgnoreCase.Compare( existingNameHash, nameHash ) == 0;
                        if( !nameHashMatches )
                        {
                           Console.WriteLine( "WARNING: Name hash in file name does not seem to match the file name, so this tool will likely not output the correct hash for name!" );
                        }
                     }

                     var isModified = StringComparer.InvariantCultureIgnoreCase.Compare( existingDataHash, dataHash ) != 0;
                     if( isModified )
                     {
                        Console.WriteLine( "WARNING: This texture has been modified so the 'FromImageData' hash is not correct." );
                     }
                     else
                     {
                        Console.WriteLine( "This texture has not been modified." );
                     }

                     Console.WriteLine();
                  }
                  else
                  {
                     Console.WriteLine( "Found no existing hashes in file name." );
                     var nameData = Encoding.UTF8.GetBytes( name );
                     var nameStream = new MemoryStream( nameData );
                     nameHash = HashHelper.Compute( nameStream );
                  }

                  Console.WriteLine( "Name Hash: " + nameHash );
                  Console.WriteLine( "Data Hash: " + dataHash );
                  Console.WriteLine( "FromImageName: [" + nameHash + "-" + dataHash + "]" );
                  Console.WriteLine( "FromImageData: [" + dataHash + "]" );
                  Console.WriteLine( "--------------------------------------" );
               }
            }
         }

         Console.WriteLine( "Press any key to exit." );
         Console.ReadKey();
      }
   }

   internal static class HashHelper
   {
      private static readonly SHA1Managed SHA1 = new SHA1Managed();
      private static readonly uint[] Lookup32 = CreateLookup32();


      public static string Compute( Stream data )
      {
         var hash = SHA1.ComputeHash( data );
         var hex = ByteArrayToHexViaLookup32( hash );
         return hex.Substring( 0, 10 );
      }

      private static uint[] CreateLookup32()
      {
         var result = new uint[ 256 ];
         for( int i = 0; i < 256; i++ )
         {
            string s = i.ToString( "X2" );
            result[ i ] = ( (uint)s[ 0 ] ) + ( (uint)s[ 1 ] << 16 );
         }
         return result;
      }

      private static string ByteArrayToHexViaLookup32( byte[] bytes )
      {
         var lookup32 = Lookup32;
         var result = new char[ bytes.Length * 2 ];
         for( int i = 0; i < bytes.Length; i++ )
         {
            var val = lookup32[ bytes[ i ] ];
            result[ 2 * i ] = (char)val;
            result[ 2 * i + 1 ] = (char)( val >> 16 );
         }
         return new string( result );
      }
   }
}
