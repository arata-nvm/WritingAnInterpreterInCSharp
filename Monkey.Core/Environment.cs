using System.Collections.Generic;

namespace Monkey.Core
{
    public class Environment
    {
        private Dictionary<string, IObject> store;

        public Environment()
        {
            this.store = new Dictionary<string, IObject>();
        }

        public IObject Get(string name)
        {
            return store.TryGetValue(name, out var value) ? value : null;
        }

        public IObject Set(string name, IObject val)
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