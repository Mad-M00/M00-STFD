using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public static class TtsTriggerScanner
{
    public class TriggerEntry
    {
        public string BlockName;
        public List<byte> Triggers = new List<byte>();
        public List<byte> TriggeredBy = new List<byte>();
        public bool Unlock;
    }

    public class PoiResult
    {
        public string Poi;
        public List<TriggerEntry> Entries = new List<TriggerEntry>();
    }

    static int PopCount(byte[] bytes)
    {
        int c = 0;
        foreach (byte b in bytes)
            for (int i = 0; i < 8; i++)
                if ((b & (1 << i)) != 0) c++;
        return c;
    }

    static Dictionary<int, string> LoadNim(string path)
    {
        var map = new Dictionary<int, string>();
        using (var br = new BinaryReader(File.OpenRead(path)))
        {
            br.ReadInt32(); // version
            int count = br.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int id = br.ReadInt32();
                string name = br.ReadString();
                map[id] = name;
            }
        }
        return map;
    }

    public static PoiResult Scan(string ttsPath)
    {
        string nimPath = Path.ChangeExtension(ttsPath, null) + ".blocks.nim";
        if (!File.Exists(nimPath)) return null;
        var result = new PoiResult { Poi = Path.GetFileNameWithoutExtension(ttsPath) };
        Dictionary<int, string> nim = LoadNim(nimPath);

        using (var fs = File.OpenRead(ttsPath))
        using (var br = new BinaryReader(fs))
        {
            if (br.ReadChar() != 't' || br.ReadChar() != 't' || br.ReadChar() != 's' || br.ReadChar() != (char)0)
                return null;
            uint ver = br.ReadUInt32();
            if (ver <= 16) return null; // no trigger section (or legacy layout we don't handle)
            int sx = br.ReadInt16(), sy = br.ReadInt16(), sz = br.ReadInt16();
            long blockCount = (long)sx * sy * sz;
            long blocksStart = fs.Position;
            fs.Seek(blockCount * 4, SeekOrigin.Current);      // block values
            fs.Seek(blockCount, SeekOrigin.Current);          // density (ver > 4 path)
            if (ver > 8) fs.Seek(blockCount * 2, SeekOrigin.Current); // damage
            if (ver >= 10)                                    // textures bitstream
            {
                int n = br.ReadInt32();
                byte[] bits = br.ReadBytes(n);
                fs.Seek((long)PopCount(bits) * 8, SeekOrigin.Current);
            }
            if (ver >= 17)                                    // water bitstream
            {
                int n = br.ReadInt32();
                byte[] bits = br.ReadBytes(n);
                fs.Seek((long)PopCount(bits) * 2, SeekOrigin.Current);
            }
            int teCount = br.ReadInt16();                     // tile entities (ver > 12)
            for (int i = 0; i < teCount; i++)
            {
                int len = br.ReadInt16();
                br.ReadByte(); // TileEntityType
                fs.Seek(len, SeekOrigin.Current);
            }
            int trigCount = br.ReadInt16();                   // triggers (ver > 15)
            for (int i = 0; i < trigCount; i++)
            {
                int len = br.ReadInt16();
                int x = br.ReadInt32(), y = br.ReadInt32(), z = br.ReadInt32();
                long next = fs.Position + len;
                var e = new TriggerEntry();
                ushort tver = br.ReadUInt16();
                if (tver >= 2) br.ReadByte(); // NeedsTriggered
                int n1 = br.ReadByte(); for (int k = 0; k < n1; k++) e.Triggers.Add(br.ReadByte());
                int n2 = br.ReadByte(); for (int k = 0; k < n2; k++) e.TriggeredBy.Add(br.ReadByte());
                int n3 = br.ReadByte(); for (int k = 0; k < n3; k++) br.ReadByte(); // TriggeredValues
                if (tver >= 3) br.ReadBoolean(); // ExcludeIcon
                if (tver >= 4) br.ReadBoolean(); // UseOrForMultipleTriggers
                if (tver >= 5) e.Unlock = br.ReadBoolean();
                fs.Seek(next, SeekOrigin.Begin);

                // resolve the block name at the trigger's position
                long offset = x + (long)y * sx + (long)z * sx * sy;
                long save = fs.Position;
                fs.Seek(blocksStart + offset * 4, SeekOrigin.Begin);
                uint raw = br.ReadUInt32();
                fs.Seek(save, SeekOrigin.Begin);
                int typeId = (int)(raw & 0xFFFF);
                string name;
                e.BlockName = nim.TryGetValue(typeId, out name) ? name : ("#" + typeId);
                result.Entries.Add(e);
            }
        }
        return result;
    }

    public static string Report(string poiDir, string doorListFile, string switchListFile)
    {
        var doors = new HashSet<string>(File.ReadAllLines(doorListFile).Where(s => s.Length > 0));
        var switches = new HashSet<string>(File.ReadAllLines(switchListFile).Where(s => s.Length > 0));
        var sb = new StringBuilder();
        int scanned = 0, failed = 0;
        var rows = new List<string[]>();
        foreach (string tts in Directory.GetFiles(poiDir, "*.tts"))
        {
            PoiResult r = null;
            try { r = Scan(tts); scanned++; }
            catch { failed++; continue; }
            if (r == null || r.Entries.Count == 0) continue;

            // channels driven by a player-usable switch
            var switchChannels = new HashSet<byte>(
                r.Entries.Where(e => switches.Contains(e.BlockName)).SelectMany(e => e.Triggers));

            foreach (var e in r.Entries)
            {
                if (!doors.Contains(e.BlockName) || e.TriggeredBy.Count == 0) continue;
                bool bySwitch = e.TriggeredBy.Any(c => switchChannels.Contains(c));
                rows.Add(new[]
                {
                    r.Poi,
                    e.BlockName,
                    e.Unlock ? "unlocks" : "STAYS-LOCKED",
                    bySwitch ? "switch" : "other-trigger"
                });
            }
        }
        sb.AppendLine("scanned=" + scanned + " failed=" + failed);
        foreach (var row in rows)
            sb.AppendLine(string.Join("|", row));
        return sb.ToString();
    }
}
