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
                string message = "Please enter your English text:";
                Console.WriteLine(message);
                manager.speak_text(message);

                manager.read_from_console();

                //check if the text length is valid 
                if (manager.is_length_valid())
                {
                    //if true , print a success message
                    Console.WriteLine("\n[success] the text length is perfect!");

                    //call the method to get the list of sentences
                    List<string> sentences = manager.split_text_into_sentences();

                    //generate the diminishing cycles
                    List<string> cycles = manager.generate_diminishing_cycles(sentences);

                    Console.WriteLine("\n==== display diminishing cycles ===");

                    //loop through and print each cycle              
                    for (int i = 0; i < cycles.Count; i++)
                    {
                        // Clear the console screen for the new cycle
                        Console.Clear();

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
    
    public class TextManager
    {        
        SpeechSynthesizer synth = new SpeechSynthesizer();
        public static bool is_reading { get; private set; }
        public TextManager()
        {
            synth.SelectVoice("Microsoft David Desktop");
            synth.Volume = 50; //range: 0 - 100
            synth.Rate = -1; // speed range: (-10) to 10
        }

        public static void set_speaking_status(bool status)
        {
            is_reading = status;
        }

        public void speak_text_async(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            synth.SpeakAsyncCancelAll();
            synth.SpeakAsync(text);
        }

       
        //these constants set the minimum and maximum text length
        //we use constants so we can change the limits easily in the future
        private const int MIN_LENGTH = 366;
        private const int MAX_LENGTH = 1024;

        public string longest_word = "";

        //this properety stors the text that the user inputs
        public string input_text { get; set; }


        public void read_from_console()
        {
            // Set UTF-8 encoding
            Console.InputEncoding = Console.OutputEncoding = Encoding.UTF8;
            // Efficient text builder
            var sb = new StringBuilder();
            // Open stream to bypass 254-character limit
            var rd = new StreamReader(Console.OpenStandardInput(), Encoding.UTF8, false, 4096);
            /*
             * Console.OpenStandardInput(): open the raw console input stream to capture pasted (entered) text directly
             * Encoding.UTF8: tells the reader to use utf-8 format so special symbols do not break
             * false: disables checking for a byte order mark (bom) at the start of the text
             * 4096: sets a large 4kb buffer size to read long lines of text all at once
             */

            string ln; // Holds current line
                       // Loop until an empty line is detected
            while ((ln = rd.ReadLine()) != null && ln.Trim() != "")
                sb.AppendLine(ln); // Add line to builder
            input_text = sb.ToString().Trim(); // Save clean final text
        }

        //this method checks if the text length is correct
        //it returns true if the length is between MIN_LENGTH and MAX_LENGTH
        //it returns false if the text is too short or too long
        public bool is_length_valid()
        {
            //check if the text is null or empty first
            if (string.IsNullOrEmpty(input_text ))
            {
                return false;
            }

            //get the total number of characters in the text
            int text_length = input_text.Length;

            //the text must be >= MIN_LENGTH and <= MAX_LENGTH characters
            if (text_length >= MIN_LENGTH  && text_length <= MAX_LENGTH)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //this method splits the text into a list of sentences
        //each sentence is separated by dot (.)
        public List<string> split_text_into_sentences()
        {
            //creat an empty list to store our sentences
            List<string> sentence_list = new List<string>();

            //split the text whenever there is a dot
            string[] raw_sentences = input_text.Split(new[]{ '.'},StringSplitOptions.RemoveEmptyEntries);

            //loop through each raw sentence to clean it
            foreach (string sentence in raw_sentences)
            {
                //trim standard spaces, newlines, and hidden web characters like NBSP
                string clean_sentence = sentence.Trim(' ', '\r', '\n', '\t', '\u00a0');

                if (!string .IsNullOrEmpty (clean_sentence )) 
                {
                    //add the dot back safly
                    sentence_list.Add(clean_sentence + ".");
                }                

            }
            return sentence_list;
        }


        //this method creates the diminishing text cycles
        //it remove the first sentence in each new cycle
        public List<string> generate_diminishing_cycles(List<string > sentences)
        {
            List<string> cycles_list = new List<string>();

            //outer loop to control the starting sentence of each cycle
            for (int i = 0; i < sentences.Count; i++)
            {
                //skip 'i' sentences to create the diminshing effect
                var remaining_sentences = sentences.Skip(i);

                //combine the remaining sentences back into on full text
                string cycle_text = string.Join(Environment.NewLine, remaining_sentences);

                //add the new cycle text to our list
                cycles_list.Add(cycle_text);
            }
            return cycles_list;
        }

        //this method prints a cycle text, but makes the first sentence yellow
        public void print_cycle_with_highlight(string cycle_text)
        {
            //split the text into the first line and the reset of the lines
            //we use StringSplitOptions.None to keep empty linse if they exist
            string[] lines = cycle_text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

            if (lines.Length == 0) return;

            //print the very first sentence in yellow
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(lines[0]);
            longest_word = find_longest_word(lines[0]);
            //reset color back to normal            
            Console.ResetColor();            

            //print the remaining sentences in the default color
            for (int i = 1; i < lines.Length; i++)
            {
                Console.WriteLine(lines[i]);
            }

            
        }

        //this method finds the longest word in a given setence, ignore puntuation
        public string find_longest_word(string sentence)
        {
            if (string.IsNullOrEmpty(sentence)) return string.Empty;

            //remove punctuation markes to get pure words
            char[] punctuation = { '.', ',', '!', '?', ';', ':', '(', ')', '"'};
            string clean_sentence = sentence.Trim();
            foreach (char p in punctuation)
            {
                clean_sentence = clean_sentence.Replace(p.ToString(), "");
            }

            //split sentence into words
            string[] words = clean_sentence.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string longest_word = string.Empty;

            //loop to find the word with maximum length
            foreach (string word in words )
            {
                if (word.Length > longest_word.Length)
                {
                    longest_word = word;
                }
            }
            return longest_word;
        }

        //this method converts text to speech completely offline
            
        public void speak_text(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            is_reading = true;
            synth.Speak(text); //speek synchronously                        
            is_reading = false;
        }

        public void get_installed_voices()
        {
            foreach (var voice in synth.GetInstalledVoices()) 
            {
                var info = voice.VoiceInfo;
                Console.WriteLine(info.Name);
            }            
        }

        public void stop_speaking()
        {
            synth.SpeakAsyncCancelAll();
        }

        public void print_ascii_art(string text)
        {
            var art = new AsciiArt(text);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(art.ToString());
            Console.ResetColor();
        }
    }
    class FocusMonitor
    {
        private static NotifyIcon try_icon = new NotifyIcon();

        [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
        [DllImport("kernel32.dll")] private static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")] private static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

        [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_MAXIMIZE = 3;

        [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")] private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;
        private const int WS_MINIMIZEBOX = 0x00020000;
        private const int WS_MAXIMIZEBOX = 0x00010000;

        public static void MaximizeAndLockConsole()
        {
            IntPtr consoleWindow = GetConsoleWindow();
            ShowWindow(consoleWindow, SW_MAXIMIZE);
            int currentStyle = GetWindowLong(consoleWindow, GWL_STYLE);
            SetWindowLong(consoleWindow, GWL_STYLE, currentStyle & ~WS_MINIMIZEBOX & ~WS_MAXIMIZEBOX);
        }
        

        private static SpeechSynthesizer voice_notification = new SpeechSynthesizer();
        static FocusMonitor()
        {
            voice_notification.SelectVoice("Microsoft Zira Desktop");
            voice_notification.Volume = 100; //range: 0 - 100
            voice_notification.Rate = -1; // speed range: (-10) to 10

            try_icon.Icon = System.Drawing.SystemIcons.Information;
            try_icon.Visible = true;
        }        

        

        public static void StartMonitoring()
        {
            

            // 2 = GA_ROOT (Gets the top-level window)
            IntPtr myConsoleRoot = GetAncestor(GetConsoleWindow(), 2);
            //SpeechSynthesizer voice_notification = new SpeechSynthesizer();
            Thread monitorThread = new Thread(() =>
            {
                while (true)
                {
                    IntPtr activeRoot = GetAncestor(GetForegroundWindow(), 2);

                    if (activeRoot != myConsoleRoot)
                    {
                        if (TextManager.is_reading)
                        {
                            try_icon.ShowBalloonTip(
                            1500,
                            "Mind Injection",
                            "Focus lost! Return to your session.",
                            ToolTipIcon.Warning);
                            voice_notification.Speak("Return to Mind Injection");
                        }
                        
                    }
                    Thread.Sleep(500);
                }
            });
            monitorThread.IsBackground = true;
            monitorThread.Start();
        }
    }

}
