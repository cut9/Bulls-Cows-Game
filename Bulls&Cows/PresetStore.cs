namespace Bulls_Cows
{
    internal class PresetStore
    {
        private List<Preset> _presets = new();

        public IReadOnlyCollection<Preset> All => _presets;

        public void AddOrUpdate(Preset preset) => _presets.Add(preset);
        public void Insert(int index, Preset preset)
        {
            if (!(index >= 0 && index <= _presets.Count))
                return;
            _presets.Insert(index, preset);
        }
        public bool TryGet(int id, out Preset preset)
        {
            if (id >= 0 && id < _presets.Count)
            {
                preset = _presets[id];
                return true;
            }
            preset = null;
            return false;
        }
        public bool TryRemove(int id)
        {
            if (id >= 0 && id < _presets.Count)
            {
                _presets.RemoveAt(id);
                return true;
            }
            return false;
        }
    }
}
