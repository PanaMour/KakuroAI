public class AITracker
{
    public int mistakes { get; private set; }
    public float elapsedTime { get; private set; }
    public int hintsUsed { get; private set; }

    public void RecordMistake()
    {
        mistakes++;
    }

    public void AddElapsedTime(float deltaTime)
    {
        elapsedTime += deltaTime;
    }

    public void RecordHint()
    {
        hintsUsed++;
    }

    // New method to update all stats at once
    public void UpdateStats(float newElapsedTime, int newMistakes, int newHints)
    {
        elapsedTime = newElapsedTime;
        mistakes = newMistakes;
        hintsUsed = newHints;
    }

    public void Reset()
    {
        mistakes = 0;
        elapsedTime = 0;
        hintsUsed = 0;
    }
}
