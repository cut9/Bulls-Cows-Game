using System.Text;

namespace Bulls_Cows
{
    internal record AnswerSettings(int MaxAnswerLength, bool UniqueOnly) : IAnswerSettings;
}