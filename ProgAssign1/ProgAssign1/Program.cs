using System;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using System.Collections.Generic;
using System.Diagnostics;

namespace Assignment1
{
    public class Program
    {
        private int skippedRowsCount = 0;
        private int validRowsCount = 0;

        public void WalkAndProcess(string path, string outputFilePath)
        {
            try
            {
                string[] directories = Directory.GetDirectories(path);

                if (directories == null) return;

                // Traverse directories
                foreach (string dirpath in directories)
                {
                    WalkAndProcess(dirpath, outputFilePath);
                    Console.WriteLine("Dir: " + dirpath);
                }

                // Process files
                string[] fileList = Directory.GetFiles(path, "CustomerData*.csv");
                foreach (string filepath in fileList)
                {
                    Console.WriteLine("Processing File: " + filepath);
                    ProcessCSV(filepath, outputFilePath, path);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("Error: Access to directory denied - " + e.Message);
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine("Error: Directory not found - " + e.Message);
            }
            catch (IOException e)
            {
                Console.WriteLine("I/O Error: " + e.Message);
            }
        }

        private void ProcessCSV(string filePath, string outputFilePath, string directoryPath)
        {
            string date = ExtractDateFromPath(directoryPath); // Extract yyyy/mm/dd from directory path

            try
            {
                using (TextFieldParser parser = new TextFieldParser(filePath))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    bool headerSkipped = false;
                    List<string> validRows = new List<string>();

                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();

                        // Skip the header
                        if (!headerSkipped)
                        {
                            headerSkipped = true;
                            continue;
                        }

                        // Check if the row has any missing fields (incomplete row)
                        if (fields.Length < 10 || Array.Exists(fields, string.IsNullOrWhiteSpace))
                        {
                            skippedRowsCount++;
                            continue;
                        }

                        // Append the extracted date (yyyy/mm/dd) as an additional column
                        string validRow = string.Join(",", fields) + "," + date;
                        validRows.Add(validRow);
                        validRowsCount++;
                    }

                    // Write valid rows to output CSV file
                    WriteToOutputCSV(outputFilePath, validRows);
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("Error: File not found - " + e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("Error: Access denied - " + e.Message);
            }
            catch (IOException e)
            {
                Console.WriteLine("I/O Error: " + e.Message);
            }
        }

        private void WriteToOutputCSV(string outputFilePath, List<string> validRows)
        {
            try
            {
                // Append valid rows to Output.csv
                using (StreamWriter sw = new StreamWriter(outputFilePath, append: true))
                {
                    foreach (var row in validRows)
                    {
                        sw.WriteLine(row);
                    }
                }
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("Error: Access denied when writing to output file - " + e.Message);
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine("Error: Output directory not found - " + e.Message);
            }
            catch (IOException e)
            {
                Console.WriteLine("I/O Error while writing to output file: " + e.Message);
            }
        }

        private string ExtractDateFromPath(string path)
        {
            try
            {
                // Assuming directory path format is <year>/<month>/<date>
                string[] splitPath = path.Split(Path.DirectorySeparatorChar);
                string year = splitPath[splitPath.Length - 3];
                string month = splitPath[splitPath.Length - 2];
                string day = splitPath[splitPath.Length - 1];

                return $"{year}/{month}/{day}";
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine("Error extracting date from path - " + e.Message);
                return "Unknown";
            }
        }

        private void LogSummary(string logFilePath, TimeSpan executionTime)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(logFilePath, append: true))
                {
                    sw.WriteLine("Log Entry: " + DateTime.Now);
                    sw.WriteLine("Total Execution Time: " + executionTime.TotalSeconds + " seconds");
                    sw.WriteLine("Total Valid Rows: " + validRowsCount);
                    sw.WriteLine("Total Skipped Rows: " + skippedRowsCount);
                    sw.WriteLine("--------------------------------------------");
                }
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("Error: Access denied when writing to log file - " + e.Message);
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine("Error: Logs directory not found - " + e.Message);
            }
            catch (IOException e)
            {
                Console.WriteLine("I/O Error while writing to log file: " + e.Message);
            }
        }

        public static void Main(string[] args)
        {
            // Start timer for execution time
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Program walker = new Program();
            string rootDirectory = @"C:\Users\Vishnu Teja\source\repos\dotNET Assignment 1\dotNET Assignment 1\Sample Data\";
            string outputFilePath = @"C:\Users\Vishnu Teja\source\repos\dotNetProject\dotNetProject\Output\Output.csv";
            string logFilePath = @"C:\Users\Vishnu Teja\source\repos\dotNetProject\dotNetProject\Log\Log.txt";

            try
            {
                // Initialize output file with header
                using (StreamWriter sw = new StreamWriter(outputFilePath))
                {
                    sw.WriteLine("FirstName,LastName,StreetNumber,Street,City,Province,PostalCode,Country,PhoneNumber,EmailAddress,Date");
                }

                walker.WalkAndProcess(rootDirectory, outputFilePath);

                // Stop the timer and log the total execution time
                stopwatch.Stop();
                walker.LogSummary(logFilePath, stopwatch.Elapsed);

                Console.WriteLine("Process completed.");
                Console.WriteLine("Total skipped rows: " + walker.skippedRowsCount);
                Console.WriteLine("Total valid rows: " + walker.validRowsCount);


            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("Error: Access denied - " + e.Message);
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine("Error: Directory not found - " + e.Message);
            }
            catch (IOException e)
            {
                Console.WriteLine("I/O Error: " + e.Message);
            }
        }
    }
}