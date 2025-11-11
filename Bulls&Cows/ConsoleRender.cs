using System.Drawing;

namespace Bulls_Cows
{
    internal class ConsoleRender : IRender
    {
        private const int _unselectedOptionIndent = 2;
        private const int _selectedOptionIndent = 1;

        private const int _inDisplayLeft = _poolDisplayWidth + 1;
        private const int _inDisplayTop = 2;
        private const int _outDisplayLeft = _poolDisplayWidth + 1;
        private const int _outDisplayTop = 3;
        private const int _poolDisplayLeft = 1;
        private const int _poolDisplayTop = 1;
        private const int _poolDisplayWidth = 14;
        private const int _poolDisplayColumnWidth = 2;
        private const int _slotDisplayWidth = 3;
        private const int _userRightGuessHistoryDisplayLeft = _poolDisplayWidth + 1;
        private const int _userRightGuessHistoryDisplayTop = 4;
        private const int _userWrongGuessHistoryDisplayLeft = _poolDisplayWidth + 1;
        private const int _userWrongGuessHistoryDisplayTop = 5;
        private const int _attemptDisplayLeft = _poolDisplayWidth + 1;
        private const int _attemptDisplayTop = 1;

        private const int _minLeftWidth = 8;
        private const int _minTopWidth = 6;

        private int _menuYBuffer = 0;
        private int _historyYBuffer = 0;

        private const ConsoleColor _unselectedOptionColor = ConsoleColor.Gray;
        private const ConsoleColor _selectedOptionColor = ConsoleColor.Yellow;
        private Dictionary<OutputCharStates, ConsoleColor> _outputColors = new();
        private readonly List<ConsoleColor> _charColor = new()
        {
            ConsoleColor.DarkGray, ConsoleColor.Green, ConsoleColor.Red, ConsoleColor.Cyan, ConsoleColor.Magenta
        };

        public ConsoleRender()
        {
            Console.CursorVisible = false;
        }

        public void SetCharStates(List<OutputCharStates> states)
        {
            _outputColors = states
                .Zip(_charColor, (state, color) => new { state, color })
                .ToDictionary(x => x.state, x => x.color);
        }

        public void StateSetColor(OutputCharStates state, ref int chose)
        {
            int Length = Enum.GetValues(typeof(ConsoleColor)).Length;
            chose = Math.Clamp(chose, -1, Length);
            if (chose == -1)
                chose = Length - 1;
            if (chose == Length)
                chose = 0;
            _outputColors[state] = (ConsoleColor)chose;
        }

        public int StateGetColor(OutputCharStates state) => (int)_outputColors[state];

        public void ShowAllStateColors(int selected = -1)
        {
            Console.SetCursorPosition(0, 0);
            int currentIndent;
            int index = 0;

            foreach (var item in _outputColors)
            {
                currentIndent = _unselectedOptionIndent;

                if (index == selected)
                    currentIndent = _selectedOptionIndent;

                string StateColor = $"{new string(' ', currentIndent)}{item.Key.ToString()}{new string(' ', currentIndent)}";
                WriteColored(StateColor + "\n", item.Value);
                index++;
            }
        }

        public void DisplayHowToPlay()
        {
            Console.WriteLine(@"Ваша задача, узнать секретную последовательность символов за определённое количество ходов.
Символы которые могут быть в последовательности изображены в левой части интерфейса.
Сверху расположен счётчик ваших попыток, под ним поле для ввода вашей догадки.
Строка под полем это Ваш последний ввод, ниже расположена история ваших вводов.");
            WriteColored("Если символа нет в последовательности он отображается этим цветом.\n", _outputColors[OutputCharStates.None]);
            WriteColored("Если символ есть в последовательности и в Вашей догадке он находится в правильной позиции он отображается этим цветом.\n", _outputColors[OutputCharStates.Right]);
            WriteColored("Если же символ находится в НЕ правильной позиции, Вы увидите этот цвет.\n", _outputColors[OutputCharStates.Wrong]);
            Console.WriteLine(@"Так же есть дополнительные настройки, а именно:
= Unique Only =
  Если данная настройка включена, последовательность НЕ будет содержать одинаковые символы (Например 112223333)
= Numbers Only =
  Если данная настройка включена, вместо цветных подсказок Вы узнаете только количество правильно и не правильно стоящих символов в Вашей догадке.
= Отображение повтора правильно стоящих символов =");
            WriteColored("  Если эта настройка включена и указанный Вами символ стоит в правильной позиции, Вы увидите есть ли в данной последовательности ещё такой же символ.\n", _outputColors[OutputCharStates.SeveralRight]);
            Console.WriteLine("= Отображение повтора НЕ правильных стоящих символов =");
            WriteColored("  Если эта настройка включена и указанный Вами символ стоит в НЕ правильной позиции, Вы увидите есть ли в данной последовательности ещё такой же символ.\n", _outputColors[OutputCharStates.SeveralWrong]);
            Console.WriteLine(@"(Все цвета Вы можете изменить в настройках)
Например, последовательность:
117273
Вы вводите:
177231
Если последние две настройки выключены (false), игра покажет Вам, такой вывод:");
            WriteColored("1", _outputColors[OutputCharStates.Right]); WriteColored("7", _outputColors[OutputCharStates.Wrong]);
            WriteColored("7", _outputColors[OutputCharStates.Right]); WriteColored("2", _outputColors[OutputCharStates.Right]);
            WriteColored("3", _outputColors[OutputCharStates.Wrong]); WriteColored("1", _outputColors[OutputCharStates.Wrong]);
            Console.WriteLine("\nЕсли же две последние настройки включены (true), Вы увидите данный вывод:");
            WriteColored("1", _outputColors[OutputCharStates.SeveralRight]); WriteColored("7", _outputColors[OutputCharStates.SeveralWrong]);
            WriteColored("7", _outputColors[OutputCharStates.SeveralRight]); WriteColored("2", _outputColors[OutputCharStates.Right]);
            WriteColored("3", _outputColors[OutputCharStates.Wrong]); WriteColored("1", _outputColors[OutputCharStates.SeveralWrong]);
        }

        public void DisplayFrame(int poolLength, CellInfo[] slot)
        {
            Point _frameFirstDot = new Point(0, 0);
            Point _frameLastDot = new Point();
            int frameYBuffer = 1;
            int itemsPerRow = Math.Max(1, _poolDisplayWidth / _poolDisplayColumnWidth);
            int poolRows = (int)Math.Ceiling(poolLength / (double)itemsPerRow);

            _frameLastDot.Y = Math.Max(_minTopWidth, Math.Max(poolRows, _userWrongGuessHistoryDisplayTop + _historyYBuffer + frameYBuffer));
            _frameLastDot.X = _minLeftWidth + _attemptDisplayLeft + slot.Length * _slotDisplayWidth;

            SetCursorWrite(0, _frameLastDot.Y - 1, new string(' ', Math.Max(0, _frameLastDot.X)));

            DrawFrame(_frameFirstDot, _frameLastDot);

            for (int y = 1; y < _frameLastDot.Y; y++)
                SetCursorWrite(_poolDisplayWidth, y, "║");
            SetCursorWrite(_poolDisplayWidth, 0, "╦");
            SetCursorWrite(_poolDisplayWidth, _frameLastDot.Y, "╩");
        }

        private void DrawFrame(Point p1, Point p2)
        {
            if (p2.X >= 2)
            {
                var topLine = "╔" + new string('═', Math.Max(0, p2.X - p1.X - 1)) + "╗";
                SetCursorWrite(p1.X, p1.Y, topLine);

                var bottomLine = "╚" + new string('═', Math.Max(0, p2.X - p1.X - 1)) + "╝";
                SetCursorWrite(p1.X, p2.Y, bottomLine);
            }
            for (int y = p1.Y + 1; y < p2.Y; y++)
            {
                SetCursorWrite(p1.X, y, "║");
                SetCursorWrite(p2.X, y, "║");
            }
        }

        private void SetCursorWrite(int left, int top, string text)
        {
            try
            {
                if (left < 0 || top < 0) return;
                int neededWidth = left + text.Length;
                if (Console.BufferWidth < neededWidth) Console.SetBufferSize(Math.Max(Console.BufferWidth, neededWidth), Console.BufferHeight);
                if (Console.BufferHeight < top) Console.SetBufferSize(Console.BufferWidth, Math.Max(Console.BufferHeight, top + 1));
                Console.SetCursorPosition(left, top);
                Console.Write(text);
                _menuYBuffer = Math.Max(_menuYBuffer, top + 1);
            }
            catch (ArgumentOutOfRangeException)
            {

            }
        }

        public List<int> SaveColors()
        {
            return _outputColors.Values.Select(n => (int)n).ToList();
        }

        public void DisplayMenu(ContextMenu contextMenu)
        {
            int currentIndent;
            int topParallax = _menuYBuffer;
            for (int i = 0 + topParallax; i < contextMenu.ContextMenuOptions.Count + topParallax; i++)
            {
                currentIndent = _unselectedOptionIndent;
                var color = _unselectedOptionColor;
                if (i == contextMenu.SelectedOption + topParallax)
                {
                    color = _selectedOptionColor;
                    currentIndent = _selectedOptionIndent;
                }
                string menuOption = $"{new string(' ', currentIndent)}{contextMenu.ContextMenuOptions[i - topParallax].Option}{new string(' ', currentIndent)}";
                Console.SetCursorPosition(0, i);
                WriteColored(menuOption, color);
            }
            _menuYBuffer = topParallax;
        }
        public void DisplayIn(Preset preset, char[] userInput, int pos)
        {
            Console.SetCursorPosition(_inDisplayLeft, _inDisplayTop);
            Console.Write("=Input=:");
            for (int i = 0; i < preset.AnswerSettings.MaxAnswerLength; i++)
            {
                Console.ForegroundColor = _unselectedOptionColor;
                if (i == pos)
                    Console.ForegroundColor = _selectedOptionColor;
                Console.Write($"[{(userInput[i] == '\0' ? ' ' : userInput[i])}]");
            }
            Console.ResetColor();
        }

        public void DisplayOut(OutputCharInfo[] matchesInfo, OutputSettings outputSettings)
        {
            Console.SetCursorPosition(_outDisplayLeft, _outDisplayTop);
            Console.Write("=LastIn:");
            foreach (var ci in matchesInfo)
                WriteColored($"[{ci.Char}]", GetCharColor(ci, outputSettings));
        }

        private void WriteColored(string text, ConsoleColor color)
        {
            var prev = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = color;
                Console.Write(text);
            }
            finally
            {
                Console.ForegroundColor = prev;
            }
        }

        public void DisplayCurrentPool(List<char> guessPool, int poolBaseLength)
        {
            Point poolPlace = new Point(_poolDisplayLeft, _poolDisplayTop);
            SetCursorWrite(poolPlace.X, poolPlace.Y, "--- Chars ---");
            poolPlace.Y++;
            int itemsPerRow = Math.Max(1, _poolDisplayWidth / _poolDisplayColumnWidth);
            int rowsNeeded = (int)Math.Ceiling(poolBaseLength / (double)itemsPerRow);

            for (int r = 0; r < rowsNeeded; r++)
            {
                int y = poolPlace.Y + r;
                if (y >= 0)
                {
                    SetCursorWrite(poolPlace.X, y, new string(' ', Math.Max(0, _poolDisplayWidth - 1)));
                }
            }

            DwarPool(poolPlace, guessPool, _poolDisplayWidth);
        }

        private void DwarPool(Point poolPlace, List<char> guessPool, int PoolDisplayWidth)
        {
            int itemsPerRow = Math.Max(1, PoolDisplayWidth / _poolDisplayColumnWidth);
            for (int i = 0 + poolPlace.X; i < guessPool.Count + poolPlace.X; i++)
            {
                int row = (i - poolPlace.X) / itemsPerRow;
                int col = (i - poolPlace.X) % itemsPerRow;
                int x = poolPlace.X + col * _poolDisplayColumnWidth;
                int y = poolPlace.Y + row;
                SetCursorWrite(x, y, $"{guessPool[i - poolPlace.X]}");
            }
        }

        public void DisplayAttempts(int CurrentAttempt, int NumberOfAttempts)
        {
            Console.SetCursorPosition(_attemptDisplayLeft, _attemptDisplayTop);
            Console.Write($"Attempt: {CurrentAttempt} of {NumberOfAttempts}");
        }

        public void DisplayRightGuessHistory(CellInfo[] slot, OutputSettings outputSettings)
        {
            Console.SetCursorPosition(_userRightGuessHistoryDisplayLeft, _userRightGuessHistoryDisplayTop);
            Console.Write("=Answer:");
            int contentLeft = _userRightGuessHistoryDisplayLeft + _minLeftWidth;
            for (int i = 0; i < slot.Length; i++)
            {
                var cs = slot[i];
                int colLeft = contentLeft + i * _slotDisplayWidth;
                int colTop = _userRightGuessHistoryDisplayTop;
                Console.SetCursorPosition(colLeft, colTop);
                WriteColored($"[{(cs.Slot.Char == '\0' ? ' ' : cs.Slot.Char)}]", GetCharColor(cs.Slot, outputSettings));
            }
        }

        public void DisplayWrongGuessHistory(CellInfo[] slot, OutputSettings outputSettings)
        {
            Console.SetCursorPosition(_userWrongGuessHistoryDisplayLeft, _userWrongGuessHistoryDisplayTop);
            Console.Write("History:");
            int contentLeft = _userWrongGuessHistoryDisplayLeft + _minLeftWidth;

            for (int i = 0; i < slot.Length; i++)
            {
                var cs = slot[i];
                if (cs.WrongGuesses == null || cs.Slot.Char != '\0')
                    continue;
                for (int j = 0; j < cs.WrongGuesses.Count; j++)
                {
                    int WrongGuesseCursorLeft = contentLeft + i * _slotDisplayWidth;
                    int WrongGuesseCursorTop = _userWrongGuessHistoryDisplayTop + j;
                    var WrongGuesses = cs.WrongGuesses.ToList();
                    Console.SetCursorPosition(WrongGuesseCursorLeft, WrongGuesseCursorTop);
                    WriteColored($" {WrongGuesses[j].Char} ", GetCharColor(WrongGuesses[j], outputSettings));
                }
                _historyYBuffer = Math.Max(_historyYBuffer, cs.WrongGuesses.Count);
            }
        }

        private ConsoleColor GetCharColor(OutputCharInfo ci, OutputSettings co)
        {
            OutputCharStates option = ci.Type switch
            {
                CharType.NotContained => OutputCharStates.None,
                CharType.RightPosition => (ci.Count > 1 && co.DisplayRightRepetitions)
                    ? OutputCharStates.SeveralRight
                    : OutputCharStates.Right,
                CharType.WrongPosition => (ci.Count > 1 && co.DisplayWrongRepetitions)
                    ? OutputCharStates.SeveralWrong
                    : OutputCharStates.Wrong,
                _ => OutputCharStates.None
            };

            if (!_outputColors.TryGetValue(option, out var color)) color = ConsoleColor.White;
            return color;
        }

        public void SceneTransition()
        {
            Console.Clear();
            Console.CursorVisible = false;
            _menuYBuffer = 0;
            _historyYBuffer = 0;
        }

        public void DisplaySettings(Preset preset, GameState _gameState, int selectedPresetIndex, int presetsCount)
        {
            Point point = new Point(1, 1);
            List<string> strings = new List<string>()
            {
                "------------ Настройки! -----------",
                $"Номер пресета: {selectedPresetIndex + 1}/{presetsCount}",
                $"Пресет: {preset.Name}",
                $"Длина значения: {preset.AnswerSettings.MaxAnswerLength}",
                $"Число попыток: {preset.NumberOfAttempts}",
                $"Только уникальные: {preset.AnswerSettings.UniqueOnly}",
                "----------- Отображение! ----------",
                $"Только цифры: {preset.OutputSettings.DisplayNumbersOnly}",
                $"Повторение правильных: {preset.OutputSettings.DisplayRightRepetitions}",
                $"Повторение НЕ правильных: {preset.OutputSettings.DisplayWrongRepetitions}",
                "-------- Общая статистика! --------",
                $"Победы: {_gameState.Win}",
                $"Проигрыши: {_gameState.Loss}"
            };
            for (int i = 0; i < strings.Count; i++, point.Y++)
            {
                SetCursorWrite(point.X, point.Y, strings[i]);
            }
            int Width = 36;
            Point poolPlace = new Point(38, 1);
            DrawFrame(new Point(0, 0), new Point(Width, point.Y));
            SetCursorWrite(poolPlace.X, poolPlace.Y, "---------- Пул символов! ----------");
            DwarPool(new Point(poolPlace.X, poolPlace.Y + 1), preset.CharsPool.ToList(), Width);
            int itemsPerRow = Math.Max(1, Width / _poolDisplayColumnWidth);
            int rowsNeeded = (int)Math.Ceiling(preset.CharsPool.Count / (double)itemsPerRow);
            DrawFrame(new Point(poolPlace.X - 1, poolPlace.Y - 1), new Point(poolPlace.X + Width - 1, rowsNeeded + 2));
        }

        public void DisplayWinScreen(int CurrentAttempt, int NumberOfAttempts)
        {
            SetCursorWrite(0, _menuYBuffer, $"Победа! Было потрачено попыток {CurrentAttempt} из {NumberOfAttempts}");
        }

        public void DisplayLossScreen(string Answer)
        {
            SetCursorWrite(0, _menuYBuffer, $"Поражение! Загаданное значение было \"{Answer}\"");
        }

        public void DisplayNumbersOnlyHistory(List<HistoryData> guessesHistory)
        {
            int top = _outDisplayTop;

            for (int i = guessesHistory.Count; i > 0; i--)
            {
                var History = guessesHistory[i - 1];
                SetCursorWrite(_outDisplayLeft, top, new string(' ', _outDisplayLeft));
                SetCursorWrite(_outDisplayLeft, top, $"R{History.RightCount}");
                SetCursorWrite(_outDisplayLeft + 4, top, $"W{History.WrongCount}");

                int left = _outDisplayLeft + _minLeftWidth;
                foreach (var ch in History.Guess)
                {
                    SetCursorWrite(left, top, $"[{ch}]");
                    left += _slotDisplayWidth;
                }
                top++;
                _historyYBuffer = Math.Max(_historyYBuffer, guessesHistory.Count - 2);
            }
        }

        public void DisplayLog(string Log)
        {
            SetCursorWrite(0, 0, Log);
        }

        public bool TryLoadSettings(List<int> colors)
        {
            var keys = _outputColors.Keys.ToList();
            int Length = Enum.GetValues(typeof(ConsoleColor)).Length;
            for (int i = 0; i < keys.Count; i++)
            {
                if (i > colors.Count)
                    return false;
                if (colors[i] < 0 || colors[i] > Length - 1)
                    return false;
                _outputColors[keys[i]] = (ConsoleColor)colors[i];
            }
            return true;
        }
    }
}
