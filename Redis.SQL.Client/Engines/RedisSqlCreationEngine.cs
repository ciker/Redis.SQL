using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Redis.SQL.Client.Analyzer.Interfaces;
using Redis.SQL.Client.Analyzer.Lexers;
using Redis.SQL.Client.Analyzer.Parsers;
using Redis.SQL.Client.Engines.Interfaces;
using Redis.SQL.Client.Exceptions;
using Redis.SQL.Client.Models;
using Redis.SQL.Client.RedisClients;
using Redis.SQL.Client.RedisClients.Interfaces;

namespace Redis.SQL.Client.Engines
{
    internal class RedisSqlCreationEngine : ICreationEngine
    {
        private readonly IRedisHashStorageClient _hashClient;
        private readonly IRedisSetStorageClient _setClient;
        private readonly IRedisStringStorageClient _stringClient;
        private readonly ILexer _creationalLexicalTokenizer;
        private readonly ICustomizedParser _creationalParser;


        internal RedisSqlCreationEngine()
        {
            _hashClient = new RedisHashStorageClient();
            _setClient = new RedisSetStorageClient();
            _stringClient = new RedisStringStorageClient();
            _creationalLexicalTokenizer = new CreationalLexicalTokenizer();
            _creationalParser = new CreationalParser();
        }

        public async Task ExecuteCreateStatement(string createStatement)
        {
            var tokens = _creationalLexicalTokenizer.Tokenize(createStatement).ToList();
            var model = (CreationModel)_creationalParser.ParseTokens(tokens);
            await CreateEntity(model.EntityName, model.Properties);
        }

        public async Task CreateEntity<TEntity>()
        {
            var entityName = Helpers.GetTypeName<TEntity>();
            var properties = Helpers.GetTypeProperties<TEntity>();
            var propertiesDictionary = properties.ToDictionary(x => x.Name, x => x.PropertyType.Name.ToLower());
            await CreateEntity(entityName, propertiesDictionary);
        }

        private async Task CreateEntity(string entityName, IDictionary<string, string> properties)
        {
            var mutex = Semaphores.GetEntitySemaphore(entityName);
            await mutex.WaitAsync();

            try
            {
                if (await _setClient.SetContains(Constants.AllEntityNamesSetKeyName, entityName))
                {
                    throw new EntityAlreadyExistsException(entityName);
                }

                foreach (var property in properties)
                {
                    await _hashClient.SetHashField(Helpers.GetEntityPropertyTypesKey(entityName), property.Key, property.Value);
                }

                await _setClient.AddToSet(Constants.AllEntityNamesSetKeyName, entityName);
                await _stringClient.StoreValue(Helpers.GetEntityCountKey(entityName), 0.ToString());
            }
            finally
            {
                mutex.Release();
            }
        }
    }
}