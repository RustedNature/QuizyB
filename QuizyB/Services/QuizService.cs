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

    public Question? CurrentQuestion { get; set; }
    private List<Question> Questions { get; set; } = [];

    public bool IsAnswered { get; set; }
    public bool IsCorrect { get; set; }

    public void NextQuestion()
    {
        Questions.Remove(CurrentQuestion!);
        IsAnswered = false;
        IsCorrect = false;
        RandomQuestion();
    }

    private void RandomQuestion()
    {
        if (Questions.Count <= 0) return;
        var randIndex = new Random().Next(Questions.Count);
        CurrentQuestion = Questions[randIndex];
    }

    public async Task LoadQuestions(string? url)
    {
        try
        {
            var json = await _httpClient.GetStringAsync(url);
            Questions = JsonSerializer.Deserialize<List<Question>>(json)!;
            RandomQuestion();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading questions: {ex.Message}");
        }
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

        IsAnswered = true;
    }

    public void Reset()
    {
        CurrentQuestion = null;
        Questions = [];
        IsAnswered = false;
        IsCorrect = false;
    }
}

public class Question
{
    [JsonPropertyName("questionText")] public string? QuestionText { get; set; }

    [JsonPropertyName("answers")] public List<string>? Answers { get; set; }

    [JsonPropertyName("correctAnswerIndex")]
    public int CorrectAnswerIndex { get; set; }
}