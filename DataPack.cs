using System.IO.Compression;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace ConsoleCS
{
    // this datapack file structure based on "Random SkyBlock Island" by @Awhikax
    // https://www.planetminecraft.com/project/random-skyblock-island/
    public class DataPack
    {
        // minecraft assets directory
        // under ".minecraft" folder
        const string ASSETS_PATH = @"D:\My_Software\PCL2\.minecraft\assets\";
        const string INDEXES_PATH = ASSETS_PATH + @"indexes\";
        const string OBJECTS_PATH = ASSETS_PATH + @"objects\";

        const string PACKNAME = "Random Skyblock Island";
        const string NAMESPACE = "rd_skyblock_is";
        string root = Environment.CurrentDirectory;

        // a temp folder for language files
        const string TEMP_PATH = @"D:\Documents\Visual Studio Code\mcmod\datapack\temp\";
        const string LANG_PATH = TEMP_PATH + @"lang\";

        List<string> cmds = new List<string>();
        int nBlocks = 0;

        public void Run()
        {
            CreateDatapack();
        }

        // get all language files from all installed minecraft versions
        // >>> Powershell verion : lang.ps1
        public void GetLangFile()
        {
            var indices = Directory.GetFiles(INDEXES_PATH);
            foreach (var item in indices)
            {
                var jstr = File.ReadAllText(item);
                var jdata = JsonNode.Parse(jstr)!;
                var jd1 = jdata["objects"]!;
                var jdl = jd1["minecraft/lang/zh_cn.lang"] is null ? jd1["minecraft/lang/zh_cn.json"]! : jd1["minecraft/lang/zh_cn.lang"]!;
                var hash = jdl["hash"]!.GetValue<string>();
                var sfn = OBJECTS_PATH + hash[..2] + @"\" + hash;
                var dfn = LANG_PATH + "lang_" + item.Split('\\')[^1];
                File.Copy(sfn, dfn, true);
            }
        }

        // translate charactors if it's unicode code
        public void TranslateUnicode()
        {
            var langs = Directory.GetFiles(LANG_PATH);
            foreach (var item in langs)
            {
                var con = File.ReadAllText(item);
                var res = Regex.Unescape(con);
                File.WriteAllText(item, res);
            }
        }

        // only keep block keys in language file
        // >>> Powershell version : simplang.ps1
        // >>> also include unicode translate
        public void Simplify()
        {
            // selected language file
            const string FILE = @"D:\Documents\Visual Studio Code\mcmod\datapack\temp\lang_17.json";

            var con = File.ReadAllLines(FILE).ToList();
            foreach (var line in con)
            {
                if (!(line.Contains("block.minecraft.") &&
                    !line.Contains("block.minecraft.banner")))
                    con.Remove(line); // only keep block keys
            }
            File.WriteAllLines(FILE, con); // write back to file

            //File.WriteAllLines(
            //    FILE,
            //    File.ReadAllLines(FILE)
            //        .Where(s =>
            //        s.Contains("block.minecraft.") &&
            //        !s.Contains("block.minecraft.banner")
            //    ));
        }

        // create fill functions for every block id
        public void Cmdtxt()
        {
            // target language file that only block keys left
            // ==> use Simplify() to clearup the file
            const string FILE = @"D:\Documents\Visual Studio Code\mcmod\datapack\temp\lang_17.json";

            // minecraft command
            const string CMD1 = @"execute if score $Random rd_skyblock_is matches ";
            // your island position and size
            const string CMD2 = " run fill 1 66 1 -1 64 -1 ";
            const string CMD3 = " run fill -1 64 -2 4 66 -4 ";

            var lines = File.ReadAllLines(FILE);
            var res = new List<string>();
            var i = 0;
            foreach (var line in lines)
            {
                var ks = line.Split(':')[0].Split('.');
                if (ks.Length == 3)
                {
                    if (ks[2] == "grass\"") continue; // 392
                    if (ks[2] == "ominous_banner\"") continue; // 613
                    if (ks[2] == "set_spawn\"") continue; // 821
                    var id = ks[1] + ":" + ks[2].TrimEnd('"');
                    res.Add(CMD1 + i + CMD2 + id);
                    res.Add(CMD1 + i + CMD3 + id);
                    i++;
                }
            }
            nBlocks = i;
            cmds.AddRange(res);
            // File.WriteAllLines(FUNCTION_FILL, res);
        }

        // create datapack zip file
        // other function scripts is same in "Random SkyBlock Island" by @Awhikax
        // https://www.planetminecraft.com/project/random-skyblock-island/
        public void CreateDatapack()
        {
            Cmdtxt(); // call this once is enough
            string temp = Path.Combine(root, PACKNAME);
            const string VANILLA_FUNCTIONS = "data/minecraft/tags/function/";
            Directory.CreateDirectory(Path.Combine(temp, VANILLA_FUNCTIONS));
            File.WriteAllText(
                Path.Combine(temp, VANILLA_FUNCTIONS, "load.json"),
                "{\"values\":[\"rd_skyblock_is:setup\"]}");
            File.WriteAllText(
                Path.Combine(temp, VANILLA_FUNCTIONS, "tick.json"),
                "{\"values\":[\"rd_skyblock_is:main\"]}");

            const string USER_FUNCTIONS = "data/" + NAMESPACE + "/function/";
            Directory.CreateDirectory(Path.Combine(temp, USER_FUNCTIONS, "cmd"));
            Directory.CreateDirectory(Path.Combine(temp, USER_FUNCTIONS, "mechanics/loop"));

            File.WriteAllLines(
                Path.Combine(temp, USER_FUNCTIONS, "main.mcfunction"),
                ["execute if score $Game rd_skyblock_is matches 1 run function rd_skyblock_is:mechanics/loop/tick"]);
            File.WriteAllLines(
                Path.Combine(temp, USER_FUNCTIONS, "setup.mcfunction"),
                [
                    "scoreboard objectives add rd_skyblock_is dummy",
                    $"scoreboard players set $Blocks rd_skyblock_is {nBlocks}",
                    "scoreboard players remove $Game rd_skyblock_is 0",
                    "scoreboard players remove $Delay rd_skyblock_is 0",
                    "execute if score $Delay rd_skyblock_is matches 0 run scoreboard players set $Delay rd_skyblock_is 180",
                    "bossbar add rd_skyblock_is {\"text\":\"\"}",
                    "bossbar set rd_skyblock_is color blue",
                    "bossbar set rd_skyblock_is style progress",
                    "execute store result bossbar rd_skyblock_is max run scoreboard players get $Delay rd_skyblock_is",
                    "execute store result bossbar rd_skyblock_is value run scoreboard players get $Second rd_skyblock_is",
                ]);

            File.WriteAllLines(
                Path.Combine(temp, USER_FUNCTIONS, "cmd/start.mcfunction"),
                [
                    "scoreboard players set $Game rd_skyblock_is 1",
                    "scoreboard players operation $Second rd_skyblock_is = $Delay rd_skyblock_is",
                    "execute store result bossbar rd_skyblock_is max run scoreboard players get $Delay rd_skyblock_is",
                    "execute store result bossbar rd_skyblock_is value run scoreboard players get $Second rd_skyblock_is",
                    "bossbar set rd_skyblock_is name [\"\",{\"text\":\"Blocks change in \",\"color\":\"gray\"},{\"score\":{\"name\":\"$Second\",\"objective\":\"rd_skyblock_is\"},\"color\":\"gold\"},{\"text\":\"s\",\"color\":\"red\"}]",
                    "bossbar set rd_skyblock_is players @a",
                ]);
            File.WriteAllLines(
                Path.Combine(temp, USER_FUNCTIONS, "cmd/stop.mcfunction"),
                [
                    "scoreboard players set $Game rd_skyblock_is 0",
                    "scoreboard players reset $Second rd_skyblock_is",
                    "bossbar set rd_skyblock_is players",
                ]);

            File.WriteAllLines(
                Path.Combine(temp, USER_FUNCTIONS, "mechanics/change_blocks.mcfunction"),
                cmds);
            File.WriteAllLines(
                Path.Combine(temp, USER_FUNCTIONS, "mechanics/change_island.mcfunction"),
                [
                    "scoreboard players operation $Second rd_skyblock_is = $Delay rd_skyblock_is",
                    "function rd_skyblock_is:mechanics/get_random",
                    "scoreboard players operation $Random rd_skyblock_is %= $Blocks rd_skyblock_is",
                    "function rd_skyblock_is:mechanics/change_blocks",
                ]);
            File.WriteAllLines(
                Path.Combine(temp, USER_FUNCTIONS, "mechanics/get_random.mcfunction"),
                [
                    "summon area_effect_cloud ~ ~ ~ {Tags:[\"rd_skyblock_is\"]}",
                    "execute store result score $Random rd_skyblock_is run data get entity @e[type=area_effect_cloud,tag=rd_skyblock_is,limit=1] UUID[0]",
                    "kill @e[type=area_effect_cloud,tag=rd_skyblock_is]",
                ]);

            File.WriteAllLines(
                Path.Combine(temp, USER_FUNCTIONS, "mechanics/loop/second.mcfunction"),
                [
                    "scoreboard players reset $Tick rd_skyblock_is",
                    "scoreboard players remove $Second rd_skyblock_is 1",
                    "execute store result bossbar rd_skyblock_is value run scoreboard players get $Second rd_skyblock_is",
                    "bossbar set rd_skyblock_is name [\"\",{\"text\":\"Blocks change in \",\"color\":\"gray\"},{\"score\":{\"name\":\"$Second\",\"objective\":\"rd_skyblock_is\"},\"color\":\"gold\"},{\"text\":\"s\",\"color\":\"red\"}]",
                    "execute if score $Second rd_skyblock_is matches ..0 run function rd_skyblock_is:mechanics/change_island",
                ]);
            File.WriteAllLines(
                Path.Combine(temp, USER_FUNCTIONS, "mechanics/loop/tick.mcfunction"),
                [
                    "scoreboard players add $Tick rd_skyblock_is 1",
                    "execute if score $Tick rd_skyblock_is matches 20.. run function rd_skyblock_is:mechanics/loop/second",
                ]);

            File.WriteAllText(
                Path.Combine(temp, "pack.mcmeta"),
                "{\"pack\":{\"pack_format\":48,\"description\":\"§6Random SkyBlock Island\\n§dBy §5Awhikax\\n§8Version: §7R1.16.5-2§r\"}}"
                );

            ZipFile.CreateFromDirectory(temp, Path.Combine(root, PACKNAME + ".zip"));
            Console.WriteLine("Conpressed Datapack file : " + Path.Combine(root, PACKNAME + ".zip"));
            Console.WriteLine("Unconpressed folder : " + root);
        }
    }
}
