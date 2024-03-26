using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DN_Henkel_Vision.Memory
{
    /// <summary>
    /// Represents a class for storing information about a trainee.
    /// </summary>
    internal class Trainee
    {
        public string Component;
        public string Description;
        public string Cause;
        public string Classification;
        public string Type;

        /// <summary>
        /// Initializes a new instance of the Trainee class using a Fault object.
        /// </summary>
        /// <param name="fault">The Fault object to initialize the Trainee object.</param>
        public Trainee(Fault fault)
        {
            Component = fault.Component;
            Description = fault.Description;
            Cause = fault.Cause;
            Classification = fault.Classification;
            Type = fault.Type;
        }

        /// <summary>
        /// Initializes a new instance of the Trainee class using a description.
        /// </summary>
        /// <param name="description">The description to initialize the Trainee object.</param>
        public Trainee(string description)
        {
            Component = description;
        }

        /// <summary>
        /// Returns a string that represents the current Trainee object.
        /// </summary>
        /// <returns>A string that represents the current Trainee object.</returns>
        public override string ToString()
        {
            return $"{Component}\t{Description}\t{Cause}\t{Classification}\t{Type}";
        }
    }
}
