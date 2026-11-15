using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Bartz24.FF12;

/// <summary>
/// One piece of a line built by <see cref="TextSanitizer.FitParts"/>. Only <see cref="Name"/> is ever shortened.
/// </summary>
public readonly record struct TextPart(string Name, string Suffix = "", bool CanShorten = true);

public static class TextSanitizer
{
    public const int MaxChoiceItemLength = 127;

    private static readonly HashSet<char> SupportedExtra = new("¡«°»¿ÀÁÂÄÆÇÈÉÊËÌÍÎÏÑÒÓÔÖ×ÙÚÛÜßàáâäæçèéêëìíîïñòóôöùúûüŒœ—“”„…←↑→↓⇒∞★♪");

    private static readonly Dictionary<char, string> Replacements = new()
    {
        { '‘', "'" }, { '’', "'" }, { '‚', "'" }, { '‛', "'" }, { '′', "'" }, { '´', "'" },
        { '″', "\"" },
        { '‐', "-" }, { '‑', "-" }, { '‒', "-" }, { '–', "-" }, { '―', "-" }, { '−', "-" },
        { '™', "TM" }, { '©', "(c)" }, { '®', "(R)" },
        { '½', "1/2" }, { '¼', "1/4" }, { '¾', "3/4" },
        { 'ø', "o" }, { 'Ø', "O" }, { 'ł', "l" }, { 'Ł', "L" }, { 'đ', "d" }, { 'Đ', "D" }, { 'ı', "i" },
        { '•', "*" }, { '·', "." },
        // Braces start text tags ({item}, {/item}, ...), so replace those.
        { '{', "(" }, { '}', ")" },
        { '\n', " " }, { '\r', " " }, { '\t', " " },
    };

    private static readonly HashSet<char> DoubleWidth = new("°⇒★");

    public static bool IsSupported(char c)
    {
        return (c >= ' ' && c <= '~') || SupportedExtra.Contains(c);
    }

    public static int Length(string text)
    {
        return text.Sum(c => DoubleWidth.Contains(c) ? 2 : 1);
    }

    public static string Truncate(string text, int maxLength)
    {
        if (Length(text) <= maxLength)
        {
            return text;
        }

        StringBuilder result = new();
        int length = 0;
        foreach (char c in text)
        {
            int width = DoubleWidth.Contains(c) ? 2 : 1;
            // Leave a unit for the …
            if (length + width > maxLength - 1)
            {
                break;
            }

            result.Append(c);
            length += width;
        }

        return result.ToString().TrimEnd(' ', ',') + "…";
    }

    public static string FitParts(IReadOnlyList<TextPart> parts, string separator, int maxLength)
    {
        List<TextPart> sanitized = parts.Select(p => p with { Name = Sanitize(p.Name), Suffix = Sanitize(p.Suffix) }).ToList();
        string joined = string.Join(separator, sanitized.Select(p => p.Name + p.Suffix));
        if (Length(joined) <= maxLength)
        {
            return joined;
        }

        const int minNameLength = 2;

        List<int> shortenable = sanitized.Where(p => p.CanShorten).Select(p => Length(p.Name)).ToList();
        int fixedLength = Length(separator) * (sanitized.Count - 1)
            + sanitized.Sum(p => Length(p.Suffix) + (p.CanShorten ? 0 : Length(p.Name)));
        int budget = maxLength - fixedLength;

        int cap = shortenable.Count == 0 ? 0 : shortenable.Max();
        while (cap >= minNameLength && shortenable.Sum(l => Math.Min(l, cap)) > budget)
        {
            cap--;
        }

        if (cap < minNameLength)
        {
            return Truncate(joined, maxLength);
        }

        return string.Join(separator, sanitized.Select(p => (p.CanShorten ? Truncate(p.Name, cap) : p.Name) + p.Suffix));
    }

    public static string Sanitize(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        StringBuilder result = new();
        foreach (Rune rune in Normalize(text, NormalizationForm.FormC).EnumerateRunes())
        {
            if (!rune.IsBmp)
            {
                result.Append('?');
                continue;
            }

            char c = (char)rune.Value;
            if (Replacements.TryGetValue(c, out string replacement))
            {
                result.Append(replacement);
            }
            else if (IsSupported(c))
            {
                result.Append(c);
            }
            else
            {
                result.Append(Decompose(c));
            }
        }

        return result.ToString();
    }
    
    private static string Decompose(char c)
    {
        StringBuilder kept = new();
        foreach (char part in Normalize(c.ToString(), NormalizationForm.FormKD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(part) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (Replacements.TryGetValue(part, out string replacement))
            {
                kept.Append(replacement);
            }
            else if (IsSupported(part))
            {
                kept.Append(part);
            }
            else
            {
                return "?";
            }
        }

        return kept.Length > 0 ? kept.ToString() : "?";
    }

    private static string Normalize(string text, NormalizationForm form)
    {
        try
        {
            return text.Normalize(form);
        }
        catch (System.ArgumentException)
        {
            return text;
        }
    }
}
