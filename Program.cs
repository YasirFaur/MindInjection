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

namespace injection
{
    internal class Program
    {        
        static void Main(string[] args)
        {
            Console.Title = "MindInjection - Focus & Mastery System";
            FocusMonitor.MaximizeAndLockConsole();
            FocusMonitor.StartMonitoring();

            //creat an object of the TextManager class
            TextManager manager = new TextManager();
            //manager.get_installed_voices();
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

            while (true)
            {
                // print a message to the user.
                Console.Clear();
                string message = "Please enter your English text:";
                Console.WriteLine(message);
                manager.speak_text(message);

                manager.read_from_console();

                //check if the text length is valid 
                if (manager.is_length_valid())
                {
                    
                    //call the method to get the list of sentences
                    List<string> sentences = manager.split_text_into_sentences();

                    //generate the diminishing cycles
                    List<string> cycles = manager.generate_diminishing_cycles(sentences);                 

                    //loop through and print each cycle              
                    for (int i = 0; i < cycles.Count; i++)
                    {
                        // Clear the console screen for the new cycle
                        Console.Clear();
                        string totale_injection_time = manager.estimated_time();
                        string current_cycle_time = manager.calculate_current_cycle_time(cycles[i]);

                        int total_length = TextManager.input_text.Replace("\r\n", "").Length;
                        int current_lenght = cycles[i].Replace("\t", "").Replace("\r\n", "").Length;
                        double percentage = total_length > 0 ?  1 - ((double)current_lenght / total_length) : 0;
                        percentage *= 100;

                        Console.WriteLine(
                            "\t|| " + "Total Injection Time: " +  totale_injection_time +
                            " || " + "Current Cycle Time: " + current_cycle_time +
                            " || " + (int) percentage +
                            "% " + TextManager.GetProgressBar((int)percentage) + 
                            " || " + Environment.NewLine);                        

                        // Print the current cycle text with visual highlighting
                        manager.print_cycle_with_highlight(cycles[i]);
                        
                        // Speak the full cycle text synchronously 
                        manager.speak_text(cycles[i]);

                        // Get the longest word in lowercase to use for the focus test
                        string test_word = manager.longest_word.ToLower();

                        // Start a single stable loop for testing the user's focus
                        while (true)
                        {
                            // Clear the console to avoid duplication on wrong answers
                            //Console.Clear();

                            // Display the big ASCII art text for the test word
                            manager.print_ascii_art(test_word);

                            // Print a new line space for visual balance
                            Console.WriteLine(Environment.NewLine);

                            // Tell the focus monitor that the voice system is active now
                            TextManager.set_speaking_status(true);

                            // Speak the prompt in the background without freezing the input
                            manager.speak_text_async("type! " + manager.longest_word);

                            // Wait and capture the standard input from the user
                            string user_input = Console.ReadLine()?.Trim().ToLower();

                            // Tell the focus monitor that the voice system is resting now
                            TextManager.set_speaking_status(false);

                            // Check if the user input matches the test word exactly
                            if (user_input == test_word)
                            {
                                // Exit the test loop safely and move to the next cycle
                                break;
                            }
                            // Clear the screen after finishing all the diminishing cycles
                            Console.Clear();
                        }
                    }
                    
                }
                else
                {
                    //if false, print an error message
                    Console.WriteLine("\n[error] invalid text length. try again");
                }
                
            }                        
        }
    }
}
