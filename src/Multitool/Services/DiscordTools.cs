using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Multitool.Services;

/// <summary>
/// Outils Discord sans token : envoi via webhook (le votre),
/// infos webhook/invite via l'API publique, date de creation depuis un ID (snowflake).
/// </summary>
public static class DiscordTools
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(10) };
    private const long DiscordEpoch = 1420070400000L;

    static DiscordTools()
    {
        Http.DefaultRequestHeaders.UserAgent.ParseAdd("FoxMultitool/1.0");
    }

    public static bool IsWebhookUrl(string url)
        => Regex.IsMatch(url.Trim(),
            @"^https://(ptb\.|canary\.)?discord\.com/api/webhooks/\d+/[A-Za-z0-9_\-]+/?$");

    public static async Task<string> SendWebhookAsync(string url, string content, string username)
    {
        url = url.Trim();
        if (!IsWebhookUrl(url)) return Lang.T("v_dis_badurl");
        if (string.IsNullOrWhiteSpace(content)) return Lang.T("v_dis_emptymsg");
        try
        {
            var payload = new { content, username = string.IsNullOrWhiteSpace(username) ? null : username.Trim() };
            using var resp = await Http.PostAsync(url,
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));
            return resp.IsSuccessStatusCode
                ? Lang.T("v_dis_sent")
                : $"HTTP {(int)resp.StatusCode} : {(await resp.Content.ReadAsStringAsync()).Trim()}";
        }
        catch (Exception ex) { return $"{Lang.T("svc_error")}{ex.Message}"; }
    }

    public static async Task<string> WebhookInfoAsync(string url)
    {
        url = url.Trim().TrimEnd('/');
        if (!IsWebhookUrl(url)) return Lang.T("v_dis_badurl");
        try
        {
            using var resp = await Http.GetAsync(url);
            if (!resp.IsSuccessStatusCode) return $"HTTP {(int)resp.StatusCode}";
            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            var r = doc.RootElement;
            string Get(string k) => r.TryGetProperty(k, out var v) ? v.GetString() ?? "?" : "?";
            return $"Name: {Get("name")}\nChannel ID: {Get("channel_id")}\nServer ID: {Get("guild_id")}\nType: {Get("type")}";
        }
        catch (Exception ex) { return $"{Lang.T("svc_error")}{ex.Message}"; }
    }

    public static string SnowflakeInfo(string input)
    {
        if (!ulong.TryParse(input.Trim(), out ulong id) || id == 0)
            return Lang.T("v_dis_badid");
        try
        {
            var dt = DateTimeOffset.FromUnixTimeMilliseconds((long)(id >> 22) + DiscordEpoch);
            var age = DateTimeOffset.UtcNow - dt;
            string ageTxt = age.TotalDays >= 365
                ? $"{(int)(age.TotalDays / 365)}y {(int)(age.TotalDays % 365)}d"
                : $"{(int)age.TotalDays}d";
            return $"{Lang.T("v_dis_created")}{dt:yyyy-MM-dd HH:mm:ss} UTC\n" +
                   $"Local: {dt.ToLocalTime():yyyy-MM-dd HH:mm:ss}\n" +
                   $"{Lang.T("v_dis_age")}{ageTxt}";
        }
        catch { return Lang.T("v_dis_badid"); }
    }

    public static async Task<string> InviteInfoAsync(string input)
    {
        string code = input.Trim();
        foreach (var prefix in new[] { "https://discord.gg/", "http://discord.gg/", "discord.gg/",
                                       "https://discord.com/invite/", "https://canary.discord.com/invite/" })
            if (code.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                code = code[prefix.Length..];
        code = code.Split(['/', '?', '#'])[0];
        if (string.IsNullOrWhiteSpace(code) || code.Any(c => !char.IsLetterOrDigit(c) && c is not '-' and not '_'))
            return Lang.T("v_dis_badinvite");
        try
        {
            using var resp = await Http.GetAsync($"https://discord.com/api/v10/invites/{code}?with_counts=true");
            if (!resp.IsSuccessStatusCode) return $"HTTP {(int)resp.StatusCode} ({Lang.T("v_dis_badinvite")})";
            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            var r = doc.RootElement;
            string guild = r.TryGetProperty("guild", out var g) && g.TryGetProperty("name", out var n) ? n.GetString() ?? "?" : "?";
            string chan = r.TryGetProperty("channel", out var c) && c.TryGetProperty("name", out var cn) ? cn.GetString() ?? "?" : "?";
            string members = r.TryGetProperty("approximate_member_count", out var m) ? m.ToString() : "?";
            string online = r.TryGetProperty("approximate_presence_count", out var p) ? p.ToString() : "?";
            return $"Server: {guild}\nChannel: #{chan}\nMembers: {members} ({online} online)";
        }
        catch (Exception ex) { return $"{Lang.T("svc_error")}{ex.Message}"; }
    }
}
