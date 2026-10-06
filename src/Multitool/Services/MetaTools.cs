using MetadataExtractor;
using System.IO;

namespace Multitool.Services;

/// <summary>Lecture + suppression de metadonnees de fichiers (EXIF/IPTC/XMP...).</summary>
public static class MetaTools
{
    public static (int Tags, string Text) Scan(string path)
    {
        try
        {
            var dirs = ImageMetadataReader.ReadMetadata(path).ToList();
            var sb = new System.Text.StringBuilder();
            int n = 0;
            foreach (var d in dirs)
            {
                if (d.HasError) continue;
                foreach (var t in d.Tags)
                {
                    sb.AppendLine($"{d.Name} - {t.Name} = {t.Description}");
                    n++;
                }
            }
            if (n == 0) sb.AppendLine("(no metadata)");
            return (n, sb.ToString().Trim());
        }
        catch (Exception ex) { return (0, $"Error: {ex.Message}"); }
    }

    public static string Clean(string path)
    {
        string ext = Path.GetExtension(path).ToLowerInvariant();
        bool supported = ext is ".jpg" or ".jpeg" or ".png";
        string outExt = supported ? ext : ".png"; // autres formats : re-encode PNG sans metadonnees
        string dest = Path.Combine(Path.GetDirectoryName(path) ?? AppPaths.DataDir,
            $"{Path.GetFileNameWithoutExtension(path)}-clean{outExt}");
        byte[] data = File.ReadAllBytes(path);
        byte[] clean = ext switch
        {
            ".jpg" or ".jpeg" => StripJpeg(data),
            ".png" => StripPng(data),
            _ => GdiStrip(path),
        };
        File.WriteAllBytes(dest, clean);
        return dest;
    }

    // Suppression au niveau octet : tous les segments APPn (EXIF/IPTC/XMP...)
    // et COM sont abandonnes, sauf APP0 (JFIF) et APP14 (couleur Adobe).
    private static byte[] StripJpeg(byte[] src)
    {
        if (src.Length < 4 || src[0] != 0xFF || src[1] != 0xD8)
            throw new InvalidDataException("Not a JPEG.");
        var out_ = new List<byte> { 0xFF, 0xD8 };
        int pos = 2;
        while (pos + 3 < src.Length)
        {
            if (src[pos] != 0xFF) break;
            byte marker = src[pos + 1];
            if (marker == 0xD9) { out_.AddRange(new byte[] { 0xFF, 0xD9 }); break; }
            if (marker == 0xDA) { out_.AddRange(src[pos..]); break; } // SOS + donnees : copie brute
            if (marker is 0x01 or >= 0xD0 and <= 0xD7) { out_.AddRange(src[pos..(pos + 2)]); pos += 2; continue; }
            int len = (src[pos + 2] << 8) | src[pos + 3];
            if (len < 2 || pos + 2 + len > src.Length) break;
            bool drop = (marker is >= 0xE1 and <= 0xEF and not 0xEE) || marker == 0xFE;
            if (!drop) out_.AddRange(src[pos..(pos + 2 + len)]);
            pos += 2 + len;
        }
        return [.. out_];
    }

    // PNG : abandonne les chunks texte (tEXt/zTXt/iTXt) et eXIf, le reste est copie tel quel (CRC intacts).
    private static byte[] StripPng(byte[] src)
    {
        byte[] sig = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        if (src.Length < 8 || !src[..8].SequenceEqual(sig))
            throw new InvalidDataException("Not a PNG.");
        var out_ = new List<byte>(sig);
        int pos = 8;
        while (pos + 11 < src.Length)
        {
            int len = (src[pos] << 24) | (src[pos + 1] << 16) | (src[pos + 2] << 8) | src[pos + 3];
            string type = System.Text.Encoding.ASCII.GetString(src, pos + 4, 4);
            if (len < 0 || pos + 12 + len > src.Length) break;
            if (type is not ("tEXt" or "zTXt" or "iTXt" or "eXIf"))
                out_.AddRange(src[pos..(pos + 12 + len)]);
            pos += 12 + len;
            if (type == "IEND") break;
        }
        return [.. out_];
    }

    // Autres formats : re-encodage GDI+ sans les proprietes (au mieux).
    private static byte[] GdiStrip(string path)
    {
        using var bmp = new System.Drawing.Bitmap(path);
        foreach (int id in bmp.PropertyIdList)
        {
            try { bmp.RemovePropertyItem(id); } catch { }
        }
        using var ms = new MemoryStream();
        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        return ms.ToArray();
    }
}
