using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulls_Cows
{
    internal interface IGameSaver
    {
        public void SavePresetList(PresetStore presetStore);
        public void SaveGameState(GameState gameState);
        public void SaveOutputColorSettings(IRender render);
    }
}
