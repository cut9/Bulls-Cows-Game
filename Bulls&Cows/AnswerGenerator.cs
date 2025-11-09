using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulls_Cows
{
    internal class AnswerGenerator : IAnswerGenerator
    {
        public List<char> _pool {  get; private set; }
        public IAnswerSettings _settings { get; private set; }
        public IRandomProvider _rng { get; private set; }
        private StringBuilder _answer = new();
        private bool _setted = false;
        public void SetSettings(IReadOnlySet<char> pool, IAnswerSettings settings, IRandomProvider rng)
        {
            _pool = pool.ToList();
            _settings = settings;
            _rng = rng;
            _answer.Clear();
            _setted = true;
        }
        public bool TryGenerate(out string answer)
        {
            answer = string.Empty;
            if (!_setted)
                return false;
            int min = 0;
            int max = _pool.Count - 1;

            if (_settings.UniqueOnly)
            {
                if (_pool.Count < _settings.MaxAnswerLength)
                {
                    return false;
                }
                var used = new HashSet<char>();
                while (_answer.Length < _settings.MaxAnswerLength)
                {
                    int idx = _rng.Next(min, max + 1);
                    char c = _pool[idx];
                    if (used.Add(c))
                        _answer.Append(c);
                }
            }
            else
            {
                while (_answer.Length < _settings.MaxAnswerLength)
                {
                    int idx = _rng.Next(min, max + 1);
                    _answer.Append(_pool[idx]);
                }
            }
            _setted = false;
            answer = _answer.ToString();
            return true;
        }
    }
}
