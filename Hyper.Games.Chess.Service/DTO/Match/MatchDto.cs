using Hyper.Games.Chess.Core.User;
using Hyper.Games.Chess.Service.DTO.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hyper.Games.Chess.Service.DTO.Match
{
    public class MatchDto
    {
        public string MatchId { get; set; }

        public UserDto White { get; set; }

        public UserDto Black { get; set; }

        public bool IsGameInitialized { get; set; }

        public bool IsDrawOffered { get; set; }

        public string DisconnectedPlayer { get; set; }

        public DateTime GameFoundTime { get; set; }

        public DateTime GameStartTime { get; set; }

        public TimeSpan InitialTime { get; set; }

        public TimeSpan AddedTimePerMove { get; set; }
    }
}