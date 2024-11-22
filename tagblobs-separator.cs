using System.Text;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Please provide the folder path as an argument.");
            return;
        }

        string folderPath = args[0];
        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine("The provided folder path does not exist.");
            return;
        }

        string[] files = Directory.GetFiles(folderPath, "*");
        string programRoot = AppDomain.CurrentDomain.BaseDirectory;
        string outputFolder = Path.Combine(programRoot, "ExtractedFiles");

        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        byte[] pattern = { 0x42, 0x4C, 0x41, 0x4D, 0x74, 0x61, 0x67, 0x21 };
        int patternLength = pattern.Length;
        int fileCounter = 0;
        const int bytesToDisregard = 0x3C;
        const int zeroBytesThreshold = 0xFFFF;

        foreach (string filePath in files)
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);

            for (int i = 0; i < fileBytes.Length - patternLength; i++)
            {
                if (IsPatternMatch(fileBytes, i, pattern))
                {
                    int extensionStartIndex = i - 12;
                    if (extensionStartIndex + 4 > fileBytes.Length)
                        break;

                    string fileExtension = Encoding.ASCII.GetString(fileBytes, extensionStartIndex, 4).Trim('\0');

                    fileExtension = MakeValidFileName(fileExtension);

                    int dataStartIndex = i - 0x30;
                    if (dataStartIndex < 0)
                        dataStartIndex = 0;

                    int nextPatternIndex = FindNextPatternIndex(fileBytes, i + patternLength, pattern);
                    if (nextPatternIndex == -1)
                        nextPatternIndex = fileBytes.Length;

                    int dataLength = nextPatternIndex - dataStartIndex;

                    if (dataLength > bytesToDisregard)
                    {
                        dataLength -= bytesToDisregard;
                    }
                    else
                    {
                        continue;
                    }

                    int zeroBytesCount = CountTrailingZeroBytes(fileBytes, dataStartIndex, dataLength);
                    if (zeroBytesCount >= zeroBytesThreshold)
                    {
                        dataLength -= zeroBytesCount;
                    }

                    byte[] dataToWrite = new byte[dataLength];
                    Array.Copy(fileBytes, dataStartIndex, dataToWrite, 0, dataLength);

                    string outputFileName = Path.Combine(outputFolder, $"{fileCounter}.{fileExtension}");
                    File.WriteAllBytes(outputFileName, dataToWrite);

                    Console.WriteLine($"Original File: {Path.GetFileName(filePath)}, Extracted File Number: {fileCounter}, Extension: {fileExtension}");

                    fileCounter++;
                    i = nextPatternIndex - 1;
                }
            }
        }
    }

    static bool IsPatternMatch(byte[] fileBytes, int index, byte[] pattern)
    {
        for (int i = 0; i < pattern.Length; i++)
        {
            if (fileBytes[index + i] != pattern[i])
                return false;
        }
        return true;
    }

    static int FindNextPatternIndex(byte[] fileBytes, int startIndex, byte[] pattern)
    {
        for (int i = startIndex; i < fileBytes.Length - pattern.Length; i++)
        {
            if (IsPatternMatch(fileBytes, i, pattern))
                return i;
        }
        return -1;
    }

    static int CountTrailingZeroBytes(byte[] fileBytes, int startIndex, int dataLength)
    {
        int zeroBytesCount = 0;
        for (int i = dataLength - 1; i >= 0; i--)
        {
            if (fileBytes[startIndex + i] == 0x00)
                zeroBytesCount++;
            else
                break;
        }
        return zeroBytesCount;
    }

    static string MakeValidFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        foreach (var invalidChar in invalidChars)
        {
            fileName = fileName.Replace(invalidChar, '_');
        }
        return string.IsNullOrWhiteSpace(fileName) ? "default" : fileName;
    }
}
