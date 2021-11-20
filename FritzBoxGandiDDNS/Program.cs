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

async Task PutRecord(string name, string type, string? address)
{
    if (string.IsNullOrWhiteSpace(address)) return;
    var response = await (await http.PutAsJsonAsync(
            $"https://dns.api.gandi.net/api/v5/domains/{domainName}/records/{recordName}/{type}",
            new GandiAUpdateRequest(type, 300, address)))
        .Content.ReadAsStringAsync();
    var now = $"{DateTime.Now:yyyy-MM-ddTHH\\:mm\\:ss}";
    Console.WriteLine($"[{now}] Tried updating {name} {type} record {recordName}.{domainName} to {address} (Response: \"{response}\")");
}

Task PutIPv4(string? address) => PutRecord("IPv4", "A", address);
Task PutIPv6(string? address) => PutRecord("IPv6", "AAAA", address);

async Task UpdateRecords()
{
    Console.WriteLine("Updating IP from server");
    await Task.WhenAll(PutIPv4(await http.GetStringAsync("https://ipv4.icanhazip.com/")), PutIPv6(await http.GetStringAsync("https://ipv6.icanhazip.com/")));
}

app.MapGet("/update", async (string? ipv4, string? ipv6) =>
{
    Console.WriteLine("Pinged by Fritz!BOX, updating IP");
    await Task.WhenAll(PutIPv4(ipv4), PutIPv6(ipv6));
});

CancellationTokenSource cts = new();

var regularUpdate = Task.Run(async () =>
{
    await UpdateRecords();
    await Task.Delay(TimeSpan.FromMinutes(30), cts.Token);
}, cts.Token);

await app.RunAsync();

cts.Cancel();

#pragma warning disable IDE1006 // Naming Styles
record GandiAUpdateRequest(string rrset_type = "A", int rrset_ttl = 300, params string[] rrset_values);