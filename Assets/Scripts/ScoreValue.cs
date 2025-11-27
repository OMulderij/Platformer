public class ScoreValue
{
    public string Name
    {
        get
        {
            return Name;
        }
        
        set
        {
            Name = value;
        }
    }
    public string Score
    {
        get
        {
            return Score;
        }

        set
        {
            Score = value;
        }
    }
    public ScoreValue(string nameString, string scoreValue)
    {
        Name = nameString;
        Score = scoreValue;
    }
}
