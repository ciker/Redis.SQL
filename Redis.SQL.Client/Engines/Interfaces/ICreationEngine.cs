﻿using System.Threading.Tasks;

namespace Redis.SQL.Client.Engines.Interfaces
{
    public interface ICreationEngine
    {
        Task<bool> CreateEntity<TEntity>();
    }
}
