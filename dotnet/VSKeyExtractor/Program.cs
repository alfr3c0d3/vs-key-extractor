using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var jsonFilePath = configuration["JsonFilePath"];

try
{
    var products = await LoadProductsFromJsonAsync(jsonFilePath);

    foreach (var product in products)
        await ExtractLicenseAsync(product);

    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred while processing products: {ex.InnerException?.Message ?? ex.Message}!");
}

async Task<List<Product>> LoadProductsFromJsonAsync(string? jsonFilePath)
{
    try
    {
        if (string.IsNullOrWhiteSpace(jsonFilePath))
            return [];

        var jsonText = await File.ReadAllTextAsync(jsonFilePath);
        return JsonConvert.DeserializeObject<List<Product>>(jsonText) ?? [];
    }
    catch (Exception)
    {
        Console.WriteLine($"Failed to load products from JSON file: {jsonFilePath}!");
        return [];
    }
}

#pragma warning disable CA1416
async Task ExtractLicenseAsync(Product product)
{
    if (string.IsNullOrWhiteSpace(product.Guid) || string.IsNullOrWhiteSpace(product.Mpc))
        return;

    var encrypted = Registry.GetValue($"HKEY_CLASSES_ROOT\\Licenses\\{product.Guid}\\{product.Mpc}", "", null);
    if (encrypted == null) return;

    try
    {
        var secret = ProtectedData.Unprotect((byte[])encrypted, null, DataProtectionScope.CurrentUser);
        var unicode = new UnicodeEncoding();
        var str = unicode.GetString(secret);

        foreach (var sub in str.Split('\0'))
        {
            var match = Regex.Match(sub, @"\w{5}-\w{5}-\w{5}-\w{5}-\w{5}");
            if (match.Success)
            {
                Console.WriteLine($"Found key for {product.Name}: {match.Captures[0]}");
            }
        }

        await Task.CompletedTask;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while extracting license for {product.Name}: {ex.Message}");
    }
}
#pragma warning restore CA1416
