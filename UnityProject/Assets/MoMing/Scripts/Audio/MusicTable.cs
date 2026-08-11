using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音乐表格：把一份 CSV 解析成一张“Id → 音乐配置”的表。
/// 设计目标：策划/音效只负责“填表”（编辑 CSV），程序在合适的时机按 Id 播放。
/// 兼容 Unity 2022.3（纯 C#，无 async/await，无 6000 专属 API）。
///
/// CSV 约定：
///   - 第一行是表头（列名），列的顺序无所谓，按列名匹配。
///   - 以 '#' 开头的行是注释，会被跳过。
///   - 字段里请不要出现英文逗号 ','（用作分隔符）。
///   - 必填列：Id, ClipName。其余列缺省用默认值。
/// </summary>
public class MusicTable
{
    // 一行 = 一条音乐配置
    [System.Serializable]
    public class Entry
    {
        public string id;          // 唯一键，程序用它来点播，如 "bgm_puzzle"
        public string clipName;    // 音频文件名（放在 Resources 下，见 MusicManager.clipResourceFolder）
        public float volume = 1f;  // 音量 0~1
        public bool loop = true;   // 是否循环（BGM/氛围一般 true）
        public float fadeIn = 1f;  // 淡入秒数
        public float fadeOut = 1f; // 淡出秒数
        public string note = "";   // 备注（给人看的，程序不用）
    }

    private readonly Dictionary<string, Entry> _map = new Dictionary<string, Entry>();

    public int Count => _map.Count;
    public IEnumerable<Entry> All => _map.Values;

    /// <summary>按 Id 取一条配置，找不到返回 null。</summary>
    public Entry Get(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        Entry e;
        return _map.TryGetValue(id, out e) ? e : null;
    }

    public bool Contains(string id) => !string.IsNullOrEmpty(id) && _map.ContainsKey(id);

    /// <summary>
    /// 把一整段 CSV 文本解析成表。解析失败的单行会跳过并打日志，不会抛异常。
    /// </summary>
    public static MusicTable Parse(string csvText)
    {
        var table = new MusicTable();
        if (string.IsNullOrEmpty(csvText))
        {
            Debug.LogWarning("[MusicTable] CSV 内容为空。");
            return table;
        }

        // 统一换行符后按行切
        string[] lines = csvText.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

        // 找表头（第一条非空、非注释行）
        int headerIndex = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            string t = lines[i].Trim();
            if (t.Length == 0 || t.StartsWith("#")) continue;
            headerIndex = i;
            break;
        }
        if (headerIndex < 0)
        {
            Debug.LogWarning("[MusicTable] 找不到表头行。");
            return table;
        }

        // 表头列名 → 列号
        string[] headers = SplitLine(lines[headerIndex]);
        var col = new Dictionary<string, int>();
        for (int c = 0; c < headers.Length; c++)
            col[headers[c].Trim().ToLowerInvariant()] = c;

        if (!col.ContainsKey("id") || !col.ContainsKey("clipname"))
        {
            Debug.LogError("[MusicTable] 表头必须包含 Id 和 ClipName 两列。");
            return table;
        }

        // 逐行解析数据
        for (int i = headerIndex + 1; i < lines.Length; i++)
        {
            string raw = lines[i].Trim();
            if (raw.Length == 0 || raw.StartsWith("#")) continue;

            string[] cells = SplitLine(lines[i]);

            var e = new Entry();
            e.id = Cell(cells, col, "id");
            e.clipName = Cell(cells, col, "clipname");
            if (string.IsNullOrEmpty(e.id))
            {
                Debug.LogWarning("[MusicTable] 第 " + (i + 1) + " 行没有 Id，跳过。");
                continue;
            }

            e.volume = ParseFloat(Cell(cells, col, "volume"), 1f);
            e.loop = ParseBool(Cell(cells, col, "loop"), true);
            e.fadeIn = ParseFloat(Cell(cells, col, "fadein"), 1f);
            e.fadeOut = ParseFloat(Cell(cells, col, "fadeout"), 1f);
            e.note = Cell(cells, col, "note");

            if (table._map.ContainsKey(e.id))
                Debug.LogWarning("[MusicTable] Id 重复：" + e.id + "，后者覆盖前者。");
            table._map[e.id] = e;
        }

        Debug.Log("[MusicTable] 解析完成，共 " + table.Count + " 条音乐配置。");
        return table;
    }

    // --- 小工具 ---

    static string[] SplitLine(string line)
    {
        return line.Split(',');
    }

    static string Cell(string[] cells, Dictionary<string, int> col, string name)
    {
        int idx;
        if (!col.TryGetValue(name, out idx)) return "";
        if (idx < 0 || idx >= cells.Length) return "";
        return cells[idx].Trim();
    }

    static float ParseFloat(string s, float fallback)
    {
        float v;
        // InvariantCulture：避免某些系统把小数点当成逗号
        return float.TryParse(s, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out v) ? v : fallback;
    }

    static bool ParseBool(string s, bool fallback)
    {
        if (string.IsNullOrEmpty(s)) return fallback;
        s = s.Trim().ToLowerInvariant();
        if (s == "1" || s == "true" || s == "yes" || s == "y") return true;
        if (s == "0" || s == "false" || s == "no" || s == "n") return false;
        return fallback;
    }
}
