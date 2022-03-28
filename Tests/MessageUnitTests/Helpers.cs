// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

namespace MessageUnitTests
{
    internal static class Helpers
    {
        public static void DumpBuffer(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                Debug.Write($"0x{buffer[i].ToString("X2")}, ");
            }

            Debug.WriteLine("");
        }
    }
}
