namespace api.Models.Azure.BlobStorage;

public class ItemBlobStorageAzure
{
  public string Name { get; set; }

  public string ContentType { get; set; }

  public Stream Content { get; set; }
}