﻿using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Vostok.Commons.Helpers.Topology
{
    [PublicAPI]
    internal class ReplicaComparer : IEqualityComparer<Uri>
    {
        public static readonly ReplicaComparer Instance = new ReplicaComparer();

        public bool Equals(Uri r1, Uri r2)
        {
            if (ReferenceEquals(r1, r2))
                return true;

            if (r1 == null || r2 == null)
                return false;

            if (r1.Port != r2.Port)
                return false;

            var host1 = r1.DnsSafeHost;
            var host2 = r2.DnsSafeHost;

            LocateHostname(host1, out var length1);
            LocateHostname(host2, out var length2);

            return length1 == length2
                   && string.Compare(host1, 0, host2, 0, length1, StringComparison.OrdinalIgnoreCase) == 0
                   && string.Compare(r1.AbsolutePath, r2.AbsolutePath, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public int GetHashCode(Uri replica)
        {
            var host = replica.DnsSafeHost;

            LocateHostname(host, out var length);

#if NET6_0_OR_GREATER
            var hostSpan = host.AsSpan();
            if (length < host.Length)
                hostSpan = hostSpan.Slice(0, length);

            return (string.GetHashCode(hostSpan, StringComparison.OrdinalIgnoreCase) * 397)
                   ^ (string.GetHashCode(replica.AbsolutePath, StringComparison.OrdinalIgnoreCase) * 397)
                   ^ replica.Port;
#else
            if (length < host.Length)
                host = host.Substring(0, length);

            return (StringComparer.OrdinalIgnoreCase.GetHashCode(host) * 397)
                   ^ (StringComparer.OrdinalIgnoreCase.GetHashCode(replica.AbsolutePath) * 397)
                   ^ replica.Port;
#endif
        }

        private static void LocateHostname(string host, out int length)
        {
            length = host.Length;

            if (length <= 0)
                return;

            var dotIndex = host.IndexOf('.');
            if (dotIndex < 0)
                return;

            if (IsIpAddress(host))
                return;

            length = dotIndex;
        }
        
        private static bool IsIpAddress(string host)
        {
            var numParts = 0;
            var currentOctetValue = 0;
            var hasAtLeastOneDigitInOctet = false;

            foreach (var c in host)
            {
                // note (lunev.d, 01.03.2023): We don't support port specification at the end of address
                // and so we can consider that if an address contains colon - it's an IPv6 address
                if (c == ':')
                    return true;

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