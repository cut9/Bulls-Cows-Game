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

            double N_bits;
            if (UniqueOnly)
            {
                N_bits = 0.0;
                for (int i = 0; i < AnswerLength; i++)
                    N_bits += Math.Log2(PoolLength - i);
            }
            else
            {
                N_bits = AnswerLength * Math.Log2(PoolLength);
            }

            double feedbackStates;
            if (NumberDisplayOnlyMode)
            {
                feedbackStates = AnswerLength + 1;
            }
            else
            {
                if (DisplayRight && DisplayWrong)
                {
                    feedbackStates = Math.Pow(2, AnswerLength);
                }
                else if (!DisplayRight && !DisplayWrong)
                {
                    feedbackStates = (AnswerLength + 1) * (AnswerLength + 2) / 2.0;
                }
                else
                {
                    double a = Math.Pow(2, AnswerLength); 
                    double b = (AnswerLength + 1) * (AnswerLength + 2) / 2.0;
                    feedbackStates = Math.Ceiling((a + b) / 2.0);
                }
            }

            double F_bits = Math.Log2(Math.Max(1.0, feedbackStates));

            int minAttempts = (int)Math.Ceiling(N_bits / Math.Max(1e-9, F_bits)) + 2;
            int maxAttempts = Math.Max(minAttempts, (int)Math.Ceiling(F_bits)) + 2;

            NumberOfAttemts = rng.Next(minAttempts, maxAttempts);

            while (Pool.Count < PoolLength)
            {
                int index = rng.Next(0, BasePool.Length);
                Pool.Add(BasePool[index]);
            }

            Pool = Pool.OrderBy(x => x).ToHashSet();

            AnswerSettings answerSettings = new AnswerSettings(AnswerLength, UniqueOnly);
            OutputSettings outputSettings = new OutputSettings(DisplayRight, DisplayWrong, NumberDisplayOnlyMode);
            return new Preset(Name, NumberOfAttemts, Pool, answerSettings, outputSettings);
        }
    }
}
