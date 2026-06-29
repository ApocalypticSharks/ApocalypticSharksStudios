using System;
using System.IO;

public static class ProcessArmSegment
{
    public static void Main(string[] args)
    {
        string root = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".."));
        string path = args.Length > 0
            ? args[0]
            : Path.Combine(root, "Assets", "Sprites", "Player", "medic_arm_full.png");

        if (!File.Exists(path))
        {
            Console.WriteLine("Missing: " + path);
            return;
        }

        PlayerSpritePipeline.ProcessSegment(path, path);
        Console.WriteLine("Processed: " + path);
    }
}
