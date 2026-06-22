using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using WenceyWang.FIGlet;
namespace injection
{
    public class TextManager
    {
        DataBaseManager dbm = new DataBaseManager();

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
        private const int MIN_LENGTH = 256;
        private const int MAX_LENGTH = 1024;

        public string longest_word = "";

        //this properety stors the text that the user inputs
        public static string input_text { get; set; }
        public int current_text_length = 0;

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
            if (string.IsNullOrEmpty(input_text))
            {
                return false;
            }

            //get the total number of characters in the text
            int text_length = input_text.Length;

            //the text must be >= MIN_LENGTH and <= MAX_LENGTH characters
            if (text_length >= MIN_LENGTH && text_length <= MAX_LENGTH)
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
            string[] raw_sentences = input_text.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            //loop through each raw sentence to clean it
            foreach (string sentence in raw_sentences)
            {
                //trim standard spaces, newlines, and hidden web characters like NBSP
                string clean_sentence = sentence.Trim(' ', '\r', '\n', '\t', '\u00a0');

                if (!string.IsNullOrEmpty(clean_sentence))
                {
                    //add the dot back safly
                    sentence_list.Add(clean_sentence + ".");
                }

            }
            dbm.save_session(string.Join(Environment.NewLine, sentence_list));  
            return sentence_list;
        }


        //this method creates the diminishing text cycles
        //it remove the first sentence in each new cycle
        public List<string> generate_diminishing_cycles(List<string> sentences)
        {
            List<string> cycles_list = new List<string>();

            //outer loop to control the starting sentence of each cycle
            for (int i = 0; i < sentences.Count; i++)
            {
                //skip 'i' sentences to create the diminshing effect
                var remaining_sentences = sentences.Skip(i).Select(s => "\t" + s);                

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

        public TimeSpan calculate_estimated_time(string text)
        {
            if (string.IsNullOrEmpty(text)) return TimeSpan.Zero;
            // multiply text length by 0.45 seconds
            double totale_seconds = text.Length * 0.45;
            return TimeSpan.FromSeconds(totale_seconds);
        }
        
        public string estimated_time()
        {
            TimeSpan estimated = calculate_estimated_time(input_text);
            return estimated.ToString(@"hh\:mm\:ss");
        }

        public string calculate_current_cycle_time(string text)
        {
            if (string.IsNullOrEmpty(text)) return "00:00:00";
            double totale_seconds = text.Length * 0.076;

            return TimeSpan.FromSeconds(totale_seconds).ToString(@"hh\:mm\:ss");
        }

        //calculate the remaining time based on the fading text length.
        public string calculate_remaining_time(string current_cycle_text)
        {
            if (string.IsNullOrEmpty(current_cycle_text) || 
                string.IsNullOrEmpty(input_text)) return "00:00:00";
            //the remaining time is directly linked to the size of the text left to process, we use the same formula as the estimated time but with the current cycle text length

            double remaining_seconds = current_cycle_text.Length * 0.45;
            return TimeSpan.FromSeconds(remaining_seconds).ToString(@"hh\:mm\:ss");
        }

        // Import Windows API to enable ANSI colors
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        public static void EnableAnsiColors()
        {
            // Get Standard Output Handle
            var handle = GetStdHandle(-11);
            // Get current console mode
            GetConsoleMode(handle, out uint mode);
            // Enable virtual terminal processing (0x0004)
            SetConsoleMode(handle, mode | 0x0004);
        }

        // This method generates a 10-character progress bar based on percentage
        public static string GetProgressBar(int percentage)
        {
            if (percentage < 0) percentage = 0;
            if (percentage > 100) percentage = 100;

            int plusCount = percentage / 10;
            int minusCount = 10 - plusCount;

            // ANSI Color Codes
            string greenColor = "\x1b[32m";
            string redColor = "\x1b[31m";
            string resetColor = "\x1b[0m";

            // Build the colored progress bar string
            string progress = $"{greenColor}{new string('+', plusCount)}";
            string remaining = $"{redColor}{new string('-', minusCount)}";

            return $"[{progress}{remaining}{resetColor}]";
        }

        public string GetTheFirst1024Characters(string text) 
        {
            if (text.Length <= 1024) return text;
            //find the last dot before 1024 characters to avoid cutting in the middle of a sentence
            int cutIndex = text.Substring(0, 1024).LastIndexOf('.');
            //fallback to the last space if no dot is found
            if(cutIndex == -1) cutIndex = text.Substring(0, 1024).LastIndexOf(' ');
            //fallback to 1024 if no space is found
            if (cutIndex == -1) cutIndex = 1024;
            if (cutIndex == -1 ) cutIndex = 1024; // If no space found, cut at 1024

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[Warning] Text was longer than 1024 chars and truncated to {cutIndex} chars!");
            Console.ResetColor();
            Thread.Sleep(2000);

            return text.Substring(0, cutIndex).Trim();
        }

        public void cumulative_review_session(string full_text)
        {
            if (string.IsNullOrWhiteSpace(full_text)) return;

            // 1. Split text into sentences
            string[] sentences = full_text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            string accumulatedText = "";

            for (int i = 0; i < sentences.Length; i++)
            {
                string currentSentence = sentences[i].Trim();
                if (string.IsNullOrWhiteSpace(currentSentence)) continue;

                // Build the cumulative layer
                accumulatedText = string.IsNullOrEmpty(accumulatedText)
                    ? currentSentence
                    : accumulatedText + "." + Environment.NewLine + currentSentence;

                // 2 & 7. Display the current accumulated text layer
                Console.Clear();
                Console.WriteLine(Environment.NewLine + $"--- Layer {i + 1} of {sentences.Length} ---\n");                
                Console.WriteLine(accumulatedText + ".");                

                // 3 & 7. Call TTS Engine to read the newly formed text block
                speak_text(accumulatedText);

                // 4. Select longest word from the *current* sentence only
                string longestWord = find_longest_word(currentSentence);

                if (!string.IsNullOrEmpty(longestWord))
                {
                    // 5. Ask user to type it to confirm attention
                    Console.WriteLine(Environment.NewLine);                    
                    print_ascii_art(longestWord);
                    speak_text("Type: " + longestWord);                    
                    string userInput = Console.ReadLine()?.Trim();                    

                    // 6. Loop until correct
                    while (!string.Equals(userInput, longestWord, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        print_ascii_art(longestWord);
                        speak_text("Type: " + longestWord);
                        Console.ResetColor();
                        userInput = Console.ReadLine()?.Trim();
                    }                    
                }
            }

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Cumulative Spaced-Review Session Completed Successfully!");
            Console.ResetColor();            
        }

        // Helper method to extract the longest pure word from a sentence
        private string find_longest_word(string sentence)
        {
            // Remove punctuation to get pure words
            string cleanSentence = Regex.Replace(sentence, @"[^\w\s]", "");
            string[] words = cleanSentence.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            string longest = "";
            foreach (string word in words)
            {
                if (word.Length > longest.Length)
                {
                    longest = word;
                }
            }
            return longest;
        }

        public void remove_completed_review(string fileName, string targetDate)
        {
            string _folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database");
            string schedule_file = Path.Combine(_folderPath, "review_schedule.csv");
            if (!File.Exists(schedule_file)) return;            
            var linesToKeep = File.ReadAllLines(schedule_file)
                .Where(line => {
                    string[] parts = line.Split(',');
                    if (parts.Length == 2)
                    {
                        string name = parts[0].Trim();
                        string date = parts[1].Trim();
                        
                        return !(string.Equals(name, fileName, StringComparison.OrdinalIgnoreCase) && date == targetDate);
                    }
                    return true;
                }).ToList();

            File.WriteAllLines(schedule_file, linesToKeep);
        }

    }
}