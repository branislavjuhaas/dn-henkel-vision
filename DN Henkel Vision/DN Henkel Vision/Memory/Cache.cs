namespace DN_Henkel_Vision.Memory
{
    internal class Cache
    {
        public static string LastPlacement = "";

        public static int CurrentReview = 0;

        public static int LastIndex = 0;

        /// <summary>
        /// Clears the cache and resets all the variables.
        /// </summary>
        public static void Clear()
        {
            LastPlacement = "";
            CurrentReview = 0;
            LastIndex = 0;
        }
    }
}
