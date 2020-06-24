using System;
using System.Globalization;

namespace XUnity.AutoTranslator.Plugin.Core.UIResize
{
   class IgnoreFontSize : IFontResizeCommand
   {
      public IgnoreFontSize( string[] args )
      {
         if( args.Length != 0 ) throw new ArgumentException( "IgnoreFontSize requires zero argument." );
      }

      public int? GetSize( int currentSize )
      {
         return null;
      }
   }
}
