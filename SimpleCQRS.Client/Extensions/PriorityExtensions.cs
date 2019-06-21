using SimpleCQRS.Client.Enumerations;

namespace SimpleCQRS.Client.Extensions
{
    internal static class PriorityExtensions
    {
        internal static byte GetValue(this Priority priority)
        {
            switch (priority)
            {
                case Priority.Lowest:
                    return 1;
                
                case Priority.VeryLow:
                    return 2;

                case Priority.Low:
                    return 3;

                case Priority.Normal:
                    return 4;

                case Priority.AboveNormal:
                    return 5;

                case Priority.High:
                    return 6;

                case Priority.VeryHigh:
                    return 7;

                case Priority.Highest:
                    return 8;

                default:
                    return 4;
            }
        }
    }
}