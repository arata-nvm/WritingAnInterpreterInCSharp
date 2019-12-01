using System.Collections.Generic;

namespace Monkey.Core
{
    public class Environment
    {
        private readonly Dictionary<string, IObject> _variables;
        private readonly Dictionary<string, IObject> _constants;

        private Environment _outer;
        
        public Environment()
        {
            this._variables = new Dictionary<string, IObject>();
            this._constants = new Dictionary<string, IObject>();
        }

        public IObject Get(string name)
        {
            if (_variables.TryGetValue(name, out var value))
                return value;

            if (_constants.TryGetValue(name, out value))
                return value;

            return _outer?.Get(name);
        }

        public IObject SetVariable(string name, IObject val)
        {
            if (_constants.ContainsKey(name))
                return new Error {Message = "constant with the same name is already declared"};
            
            _variables[name] = val;
            return val;
        }
        
        public IObject SetConstant(string name, IObject val)
        {
            if (_variables.ContainsKey(name))
                return new Error {Message = "variable with the same name is already declared"};
            
            if (_constants.ContainsKey(name))
                return new Error {Message = "constant value cannot be changed"};
            
            _constants[name] = val;
            return val;
        }

        public bool ExistsVariable(string name)
        {
            return _variables.ContainsKey(name);
        }

        public bool ExistsConstant(string name)
        {
            return _constants.ContainsKey(name);
        }


        public Environment CreateEnclosedEnvironment()
        {
            return new Environment {_outer = this};
        }
    }
}