using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Bulls_Cows
{
    internal class GameSaver(string _mainPath, string _gameStatePath, string _presetStorePath, string _outputColorsPath) : IGameSaver
    {
        private JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
        };

        public void SaveOutputColorSettings(IRender render)
        {
            Directory.CreateDirectory(_mainPath);
            string filePath = Path.Combine(_mainPath, _outputColorsPath);
            List<int> colorList = render.SaveColors();
            string json = JsonSerializer.Serialize(colorList, JsonOptions);
            File.WriteAllText(filePath, json);
        }

        public void SaveGameState(GameState gameState)
        {
            Directory.CreateDirectory(_mainPath);
            string filePath = Path.Combine(_mainPath, _gameStatePath);
            var gameStateSave = new GameStateFile(gameState.SelectedPresetIndex, gameState.Win, gameState.Loss, gameState.LastDaily);
            string json = JsonSerializer.Serialize(gameStateSave, JsonOptions);
            File.WriteAllText(filePath, json);
        }

        public void SavePresetList(PresetStore presetStore)
        {
            Directory.CreateDirectory(_mainPath);
            string filePath = Path.Combine(_mainPath, _presetStorePath);
            string json = JsonSerializer.Serialize(presetStore.All, JsonOptions);
            File.WriteAllText(filePath, json);
        }
    }
}
