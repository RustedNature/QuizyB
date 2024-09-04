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
    internal List<Question> Questions { get; private set; } = [];
    internal int RemainQuestionsToSolve { get; private set; }
    internal int QuestionsPoolCount { get; private set; }
    public bool IsQuestionAnswered { get; private set; }
    public bool IsAnswerCorrect { get; private set; }
    public bool IsQuizLoadedSuccessfully { get; private set; }
    public bool IsLoadingQuiz { get; private set; }
    public bool IsFinished { get; private set; }
    public int Points { get; private set; }
    public bool IsLastQuestion { get; private set; }

    public void NextQuestion()
    {
        IsQuestionAnswered = false;
        IsAnswerCorrect = false;
        RandomQuestion();
        if (Questions.Count == 0) IsFinished = true;
    }

    private void RandomQuestion()
    {
        if (Questions.Count <= 0) return;
        var randIndex = new Random().Next(Questions.Count);
        CurrentQuestion = Questions[randIndex];
        SetIsLastQuestion();
    }

    public async Task LoadQuestions(string? url)
    {
        IsLoadingQuiz = true;
        try
        {
            var json = await _httpClient.GetStringAsync(url);
            Questions = JsonSerializer.Deserialize<List<Question>>(json)!;
            QuestionsPoolCount = Questions.Count;
            IsQuizLoadedSuccessfully = true;
            RemainQuestionsToSolve = Questions.Count;
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
        if (Questions.Count <= 0) return;
        var random = new Random();
        foreach (var question in Questions)
        {
            var correctAnswer = question.Answers![question.CorrectAnswerIndex];
            question.Answers = question.Answers
                .OrderBy(_ => random.Next())
                .ToList();

            SetNewCorrectAnswerIndex(question, correctAnswer);
        }
    }

    private void SetNewCorrectAnswerIndex(Question question, string correctAnswer)
    {
        question.CorrectAnswerIndex = question.Answers!.FindIndex(answer => answer == correctAnswer);
    }

    public string GetButtonTheme(int index)
    {
        if (!IsQuestionAnswered) return "btn btn-primary";

        if (index == CurrentQuestion!.CorrectAnswerIndex) return "btn btn-success";

        return "btn btn-danger";
    }

    public void CheckAnswer(int chosenAnswerIndex)
    {
        if (CurrentQuestion == null) return;
        if (CurrentQuestion.CorrectAnswerIndex == chosenAnswerIndex)
        {
            IsAnswerCorrect = true;
            Points++;
        }

        RemoveCurrentQuestionFromPool();
        RemainQuestionsToSolve--;
        IsQuestionAnswered = true;
    }

    private void SetIsLastQuestion()
    {
        if (Questions.Count == 1) IsLastQuestion = true;
    }

    private void RemoveCurrentQuestionFromPool()
    {
        Questions.Remove(CurrentQuestion!);
    }

    public void Reset()
    {
        CurrentQuestion = null;
        Questions = [];
        IsQuestionAnswered = false;
        IsAnswerCorrect = false;
        IsQuizLoadedSuccessfully = false;
        IsFinished = false;
        IsLastQuestion = false;
        Points = 0;
    }
}

public class Question
{
    [JsonPropertyName("questionText")] public string? QuestionText { get; set; }
    [JsonPropertyName("answers")] public List<string>? Answers { get; set; }

    [JsonPropertyName("correctAnswerIndex")]
    public int CorrectAnswerIndex { get; set; }
}