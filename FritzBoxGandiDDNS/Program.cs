using System.Net.Http.Headers;

var app = WebApplication.Create(args);
var domainInfo = app.Configuration.GetSection("GandiDomain");

string domainName = domainInfo.GetValue<string>("DomainName");
string apiKey = domainInfo.GetValue<string>("ApiKey");
string recordName = domainInfo.GetValue<string>("RecordName") ?? "@";

using HttpClient http = new();
http.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

app.MapGet("/update", async (string? ipv4, string? ipv6) =>
{
    async Task<string> Put(string type, string address) =>
        await (await http.PutAsJsonAsync(
            $"https://dns.api.gandi.net/api/v5/domains/{domainName}/records/{recordName}/A",
            new GandiAUpdateRequest(type, 300, address)))
        .Content.ReadAsStringAsync();

    var now = $"{DateTime.Now:yyyy-MM-ddTHH\\:mm\\:ss}";
    if (ipv4 != null)
    {
        Console.WriteLine($"[{now}] Tried updating IPv4 A record {recordName}.{domainName} to {ipv4} (Response: \"{await Put("A", ipv4)}\")");
    }
    if (ipv6 != null)
    {
        Console.WriteLine($"[{now}] Tried updating IPv6 AAAA record {recordName}.{domainName} to {ipv6} (Response: \"{await Put("AAAA", ipv6)}\")");
    }
});

await app.RunAsync();

#pragma warning disable IDE1006 // Naming Styles
record GandiAUpdateRequest(string rrset_type = "A", int rrset_ttl = 300, params string[] rrset_values);