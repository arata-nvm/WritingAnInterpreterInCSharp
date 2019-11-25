using System.Collections.Generic;

namespace Monkey.Core
{
    public class Environment
    {
        private Dictionary<string, IObject> variables;
        private Dictionary<string, IObject> constants;
        
        public Environment()
        {
            this.variables = new Dictionary<string, IObject>();
            this.constants = new Dictionary<string, IObject>();
        }

        public IObject Get(string name)
        {
            if (variables.TryGetValue(name, out var value))
            {
                return value;
            }
            else if (constants.TryGetValue(name, out value))
            {
                return value;
            }

            return null;
        }

        public IObject SetVariable(string name, IObject val)
        {
            if (constants.ContainsKey(name))
                return new Error {Message = "constant with the same name is already declared"};
            
            variables[name] = val;
            return val;
        }
        
        public IObject SetConstant(string name, IObject val)
        {
            if (variables.ContainsKey(name))
                return new Error {Message = "variable with the same name is already declared"};
            
            if (constants.ContainsKey(name))
                return new Error {Message = "constant value cannot be changed"};
            
            constants[name] = val;
            return val;
        }


        public Environment Clone()
        {
            return new Environment {variables = this.variables};
        }
    }
}