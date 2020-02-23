using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KoikatsuTextResourceRedirector
{
   /// <summary>
   /// All credits to https://github.com/GeBo1 for implementation.
   /// </summary>
   class TextAssetHelper
   {
      public bool Enabled { get; }
      public IEnumerable<string> RowSplitStrings { get; }
      public IEnumerable<string> ColSplitStrings { get; }
      public IEnumerable<string> InvalidColStrings { get; }

      public TextAssetHelper( IEnumerable<string> rowSplitStrings = null, IEnumerable<string> colSplitStrings = null )
      {
         int comp( string a, string b ) => b.Length.CompareTo( a.Length );

         List<string> tmpList = new List<string>();

         // row split strings
         tmpList.AddRange( rowSplitStrings?.ToArray() ?? new string[ 0 ] );
         tmpList.Sort( comp );
         RowSplitStrings = tmpList.ToArray();

         // col split strings
         tmpList.Clear();
         tmpList.AddRange( colSplitStrings?.ToArray() ?? new string[ 0 ] );
         tmpList.Sort( comp );
         ColSplitStrings = tmpList.ToArray();

         // invalid col strings
         tmpList.Clear();
         tmpList.AddRange( RowSplitStrings );
         tmpList.AddRange( ColSplitStrings );
         tmpList.Sort( comp );
         InvalidColStrings = tmpList.ToArray();

         Enabled = ColSplitStrings.Any() && RowSplitStrings.Any();

      }

      public bool IsTable( TextAsset textAsset )
      {
         return IsTable( textAsset.text );
      }

      public bool IsTable( string table )
      {
         foreach( string colSplit in ColSplitStrings )
         {
            if( table.Contains( colSplit ) )
            {
               return true;
            }
         }
         return false;
      }

      public bool IsTableRow( string row )
      {
         foreach( string rowSplit in ColSplitStrings )
         {
            if( row.Contains( rowSplit ) )
            {
               return false;
            }
         }
         return true;
      }

      public IEnumerable<string> SplitTableToRows( TextAsset textAsset )
      {
         return SplitTableToRows( textAsset.text );
      }

      public IEnumerable<string> SplitTableToRows( string table )
      {
         if( !IsTable( table ) )
         {
            return null;
         }
         return table.Split( RowSplitStrings.ToArray(), StringSplitOptions.None );
      }

      public IEnumerable<string> SplitRowToCells( string row )
      {

#if DEBUG
            if (!IsTableRow(row))
            {
                throw new ArgumentException("row does not contain a table row)");
            }
#endif
         return row.Split( ColSplitStrings.ToArray(), StringSplitOptions.None );
      }

      public void ActOnCells( TextAsset textAsset, Action<string> cellAction, out TableResult tableResult )
      {
         ActOnCells( textAsset, ( cell ) => { cellAction( cell ); return false; }, out tableResult );
      }

      public bool ActOnCells( TextAsset textAsset, Func<string, bool> cellAction, out TableResult tableResult )
      {
         tableResult = new TableResult();
         //foreach (string row in EnumerateRows(textAsset))

         var rows = SplitTableToRows( textAsset );
         if( rows == null ) return false;

         foreach( string row in rows )
         {
            tableResult.Rows++;
            int colCount = 0;

            //foreach (string col in EnumerateCols(row))
            foreach( string col in SplitRowToCells( row ) )
            {
               colCount++;
               if( cellAction( col ) )
               {
                  tableResult.CellsActedOn++;
               }
            }
            tableResult.Cols = Math.Max( tableResult.Cols, colCount );
         }
         return tableResult.CellsActedOn > 0;
      }

      public string ProcessTable( TextAsset textAsset, Func<string, string> columnTransform, out TableResult tableResult )
      {
         tableResult = new TableResult();
         string colJoin = ColSplitStrings.First();
         StringBuilder result = new StringBuilder( textAsset.text.Length * 2 );
         //foreach (string row in EnumerateRows(textAsset))
         var rows = SplitTableToRows( textAsset );
         if( rows == null ) return null;

         foreach( string row in rows )
         {
            tableResult.Rows++;
            int colCount = 0;

            bool rowUpdated = false;
            //foreach (string col in EnumerateCols(row))
            foreach( string col in SplitRowToCells( row ) )
            {
               colCount++;
               string newCol = columnTransform( col );
               if( newCol != null && col != newCol )
               {
                  tableResult.CellsUpdated++;
                  rowUpdated = true;
                  foreach( string invalid in InvalidColStrings )
                  {
                     newCol = newCol.Replace( invalid, " " );
                  }
                  result.Append( newCol );
               }
               else
               {
                  result.Append( col );
               }
               result.Append( colJoin );
            }
            // row complete
            // remove trailing colSplit
            result.Length -= colJoin.Length;
            result.Append( Environment.NewLine );
            if( rowUpdated )
            {
               tableResult.RowsUpdated++;
            }
            tableResult.Cols = Math.Max( tableResult.Cols, colCount );
         }
         // table complete
         // remove last newline
         result.Length -= Environment.NewLine.Length;

         if( !tableResult.Updated )
         {
            return textAsset.text;
         }
         return result.ToString();
      }

      public class TableResult
      {
         public int Rows;
         public int Cols;
         public int RowsUpdated;
         public int CellsUpdated;
         public int CellsActedOn;

         public TableResult()
         {
            Rows = Cols = RowsUpdated = CellsUpdated = CellsActedOn = 0;
         }

         public bool Updated { get => RowsUpdated > 0; }

      }
   }
}
