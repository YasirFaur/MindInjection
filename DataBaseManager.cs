// Import system libraries for basic operations
using System;
// Import collection libraries to use Lists
using System.Collections.Generic;
// Import IO libraries to work with files and folders
using System.IO;
// Import Linq for data filtering and transformation
using System.Linq;
// Import Regex to clean text and remove bad characters
using System.Text.RegularExpressions;

// Define the project namespace
namespace injection
{
    // Define the class to manage database files
    public class DataBaseManager
    {
        // Create a private variable to store the folder path
        private readonly string _folderPath;

        // Constructor method to initialize the database setup
        public DataBaseManager()
        {
            // Get the application folder path and add the database directory name
            _folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database");
            // Create the database folder if it does not exist
            Directory.CreateDirectory(_folderPath);
        }

        // Method to save a text session into a file
        public void save_session(string input_text)
        {
            // Stop the method if the input text is empty
            if (string.IsNullOrWhiteSpace(input_text)) return;

            // Get only the first line of the text and trim spaces
            string first_line = input_text.Split('\n')[0].Trim();
            // If the first line is too long, cut it to 128 characters
            if (first_line.Length > 128) first_line = first_line.Substring(0, 32);

            // Replace forbidden filename characters with an underscore
            string safe_name = Regex.Replace(first_line, @"[\\/:*?""<>|]", "_").Trim();
            // Combine the folder path and the safe name with a .txt extension
            string file_path = Path.Combine(_folderPath, $"{safe_name}.txt");

            // Write and save the full text into the file
            File.WriteAllText(file_path, input_text);
        }

        // Method to read and load a saved session file
        public string load_session(string file_name)
        {
            // Combine the folder path with the requested file name
            string file_path = Path.Combine(_folderPath, file_name);
            // Return file text if it exists, otherwise return an empty string
            return File.Exists(file_path) ? File.ReadAllText(file_path) : string.Empty;
        }

        // Method to delete a session file from the disk
        public bool delete_session(string file_name)
        {
            // Combine the folder path with the requested file name
            string file_path = Path.Combine(_folderPath, file_name);
            // Return false if the file does not exist on the path
            if (!File.Exists(file_path)) return false;

            // Delete the file from the folder
            File.Delete(file_path);
            // Return true to show that the deletion was successful
            return true;
        }

        // Method to get a list of all saved text files
        public List<string> get_all_sessions()
        {
            // Search the folder for txt files, get their names, and return them as a list
            return Directory.GetFiles(_folderPath, "*.txt").Select(Path.GetFileName).ToList();
        }

        public void save_focus_wordLog(string word, double focusScore)
        {
            try
            {
                // Define a fixed file name for all logs
                string filePath = Path.Combine(_folderPath, "focus score.csv");

                // Get current date and time
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // Prepare the text line for the CSV file
                string logLine = $"{word},{timestamp},{focusScore:P0}{Environment.NewLine}";

                // If the file does not exist, create it with headers
                if (!File.Exists(filePath))
                {
                    File.WriteAllText(filePath, "Word,Timestamp,Focus Score" + Environment.NewLine);
                }

                // Add the new log line to the fixed file
                File.AppendAllText(filePath, logLine);
            }
            catch (Exception ex)
            {
                // Fail silently to keep the app running
            }
        }

        public void focus_score_chart(int[] data)
        {
            if (data == null || data.Length == 0) return;

            // Find max value to scale the chart dynamically, or set a fixed max (e.g., 100)
            int maxValue = 100;
            int step = 5; // The Y-axis steps (100, 95, 90... 0)

            // 1. Draw the chart from top to bottom
            for (int y = maxValue; y >= 0; y -= step)
            {
                // Print Y-axis labels formatted to 2 digits
                Console.Write($"{y:D3} ");

                // Print bars for each data point
                for (int x = 0; x < data.Length; x++)
                {
                    if (data[x] >= y && data[x] > 0)
                    {
                        // Set color ranges based on temperature values
                        if (data[x] >= 80) Console.ForegroundColor = ConsoleColor.Red;
                        else if (data[x] >= 60) Console.ForegroundColor = ConsoleColor.Yellow;
                        else Console.ForegroundColor = ConsoleColor.Green;

                        Console.Write("|"); // Draw the bar
                    }
                    else
                    {
                        Console.Write(" "); // Empty space
                    }
                }
                Console.ResetColor();
                Console.WriteLine();
            }

            // 2. Draw the X-axis line
            Console.Write("000 ");
            for (int x = 0; x < data.Length; x++)
            {
                Console.Write("...|");
            }
            Console.WriteLine(Environment.NewLine);
        }
    }

}