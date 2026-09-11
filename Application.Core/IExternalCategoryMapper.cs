using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Domain;
using Microsoft.Extensions.Logging;

namespace Application.Core;

public class Shops
{
    public const string YerevanCity = "Yerevan City";
}

public record ExternalCategory
{
    public string Source { get; init; }
    public string RawName { get; init; }
}

public interface IExternalCategoryMapper
{
    public (Category, SubCategory?) Map(ExternalCategory external);
}

public class ExternalCategoryMapper : IExternalCategoryMapper
{
    private readonly ILogger<ExternalCategoryMapper> _logger;
    private readonly Dictionary<(string, string), (Category, SubCategory?)> _mappings = new Dictionary<(string, string), (Category, SubCategory?)>();

    public ExternalCategoryMapper(ILogger<ExternalCategoryMapper> logger)
    {
        _logger = logger;
        var assembly = Assembly.GetExecutingAssembly();
        string resourceName = "Application.Core.category-mapping.json"; 

        using Stream stream = assembly.GetManifestResourceStream(resourceName) 
                              ?? throw new FileNotFoundException($"The resource {resourceName} не найден.");
        
        using StreamReader reader = new StreamReader(stream);
        string jsonContent = reader.ReadToEnd();
        
        var root = JsonSerializer.Deserialize<Dictionary<string, SourceConfig>>(jsonContent);
        
        if (root != null)
        {
            foreach (var (sourceName, sourceConfig) in root)
            {
                if (sourceConfig.Categories == null) continue;
                
                var sourceNameUpper = sourceName.ToUpperInvariant();

                foreach (var (mainCatName, subCategories) in sourceConfig.Categories)
                {
                    Category category = Categories.Outcome.GetCategory(mainCatName);
                    if (category == null) continue;

                    foreach (var (subCatName, rawNames) in subCategories)
                    {
                        SubCategory? subCategory = string.IsNullOrWhiteSpace(subCatName) 
                            ? null 
                            : category.Sub(subCatName);

                        if (rawNames == null) continue;

                        foreach (var rawName in rawNames)
                        {
                            string cleanedKey = rawName.Trim();
                            var key = (sourceNameUpper, cleanedKey);

                            _mappings[key] = (category, subCategory);
                        }
                    }
                }
            }
        }
    }
    
    public (Category, SubCategory?) Map(ExternalCategory external)
    {
        _logger.LogInformation($"Looking category for {external}");
        if (_mappings.TryGetValue((external.Source.ToUpperInvariant(), external.RawName.Trim(' ', '.')), out var mapped))
        {
            return mapped;
        }

        return (Categories.Outcome.DefaultCategory, null);
    }
    
    private class SourceConfig
    {
        [JsonPropertyName("Categories")]
        public Dictionary<string, Dictionary<string, List<string>>> Categories { get; set; } = new();
    }
}