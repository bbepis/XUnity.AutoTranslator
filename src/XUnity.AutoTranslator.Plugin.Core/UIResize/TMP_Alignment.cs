using System;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Constants;

namespace XUnity.AutoTranslator.Plugin.Core.UIResize
{
   class TMP_Alignment : ITMP_Alignment
   {
      private int? _mode;

      public TMP_Alignment( string[] args )
      {
         if( args.Length != 1 ) throw new ArgumentException( "TMP_Alignment requires one argument." );

         _mode = (int)EnumHelper.GetValues( UnityTypes.TextAlignmentOptions, args[ 0 ] );
      }

      public int? GetMode()
      {
         return _mode;
      }
   }
}
