using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Enter the folder path:");
        string folderPath = Console.ReadLine();

        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine("The specified folder does not exist.");
            return;
        }

        string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScenarioFilesLog.txt");

        try
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, false))
            {
                foreach (var file in Directory.EnumerateFiles(folderPath, "*.scenario", SearchOption.AllDirectories))
                {
                    string relativePath = Path.GetRelativePath(folderPath, file);

                    Console.WriteLine(relativePath);
                    writer.WriteLine(relativePath);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
