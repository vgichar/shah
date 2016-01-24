﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hyper.Games.Chess.Service
{
    public interface IQueryHandler<TResult, TQuery>
        where TQuery : IQuery<TResult>
    {
        TResult Handle(TQuery query);
    }
}
