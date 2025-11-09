using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulls_Cows
{
    internal class ContextMenu
    {
        private readonly List<ContextMenuOption> _contextMenuOptions;
        private int _selectedOption;
        public List<ContextMenuOption> ContextMenuOptions { get => _contextMenuOptions; }
        public int SelectedOption { get => _selectedOption; private set
        {
            _selectedOption = Math.Clamp(value, 0, _contextMenuOptions.Count - 1);
        }}
        public ContextMenu(List<ContextMenuOption> options)
        {
            _contextMenuOptions = options;
            SelectedOption = 0;
        }
        public void SelectedOptionReset() { SelectedOption = 0; }
        public void SelectedOptionUp() { SelectedOption--; }
        public void SelectedOptionDown() {  SelectedOption++; }
    }
}
