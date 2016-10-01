using System.Collections.Generic;

namespace CodeRedundancyCheck.Interface
{
    public class MethodDefinition
    {
        public string MethodName { get; set; }

        public string ReturnValueType { get; set; }

        public List<Variable> MethodParameters { get; set; }

    }
}