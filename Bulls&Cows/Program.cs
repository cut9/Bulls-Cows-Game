using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Xml.Linq;

Console.OutputEncoding = Encoding.UTF8;

BullsCows game = new();
game.Play();


class BullsCows
{
    private int _selectedDifficulty;
    private Difficulty _currentDifficulty;
    private List<Difficulty> _difficultyPresets = new()
    {
        // Базовые, дружелюбные уровни
        new Difficulty(name: "Новичок", minAllowed: 0, maxAllowed: 9, answerLength: 3, maxAttempts: 12, uniqueCharactersOnly: true, showRightRepetitions: false, showWrongRepetitions: false, showNumbersOnly: true),
        new Difficulty(name: "Лёгкий", minAllowed: 0, maxAllowed: 11, answerLength: 4, maxAttempts: 10, uniqueCharactersOnly: true, showRightRepetitions: false, showWrongRepetitions: false, showNumbersOnly: true),
        new Difficulty(name: "Классический", minAllowed: 0, maxAllowed: 9, answerLength: 4, maxAttempts: 8, uniqueCharactersOnly: true, showRightRepetitions: false, showWrongRepetitions: false, showNumbersOnly: true),
        // Пресеты для изучения букв/шестнадцатеричных символов
        new Difficulty(name: "Только буквы (A–H)", minAllowed: 10, maxAllowed: 17, answerLength: 4, maxAttempts: 10, uniqueCharactersOnly: true, showRightRepetitions: false, showWrongRepetitions: false, showNumbersOnly: false),
        new Difficulty(name: "Hex — начинающий", minAllowed: 10, maxAllowed: 15, answerLength: 4, maxAttempts: 9, uniqueCharactersOnly: true, showRightRepetitions: false, showWrongRepetitions: false, showNumbersOnly: false),
        new Difficulty(name: "Hex — эксперт", minAllowed: 0, maxAllowed: 15, answerLength: 6, maxAttempts: 10, uniqueCharactersOnly: true, showRightRepetitions: true, showWrongRepetitions: true, showNumbersOnly: false),
        // Для тренировки памяти (много повторов разрешено)
        new Difficulty(name: "Память: короткая", minAllowed: 0, maxAllowed: 7, answerLength: 6, maxAttempts: 10, uniqueCharactersOnly: false, showRightRepetitions: true, showWrongRepetitions: true, showNumbersOnly: false),
        new Difficulty(name: "Память: длинная", minAllowed: 0, maxAllowed: 11, answerLength: 8, maxAttempts: 14, uniqueCharactersOnly: false, showRightRepetitions: true, showWrongRepetitions: true, showNumbersOnly: false),
        // Случайные / хаотичные режимы
        new Difficulty(name: "Хаос", minAllowed: 0, maxAllowed: 17, answerLength: 6, maxAttempts: 6, uniqueCharactersOnly: false, showRightRepetitions: false, showWrongRepetitions: true, showNumbersOnly: false),
        new Difficulty(name: "Хардкор", minAllowed: 0, maxAllowed: 17, answerLength: 8, maxAttempts: 7, uniqueCharactersOnly: true, showRightRepetitions: true, showWrongRepetitions: true, showNumbersOnly: false),
        new Difficulty(name: "Невозможно", minAllowed: 0, maxAllowed: 17, answerLength: 10, maxAttempts: 6, uniqueCharactersOnly: true, showRightRepetitions: true, showWrongRepetitions: true, showNumbersOnly: false),
        // Узконаправленные задачи — сжатый алфавит
        new Difficulty(name: "Мини-алфавит", minAllowed: 0, maxAllowed: 3, answerLength: 3, maxAttempts: 8, uniqueCharactersOnly: true, showRightRepetitions: false, showWrongRepetitions: false, showNumbersOnly: true),
        new Difficulty(name: "Дуэль (2 символа)", minAllowed: 0, maxAllowed: 1, answerLength: 4, maxAttempts: 10, uniqueCharactersOnly: false, showRightRepetitions: true, showWrongRepetitions: true, showNumbersOnly: true),
        new Difficulty(name: "Пазл (узкий диапазон)", minAllowed: 0, maxAllowed: 5, answerLength: 6, maxAttempts: 9, uniqueCharactersOnly: false, showRightRepetitions: true, showWrongRepetitions: true, showNumbersOnly: false),
        // Режимы для ускорения/соревнований
        new Difficulty(name: "Спринт", minAllowed: 0, maxAllowed: 11, answerLength: 4, maxAttempts: 4, uniqueCharactersOnly: true, showRightRepetitions: false, showWrongRepetitions: false, showNumbersOnly: true),
        new Difficulty(name: "Марафон", minAllowed: 0, maxAllowed: 15, answerLength: 6, maxAttempts: 25, uniqueCharactersOnly: true, showRightRepetitions: false, showWrongRepetitions: false, showNumbersOnly: true),
        // Комбо- и обучающие режимы
        new Difficulty(name: "Детектив", minAllowed: 0, maxAllowed: 13, answerLength: 5, maxAttempts: 9, uniqueCharactersOnly: false, showRightRepetitions: true, showWrongRepetitions: true, showNumbersOnly: false),
        new Difficulty(name: "Учебный: только цифры", minAllowed: 0, maxAllowed: 9, answerLength: 5, maxAttempts: 12, uniqueCharactersOnly: false, showRightRepetitions: false, showWrongRepetitions: false, showNumbersOnly: true),
        new Difficulty(name: "Учебный: без повторов", minAllowed: 0, maxAllowed: 15, answerLength: 6, maxAttempts: 12, uniqueCharactersOnly: true, showRightRepetitions: false, showWrongRepetitions: false, showNumbersOnly: false),
        // Развлекательные, «экспериментальные" пресеты
        new Difficulty(name: "Одна из каждой (все символы)", minAllowed: 0, maxAllowed: 17, answerLength: 18, maxAttempts: 30, uniqueCharactersOnly: true, showRightRepetitions: false, showWrongRepetitions: false, showNumbersOnly: false),
        new Difficulty(name: "Сюрприз", minAllowed: 3, maxAllowed: 12, answerLength: 5, maxAttempts: 7, uniqueCharactersOnly: false, showRightRepetitions: true, showWrongRepetitions: true, showNumbersOnly: false),
        // Кастомная сложность
        new Difficulty(name: "Custom", minAllowed: 1, maxAllowed: 10, answerLength: 5, maxAttempts: 5, uniqueCharactersOnly: false, showRightRepetitions: true, showWrongRepetitions: true, showNumbersOnly: false),
    };

    private List<char> _answer = new();
    public BullsCows()
    {
        _selectedDifficulty = _difficultyPresets.Count - 1;
        _currentDifficulty = _difficultyPresets[_selectedDifficulty];
        LoadSettings();
    }
    private readonly List<char> _possibleChars = new()
    {
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H'
    };
    private List<ConsoleColor> _guessColors = new()
    {
        ConsoleColor.DarkGray, //False
        ConsoleColor.Green, //True
        ConsoleColor.Cyan, //Several True
        ConsoleColor.Red, //True Wrong Position
        ConsoleColor.Magenta //Several True Wrong Position
    };
    private Random _rand = new Random();
    public void Play()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine(@"[1]. Начать игру
[2]. Настройки
[3]. ""Как играть?""
[4]. Выход");
            int userInput = Choice(1, 4);
            switch (userInput)
            {
                case 1:
                    StartGame();
                    break;
                case 2:
                    ShowSettings();
                    break;
                case 3:
                    HowToPlay();
                    break;
                case 4:
                    return;
                default:
                    break;
            }
            Console.WriteLine("Нажмите любую клавишу что бы продолжить...");
            Console.ReadKey();
        }
    }
    private int Choice(int min, int max)
    {
        while (true)
        {
            string userTextInput = Console.ReadLine();
            if (int.TryParse(userTextInput, out int userNumberInput) && (userNumberInput >= min && userNumberInput <= max))
            {
                Console.Clear();
                return userNumberInput;
            }
            else
            {
                Console.WriteLine("Некорректное значение!");
            }
        }
    }
    private void ShowSettings()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("------------- Правила! ------------");
            Console.WriteLine(@$"Пресет сложности: {_currentDifficulty.Name}
Число попыток: {_currentDifficulty.MaxAttempts}
Длина загаданного значения: {_currentDifficulty.AnswerLength}
Нижняя граница диапазона: {_currentDifficulty.MinAllowed}
Верхняя граница диапазона: {_currentDifficulty.MaxAllowed}");
            for (int i = 0; i < _possibleChars.Count; i++)
            {
                if (i < _currentDifficulty.MinAllowed || i > _currentDifficulty.MaxAllowed)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                }
                Console.Write(_possibleChars[i]);
                Console.ResetColor();
                Console.Write(" ");
            }
            Console.WriteLine("\n--------- Особые правила! ---------");
            Console.WriteLine(@$"Только уникальные значения: {_currentDifficulty.UniqueCharactersOnly}
Индикатор повторений правильностоящих значений: {_currentDifficulty.ShowRightRepetitions}
Индикатор повторений НЕ правильностоящих значений: {_currentDifficulty.ShowWrongRepetitions}
Только цифры в подсказках: {_currentDifficulty.ShowNumbersOnly}");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine(@"[1]. Изменить настройки
[2]. Список уровней сложности
[3]. Выбрать сложность
[4]. Изменить кастомную сложность
[5]. Назад");
            int userInput = Choice(1, 5);
            switch (userInput)
            {
                case 1:
                    break;
                case 2:
                    ShowDifficultyList();
                    break;
                case 3:
                    ChangeDifficulty();
                    break;
                case 4:
                    ChangeCustumDifficulty();
                    break;
                case 5:
                    return;
            }
        }
    }
    public void ChangeCustumDifficulty()
    {
        Console.Clear();
        Console.WriteLine($"Введите новые значения\nТекущее название: {_difficultyPresets[_difficultyPresets.Count - 1].Name}");
        _difficultyPresets[_difficultyPresets.Count - 1].Name = Console.ReadLine();
        Console.WriteLine($"Текущая длина загаданного значения: {_difficultyPresets[_difficultyPresets.Count - 1].AnswerLength} (1-16)");
        _difficultyPresets[_difficultyPresets.Count - 1].AnswerLength = Choice(1, 16);
        Console.WriteLine($"Текущee количество попыток: {_difficultyPresets[_difficultyPresets.Count - 1].MaxAttempts} (1-64)");
        _difficultyPresets[_difficultyPresets.Count - 1].MaxAttempts = Choice(1, 64);
        Console.WriteLine($"Нижняя граница диапазона: {_difficultyPresets[_difficultyPresets.Count - 1].MinAllowed} (0-{_possibleChars.Count - 1})");
        _difficultyPresets[_difficultyPresets.Count - 1].MinAllowed = Choice(0, _possibleChars.Count - 1);
        Console.WriteLine($"Верхняя граница диапазона: {_difficultyPresets[_difficultyPresets.Count - 1].MaxAllowed} (нижняя-{_possibleChars.Count-1})");
        _difficultyPresets[_difficultyPresets.Count - 1].MaxAllowed = Choice(_difficultyPresets[_difficultyPresets.Count - 1].MinAllowed, _possibleChars.Count - 1);
        Console.WriteLine($"Текущее значение только уникальные символы: {_difficultyPresets[_difficultyPresets.Count - 1].UniqueCharactersOnly} (1 да, 0 нет)");
        _difficultyPresets[_difficultyPresets.Count - 1].UniqueCharactersOnly = Convert.ToBoolean(Choice(0, 1));
        Console.WriteLine($"Текущее значение показывать индикатор верностоящих: {_difficultyPresets[_difficultyPresets.Count - 1].ShowRightRepetitions} (1 да, 0 нет)");
        _difficultyPresets[_difficultyPresets.Count - 1].ShowRightRepetitions = Convert.ToBoolean(Choice(0, 1));
        Console.WriteLine($"Текущее значение показывать индикатор не верностоящих: {_difficultyPresets[_difficultyPresets.Count - 1].ShowWrongRepetitions} (1 да, 0 нет)");
        _difficultyPresets[_difficultyPresets.Count - 1].ShowWrongRepetitions = Convert.ToBoolean(Choice(0, 1));
        Console.WriteLine($"Текущее значение только цифры: {_difficultyPresets[_difficultyPresets.Count - 1].ShowNumbersOnly} (1 да, 0 нет)");
        _difficultyPresets[_difficultyPresets.Count - 1].ShowNumbersOnly = Convert.ToBoolean(Choice(0, 1));
        Console.WriteLine("Данные успешно изменены!");
        _selectedDifficulty = _difficultyPresets.Count - 1;
        _currentDifficulty = _difficultyPresets[_selectedDifficulty];
        SaveSettings();
    }

    private void ChangeDifficulty()
    {
        Console.WriteLine($"Выберите сложность");
        for (int i = 0; i < _difficultyPresets.Count; i++)
        {
            Console.WriteLine($"[{i + 1}]. {_difficultyPresets[i].Name}");
        }
        Console.WriteLine($"[{_difficultyPresets.Count + 1}]. Назад");
        int userChoice = Choice(1, _difficultyPresets.Count + 1) - 1;
        if (userChoice == _difficultyPresets.Count) 
            return;
        _selectedDifficulty = userChoice;
        _currentDifficulty = _difficultyPresets[_selectedDifficulty];
        SaveSettings();
    }
    private void ShowDifficultyList()
    {
        foreach (var item in _difficultyPresets)
        {
            Console.WriteLine($"-----------------------------------\n{item.Name} Попытки {item.MaxAttempts}, длина {item.AnswerLength}");
            for (int i = 0; i < _possibleChars.Count; i++)
            {
                if (i < item.MinAllowed || i > item.MaxAllowed)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                }
                Console.Write(_possibleChars[i]);
                Console.ResetColor();
                Console.Write(" ");
            }
            Console.WriteLine($"\nУник.:{item.UniqueCharactersOnly}, прав.:{item.ShowRightRepetitions}, не прав.:{item.ShowWrongRepetitions}, только цифры:{item.ShowNumbersOnly}");
        }
        Console.WriteLine(value: "-----------------------------------\nНажмите любую клавишу что бы продолжить...");
        Console.ReadKey();
    }
    private void HowToPlay()
    {
        Console.WriteLine(@"Компьютер загадал последовательность символов.
Ваша задача за определённое количество ходов
узнать какую последовательность загадал компьютер. 
Для этого используйте подсказки:");
        WriteColored("[Такого символа нет в последовательности]\n", _guessColors[0]);
        WriteColored("[Такой символ есть и он стоит на своём месте]\n", _guessColors[1]);
        WriteColored("[Такой символ есть но он стоит не на своём месте]\n", _guessColors[3]);
        WriteColored("[Таких символов несколько и этот стоит на своём месте]\n", _guessColors[2]);
        WriteColored("[Таких символов несколько и этот не стоит на своём месте]\n", _guessColors[4]);
        Console.WriteLine("Что бы узнать дополнительные условия, проверьте настройки.");
    }
    private void WriteColored(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ResetColor();
    }
    private void StartGame()
    {
        AnswerGenerate();
        if (UserGuess())
        {
            Console.WriteLine(value: "Поздравляю, вы победили!\nЕщё одна великая победа на ваш счёт!");
        }
        else
        {
            Console.WriteLine(value: "Попытки закончились.\nВ следующий раз точно получится!");
        }
        Console.Write("ans:");
        foreach (var item in _answer)
        {
            Console.Write($"[{item}]");
        }
        Console.WriteLine();
    }
    private void AnswerGenerate()
    {
        _answer.Clear();
        int min = _currentDifficulty.MinAllowed;
        int max = _currentDifficulty.MaxAllowed;
        int answerLength = _currentDifficulty.AnswerLength;
        if (min < 0) min = 0;
        if (max >= _possibleChars.Count) max = _possibleChars.Count - 1;
        int available = max - min + 1;
        if (_currentDifficulty.UniqueCharactersOnly && available < answerLength)
        {
            answerLength = Math.Min(answerLength, available);
        }
        while (_answer.Count < answerLength)
        {
            int idx = _rand.Next(min, max + 1);
            char candidate = _possibleChars[idx];
            if (!_currentDifficulty.UniqueCharactersOnly || !_answer.Contains(candidate))
            {
                _answer.Add(candidate);
            }
        }
    }
    private bool UserGuess()
    {
        int currentAttempts = 0;
        while (currentAttempts < _currentDifficulty.MaxAttempts)
        {
            Console.Write($"Попыток осталось {_currentDifficulty.MaxAttempts - currentAttempts}; Длина ответа {_currentDifficulty.AnswerLength}; Введите вашу догадку:\n    ");
            string? userInput = Console.ReadLine();
            if (string.IsNullOrEmpty(userInput))
                continue;
            List<char?> guess = userInput
                .ToUpper()
                .Select(c => (char?)c)
                .ToList();
            if (CheckMatches(guess))
                return true;
            currentAttempts++;
        }
        return false;
    }
    private bool CheckMatches(List<char?> guess)
    {
        int rightPositionGuessCount = 0;
        int rightPositionGuessSeveralCount = 0;
        int wrongPositionGuessCount = 0;
        int wrongPositionGuessSeveralCount = 0;
        int wrongGuessCount = 0;

        char symbolChar;
        ConsoleColor color = Console.ForegroundColor;

        int currentLine = Math.Max(0, Console.CursorTop - 1);
        Console.SetCursorPosition(0, currentLine);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, currentLine);

        Console.Write(value: "uIn:");
        for (int i = 0; i < _answer.Count; i++)
        {
            Console.Write($"[{(guess.Count > i ? guess[i] : " ")}]");
        }

        Console.Write(value: "\nout:");

        for (int i = 0; i < _answer.Count; i++)
        {
            char? g = i < guess.Count ? guess[i] : null;
            bool hasValue = g.HasValue;

            if (hasValue && guess[i] != ' ')
                symbolChar = '⊞';
            else
                symbolChar = ' ';

            bool rightPositionGuess = hasValue && _answer[i] == guess[i];
            bool rightPositionGuessSeveral = hasValue && rightPositionGuess && ContainsSeveral(guess[i]);
            bool wrongPositionGuess = hasValue && !rightPositionGuess && _answer.Contains((char)guess[i]);
            bool wrongPositionGuessSeveral = hasValue && ContainsSeveral(guess[i]);
            bool wrongGuess = hasValue && !rightPositionGuess && !wrongPositionGuess;

            if (wrongGuess)
            {
                color = _guessColors[index: 0];
                wrongGuessCount++;
            }

            if (wrongPositionGuess)
            {
                color = _guessColors[index: 3];
                wrongPositionGuessCount++;
            }

            if (wrongPositionGuessSeveral && _currentDifficulty.ShowWrongRepetitions)
            {
                color = _guessColors[index: 4];
                wrongPositionGuessSeveralCount++;
            }

            if (rightPositionGuess)
            {
                color = _guessColors[index: 1];
                rightPositionGuessCount++;
            }

            if (rightPositionGuessSeveral && _currentDifficulty.ShowRightRepetitions)
            {
                color = _guessColors[index: 2];
                rightPositionGuessSeveralCount++;
            }

            if (!_currentDifficulty.ShowNumbersOnly)
            {
                Console.Write("[");
                WriteColored(symbolChar.ToString(), color);
                Console.Write("]");
            }
        }
        if (_currentDifficulty.ShowNumbersOnly)
        {
            StringBuilder numbersGuesses = new StringBuilder();
            numbersGuesses.Append($"На своих позициях: {rightPositionGuessCount}; ");
            numbersGuesses.Append($"Не на своих позициях: {wrongPositionGuessCount}; ");
            Console.WriteLine(numbersGuesses);
        }
        if (rightPositionGuessCount == _answer.Count)
        {
            return true;
        }
        return false;
    }

    private bool ContainsSeveral(char? exceptValue)
    {
        if (!exceptValue.HasValue) return false;
        int count = 0;
        for (int i = 0; i < _answer.Count; i++)
            if (_answer[i] == exceptValue.Value) count++;
        return count > 1;
    }

    private class Difficulty
    {
        public string Name { get; set; }
        public int MinAllowed { get; set; }
        public int MaxAllowed { get; set; }
        public int AnswerLength { get; set; }
        public int MaxAttempts { get; set; }
        public bool UniqueCharactersOnly { get; set; }
        public bool ShowRightRepetitions { get; set; }
        public bool ShowWrongRepetitions { get; set; }
        public bool ShowNumbersOnly { get; set; }

        public Difficulty(string name = "Place Holder Name", int minAllowed = 0, int maxAllowed = 17,
                          int answerLength = 1, int maxAttempts = 1, bool uniqueCharactersOnly = false,
                          bool showRightRepetitions = false, bool showWrongRepetitions = false,
                          bool showNumbersOnly = false)
        {
            Name = name;
            MinAllowed = minAllowed;
            MaxAllowed = maxAllowed;
            AnswerLength = answerLength;
            MaxAttempts = maxAttempts;
            UniqueCharactersOnly = uniqueCharactersOnly;
            ShowRightRepetitions = showRightRepetitions;
            ShowWrongRepetitions = showWrongRepetitions;
            ShowNumbersOnly = showNumbersOnly;
        }
    }

    private class SettingsDto
    {
        public int SelectedDifficultyIndex { get; set; }
        public Difficulty CustomDifficulty { get; set; }
    }
    private void SaveSettings()
    {
        JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
        };

        string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "cut9", "BullsAndCows");
        Directory.CreateDirectory(dir);
        string configPath = Path.Combine(dir, "Settings.json");

        var dto = new SettingsDto
        {
            SelectedDifficultyIndex = _selectedDifficulty,
            CustomDifficulty = _difficultyPresets[_difficultyPresets.Count - 1]
        };

        string json = JsonSerializer.Serialize(dto, JsonOptions);
        File.WriteAllText(configPath, json);
    }

    private void LoadSettings()
    {
        try
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "cut9", "BullsAndCows");
            string configPath = Path.Combine(dir, "Settings.json");

            if (!File.Exists(configPath))
            {
                SaveSettings();
                return;
            }

            string jsonContent = File.ReadAllText(configPath);
            var loaded = JsonSerializer.Deserialize<SettingsDto>(jsonContent);

            if (loaded?.CustomDifficulty != null && ValidateDifficulty(loaded.CustomDifficulty))
            {
                _difficultyPresets[_difficultyPresets.Count - 1] = loaded.CustomDifficulty;
                _selectedDifficulty = Math.Clamp(loaded.SelectedDifficultyIndex, 0, _difficultyPresets.Count - 1);
                _currentDifficulty = _difficultyPresets[_selectedDifficulty];
            }
            else
            {
                Console.WriteLine("Конфиг повреждён или некорректен, использую дефолтные значения.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке настроек: {ex.Message}");
        }
    }

    private bool ValidateDifficulty(Difficulty d)
    {
        if (d == null) return false;
        if (d.MinAllowed < 0 || d.MinAllowed >= _possibleChars.Count) return false;
        if (d.MaxAllowed < d.MinAllowed || d.MaxAllowed >= _possibleChars.Count) return false;
        if (d.AnswerLength <= 0 || d.AnswerLength > 64) return false;
        // булевы поля в валидаторах не нужны — они уже bool
        return true;
    }

}