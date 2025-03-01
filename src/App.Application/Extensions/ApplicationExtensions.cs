using System.Text;

namespace App.Application.Extensions
{
    public static class ApplicationExtensions
    {
        public static string GenerateRandomUsername()
        {
            var random = new Random();
            var sb = new StringBuilder();
            sb.Append("koala_");
            for (int i = 0; i < 8; i++)
            {
                sb.Append(random.Next(0, 10));
            }
            return sb.ToString();
        }
    }
}
