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
            if (string.IsNullOrWhiteSpace(input_text)) return;

            string first_line = input_text.Split('\n')[0].Trim();
            if (first_line.Length > 128) first_line = first_line.Substring(0, 32);

            string safe_name = Regex.Replace(first_line, @"[\\/:*?""<>|]", "_").Trim();
            string file_path = Path.Combine(_folderPath, $"{safe_name}.txt");

            // Check if a file with the same name already exists
            if (File.Exists(file_path))
            {
                // Append a unique timestamp to prevent overwriting
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                file_path = Path.Combine(_folderPath, $"{safe_name}_{timestamp}.txt");
            }

            File.WriteAllText(file_path, input_text);

            schedule_file_review(safe_name);
        }

        public void schedule_file_review(string file_name)
        {
            if (string.IsNullOrWhiteSpace(file_name)) return;

            string schedule_file = Path.Combine(_folderPath, "review_schedule.csv");
            DateTime now = DateTime.Now;

            // Generate the 3 spaced repetition rows (24h, 7d, 30d)
            string[] review_dates = new string[]
            {
                $"{file_name},{now.AddDays(1):yyyy-MM-dd}",
                $"{file_name},{now.AddDays(7):yyyy-MM-dd}",
                $"{file_name},{now.AddDays(30):yyyy-MM-dd}"
            };

            // Append safely to the schedule file
            File.AppendAllLines(schedule_file, review_dates);
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

                // Multiplies the score by 100 and converts it to a whole number (e.g., 89)
                string logLine = $"{word},{timestamp},{(int)(focusScore * 100)}{Environment.NewLine}";

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

            // Limit the chart width to the latest N elements to prevent text wrapping
            const int MaxVisiblePoints = 64;
            if (data.Length > MaxVisiblePoints)
            {
                // Skip the old points and take only the latest ones
                data = data.Skip(data.Length - MaxVisiblePoints).ToArray();
            }

            int maxValue = 100;
            int step = 5;

            // 1. Draw the chart from top to bottom
            for (int y = maxValue; y >= 0; y -= step)
            {
                Console.Write($"{y:D3} ");

                for (int x = 0; x < data.Length; x++)
                {
                    if (data[x] >= y && data[x] > 0)
                    {
                        if (data[x] >= 90) Console.ForegroundColor = ConsoleColor.Green;
                        else if (data[x] >= 60) Console.ForegroundColor = ConsoleColor.Yellow;
                        else Console.ForegroundColor = ConsoleColor.Red;

                        Console.Write("|");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write(" ");
                    }
                }
                Console.WriteLine();
            }

            // 2. Draw the X-axis line            
            Console.Write("    ");
            for (int x = 0; x < data.Length; x++)
            {
                Console.Write(x % 5 == 0 ? "+" : ".");
            }
            Console.WriteLine(Environment.NewLine);
        }

        public int[] read_focus_scores()
        {
            var scoresList = new List<int>();
            string filePath = Path.Combine(_folderPath, "focus score.csv");

            if (!File.Exists(filePath)) return scoresList.ToArray();

            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    // Skip the header line (Word,Timestamp,Focus Score)
                    reader.ReadLine();

                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // Split by comma and get the 3rd column (index 2)
                        string[] columns = line.Split(',');
                        if (columns.Length >= 3 && int.TryParse(columns[2], out int score))
                        {
                            scoresList.Add(score);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Fail silently
            }

            return scoresList.ToArray();
        }

        public string analyze_focus_trend(int[] scores)
        {
            if (scores == null || scores.Length < 2)
                return "Not Enough Data";

            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
            int n = scores.Length;

            for (int i = 0; i < n; i++)
            {
                sumX += i;            // Sequential time index
                sumY += scores[i];     // Focus score value
                sumXY += i * scores[i];
                sumX2 += i * i;
            }

            // Calculate the Slope
            double slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);

            // Return the trend result as text
            if (slope > 0.1) return "\t[ Your focus level is going up. ]";
            if (slope < -0.1) return "\t[ Your focus level is going down! ]";
            return "\t[ Your focus level is stable. ]";
        }

        public double calculate_standard_deviation(int[] scores)
        {
            if (scores == null || scores.Length < 2) return 0.0;

            double average = scores.Average();

            // Sum of squared differences
            double sumOfSquares = scores.Sum(score => Math.Pow(score - average, 2));

            // Variance (for sample: n - 1)
            double variance = sumOfSquares / (scores.Length - 1);

            // Standard Deviation is the square root of variance
            return Math.Sqrt(variance);
        }

        public string get_focus_behavior_analysis(int[] scores)
        {
            if (scores == null || scores.Length < 2) return "Not enough data.";

            // 1. Calculate Slope (Linear Regression)
            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
            int n = scores.Length;
            for (int i = 0; i < n; i++)
            {
                sumX += i; sumY += scores[i];
                sumXY += i * scores[i]; sumX2 += i * i;
            }
            double slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);

            // 2. Calculate Standard Deviation
            double avg = scores.Average();
            double sumSq = scores.Sum(s => Math.Pow(s - avg, 2));
            double stdDev = Math.Sqrt(sumSq / (n - 1));

            // 3. Smart Fusion Logic (Slope + StdDev)
            // Case A: Improving Trend
            if (slope > 0.1)
            {
                if (stdDev <= 16.0)
                    return "\tYour focus is improving with a steady, strong rhythm. Keep going!";
                else
                    return "\tYour focus is going up, but with heavy jumps. Try to stay calm.";
            }
            // Case B: Decreasing Trend
            if (slope < -0.1)
            {
                if (stdDev <= 16.0)
                    return "\tYour focus is going down smoothly. You might need a good rest.";
                else
                    return "\tYour focus is dropping with sharp crashes. Fix your environment.";
            }
            // Case C: Stable Trend
            if (stdDev <= 9.0)
                return "\tYour focus is stable and highly controlled. Excellent consistency!";

            return "\tYour focus has normal daily shifts, but your overall level is safe.";
        }
    }

}