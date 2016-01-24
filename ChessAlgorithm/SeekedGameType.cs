﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessAlgorithm
{
    public class SeekedGameType
    {
        public TimeSpan StartTime { get; set; }

        public TimeSpan AddedTimePerMove { get; set; }

        /// <summary>
        /// ConnectionId
        /// </summary>
        public string SeekerId { get; set; }

        public int SeekerRating { get; set; }

        public int OpponentRatingFrom { get; set; }

        public int OpponentRatingTo { get; set; }
    }
}