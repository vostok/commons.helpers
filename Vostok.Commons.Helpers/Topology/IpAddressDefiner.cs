namespace Vostok.Commons.Helpers.Topology
{
    internal static class IpAddressDefiner
    {
        public static bool IsIpAddress(string host)
        {
            // note (lunev.d, 01.03.2023): We don't support port specification at the end of address
            // and so we can consider that if an address contains colon - it's an IPv6 address
            if (host.Contains(":"))
                return true;
            
            var numParts = 0;
            var currentOctetValue = 0;
            var hasAtLeastOneDigitInOctet = false;

            foreach (var c in host)
            {
                if (c == '.')
                {
                    if (numParts >= 3 || !hasAtLeastOneDigitInOctet)
                        return false;

                    currentOctetValue = 0;
                    numParts++;
                    hasAtLeastOneDigitInOctet = false;
                }
                else if (c >= '0' && c <= '9')
                {
                    var digit = c - '0';
                    currentOctetValue = currentOctetValue * 10 + digit;
                    
                    if (currentOctetValue > 255)
                        return false;
                    
                    hasAtLeastOneDigitInOctet = true;
                }
                else
                    return false;
            }

            if (numParts > 3 || !hasAtLeastOneDigitInOctet)
                return false;

            return true;
        }
    }
}