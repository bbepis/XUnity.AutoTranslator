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
   [Serializable]
   [StructLayout( LayoutKind.Sequential )]
   public class GUIContent
   {
      private string m_Text = string.Empty;

      private Texture m_Image;

      private string m_Tooltip = string.Empty;

      private static readonly GUIContent s_Text = new GUIContent();

      private static readonly GUIContent s_Image = new GUIContent();

      private static readonly GUIContent s_TextImage = new GUIContent();

      public static GUIContent none = new GUIContent( "" );

      public string text
      {
         get
         {
            return m_Text;
         }
         set
         {
            m_Text = value;
         }
      }

      public Texture image
      {
         get
         {
            return m_Image;
         }
         set
         {
            m_Image = value;
         }
      }

      public string tooltip
      {
         get
         {
            return m_Tooltip;
         }
         set
         {
            m_Tooltip = value;
         }
      }

      internal int hash
      {
         get
         {
            int result = 0;
            if( !string.IsNullOrEmpty( m_Text ) )
            {
               result = m_Text.GetHashCode() * 37;
            }

            return result;
         }
      }

      public GUIContent()
      {
      }

      public GUIContent( string text )
         : this( text, null, string.Empty )
      {
      }

      public GUIContent( Texture image )
         : this( string.Empty, image, string.Empty )
      {
      }

      public GUIContent( string text, Texture image )
         : this( text, image, string.Empty )
      {
      }

      public GUIContent( string text, string tooltip )
         : this( text, null, tooltip )
      {
      }

      public GUIContent( Texture image, string tooltip )
         : this( string.Empty, image, tooltip )
      {
      }

      public GUIContent( string text, Texture image, string tooltip )
      {
         this.text = text;
         this.image = image;
         this.tooltip = tooltip;
      }

      public GUIContent( GUIContent src )
      {
         text = src.m_Text;
         image = src.m_Image;
         tooltip = src.m_Tooltip;
      }

      internal static GUIContent Temp( string t )
      {
         s_Text.m_Text = t;
         s_Text.m_Tooltip = string.Empty;
         return s_Text;
      }

      internal static GUIContent Temp( string t, string tooltip )
      {
         s_Text.m_Text = t;
         s_Text.m_Tooltip = tooltip;
         return s_Text;
      }

      internal static GUIContent Temp( Texture i )
      {
         s_Image.m_Image = i;
         s_Image.m_Tooltip = string.Empty;
         return s_Image;
      }

      internal static GUIContent Temp( Texture i, string tooltip )
      {
         s_Image.m_Image = i;
         s_Image.m_Tooltip = tooltip;
         return s_Image;
      }

      internal static GUIContent Temp( string t, Texture i )
      {
         s_TextImage.m_Text = t;
         s_TextImage.m_Image = i;
         return s_TextImage;
      }

      internal static void ClearStaticCache()
      {
         s_Text.m_Text = null;
         s_Text.m_Tooltip = string.Empty;
         s_Image.m_Image = null;
         s_Image.m_Tooltip = string.Empty;
         s_TextImage.m_Text = null;
         s_TextImage.m_Image = null;
      }

      internal static GUIContent[] Temp( string[] texts )
      {
         GUIContent[] array = new GUIContent[ texts.Length ];
         for( int i = 0 ; i < texts.Length ; i++ )
         {
            array[ i ] = new GUIContent( texts[ i ] );
         }

         return array;
      }

      internal static GUIContent[] Temp( Texture[] images )
      {
         GUIContent[] array = new GUIContent[ images.Length ];
         for( int i = 0 ; i < images.Length ; i++ )
         {
            array[ i ] = new GUIContent( images[ i ] );
         }

         return array;
      }
   }
}
