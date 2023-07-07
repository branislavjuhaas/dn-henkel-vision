using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DN_Henkel_Vision.Memory
{
    /// <summary>
    /// The fault class storing all the pieces of information about a fault.
    /// </summary>
    public class Fault
    {
        public string Placement;
        public string Component;
        public string Description;
        public string Cause;
        public string Classification;
        public string Type;
        public int Index;
        public int[] ClassIndexes = { -1, -1, -1 };
        public float UserTime;
        public float MachineTime;

        /// <summary>
        /// The fault class storing all the pieces of information about a fault.
        /// </summary>
        /// <param name="description">Main description of the fault</param>
        /// <param name="cause">the cause of the file for classificaiton</param>
        public Fault(string description, string cause = null)
        {
            Description = description;
            Cause = cause;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Fault Clone()
        {
            Fault clone = new Fault(Description, Cause);
            clone.Placement = Placement;
            clone.Component = Component;
            clone.Classification = Classification;
            clone.Type = Type;
            clone.Index = Index;
            clone.ClassIndexes = ClassIndexes;
            clone.UserTime = UserTime;
            return clone;

        }

        /// <summary>
        /// The new ToString method for the fault class.
        /// </summary>
        /// <returns>the string of fault</returns>
        public override string ToString()
        {
            return $"{Component}\t{Placement}\t{Description}\t{Cause}\t{Classification}\t{Type}\t{ClassIndexes[0]}\t{ClassIndexes[1]}\t{ClassIndexes[2]}";
        }

        public string Export(string order, string user, string date)
        {
            string ordernumber = order.Replace(" ", "");

            if (order.StartsWith("38")) { ordernumber = ordernumber.Substring(2); }
            
            return $"{ordernumber}\t{Placement}\t{Description}\t{Component}\t{Memory.Classification.OriginalCauses[ClassIndexes[0]]}\t{Memory.Classification.OriginalClassifications[ClassIndexes[0]][ClassIndexes[1]]}\t{Memory.Classification.OriginalTypes[Memory.Classification.ClassificationsPointers[ClassIndexes[0]][ClassIndexes[1]]][ClassIndexes[2]]}\t{user}\t{date}";
        }

        /// <summary>
        /// This void checks if the characters provided are valid for a Netstal placement.
        /// Valid placement is each 2 characters long, starts with 'A' and ends with a number.
        /// </summary>
        /// <param name="placement">Placement provided</param>
        /// <returns>Whether the input is valid netstal placement</returns>
        public static bool IsValidNetstalPlacement(string placement)
        {
            if (placement == null || placement.Length != 2) { return false; }
            if (placement.ToLower().ToCharArray()[0] != 'a') { return false; }
            if (!placement.Substring(1, 1).All(char.IsDigit)) { return false; }

            return true;
        }
    }
}
