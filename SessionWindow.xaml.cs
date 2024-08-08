using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace JKLM
{
    /// <summary>
    /// Interaction logic for SessionWindow.xaml
    /// </summary>
    public partial class SessionWindow : Window
    {
        private Dictionary<int, List<string>> words = new Dictionary<int, List<string>>();
        private Dictionary<int, List<string>> resultWords = null;
        private List<string> usedWords = new List<string>();

        public SessionWindow()
        {
            InitializeComponent();
            FillWords();
            this.Deactivated += SessionWindow_Deactivated;
        }

        /// <summary>
        /// Event handler for window deactivation.
        /// </summary>
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

        /// <summary>
        /// Fills the words dictionary from JSON files.
        /// </summary>
        private void FillWords()
        {
            words.Clear();

            // string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            // string wordsDirectory = Path.Combine(appDataPath, "JKLM", "words");
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\words", "*.json");

            foreach (string file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string lengthStr = fileName.Split('_')[1];
                int length = int.Parse(lengthStr);

                string jsonContent = File.ReadAllText(file);
                List<string> wordList = JsonSerializer.Deserialize<List<string>>(jsonContent);

                words[length] = wordList;
            }
        }

        /// <summary>
        /// Gets words containing the specified input string.
        /// </summary>
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

        /// <summary>
        /// Gets a random word from the specified word dictionary within the given length range.
        /// </summary>
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

        /// <summary>
        /// Chooses a random word from the result words.
        /// </summary>
        private void ChooseRandomWord()
        {
            if (resultWords == null) { return; }

            try
            {
                int min = int.Parse(minWordLengthTextbox.Text);
                int max = int.Parse(maxWordLengthTextbox.Text);

                string rand = GetRandomWord(resultWords, min, max);
                chosenTextbox.Text = rand;
            }
            catch { }
        }

        /// <summary>
        /// Copies the specified word to the clipboard.
        /// </summary>
        private void CopyToClipboard(string word)
        {
            Clipboard.SetText(word);
        }

        /// <summary>
        /// Removes the specified word from the word list and adds it to the used words list.
        /// </summary>
        private void RemoveWord(string word)
        {
            words[word.Length].Remove(word);
            usedWords.Add(word);
            usedWordsTextbox.AppendText(word + "\n");
        }

        /// <summary>
        /// Validates that the text input contains only alphabetic characters.
        /// </summary>
        private void TextValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^a-zA-Z]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        /// <summary>
        /// Processes the input word by removing spaces and trimming whitespace.
        /// </summary>
        private string ProcessInput(string word)
        {
            return word.Replace(" ", "").Trim().ToLower();
        }

        /// <summary>
        /// Event handler for the solve button click event.
        /// </summary>
        private void solveButton_Click(object sender, RoutedEventArgs e)
        {
            if (inputTextbox.Text.Length == 0) { return; }

            string inp = ProcessInput(inputTextbox.Text);

            Dictionary<int, List<string>> res = GetWordsContainingString(words, inp);

            clearButton_Click(sender, e);

            if (res.Count == 0)
            {
                outputBox.AppendText($"There are no words found for: {inp}");
                return;
            }

            resultWords = res;

            outputBox.AppendText($"Results for: {inp}\n");
            foreach (var entry in res)
            {
                outputBox.AppendText($"Length: {entry.Key}\n");
                outputBox.AppendText($"{string.Join(", ", entry.Value)}\n\n");
            }

            ChooseRandomWord();
        }

        /// <summary>
        /// Event handler for the clear button click event.
        /// </summary>
        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            inputTextbox.Clear();
            outputBox.Clear();
            chosenTextbox.Clear();
            resultWords = null;
        }

        /// <summary>
        /// Event handler for the shuffle button click event.
        /// </summary>
        private void shuffleButton_Click(object sender, RoutedEventArgs e)
        {
            ChooseRandomWord();
        }

        /// <summary>
        /// Event handler for the copy button click event.
        /// </summary>
        private void copyButton_Click(object sender, RoutedEventArgs e)
        {
            if (chosenTextbox.Text.Length == 0) { return; }
            CopyToClipboard(chosenTextbox.Text);
            RemoveWord(chosenTextbox.Text);
            clearButton_Click(sender, e);
        }

        /// <summary>
        /// Event handler for the input textbox key down event.
        /// </summary>
        private void inputTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                solveButton_Click(sender, e);
            }
        }

        /// <summary>
        /// Validates that the text input contains only numeric characters.
        /// </summary>
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
