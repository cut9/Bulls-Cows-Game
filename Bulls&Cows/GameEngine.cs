namespace Bulls_Cows
{
    internal class GameEngine
    {
        private const int _mainContextMenuId = 0;
        private const int _settingsContextMenuId = 1;
        private const int _changePresetContextMenuId = 2;

        private const int _maxAnswerLength = 50;
        private const int _maxAttemptNumber = 256;

        private readonly List<OutputCharStates> _charStates = new List<OutputCharStates>()
        {
            OutputCharStates.None, OutputCharStates.Right, OutputCharStates.Wrong, OutputCharStates.SeveralRight, OutputCharStates.SeveralWrong
        };

        private IRender _render;
        private IInput _input;
        private IRandomProvider _randomProvider;
        private IAnswerGenerator _answerGenerator;
        private IGameSaver _gameSaver;
        private IGameLoader _gameLoader;
        private GameState _gameState;
        private PresetStore _presetStore;
        private MenuStore _menuStore;
        public GameEngine(IRender render, IInput input, IRandomProvider randomProvider, IAnswerGenerator answerGenerator, IGameSaver gameSaver, IGameLoader gameLoader)
        {
            _render = render;
            _render.SetCharStates(_charStates);
            _input = input;
            _gameSaver = gameSaver;
            _gameLoader = gameLoader;
            _randomProvider = randomProvider;
            _answerGenerator = answerGenerator;
            _gameState = new GameState();
            _presetStore = new PresetStore();
            _menuStore = new MenuStore();
            CreateGameMenus();

            if (!_gameLoader.TryLoadPresetList(_presetStore, _maxAnswerLength, _maxAttemptNumber) || _presetStore.All.Count == 0)
            {
                CreatePresets();
            }
            if (!_gameLoader.TryLoadGameState(_gameState) || _gameState.SelectedPresetIndex < 0 || _gameState.SelectedPresetIndex > _presetStore.All.Count - 1)
            {
                _gameState.SelectedPresetIndex = 0;
                _gameState.Win = 0;
                _gameState.Loss = 0;
            }
            _gameLoader.TryLoadOutputColorSettings(_render);
        }
        private void CreateGameMenus()
        {
            _menuStore.AddOrUpdate(new ContextMenu(new List<ContextMenuOption>()
            {
                new ContextMenuOption("Начать игру", Scene.Game),
                new ContextMenuOption("Настройки",Scene.Settings),
                new ContextMenuOption("Как играть?",Scene.HowToPlay),
                new ContextMenuOption("Дейли режим",Scene.DailyMode),
                new ContextMenuOption("Выход", Scene.Exit)
            }));
            _menuStore.AddOrUpdate(new ContextMenu(new List<ContextMenuOption>()
            {
                new ContextMenuOption("Настройки присетов", Scene.PresetChange),
                new ContextMenuOption("Настройки цвета", Scene.ChangeRenderColors),
                new ContextMenuOption("Назад", Scene.MainMenu)
            }));
            _menuStore.AddOrUpdate(new ContextMenu(new List<ContextMenuOption>()
            {
                new ContextMenuOption("Создать новый пресет", Scene.AddNewPreset),
                new ContextMenuOption("Выбрать пресет", Scene.PresetSelect),
                new ContextMenuOption("Изменить пресет", Scene.EditPreset),
                new ContextMenuOption("Удалить пресет", Scene.PresetDelete),
                new ContextMenuOption("Назад", Scene.Settings)
            }));
        }
        private bool _isRunning = true;
        public void Run()
        {
            _gameState.CurrentScene = Scene.MainMenu;
            while (_isRunning)
            {
                switch (_gameState.CurrentScene)
                {
                    case Scene.MainMenu: MainMenuScene(); break;
                    case Scene.Settings: SettingsScene(); break;
                    case Scene.AddNewPreset: AddNewPresetScene(); break;
                    case Scene.PresetChange: ChangePresetMenuScene(); break;
                    case Scene.ChangeRenderColors: ChangeRenderColorsScene(); break;
                    case Scene.HowToPlay: HowToPlayScene(); break;
                    case Scene.Game:
                        _gameState.SelectedPresetIndex = Math.Clamp(_gameState.SelectedPresetIndex, 0, _presetStore.All.Count - 1);
                        if (!_presetStore.TryGet(_gameState.SelectedPresetIndex, out Preset _currentPreset))
                        {
                            _gameState.CurrentScene = Scene.MainMenu;
                            continue;
                        }
                        GameScene(_currentPreset, _randomProvider); 
                        break;
                    case Scene.DailyMode:
                        if (_gameState.LastDaily == DailyRandomProvider.Seed)
                        {
                            _gameState.CurrentScene = Scene.MainMenu;
                            continue;
                        }
                        IRandomProvider DailyRng = new DailyRandomProvider();
                        GameScene(PresetGenerator.GeneratePreset(DailyRng), DailyRng);
                        _gameState.LastDaily = DailyRandomProvider.Seed;
                        _gameSaver.SaveGameState(_gameState);
                        break;
                    case Scene.Exit: _isRunning = false; continue;
                }
                _render.SceneTransition();
            }
        }
        private void HowToPlayScene()
        {
            _render.DisplayHowToPlay();
            _input.WaitUserInput();
            _gameState.CurrentScene = Scene.MainMenu;
        }
        private void AddNewPresetScene()
        {
            string Name = "";
            int AnswerLength = 0;
            int NumberOfAttemts = 0;
            bool UniqueOnly = false;
            bool NumberDisplayOnlyMode = false;
            bool DisplayRight = false;
            bool DisplayWrong = false;
            HashSet<char> Pool = new HashSet<char>();

            List<string> Logs = new List<string>()
            {
                "Введите название пресета ",
                $"Введите длину значения (не больше чем {_maxAnswerLength}) ",
                $"Введите количество попыток (не больше чем {_maxAttemptNumber}) ",
                "Значение содержит только уникальные символы? (true|false) ",
                "Режим только числа в подсказках? (true|false) ",
                "Показывать повторы правильностоящих элементов? (true|false) ",
                "Показывать повторы не правильностоящих элементов? (true|false) ",
            };

            int UserNumberInput;
            bool UserBoolInput;
            int minIntValue = 1;

            for (int i = 0; i < Logs.Count;)
            {
                _render.SceneTransition();
                _render.DisplayLog(Logs[i]);
                string UserStringInput = _input.GetUserString();
                if (UserStringInput == null || UserStringInput.Length < 1)
                {
                    _gameState.CurrentScene = Scene.PresetChange;
                    return;
                }
                switch (i)
                {
                    case 0:
                        Name = UserStringInput;
                        i++;
                        break;
                    case 1:
                        if (int.TryParse(UserStringInput, out UserNumberInput))
                        {
                            AnswerLength = Math.Clamp(UserNumberInput, minIntValue, _maxAnswerLength);
                            i++;
                        }
                        break;
                    case 2:
                        if (int.TryParse(UserStringInput, out UserNumberInput))
                        {
                            NumberOfAttemts = Math.Clamp(UserNumberInput, minIntValue, _maxAttemptNumber);
                            i++;
                        }
                        break;
                    case 3:
                        if (bool.TryParse(UserStringInput, out UserBoolInput))
                        {
                            UniqueOnly = UserBoolInput;
                            i++;
                            string PoolLogString = "Введите пул уникальных символов который будет использоваться при генерации значения \n";
                            if (UniqueOnly)
                                PoolLogString += $"(Минимум {AnswerLength} символов) \n";
                            Logs.Add(PoolLogString);
                        }
                        break;
                    case 4:
                        if (bool.TryParse(UserStringInput, out UserBoolInput))
                        {
                            NumberDisplayOnlyMode = UserBoolInput;
                            i++;
                            if (NumberDisplayOnlyMode || UniqueOnly)
                                i = Logs.Count - 1;
                        }
                        break;
                    case 5:
                        if (bool.TryParse(UserStringInput, out UserBoolInput))
                        {
                            DisplayRight = UserBoolInput;
                            i++;
                        }
                        break;
                    case 6:
                        if (bool.TryParse(UserStringInput, out UserBoolInput))
                        {
                            DisplayWrong = UserBoolInput;
                            i++;
                        }
                        break;
                    case 7:
                        Pool = UserStringInput.ToHashSet();
                        if (UniqueOnly && Pool.Count < AnswerLength)
                        {
                            Pool = null;
                            continue;
                        }
                        i++;
                        break;
                }
            }
            Preset userPreset = new Preset(Name, NumberOfAttemts, Pool,
        new AnswerSettings(AnswerLength, UniqueOnly),
        new OutputSettings(DisplayRight, DisplayWrong, NumberDisplayOnlyMode));
            _presetStore.AddOrUpdate(userPreset);
            _gameState.CurrentScene = Scene.PresetChange;
            _gameSaver.SavePresetList(_presetStore);
        }

        private void ChangeRenderColorsScene()
        {
            int stateIndex = 0;
            int colorIndex = _render.StateGetColor(_charStates[stateIndex]);

            while (true)
            {
                _render.ShowAllStateColors(stateIndex);
                var command = _input.GetCommand();

                switch (command)
                {
                    case GameCommand.Up:
                        stateIndex = Math.Clamp(stateIndex - 1, 0, _charStates.Count - 1);
                        colorIndex = _render.StateGetColor(_charStates[stateIndex]);
                        break;
                    case GameCommand.Down:
                        stateIndex = Math.Clamp(stateIndex + 1, 0, _charStates.Count - 1);
                        colorIndex = _render.StateGetColor(_charStates[stateIndex]);
                        break;
                    case GameCommand.Left: colorIndex--; break;
                    case GameCommand.Right: colorIndex++; break;
                    case GameCommand.Accept:
                    case GameCommand.Esc:
                        _gameSaver.SaveOutputColorSettings(_render);
                        _gameState.CurrentScene = Scene.Settings;
                        return;
                }
                _render.StateSetColor(_charStates[stateIndex], ref colorIndex);
            }
        }

        private void SettingsScene()
        {
            _menuStore.TryGet(_settingsContextMenuId, out var menu);
            menu.SelectedOptionReset();
            _gameState.SelectedPresetIndex = Math.Clamp(_gameState.SelectedPresetIndex, 0, _presetStore.All.Count - 1);

            while (true)
            {
                if (!_presetStore.TryGet(_gameState.SelectedPresetIndex, out Preset _currentPreset))
                {
                    _gameState.CurrentScene = Scene.MainMenu;
                    return;
                }

                _render.DisplaySettings(_currentPreset, _gameState, _gameState.SelectedPresetIndex, _presetStore.All.Count);
                _render.DisplayMenu(menu);
                var command = _input.GetCommand();

                switch (command)
                {
                    case GameCommand.Up: menu.SelectedOptionUp(); break;
                    case GameCommand.Down: menu.SelectedOptionDown(); break;
                    case GameCommand.Accept:
                        _gameState.CurrentScene = menu.ContextMenuOptions[menu.SelectedOption].GameScene;
                        return;
                    case GameCommand.Esc:
                        _gameState.CurrentScene = Scene.MainMenu;
                        return;
                }
            }
        }

        private void MainMenuScene()
        {
            _menuStore.TryGet(_mainContextMenuId, out var menu);
            menu.SelectedOptionReset();

            while (true)
            {
                _render.DisplayMenu(menu);
                var Command = _input.GetCommand();

                switch (Command)
                {
                    case GameCommand.Up: menu.SelectedOptionUp(); break;
                    case GameCommand.Down: menu.SelectedOptionDown(); break;
                    case GameCommand.Accept:
                        _gameState.CurrentScene = menu.ContextMenuOptions[menu.SelectedOption].GameScene;
                        return;
                }
            }
        }
        private void ChangePresetMenuScene()
        {
            int selectedPreset = Math.Clamp(_gameState.SelectedPresetIndex, 0, _presetStore.All.Count - 1);
            _menuStore.TryGet(_changePresetContextMenuId, out var menu);

            while (true)
            {
                if (!_presetStore.TryGet(selectedPreset, out Preset _currentPreset))
                {
                    _gameState.CurrentScene = Scene.MainMenu;
                    return;
                }

                _render.DisplaySettings(_currentPreset, _gameState, selectedPreset, _presetStore.All.Count);
                _render.DisplayMenu(menu);
                var Command = _input.GetCommand();

                switch (Command)
                {
                    case GameCommand.Up: menu.SelectedOptionUp(); break;
                    case GameCommand.Down: menu.SelectedOptionDown(); break;
                    case GameCommand.Left:
                        selectedPreset = Math.Clamp(selectedPreset - 1, -1, _presetStore.All.Count - 1);
                        if (selectedPreset == -1)
                            selectedPreset = _presetStore.All.Count - 1;
                        _render.SceneTransition();
                        break;
                    case GameCommand.Right:
                        selectedPreset = Math.Clamp(selectedPreset + 1, 0, _presetStore.All.Count);
                        if (selectedPreset == _presetStore.All.Count)
                            selectedPreset = 0;
                        _render.SceneTransition();
                        break;
                    case GameCommand.Accept:
                        switch (menu.ContextMenuOptions[menu.SelectedOption].GameScene)
                        {
                            case Scene.PresetSelect:
                                _gameState.CurrentScene = Scene.Settings;
                                _gameState.SelectedPresetIndex = selectedPreset;
                                _gameSaver.SaveGameState(_gameState);
                                return;
                            case Scene.PresetDelete:
                                _render.SceneTransition();
                                DeletePreset(selectedPreset);
                                return;
                            case Scene.EditPreset:
                                _render.SceneTransition();
                                EditPreset(selectedPreset);
                                return;
                            case Scene.AddNewPreset:
                                _gameState.CurrentScene = Scene.AddNewPreset;
                                return;
                            case Scene.Settings:
                                _gameState.CurrentScene = Scene.Settings;
                                return;
                        }
                        break;
                    case GameCommand.Esc:
                        _gameState.CurrentScene = Scene.Settings;
                        return;
                }
            }
        }
        private void EditPreset(int PreseteIndex)
        {
            if (!_presetStore.TryGet(PreseteIndex, out Preset selectedPreset))
                return;
            var menu = new ContextMenu(new List<ContextMenuOption>()
            {
                new ContextMenuOption("Имя пресета", Scene.None),
                new ContextMenuOption("Длина значения", Scene.None),
                new ContextMenuOption("Количество попыток", Scene.None),
                new ContextMenuOption("Уникальные значения", Scene.None),
                new ContextMenuOption("Только числа", Scene.None),
                new ContextMenuOption("Повторы правильностоящих элементов", Scene.None),
                new ContextMenuOption("Повторы НЕ правильностоящих элементов", Scene.None),
                new ContextMenuOption("Пул символов", Scene.None),
            });
            bool isOptionSelected = false;
            bool isPresetEdited = false;
            while (!isOptionSelected)
            {
                _render.DisplayMenu(menu);
                var command = _input.GetCommand();
                switch (command)
                {
                    case GameCommand.Up: menu.SelectedOptionUp(); break;
                    case GameCommand.Down: menu.SelectedOptionDown(); break;
                    case GameCommand.Accept: isOptionSelected = true; break;
                    case GameCommand.Esc: return;
                }
            }

            string Name = selectedPreset.Name;
            int AnswerLength = selectedPreset.AnswerSettings.MaxAnswerLength;
            int NumberOfAttemts = selectedPreset.NumberOfAttempts;
            bool UniqueOnly = selectedPreset.AnswerSettings.UniqueOnly;
            bool NumberDisplayOnlyMode = selectedPreset.OutputSettings.DisplayNumbersOnly;
            bool DisplayRight = selectedPreset.OutputSettings.DisplayRightRepetitions;
            bool DisplayWrong = selectedPreset.OutputSettings.DisplayWrongRepetitions;
            HashSet<char> Pool = selectedPreset.CharsPool;

            int minIntValue = 1;
            int UserNumberInput;
            bool UserBoolInput;
            while (!isPresetEdited)
            {
                _render.SceneTransition();
                _render.DisplayLog("Введите новое значение\n");
                string UserStringInput = _input.GetUserString();
                if (UserStringInput == null || UserStringInput.Length < 1)
                {
                    _gameState.CurrentScene = Scene.PresetChange;
                    return;
                }
                switch (menu.SelectedOption)
                {
                    case 0:
                        Name = UserStringInput;
                        isPresetEdited = true;
                        break;
                    case 1:
                        if (int.TryParse(UserStringInput, out UserNumberInput) && !(UniqueOnly && Pool.Count < UserNumberInput))
                        {
                            AnswerLength = Math.Clamp(UserNumberInput, minIntValue, _maxAnswerLength);
                            isPresetEdited = true;
                        }
                        break;
                    case 2:
                        if (int.TryParse(UserStringInput, out UserNumberInput))
                        {
                            NumberOfAttemts = Math.Clamp(UserNumberInput, minIntValue, _maxAttemptNumber);
                            isPresetEdited = true;
                        }
                        break;
                    case 3:
                        if (bool.TryParse(UserStringInput, out UserBoolInput) && !(UserBoolInput && Pool.Count < AnswerLength))
                        {
                            UniqueOnly = UserBoolInput;
                            isPresetEdited = true;
                        }
                        break;
                    case 4:
                        if (bool.TryParse(UserStringInput, out UserBoolInput))
                        {
                            NumberDisplayOnlyMode = UserBoolInput;
                            isPresetEdited = true;
                        }
                        break;
                    case 5:
                        if (bool.TryParse(UserStringInput, out UserBoolInput))
                        {
                            DisplayRight = UserBoolInput;
                            isPresetEdited = true;
                        }
                        break;
                    case 6:
                        if (bool.TryParse(UserStringInput, out UserBoolInput))
                        {
                            DisplayWrong = UserBoolInput;
                            isPresetEdited = true;
                        }
                        break;
                    case 7:
                        Pool = UserStringInput.ToHashSet();
                        if (UniqueOnly && Pool.Count < AnswerLength)
                        {
                            Pool = null;
                            continue;
                        }
                        isPresetEdited = true;
                        break;
                }
            }
            Preset editedVersion = new Preset(Name, NumberOfAttemts, Pool, new AnswerSettings(AnswerLength, UniqueOnly), new OutputSettings(DisplayRight, DisplayWrong, NumberDisplayOnlyMode));
            _presetStore.TryRemove(PreseteIndex);
            _presetStore.Insert(PreseteIndex, editedVersion);
            _gameSaver.SavePresetList(_presetStore);
        }
        private void DeletePreset(int PreseteIndex)
        {
            if (!_presetStore.TryGet(PreseteIndex, out Preset preset))
                return;
            _render.DisplayLog($"Вы уверены что хотите удалить пресет \"{preset.Name}\"?");
            if (_input.GetCommand() != GameCommand.Accept)
                return;
            _gameState.CurrentScene = Scene.PresetChange;
            if (_presetStore.All.Count < 2)
                return;
            if (_gameState.SelectedPresetIndex == PreseteIndex)
            {
                _gameState.SelectedPresetIndex = Math.Clamp(PreseteIndex, 0, _presetStore.All.Count - 1);
                _gameSaver.SaveGameState(_gameState);
            }
            _presetStore.TryRemove(PreseteIndex);
            _gameSaver.SavePresetList(_presetStore);
        }
        private void GameScene(Preset currentPreset, IRandomProvider randomProvider)
        {
            _answerGenerator.SetSettings(currentPreset.CharsPool, currentPreset.AnswerSettings, randomProvider);
            if (!_answerGenerator.TryGenerate(out string answer))
            {
                _gameState.CurrentScene = Scene.MainMenu;
                return;
            }

            bool isWin = false;
            _gameState.SetAnswer(answer);
            _gameState.ResetCurrentAttempt();
            List<char> guessPool = currentPreset.CharsPool.ToList();

            CellInfo[] slots = new CellInfo[currentPreset.AnswerSettings.MaxAnswerLength];
            for (int i = 0; i < slots.Length; i++)
                slots[i] = new CellInfo();

            List<HistoryData> guessesHistory = new List<HistoryData>();

            while (_gameState.CurrentAttempt <= currentPreset.NumberOfAttempts)
            {
                _render.DisplayFrame(guessPool.Count, slots);
                _render.DisplayCurrentPool(guessPool, currentPreset.CharsPool.Count);
                _render.DisplayAttempts(_gameState.CurrentAttempt, currentPreset.NumberOfAttempts);
                string userGuess = GetUserGuess(currentPreset, guessPool);

                if (userGuess == string.Empty)
                {
                    break;
                }
                OutputCharInfo[] matchesInfo = CalculateMatches(userGuess);

                if (!currentPreset.OutputSettings.DisplayNumbersOnly)
                {
                    _render.DisplayOut(matchesInfo, currentPreset.OutputSettings);
                    slots = UpdateOutputSlots(slots, matchesInfo, ref guessPool);
                    _render.DisplayRightGuessHistory(slots, currentPreset.OutputSettings);
                    _render.DisplayWrongGuessHistory(slots, currentPreset.OutputSettings);
                }
                else
                {
                    var R = matchesInfo.Where(n => n.Type == CharType.RightPosition);
                    var W = matchesInfo.Where(n => n.Type == CharType.WrongPosition);
                    if (R.Count() == 0 && W.Count() == 0)
                        UpdateOutputSlots(slots, matchesInfo, ref guessPool);

                    guessesHistory.Add(new HistoryData(userGuess, R.Count(), W.Count()));
                    _render.DisplayNumbersOnlyHistory(guessesHistory);
                }

                if (isWin = IsWin(matchesInfo))
                    break;
                _gameState.IncrementAttempt();
            }
            GameOver(isWin, currentPreset.NumberOfAttempts);
            _gameSaver.SaveGameState(_gameState);
            _gameState.CurrentScene = Scene.MainMenu;
        }
        public void GameOver(bool isWin, int NumberOfAttempts)
        {
            if (isWin)
            {
                _gameState.NewWin();
                _render.DisplayWinScreen(_gameState.CurrentAttempt, NumberOfAttempts);
            }
            else
            {
                _gameState.NewLoss();
                _render.DisplayLossScreen(_gameState.CurrentAnswer);
            }
            _input.WaitUserInput();
            _render.SceneTransition();
        }
        public bool IsWin(OutputCharInfo[] matchesInfo)
        {
            foreach (var c in matchesInfo)
                if (c.Type != CharType.RightPosition)
                    return false;
            return true;
        }
        private CellInfo[] UpdateOutputSlots(CellInfo[] slots, OutputCharInfo[] matchesInfo, ref List<char> guessPool)
        {
            for (int i = 0; i < slots.Length; i++)
                if (guessPool.Contains(matchesInfo[i].Char) && !slots[i].TryAdd(matchesInfo[i]))
                    guessPool.Remove(matchesInfo[i].Char);
            return slots;
        }
        private string GetUserGuess(Preset _currentPreset, List<char> guessedPool)
        {
            int pos = 0;
            char[] chars = new char[_currentPreset.AnswerSettings.MaxAnswerLength];
            while (true)
            {
                _render.DisplayIn(_currentPreset, chars, pos);
                GameCommand Command = _input.GetUserChar(out char userChar);
                switch (Command)
                {
                    case GameCommand.Accept:
                        if (!chars.Contains('\0')) return new string(chars);
                        break;
                    case GameCommand.Left: pos--; break;
                    case GameCommand.Right: pos++; break;
                    case GameCommand.Backspace: chars[pos] = '\0'; continue;
                    case GameCommand.Esc: return string.Empty;
                }
                pos = Math.Clamp(pos, 0, _currentPreset.AnswerSettings.MaxAnswerLength - 1);
                if (userChar == '\0' || !guessedPool.Contains(userChar))
                    continue;
                chars[pos] = userChar;
                if (pos + 1 < _currentPreset.AnswerSettings.MaxAnswerLength && chars[pos + 1] == '\0')
                    pos++;
            }
        }
        private OutputCharInfo[] CalculateMatches(string userGuess)
        {
            OutputCharInfo[] userGuessInfo = new OutputCharInfo[userGuess.Length];
            for (int i = 0; i < userGuess.Length; i++)
            {
                userGuessInfo[i] = new OutputCharInfo();
                userGuessInfo[i].Char = userGuess[i];
                userGuessInfo[i].Count = CharsCounter(userGuess[i]);

                if (userGuessInfo[i].Char == _gameState.CurrentAnswer[i])
                {
                    userGuessInfo[i].Type = CharType.RightPosition;
                    continue;
                }
                if (userGuessInfo[i].Count > 0)
                {
                    userGuessInfo[i].Type = CharType.WrongPosition;
                    continue;
                }
                userGuessInfo[i].Type = CharType.NotContained;
            }
            return userGuessInfo;
        }
        private int CharsCounter(char value)
        {
            int count = 0;
            for (int i = 0; i < _gameState.CurrentAnswer.Count(); i++)
                if (_gameState.CurrentAnswer[i] == value) count++;
            return count;
        }
        private void CreatePresets()
        {
            _presetStore.AddOrUpdate(new Preset("Default", 5,
                new HashSet<char> { '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B' },
            new AnswerSettings(6, false),
            new OutputSettings(true, true, false)));

            _presetStore.AddOrUpdate(new Preset("Lechu's preset", 4,
    new HashSet<char> { '2', '3', '4', '5', '6', '7' },
    new AnswerSettings(5, false),
    new OutputSettings(true, false, false)));

            _presetStore.AddOrUpdate(new Preset("Минимализм", 4,
                new HashSet<char> { '1', '2', '3' },
                new AnswerSettings(3, true),
                new OutputSettings(false, false, false)));

            _presetStore.AddOrUpdate(new Preset("Короткий", 4,
                new HashSet<char> { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J' },
                new AnswerSettings(3, true),
                new OutputSettings(false, false, false)));

            _presetStore.AddOrUpdate(new Preset("Радуга", 5,
                new HashSet<char> { 'R', 'O', 'Y', 'G', 'B', 'I', 'V' },
                new AnswerSettings(4, true),
                new OutputSettings(false, false, false)));

            _presetStore.AddOrUpdate(new Preset("Числовой страж", 7,
                new HashSet<char> { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' },
                new AnswerSettings(4, true),
                new OutputSettings(false, false, false)));

            _presetStore.AddOrUpdate(new Preset("HEX-режим", 6,
                new HashSet<char> { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' },
                new AnswerSettings(6, false),
                new OutputSettings(true, true, false)));

            _presetStore.AddOrUpdate(new Preset("Цифровая лихорадка", 6,
                new HashSet<char> { '1', '2', '3', '4', '5', '6', '7', '8', '9' },
                new AnswerSettings(6, false),
                new OutputSettings(true, false, false)));

            _presetStore.AddOrUpdate(new Preset("Букво-цифры", 6,
                new HashSet<char> { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' },
                new AnswerSettings(8, false),
                new OutputSettings(true, true, false)));

            _presetStore.AddOrUpdate(new Preset("Без повторов", 5,
                new HashSet<char> { '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B' },
                new AnswerSettings(6, true),
                new OutputSettings(false, false, false)));

            _presetStore.AddOrUpdate(new Preset("Символьный микс", 6,
                new HashSet<char> { '1', '2', '3', 'A', 'B', 'C', '!', '?' },
                new AnswerSettings(5, false),
                new OutputSettings(true, true, false)));

            _presetStore.AddOrUpdate(new Preset("Повторы и ловушки", 6,
                new HashSet<char> { '1', '2', '3', '4', '5' },
                new AnswerSettings(5, false),
                new OutputSettings(true, true, false)));

            _presetStore.AddOrUpdate(new Preset("Алфавитный вихрь", 6,
                new HashSet<char> { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L' },
                new AnswerSettings(5, true),
                new OutputSettings(false, false, false)));

            _presetStore.AddOrUpdate(new Preset("Сияние", 6,
                new HashSet<char> { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L' },
                new AnswerSettings(6, true),
                new OutputSettings(false, false, false)));

            _presetStore.AddOrUpdate(new Preset("Шах и мат", 8,
                new HashSet<char> { 'K', 'Q', 'R', 'B', 'N', 'P', '1', '2', '3' },
                new AnswerSettings(6, true),
                new OutputSettings(false, false, false)));

            _presetStore.AddOrUpdate(new Preset("Длинный", 9,
                new HashSet<char> { '1','2','3','4','5','6','7','8','9',
                        'A','B','C','D','E','F','G','H','I','J' },
                new AnswerSettings(10, false),
                new OutputSettings(true, true, false)));

            _presetStore.AddOrUpdate(new Preset("Марафон", 10,
                new HashSet<char> { '0','1','2','3','4','5','6','7','8','9',
                        'A','B','C','D','E','F','G','H','I','J',
                        'K','L','M','N','O','P' },
                new AnswerSettings(8, false),
                new OutputSettings(true, true, false)));

            _presetStore.AddOrUpdate(new Preset("Числовая рулетка", 7,
                new HashSet<char> { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' },
                new AnswerSettings(5, false),
                new OutputSettings(false, false, true)));

            _presetStore.AddOrUpdate(new Preset("Русская кириллица", 7,
                new HashSet<char> { 'А', 'Б', 'В', 'Г', 'Д', 'Е', 'Ж', 'З', 'И', 'К', 'Л', 'М' },
                new AnswerSettings(5, true),
                new OutputSettings(false, false, false)));

            _presetStore.AddOrUpdate(new Preset("Полный хаос", 6,
                new HashSet<char> { '1', '2', '3', 'A', 'B', 'В', 'Г', '!', '?' },
                new AnswerSettings(7, false),
                new OutputSettings(true, true, false)));

            _presetStore.AddOrUpdate(new Preset("Шифр морок", 5,
                new HashSet<char> { '!', '@', '#', '$', '%', '&' },
                new AnswerSettings(4, false),
                new OutputSettings(false, true, false)));

            _presetStore.AddOrUpdate(new Preset("Дупликат", 6,
                new HashSet<char> { '1', '2', '3', '4', '5', '6' },
                new AnswerSettings(4, false),
                new OutputSettings(true, true, false)));

            _presetStore.AddOrUpdate(new Preset("Код Хакера", 6,
                new HashSet<char> { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'X', 'Y' },
                new AnswerSettings(6, false),
                new OutputSettings(false, true, false)));

            _presetStore.AddOrUpdate(new Preset("Полоска странностей", 6,
                new HashSet<char> { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I' },
                new AnswerSettings(5, false),
                new OutputSettings(true, true, false)));

            _presetStore.AddOrUpdate(new Preset("Блиц", 3,
                new HashSet<char> { '1', '2', '3', '4', '5', '6', '7', '8', '9' },
                new AnswerSettings(3, true),
                new OutputSettings(false, false, true))); 

            _presetStore.AddOrUpdate(new Preset("Числовой Блиц", 4,
                new HashSet<char> { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' },
                new AnswerSettings(3, true),
                new OutputSettings(false, false, true))); 

            _presetStore.AddOrUpdate(new Preset("Секретный код", 6,
                new HashSet<char> { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' },
                new AnswerSettings(4, true),
                new OutputSettings(false, false, true)));

            _presetStore.AddOrUpdate(new Preset("Рулетка повторов", 10,
                new HashSet<char> { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' },
                new AnswerSettings(5, false), 
                new OutputSettings(false, false, true)));

            _presetStore.AddOrUpdate(new Preset("Математическая загадка", 8,
                new HashSet<char> { '1', '2', '3', '4', '5', '6', '7', '8', '9' },
                new AnswerSettings(6, false), 
                new OutputSettings(false, false, true)));

            _presetStore.AddOrUpdate(new Preset("Повторный шторм", 12,
                new HashSet<char> { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' },
                new AnswerSettings(6, false),
                new OutputSettings(false, false, true)));

            _presetStore.AddOrUpdate(new Preset("Ночное шифрование", 9,
                new HashSet<char> { '1', '2', '3', '4', '5', '6', '7' },
                new AnswerSettings(4, false),
                new OutputSettings(false, false, true))); 
        }
    }
}
