using System;
using System.IO;

class AOBScanner
{
    static void Main(string[] args)
    {
        Console.WriteLine("Enter the folder path to scan:");
        string folderPath = Console.ReadLine();

        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine("The folder path does not exist. Exiting...");
            return;
        }

        string scriptsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts");
        Directory.CreateDirectory(scriptsFolder);

        foreach (var filePath in Directory.GetFiles(folderPath))
        {
            try
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);
                byte[] startPattern = { 0x74, 0x67, 0x64, 0x61 }; // "tgda"
                byte[] stopPattern = { 0x74, 0x67, 0x62, 0x6C }; // "tgbl"

                int startIndex = AOBSearch(fileBytes, startPattern);
                if (startIndex != -1)
                {
                    startIndex += 8;
                    int stopIndex = AOBSearch(fileBytes, stopPattern, startIndex);

                    if (stopIndex != -1)
                    {
                        int length = stopIndex - startIndex;
                        byte[] extractedBytes = new byte[length];
                        Array.Copy(fileBytes, startIndex, extractedBytes, 0, length);

                        byte[] skipPattern = { 0x00, 0x00, 0x00, 0x01, 0x00 };
                        if (MatchesPattern(extractedBytes, skipPattern))
                        {
                            Console.WriteLine($"Skipped file '{filePath}' as extracted bytes only contain the skip pattern.");
                            continue;
                        }

                        if (extractedBytes.Length > 5)
                        {
                            int prunedLength = extractedBytes.Length - 5;
                            byte[] prunedBytes = new byte[prunedLength];
                            Array.Copy(extractedBytes, 4, prunedBytes, 0, prunedLength);

                            string scriptFileName = Path.Combine(scriptsFolder, Path.GetFileNameWithoutExtension(filePath) + ".hsc");
                            File.WriteAllBytes(scriptFileName, prunedBytes);

                            Console.WriteLine($"Extracted and pruned data saved to: {scriptFileName}");
                        }
                        else
                        {
                            Console.WriteLine($"Skipped file '{filePath}' as extracted bytes are too short after pruning.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file '{filePath}': {ex.Message}");
            }
        }

        Console.WriteLine("Processing completed. Press any key to exit.");
        Console.ReadKey();
    }

    static int AOBSearch(byte[] data, byte[] pattern, int startIndex = 0)
    {
        int patternLength = pattern.Length;
        for (int i = startIndex; i <= data.Length - patternLength; i++)
        {
            bool match = true;
            for (int j = 0; j < patternLength; j++)
            {
                if (data[i + j] != pattern[j])
                {
                    match = false;
                    break;
                }
            }
            if (match)
                return i;
        }
        return -1;
    }

    static bool MatchesPattern(byte[] data, byte[] pattern)
    {
        if (data.Length != pattern.Length)
            return false;

        for (int i = 0; i < pattern.Length; i++)
        {
            if (data[i] != pattern[i])
                return false;
        }
        return true;
    }
}
