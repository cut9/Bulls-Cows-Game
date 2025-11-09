namespace Bulls_Cows
{
    internal interface IAnswerSettings
    {
        int MaxAnswerLength { get; }
        bool UniqueOnly { get; }
    }
}