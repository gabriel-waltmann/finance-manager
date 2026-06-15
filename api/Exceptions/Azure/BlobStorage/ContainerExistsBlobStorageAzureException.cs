namespace api.Exceptions.Azure.BlobStorage;

public class ContainerExistsBlobStorageAzureException : Exception
{
  private static string BuildMessage(string containerName)
  {
    return $"Container with name {containerName} already exists.";
  }

  public ContainerExistsBlobStorageAzureException() { }

  public ContainerExistsBlobStorageAzureException(string containerName)
    : base(BuildMessage(containerName)) { }
    
  public ContainerExistsBlobStorageAzureException(string containerName, Exception inner)
    : base(BuildMessage(containerName), inner) { }
}