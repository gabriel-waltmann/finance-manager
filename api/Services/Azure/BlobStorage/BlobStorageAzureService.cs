using api.Exceptions.Azure.BlobStorage;
using api.Models.Azure.BlobStorage;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace api.Services.Azure.BlobStorage;

public class BlobStorageAzureService(
  BlobServiceClient blobServiceClient
)
  {
    private readonly BlobServiceClient _blobServiceClient = blobServiceClient;

    public async Task CreateContainerAsync(string containerName)
    {
      BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

      if (await containerClient.ExistsAsync()){
        throw new ContainerExistsBlobStorageAzureException(containerName);
      }

      await containerClient.CreateAsync();
    }

    public async IAsyncEnumerable<ContainerBlobStorageAzure> GetContainersAsync()
    {
      AsyncPageable<BlobContainerItem> containers = _blobServiceClient.GetBlobContainersAsync();

      await foreach (var item in containers)
      {
          yield return new ContainerBlobStorageAzure() { Name = item.Name };
      }
    }

    public async Task DeleteContainerAsync(string containerName)
    {
      BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

      if (!await containerClient.ExistsAsync()){
        throw new ContainerNotExistsBlobStorageAzureException(containerName);
      }

      await containerClient.DeleteAsync();
    }

    public async IAsyncEnumerable<InfoBlobStorageAzure> ListBlobsInContainerAsync(string containerName)
    {
      BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

      if (!await containerClient.ExistsAsync()){
        throw new ContainerNotExistsBlobStorageAzureException(containerName);
      }

      AsyncPageable<BlobItem> blobs = containerClient.GetBlobsAsync();

      // Map the SDK objects to model objects and return them
      await foreach(var blob in blobs)
      {
          InfoBlobStorageAzure model = new ()
          {
              Name = blob.Name,
              Tags = blob.Tags,
              ContentEncoding = blob.Properties.ContentEncoding,
              ContentType = blob.Properties.ContentType,
              Size = blob.Properties.ContentLength,
              CreatedOn = blob.Properties.CreatedOn,
              AccessTier = blob.Properties.AccessTier?.ToString(),
              BlobType = blob.Properties.BlobType?.ToString()
          };

          yield return model;
      }
    }

    public async Task UploadBlobAsync(string containerName, string blobName, string contentType, Stream content)
    {
      var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

      if (!await containerClient.ExistsAsync()){
        throw new ContainerNotExistsBlobStorageAzureException(containerName);
      }

      var blobClient = containerClient.GetBlobClient(blobName);
      
      var options = new BlobUploadOptions() { HttpHeaders = new BlobHttpHeaders() { ContentType = contentType } };
      
      await blobClient.UploadAsync(content, options);
    }

    public async Task<ItemBlobStorageAzure> GetBlobContentsAsync(string containerName, string blobName)
    {
      var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
      
      if (!await containerClient.ExistsAsync()){
        throw new ContainerNotExistsBlobStorageAzureException(containerName);
      }

      var blobClient = containerClient.GetBlobClient(blobName);

      if (!await blobClient.ExistsAsync())
      {
        throw new NotExistsBlobStorageAzureException(blobName);
      }
                  
      return new ItemBlobStorageAzure()
      {
          Name = blobName,
          ContentType = blobClient.GetProperties().Value.ContentType,
          Content = await blobClient.OpenReadAsync()
      };
    }

    public async Task DeleteBlobAsync(string containerName, string blobName)
    {
      var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

      if (!await containerClient.ExistsAsync()){
        throw new ContainerNotExistsBlobStorageAzureException(containerName);
      }

      var blobClient = containerClient.GetBlobClient(blobName);

      if (!await blobClient.ExistsAsync())
      {
        throw new NotExistsBlobStorageAzureException(blobName);
      }

      await blobClient.DeleteAsync();
    }
  }
