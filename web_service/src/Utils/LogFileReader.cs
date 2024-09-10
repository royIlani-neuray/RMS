/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Text;

namespace WebService.Utils;

/// <summary>
/// A class used for reading part of log files 
/// </summary>
public class LogFileReader
{
    /// <summary>
    /// Reads the last n lines of a file.
    /// </summary>
    /// <param name="filePath">The path of the log file.</param>
    /// <param name="lineCount">The number of lines to read from the end of the file.</param>
    /// <returns>A string containing the last n lines.</returns>
    public static string ReadLastLines(string filePath, int lineCount)
    {
        var lines = new List<string>(); // Store lines in a list for correct order
        const int bufferSize = 1024; // Read in chunks of 1KB
        char[] buffer = new char[bufferSize];
        StringBuilder currentLine = new StringBuilder();

        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var streamReader = new StreamReader(fileStream))
        {
            long filePosition = fileStream.Length;
            
            while (filePosition > 0 && lines.Count < lineCount)
            {
                // Calculate how much to read and seek back
                int bytesToRead = (int)Math.Min(bufferSize, filePosition);
                filePosition -= bytesToRead;
                fileStream.Seek(filePosition, SeekOrigin.Begin);

                // Read the buffer
                buffer = new char[bufferSize]; // Reset the buffer
                streamReader.DiscardBufferedData(); // Reset the StreamReader's internal buffer
                streamReader.BaseStream.Seek(filePosition, SeekOrigin.Begin);
                int bufferLength = streamReader.Read(buffer, 0, bytesToRead);

                // Process the buffer in reverse
                for (int i = bufferLength - 1; i >= 0; i--)
                {
                    if (buffer[i] == '\n')
                    {
                        if (currentLine.Length > 0)
                        {
                            lines.Insert(0, currentLine.ToString());
                            currentLine.Clear();
                        }
                    }
                    else
                    {
                        // Prepend the character to the current line
                        currentLine.Insert(0, buffer[i]);
                    }

                    // Stop if we have enough lines
                    if (lines.Count == lineCount)
                    {
                        break;
                    }
                }
            }

            // Add the last line if it exists and hasn't been added yet
            if (currentLine.Length > 0 && lines.Count < lineCount)
            {
                lines.Insert(0, currentLine.ToString());
            }
        }

        return string.Join("\n", lines);
    }


}