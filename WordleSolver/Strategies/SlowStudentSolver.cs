using System.Collections.Generic;
using System.Linq;

namespace WordleSolver.Strategies;

/// <summary>
/// Example solver that simply iterates through a fixed list of words.
/// Students will replace this with a smarter algorithm.
/// </summary>
public sealed class SlowStudentSolver : IWordleSolverStrategy
{
    /// <summary>Absolute or relative path of the word-list file.</summary>
    private static readonly string WordListPath = Path.Combine("data", "wordle.txt");

    /// <summary>In-memory dictionary of valid five-letter words.</summary>
    private static readonly List<string> WordList = LoadWordList();

    /// <summary>
    /// Remaining words that can be chosen
    /// </summary>
    private List<string> _remainingWords = new();

    // TODO: ADD your own private variables that you might need

    /// <summary>
    /// Loads the dictionary from disk, filtering to distinct five-letter lowercase words.
    /// </summary>
    private static List<string> LoadWordList()
    {
        if (!File.Exists(WordListPath))
            throw new FileNotFoundException($"Word list not found at path: {WordListPath}");

        return File.ReadAllLines(WordListPath)
            .Select(w => w.Trim().ToLowerInvariant())
            .Where(w => w.Length == 5)
            .Distinct()
            .ToList();
    }

    /// <inheritdoc/>
    public void Reset()
    {
        _remainingWords = [.. WordList];
    }

    /// <summary>
    /// Determines the next word to guess given feedback from the previous guess.
    /// </summary>
    public string PickNextGuess(GuessResult previousResult)
    {
        if (!previousResult.IsValid)
            throw new InvalidOperationException("PickNextGuess shouldn't be called if previous result isn't valid");

        // FIRST GUESS
        if (previousResult.Guesses.Count == 0)
        {
            string firstWord = "slate";

            _remainingWords.Remove(firstWord);

            return firstWord;
        }

        // FILTER USING LAST GUESS
        var lastGuess = previousResult.Guesses.Last();

        _remainingWords = _remainingWords
            .Where(word => IsPossibleWord(word, lastGuess.Word, lastGuess.LetterStatuses))
            .ToList();

        string choice = ChooseBestRemainingWord(previousResult);
        _remainingWords.Remove(choice);

        return choice;
    }

    /// <summary>
    /// Pick best remaining word (simple heuristic)
    /// </summary>
    public string ChooseBestRemainingWord(GuessResult previousResult)
    {
        if (_remainingWords.Count == 0)
            throw new InvalidOperationException("No remaining words to choose from");

        return _remainingWords.First();
    }

    /// <summary>
    /// Checks if a word is still possible based on Wordle feedback
    /// </summary>
    private bool IsPossibleWord(string candidate, string guess, LetterStatus[] status)
    {
        for (int i = 0; i < 5; i++)
        {
            char letter = guess[i];

            if (status[i] == LetterStatus.Correct)
            {
                if (candidate[i] != letter)
                    return false;
            }
            else if (status[i] == LetterStatus.Misplaced)
            {
                if (candidate[i] == letter)
                    return false;

                if (!candidate.Contains(letter))
                    return false;
            }
            else if (status[i] == LetterStatus.Unused)
            {
                if (candidate.Contains(letter))
                    return false;
            }
        }

        return true;
    }
}