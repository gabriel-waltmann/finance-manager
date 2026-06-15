namespace api.Exceptions.Azure.BlobStorage;

public class ExistsBlobStorageAzureException : Exception
{
  private static string BuildMessage(string containerName)
  {
    return $"Blob with name {containerName} already exists.";
  }

  public ExistsBlobStorageAzureException() { }

  public ExistsBlobStorageAzureException(string containerName)
    : base(BuildMessage(containerName)) { }
    
  public ExistsBlobStorageAzureException(string containerName, Exception inner)
    : base(BuildMessage(containerName), inner) { }
}