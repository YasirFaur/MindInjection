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

namespace injection
{
    internal class Program
    {
        private static TextManager manager = new TextManager();

        static void Main(string[] args)
        {
            Console.Title = "MindInjection - Focus & Mastery System";
            FocusMonitor.MaximizeAndLockConsole();
            FocusMonitor.StartMonitoring();

            Console.WriteLine(Environment.NewLine);
            manager.print_ascii_art("   MindInjection");
            manager.speak_text("Mind Injection");

            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine(Environment.NewLine + "   Where Repetition Meets Focus, Mastery Becomes Inevitable.");
            manager.speak_text("Where Repetition Meets Focus, Mastery Becomes Inevitable.");
            Console.WriteLine("   Created by Yasir Faur.");
            manager.speak_text("Created by Yasir Faur.");

            Console.ResetColor();
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
                    List<string> sentences = manager.split_text_into_sentences();
                    List<string> cycles = manager.generate_diminishing_cycles(sentences);

                    for (int i = 0; i < cycles.Count; i++)
                    {
                        Console.Clear();
                        string totale_injection_time = manager.estimated_time();
                        string current_cycle_time = manager.calculate_current_cycle_time(cycles[i]);

                        int total_length = TextManager.input_text.Replace("\r\n", "").Length;
                        int current_lenght = cycles[i].Replace("\t", "").Replace("\r\n", "").Length;
                        double percentage = total_length > 0 ? 1 - ((double)current_lenght / total_length) : 0;
                        percentage *= 100;

                        Console.WriteLine(
                            "\t|| " + "Total Injection Time: " + totale_injection_time +
                            " || " + "Current Cycle Time: " + current_cycle_time +
                            " || " + (int)percentage +
                            "% " + TextManager.GetProgressBar((int)percentage) +
                            " || " + Environment.NewLine);

                        manager.print_cycle_with_highlight(cycles[i]);
                        manager.speak_text(cycles[i]);

                        string test_word = manager.longest_word.ToLower();

                        while (true)
                        {
                            manager.print_ascii_art(test_word);
                            Console.WriteLine(Environment.NewLine);

                            TextManager.set_speaking_status(true);
                            manager.speak_text_async("type! " + manager.longest_word);

                            string user_input = Console.ReadLine()?.Trim().ToLower();
                            TextManager.set_speaking_status(false);

                            if (user_input == test_word)
                            {
                                break;
                            }
                            Console.Clear();
                        }
                    }
                    
                    Console.Clear();
                    Console.WriteLine("\n[Process Finished. Returning to Main Menu...]");
                    Thread.Sleep(2000);
                    return;
                }
                else
                {
                    Console.WriteLine("\n[error] invalid text length. try again");
                    Thread.Sleep(2000);
                }
            }
        }

        private static void view()
        {
            Console.Clear();
            Console.WriteLine("View option selected.");
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
                manager.print_ascii_art("   Select an option (1-4): ");

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
    }
}