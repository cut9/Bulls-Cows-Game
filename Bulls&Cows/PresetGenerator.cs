namespace Bulls_Cows
{
    internal static class PresetGenerator
    {
        public static Preset GeneratePreset(IRandomProvider rng)
        {
            string BasePool = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

            HashSet<char> Pool = new HashSet<char>();

            string Name = "GeneratedPreset";
            bool NumberDisplayOnlyMode = rng.NextDouble() < 0.2;
            bool UniqueOnly = rng.NextDouble() < 0.6;
            bool DisplayRight = rng.Next(0, 2) == 0;
            bool DisplayWrong = rng.Next(0, 2) == 0;

            int AnswerLength = rng.Next(4, 9);

            int PoolLength = UniqueOnly ? rng.Next(AnswerLength, 33) : rng.Next(3, 33);

            int NumberOfAttemts;

            double N = 0;
            double F = Math.Log2((AnswerLength + 1) * (AnswerLength + 2) / 2);

            if (UniqueOnly)
            {
                for (int i = 0; i < AnswerLength; i++)
                    N += Math.Log2(PoolLength - i);
            }
            else
            {
                N = AnswerLength * Math.Log2(PoolLength);
            }

            int minAttempts = (int)Math.Ceiling(N / F) + 2;
            int maxAttempts = Math.Max(minAttempts, (int)F) + 2;

            NumberOfAttemts = rng.Next(minAttempts, maxAttempts);

            if (NumberDisplayOnlyMode)
            {
                if (!UniqueOnly)
                {
                    NumberOfAttemts *= 2;
                }
                NumberOfAttemts += (NumberOfAttemts / 2) + 2;
            }

            Pool = BasePool.Take(PoolLength).ToHashSet();

            AnswerSettings answerSettings = new AnswerSettings(AnswerLength, UniqueOnly);
            OutputSettings outputSettings = new OutputSettings(DisplayRight, DisplayWrong, NumberDisplayOnlyMode);
            return new Preset(Name, NumberOfAttemts, Pool, answerSettings, outputSettings);
        }
    }
}
