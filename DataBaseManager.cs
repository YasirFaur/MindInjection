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
                // Get the first line of the input text
                string firstLine = TextManager.input_text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)[0];

                // Use the first 32 characters as the file name
                string safeFileName = string.Concat(firstLine.Take(32)).Trim();
                string filePath = $"{_folderPath}/{safeFileName}.csv";

                // Get current date and time
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // Prepare the text line for the CSV file
                string logLine = $"{word},{timestamp},{focusScore:P0}{Environment.NewLine}";

                // If the file does not exist, create it with headers
                if (!File.Exists(filePath))
                {
                    File.WriteAllText(filePath, "Word,Timestamp,Focus Score" + Environment.NewLine);
                }

                // Add the new log line to the file
                File.AppendAllText(filePath, logLine);
            }
            catch (Exception ex)
            {
                // Fail silently to keep the app running
            }
        }
    }

}