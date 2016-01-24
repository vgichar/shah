using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedAlgorithm
{
    public interface IAlgorithm
    {
        object SeekGame(object gameType, List<object> hostedGames);
    }
}
