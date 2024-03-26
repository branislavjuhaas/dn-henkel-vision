using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DN_Henkel_Vision.Memory
{
    /// <summary>
    /// Represents a class for storing training information derived from the correct and incorrect faults.
    /// </summary>
    internal class Trainer
    {
        public static List<Trainee> Correct = new();
        public static List<Trainee> Incorrect = new();
    }
}
