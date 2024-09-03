using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuizyB.Services;

public class QuizService
{
    private readonly HttpClient _httpClient;

    public QuizService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Question? CurrentQuestion { get; private set; }
    internal List<Question> Questions { get; set; } = [];

    internal int QuestionsToSolve { get; private set; }
    internal int MaxQuestionsCount { get; set; }
    public bool IsAnswered { get; set; }
    public bool IsCorrect { get; set; }
    public bool QuizLoadedSuccessfully { get; set; }
    public bool IsLoadingQuiz { get; set; }

    public bool IsFinished { get; set; }

    public void NextQuestion()
    {
        IsAnswered = false;
        IsCorrect = false;
        RandomQuestion();
        if (Questions.Count == 0) IsFinished = true;
    }

    private void RandomQuestion()
    {
        if (Questions.Count <= 0) return;
        var randIndex = new Random().Next(Questions.Count);
        CurrentQuestion = Questions[randIndex];
    }

    public async Task LoadQuestions(string? url)
    {
        IsLoadingQuiz = true;
        try
        {
            var json = await _httpClient.GetStringAsync(url);
            Questions = JsonSerializer.Deserialize<List<Question>>(json)!;
            MaxQuestionsCount = Questions.Count;
            QuizLoadedSuccessfully = true;
            QuestionsToSolve = Questions.Count;
            RandomizeAnswerIndexes();
            RandomQuestion();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading questions: {ex.Message}");
        }
        finally
        {
            IsLoadingQuiz = false;
        }
    }

    private void RandomizeAnswerIndexes()
    {
    }

    public string GetButtonTheme(int index)
    {
        if (!IsAnswered) return "btn btn-primary";

        if (index == CurrentQuestion!.CorrectAnswerIndex) return "btn btn-success";

        return "btn btn-danger";
    }

    public void CheckAnswer(int chosenAnswerIndex, ref int points)
    {
        if (CurrentQuestion == null) return;
        if (CurrentQuestion.CorrectAnswerIndex == chosenAnswerIndex)
        {
            IsCorrect = true;
            points++;
        }

        Questions.Remove(CurrentQuestion!);
        QuestionsToSolve--;
        IsAnswered = true;
    }

    public void Reset()
    {
        CurrentQuestion = null;
        Questions = [];
        IsAnswered = false;
        IsCorrect = false;
        QuizLoadedSuccessfully = false;
        IsFinished = false;
    }
}

public class Question
{
    [JsonPropertyName("questionText")] public string? QuestionText { get; set; }

    [JsonPropertyName("answers")] public List<string>? Answers { get; set; }

    [JsonPropertyName("correctAnswerIndex")]
    public int CorrectAnswerIndex { get; set; }
}