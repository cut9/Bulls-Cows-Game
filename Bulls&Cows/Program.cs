using Bulls_Cows;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Xml.Linq;

Console.OutputEncoding = Encoding.UTF8;

string _mainPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "cut9", "BullsAndCows");
const string _gameStatePath = "GameState.json";
const string _presetStorePath = "Presets.json";
const string _outputColorsPath = "OutputSettings.json";

IRender render = new ConsoleRender();
IInput input = new ConsoleInput();
IRandomProvider random = new SystemRandomProvider();
IAnswerGenerator answerGenerator = new AnswerGenerator();
IGameSaver saver = new GameSaver(_mainPath, _gameStatePath, _presetStorePath, _outputColorsPath);
IGameLoader loader = new GameLoader(_mainPath, _gameStatePath, _presetStorePath, _outputColorsPath);

GameEngine G = new GameEngine(render, input, random, answerGenerator, saver, loader);
G.Run();
