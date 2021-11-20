using Microsoft.Extensions.Primitives;

var app = WebApplication.Create(args);
var domainInfo = app.Configuration.GetSection("GandiDomain");

string domainName = "", apiKey = "", recordName = "@";

Action updateConfig = () =>
{
    domainName = domainInfo.GetValue<string>("DomainName");
    apiKey = domainInfo.GetValue<string>("ApiKey");
    recordName = domainInfo.GetValue<string>("RecordName") ?? "@";
};
updateConfig();

ChangeToken.OnChange(() => app.Configuration.GetReloadToken(), updateConfig);

using HttpClient http = new();
http.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

app.MapGet("/update", async (string? ipv4, string? ipv6) =>
{
    var now = $"{DateTime.Now:yyyy-MM-ddTHH\\:mm\\:ss}";
    async Task Put(string name, string type, string address)
    {
        var response = await (await http.PutAsJsonAsync(
                $"https://dns.api.gandi.net/api/v5/domains/{domainName}/records/{recordName}/{type}",
                new GandiAUpdateRequest(type, 300, address)))
            .Content.ReadAsStringAsync();
        Console.WriteLine($"[{now}] Tried updating {name} {type} record {recordName}.{domainName} to {ipv4} (Response: \"{response}\")");
    }

    if (ipv4 != null) await Put("IPv4", "A", ipv4);
    if (ipv6 != null) await Put("IPv6", "AAAA", ipv6);
});

await app.RunAsync();

#pragma warning disable IDE1006 // Naming Styles
record GandiAUpdateRequest(string rrset_type = "A", int rrset_ttl = 300, params string[] rrset_values);