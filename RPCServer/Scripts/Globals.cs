using System.Collections;
using System.Collections.Generic;

namespace Game
{
    public static class Globals
    {
        public static readonly int DATA_SZIE = 1024 * 8;
        public static readonly int BUFFER_SIZE = 1024 * 100;
        private static readonly int TA = 63689;
        private static readonly int TB = 378551;

        public static int Hash(string id)
        {
            int seed = TA;
            int hash = 0;
            foreach (char c in id)
            {
                hash = hash * seed + c;
                seed *= TB;
            }

            return hash;
        }
    }

    public enum DateType
    {
        Empty = 0,
        Boolean = 3,
        Char = 4,
        SByte = 5,
        Byte = 6,
        Int16 = 7,
        UInt16 = 8,
        Int32 = 9,
        UInt32 = 10,
        Int64 = 11,
        UInt64 = 12,
        Single = 13,
        Double = 14,
        Decimal = 15,
        String = 18,
        Message = 19
    }
}