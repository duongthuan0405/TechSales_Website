using System.Buffers.Text;
using System.Text;

namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int n = 32;
            byte[] data = new byte[n];
            Random r = new Random();
            r.NextBytes(data);
            string s = Convert.ToBase64String(data);
            Console.WriteLine(s);

        }
    }
}
