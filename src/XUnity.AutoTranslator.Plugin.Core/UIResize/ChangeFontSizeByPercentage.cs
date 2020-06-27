using System;
using System.Globalization;

namespace XUnity.AutoTranslator.Plugin.Core.UIResize
{
   class ChangeFontSizeByPercentage : IFontResizeCommand
   {
      private double _perc;

      public ChangeFontSizeByPercentage( string[] args )
      {
         if( args.Length != 1 ) throw new ArgumentException( "ChangeFontSizeByPercentage requires one argument." );

         _perc = double.Parse( args[ 0 ], CultureInfo.InvariantCulture );
      }

      public int? GetSize( int currentSize )
      {
         return (int)( currentSize * _perc );
      }
   }
}
