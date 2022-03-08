using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ContactsTableCosmosWebApp.Models.Abstract;
using ContactsTableCosmosWebApp.Models.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace ContactsTableCosmosWebApp.Models.Concrete
{
  public class TableContactRepository : IContactRepository
  {
    private readonly ILogger<TableContactRepository> _logger;
    private readonly CloudStorageAccount _cloudStorageAccount;
    private readonly CloudTableClient _cloudTableClient;
    private readonly CloudTable _cloudTable;

    public TableContactRepository(IOptions<StorageUtility> storageUtility, ILogger<TableContactRepository> logger)
    {
      _logger = logger;
      _cloudStorageAccount = storageUtility.Value.StorageAccount;
      _cloudTableClient = _cloudStorageAccount.CreateCloudTableClient();
      _cloudTable = _cloudTableClient.GetTableReference("Contacts");
      _cloudTable.CreateIfNotExistsAsync().GetAwaiter().GetResult();
    }

    private ContactTable SetContactTableObject(Contact contact)
    {
      contact.Id = Guid.NewGuid().ToString();
      ContactTable contactTable = new ContactTable
      {
        Id = contact.Id,
        PartitionKey = contact.ContactName,
        RowKey = contact.Phone,
        ContactType = contact.ContactType,
        Email = contact.Email
      };
      return contactTable;
    }

    private Contact SetContactObject(ContactTable contactTable)
    {
      Contact contact = new Contact
      {
        Id = contactTable.Id,
        ContactName = contactTable.PartitionKey,
        Phone = contactTable.RowKey,
        ContactType = contactTable.ContactType,
        Email = contactTable.Email
      };
      return contact;
    }

    private List<Contact> SetContactsList(List<ContactTable> contactTableList)
    {
      List<Contact> contactsList = new List<Contact>();
      foreach (var item in contactTableList)
      {
        contactsList.Add(SetContactObject(item));
      }
      return contactsList;
    }
    public async Task<Contact> CreateAsync(Contact contact)
    {
      ContactTable contactTable = SetContactTableObject(contact);
      TableOperation insertOperation = TableOperation.Insert(contactTable);
      TableResult tableResult = await _cloudTable.ExecuteAsync(insertOperation);
      ContactTable insertedContact = tableResult.Result as ContactTable;
      _logger.LogInformation($"--- TableContactRepository.CreateAsync, New contact with name '{contact.ContactName}' created successfully ---");
      return SetContactObject(insertedContact);
    }

    public async Task DeleteAsync(string id, string contactName, string phone)
    {
      var partionKey = contactName;
      var rowKey = phone;
      TableOperation retrieveOperation = TableOperation.Retrieve<ContactTable>(partionKey, rowKey);
      TableResult tableResult = await _cloudTable.ExecuteAsync(retrieveOperation);
      var contactTable = tableResult.Result as ContactTable;
      if (contactTable != null)
      {
        TableOperation deleteContact = TableOperation.Delete(contactTable);
        var updateResult = await _cloudTable.ExecuteAsync(deleteContact);
      }

      _logger.LogInformation($"--- TableContactRepository.DeleteAsync, New contact with name '{contactName}' deleted successfully ---");
    }

    public async Task<Contact> FindContactAsync(string id)
    {
      TableQuery<ContactTable> query = new TableQuery<ContactTable>().Where(TableQuery.GenerateFilterCondition("Id", QueryComparisons.Equal, id));
      TableContinuationToken tcToken = null;
      var contactTableResult = await _cloudTable.ExecuteQuerySegmentedAsync(query, tcToken);
      return SetContactObject(contactTableResult.Results[0]);
    }

    public async Task<List<Contact>> FindContactByPhoneAsync(string phone)
    {
      var rowKey = phone;
      TableQuery<ContactTable> query = new TableQuery<ContactTable>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey));
      TableContinuationToken tcToken = null;
      var contactTableResult = await _cloudTable.ExecuteQuerySegmentedAsync(query, tcToken);
      return SetContactsList(contactTableResult.Results);

    }

    public async Task<List<Contact>> FindContactCPAsync(string contactName, string phone)
    {
      var partitionKey = contactName;
      var rowKey = phone;
      TableOperation retrieveOperation = TableOperation.Retrieve<ContactTable>(partitionKey, rowKey);
      TableResult tableResult = await _cloudTable.ExecuteAsync(retrieveOperation);
      var result = tableResult.Result as ContactTable;
      var contact = SetContactObject(result);
      return new List<Contact> { contact };
    }

    public async Task<List<Contact>> FindContactsByContactNameAsync(string contactName)
    {
      var partitionKey = contactName;
      TableQuery<ContactTable> query = new TableQuery<ContactTable>()
          .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
      TableContinuationToken tcToken = null;
      var contactTableResult = await _cloudTable.ExecuteQuerySegmentedAsync(query, tcToken);
      return SetContactsList(contactTableResult.Results);
    }

    public async Task<List<Contact>> GetAllContactsAsync()
    {
      TableQuery<ContactTable> query = new TableQuery<ContactTable>();
      TableContinuationToken tcToken = null;
      var result = await _cloudTable.ExecuteQuerySegmentedAsync(query, tcToken);
      _logger.LogInformation($"--- TableContactRepository.GetAllContactsAsync, Retrieved all records successfully ---");
      return SetContactsList(result.Results);
    }

    public async Task<Contact> UpdateAsync(Contact contact)
    {
      var contactTable = SetContactTableObject(contact);
      TableOperation retrieveOperation = TableOperation.Retrieve<ContactTable>(contactTable.PartitionKey, contactTable.RowKey);
      TableResult tableResult = await _cloudTable.ExecuteAsync(retrieveOperation);
      var contactToUpdate = tableResult.Result as ContactTable;
      contactToUpdate.ContactType = contactTable.ContactType;
      contactToUpdate.Email = contactTable.Email;
      if (contactToUpdate != null)
      {
        TableOperation updateContact = TableOperation.Replace(contactToUpdate);
        var updateResult = await _cloudTable.ExecuteAsync(updateContact);
        var contactTableResult = updateResult.Result as ContactTable;
        _logger.LogInformation($"--- TableContactRepository.UpdateAsync, Updated '{contact.ContactName}' record successfully ---");
        return SetContactObject(contactTableResult);
      }

      return null;
    }

    public void ClearCache()
    {
      throw new NotImplementedException();

      // Implement if cache is used.
    }
  }
}
