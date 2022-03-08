using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using ContactsTableCosmosWebApp.Models.Abstract;
using ContactsTableCosmosWebApp.Models.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace ContactsTableCosmosWebApp.Models.Concrete
{
  public class CosmosContactRepository : IContactRepository
  {
    private readonly ILogger<CosmosContactRepository> _logger;
    private readonly IConfiguration _configuration;
    private readonly IDistributedCache _distributedCache;
    private readonly string _cosmosEndpoint;
    private readonly string _cosmosKey;
    private readonly string _databaseId;
    private readonly string _containerId;
    private Database _database;
    private Container _container;
    private CosmosClient _cosmosClient;

    public CosmosContactRepository(IOptions<CosmosUtility> cosmosUtility, ILogger<CosmosContactRepository> logger, IConfiguration configuration, IDistributedCache distributedCache)
    {
      _logger = logger;
      _configuration = configuration;
      _distributedCache = distributedCache;

      _cosmosEndpoint = cosmosUtility.Value.CosmosEndpoint;
      _cosmosKey = cosmosUtility.Value.CosmosKey;
      _databaseId = "multiDb";
      _containerId = "contacts";

      _cosmosClient = new CosmosClient(_cosmosEndpoint, _cosmosKey);
      _cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseId).GetAwaiter().GetResult();
      _database = _cosmosClient.GetDatabase(_databaseId);
      _database.CreateContainerIfNotExistsAsync(_containerId, "/contactName").GetAwaiter().GetResult();
      _container = _database.GetContainer(_containerId);

    }

    private async Task<List<Contact>> GetContacts(string sqlQuery)
    {
      QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
      FeedIterator<Contact> queryResultIterator = _container.GetItemQueryIterator<Contact>(queryDefinition);
      List<Contact> contactsList = new List<Contact>();
      while (queryResultIterator.HasMoreResults)
      {
        FeedResponse<Contact> currentResultSet = await queryResultIterator.ReadNextAsync();
        foreach (var item in currentResultSet)
        {
          contactsList.Add(item);
        }

        return contactsList;
      }
      return null;
    }
    public async Task<Contact> CreateAsync(Contact contact)
    {
      contact.Id = Guid.NewGuid().ToString();
      ItemResponse<Contact> response = await _container.CreateItemAsync<Contact>(contact);
      if (contact != null)
      {
        _logger.LogInformation($"---CosmosContactRepository.CreateAsync, New Contact with name : '{contact.ContactName}' created successfully---");
        return contact;
      }
      return null;
    }

    public async Task DeleteAsync(string id, string contactName, string phone)
    {
      ItemResponse<Contact> response = await _container.DeleteItemAsync<Contact>(id, new PartitionKey(contactName));
      _logger.LogInformation($"---CosmosContactRepository.DeleteAsync, Deleted Contact with name : '{contactName}' , successfully---");
    }
    public async Task<Contact> FindContactAsync(string id)
    {
      var sqlQuery = $"Select * from c where c.id='{id}'";
      var contactsList = await GetContacts(sqlQuery);
      return contactsList[0];
    }

    public async Task<List<Contact>> FindContactByPhoneAsync(string phone)
    {
      var sqlQuery = $"Select * from c where c.phone='{phone}'";
      var contactsList = await GetContacts(sqlQuery);
      return contactsList;
    }

    public async Task<List<Contact>> FindContactCPAsync(string contactName, string phone)
    {
      var sqlQuery = $"Select * from c where c.contactName = '{contactName}' and c.phone = '{phone}'";
      var contactsList = await GetContacts(sqlQuery);
      return contactsList;
    }

    public async Task<List<Contact>> FindContactsByContactNameAsync(string contactName)
    {
      var sqlQuery = $"Select * from c where c.contactName='{contactName}'";
      var contactsList = await GetContacts(sqlQuery);
      return contactsList;
    }

    public void ClearCache()
    {
      //_distributedCache.RefreshAsync("contactsList");
    }
    public async Task<List<Contact>> GetAllContactsAsync()
    {
      var sqlQuery = $"Select * from c";
      var contactsList = await GetContacts(sqlQuery);
      return contactsList;
    }

    public async Task<Contact> UpdateAsync(Contact contact)
    {
      ItemResponse<Contact> contactResponse = await _container.ReadItemAsync<Contact>(contact.Id, new PartitionKey(contact.ContactName));
      var contactResult = contactResponse.Resource;
      contactResult.Id = contact.Id;
      contactResult.ContactName = contact.ContactName;
      contactResult.ContactType = contact.ContactType;
      contactResult.Phone = contact.Phone;
      contactResult.Email = contact.Email;
      contactResponse = await _container.ReplaceItemAsync(contactResult, contactResult.Id);
      if (contactResponse != null)
      {
        _logger.LogInformation($"---CosmosContactRepository.UpdateAsync, Updated Contact record with name : '{contact.ContactName}' , successfully---");
        return contactResponse;
      }
      return null;
    }
  }
}
