using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;

namespace Bartz24.FF12;

/// <summary>
/// Navi icon data ("NAVIICN2", ebp section slot 4). The map screen doesn't read it from the ebp:
/// it uses its own copy per map in worldmap/menumap_subNN.bin.
/// </summary>
public static class EbpNaviIcons
{
    public const int SectionCount = 10;

    /// <summary>
    /// Drawn by the map screen only and empty in vanilla. The minimap ties its sections' entries to
    /// map objects by index, so it can't take extras. Entries with a Loc x or z of 0 are skipped.
    /// </summary>
    public const int MapScreenSection = 7;

    private const int EbpSlotTableOffset = 0x10;
    private const int NaviSlot = 4;
    private const int HeaderSize = 0x0A + 4 * SectionCount;
    private const int EntrySize = 0x10;
    private static byte[] Magic = Encoding.ASCII.GetBytes("NAVIICN2");

    /// <summary>
    /// Coordinates are world x/z minus the map's map screen origin. Icon is drawn with a leader line
    /// to Loc when they differ. Condition 0 always shows.
    /// </summary>
    public readonly record struct Entry(short LocX, short LocZ, short IconX, short IconZ, ushort Sprite, ushort Condition, uint Extra = 0);

    /// <summary>The ebp's navi sections; all empty when the map has no navi data.</summary>
    public static List<Entry>[] ReadSections(byte[] ebp)
    {
        int offset = (int)BinaryPrimitives.ReadUInt32LittleEndian(ebp.AsSpan(EbpSlotTableOffset + 4 * NaviSlot));
        return offset == 0 ? EmptySections() : ReadBlob(ebp, offset);
    }

    public static List<Entry>[] ReadBlob(byte[] data, int offset)
    {
        if (!data.AsSpan(offset, Magic.Length).SequenceEqual(Magic))
        {
            throw new InvalidOperationException("Not NAVIICN2 data.");
        }

        int count = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(offset + 8));
        if (count != SectionCount)
        {
            throw new InvalidOperationException($"Unexpected NAVIICN2 section count {count}.");
        }

        List<Entry>[] sections = EmptySections();
        for (int i = 0; i < SectionCount; i++)
        {
            int tableEntry = offset + 0x0A + 4 * i;
            int entriesOffset = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(tableEntry));
            int entryCount = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(tableEntry + 2));
            for (int e = 0; e < entryCount; e++)
            {
                sections[i].Add(ReadEntry(data.AsSpan(offset + entriesOffset + EntrySize * e, EntrySize)));
            }
        }

        return sections;
    }

    public static byte[] BuildBlob(List<Entry>[] sections)
    {
        if (sections.Length != SectionCount)
        {
            throw new ArgumentException($"Expected {SectionCount} sections.", nameof(sections));
        }

        int total = 0;
        foreach (List<Entry> s in sections)
        {
            total += s.Count;
        }

        byte[] blob = new byte[HeaderSize + EntrySize * total];
        Magic.CopyTo(blob, 0);
        BinaryPrimitives.WriteUInt16LittleEndian(blob.AsSpan(8), SectionCount);

        int cursor = HeaderSize;
        for (int i = 0; i < SectionCount; i++)
        {
            if (sections[i].Count == 0)
            {
                continue;
            }

            if (cursor > ushort.MaxValue)
            {
                throw new InvalidOperationException("NAVIICN2 data is too large.");
            }

            BinaryPrimitives.WriteUInt16LittleEndian(blob.AsSpan(0x0A + 4 * i), (ushort)cursor);
            BinaryPrimitives.WriteUInt16LittleEndian(blob.AsSpan(0x0A + 4 * i + 2), (ushort)sections[i].Count);
            foreach (Entry e in sections[i])
            {
                WriteEntry(blob.AsSpan(cursor, EntrySize), e);
                cursor += EntrySize;
            }
        }

        return blob;
    }

    /// <summary>The ebp's navi data with the entries added to the given section, as a standalone blob.</summary>
    public static byte[] BuildWithEntries(byte[] ebp, int section, IEnumerable<Entry> entries)
    {
        if (section is < 0 or >= SectionCount)
        {
            throw new ArgumentOutOfRangeException(nameof(section));
        }

        List<Entry>[] sections = ReadSections(ebp);
        sections[section].AddRange(entries);
        return BuildBlob(sections);
    }

    private static List<Entry>[] EmptySections()
    {
        List<Entry>[] sections = new List<Entry>[SectionCount];
        for (int i = 0; i < SectionCount; i++)
        {
            sections[i] = new List<Entry>();
        }

        return sections;
    }

    private static Entry ReadEntry(ReadOnlySpan<byte> s)
    {
        return new Entry(
            BinaryPrimitives.ReadInt16LittleEndian(s),
            BinaryPrimitives.ReadInt16LittleEndian(s[2..]),
            BinaryPrimitives.ReadInt16LittleEndian(s[4..]),
            BinaryPrimitives.ReadInt16LittleEndian(s[6..]),
            BinaryPrimitives.ReadUInt16LittleEndian(s[8..]),
            BinaryPrimitives.ReadUInt16LittleEndian(s[10..]),
            BinaryPrimitives.ReadUInt32LittleEndian(s[12..]));
    }

    private static void WriteEntry(Span<byte> s, Entry e)
    {
        BinaryPrimitives.WriteInt16LittleEndian(s, e.LocX);
        BinaryPrimitives.WriteInt16LittleEndian(s[2..], e.LocZ);
        BinaryPrimitives.WriteInt16LittleEndian(s[4..], e.IconX);
        BinaryPrimitives.WriteInt16LittleEndian(s[6..], e.IconZ);
        BinaryPrimitives.WriteUInt16LittleEndian(s[8..], e.Sprite);
        BinaryPrimitives.WriteUInt16LittleEndian(s[10..], e.Condition);
        BinaryPrimitives.WriteUInt32LittleEndian(s[12..], e.Extra);
    }
}
