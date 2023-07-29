using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Utilities;
using XUnity.Common.Constants;
using XUnity.Common.Extensions;
using XUnity.Common.Logging;
using XUnity.Common.Utilities;

namespace XUnity.AutoTranslator.Plugin.Core.Utilities
{
   internal static class UtageHelper
   {
      private static object AdvManager;
      private static HashSet<string> Labels = new HashSet<string>();

      public static void FixLabel( ref string label )
      {
         var empty = new object[ 0 ];
         if( AdvManager == null && UnityTypes.AdvDataManager != null )
         {
            try
            {
               AdvManager = GameObject.FindObjectOfType( UnityTypes.AdvDataManager.UnityType );
#if IL2CPP
               AdvManager = Il2CppUtilities.CreateProxyComponentWithDerivedType( ( (Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase)AdvManager ).Pointer, UnityTypes.AdvDataManager.ClrType );
#endif

               var ScenarioDataTblProperty = UnityTypes.AdvDataManager.ClrType.GetProperty( "ScenarioDataTbl" );
               var ScenarioDataTbl = ScenarioDataTblProperty.GetValue( AdvManager, empty );

#if IL2CPP
               var iterable1 = new ManagedDictionaryEnumerable( ScenarioDataTbl );
#else
               ScenarioDataTbl.TryCastTo<IEnumerable>( out var iterable1 );
#endif

               foreach( object labelToAdvScenarioDataKeyValuePair in iterable1 )
               {
                  var labelToAdvScenarioDataKeyValuePairType = labelToAdvScenarioDataKeyValuePair.GetType();

                  var AdvScenarioDataKey = (string)labelToAdvScenarioDataKeyValuePairType.GetProperty( "Key" )
                     .GetValue( labelToAdvScenarioDataKeyValuePair, empty );

                  Labels.Add( AdvScenarioDataKey );

                  var AdvScenarioData = labelToAdvScenarioDataKeyValuePairType.GetProperty( "Value" )
                     .GetValue( labelToAdvScenarioDataKeyValuePair, empty );

                  if( AdvScenarioData != null )
                  {
                     var ScenarioLabelsProperty = AdvScenarioData.GetType().GetProperty( "ScenarioLabels" );

                     var labelToAdvScenarioLabelData = ScenarioLabelsProperty.GetValue( AdvScenarioData, empty );

#if IL2CPP
                     var iterable2 = new ManagedDictionaryEnumerable( labelToAdvScenarioLabelData );
#else
                     labelToAdvScenarioLabelData.TryCastTo<IEnumerable>( out var iterable2 );
#endif

                     foreach( object labelToAdvScenarioLabelDataKeyValuePair in iterable2 )
                     {
                        var labelToAdvScenarioLabelDataKeyValuePairType = labelToAdvScenarioLabelDataKeyValuePair.GetType();

                        var AdvScenarioLabelDataKey = (string)labelToAdvScenarioLabelDataKeyValuePairType.GetProperty( "Key" )
                           .GetValue( labelToAdvScenarioLabelDataKeyValuePair, empty );

                        Labels.Add( AdvScenarioLabelDataKey );
                     }
                  }
               }
            }
            catch( Exception e )
            {
               XuaLogger.AutoTranslator.Warn( e, "An error occurred while setting up scenario set." );
            }
         }

         if( !Labels.Contains( label ) )
         {
            var scope = TranslationScopeHelper.GetScope( null );
            if( AutoTranslationPlugin.Current.TextCache.TryGetReverseTranslation( label, scope, out string key ) )
            {
               label = key;
            }
         }
      }
   }
}
