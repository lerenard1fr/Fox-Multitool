namespace Multitool.Services;

public static class Lang
{
    public static string Current { get; private set; } = "en";
    public static event Action? Changed;

    private static readonly Dictionary<string, Dictionary<string, string>> Strings = new()
    {
        ["en"] = new()
        {
            ["menu_network"] = "🌐  Network",
            ["menu_system"] = "🖥️  System",
            ["menu_osint"] = "🔍  OSINT",
            ["menu_generators"] = "⚙️  Generators",
            ["menu_options"] = "Options",
            ["nav_base"] = "Base IP / Ping / Ports",
            ["nav_nmap"] = "Nmap Scan",
            ["nav_dns"] = "DNS / NS Lookup",
            ["nav_extra"] = "Traceroute / ARP / Netstat",
            ["nav_sys"] = "Info / Process / Clean",
            ["nav_osint"] = "IP Lookup",
            ["nav_gen"] = "Pass / GUID / Hash",
            ["status_checking"] = "Checking tools...",
            ["status_ok"] = "● nmap OK ({0})  •  {1}/{2} tools ready",
            ["status_missing"] = "● nmap MISSING — go to Network > Nmap Scan to install  •  {1}/{2} tools ready",
            ["settings_title"] = "Options",
            ["settings_theme"] = "Theme (default: System)",
            ["settings_theme_system"] = "System (follows Windows)",
            ["settings_theme_dark"] = "Dark",
            ["settings_theme_light"] = "Light",
            ["settings_lang"] = "Language (default: English)",
            ["settings_lang_en"] = "English",
            ["settings_lang_fr"] = "Français",
            ["settings_saved"] = "Saved automatically.",
            ["settings_theme_note"] = "System = uses your Windows theme at startup.",
        },
        ["fr"] = new()
        {
            ["menu_network"] = "🌐  Réseau",
            ["menu_system"] = "🖥️  Système",
            ["menu_osint"] = "🔍  OSINT",
            ["menu_generators"] = "⚙️  Générateurs",
            ["menu_options"] = "Options",
            ["nav_base"] = "Base IP / Ping / Ports",
            ["nav_nmap"] = "Scan Nmap",
            ["nav_dns"] = "DNS / NS Lookup",
            ["nav_extra"] = "Traceroute / ARP / Netstat",
            ["nav_sys"] = "Infos / Process / Nettoyage",
            ["nav_osint"] = "IP Lookup",
            ["nav_gen"] = "MDP / GUID / Hash",
            ["status_checking"] = "Vérification outils...",
            ["status_ok"] = "● nmap OK ({0})  •  {1}/{2} outils dispo",
            ["status_missing"] = "● nmap MANQUANT — va dans Réseau > Nmap Scan pour l'installer  •  {1}/{2} outils dispo",
            ["settings_title"] = "Options",
            ["settings_theme"] = "Thème (défaut : Système)",
            ["settings_theme_system"] = "Système (suit Windows)",
            ["settings_theme_dark"] = "Sombre",
            ["settings_theme_light"] = "Clair",
            ["settings_lang"] = "Langue (défaut : Anglais)",
            ["settings_lang_en"] = "English",
            ["settings_lang_fr"] = "Français",
            ["settings_saved"] = "Enregistré automatiquement.",
            ["settings_theme_note"] = "Système = utilise votre thème Windows au démarrage.",
        },
    };

    public static void Set(string lang)
    {
        if (lang is not ("en" or "fr")) lang = "en";
        Current = lang;
        Changed?.Invoke();
    }

    public static string T(string key)
        => Strings[Current].TryGetValue(key, out var v) ? v
        : Strings["en"].TryGetValue(key, out var e) ? e : key;
}
