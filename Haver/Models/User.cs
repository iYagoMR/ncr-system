namespace Haver
{
    public class User
    {
        public int ID { get; set; }

        public string FullName
        {

            get
            {
                return $"{FirstName} {LastName}";
            }
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Role { get; set; }
    }
}
