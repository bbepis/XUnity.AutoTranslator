using System;
using System.Globalization;

namespace XUnity.AutoTranslator.Plugin.Core.UIResize
{
   class ChangeFontSize : IFontResizeCommand
   {
      private int? _size;

      public ChangeFontSize( string[] args )
      {
         if( args.Length != 1 ) throw new ArgumentException( "ChangeFontSize requires one argument." );

         _size = int.Parse( args[ 0 ], CultureInfo.InvariantCulture );
      }

      public int? GetSize( int currentSize )
      {
         return _size;
      }
   }
}
