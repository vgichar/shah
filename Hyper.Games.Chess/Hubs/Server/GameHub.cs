using SignalR.Extras.Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using Hyper.SignalR.Session.Extensions;
using Hyper.SignalR.Session.Core;
using System.Threading.Tasks;
using Hyper.Games.Chess.Hubs.Client;
using Valil.Chess.Model;
using Hyper.Games.Chess.Infrastructure.DB;
using Hyper.Games.Chess.Core.Game;
using Hyper.Games.Chess.Service.DTO.Match;
using ChessAlgorithm;
using Hyper.Games.Chess.Infrastructure.Helpers;
using Hyper.Games.Chess.Core.User;
using Hyper.Games.Chess.Service.DTO.User;
using Hyper.Games.Chess.Service.Query.User;
using Hyper.Games.Chess.Service;
using Hyper.Games.Chess.Service.Command.User;
using Hyper.Games.Chess.Service.Command.Game;
using Hyper.Games.Chess.Service.Query.Game;
using Microsoft.AspNet.SignalR.Client;
using Hyper.Games.Chess.Service.DTO.Game;
using Hyper.SignalR.Session;

namespace Hyper.Games.Chess.Hubs.Server
{
    public partial class GameHub : LifetimeHub
    {
        #region Properties

        public string UserName
        {
            get
            {
                return Context.User.Identity.Name;
            }
        }

        public string SessionId
        {
            get
            {
                return this.GetSessionId();
            }
        }

        public string ConnectionId
        {
            get
            {
                return Context.ConnectionId;
            }
        }

        public Storage Session
        {
            get
            {
                return this.GetSession();
            }
        }

        public Storage Storage
        {
            get
            {
                return this.GetStorage();
            }
        }

        #endregion

        private MongoRepository _mongoRepo;
        private MatchmakingHubClient _matchmakingClient;

        private IQueryHandler<UserDto, GetUserByUserNameQuery> _getUserByUserNameQueryHandler;
        private IQueryHandler<List<GameMoveDto>, GetGameMovesQuery> _getGameMovesQueryHandler;
        private ICommandHandler<UpdateUserRatingCommand> _updateUserRatingCommandHandler;
        private ICommandHandler<MakeMoveCommand> _makeMoveCommandHandler;
        private ICommandHandler<StartGameCommand> _startGameCommandHandler;
        private ICommandHandler<UpdateGameResultCommand> _updateGameResultCommandHandler;

        public GameHub(
            MongoRepository mongoRepo,
            MatchmakingHubClient matchmakingClient,
            IQueryHandler<UserDto, GetUserByUserNameQuery> getUserByUserNameQueryHandler,
            IQueryHandler<List<GameMoveDto>, GetGameMovesQuery> getGameMovesQueryHandler,
            ICommandHandler<UpdateUserRatingCommand> updateUserRatingCommandHandler,
            ICommandHandler<MakeMoveCommand> makeMoveCommandHandler,
            ICommandHandler<StartGameCommand> startGameCommandHandler,
            ICommandHandler<UpdateGameResultCommand> updateGameResultCommandHandler)
        {
            _mongoRepo = mongoRepo;
            _matchmakingClient = matchmakingClient;
            _getUserByUserNameQueryHandler = getUserByUserNameQueryHandler;
            _getGameMovesQueryHandler = getGameMovesQueryHandler;
            _updateUserRatingCommandHandler = updateUserRatingCommandHandler;
            _makeMoveCommandHandler = makeMoveCommandHandler;
            _startGameCommandHandler = startGameCommandHandler;
            _updateGameResultCommandHandler = updateGameResultCommandHandler;
        }

        #region Lifetime Handlers

        /// <summary>
        /// Send the session id to the Caller on front-end
        /// If the Caller has a match then reconnect him
        /// </summary>
        /// <returns></returns>
        public override Task OnConnected()
        {
            if (Storage[SessionId] != null)
            {
                throw new HubException("Client connected twice");
            }
            Storage[SessionId] = UserName; // used by matchmaking server hub client
            InitPlayerInfo(UserName, ConnectionId, SessionId);

            if (IsHavingMatchS)
            {
                ReconnectToMatch();
            }

            return base.OnConnected();
        }

        private void InitPlayerInfo(string username, string connectionId, string sessionId)
        {
            var user = _getUserByUserNameQueryHandler.Handle(new GetUserByUserNameQuery(username));
            user.SessionId = sessionId;
            user.ConnectionId = connectionId;
            Clients.Client(user.ConnectionId).initPlayerInfo(user);
        }

        /// <summary>
        /// If the Caller has a match then reconnect him
        /// </summary>
        /// <returns></returns>
        public override Task OnReconnected()
        {
            Storage[SessionId] = UserName; // used by matchmaking server hub client
            if (IsHavingMatchS)
            {
                if (MatchS.DisconnectedPlayer == SessionId)
                {
                    MatchS.DisconnectedPlayer = "";
                }
                ReconnectToMatch();
            }
            return base.OnReconnected();
        }

        /// <summary>
        /// If the Caller is searching for match - stop it
        /// If the Caller has Match and disconnects manualy - leave match else suspend match
        /// </summary>
        /// <param name="stopCalled"></param>
        /// <returns></returns>
        public override Task OnDisconnected(bool stopCalled)
        {
            Storage[SessionId] = null; // used by matchmaking server hub client
            if (IsSearchingForMatchS)
            {
                StopFindMatch();
            }
            if (IsHavingMatchS)
            {
                CheckForTimeout();
                NotifyLeavingMatch();
            }
            return base.OnDisconnected(stopCalled);
        }

        #endregion

        /// <summary>
        /// Search for a match
        /// </summary>
        public void FindMatch(TimeSpan startTime, TimeSpan addedTimePerMove, int opponentRatingFrom, int opponentRatingTo)
        {
            if (!IsSearchingForMatchS && !IsHavingMatchS)
            {
                try
                {
                    opponentRatingFrom = Math.Max(0, opponentRatingFrom);
                    opponentRatingTo = Math.Max(0, opponentRatingTo);

                    opponentRatingFrom = Math.Min(4000, opponentRatingFrom);
                    opponentRatingTo = Math.Min(4000, opponentRatingTo);

                    // fast int swap
                    if (opponentRatingFrom > opponentRatingTo)
                    {
                        opponentRatingFrom ^= opponentRatingTo;
                        opponentRatingTo ^= opponentRatingFrom;
                        opponentRatingFrom ^= opponentRatingTo;
                    }

                    var seekerUser = _getUserByUserNameQueryHandler.Handle(new GetUserByUserNameQuery(UserName));
                    _matchmakingClient.MatchmakingServerHub.Invoke("FindMatch", new SeekedGameType
                    {
                        SeekerId = ConnectionId,
                        StartTime = startTime,
                        AddedTimePerMove = addedTimePerMove,
                        OpponentRatingFrom = opponentRatingFrom,
                        OpponentRatingTo = opponentRatingTo,
                        SeekerRating = seekerUser.Rating
                    });

                    IsSearchingForMatchS = true;
                    Clients.Caller.searching();
                }
                catch (Exception ex)
                {
                    throw new Microsoft.AspNet.SignalR.Client.HubException("FindMatch", ex);
                }
            }
        }

        /// <summary>
        /// Stop searching for a match
        /// </summary>
        public void StopFindMatch()
        {
            if (IsSearchingForMatchS)
            {
                try
                {
                    _matchmakingClient.MatchmakingServerHub.Invoke("StopFindMatch", ConnectionId);
                    IsSearchingForMatchS = false;
                    Clients.Caller.searchingStopped();
                }
                catch (Exception ex)
                {
                    throw new Microsoft.AspNet.SignalR.Client.HubException("StopFindMatch", ex);
                }
            }
        }

        /// <summary>
        /// Abandon match
        /// </summary>
        public void LeaveMatch()
        {
            if (IsHavingMatchS)
            {
                lock (MatchS)
                {
                    if (IsHavingMatchS)
                    {
                        try
                        {
                            NotifyLeavingMatch(true);
                            if (ValilGameS.MoveCount >= 3)
                            {
                                if (MatchS.White.SessionId == SessionId)
                                {
                                    CalculateAndUpdateResult(GameResult.BlackWin);
                                }
                                else
                                {
                                    CalculateAndUpdateResult(GameResult.WhiteWin);
                                }
                                InitPlayerInfo(MatchS.White.UserName, MatchS.White.ConnectionId, MatchS.White.SessionId);
                                InitPlayerInfo(MatchS.Black.UserName, MatchS.Black.ConnectionId, MatchS.Black.SessionId);
                            }
                            EndGameSession();
                        }
                        catch (Exception ex)
                        {
                            throw new Microsoft.AspNet.SignalR.Client.HubException("LeaveMatch", ex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tell others about Caller leaving match
        /// </summary>
        private void NotifyLeavingMatch(bool hardLeaving = false)
        {
            try
            {
                Groups.Remove(ConnectionId, MatchS.MatchId);
                if (hardLeaving)
                {
                    Clients.OthersInGroup(MatchS.MatchId).playerLeft(SessionId);
                }
                else
                {
                    MatchS.DisconnectedPlayer = SessionId;
                    Clients.OthersInGroup(MatchS.MatchId).playerDisconnected(SessionId);
                }
            }
            catch (Exception ex)
            {
                throw new Microsoft.AspNet.SignalR.Client.HubException("NotifyLeavingMatch", ex);
            }
        }

        /// <summary>
        /// If the Caller has match - try to reconnect to match
        /// </summary>
        public void ReconnectToMatch()
        {
            if (IsHavingMatchS)
            {
                lock (MatchS)
                {
                    if (IsHavingMatchS && ValilGameS.MoveCount >= 3)
                    {
                        try
                        {
                            MatchS.IsDrawOffered = false;
                            Groups.Add(ConnectionId, MatchS.MatchId);
                            Clients.OthersInGroup(MatchS.MatchId).playerReconnected(SessionId);

                            var gameMoves = _getGameMovesQueryHandler.Handle(new GetGameMovesQuery(MatchS.MatchId));
                            var whiteTime = ValilGameS.GetWhiteTimeLeft(MatchS.GameStartTime, MatchS.InitialTime, MatchS.AddedTimePerMove);
                            var blackTime = ValilGameS.GetBlackTimeLeft(MatchS.GameStartTime, MatchS.InitialTime, MatchS.AddedTimePerMove);
                            Clients.Caller.reportMatch(MatchS, gameMoves, whiteTime, blackTime);
                            Clients.OthersInGroup(MatchS.MatchId).drawDenied();
                        }
                        catch (Exception ex)
                        {
                            throw new Microsoft.AspNet.SignalR.Client.HubException("ReconnectToMatch", ex);
                        }
                    }
                    else
                    {
                        EndGameSession();
                    }
                }
            }
        }

        /// <summary>
        /// Offer a draw (GameResult.Tie) to opponent
        /// </summary>
        public void OfferDraw()
        {
            if (IsHavingMatchS && ValilGameS.MoveCount >= 3)
            {
                lock (MatchS)
                {
                    if (IsHavingMatchS)
                    {
                        if (MatchS.IsDrawOffered && ValilGameS.MoveCount >= 3)
                        {
                            GameResult result = GameResult.Tie;
                            Clients.Group(MatchS.MatchId).gameEnded(result.ToString());
                            InitPlayerInfo(MatchS.White.UserName, MatchS.White.ConnectionId, MatchS.White.SessionId);
                            InitPlayerInfo(MatchS.Black.UserName, MatchS.Black.ConnectionId, MatchS.Black.SessionId);
                            CalculateAndUpdateResult(result);
                            EndGameSession();
                        }
                        else
                        {
                            MatchS.IsDrawOffered = true;
                            Clients.OthersInGroup(MatchS.MatchId).drawOffered();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Accept a draw (GameResult.Tie) offered by opponent
        /// </summary>
        public void AcceptDraw()
        {
            if (IsHavingMatchS && MatchS.IsDrawOffered)
            {
                lock (MatchS)
                {
                    if (IsHavingMatchS && MatchS.IsDrawOffered)
                    {
                        GameResult result = GameResult.Tie;
                        Clients.Group(MatchS.MatchId).gameEnded(result.ToString());
                        InitPlayerInfo(MatchS.White.UserName, MatchS.White.ConnectionId, MatchS.White.SessionId);
                        InitPlayerInfo(MatchS.Black.UserName, MatchS.Black.ConnectionId, MatchS.Black.SessionId);
                        CalculateAndUpdateResult(result);
                        EndGameSession();
                    }
                }
            }
        }

        /// <summary>
        /// Deny a draw (GameResult.Tie) offered by opponent
        /// </summary>
        public void DenyDraw()
        {
            if (IsHavingMatchS && MatchS.IsDrawOffered)
            {
                lock (MatchS)
                {
                    if (IsHavingMatchS && MatchS.IsDrawOffered)
                    {
                        MatchS.IsDrawOffered = false;
                        Clients.OthersInGroup(MatchS.MatchId).drawDenied();
                    }
                }
            }
        }

        /// <summary>
        /// Check if one of the players has a timeout
        /// </summary>
        public bool CheckForTimeout()
        {
            if (IsHavingMatchS)
            {
                lock (MatchS)
                {
                    if (IsHavingMatchS)
                    {
                        var whiteTime = ValilGameS.GetWhiteTimeLeft(MatchS.GameStartTime, MatchS.InitialTime, MatchS.AddedTimePerMove);
                        var blackTime = ValilGameS.GetBlackTimeLeft(MatchS.GameStartTime, MatchS.InitialTime, MatchS.AddedTimePerMove);
                        Clients.Group(MatchS.MatchId).updateTime(whiteTime, blackTime);

                        if (whiteTime.TotalSeconds < 0 || blackTime.TotalMilliseconds < 0)
                        {
                            GameResult result = whiteTime.TotalSeconds <= 0 ? GameResult.BlackWin : GameResult.WhiteWin;
                            Clients.Group(MatchS.MatchId).gameEnded(result.ToString());
                            InitPlayerInfo(MatchS.White.UserName, MatchS.White.ConnectionId, MatchS.White.SessionId);
                            InitPlayerInfo(MatchS.Black.UserName, MatchS.Black.ConnectionId, MatchS.Black.SessionId);
                            CalculateAndUpdateResult(result);
                            EndGameSession();
                            return true;
                        }
                        return false;
                    }
                }
            }
            return false;
        }

        public bool ClaimWinOnDisconnect()
        {
            if (IsHavingMatchS && !string.IsNullOrEmpty(MatchS.DisconnectedPlayer))
            {
                lock (MatchS)
                {
                    if (IsHavingMatchS && !string.IsNullOrEmpty(MatchS.DisconnectedPlayer))
                    {
                        if (MatchS.DisconnectedPlayer != SessionId
                            && (MatchS.DisconnectedPlayer == MatchS.White.SessionId
                                || MatchS.DisconnectedPlayer == MatchS.Black.SessionId)
                            )
                        {
                            GameResult result = MatchS.DisconnectedPlayer == MatchS.White.SessionId? GameResult.BlackWin: GameResult.WhiteWin;
                            Clients.Group(MatchS.MatchId).gameEnded(result.ToString());
                            InitPlayerInfo(MatchS.White.UserName, MatchS.White.ConnectionId, MatchS.White.SessionId);
                            InitPlayerInfo(MatchS.Black.UserName, MatchS.Black.ConnectionId, MatchS.Black.SessionId);
                            CalculateAndUpdateResult(result);
                            EndGameSession();
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Do a move in a game
        /// </summary>
        /// <param name="from">board position, e.g. "c3"</param>
        /// <param name="to">board position, e.g. "c3"</param>
        /// <returns>Return if the move is valid</returns>
        public bool Move(string from, string to)
        {
            if (IsHavingMatchS)
            {
                lock (MatchS)
                {
                    if (IsHavingMatchS)
                    {
                        try
                        {
                            var gameResult = MakeMove(from, to);
                            Clients.OthersInGroup(MatchS.MatchId).moved(from, to);

                            var whiteTime = ValilGameS.GetWhiteTimeLeft(MatchS.GameStartTime, MatchS.InitialTime, MatchS.AddedTimePerMove);
                            var blackTime = ValilGameS.GetBlackTimeLeft(MatchS.GameStartTime, MatchS.InitialTime, MatchS.AddedTimePerMove);
                            Clients.Group(MatchS.MatchId).updateTime(whiteTime, blackTime);

                            if (gameResult != GameResult.NoResult)
                            {
                                Clients.Group(MatchS.MatchId).gameEnded(gameResult.ToString());
                                InitPlayerInfo(MatchS.White.UserName, MatchS.White.ConnectionId, MatchS.White.SessionId);
                                InitPlayerInfo(MatchS.Black.UserName, MatchS.Black.ConnectionId, MatchS.Black.SessionId);
                                EndGameSession();
                            }
                        }
                        catch
                        {
                            ValilGameS.Previous();
                            return false;
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Do a promotion move in a game
        /// </summary>
        /// <param name="from">board position, e.g. "c3"</param>
        /// <param name="to">board position, e.g. "c3"</param>
        /// <param name="promotionPiece">board piece, e.g. 'q'</param>
        /// <returns>Return if the move is valid</returns>
        public bool MovePromotion(string from, string to, char promotionPiece)
        {
            if (IsHavingMatchS)
            {
                try
                {
                    var gameResult = MakeMove(from, to, promotionPiece.ToString());
                    Clients.OthersInGroup(MatchS.MatchId).movedPromoted(from, to, promotionPiece);

                    var whiteTime = ValilGameS.GetWhiteTimeLeft(MatchS.GameStartTime, MatchS.InitialTime, MatchS.AddedTimePerMove);
                    var blackTime = ValilGameS.GetBlackTimeLeft(MatchS.GameStartTime, MatchS.InitialTime, MatchS.AddedTimePerMove);
                    Clients.Group(MatchS.MatchId).updateTime(whiteTime, blackTime);

                    if (gameResult != GameResult.NoResult)
                    {
                        Clients.Group(MatchS.MatchId).gameEnded(gameResult.ToString());
                        InitPlayerInfo(MatchS.White.UserName, MatchS.White.ConnectionId, MatchS.White.SessionId);
                        InitPlayerInfo(MatchS.Black.UserName, MatchS.Black.ConnectionId, MatchS.Black.SessionId);
                        EndGameSession();
                    }
                }
                catch
                {
                    ValilGameS.Previous();
                    return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Makes move (and checks for move legality) in session "Valil" game
        /// Flags the game as started in session and sets start DateTime database if it's first move
        /// Makes move in database
        /// Updates palyer ratings if the game is ended
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="promotionPiece"></param>
        /// <returns>Game result: NoResult - the game continues, WhiteWin, BlackWin, Tie</returns>
        private GameResult MakeMove(string from, string to, string promotionPiece = "")
        {
            // check for move legality and update game state in session
            try
            {
                Move myMove = Utils.GetCANMove(ValilGameS, from + to + promotionPiece);
                ValilGameS.Make(myMove);
            }
            catch
            {
                throw new HubException("Invalid move");
            }

            // Start "timer"
            if (!MatchS.IsGameInitialized)
            {
                _startGameCommandHandler.Handle(new StartGameCommand(MatchS.MatchId, MatchS.White.UserName, MatchS.Black.UserName));
                MatchS.GameStartTime = DateTime.Now;
                MatchS.IsGameInitialized = true;
            }

            // check result
            GameResult result = CalculateAndUpdateResult();

            // add move in database
            _makeMoveCommandHandler.Handle(new MakeMoveCommand(MatchS.MatchId, from, to, promotionPiece, result));

            return result;
        }

        private GameResult CalculateAndUpdateResult(GameResult result = GameResult.NoResult)
        {
            if (ValilGameS.IsEnded && result == GameResult.NoResult)
            {
                if (ValilGameS.CurrentBoard.BlackKingInCheck() && ValilGameS.Status == GameStatus.Checkmate)
                {
                    result = GameResult.WhiteWin;
                }
                else if (ValilGameS.CurrentBoard.WhiteKingInCheck() && ValilGameS.Status == GameStatus.Checkmate)
                {
                    result = GameResult.BlackWin;
                }
                else
                {
                    result = GameResult.Tie;
                }
            }

            if (result != GameResult.NoResult)
            {
                var whiteRating = MatchS.White.Rating;
                var blackRating = MatchS.Black.Rating;

                EloRatingCalculator.UpdateScores(ref whiteRating, ref blackRating, result);

                // update ratings in database
                _updateUserRatingCommandHandler.Handle(new UpdateUserRatingCommand(MatchS.White.UserName, whiteRating));
                _updateUserRatingCommandHandler.Handle(new UpdateUserRatingCommand(MatchS.Black.UserName, blackRating));
                _updateGameResultCommandHandler.Handle(new UpdateGameResultCommand(MatchS.MatchId, result));
            }

            return result;
        }

        private void EndGameSession()
        {
            if (MatchS != null)
            {
                var whiteSessionId = MatchS.White.SessionId;
                var blackSessionId = MatchS.Black.SessionId;

                var wSession = SignalRContext.SessionProvider[whiteSessionId];
                wSession[NamingContract.Session_Match] = null;
                wSession[NamingContract.Session_Game] = null;

                var bSession = SignalRContext.SessionProvider[blackSessionId];
                bSession[NamingContract.Session_Match] = null;
                bSession[NamingContract.Session_Game] = null;
            }
        }
    }

    /// <summary>
    /// Class with helper properties for session manipulation
    /// </summary>
    public partial class GameHub
    {
        /// <summary>
        /// Match between 2 palyers
        /// </summary>
        private MatchDto MatchS
        {
            get
            {
                MatchDto match = Session[NamingContract.Session_Match] as MatchDto;
                return match;
            }
            set
            {
                Session[NamingContract.Session_Match] = value;
            }
        }

        /// <summary>
        /// Vaiil chess game for validation
        /// </summary>
        private ValilGame ValilGameS
        {
            get
            {
                return Session[NamingContract.Session_Game] as ValilGame;
            }
            set
            {
                Session[NamingContract.Session_Game] = value;
            }
        }

        /// <summary>
        /// Check if the Caller is searchg for match
        /// </summary>
        private bool IsSearchingForMatchS
        {
            get
            {
                bool? searchForMatch = Session[NamingContract.Session_SearchingForMatch] as bool?;
                return searchForMatch.HasValue && searchForMatch.Value;
            }
            set
            {
                Session[NamingContract.Session_SearchingForMatch] = value;
            }
        }

        /// <summary>
        /// Check if the Caller has match
        /// </summary>
        private bool IsHavingMatchS
        {
            get
            {
                return MatchS != null;
            }
        }
    }
}