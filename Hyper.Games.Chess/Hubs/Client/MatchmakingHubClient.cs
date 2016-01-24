using ChessAlgorithm;
using Hyper.Games.Chess.Hubs.Server;
using Hyper.Games.Chess.Service;
using Hyper.Games.Chess.Service.DTO.Match;
using Hyper.Games.Chess.Service.DTO.User;
using Hyper.Games.Chess.Service.Query.User;
using Hyper.SignalR.Session;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Client;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Valil.Chess.Model;

namespace Hyper.Games.Chess.Hubs.Client
{
    public class MatchmakingHubClient
    {
        private IHubContext _gameHub;
        private IQueryHandler<UserDto, GetUserByUserNameQuery> _getUserByUserNameQueryHandler;
        public IHubProxy MatchmakingServerHub;


        public MatchmakingHubClient(IQueryHandler<UserDto, GetUserByUserNameQuery> getUserByUserNameQueryHandler)
        {
            _getUserByUserNameQueryHandler = getUserByUserNameQueryHandler;
            _gameHub = GlobalHost.ConnectionManager.GetHubContext<GameHub>();

            string matchmakongServerUrl = ConfigurationManager.AppSettings["MatchmakingServerUrl"];
            string matchmakingServerHub = ConfigurationManager.AppSettings["MatchmakingServerHub"];

            var connection = new HubConnection(matchmakongServerUrl);
            MatchmakingServerHub = connection.CreateHubProxy(matchmakingServerHub);

            RegisterHandlers();

            connection.Start();
        }

        private void RegisterHandlers()
        {
            MatchmakingServerHub.On<FoundGameType>("reportMatch", OnReportMatch);
        }

        private void OnReportMatch(FoundGameType foundGameType)
        {
            MatchDto matchDto = new MatchDto
            {
                MatchId = Guid.NewGuid().ToString(),
                IsGameInitialized = false,
                IsDrawOffered = false,
                GameFoundTime = DateTime.Now,
                InitialTime = foundGameType.StartTime,
                AddedTimePerMove = foundGameType.AddedTimePerMove
            };

            ValilGame valilGame = InitValilGame();

            UserDto hoster = InitPlayer(foundGameType.HosterId, matchDto, valilGame);
            UserDto seeker = InitPlayer(foundGameType.SeekerId, matchDto, valilGame);

            matchDto.White = hoster;
            matchDto.Black = seeker;

            _gameHub.Clients.Group(matchDto.MatchId).reportMatch(matchDto);
        }

        private UserDto InitPlayer(string playerConnectionId, MatchDto matchDto, ValilGame valilGame)
        {
            string playerSessionId = SignalRContext.GetSessionIdByConnectionId(playerConnectionId);
            string playerUsername = SignalRContext.StorageProvider.Current[playerSessionId] as string;

            UserDto player = _getUserByUserNameQueryHandler.Handle(new GetUserByUserNameQuery(playerUsername));

            player.ConnectionId = playerConnectionId;
            player.SessionId = playerSessionId;

            SignalRContext.SessionProvider[player.SessionId][NamingContract.Session_SearchingForMatch] = false;
            SignalRContext.SessionProvider[player.SessionId][NamingContract.Session_Match] = matchDto;
            SignalRContext.SessionProvider[player.SessionId][NamingContract.Session_Game] = valilGame;

            _gameHub.Groups.Add(player.ConnectionId, matchDto.MatchId).Wait();

            return player;
        }

        private ValilGame InitValilGame()
        {
            GameStringDescriptionProxy gameLoader = new GameStringDescriptionProxy();
            gameLoader.Model = new ValilGame();
            gameLoader.LoadNewGame();
            return gameLoader.Model;
        }
    }
}