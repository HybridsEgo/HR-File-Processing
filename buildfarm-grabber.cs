using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Enter the directory path:");
        string directoryPath = Console.ReadLine();

        const string outputFileName = "output.txt";
        const string fileExtension = ".scenario_lightmap_bsp_data";

        if (!Directory.Exists(directoryPath))
        {
            Console.WriteLine("The directory does not exist.");
            return;
        }

        byte[] aobPattern = Encoding.ASCII.GetBytes("BNGFARM");

        string[] files = Directory.GetFiles(directoryPath, $"*{fileExtension}", SearchOption.AllDirectories);
        int totalFiles = files.Length;

        if (totalFiles == 0)
        {
            Console.WriteLine($"No files found with the '{fileExtension}' extension in the directory.");
            return;
        }

        int processedCount = 0;
        int matchCount = 0;
        int totalOccurrences = 0;

        using (StreamWriter writer = new StreamWriter(outputFileName, append: true))
        {
            foreach (string file in files)
            {
                processedCount++;
                string relativePath = Path.GetRelativePath(directoryPath, file);

                try
                {
                    byte[] fileBytes = File.ReadAllBytes(file);

                    int[] occurrences = FindOccurrences(fileBytes, aobPattern);
                    if (occurrences.Length > 0)
                    {
                        matchCount++;
                        totalOccurrences += occurrences.Length;

                        int occurrenceNumber = 1;
                        foreach (int offset in occurrences)
                        {
                            int start = Math.Max(0, offset - 0x20);
                            int length = Math.Min(0x80, fileBytes.Length - start);
                            byte[] snippet = new byte[length];
                            Array.Copy(fileBytes, start, snippet, 0, length);

                            string snippetString = BitConverter.ToString(snippet).Replace("-", " ");

                            snippetString = CullZeroGroups(snippetString);

                            string readableString = ConvertBytesToString(snippet);

                            writer.WriteLine($"{relativePath} : Occurrence #{occurrenceNumber} : {readableString}");
                            occurrenceNumber++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading file {relativePath}: {ex.Message}");
                }


                double percentComplete = (double)processedCount / totalFiles * 100;
                Console.WriteLine($"{percentComplete:F1}% : {processedCount}/{totalFiles} : Files Found: {matchCount} : Total Occurrences: {totalOccurrences} : {relativePath}");
            }
        }

        Console.WriteLine($"Search completed. Results saved to {outputFileName}");
    }

    static int[] FindOccurrences(byte[] fileBytes, byte[] pattern)
    {
        var offsets = new System.Collections.Generic.List<int>();
        for (int i = 0; i <= fileBytes.Length - pattern.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < pattern.Length; j++)
            {
                if (fileBytes[i + j] != pattern[j])
                {
                    match = false;
                    break;
                }
            }

            if (match)
            {
                offsets.Add(i);
            }
        }

        return offsets.ToArray();
    }

    static string CullZeroGroups(string input)
    {
        string pattern = @"(?:00\s?){4,}";
        return Regex.Replace(input, pattern, "");
    }

    static string ConvertBytesToString(byte[] bytes)
    {
        StringBuilder sb = new StringBuilder();
        foreach (byte b in bytes)
        {
            if (b >= 32 && b <= 126)
            {
                sb.Append((char)b);
            }
            else
            {
                sb.Append(" ");
            }
        }

        string result = sb.ToString().Trim();

        result = Regex.Replace(result, @"\s+", " ");

        return result;
    }

}
