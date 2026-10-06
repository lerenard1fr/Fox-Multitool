using PhoneNumbers;

namespace Multitool.Services;

public sealed record PhoneInfo(bool Ok, bool Valid, string Region, string Type,
    string Carrier, string Geo, string Intl, string E164);

/// <summary>Analyse de numero hors-ligne (libphonenumber, aucune API).</summary>
public static class PhoneTools
{
    public static PhoneInfo Parse(string input, string defaultRegion = "FR")
    {
        try
        {
            var util = PhoneNumberUtil.GetInstance();
            var num = util.Parse(input.Trim(), defaultRegion);
            bool valid = util.IsValidNumber(num);
            string region = util.GetRegionCodeForNumber(num) ?? "?";
            string type = util.GetNumberType(num).ToString();
            string carrier;
            try { carrier = PhoneNumberToCarrierMapper.GetInstance().GetNameForNumber(num, Locale.English); }
            catch { carrier = ""; }
            string geo;
            try { geo = PhoneNumberOfflineGeocoder.GetInstance().GetDescriptionForNumber(num, Locale.English); }
            catch { geo = ""; }
            string intl;
            try { intl = util.Format(num, PhoneNumberFormat.INTERNATIONAL); }
            catch { intl = ""; }
            string e164;
            try { e164 = util.Format(num, PhoneNumberFormat.E164); }
            catch { e164 = ""; }
            return new PhoneInfo(true, valid, region, type, carrier, geo, intl, e164);
        }
        catch
        {
            return new PhoneInfo(false, false, "", "", "", "", "", "");
        }
    }
}
