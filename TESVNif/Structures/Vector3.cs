﻿using System;
using SSE.TESVNif.BlockStructure;

namespace SSE.TESVNif.Structures
{
    public class Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3(CompoundData data)
        {
            X = data.GetBasic<float>("x");
            Y = data.GetBasic<float>("y");
            Z = data.GetBasic<float>("z");
        }
    }
}