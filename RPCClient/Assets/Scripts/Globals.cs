using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public static class Globals
    {
        public static readonly int DATA_SZIE = 1024 * 8;
        public static readonly int BUFFER_SIZE = 1024 * 100;
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