namespace api.Exceptions.Azure.BlobStorage;

public class ContainerNotExistsBlobStorageAzureException : Exception
{
  private static string BuildMessage(string containerName)
  {
    return $"Container with name {containerName} not exists.";
  }

  public ContainerNotExistsBlobStorageAzureException() { }

  public ContainerNotExistsBlobStorageAzureException(string containerName)
    : base(BuildMessage(containerName)) { }
    
  public ContainerNotExistsBlobStorageAzureException(string containerName, Exception inner)
    : base(BuildMessage(containerName), inner) { }
}