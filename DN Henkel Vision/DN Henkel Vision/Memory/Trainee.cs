using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DN_Henkel_Vision.Memory
{
    internal class Trainee
    {
        public string Component;
        public string Description;
        public string Cause;
        public string Classification;
        public string Type;

        public Trainee(Fault fault)
        {
            Component = fault.Component;
            Description = fault.Description;
            Cause = fault.Cause;
            Classification = fault.Classification;
            Type = fault.Type;
        }

        public Trainee(string description)
        {
            Component = description;
        }

        public override string ToString()
        {
            return $"{Component}\t{Description}\t{Cause}\t{Classification}\t{Type}";
        }
    }
}
