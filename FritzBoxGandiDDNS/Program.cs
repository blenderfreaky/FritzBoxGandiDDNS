using System.Net.Http.Headers;

//var config = new ConfigurationBuilder().AddJsonFile("config.json").Build();
var app = WebApplication.Create(args);
var domainInfo = app.Configuration.GetSection("GandiDomain");

string domainName = domainInfo.GetValue<string>("DomainName");
string apiKey = domainInfo.GetValue<string>("ApiKey");
string recordName = domainInfo.GetValue<string>("RecordName") ?? "@";
string recordType = domainInfo.GetValue<string>("RecordType") ?? "A";

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

#pragma warning disable IDE1006 // Naming Styles
record GandiAUpdateRequest(string rrset_type = "A", int rrset_ttl = 300, params string[] rrset_values);