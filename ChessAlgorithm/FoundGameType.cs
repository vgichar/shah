using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessAlgorithm
{
    public class FoundGameType
    {
        public TimeSpan StartTime { get; set; }

        public TimeSpan AddedTimePerMove { get; set; }

        /// <summary>
        /// ConnectionId
        /// </summary>
        public string HosterId { get; set; }

        /// <summary>
        /// ConnectionId
        /// </summary>
        public string SeekerId { get; set; }

        public override string ToString()
        {
            return string.Format("Match made for: {0} and {1}", HosterId, SeekerId);
        }
    }
}
