using System.Net.Http;
using System.Text.Json;

namespace Multitool.Services;

public static class OsintTools
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(10) };

    public static async Task<string> IpLookupAsync(string ip)
    {
        try
        {
            var json = await Http.GetStringAsync($"http://ip-api.com/json/{ip.Trim()}?fields=status,message,country,regionName,city,zip,lat,lon,timezone,isp,org,as,query");
            using var doc = JsonDocument.Parse(json);
            var r = doc.RootElement;
            if (r.GetProperty("status").GetString() != "success")
                return $"Échec: {r.GetProperty("message").GetString()}";
            return $"IP: {r.GetProperty("query").GetString()}\n" +
                   $"Pays: {r.GetProperty("country").GetString()}\n" +
                   $"Région: {r.GetProperty("regionName").GetString()}\n" +
                   $"Ville: {r.GetProperty("city").GetString()} {r.GetProperty("zip").GetString()}\n" +
                   $"Lat/Lon: {r.GetProperty("lat")}, {r.GetProperty("lon")}\n" +
                   $"Timezone: {r.GetProperty("timezone").GetString()}\n" +
                   $"ISP: {r.GetProperty("isp").GetString()}\n" +
                   $"Org: {r.GetProperty("org").GetString()}\n" +
                   $"AS: {r.GetProperty("as").GetString()}";
        }
        catch (Exception ex) { return $"Erreur: {ex.Message}"; }
    }
}
