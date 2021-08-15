using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace UnityEngine
{
   public sealed class WWW : IDisposable
   {
      internal IntPtr m_Ptr;

      public Dictionary<string, string> responseHeaders => throw new NotImplementedException();

      private string responseHeadersString
      {
         get;
      }

      public string text => throw new NotImplementedException();

      internal static Encoding DefaultEncoding => Encoding.ASCII;

      [Obsolete( "Please use WWW.text instead" )]
      public string data => text;

      public byte[] bytes
      {
         get;
      }

      public int size
      {
         get;
      }

      public string error
      {
         get;
      }

      public Texture2D texture => GetTexture( markNonReadable: false );

      public Texture2D textureNonReadable => GetTexture( markNonReadable: true );

      [Obsolete( "Obsolete msg (UnityUpgradable) -> * UnityEngine.WWWAudioExtensions.GetAudioClip(UnityEngine.WWW)", true )]
      public Object audioClip => null;

      [Obsolete( "Obsolete msg (UnityUpgradable) -> * UnityEngine.WWWAudioExtensions.GetMovieTexture(UnityEngine.WWW)", true )]
      public Object movie => null;

      public bool isDone
      {
         get;
      }

      public float progress
      {
         get;
      }

      public float uploadProgress
      {
         get;
      }

      public int bytesDownloaded
      {
         get;
      }

      public Object oggVorbis => null;

      public string url
      {
         get;
      }

      public ThreadPriority threadPriority
      {
         get;
         set;
      }

      public WWW( string url )
      {
         InitWWW( url, null, null );
      }

      public WWW( string url, byte[] postData )
      {
         InitWWW( url, postData, null );
      }

      [Obsolete( "This overload is deprecated. Use UnityEngine.WWW.WWW(string, byte[], System.Collections.Generic.Dictionary<string, string>) instead.", true )]
      public WWW( string url, byte[] postData, Hashtable headers )
      {
      }

      public WWW( string url, byte[] postData, Dictionary<string, string> headers )
      {
         string[] iHeaders = FlattenedHeadersFrom( headers );
         InitWWW( url, postData, iHeaders );
      }

      internal WWW( string url, Hash128 hash, uint crc )
      {
         INTERNAL_CALL_WWW( this, url, ref hash, crc );
      }

      public void Dispose()
      {
         DestroyWWW( cancel: true );
      }

      ~WWW()
      {
         DestroyWWW( cancel: false );
      }

      private extern void DestroyWWW( bool cancel );

      public extern void InitWWW( string url, byte[] postData, string[] iHeaders );

      internal extern bool enforceWebSecurityRestrictions();

      public static string EscapeURL( string s )
      {
         Encoding uTF = Encoding.UTF8;
         return EscapeURL( s, uTF );
      }

      public static string EscapeURL( string s, Encoding e ) => throw new NotImplementedException();

      public static string UnEscapeURL( string s )
      {
         Encoding uTF = Encoding.UTF8;
         return UnEscapeURL( s, uTF );
      }

      public static string UnEscapeURL( string s, Encoding e ) => throw new NotImplementedException();

      private Encoding GetTextEncoder()
      {
         string value = null;
         if( responseHeaders.TryGetValue( "CONTENT-TYPE", out value ) )
         {
            int num = value.IndexOf( "charset", StringComparison.OrdinalIgnoreCase );
            if( num > -1 )
            {
               int num2 = value.IndexOf( '=', num );
               if( num2 > -1 )
               {
                  string text = value.Substring( num2 + 1 ).Trim().Trim( '\'', '"' )
                     .Trim();
                  int num3 = text.IndexOf( ';' );
                  if( num3 > -1 )
                  {
                     text = text.Substring( 0, num3 );
                  }

                  try
                  {
                     return Encoding.GetEncoding( text );
                  }
                  catch( Exception )
                  {
                  }
               }
            }
         }

         return Encoding.UTF8;
      }

      private extern Texture2D GetTexture( bool markNonReadable );

      internal extern Object GetAudioClipInternal( bool threeD, bool stream, bool compressed, AudioType audioType );

      internal extern Object GetMovieTextureInternal();

      public extern void LoadImageIntoTexture( Texture2D tex );

      public static extern string GetURL( string url );

      [Obsolete( "All blocking WWW functions have been deprecated, please use one of the asynchronous functions instead.", true )]
      public static Texture2D GetTextureFromURL( string url )
      {
         return new WWW( url ).texture;
      }

      [Obsolete( "LoadUnityWeb is no longer supported. Please use javascript to reload the web player on a different url instead", true )]
      public void LoadUnityWeb()
      {
      }

      private static extern void INTERNAL_CALL_WWW( WWW self, string url, ref Hash128 hash, uint crc );

      public static WWW LoadFromCacheOrDownload( string url, int version )
      {
         uint crc = 0u;
         return LoadFromCacheOrDownload( url, version, crc );
      }

      public static WWW LoadFromCacheOrDownload( string url, int version, uint crc )
      {
         Hash128 hash = new Hash128( 0u, 0u, 0u, (uint)version );
         return LoadFromCacheOrDownload( url, hash, crc );
      }

      public static WWW LoadFromCacheOrDownload( string url, Hash128 hash )
      {
         uint crc = 0u;
         return LoadFromCacheOrDownload( url, hash, crc );
      }

      public static WWW LoadFromCacheOrDownload( string url, Hash128 hash, uint crc )
      {
         return new WWW( url, hash, crc );
      }

      private static string[] FlattenedHeadersFrom( Dictionary<string, string> headers )
      {
         if( headers == null )
         {
            return null;
         }

         string[] array = new string[ headers.Count * 2 ];
         int num = 0;
         foreach( KeyValuePair<string, string> header in headers )
         {
            array[ num++ ] = header.Key.ToString();
            array[ num++ ] = header.Value.ToString();
         }

         return array;
      }

      internal static Dictionary<string, string> ParseHTTPHeaderString( string input )
      {
         if( input == null )
         {
            throw new ArgumentException( "input was null to ParseHTTPHeaderString" );
         }

         Dictionary<string, string> dictionary = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );
         StringReader stringReader = new StringReader( input );
         int num = 0;
         while( true )
         {
            string text = stringReader.ReadLine();
            if( text == null )
            {
               break;
            }

            if( num++ == 0 && text.StartsWith( "HTTP" ) )
            {
               dictionary[ "STATUS" ] = text;
               continue;
            }

            int num2 = text.IndexOf( ": " );
            if( num2 != -1 )
            {
               string key = text.Substring( 0, num2 ).ToUpper();
               string text2 = text.Substring( num2 + 2 );
               if( dictionary.TryGetValue( key, out string value ) )
               {
                  text2 = value + "," + text2;
               }

               dictionary[ key ] = text2;
            }
         }

         return dictionary;
      }
   }
}
