using System.Collections.Generic;

namespace monkey_csharp.Monkey.Core
{
    public class Environment
    {
        private Dictionary<string, Object> store;

        public Environment()
        {
            this.store = new Dictionary<string, Object>();
        }

        public Object Get(string name)
        {
            return store.TryGetValue(name, out var value) ? value : null;
        }

        public Object Set(string name, Object val)
        {
            store[name] = val;
            return val;
        }

        public Environment Clone()
        {
            return new Environment {store = this.store};
        }
    }
}