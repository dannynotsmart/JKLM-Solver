using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.Json;
using System.IO;
using System.Text.RegularExpressions;

namespace JKLM
{
    /// <summary>
    /// Interaction logic for SessionWindow.xaml
    /// </summary>
    public partial class SessionWindow : Window
    {
        Dictionary<int, List<string>> words = new Dictionary<int, List<string>>();
        Dictionary<int, List<string>> resultWords = null;
        List<string> usedWords = new List<string>();

        public SessionWindow()
        {
            InitializeComponent();
            FillWords();

            this.Deactivated += SessionWindow_Deactivated;
        }

        private void SessionWindow_Deactivated(object? sender, EventArgs e)
        {
            if ((bool)autoTypeCheckbox.IsChecked && chosenTextbox.Text.Length != 0)
            {
                try
                {
                    float delay = float.Parse(delayTextbox.Text);
                    Keyboard.Type(chosenTextbox.Text, delay);

                    if ((bool)autoEnterCheckbox.IsChecked)
                    {
                        Keyboard.Type(Key.Enter);
                    }
                    RemoveWord(chosenTextbox.Text);
                    inputTextbox.Clear();
                    outputBox.Clear();
                    chosenTextbox.Clear();
                    resultWords = null;
                }
                catch { }
            }
        }

        private void FillWords()
        {
            this.words.Clear();

            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string wordsDirectory = System.IO.Path.Combine(appDataPath, "JKLM", "words");

            string[] files = Directory.GetFiles(wordsDirectory, "*.json");

            foreach (string file in files)
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(file);
                string lengthStr = fileName.Split('_')[1];
                int length = int.Parse(lengthStr);

                string jsonContent = File.ReadAllText(file);
                List<string> wordList = JsonSerializer.Deserialize<List<string>>(jsonContent);

                words[length] = wordList;
            }
        }

        private Dictionary<int, List<string>> GetWordsContainingString(Dictionary<int, List<string>> words, string inputString)
        {
            Dictionary<int, List<string>> result = new Dictionary<int, List<string>>();

            foreach (var entry in words)
            {
                List<string> matchingWords = entry.Value.FindAll(word => word.Contains(inputString));

                if (matchingWords.Count > 0)
                {
                    result[entry.Key] = matchingWords;
                }
            }

            return result;
        }

        private string GetRandomWord(Dictionary<int, List<string>> words, int minLength, int maxLength)
        {
            List<string> filteredWords = new List<string>();

            foreach (var entry in words)
            {
                if (entry.Key >= minLength && entry.Key <= maxLength)
                {
                    filteredWords.AddRange(entry.Value);
                }
            }

            if (filteredWords.Count == 0)
            {
                return null;
            }

            Random random = new Random();
            int randomIndex = random.Next(filteredWords.Count);

            return filteredWords[randomIndex];
        }

        private void ChooseRandomWord()
        {
            if (resultWords == null) { return; }
            try
            {
                int min = Int32.Parse(minWordLengthTextbox.Text);
                int max = Int32.Parse(maxWordLengthTextbox.Text);

                string rand = GetRandomWord(resultWords, min, max);
                chosenTextbox.Text = rand;
            }
            catch { }
        }
        private void CopyToClipboard(string word)
        {
            Clipboard.SetText(word);
        }

        private void RemoveWord(string word)
        {
            words[word.Length].Remove(word);
            usedWords.Add(word);
            usedWordsTextbox.AppendText(word + "\n");
        }

        private void TextValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^a-zA-Z]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private string ProcessInput(string word)
        {
            return word.Replace(" ", "").Trim().ToLower();
        }

        private void solveButton_Click(object sender, RoutedEventArgs e)
        {
            if (inputTextbox.Text.Length == 0) { return; }

            string inp = ProcessInput(inputTextbox.Text);

            Dictionary<int, List<string>> res = GetWordsContainingString(words, inp);

            clearButton_Click(sender, e);

            if (res.Count == 0)
            {
                outputBox.AppendText($"There are no words found for: {inp}");
                this.resultWords = null;
                return;
            }

            this.resultWords = res;

            outputBox.AppendText($"Results for: {inp}\n");
            foreach (var entry in res)
            {
                outputBox.AppendText($"Length: {entry.Key}\n");
                outputBox.AppendText($"{string.Join(", ", entry.Value)}\n\n");
            }

            ChooseRandomWord();
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            inputTextbox.Clear();
            outputBox.Clear();
            chosenTextbox.Clear();
            resultWords = null;
        }

        private void shuffleButton_Click(object sender, RoutedEventArgs e)
        {
            ChooseRandomWord();
        }

        private void copyButton_Click(object sender, RoutedEventArgs e)
        {
            if (chosenTextbox.Text.Length == 0) { return; }
            CopyToClipboard(chosenTextbox.Text);
            RemoveWord(chosenTextbox.Text);
            clearButton_Click(sender, e);
        }

        private void inputTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                solveButton_Click(sender, e);
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}