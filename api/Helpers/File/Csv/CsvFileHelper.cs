using System.Globalization;
using CsvHelper;

namespace api.Helpers.File.Csv;

public static class CsvFileHelper
{
  public static List<ModelFile> Read<ModelFile>(string filePath)
  {
    try
    {
      using var reader = new StreamReader(filePath);
      using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
      
      var records = csv.GetRecords<ModelFile>();

      return records.ToList();
    }
    catch (Exception ex)
    {
      System.Console.WriteLine(ex.Message);

      return [];
    }
  } 
}