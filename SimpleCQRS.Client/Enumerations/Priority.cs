namespace SimpleCQRS.Client.Enumerations
{
    public enum Priority
    {
        /// <summary>
        /// Lowest message priority.
        /// </summary>
        Lowest = 0,
        
        /// <summary>
        /// Between Low and Lowest message priority.
        /// </summary>
        VeryLow	= 1,
        
        /// <summary>
        /// Low message priority.
        /// </summary>
        Low	= 2,
        
        /// <summary>
        /// Normal message priority.
        /// </summary>
        Normal = 3,
        
        /// <summary>
        /// Between High and Normal message priority.
        /// </summary>
        AboveNormal	= 4,
        
        /// <summary>
        /// High message priority.
        /// </summary>
        High = 5,
        
        /// <summary>
        /// Between Highest and High message priority.
        /// </summary>
        VeryHigh = 6,
        
        /// <summary>
        /// Highest message priority.
        /// </summary>
        Highest	= 7	
    }
}