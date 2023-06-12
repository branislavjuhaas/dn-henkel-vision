namespace DN_Henkel_Vision.Memory
{
    internal class Cache
    {
        public static Fault PreviewFault = new("");
        public static string LastPlacement = "";

        public static int CurrentReview = 0;

        public static void Clear()
        {
            PreviewFault = new Fault("");
            LastPlacement = "";
            CurrentReview = 0;
        }
    }
}
