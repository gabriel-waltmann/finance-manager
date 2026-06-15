namespace api.Exceptions.Azure.BlobStorage;

public class NotExistsBlobStorageAzureException : Exception
{
  private static string BuildMessage(string blobName)
  {
    return $"Blob with name {blobName} not exists.";
  }

  public NotExistsBlobStorageAzureException() { }

  public NotExistsBlobStorageAzureException(string containerName)
    : base(BuildMessage(containerName)) { }
    
  public NotExistsBlobStorageAzureException(string containerName, Exception inner)
    : base(BuildMessage(containerName), inner) { }
}