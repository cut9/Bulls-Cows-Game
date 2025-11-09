using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulls_Cows
{
    internal class MenuStore
    {
        private List<ContextMenu> _contextMenus = new();

        public IReadOnlyCollection<ContextMenu> All => _contextMenus;

        public void AddOrUpdate(ContextMenu contextMenu) => _contextMenus.Add(contextMenu);
        public bool TryGet(int id, out ContextMenu contextMenu)
        {
            if (id >= 0 && id < _contextMenus.Count)
            {
                contextMenu = _contextMenus[id];
                return true;
            }
            contextMenu = null;
            return false;
        }
        public bool Remove(int id)
        {
            if (id >= 0 && id < _contextMenus.Count)
            {
                _contextMenus.RemoveAt(id);
                return true;
            }
            return false;
        }
    }
}
