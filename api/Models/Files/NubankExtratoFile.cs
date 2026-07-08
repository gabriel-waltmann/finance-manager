using CsvHelper.Configuration.Attributes;

namespace api.Models.Files;

public class NubankExtratoFile
{
  [Name("Data")]
  public required string Data { get; set; }

  [Name("Valor")]
  public required string Valor { get; set; }

  [Name("Identificador")]
  public required string Identificador { get; set; }

  [Name("Descrição")]
  public required string Descricao { get; set; }
}
