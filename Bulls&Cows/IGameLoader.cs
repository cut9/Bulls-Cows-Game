using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulls_Cows
{
    internal interface IGameLoader
    {
        public bool TryLoadPresetList(PresetStore presetStore, int _maxAnswerLength, int _maxAttemptNumber);
        public bool TryLoadGameState(GameState gameState);
        public bool TryLoadOutputColorSettings(IRender render);
    }
}
