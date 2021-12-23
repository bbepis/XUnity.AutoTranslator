using System;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Constants;

namespace XUnity.AutoTranslator.Plugin.Core.UIResize
{
   class TMP_Overflow : ITMP_OverflowMode
   {
      private int? _mode;

      public TMP_Overflow( string[] args )
      {
         if( args.Length != 1 ) throw new ArgumentException( "TMP_Overflow requires one argument." );

         _mode = (int)EnumHelper.GetValues( UnityTypes.TextOverflowModes, args[ 0 ] );
      }

      public int? GetMode()
      {
         return _mode;
      }
   }
}
