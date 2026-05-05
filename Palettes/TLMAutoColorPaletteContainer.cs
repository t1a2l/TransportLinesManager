using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Commons.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TransportLinesManager.Palettes
{
    public class TLMAutoColorPaletteContainer
    {
        public const string PALETTE_RANDOM = "<RANDOM>";
        public const char SERIALIZER_ITEM_SEPARATOR = '∞';
        private static readonly RandomPastelColorGenerator gen = new();
        private static Dictionary<string, TLMAutoColorPalette> m_palettes = null;
        public static readonly List<Color32> SaoPaulo2035 = new([
            new(117, 0, 0, 255),
            new(0, 13, 160, 255),
            new(0, 128, 27, 255),
            new(250, 0, 0, 255),
            new(255, 213, 3, 255),
            new(165, 67, 153, 255),
            new(244, 115, 33, 255),
            new(159, 24, 102, 255),
            new(158, 158, 148, 255),
            new(0, 168, 142, 255),
            new(4, 124, 140, 255),
            new(240, 78, 35, 255),
            new(4, 43, 106, 255),
            new(0, 172, 92, 255),
            new(30, 30, 30, 255),
            new(180, 178, 177, 255),
            new(255, 255, 255, 255),
            new(245, 158, 55, 255),
            new(167, 139, 107, 255),
            new(0, 149, 218, 255),
            new(252, 124, 161, 255),
            new(95, 44, 143, 255),
            new(92, 58, 14, 255),
            new(0, 0, 0, 255),
            new(100, 100, 100, 255),
            new(202, 187, 168, 255),
            new(0, 0, 255, 255),
            new(208, 45, 255, 255),
            new(0, 255, 0, 255),
            new(255, 252, 186, 255)
        ]);
        public static readonly List<Color32> London2016 = new([
            new(137,78,36,255),
            new(220,36,31,255),
            new(225,206,0,255),
            new(0,114,41,255),
            new(215,153,175,255),
            new(134,143,152,255),
            new(117,16,86,255),
            new(0,0,0,255),
            new(0,25,168,255),
            new(0,160,226,255),
            new(118,208,189,255),
            new(102,204,0,255),
            new(232,106,16,255)
        ]);
        public static readonly List<Color32> Rainbow = new([
            new(   25,12 ,243 ,255),
            new(   36,12 ,243 ,255),
            new(   56,73 ,245 ,255),
            new(   85,156,246 ,255),
            new(  111,233,179 ,255),
            new(   93,201, 97 ,255),
            new(   80,170, 40 ,255),
            new(   81,164, 25 ,255),
            new(  115,195, 29 ,255),
            new(  152,220, 31 ,255),
            new(  249,254, 41 ,255),
            new(  233,222, 36 ,255),
            new(  227,194, 33 ,255),
            new(  219,161, 32 ,255),
            new(  202, 96, 26 ,255),
            new(  192, 49, 24 ,255),
            new(  189,  3, 23 ,255),
            new(  133,  0, 28 ,255),
            new(   73,  1, 63 ,255),
            new(   44,  4, 94 ,255)
        ]);

        public static readonly List<Color32> RainbowShort = new([
            new( 160,  0,200  ,255),
            new( 130,  0,220  ,255),
            new(  30, 60,255  ,255),
            new(   0,160,255  ,255),
            new(   0,200,200  ,255),
            new(   0,210,140  ,255),
            new(   0,220,  0  ,255),
            new( 160,230, 50  ,255),
            new( 230,220, 50  ,255),
            new( 230,175, 45  ,255),
            new( 240,130, 40  ,255),
            new( 250, 60, 60  ,255),
            new( 240,  0,130  ,255)
        ]);


        public static readonly List<Color32> WorldMix = new([
            new(0, 0, 0        ,255),
            new(230, 25, 75     ,255),
            new(60, 180, 75     ,255),
            new(255, 225, 25    ,255),
            new(0, 130, 200     ,255),
            new(245, 130, 48    ,255),
            new(145, 30, 180    ,255),
            new(70, 240, 240    ,255),
            new(240, 50, 230    ,255),
            new(210, 245, 60    ,255),
            new(250, 190, 190 ,255),
            new(0, 128, 128     ,255),
            new(230, 190, 255 ,255),
            new(170, 110, 40    ,255),
            new(255, 250, 200 ,255),
            new(128, 0, 0 ,255),
            new(170, 255, 195 ,255),
            new(128, 128, 0     ,255),
            new(255, 215, 180 ,255),
            new(0, 0, 128      ,255),
            new(128, 128, 128 ,255),
            new(255, 255, 255 ,255)
        ]);

        public static readonly List<Color32> MSMetroUI = new([
            new(51,153,51,255),
            new(162, 0, 255, 255),
            new(27, 161, 226, 255),
            new(140, 191, 38, 255),
            new(229, 20, 0, 255),
            new(255, 0, 151, 255),
            new(230, 113, 184, 255),
            new(160, 80, 0, 255),
            new(0, 171, 169, 255),
            new(240, 150, 9, 255),
        ]);

        public static readonly List<Color32> MatColor100 = new([
    new(0xcf,0xd8,0xdc,255),        new(0xff,0xcd,0xd2,255),    new(0xf8,0xbb,0xd0,255),    new(0xe1,0xbe,0xe7,255),    new(0xd1,0xc4,0xe9,255),    new(0xc5,0xca,0xe9,255),    new(0xbb,0xde,0xfb,255),    new(0xb3,0xe5,0xfc,255),    new(0xb2,0xeb,0xf2,255),    new(0xb2,0xdf,0xdb,255),    new(0xc8,0xe6,0xc9,255),    new(0xdc,0xed,0xc8,255),    new(0xf0,0xf4,0xc3,255),    new(0xff,0xf9,0xc4,255),    new(0xff,0xec,0xb3,255),    new(0xff,0xe0,0xb2,255),    new(0xff,0xcc,0xbc,255),    new(0xd7,0xcc,0xc8,255),    new(0xf5,0xf5,0xf5,255),    new(0xcf,0xd8,0xdc,255),
      ]);
        public static readonly List<Color32> MatColor500 = new([
    new(0x60,0x7d,0x8b,255),         new(0xf4,0x43,0x36,255),   new(0xe9,0x1e,0x63,255),    new(0x9c,0x27,0xb0,255),    new(0x67,0x3a,0xb7,255),    new(0x3f,0x51,0xb5,255),    new(0x21,0x96,0xf3,255),    new(0x03,0xa9,0xf4,255),    new(0x00,0xbc,0xd4,255),    new(0x00,0x96,0x88,255),    new(0x4c,0xaf,0x50,255),    new(0x8b,0xc3,0x4a,255),    new(0xcd,0xdc,0x39,255),    new(0xff,0xeb,0x3b,255),    new(0xff,0xc1,0x07,255),    new(0xff,0x98,0x00,255),    new(0xff,0x57,0x22,255),    new(0x79,0x55,0x48,255),    new(0x9e,0x9e,0x9e,255),    new(0x60,0x7d,0x8b,255),
      ]);
        public static readonly List<Color32> MatColor900 = new([
    new(0x26,0x32,0x38,255),         new(0xb7,0x1c,0x1c,255),   new(0x88,0x0e,0x4f,255),    new(0x4a,0x14,0x8c,255),    new(0x31,0x1b,0x92,255),    new(0x1a,0x23,0x7e,255),    new(0x0d,0x47,0xa1,255),    new(0x01,0x57,0x9b,255),    new(0x00,0x60,0x64,255),    new(0x00,0x4d,0x40,255),    new(0x1b,0x5e,0x20,255),    new(0x33,0x69,0x1e,255),    new(0x82,0x77,0x17,255),    new(0xf5,0x7f,0x17,255),    new(0xff,0x6f,0x00,255),    new(0xe6,0x51,0x00,255),    new(0xbf,0x36,0x0c,255),    new(0x3e,0x27,0x23,255),    new(0x21,0x21,0x21,255),    new(0x26,0x32,0x38,255),
        ]);
        public static readonly List<Color32> MatColorA200 = new([
   new(0xff,0x6e,0x40,255),            new(0xff,0x52,0x52,255),  new(0xff,0x40,0x81,255),    new(0xe0,0x40,0xfb,255),    new(0x7c,0x4d,0xff,255),    new(0x53,0x6d,0xfe,255),    new(0x44,0x8a,0xff,255),    new(0x40,0xc4,0xff,255),    new(0x18,0xff,0xff,255),    new(0x64,0xff,0xda,255),    new(0x69,0xf0,0xae,255),    new(0xb2,0xff,0x59,255),    new(0xee,0xff,0x41,255),    new(0xff,0xff,0x00,255),    new(0xff,0xd7,0x40,255),    new(0xff,0xab,0x40,255),
       ]);
        public static readonly List<Color32> MatColorA400 = new([
   new(0xff,0x3d,0x00,255),           new(0xff,0x17,0x44,255),   new(0xf5,0x00,0x57,255),    new(0xd5,0x00,0xf9,255),    new(0x65,0x1f,0xff,255),    new(0x3d,0x5a,0xfe,255),    new(0x29,0x79,0xff,255),    new(0x00,0xb0,0xff,255),    new(0x00,0xe5,0xff,255),    new(0x1d,0xe9,0xb6,255),    new(0x00,0xe6,0x76,255),    new(0x76,0xff,0x03,255),    new(0xc6,0xff,0x00,255),    new(0xff,0xea,0x00,255),    new(0xff,0xc4,0x00,255),    new(0xff,0x91,0x00,255),
       ]);
        public static readonly List<Color32> MatColorA700 = new([
   new(0xdd,0x2c,0x00,255),            new(0xd5,0x00,0x00,255),  new(0xc5,0x11,0x62,255),    new(0xaa,0x00,0xff,255),    new(0x62,0x00,0xea,255),    new(0x30,0x4f,0xfe,255),    new(0x29,0x62,0xff,255),    new(0x00,0x91,0xea,255),    new(0x00,0xb8,0xd4,255),    new(0x00,0xbf,0xa5,255),    new(0x00,0xc8,0x53,255),    new(0x64,0xdd,0x17,255),    new(0xae,0xea,0x00,255),    new(0xff,0xd6,0x00,255),    new(0xff,0xab,0x00,255),    new(0xff,0x6d,0x00,255),
        ]);

        public static readonly List<Color32> CPTM_SP_2000 = new([
            new(93, 47, 145, 255),
            new(124,97,78,255),
            new(150, 154, 153, 255),
            new(77, 140, 211, 255),
            new(212, 184, 136, 255),
            new(222, 142, 5, 255),
            new(67, 39, 123, 255),
            new(154, 54, 124, 255),
            new(3, 170, 87, 255),
            new(14, 14, 14, 255),
        ]);

        public static readonly List<Color32> SP_BUS_2000 = new([
            new(200, 200, 200, 255),
            new(5,225,31,255),
            new(0, 77, 133, 255),
            new(255, 245, 0, 255),
            new(218, 37, 28, 255),
            new(0, 115, 100, 255),
            new(0, 114, 184, 255),
            new(159, 44, 41, 255),
            new(229, 119, 24, 255),
        ]);

        public static readonly TLMAutoColorPalette[] defaultPaletteArray = [
                    new("São Paulo 2035", SaoPaulo2035),
                    new("London 2016", London2016),
                    new("Rainbow", Rainbow),
                    new("Rainbow Short", RainbowShort),
                    new("World Metro Mix", WorldMix),
                    new("MS Metro UI", MSMetroUI),
                    new("Material Color (100)", MatColor100),
                    new("Material Color (500)", MatColor500),
                    new("Material Color (900)", MatColor900),
                    new("Material Color (A200)", MatColorA200),
                    new("Material Color (A400)", MatColorA400),
                    new("Material Color (A700)", MatColorA700),
                    new("São Paulo CPTM 2000", CPTM_SP_2000),
                    new("São Paulo Bus Area 2000", SP_BUS_2000),
                ];

        public static string[] PaletteList
        {
            get
            {
                LogUtils.DoLog("TLMAutoColorPalettes paletteList");
                if (m_palettes == null)
                {
                    Init();
                }
                return [.. new string[] { "<" + Locale.Get("TLM_RANDOM") + ">" }.Union(m_palettes.Keys).OrderBy(x => x)];
            }
        }

        public static string[] PaletteListForEditing
        {
            get
            {
                LogUtils.DoLog("TLMAutoColorPalettes paletteListForEditing");
                if (m_palettes == null)
                {
                    Init();
                }
                return [.. new string[] { "-" + Locale.Get("SELECT") + "-" }.Union(m_palettes.Keys.OrderBy(x => x))];
            }
        }

        private static void Init()
        {
            LogUtils.DoLog("TLMAutoColorPalettes init()");
            Reload();
        }

        public static void Reload()
        {
            m_palettes = [];
            Load();
        }

        private static Dictionary<string, string> GetPalettesAsDictionary()
        {
            Dictionary<string, string> result = [];
            foreach (var pal in m_palettes)
            {
                if (!result.ContainsKey(pal.Key))
                {
                    result[pal.Key] = pal.Value.ToFileContent();
                }
            }
            return result;
        }

        public static void SaveAll()
        {
            FileUtils.EnsureFolderCreation(TLMController.PalettesFolder);
            var filesToSave = GetPalettesAsDictionary();
            foreach (var file in filesToSave)
            {
                File.WriteAllText(TLMController.PalettesFolder + Path.DirectorySeparatorChar + file.Key + TLMAutoColorPalette.EXT_PALETTE, file.Value);
            }
        }

        public static void Save(string palette)
        {
            if (!palette.IsNullOrWhiteSpace() && m_palettes.ContainsKey(palette))
            {
                m_palettes[palette].Save();
            }
        }

        private static void Load()
        {
            m_palettes = [];
            FileUtils.EnsureFolderCreation(TLMController.PalettesFolder);
            foreach (var filename in Directory.GetFiles(TLMController.PalettesFolder, "*" + TLMAutoColorPalette.EXT_PALETTE).Select(x => x.Split(Path.DirectorySeparatorChar).Last()))
            {
                string fileContents = File.ReadAllText(TLMController.PalettesFolder + Path.DirectorySeparatorChar + filename, Encoding.UTF8);
                var name = filename.Substring(0, filename.Length - 4);
                m_palettes[name] = TLMAutoColorPalette.FromFileContent(name, [.. fileContents.Split(TLMAutoColorPalette.ENTRY_SEPARATOR).Select(x => x?.Trim()).Where(x => !string.IsNullOrEmpty(x))]);
                LogUtils.DoLog("LOADED PALETTE ({0}) QTT: {1}", filename, m_palettes[name].Count);
            }
        }


        public static Color32 GetColor(int number, string[] paletteOrderSearch, bool randomOnPaletteOverflow, bool avoidRandom = false)
        {
            foreach (var paletteName in paletteOrderSearch)
            {
                if (!paletteName.IsNullOrWhiteSpace() && m_palettes.ContainsKey(paletteName))
                {
                    TLMAutoColorPalette palette = m_palettes[paletteName];
                    if (!randomOnPaletteOverflow || number <= palette.Colors.Count)
                    {
                        return palette[number % palette.Count];
                    }
                }
            }
            return avoidRandom ? (Color32)Color.clear : gen.GetNext();
        }

        public static List<Color32> GetColors(string paletteName)
        {
            if (!paletteName.IsNullOrWhiteSpace() && m_palettes.ContainsKey(paletteName))
            {
                TLMAutoColorPalette palette = m_palettes[paletteName];
                return palette.Colors;
            }
            return null;
        }

        public static TLMAutoColorPalette GetPalette(string paletteName)
        {
            if (!paletteName.IsNullOrWhiteSpace() && m_palettes.ContainsKey(paletteName))
            {
                TLMAutoColorPalette palette = m_palettes[paletteName];
                return palette;
            }
            return null;
        }

        public static void AddPalette(string paletteName)
        {
            if (!paletteName.IsNullOrWhiteSpace() && !m_palettes.ContainsKey(paletteName))
            {
                m_palettes[paletteName] = new TLMAutoColorPalette(paletteName, [Color.white]);
            }
        }

    }

}

