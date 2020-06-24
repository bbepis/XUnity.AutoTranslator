using System;
using System.Globalization;

namespace XUnity.AutoTranslator.Plugin.Core.UIResize
{
   class UGUI_ChangeLineSpacing : IUGUI_LineSpacingCommand
   {
      private float? _size;

      public UGUI_ChangeLineSpacing( string[] args )
      {
         if( args.Length != 1 ) throw new ArgumentException( "UGUI_ChangeLineSpacing requires one argument." );

         _size = float.Parse( args[ 0 ], CultureInfo.InvariantCulture );
      }

      public float? GetLineSpacing( float currentSize )
      {
         return _size;
      }
   }
}
