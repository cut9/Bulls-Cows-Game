using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulls_Cows
{
    internal class GameState
    {
        public string? CurrentAnswer { get; private set; }
        public int CurrentAttempt { get; private set; }
        public int SelectedPresetIndex { get; set; } = 0;
        public Scene CurrentScene { get; set; }
        public int Loss { get; set; }
        public int Win { get; set; }
        public void SetAnswer(string ans) => CurrentAnswer = ans;
        public void ResetCurrentAttempt() => CurrentAttempt = 1;
        public void IncrementAttempt() => CurrentAttempt++;
        public void NewWin() => Win++;
        public void NewLoss() => Loss++;
        public int LastDaily {  get; set; }
    }
}
