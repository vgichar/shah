using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Text;
using Valil.Chess.Model.Properties;

namespace Valil.Chess.Model
{
    /// <summary>
    /// Game string description proxy.
    /// </summary>
    public class GameStringDescriptionProxy : Component, INotifyPropertyChanged
    {
        /// <summary>
        /// Maximum line width.
        /// </summary>
        private const int MaxLineWidth = 80;

        /// <summary>
        /// The model.
        /// </summary>
        private ValilGame model;

        /// <summary>
        /// The move history string description list.
        /// </summary>
        private MoveHistoryStringDescriptionBindingList moveHistoryStringDescriptionList;

        /// <summary>
        /// The game status text.
        /// </summary>
        private string gameStatusText;
        /// <summary>
        /// The side to move text.
        /// </summary>
        private string sideToMoveText;

        /// <summary>
        /// PropertyChanged event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises PropertyChanged event.
        /// </summary>
        /// <param name="propertyName"></param>
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        }

        /// <summary>
        /// The model.
        /// </summary>
        /// <value></value>
        public ValilGame Model
        {
            get { return model; }
            set
            {
                // the model can be set only once 
                if (model == null && value != null)
                {
                    // the game must not be initialized
                    if (value.IsInitialized) { throw new ArgumentException(Resources.IllegalGameStateMsg, "model"); }

                    model = value;

                    // hook the events
                    model.Moving += model_Moving;
                    model.Moved += model_Moved;
                    model.GoneBack += model_GoneBack;
                    model.GoneForward += model_GoneForward;
                    model.Modified += model_Modified;
                    model.BoardConfigurationLoaded += model_BoardConfigurationLoaded;
                    model.GameBoardConfigurationLoaded += model_GameBoardConfigurationLoaded;
                    model.GameMoveSectionLoaded += model_GameMoveSectionLoaded;

                    isModelLoadingMoves = false;

                    // start to raise list change events
                    moveHistoryStringDescriptionList.RaiseListChangeEvents = true;
                }
            }
        }

        /// <summary>
        /// The game status text.
        /// </summary>
        public string GameStatusText
        {
            get { return gameStatusText; }

            private set
            {
                if (gameStatusText != value)
                {
                    gameStatusText = value;
                    OnPropertyChanged("GameStatusText");
                }
            }
        }

        /// <summary>
        /// The side to move text.
        /// </summary>
        /// <value></value>
        public string SideToMoveText
        {
            get { return sideToMoveText; }

            private set
            {
                if (sideToMoveText != value)
                {
                    sideToMoveText = value;
                    OnPropertyChanged("SideToMoveText");
                }
            }
        }

        /// <summary>
        /// The move history string description list.
        /// </summary>
        /// <value></value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public MoveHistoryStringDescriptionBindingList MoveHistoryStringDescriptionList
        {
            get { return moveHistoryStringDescriptionList; }
        }

        /// <summary>
        /// Keeps the STR without the result tag.
        /// </summary>
        private string sixTagPairs;
        /// <summary>
        /// Keeps the FEN tag pairs.
        /// </summary>
        private string fenTagPairs;
        /// <summary>
        /// Keeps the result ("*", "0-1", "1-0" or "1/2-1/2").
        /// </summary>
        private string result;

        /// <summary>
        /// Helper string for constructing the move index.
        /// </summary>
        private string tempIndex;
        /// <summary>
        /// Helper string for constructing the move SAN.
        /// </summary>
        private string tempSAN;

        /// <summary>
        /// The autosave handler delegate.
        /// </summary>
        private AutoSavePGNHandler save;
        /// <summary>
        /// True to if the model loads moves, false otherwise.
        /// </summary>
        private bool isModelLoadingMoves;

        /// <summary>
        /// The autosave handler delegate.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public AutoSavePGNHandler Save
        {
            get { return save; }
            set { save = value; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameStringDescriptionProxy()
        {
            moveHistoryStringDescriptionList = new MoveHistoryStringDescriptionBindingList();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="container">The container for the component.</param>
        public GameStringDescriptionProxy(IContainer container)
            : this()
        {
            container.Add(this);
        }

        private void model_Moving(object sender, CancelMoveEventArgs e)
        {
            // set the index information
            tempIndex = Utils.GetSANIndex(model);

            // set the SAN information before the move is made
            tempSAN = Utils.GetSANBegin(model, e.Move);
        }

        private void model_Moved(object sender, MoveEventArgs e)
        {
            // if there are more moves in this list than the move index
            // it means that the model moved when the current board index was not the last one
            if (e.Index < moveHistoryStringDescriptionList.Count)
            {
                moveHistoryStringDescriptionList.RemoveRange(e.Index, moveHistoryStringDescriptionList.Count - e.Index);
            }

            // set the rest of the SAN information after the move is made
            tempSAN += Utils.GetSANEnd(model, e.Move);

            // add the move string description to the history
            moveHistoryStringDescriptionList.Add(new MoveStringDescription(tempIndex, tempSAN));

            // set the board index
            moveHistoryStringDescriptionList.BoardIndex = e.Index + 1;

            // set the result
            SetResult();

            // set the statuses text
            SetStatusesText();

            // try to autosave - call the autosave handler delegate
            // if it's not null and if the model is not loading moves
            if (Save != null && !isModelLoadingMoves)
            {
                Save();
            }
        }

        void model_GoneForward(object sender, MoveEventArgs e)
        {
            // set the board index
            moveHistoryStringDescriptionList.BoardIndex = e.Index + 1;

            // set the statuses text
            SetStatusesText();
        }

        void model_GoneBack(object sender, MoveEventArgs e)
        {
            // set the board index
            moveHistoryStringDescriptionList.BoardIndex = e.Index + 1;

            // set the statuses text
            SetStatusesText();
        }

        void model_Modified(object sender, EventArgs e)
        {
            if (model.IsFirst)
            {
                // set the board index before the first move
                moveHistoryStringDescriptionList.BoardIndex = 0;
            }

            if (model.IsLast)
            {
                // set the board index after the last move
                moveHistoryStringDescriptionList.BoardIndex = moveHistoryStringDescriptionList.Count;
            }

            // set the statuses text
            SetStatusesText();
        }

        /// <summary>
        /// Initialize from a board configuration.
        /// </summary>
        private void Initialize()
        {
            sixTagPairs = "[Event \"?\"]\r\n[Site \"?\"]\r\n[Date \"" + DateTime.Now.ToString("yyyy.MM.dd", CultureInfo.InvariantCulture) + "\"]\r\n[Round \"?\"]\r\n[White \"?\"]\r\n[Black \"?\"]\r\n";// set the six tag pairs

            // if the game is not in starting position, set the FEN tag pairs
            if (!model.CurrentBoard.IsInStartingPosition())
            {
                fenTagPairs = "[SetUp \"1\"]\r\n[FEN \"" + Utils.GetFEN(model.CurrentBoard) + "\"]\r\n";
            }
            else
            {
                fenTagPairs = "";
            }

            // set the result 
            SetResult();

            // clear the move history
            moveHistoryStringDescriptionList.Clear();
        }

        private void model_BoardConfigurationLoaded(object sender, EventArgs e)
        {
            // initialize
            Initialize();

            // try to autosave - call the autosave handler delegate, if any
            if (Save != null) { Save(); }

            // set the board index after the last move
            moveHistoryStringDescriptionList.BoardIndex = moveHistoryStringDescriptionList.Count;

            // set the statuses text
            SetStatusesText();
        }

        private void model_GameBoardConfigurationLoaded(object sender, EventArgs e)
        {
            // initialize
            Initialize();

            // stop autosaving
            isModelLoadingMoves = true;

            // stop raising ListChanged events
            moveHistoryStringDescriptionList.RaiseListChangeEvents = false;
        }

        private void model_GameMoveSectionLoaded(object sender, EventArgs e)
        {
            // the game is loaded, start autosaving again
            isModelLoadingMoves = false;

            // try to autosave - call the autosave handler delegate, if any
            if (Save != null) { Save(); }

            // start raising ListChanged events
            moveHistoryStringDescriptionList.RaiseListChangeEvents = true;

            // refresh the binding list
            moveHistoryStringDescriptionList.Refresh();

            // set the statuses text
            SetStatusesText();
        }

        /// <summary>
        /// Sets the result.
        /// </summary>
        private void SetResult()
        {
            switch (model.Status)
            {
                case GameStatus.Normal:
                    result = "*";
                    break;
                case GameStatus.Checkmate:
                    result = model.CurrentBoard.Status.WhiteTurn ? "0-1" : "1-0";
                    break;
                case GameStatus.Draw50Move:
                case GameStatus.DrawInsufficientMaterial:
                case GameStatus.DrawRepetition:
                case GameStatus.Stalemate:
                    result = "1/2-1/2";
                    break;
                default:
                    result = "*";
                    break;
            }
        }

        /// <summary>
        /// Write the game to a text writer in PGN.
        /// Does nothing if the model is null.
        /// </summary>
        /// <param name="tw"></param>
        public void WritePGNTo(TextWriter tw)
        {
            if (model == null) { return; }

            // write the six tag pairs
            tw.Write(sixTagPairs);

            // write the Result tag pair
            tw.Write("[Result \"");
            tw.Write(result);
            tw.Write("\"]\r\n");

            // write the FEN tag pairs
            tw.Write(fenTagPairs);

            tw.Write("\r\n");

            int? lineWidth = null;

            // loop through the history
            foreach (MoveStringDescription moveStringDescr in moveHistoryStringDescriptionList)
            {
                // if this is the start of the move text section or White is to move write the index
                // lineWidth can be null only at the begining!
                if (lineWidth == null || moveStringDescr.IsWhiteMove)
                {
                    if (lineWidth == null) { lineWidth = 0; }

                    // if the line width will be more than MaxLineWidth add a new line
                    if (lineWidth + moveStringDescr.Index.Length + 1 > MaxLineWidth)
                    {
                        tw.Write('\r');
                        tw.Write('\n');
                        lineWidth = 0;
                    }

                    tw.Write(moveStringDescr.Index);
                    tw.Write(' ');
                    lineWidth += moveStringDescr.Index.Length + 1;
                }

                // if the line width will be more than MaxLineWidth add a new line
                if (lineWidth + moveStringDescr.SAN.Length + 1 > MaxLineWidth)
                {
                    tw.Write('\r');
                    tw.Write('\n');
                    lineWidth = 0;
                }

                // write the move SAN
                tw.Write(moveStringDescr.SAN);
                tw.Write(' ');
                lineWidth += moveStringDescr.SAN.Length + 1;
            }

            // write the result at the end
            tw.Write(result);

            tw.Flush();
        }

        /// <summary>
        /// Loads this board configuration into the model.
        /// Throws ArgumentException if the board configuration is not valid.
        /// Does nothing if the model is null.
        /// </summary>
        /// <param name="fen"></param>
        public void LoadBoardConfiguration(string fen)
        {
            if (model == null) { return; }

            // build the event args
            CancelEventArgs emptyArgs = new CancelEventArgs();

            // raise the Loading event on the model
            model.OnLoading(emptyArgs);

            // if the operation was cancelled
            if (emptyArgs.Cancel) { return; }

            // set the board
            try
            {
                model.CurrentBoard = Utils.GetFENBoard(fen);
            }
            catch
            {
                throw;
            }
            finally
            {
                // raise the BoardConfigurationLoaded event on the model
                model.OnBoardConfigurationLoaded(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the board configuration as a FEN string.
        /// Returns null if the model is null or not initialized.
        /// </summary>
        public string GetBoardConfiguration()
        {
            return model != null && model.IsInitialized ? Utils.GetFEN(model.CurrentBoard) : null;
        }

        /// <summary>
        /// Initializes the model from PGN tag pair section.
        /// Throws ArgumentException if the board configuration is not valid.
        /// </summary>
        /// <param name="tr">The text reader which is read from</param>
        private void InitializeGameFromPGNTagPairs(TextReader tr)
        {
            string line;

            // skip the empty lines
            do
            {
                line = tr.ReadLine();
            }
            while (line != null && line.Length == 0);

            // read the tag pair section
            // we are interested only if the game is not from the starting position
            // in which case the game is initialized with the FEN
            while (line != null && line.Length > 0 && line[0] == '[' && line[line.Length - 1] == ']')
            {
                if (line.Length > 8 && line[1] == 'F' && line[2] == 'E' && line[3] == 'N' && line[4] == ' ' && line[5] == '"' && line[line.Length - 2] == '"')
                {
                    model.CurrentBoard = Utils.GetFENBoard(line.Substring(6, line.Length - 8));
                    return;
                }
                line = tr.ReadLine();
            }

            // if there was no FEN
            // initialize the model with the starting position
            model.CurrentBoard = Board.GetStartingBoard();
        }

        /// <summary>
        /// Traverses the PGN move text section, adding the moves to the model.
        /// Throws ArgumentException if there is an illegal SAN move.
        /// </summary>
        /// <param name="tr">The texr reader which is read from</param>
        private void TraversePGNMovetext(TextReader tr)
        {
            string line;
            int index, from, to;

            // skip the empty lines
            do
            {
                line = tr.ReadLine();
            }
            while (line != null && line.Length == 0);

            // read the lines
            while (line != null && line.Length > 0)
            {
                // it's an escape line, skip it
                if (line[0] == '%')
                {
                    line = tr.ReadLine();
                    continue;
                }

                index = 0;

                // loop through the line characters
                while (index < line.Length)
                {
                    // skip the white spaces
                    if (line[index] == ' ')
                    {
                        while (++index < line.Length && line[index] == ' ') ;
                    }

                    // it's a "rest of line" comment, go to the next line
                    if (index < line.Length && line[index] == ';')
                    {
                        line = tr.ReadLine();
                        index = 0;
                        continue;
                    }

                    // skip the move numbers, the NAG or the game termination tokens
                    if (index < line.Length && ((line[index] >= '0' && line[index] <= '9') || line[index] == '$' || line[index] == '*'))
                    {
                        while (++index < line.Length && line[index] != ' ') ;
                    }
                    // skip the multiline commentaries
                    else if (index < line.Length && line[index] == '{')
                    {
                        while (line[index++] != '}')
                        {
                            if (index >= line.Length)
                            {
                                line = tr.ReadLine();
                                index = 0;
                            }
                        }

                    }
                    // skip the RAV which can be nested
                    else if (index < line.Length && line[index] == '(')
                    {
                        int depth = 1;

                        while (depth != 0)
                        {
                            if (++index >= line.Length)
                            {
                                line = tr.ReadLine();
                                index = 0;
                            }

                            if (line[index] == '(')
                            {
                                depth++;
                            }
                            else if (line[index] == ')')
                            {
                                depth--;
                            }
                        }

                        index++;
                    }
                    // if the token it's a move, make it
                    else if (index < line.Length)
                    {
                        from = index;

                        while (++index < line.Length && line[index] != ' ') ;

                        to = index;

                        // remove any !? annotations
                        if (line[to - 1] == '!' || line[to - 1] == '?')
                        {
                            to--;
                        }
                        if (line[to - 1] == '!' || line[to - 1] == '?')
                        {
                            to--;
                        }

                        // make the move
                        model.Make(Utils.GetSANMove(model, line.Substring(from, to - from)));
                    }

                }

                line = tr.ReadLine();

            }
        }

        /// <summary>
        /// Loads the board configuration and the moves from PGN into the model.
        /// Throws ArgumentException if the board configuration or one of the moves is not valid.
        /// Does nothing if the model is null.
        /// </summary>
        /// <param name="tr"></param>
        public void LoadGame(TextReader tr)
        {
            if (model == null) { return; }

            // build the event args
            CancelEventArgs emptyArgs = new CancelEventArgs();

            // raise the Loading event on the model
            model.OnLoading(emptyArgs);

            // if the operation was cancelled
            if (emptyArgs.Cancel) { return; }

            try
            {
                // initialize the game form the PGN tag pairs
                try
                {
                    InitializeGameFromPGNTagPairs(tr);
                }
                catch
                {
                    throw;
                }
                finally
                {
                    // raise the GameBoardConfigurationLoaded event on the model
                    model.OnGameBoardConfigurationLoaded(EventArgs.Empty);
                }

                // traverse the PGN movetext section
                TraversePGNMovetext(tr);
            }
            catch
            {
                throw;
            }
            finally
            {
                // raise the GameMoveSectionLoaded event on the model
                model.OnGameMoveSectionLoaded(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Loads a new game into the model.
        /// Does nothing if the model is null.
        /// </summary>
        public void LoadNewGame()
        {
            if (model == null) { return; }

            // build the event args
            CancelEventArgs emptyArgs = new CancelEventArgs();

            // raise the Loading event on the model
            model.OnLoading(emptyArgs);

            // if the operation was cancelled
            if (emptyArgs.Cancel) { return; }

            // set the starting board
            model.CurrentBoard = Board.GetStartingBoard();

            // raise the ConfigurationLoaded event on the model
            model.OnBoardConfigurationLoaded(EventArgs.Empty);
        }

        /// <summary>
        /// Sets the statuses text.
        /// </summary>
        private void SetStatusesText()
        {
            switch (model.Status)
            {
                case GameStatus.Checkmate:
                    GameStatusText = Resources.CheckmateText;
                    SideToMoveText = String.Empty;
                    break;

                case GameStatus.Draw50Move:
                    GameStatusText = Resources.Draw50MoveRuleText;
                    SideToMoveText = String.Empty;
                    break;

                case GameStatus.DrawInsufficientMaterial:
                    GameStatusText = Resources.DrawInsufficientMaterialText;
                    SideToMoveText = String.Empty;
                    break;

                case GameStatus.DrawRepetition:
                    GameStatusText = Resources.DrawRepetitionText;
                    SideToMoveText = String.Empty;
                    break;

                case GameStatus.Stalemate:
                    GameStatusText = Resources.StalemateText;
                    SideToMoveText = String.Empty;
                    break;

                case GameStatus.Normal:
                    GameStatusText = String.Empty;
                    SideToMoveText = model.CurrentBoard.Status.WhiteTurn ? Resources.WhiteToMoveText : Resources.BlackToMoveText;
                    break;

                case GameStatus.Check:
                    GameStatusText = Resources.CheckText;
                    SideToMoveText = model.CurrentBoard.Status.WhiteTurn ? Resources.WhiteToMoveText : Resources.BlackToMoveText;
                    break;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // unhook the events
                if (model != null)
                {
                    model.Moving -= model_Moving;
                    model.Moved -= model_Moved;
                    model.GoneBack -= model_GoneBack;
                    model.GoneForward -= model_GoneForward;
                    model.Modified -= model_Modified;
                    model.BoardConfigurationLoaded -= model_BoardConfigurationLoaded;
                    model.GameBoardConfigurationLoaded -= model_GameBoardConfigurationLoaded;
                    model.GameMoveSectionLoaded -= model_GameMoveSectionLoaded;
                }
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// A delegate type for autosaving the game in PGN format.
    /// </summary>
    public delegate void AutoSavePGNHandler();
}
