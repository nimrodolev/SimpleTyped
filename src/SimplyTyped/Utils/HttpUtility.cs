using System.Net;

namespace SimplyTyped.Utils
{
    internal class HttpUtility
    {
        public static bool IsSuccessStatusCode(HttpStatusCode code)
        {
            var n = (int)code;
            return n >= 200 && n < 300;
        }
    }
}