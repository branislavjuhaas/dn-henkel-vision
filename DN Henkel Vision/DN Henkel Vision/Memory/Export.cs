using DN_Henkel_Vision.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DN_Henkel_Vision.Memory
{
    internal class Export : Drive
    {
        private static string s_header = "SZSK2ZZ69JA93CDNHENKELVISION84SAW4K7DNHENKELVISIONXNESVGSYDNHENKELVISIONFMRR4YD5DNHENKELVISION3VY0RS\r\n"
                                       + "LQZF15    N85J1DNHENKELVISIONJXG7Z5PEDNHENKELVISIONPXBXJCHRDNHENKELVISION50PUYSK7DNHENKELVISIONTU6EP\r\n"
                                       + "S1PX  6N  02W7Q3DNHENKELVISION2YCSP43WDNHENKELVISIONCFFQAQT2DNHENKELVISIONTG8W2H03DNHENKELVISION7UTJ\r\n"
                                       + "4C9Y  BL    R50EUDNHENKELVISIONCE6WZVLGDNHENKELVISION52FPJS6PDNHENKELVISION1K3U7H63DNHENKELVISIONWYA\r\n"
                                       + "QLNF  16X1  V1ALR7DNHENKELVISIONTJH0LHPSDNHENKELVISIONQN0USUBXDNHENKELVISIONK7AX8B21DNHENKELVISION4Z\r\n"
                                       + "Y3  F0LGYV4LFQ0QTGQDNHENKELVISION3QHAW00HDNHENKELVISIONEJGLJTWJDNHENKELVISION7CB9J59GDNHENKELVISIONH\r\n";

        private static string s_adnetstal = "NWEWT0SCTTAEL1PB9NACDNHENKELVISIONNJCEP8XHDNHENKELVISIONW2P1R7QTDNHENKELVISIONR77KK0EBDNHENKELVISION";
        private static string s_adauftrag = "AWUWF0TCRTAEG1PB9NACDNHENKELVISIONNJCEP8XHDNHENKELVISIONW2P1R7QTDNHENKELVISIONR77KK0EBDNHENKELVISION";
        private static string s_adauftragfeauf = "AWUWF0TCRTAEG1FBANUCFNHENKELVISIONNJCEP8XHDNHENKELVISIONW2P1R7QTDNHENKELVISIONR77KK0EBDNHENKELVISION";


        public static int GraphicalCount = 36;
        
        public static List<string> Unexported = new();

        public static int BreakingPoint = 0;

        public static float[] UserService = new float[GraphicalCount];
        public static float[] MachService = new float[GraphicalCount];

        public static float[] UserExports = new float[GraphicalCount];
        public static float[] MachExports = new float[GraphicalCount];

        #region Graphing

        public static int ServiceMaximal = 2;
        public static int ServiceHalf = 1;

        public static int ExportsMaximal = 2;
        public static int ExportsHalf = 1;


        public static List<int> UserServiceGraph = new();
        public static List<int> MachServiceGraph = new();

        public static List<int> UserExportsGraph = new();
        public static List<int> MachExportsGraph = new();

        public static int AuftragSplitter = 0;
        public static int NetstalSplitter = 0;
        #endregion

        public static void Evaluate()
        {
            UserServiceGraph.Clear();
            MachServiceGraph.Clear();
            UserExportsGraph.Clear();
            MachExportsGraph.Clear();

            if (ServiceSum().Max() > ServiceMaximal)
            {
                ServiceMaximal = (int)((float)Math.Ceiling(ServiceSum().Max() / 2f) * 2f);
                ServiceHalf = ServiceMaximal / 2;
            }

            if (ExportsSum().Max() > ExportsMaximal)
            {
                ExportsMaximal = (int)((float)Math.Ceiling(ExportsSum().Max() / 2f) * 2f);
                ExportsHalf = ExportsMaximal / 2;
            }

            float serviceMultiplier = 158f / (float)ServiceMaximal;
            float exportsMultiplier = 158f / (float)ExportsMaximal;

            for (int i = 0; i < GraphicalCount; i++) 
            {
                UserServiceGraph.Add((int)(UserService[i] * serviceMultiplier));
                MachServiceGraph.Add((int)(MachService[i] * exportsMultiplier) + UserServiceGraph[i]);

                UserExportsGraph.Add((int)(UserExports[i] * exportsMultiplier));
                MachExportsGraph.Add((int)(MachExports[i] * exportsMultiplier) + UserExportsGraph[i]);
            }
        }

        public static string GraphTime(int scenario)
        {
            switch (scenario)
            {
                case 0: return ServiceMaximal.ToString() + "h";
                case 1: return ServiceHalf.ToString() + "h";
                case 2: return ExportsMaximal.ToString() + "h";
                case 3: return ExportsHalf.ToString() + "h";
                default: return String.Empty;
            }
        }

        public static float[] ServiceSum()
        {
            float[] output = new float[GraphicalCount];

            for (int i = 0; i < GraphicalCount; i++)
            {
                output[i] = UserService[i] + MachService[i];
            }

            return output;
        }

        public static float[] ExportsSum()
        {
            float[] output = new float[GraphicalCount];

            for (int i = 0; i < GraphicalCount; i++)
            {
                output[i] = UserExports[i] + MachExports[i];
            }

            return output;
        }

        public static float OrdersTime(string[] orders, bool machine = true, bool netstal = false)
        {
            float output = 0f;

            foreach (string order in orders)
            {
                if (order.StartsWith("20") != netstal) { continue; }
                
                int index = Manager.OrdersRegistry.IndexOf(order);

                float multiplier = (1f - ((float)Manager.Exports[index] / (float)Manager.Contents[index]));


                output += (float)Manager.Users[index] * multiplier;

                if (!machine) { continue; }

                output += (float)Manager.Machines[index] * multiplier;
            }

            return output;
        }

        public static async Task<string> ExportFaults(float time,string username, DateTime date, bool netstal = false)
        {
            float remain = time * 60f;

            List<string> exported = new();

            string output = Header();

            foreach (string order in Unexported)
            {
                if (order.StartsWith("20") != netstal) { continue; }

                int index = Manager.OrdersRegistry.IndexOf(order);

                Fault[] exports = Drive.LoadFaults(order)[0].ToArray();

                int ex = 0;

                float timeperfault = ((float)(Manager.Users[index] + Manager.Machines[index]) / (float)exports.Length);

                for (int i = Manager.Exports[index]; i < exports.Length; i++)
                {
                    output += "\r\n" + (exports[i].Export(order, username, date.ToString("yyyy-MM-dd")));
                    remain -= timeperfault;
                    ex++;
                    if (remain <= 0f) { break; }
                }

                Manager.Exports[index] += ex;

                if (Manager.Exports[index] >= Manager.Contents[index])
                {
                    exported.Add(order);
                }

                if (remain <= 0f) { break; }
            }

            foreach (string order in exported)
            {
                Unexported.Remove(order);
            }

            return output;
        }

        public static string Header(bool netstal = false)
        {
            string pseudoheader = s_header.Replace("DNHENKELVISION", "##############");

            List<int> indexes = new();
            List<char> values = new();

            int ri = Random.Shared.Next(65, 90);
            int rii = Random.Shared.Next(65, 90);

            int seed = ri * rii;

            char[] date = DateTime.Now.ToString("ddMMyy").ToCharArray();
            char[] user = Regex.Replace(System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToUpper(), "[^A-Z0-9]", "").ToCharArray();
            char[] version = Assembly.GetExecutingAssembly().GetName().Version.ToString().Replace('.', 'N').ToCharArray();

            HeaderReplace(ref pseudoheader, ref indexes, ref values, 77, (char)ri);
            HeaderReplace(ref pseudoheader, ref indexes, ref values, 78, (char)rii);
            
            
            HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), date[0]);
            HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), date[1]);
            HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), date[2]);
            HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), date[3]);
            HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), date[4]);
            HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), date[5]);

            HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), (char)(user.Length + 65));

            foreach (char userchar in user)
            {
                HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), userchar);
            }

            HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), (char)(version.Length + 65));

            foreach (char versionchar in version)
            {
                HeaderReplace(ref pseudoheader, ref indexes, ref values, Sequence(ref seed, 10, 605), versionchar);
            }

            char[] pseudo = pseudoheader.Replace("##############", "DNHENKELVISION").ToCharArray();

            for (int i = 0; i < indexes.Count; i++)
            {
                pseudo[indexes[i]] = values[i];
            }

            if (netstal)
            {
                return new string(pseudo) + s_adnetstal;
            }

            return new string(pseudo) + s_adauftrag;
        }

        public static void HeaderReplace( ref string pseudoheader, ref List<int> replacedindex, ref List<char> replacedvalue, int index, char replacement )
        {
            while (pseudoheader.ToCharArray()[index] == '#' || pseudoheader.ToCharArray()[index] == ' ' || pseudoheader.ToCharArray()[index] == '$' || pseudoheader.ToCharArray()[index] == '\r' || pseudoheader.ToCharArray()[index] == '\n') 
            {
                index += 2; 
                if (index >= pseudoheader.Length) { index = 0; }
            }

            replacedindex.Add(index);
            replacedvalue.Add(replacement);

            char[] pseudo = pseudoheader.ToCharArray();
            pseudo[index] = '$';
            pseudoheader = new string(pseudo);
        }

        public static int Sequence(ref int seed, int min = int.MinValue, int max = int.MaxValue)
        {
            seed = (seed * 1103515245 + 12345) & int.MaxValue;

            return (seed % (max - min + 1)) + min;
        }
    }
}
