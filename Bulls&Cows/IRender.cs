using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulls_Cows
{
    internal interface IRender
    {
        public List<int> SaveColors();
        public bool TryLoadSettings(List<int> colors);
        public void ShowAllStateColors(int selected = -1);
        public void SetCharStates(List<OutputCharStates> states);
        public int StateGetColor(OutputCharStates state);
        public void StateSetColor(OutputCharStates state, ref int chose);
        public void DisplayHowToPlay();
        public void DisplayLog(string Log);
        public void DisplaySettings(Preset preset, GameState _gameState, int selectedPresetIndex, int presetsCount);
        public void SceneTransition();
        public void DisplayIn(Preset preset, char[] userInput, int pos);
        public void DisplayOut(OutputCharInfo[] matchesInfo, OutputSettings outputSettings);
        public void DisplayNumbersOnlyHistory(List<HistoryData> guessesHistory);
        public void DisplayCurrentPool(List<char> guessPool, int poolBaseLength);
        public void DisplayAttempts(int CurrentAttempt, int NumberOfAttempts);
        public void DisplayFrame(int PoolLength, CellInfo[] slot);
        public void DisplayRightGuessHistory(CellInfo[] slot, OutputSettings outputSettings);
        public void DisplayWrongGuessHistory(CellInfo[] slot, OutputSettings outputSettings);
        public void DisplayMenu(ContextMenu contextMenu);
        public void DisplayWinScreen(int CurrentAttempt, int NumberOfAttempts);
        public void DisplayLossScreen(string Answer);
    }
}
