using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace UnityEngine
{
   public class TextEditor
   {
      public enum DblClickSnapping : byte
      {
         WORDS,
         PARAGRAPHS
      }

      private enum CharacterType
      {
         LetterLike,
         Symbol,
         Symbol2,
         WhiteSpace
      }

      private enum TextEditOp
      {
         MoveLeft,
         MoveRight,
         MoveUp,
         MoveDown,
         MoveLineStart,
         MoveLineEnd,
         MoveTextStart,
         MoveTextEnd,
         MovePageUp,
         MovePageDown,
         MoveGraphicalLineStart,
         MoveGraphicalLineEnd,
         MoveWordLeft,
         MoveWordRight,
         MoveParagraphForward,
         MoveParagraphBackward,
         MoveToStartOfNextWord,
         MoveToEndOfPreviousWord,
         SelectLeft,
         SelectRight,
         SelectUp,
         SelectDown,
         SelectTextStart,
         SelectTextEnd,
         SelectPageUp,
         SelectPageDown,
         ExpandSelectGraphicalLineStart,
         ExpandSelectGraphicalLineEnd,
         SelectGraphicalLineStart,
         SelectGraphicalLineEnd,
         SelectWordLeft,
         SelectWordRight,
         SelectToEndOfPreviousWord,
         SelectToStartOfNextWord,
         SelectParagraphBackward,
         SelectParagraphForward,
         Delete,
         Backspace,
         DeleteWordBack,
         DeleteWordForward,
         DeleteLineBack,
         Cut,
         Copy,
         Paste,
         SelectAll,
         SelectNone,
         ScrollStart,
         ScrollEnd,
         ScrollPageUp,
         ScrollPageDown
      }

      public int controlID = 0;

      public GUIStyle style = GUIStyle.none;

      public bool multiline = false;

      public bool hasHorizontalCursorPos = false;

      public bool isPasswordField = false;

      internal bool m_HasFocus;

      public Vector2 scrollOffset = Vector2.zero;

      private GUIContent m_Content = new GUIContent();

      private Rect m_Position;

      private int m_CursorIndex = 0;

      private int m_SelectIndex = 0;

      private bool m_RevealCursor = false;

      public Vector2 graphicalCursorPos;

      public Vector2 graphicalSelectCursorPos;

      private bool m_MouseDragSelectsWholeWords = false;

      private int m_DblClickInitPos = 0;

      private DblClickSnapping m_DblClickSnap = DblClickSnapping.WORDS;

      private bool m_bJustSelected = false;

      private int m_iAltCursorPos = -1;

      private string oldText;

      private int oldPos;

      private int oldSelectPos;

      private static Dictionary<Event, TextEditOp> s_Keyactions;

      [Obsolete( "Please use 'text' instead of 'content'", false )]
      public GUIContent content
      {
         get
         {
            return m_Content;
         }
         set
         {
            m_Content = value;
         }
      }

      public string text
      {
         get
         {
            return m_Content.text;
         }
         set
         {
            m_Content.text = value;
            ClampTextIndex( ref m_CursorIndex );
            ClampTextIndex( ref m_SelectIndex );
         }
      }

      public Rect position
      {
         get
         {
            return m_Position;
         }
         set
         {
            if( !( m_Position == value ) )
            {
               m_Position = value;
               UpdateScrollOffset();
            }
         }
      }

      public int cursorIndex
      {
         get
         {
            return m_CursorIndex;
         }
         set
         {
            int cursorIndex = m_CursorIndex;
            m_CursorIndex = value;
            ClampTextIndex( ref m_CursorIndex );
            if( m_CursorIndex != cursorIndex )
            {
               m_RevealCursor = true;
            }
         }
      }

      public int selectIndex
      {
         get
         {
            return m_SelectIndex;
         }
         set
         {
            m_SelectIndex = value;
            ClampTextIndex( ref m_SelectIndex );
         }
      }

      public bool hasSelection => cursorIndex != selectIndex;

      public string SelectedText
      {
         get
         {
            if( cursorIndex == selectIndex )
            {
               return "";
            }

            if( cursorIndex < selectIndex )
            {
               return text.Substring( cursorIndex, selectIndex - cursorIndex );
            }

            return text.Substring( selectIndex, cursorIndex - selectIndex );
         }
      }

      public TextEditor()
      {
      }

      private void ClearCursorPos()
      {
         hasHorizontalCursorPos = false;
         m_iAltCursorPos = -1;
      }

      public void OnFocus()
      {
         if( multiline )
         {
            int num3 = cursorIndex = ( selectIndex = 0 );
         }
         else
         {
            SelectAll();
         }

         m_HasFocus = true;
      }

      public void OnLostFocus()
      {
         m_HasFocus = false;
         scrollOffset = Vector2.zero;
      }

      private void GrabGraphicalCursorPos()
      {
         if( !hasHorizontalCursorPos )
         {
            graphicalCursorPos = style.GetCursorPixelPosition( position, m_Content, cursorIndex );
            graphicalSelectCursorPos = style.GetCursorPixelPosition( position, m_Content, selectIndex );
            hasHorizontalCursorPos = false;
         }
      }

      public bool HandleKeyEvent( Event e ) => throw new NotImplementedException();

      public bool DeleteLineBack()
      {
         if( hasSelection )
         {
            DeleteSelection();
            return true;
         }

         int num = cursorIndex;
         int num2 = num;
         while( num2-- != 0 )
         {
            if( text[ num2 ] == '\n' )
            {
               num = num2 + 1;
               break;
            }
         }

         if( num2 == -1 )
         {
            num = 0;
         }

         if( cursorIndex != num )
         {
            m_Content.text = text.Remove( num, cursorIndex - num );
            int num5 = selectIndex = ( cursorIndex = num );
            return true;
         }

         return false;
      }

      public bool DeleteWordBack()
      {
         if( hasSelection )
         {
            DeleteSelection();
            return true;
         }

         int num = FindEndOfPreviousWord( cursorIndex );
         if( cursorIndex != num )
         {
            m_Content.text = text.Remove( num, cursorIndex - num );
            int num4 = selectIndex = ( cursorIndex = num );
            return true;
         }

         return false;
      }

      public bool DeleteWordForward()
      {
         if( hasSelection )
         {
            DeleteSelection();
            return true;
         }

         int num = FindStartOfNextWord( cursorIndex );
         if( cursorIndex < text.Length )
         {
            m_Content.text = text.Remove( cursorIndex, num - cursorIndex );
            return true;
         }

         return false;
      }

      public bool Delete()
      {
         if( hasSelection )
         {
            DeleteSelection();
            return true;
         }

         if( cursorIndex < text.Length )
         {
            m_Content.text = text.Remove( cursorIndex, 1 );
            return true;
         }

         return false;
      }

      public bool CanPaste() => throw new NotImplementedException();

      public bool Backspace()
      {
         if( hasSelection )
         {
            DeleteSelection();
            return true;
         }

         if( cursorIndex > 0 )
         {
            m_Content.text = text.Remove( cursorIndex - 1, 1 );
            selectIndex = --cursorIndex;
            ClearCursorPos();
            return true;
         }

         return false;
      }

      public void SelectAll()
      {
         cursorIndex = 0;
         selectIndex = text.Length;
         ClearCursorPos();
      }

      public void SelectNone()
      {
         selectIndex = cursorIndex;
         ClearCursorPos();
      }

      public bool DeleteSelection()
      {
         if( cursorIndex == selectIndex )
         {
            return false;
         }

         if( cursorIndex < selectIndex )
         {
            m_Content.text = text.Substring( 0, cursorIndex ) + text.Substring( selectIndex, text.Length - selectIndex );
            selectIndex = cursorIndex;
         }
         else
         {
            m_Content.text = text.Substring( 0, selectIndex ) + text.Substring( cursorIndex, text.Length - cursorIndex );
            cursorIndex = selectIndex;
         }

         ClearCursorPos();
         return true;
      }

      public void ReplaceSelection( string replace )
      {
         DeleteSelection();
         m_Content.text = text.Insert( cursorIndex, replace );
         selectIndex = ( cursorIndex += replace.Length );
         ClearCursorPos();
      }

      public void Insert( char c )
      {
         ReplaceSelection( c.ToString() );
      }

      public void MoveSelectionToAltCursor()
      {
         if( m_iAltCursorPos != -1 )
         {
            int iAltCursorPos = m_iAltCursorPos;
            string selectedText = SelectedText;
            m_Content.text = text.Insert( iAltCursorPos, selectedText );
            if( iAltCursorPos < cursorIndex )
            {
               cursorIndex += selectedText.Length;
               selectIndex += selectedText.Length;
            }

            DeleteSelection();
            int num3 = selectIndex = ( cursorIndex = iAltCursorPos );
            ClearCursorPos();
         }
      }

      public void MoveRight()
      {
         ClearCursorPos();
         if( selectIndex == cursorIndex )
         {
            cursorIndex++;
            DetectFocusChange();
            selectIndex = cursorIndex;
         }
         else if( selectIndex > cursorIndex )
         {
            cursorIndex = selectIndex;
         }
         else
         {
            selectIndex = cursorIndex;
         }
      }

      public void MoveLeft()
      {
         if( selectIndex == cursorIndex )
         {
            cursorIndex--;
            selectIndex = cursorIndex;
         }
         else if( selectIndex > cursorIndex )
         {
            selectIndex = cursorIndex;
         }
         else
         {
            cursorIndex = selectIndex;
         }

         ClearCursorPos();
      }

      public void MoveUp()
      {
         if( selectIndex < cursorIndex )
         {
            selectIndex = cursorIndex;
         }
         else
         {
            cursorIndex = selectIndex;
         }

         GrabGraphicalCursorPos();
         graphicalCursorPos.y -= 1f;
         int num2 = cursorIndex = ( selectIndex = style.GetCursorStringIndex( position, m_Content, graphicalCursorPos ) );
         if( cursorIndex <= 0 )
         {
            ClearCursorPos();
         }
      }

      public void MoveDown()
      {
         if( selectIndex > cursorIndex )
         {
            selectIndex = cursorIndex;
         }
         else
         {
            cursorIndex = selectIndex;
         }

         GrabGraphicalCursorPos();
         graphicalCursorPos.y += style.lineHeight + 5f;
         int num2 = cursorIndex = ( selectIndex = style.GetCursorStringIndex( position, m_Content, graphicalCursorPos ) );
         if( cursorIndex == text.Length )
         {
            ClearCursorPos();
         }
      }

      public void MoveLineStart()
      {
         int num = ( selectIndex >= cursorIndex ) ? cursorIndex : selectIndex;
         int num2 = num;
         int num5;
         while( num2-- != 0 )
         {
            if( text[ num2 ] == '\n' )
            {
               num5 = ( selectIndex = ( cursorIndex = num2 + 1 ) );
               return;
            }
         }

         num5 = ( selectIndex = ( cursorIndex = 0 ) );
      }

      public void MoveLineEnd()
      {
         int num = ( selectIndex <= cursorIndex ) ? cursorIndex : selectIndex;
         int i = num;
         int length;
         int num4;
         for( length = text.Length ; i < length ; i++ )
         {
            if( text[ i ] == '\n' )
            {
               num4 = ( selectIndex = ( cursorIndex = i ) );
               return;
            }
         }

         num4 = ( selectIndex = ( cursorIndex = length ) );
      }

      public void MoveGraphicalLineStart()
      {
         int num2 = cursorIndex = ( selectIndex = GetGraphicalLineStart( ( cursorIndex >= selectIndex ) ? selectIndex : cursorIndex ) );
      }

      public void MoveGraphicalLineEnd()
      {
         int num2 = cursorIndex = ( selectIndex = GetGraphicalLineEnd( ( cursorIndex <= selectIndex ) ? selectIndex : cursorIndex ) );
      }

      public void MoveTextStart()
      {
         int num3 = selectIndex = ( cursorIndex = 0 );
      }

      public void MoveTextEnd()
      {
         int num2 = selectIndex = ( cursorIndex = text.Length );
      }

      private int IndexOfEndOfLine( int startIndex )
      {
         int num = text.IndexOf( '\n', startIndex );
         return ( num == -1 ) ? text.Length : num;
      }

      public void MoveParagraphForward()
      {
         cursorIndex = ( ( cursorIndex <= selectIndex ) ? selectIndex : cursorIndex );
         if( cursorIndex < text.Length )
         {
            int num3 = selectIndex = ( cursorIndex = IndexOfEndOfLine( cursorIndex + 1 ) );
         }
      }

      public void MoveParagraphBackward()
      {
         cursorIndex = ( ( cursorIndex >= selectIndex ) ? selectIndex : cursorIndex );
         if( cursorIndex > 1 )
         {
            int num3 = selectIndex = ( cursorIndex = text.LastIndexOf( '\n', cursorIndex - 2 ) + 1 );
         }
         else
         {
            int num3 = selectIndex = ( cursorIndex = 0 );
         }
      }

      public void MoveCursorToPosition( Vector2 cursorPosition ) => throw new NotImplementedException();

      public void MoveAltCursorToPosition( Vector2 cursorPosition ) => throw new NotImplementedException();

      public bool IsOverSelection( Vector2 cursorPosition ) => throw new NotImplementedException();

      public void SelectToPosition( Vector2 cursorPosition )
      {
         if( !m_MouseDragSelectsWholeWords )
         {
            cursorIndex = style.GetCursorStringIndex( position, m_Content, cursorPosition + scrollOffset );
            return;
         }

         int num = style.GetCursorStringIndex( position, m_Content, cursorPosition + scrollOffset );
         if( m_DblClickSnap == DblClickSnapping.WORDS )
         {
            if( num < m_DblClickInitPos )
            {
               cursorIndex = FindEndOfClassification( num, -1 );
               selectIndex = FindEndOfClassification( m_DblClickInitPos, 1 );
               return;
            }

            if( num >= text.Length )
            {
               num = text.Length - 1;
            }

            cursorIndex = FindEndOfClassification( num, 1 );
            selectIndex = FindEndOfClassification( m_DblClickInitPos - 1, -1 );
         }
         else if( num < m_DblClickInitPos )
         {
            if( num > 0 )
            {
            }
            else
            {
               cursorIndex = 0;
            }

            selectIndex = text.LastIndexOf( '\n', m_DblClickInitPos );
         }
         else
         {
            if( num < text.Length )
            {
               cursorIndex = IndexOfEndOfLine( num );
            }
            else
            {
               cursorIndex = text.Length;
            }

         }
      }

      public void SelectLeft()
      {
         if( m_bJustSelected && this.cursorIndex > selectIndex )
         {
            int cursorIndex = this.cursorIndex;
            this.cursorIndex = selectIndex;
            selectIndex = cursorIndex;
         }

         m_bJustSelected = false;
         this.cursorIndex--;
      }

      public void SelectRight()
      {
         if( m_bJustSelected && this.cursorIndex < selectIndex )
         {
            int cursorIndex = this.cursorIndex;
            this.cursorIndex = selectIndex;
            selectIndex = cursorIndex;
         }

         m_bJustSelected = false;
         this.cursorIndex++;
      }

      public void SelectUp()
      {
         GrabGraphicalCursorPos();
         graphicalCursorPos.y -= 1f;
         cursorIndex = style.GetCursorStringIndex( position, m_Content, graphicalCursorPos );
      }

      public void SelectDown()
      {
         GrabGraphicalCursorPos();
         graphicalCursorPos.y += style.lineHeight + 5f;
         cursorIndex = style.GetCursorStringIndex( position, m_Content, graphicalCursorPos );
      }

      public void SelectTextEnd()
      {
         cursorIndex = text.Length;
      }

      public void SelectTextStart()
      {
         cursorIndex = 0;
      }

      public void MouseDragSelectsWholeWords( bool on )
      {
         m_MouseDragSelectsWholeWords = on;
         m_DblClickInitPos = cursorIndex;
      }

      public void DblClickSnap( DblClickSnapping snapping )
      {
         m_DblClickSnap = snapping;
      }

      private int GetGraphicalLineStart( int p )
      {
         Vector2 cursorPixelPosition = style.GetCursorPixelPosition( position, m_Content, p );
         cursorPixelPosition.x = 0f;
         return style.GetCursorStringIndex( position, m_Content, cursorPixelPosition );
      }

      private int GetGraphicalLineEnd( int p )
      {
         Vector2 cursorPixelPosition = style.GetCursorPixelPosition( position, m_Content, p );
         cursorPixelPosition.x += 5000f;
         return style.GetCursorStringIndex( position, m_Content, cursorPixelPosition );
      }

      private int FindNextSeperator( int startPos )
      {
         int length = text.Length;
         while( startPos < length && !isLetterLikeChar( text[ startPos ] ) )
         {
            startPos++;
         }

         while( startPos < length && isLetterLikeChar( text[ startPos ] ) )
         {
            startPos++;
         }

         return startPos;
      }

      private static bool isLetterLikeChar( char c )
      {
         return char.IsLetterOrDigit( c ) || c == '\'';
      }

      private int FindPrevSeperator( int startPos )
      {
         startPos--;
         while( startPos > 0 && !isLetterLikeChar( text[ startPos ] ) )
         {
            startPos--;
         }

         while( startPos >= 0 && isLetterLikeChar( text[ startPos ] ) )
         {
            startPos--;
         }

         return startPos + 1;
      }

      public void MoveWordRight()
      {
         cursorIndex = ( ( cursorIndex <= selectIndex ) ? selectIndex : cursorIndex );
         int num3 = cursorIndex = ( selectIndex = FindNextSeperator( cursorIndex ) );
         ClearCursorPos();
      }

      public void MoveToStartOfNextWord()
      {
         ClearCursorPos();
         if( cursorIndex != selectIndex )
         {
            MoveRight();
         }
         else
         {
            int num3 = cursorIndex = ( selectIndex = FindStartOfNextWord( cursorIndex ) );
         }
      }

      public void MoveToEndOfPreviousWord()
      {
         ClearCursorPos();
         if( cursorIndex != selectIndex )
         {
            MoveLeft();
         }
         else
         {
            int num3 = cursorIndex = ( selectIndex = FindEndOfPreviousWord( cursorIndex ) );
         }
      }

      public void SelectToStartOfNextWord()
      {
         ClearCursorPos();
         cursorIndex = FindStartOfNextWord( cursorIndex );
      }

      public void SelectToEndOfPreviousWord()
      {
         ClearCursorPos();
         cursorIndex = FindEndOfPreviousWord( cursorIndex );
      }

      private CharacterType ClassifyChar( char c )
      {
         if( char.IsWhiteSpace( c ) )
         {
            return CharacterType.WhiteSpace;
         }

         if( char.IsLetterOrDigit( c ) || c == '\'' )
         {
            return CharacterType.LetterLike;
         }

         return CharacterType.Symbol;
      }

      public int FindStartOfNextWord( int p )
      {
         int length = text.Length;
         if( p == length )
         {
            return p;
         }

         char c = text[ p ];
         CharacterType characterType = ClassifyChar( c );
         if( characterType != CharacterType.WhiteSpace )
         {
            p++;
            while( p < length && ClassifyChar( text[ p ] ) == characterType )
            {
               p++;
            }
         }
         else if( c == '\t' || c == '\n' )
         {
            return p + 1;
         }

         if( p == length )
         {
            return p;
         }

         c = text[ p ];
         if( c == ' ' )
         {
            while( p < length && char.IsWhiteSpace( text[ p ] ) )
            {
               p++;
            }
         }
         else if( c == '\t' || c == '\n' )
         {
            return p;
         }

         return p;
      }

      private int FindEndOfPreviousWord( int p )
      {
         if( p == 0 )
         {
            return p;
         }

         p--;
         while( p > 0 && text[ p ] == ' ' )
         {
            p--;
         }

         CharacterType characterType = ClassifyChar( text[ p ] );
         if( characterType != CharacterType.WhiteSpace )
         {
            while( p > 0 && ClassifyChar( text[ p - 1 ] ) == characterType )
            {
               p--;
            }
         }

         return p;
      }

      public void MoveWordLeft()
      {
         cursorIndex = ( ( cursorIndex >= selectIndex ) ? selectIndex : cursorIndex );
         cursorIndex = FindPrevSeperator( cursorIndex );
         selectIndex = cursorIndex;
      }

      public void SelectWordRight()
      {
         ClearCursorPos();
         int selectIndex = this.selectIndex;
         if( cursorIndex < this.selectIndex )
         {
            this.selectIndex = cursorIndex;
            MoveWordRight();
            this.selectIndex = selectIndex;
            cursorIndex = ( ( cursorIndex >= this.selectIndex ) ? this.selectIndex : cursorIndex );
         }
         else
         {
            this.selectIndex = cursorIndex;
            MoveWordRight();
            this.selectIndex = selectIndex;
         }
      }

      public void SelectWordLeft()
      {
         ClearCursorPos();
         int selectIndex = this.selectIndex;
         if( cursorIndex > this.selectIndex )
         {
            this.selectIndex = cursorIndex;
            MoveWordLeft();
            this.selectIndex = selectIndex;
            cursorIndex = ( ( cursorIndex <= this.selectIndex ) ? this.selectIndex : cursorIndex );
         }
         else
         {
            this.selectIndex = cursorIndex;
            MoveWordLeft();
            this.selectIndex = selectIndex;
         }
      }

      public void ExpandSelectGraphicalLineStart()
      {
         ClearCursorPos();
         if( this.cursorIndex < selectIndex )
         {
            this.cursorIndex = GetGraphicalLineStart( this.cursorIndex );
            return;
         }

         int cursorIndex = this.cursorIndex;
         this.cursorIndex = GetGraphicalLineStart( selectIndex );
         selectIndex = cursorIndex;
      }

      public void ExpandSelectGraphicalLineEnd()
      {
         ClearCursorPos();
         if( this.cursorIndex > selectIndex )
         {
            this.cursorIndex = GetGraphicalLineEnd( this.cursorIndex );
            return;
         }

         int cursorIndex = this.cursorIndex;
         this.cursorIndex = GetGraphicalLineEnd( selectIndex );
         selectIndex = cursorIndex;
      }

      public void SelectGraphicalLineStart()
      {
         ClearCursorPos();
         cursorIndex = GetGraphicalLineStart( cursorIndex );
      }

      public void SelectGraphicalLineEnd()
      {
         ClearCursorPos();
         cursorIndex = GetGraphicalLineEnd( cursorIndex );
      }

      public void SelectParagraphForward()
      {
         ClearCursorPos();
         bool flag = cursorIndex < selectIndex;
         if( cursorIndex < text.Length )
         {
            cursorIndex = IndexOfEndOfLine( cursorIndex + 1 );
            if( flag && cursorIndex > selectIndex )
            {
               cursorIndex = selectIndex;
            }
         }
      }

      public void SelectParagraphBackward()
      {
         ClearCursorPos();
         bool flag = cursorIndex > selectIndex;
         if( cursorIndex > 1 )
         {
            cursorIndex = text.LastIndexOf( '\n', cursorIndex - 2 ) + 1;
            if( flag && cursorIndex < selectIndex )
            {
               cursorIndex = selectIndex;
            }
         }
         else
         {
            int num3 = selectIndex = ( cursorIndex = 0 );
         }
      }

      public void SelectCurrentWord()
      {
         ClearCursorPos();
         int length = text.Length;
         selectIndex = cursorIndex;
         if( length != 0 )
         {
            if( cursorIndex >= length )
            {
               cursorIndex = length - 1;
            }

            if( selectIndex >= length )
            {
               selectIndex--;
            }

            if( cursorIndex < selectIndex )
            {
               cursorIndex = FindEndOfClassification( cursorIndex, -1 );
               selectIndex = FindEndOfClassification( selectIndex, 1 );
            }
            else
            {
               cursorIndex = FindEndOfClassification( cursorIndex, 1 );
               selectIndex = FindEndOfClassification( selectIndex, -1 );
            }

            m_bJustSelected = true;
         }
      }

      private int FindEndOfClassification( int p, int dir )
      {
         int length = text.Length;
         if( p >= length || p < 0 )
         {
            return p;
         }

         CharacterType characterType = ClassifyChar( text[ p ] );
         do
         {
            p += dir;
            if( p < 0 )
            {
               return 0;
            }

            if( p >= length )
            {
               return length;
            }
         }
         while( ClassifyChar( text[ p ] ) == characterType );
         if( dir == 1 )
         {
            return p;
         }

         return p + 1;
      }

      public void SelectCurrentParagraph()
      {
         ClearCursorPos();
         int length = text.Length;
         if( cursorIndex < length )
         {
            cursorIndex = IndexOfEndOfLine( cursorIndex ) + 1;
         }

         if( selectIndex != 0 )
         {
            selectIndex = text.LastIndexOf( '\n', selectIndex - 1 ) + 1;
         }
      }

      public void UpdateScrollOffsetIfNeeded( Event evt ) => throw new NotImplementedException();

      private void UpdateScrollOffset()
      {
         int cursorIndex = this.cursorIndex;
         graphicalCursorPos = style.GetCursorPixelPosition( new Rect( 0f, 0f, position.width, position.height ), m_Content, cursorIndex );
         Rect rect = style.padding.Remove( position );
         Vector2 vector = style.CalcSize( m_Content );
         Vector2 vector2 = new Vector2( vector.x, style.CalcHeight( m_Content, position.width ) );
         if( vector2.x < position.width )
         {
            scrollOffset.x = 0f;
         }
         else if( m_RevealCursor )
         {
            if( graphicalCursorPos.x + 1f > scrollOffset.x + rect.width )
            {
               scrollOffset.x = graphicalCursorPos.x - rect.width;
            }

            if( graphicalCursorPos.x < scrollOffset.x + (float)style.padding.left )
            {
               scrollOffset.x = graphicalCursorPos.x - (float)style.padding.left;
            }
         }

         if( vector2.y < rect.height )
         {
            scrollOffset.y = 0f;
         }
         else if( m_RevealCursor )
         {
            if( graphicalCursorPos.y + style.lineHeight > scrollOffset.y + rect.height + (float)style.padding.top )
            {
               scrollOffset.y = graphicalCursorPos.y - rect.height - (float)style.padding.top + style.lineHeight;
            }

            if( graphicalCursorPos.y < scrollOffset.y + (float)style.padding.top )
            {
               scrollOffset.y = graphicalCursorPos.y - (float)style.padding.top;
            }
         }

         if( scrollOffset.y > 0f && vector2.y - scrollOffset.y < rect.height )
         {
            scrollOffset.y = vector2.y - rect.height - (float)style.padding.top - (float)style.padding.bottom;
         }

         scrollOffset.y = ( ( !( scrollOffset.y < 0f ) ) ? scrollOffset.y : 0f );
         m_RevealCursor = false;
      }

      public void DrawCursor( string newText )
      {
         string text = this.text;
         int num = cursorIndex;
         if( Input.compositionString.Length > 0 )
         {
            m_Content.text = newText.Substring( 0, cursorIndex ) + Input.compositionString + newText.Substring( selectIndex );
            num += Input.compositionString.Length;
         }
         else
         {
            m_Content.text = newText;
         }

         graphicalCursorPos = style.GetCursorPixelPosition( new Rect( 0f, 0f, position.width, position.height ), m_Content, num );
         Vector2 contentOffset = style.contentOffset;
         style.contentOffset -= scrollOffset;
         style.Internal_clipOffset = scrollOffset;
         Input.compositionCursorPos = graphicalCursorPos + new Vector2( position.x, position.y + style.lineHeight ) - scrollOffset;
         if( Input.compositionString.Length > 0 )
         {
            style.DrawWithTextSelection( position, m_Content, controlID, cursorIndex, cursorIndex + Input.compositionString.Length, drawSelectionAsComposition: true );
         }
         else
         {
            style.DrawWithTextSelection( position, m_Content, controlID, cursorIndex, selectIndex );
         }

         if( m_iAltCursorPos != -1 )
         {
            style.DrawCursor( position, m_Content, controlID, m_iAltCursorPos );
         }

         style.contentOffset = contentOffset;
         style.Internal_clipOffset = Vector2.zero;
         m_Content.text = text;
      }

      private bool PerformOperation( TextEditOp operation )
      {
         m_RevealCursor = true;
         switch( operation )
         {
            case TextEditOp.MoveLeft:
               MoveLeft();
               break;
            case TextEditOp.MoveRight:
               MoveRight();
               break;
            case TextEditOp.MoveUp:
               MoveUp();
               break;
            case TextEditOp.MoveDown:
               MoveDown();
               break;
            case TextEditOp.MoveLineStart:
               MoveLineStart();
               break;
            case TextEditOp.MoveLineEnd:
               MoveLineEnd();
               break;
            case TextEditOp.MoveWordRight:
               MoveWordRight();
               break;
            case TextEditOp.MoveToStartOfNextWord:
               MoveToStartOfNextWord();
               break;
            case TextEditOp.MoveToEndOfPreviousWord:
               MoveToEndOfPreviousWord();
               break;
            case TextEditOp.MoveWordLeft:
               MoveWordLeft();
               break;
            case TextEditOp.MoveTextStart:
               MoveTextStart();
               break;
            case TextEditOp.MoveTextEnd:
               MoveTextEnd();
               break;
            case TextEditOp.MoveParagraphForward:
               MoveParagraphForward();
               break;
            case TextEditOp.MoveParagraphBackward:
               MoveParagraphBackward();
               break;
            case TextEditOp.MoveGraphicalLineStart:
               MoveGraphicalLineStart();
               break;
            case TextEditOp.MoveGraphicalLineEnd:
               MoveGraphicalLineEnd();
               break;
            case TextEditOp.SelectLeft:
               SelectLeft();
               break;
            case TextEditOp.SelectRight:
               SelectRight();
               break;
            case TextEditOp.SelectUp:
               SelectUp();
               break;
            case TextEditOp.SelectDown:
               SelectDown();
               break;
            case TextEditOp.SelectWordRight:
               SelectWordRight();
               break;
            case TextEditOp.SelectWordLeft:
               SelectWordLeft();
               break;
            case TextEditOp.SelectToEndOfPreviousWord:
               SelectToEndOfPreviousWord();
               break;
            case TextEditOp.SelectToStartOfNextWord:
               SelectToStartOfNextWord();
               break;
            case TextEditOp.SelectTextStart:
               SelectTextStart();
               break;
            case TextEditOp.SelectTextEnd:
               SelectTextEnd();
               break;
            case TextEditOp.ExpandSelectGraphicalLineStart:
               ExpandSelectGraphicalLineStart();
               break;
            case TextEditOp.ExpandSelectGraphicalLineEnd:
               ExpandSelectGraphicalLineEnd();
               break;
            case TextEditOp.SelectParagraphForward:
               SelectParagraphForward();
               break;
            case TextEditOp.SelectParagraphBackward:
               SelectParagraphBackward();
               break;
            case TextEditOp.SelectGraphicalLineStart:
               SelectGraphicalLineStart();
               break;
            case TextEditOp.SelectGraphicalLineEnd:
               SelectGraphicalLineEnd();
               break;
            case TextEditOp.Delete:
               return Delete();
            case TextEditOp.Backspace:
               return Backspace();
            case TextEditOp.Cut:
               return Cut();
            case TextEditOp.Copy:
               Copy();
               break;
            case TextEditOp.Paste:
               return Paste();
            case TextEditOp.SelectAll:
               SelectAll();
               break;
            case TextEditOp.SelectNone:
               SelectNone();
               break;
            case TextEditOp.DeleteWordBack:
               return DeleteWordBack();
            case TextEditOp.DeleteLineBack:
               return DeleteLineBack();
            case TextEditOp.DeleteWordForward:
               return DeleteWordForward();
            default:
               break;
         }

         return false;
      }

      public void SaveBackup()
      {
         oldText = text;
         oldPos = cursorIndex;
         oldSelectPos = selectIndex;
      }

      public void Undo()
      {
         m_Content.text = oldText;
         cursorIndex = oldPos;
         selectIndex = oldSelectPos;
      }

      public bool Cut()
      {
         if( isPasswordField )
         {
            return false;
         }

         Copy();
         return DeleteSelection();
      }

      public void Copy() => throw new NotImplementedException();

      private static string ReplaceNewlinesWithSpaces( string value ) => throw new NotImplementedException();

      public bool Paste() => throw new NotImplementedException();

      private static void MapKey( string key, TextEditOp action ) => throw new NotImplementedException();

      private void InitKeyActions() => throw new NotImplementedException();

      public void DetectFocusChange() => throw new NotImplementedException();

      private void ClampTextIndex( ref int index ) => throw new NotImplementedException();
   }
}
