using System.Text.Json;

namespace Bulls_Cows
{
    internal class GameLoader(string _mainPath, string _gameStatePath, string _presetStorePath, string _outputColorsPath) : IGameLoader
    {
        public bool TryLoadOutputColorSettings(IRender render)
        {
            string filePath = Path.Combine(_mainPath, _outputColorsPath);
            if (!File.Exists(filePath))
                return false;
            string jsonContent = File.ReadAllText(filePath);
            var loaded = JsonSerializer.Deserialize<List<int>>(jsonContent);
            if (loaded == null)
                return false;
            return render.TryLoadSettings(loaded);
        }

        public bool TryLoadGameState(GameState gameState)
        {
            string filePath = Path.Combine(_mainPath, _gameStatePath);
            if (!File.Exists(filePath))
                return false;
            string jsonContent = File.ReadAllText(path: filePath);
            var loaded = JsonSerializer.Deserialize<GameStateFile>(jsonContent);
            if (loaded == null)
                return false;
            gameState.Win = loaded.WinsCount;
            gameState.Loss = loaded.LoseCount;
            gameState.SelectedPresetIndex = loaded.SelectedIndex;
            return true;
        }
        public bool TryLoadPresetList(PresetStore presetStore, int _maxAnswerLength, int _maxAttemptNumber)
        {
            string filePath = Path.Combine(_mainPath, _presetStorePath);
            if (!File.Exists(filePath))
                return false;
            string jsonContent = File.ReadAllText(filePath);
            var loaded = JsonSerializer.Deserialize<List<Preset>>(jsonContent);
            if (loaded == null)
                return false;
            foreach (var preset in loaded)
            {
                bool answerLength = preset.AnswerSettings.MaxAnswerLength > _maxAnswerLength || preset.AnswerSettings.MaxAnswerLength < 1;
                bool numberOfAttempt = preset.NumberOfAttempts > _maxAttemptNumber || preset.NumberOfAttempts < 1;
                bool uniqueOnly = preset.AnswerSettings.UniqueOnly && preset.CharsPool.Count < preset.AnswerSettings.MaxAnswerLength;
                if (!(answerLength || numberOfAttempt || uniqueOnly))
                {
                    presetStore.AddOrUpdate(preset);
                }
            }
            if (presetStore.All.Count == 0)
                return false;
            return true;
        }
    }
}
