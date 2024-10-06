public static class RngUtils
{
    private static System.Random rng;
    public static System.Random Rng
    {
        get
        {
            if (rng == null)
            {
                rng = new System.Random();
            }
            return rng;
        }
    }

    public static bool CoinFlip() => Rng.Next(2) % 2 == 0;
    public static bool RollWithinLimitCheck(int limit, int maxValueForRoll = 100) => Rng.Next(maxValueForRoll) < limit;  
}
