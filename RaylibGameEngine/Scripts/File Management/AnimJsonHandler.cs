using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Numerics;
using System.IO;
using static Levels.FileManager;

namespace AnimJson
{
    public static class AnimJsonManager
    {
        public static void SaveAnim(Animation anim, string fileName)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(anim, options);
            File.WriteAllText(assetsDir + fileName, jsonString);
            Console.WriteLine(File.ReadAllText(assetsDir + fileName));
        }
    }

    public class Animation
    {
        public int[] frames { get; set; }
    }
}