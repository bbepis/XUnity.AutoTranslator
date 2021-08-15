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
   public sealed class Font : Object
   {
      public string[] fontNames
      {
         get => throw new NotImplementedException();
         set => throw new NotImplementedException();
      }

      public int lineHeight => throw new NotImplementedException();

      public int fontSize => throw new NotImplementedException();

      public Font()
      {
      }

      public Font( string name )
      {
      }

      private Font( string[] names, int size )
      {
      }

      public static extern string[] GetOSInstalledFontNames();

      public static Font CreateDynamicFontFromOSFont( string fontname, int size )
      {
         return new Font( new string[ 1 ]
         {
            fontname
         }, size );
      }

      public static Font CreateDynamicFontFromOSFont( string[] fontnames, int size )
      {
         return new Font( fontnames, size );
      }
   }
}
