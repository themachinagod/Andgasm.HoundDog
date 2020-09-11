namespace Andgasm.HoundDog.AccountManagement.Interfaces
{
    public class SimpleDate
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }

        public string UTCString {
            get
            {
                return $"{Year}-{Month}-{Day}";
            }
        }

        public SimpleDate()
        {
        }
        
        public SimpleDate(int year, int month, int day)
        {
            Year = year;
            Month = month;
            Day = day;
        }
    }
}
