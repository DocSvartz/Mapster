using System;
using System.Text;

namespace ExpressionDebugger.Helpers
{
    public static class RandomNamespaceGenerator
    {
        private static readonly Random _random = new Random();
        private const string Consonants = "bcdfghjklmnpqrstvwxyzBCDFGHJKLMNPQRSTVWXYZ";
        private const string Vowels = "aeiouAEIOU";
        private const string Digits = "0123456789";

        public static string Generate(int minParts = 2, int maxParts = 4)
        {
            if (minParts < 1) minParts = 1;
            if (maxParts < minParts) maxParts = minParts;

            int partsCount = _random.Next(minParts, maxParts + 1);
            var sb = new StringBuilder();

            for (int i = 0; i < partsCount; i++)
            {
                if (i > 0) sb.Append('.');
                sb.Append(GeneratePart());
            }

            return sb.ToString();
        }

        private static string GeneratePart(int minLength = 2, int maxLength = 10)
        {
            if (minLength < 1) minLength = 1;
            if (maxLength < minLength) maxLength = minLength;

            int length = _random.Next(minLength, maxLength + 1);
            var sb = new StringBuilder(length);

            sb.Append(Consonants[_random.Next(Consonants.Length)]);

            for (int i = 1; i < length; i++)
            {
                string pool = (i % 2 == 0) ? Vowels : Consonants;

                if (_random.NextDouble() < 0.1)
                {
                    pool = Digits;
                }

                sb.Append(pool[_random.Next(pool.Length)]);
            }

            return sb.ToString();
        }
    }
}

