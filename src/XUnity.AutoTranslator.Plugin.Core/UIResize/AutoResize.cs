using System;
using System.Globalization;

namespace XUnity.AutoTranslator.Plugin.Core.UIResize
{
   class AutoResize : IFontAutoResizeCommand
   {
      private bool _shouldAutoResize;
      private double? _minSize;
      private double? _maxSize;

      public AutoResize( string[] args )
      {
         if( args.Length < 1 ) throw new ArgumentException( "ChangeFontSize requires one, two or three argument." );

         _shouldAutoResize = bool.Parse( args[ 0 ] );

         if( args.Length >= 2 )
         {
            _minSize = ParseMinMaxSize( args[ 1 ] );
         }
         if( args.Length >= 3 )
         {
            _maxSize = ParseMinMaxSize( args[ 2 ] );
         }
      }

      private static double? ParseMinMaxSize( string arg )
      {
         if( string.Equals( arg, "keep", StringComparison.OrdinalIgnoreCase ) )
         {
            return null;
         }
         else if( string.Equals( arg, "none", StringComparison.OrdinalIgnoreCase ) )
         {
            return double.NaN;
         }
         else
         {
            return double.Parse( arg, CultureInfo.InvariantCulture );
         }
      }

      public double? GetMinSize()
      {
         return _minSize;
      }

      public double? GetMaxSize()
      {
         return _maxSize;
      }

      public bool ShouldAutoResize()
      {
         return _shouldAutoResize;
      }
   }
}
