using System;
using System.Globalization;

namespace XUnity.AutoTranslator.Plugin.Core.UIResize
{
   class UGUI_ChangeLineSpacingByPercentage : ILineSpacingCommand
   {
      private float _perc;

      public UGUI_ChangeLineSpacingByPercentage( string[] args )
      {
         if( args.Length != 1 ) throw new ArgumentException( "UGUI_ChangeLineSpacingByPercentage requires one argument." );

         _perc = float.Parse( args[ 0 ], CultureInfo.InvariantCulture );
      }

      public float GetLineSpacing( float currentSize )
      {
         return currentSize * _perc;
      }
   }
}
