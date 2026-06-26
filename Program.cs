using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using WenceyWang.FIGlet;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Xml.Schema;
using JetBrains.Annotations;
using System.Drawing.Text;

namespace injection
{
    internal class Program
    {
        private static TextManager manager = new TextManager();
        private static DataBaseManager dbm = new DataBaseManager();        
        static void Main(string[] args)
        {
            EnableAutoStart();

            TextManager.EnableAnsiColors();

            Console.Title = "MindInjection - Focus & Mastery System ( :!: DEMO :!: )";
            FocusMonitor.MaximizeAndLockConsole();
            FocusMonitor.StartMonitoring();

            Console.WriteLine(Environment.NewLine);
            manager.print_ascii_art("   MindInjection");
            

            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine(Environment.NewLine + "   Where Repetition Meets Focus, Mastery Becomes Inevitable.");
            
            Console.WriteLine("   Created by Yasir Faur." + Environment.NewLine);
            

            Console.ResetColor();

            int[] focus_scors = dbm.read_focus_scores();

            dbm.focus_score_chart(focus_scors);

            colored_trend(focus_scors);

            string advice = get_stability_feedback(dbm.calculate_standard_deviation(focus_scors));
            Console.WriteLine(advice);

            string behaviors_analysis = dbm.get_focus_behavior_analysis(focus_scors);
            Console.WriteLine(behaviors_analysis);

            manager.speak_text("Mind Injection");
            manager.speak_text("Where Repetition Meets Focus, Mastery Becomes Inevitable.");
            manager.speak_text("Created by Yasir Faur.");
            manager.speak_text(advice);
            manager.speak_text(behaviors_analysis);

            Console.Clear();

            main_menu();
        }

        private static void create()
        {
            while (true)
            {
                Console.Clear();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Type: 0 to move to Main, 2: View, 3: Update, 4: Delete, 5: Exit");
                Console.ResetColor();

                string message = "Please enter a summary:";
                Console.WriteLine(message);
                manager.speak_text(message);

                manager.read_from_console();
     
                if (!string.IsNullOrEmpty(TextManager.input_text) && TextManager.input_text.Trim().Length == 1)
                {
                    switch (TextManager.input_text.Trim())
                    {
                        case "0":                            
                            return; 
                        case "2":
                            view();
                            return;
                        case "3":
                            update();
                            return;
                        case "4":
                            delete();
                            return;
                        case "5":
                            Environment.Exit(0);
                            break;
                    }
                }

                // check if the text length is valid 
                if (manager.is_length_valid())
                {
                    inject(manager.split_text_into_sentences());                    
                }
                else
                {
                    if (TextManager.input_text.Length < 256)
                    {
                        Console.WriteLine("Error: The text is too short.");
                        Thread.Sleep(2000);

                    }
                    else if (TextManager.input_text.Length > 1024)
                    {
                        TextManager.input_text = manager.GetTheFirst1024Characters(TextManager.input_text);
                        inject(manager.split_text_into_sentences());

                    }                    
                }
            }
        }

        private static void colored_trend(int[] values)
        {
            string trend_message = dbm.analyze_focus_trend(values);
            //Check the message and apply the correct color
            if (trend_message.Contains("up"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else if (trend_message.Contains("down"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }

            // 3. Print the text and reset the color
            Console.WriteLine(trend_message);
            Console.ResetColor();

        }

        public static string get_stability_feedback(double stdDev)
        {
            // Excellent Stability (Under 9)
            if (stdDev < 9.0)
            {
                return "\tYour focus is very stable. Keep up this great, balanced routine!";
            }

            // Normal Waves (From 9 up to 16)
            if (stdDev <= 16.0)
            {
                return "\tYour focus has slight ups and downs. This is natural, just stay consistent.";
            }

            // High Variance (Above 16)
            return "\tYour focus jumps up and down heavily. Try to reduce distractions around you.";
        }

        private static void inject(List<string> splited_text_into_sentences)
        {
            List<string> cycles = manager.generate_diminishing_cycles(splited_text_into_sentences);
            // Store scores of each repetition in the current session
            List<int> session_scores = new List<int>();
            // Calculate the focus ratio based on the expected time and actual time
            double focus_ratio = 0.0;

            for (int i = 0; i < cycles.Count; i++)
            {
                Console.Clear();

                Stopwatch cycle_timer = Stopwatch.StartNew();

                string totale_injection_time = manager.estimated_time();
                string current_cycle_time = manager.calculate_current_cycle_time(cycles[i]);

                int total_length = TextManager.input_text.Replace("\r\n", "").Length;
                int current_lenght = cycles[i].Replace("\t", "").Replace("\r\n", "").Length;
                double percentage = total_length > 0 ? 1 - ((double)current_lenght / total_length) : 0;
                percentage *= 100;                

                Console.WriteLine(
                    Environment.NewLine +
                    "\t|| " + "Total Injection Time:   " + totale_injection_time +
                    " || " + "Current Cycle Time: " + current_cycle_time +
                    " || " + (int)percentage +
                    "% " + TextManager.GetProgressBar((int)percentage) +
                    " || " + Environment.NewLine + 
                    "\t|| " + "Remainig Injectin time: " + manager.calculate_remaining_time(cycles[i]) + 
                    " || " +  get_colored_focus_score((int)(focus_ratio * 100)) + Environment.NewLine);

                manager.print_cycle_with_highlight(cycles[i]);
                manager.speak_text(cycles[i]);

                string test_word = manager.longest_word.ToLower();

                while (true)
                {
                    Console.WriteLine(Environment.NewLine + "Type: ");
                    manager.print_ascii_art(test_word);
                    Console.WriteLine(Environment.NewLine);

                    TextManager.set_speaking_status(true);
                    manager.speak_text_async(manager.longest_word);

                    string user_input = Console.ReadLine()?.Trim().ToLower();
                    TextManager.set_speaking_status(false);

                    if (user_input == test_word)
                    {
                        break;
                    }
                    Console.Clear();
                }
                cycle_timer.Stop();
                // Calculate the expected typing time based on the length of the longest word
                double expected_typing_longest_word = manager.longest_word.Length * 0.45;
                // Get the elapsed time in seconds
                double actua_seconds = cycle_timer.Elapsed.TotalSeconds;
                // Calculate the expected time based on the length of the cycle
                double expected_seconds = cycles[i].Length * 0.076;
                // Calculate the focus ratio (expected time / actual time)
                focus_ratio = actua_seconds > 0 ? (expected_seconds + expected_typing_longest_word )/ actua_seconds : 0;                

                if (focus_ratio > 1.0) focus_ratio = 1.0; // Cap the focus ratio at 1.0 (100%)

                
                // Convert ratio to percentage and add to session list
                int current_score = (int)(focus_ratio * 100);
                session_scores.Add(current_score);

                dbm.save_focus_wordLog(test_word,focus_ratio);
            }

            // Convert list to array for statistical calculations
            int[] scores_array = session_scores.ToArray();

            if (scores_array.Length >= 2)
            {                               
                string focus_analyasis = dbm.analyze_focus_trend(scores_array);
                Console.WriteLine(focus_analyasis);
                string behavior_report = dbm.get_focus_behavior_analysis(scores_array);                
                Console.WriteLine(behavior_report);

                FocusMonitor.voice_notification.Speak(behavior_report);                
            }
            else
            {
                // Not enough reps to calculate statistics
                Thread.Sleep(2000);
            }            

            Console.Clear();
            Console.WriteLine("\n[Process Finished. Returning to Main Menu...]");
            Thread.Sleep(2000);
            return;
        }

        public static string get_colored_focus_score(int score)
        {
            // ANSI Color Codes
            string green = "\x1b[32m";
            string yellow = "\x1b[33m";
            string red = "\x1b[31m";
            string reset = "\x1b[0m";

            string chosenColor;

            // Determine color based on score range
            if (score >= 90) chosenColor = green;
            else if (score >= 50) chosenColor = yellow;
            else chosenColor = red;

            // Return the formatted colored string
            return $"Focus Score: {chosenColor}{score}%{reset}";
        }

        private static void view()
        {
            // Clear the screen to start with a clean page
            Console.Clear();

            // Show the title of this page to the user
            Console.WriteLine(Environment.NewLine + "--- Due Reviews (Today & Past) ---\n");

            // Find the main folder path where the database is located
            string _folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database");

            // Find the exact path of the text file that holds the schedule
            string schedule_file = Path.Combine(_folderPath, "review_schedule.csv");

            // If the schedule file does not exist, tell the user and stop
            if (!File.Exists(schedule_file))
            {
                Console.WriteLine("No review schedule file found.");
                Console.WriteLine("\nPress any key to return to Main Menu...");
                Console.ReadKey();
                return;
            }

            // Read all the text lines inside the schedule file
            string[] lines = File.ReadAllLines(schedule_file);

            // Get today's date in a clean year-month-day format
            string str_today = DateTime.Today.ToString("yyyy-MM-dd");

            // Create a dictionary list to connect numbers (1, 2, 3) to file names and dates
            Dictionary<int, Tuple<string, string>> file_mapping = new Dictionary<int, Tuple<string, string>>();

            // Start counting the menu numbers from 1
            int displayIndex = 1;

            // Look at each line in the file one by one
            for (int i = 0; i < lines.Length; i++)
            {
                // Skip the line if it is empty or has only spaces
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                // Split the line into two parts using the comma sign
                string[] parts = lines[i].Split(',');

                // Check if the line has exactly two valid parts (name and date)
                if (parts.Length == 2)
                {
                    // Get the file name and clean up any extra spaces
                    string fileName = parts[0].Trim();

                    // Get the schedule date and clean up any extra spaces
                    string reviewDate = parts[1].Trim();

                    // Check if the schedule date is today or from the past
                    if (string.Compare(reviewDate, str_today) <= 0)
                    {
                        // Print the file number, name, and its due date on the screen
                        Console.WriteLine($"{displayIndex}. {fileName} (Due: {reviewDate})");

                        // Save the number, file name, and date into our dictionary list
                        file_mapping.Add(displayIndex, Tuple.Create(fileName, reviewDate));

                        // Increase the menu number by 1 for the next item
                        displayIndex++;
                    }
                }
            }

            // If no files match today or the past, show a success message and stop
            if (file_mapping.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Great job! No reviews scheduled for today.");
                Console.ResetColor();
                Console.WriteLine("\nPress any key to return to Main Menu...");
                Console.ReadKey();
                return;
            }

            // Ask the user to choose a number from the menu
            Console.Write("\nEnter file number to start review (or 0 to cancel): ");

            // Read user input, make sure it is a real number, and check if it is in our list
            if (int.TryParse(Console.ReadLine(), out int choice) && file_mapping.ContainsKey(choice))
            {
                // Get the real file name that matches the user's chosen number
                string target_file = file_mapping[choice].Item1;

                // Get the original due date that matches the user's chosen number
                string actual_due_date = file_mapping[choice].Item2;

                // Create the full path to locate the chosen file inside the folder
                string fullPath = Path.Combine(_folderPath, target_file);

                // Check if the text file actually exists in the folder
                if (File.Exists(fullPath))
                {
                    // Read all the text content inside that file
                    string file_content = File.ReadAllText(fullPath);

                    // Clear the screen to show the file content alone
                    Console.Clear();
                    Console.WriteLine($"--- Content of: '{target_file}' ---\n");

                    // Show the text content wrapped in quotation marks
                    Console.WriteLine('"' + file_content + '"');

                    // Change the text color to blue for the next instruction
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine(Environment.NewLine + "Press any key to start the review session...");

                    // Wait for the user to press a key without showing the typed letter
                    Console.ReadKey(true);
                    Console.ResetColor();

                    // Start the learning session using the file content
                    manager.cumulative_review_session(file_content);

                    // Delete this specific review entry from the schedule file using its real date
                    manager.remove_completed_review(target_file, actual_due_date);

                    // Print a line to show the end of the session
                    Console.WriteLine("\n--------------------------------");
                }
                else
                {
                    // Change color to red and show an error if the file is missing
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: File '{target_file}' not found in database folder.");
                    Console.ResetColor();
                }
            }
            // If the user did not choose 0 and typed a wrong number, show an error
            else if (choice != 0)
            {
                Console.WriteLine("Invalid selection.");
            }

            // Ask the user to press any key to go back to the home screen
            Console.WriteLine("\nPress any key to return to Main Menu...");
            Console.ReadKey();
        }

        private static void update()
        {
            Console.Clear();
            Console.WriteLine("Update option selected.");
            Console.WriteLine("\nPress any key to return to Main Menu...");
            Console.ReadKey();
        }

        private static void delete()
        {
            Console.Clear();
            Console.WriteLine("Delete option selected.");
            Console.WriteLine("\nPress any key to return to Main Menu...");
            Console.ReadKey();
        }

        private static void main_menu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine(Environment.NewLine);
                manager.print_ascii_art("   Main Menu");
                manager.print_ascii_art("      1 - Create");
                manager.print_ascii_art("      2 - View");
                manager.print_ascii_art("      3 - Update");
                manager.print_ascii_art("      4 - Delete");
                //manager.print_ascii_art("   Select an option (1-4): ");
                Console.WriteLine(Environment.NewLine + "   Select an option (1-4): ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        create();
                        break;
                    case "2":
                        view();
                        break;
                    case "3":
                        update();
                        break;
                    case "4":
                        delete();
                        break;
                }
            }
        }

        private static void EnableAutoStart()
        {
            // Get the path of the Windows Startup folder for the current user
            string startup_folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"Microsoft\Windows\Start Menu\Programs\Startup"
            );

            // Set the shortcut file name and get the current app path
            string shortcut_path = Path.Combine(startup_folder, "MindInjection.lnk");
            string appPath = Process.GetCurrentProcess().MainModule.FileName;

            // Check if the startup shortcut does not exist yet
            if (!File.Exists(shortcut_path))
            {
                try
                {
                    // Create a Windows Script Host shell object to make a shortcut
                    Type t = Type.GetTypeFromProgID("WScript.Shell");
                    dynamic shell = Activator.CreateInstance(t);
                    var shortcut = shell.CreateShortcut(shortcut_path);

                    // Configure the shortcut path, working directory, and description
                    shortcut.TargetPath = appPath;
                    shortcut.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    shortcut.Description = "MindInjection Auto Start";

                    // Save the shortcut file to the Startup folder
                    shortcut.Save();

                    Console.WriteLine("Auto-start enabled successfully!");
                }
                catch (Exception ex)
                {
                    // Show an error message if something goes wrong
                    Console.WriteLine($"Could not configure auto-start: {ex.Message}");
                }
            }
        }
    }
}