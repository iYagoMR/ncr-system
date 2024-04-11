using Haver.DraftModels;

namespace Haver.Models
{
    public class NCRNumber
    {
        public int ID { get; set; }
        public int Counter { get; set; } = 0;
        public int Year { get; set; }

        public string GenerateNCRNumber(bool isNewYear, int? prevNumber)
        {
            // Increment the counter and format the NCR number
            Counter = prevNumber.Value;

            // Check if it's a new year and reset the counter
            if (isNewYear == true)
            {
                Counter = 0;
            }

            Counter++;
            return $"{Year}-{Counter.ToString().PadLeft(3, '0')}";
        }

    }
}