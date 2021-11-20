using System.Net.Http.Headers;
namespace FritzBoxGandiDDNS;

public static class Program
{
    /// <param name="domainName">The name of your domain.</param>
    /// <param name="apiKey">Your Gandi API Key.</param>
    /// <param name="recordName">The name of the DNS record of your domain. Default: "@"</param>
    /// <param name="recordType">The type of the DNS record of your domain. Default: "A"</param>
    /// <param name="aspNetArgs">Arguments for ASP.NET.</param>
    public static async Task Main(string? domainName, string? apiKey, string? recordName = "@", string? recordType = "A", string[]? aspNetArgs = null)
    {
        var app = WebApplication.Create(aspNetArgs ?? Array.Empty<string>());
        using HttpClient http = new();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("X-Api-Key", apiKey);

        app.MapGet("/update", async (string ipv4) =>
        {
            GandiAUpdateRequest request = new();
            var response = await (
                await http.PutAsJsonAsync($"https://dns.api.gandi.net/api/v5/domains/{domainName}/records/{recordName}/{recordType}", request))
                .Content.ReadAsStringAsync();

            Console.WriteLine($"[{DateTime.Now:yyyy-MM-ddTHH\\:mm\\:ss}] {(response == "DNS Record Created" ? "Successfully updated" : "Failed to update")} IP record to {ipv4}");
        });

        await app.RunAsync();
    }

#pragma warning disable IDE1006 // Naming Styles
    record GandiAUpdateRequest(string rrset_type = "A", int rrset_ttl = 300, params string[] rrset_values);
}