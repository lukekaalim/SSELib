using System;
using System.Collections.Generic;

namespace SSE.TESVNif.BlockStructure.Logic
{
    public abstract class Syntax : IEquatable<Syntax>
    {
        public bool Equals(Syntax other)
        {
            return GetType() == other.GetType();
        }
        public override int GetHashCode()
        {
            return 0;
        }

        public class Comparer : IEqualityComparer<Syntax>
        {
            public bool Equals(Syntax x, Syntax y)
            {
                return x.GetType() == y.GetType();
            }

            public int GetHashCode(Syntax obj)
            {
                return 0;
            }
        }

        public abstract class Punchuator : Syntax
        {
            public class OpenParen : Punchuator { }
            public class CloseParen : Punchuator { }
        }
        public abstract class Operator : Syntax
        {
            public abstract class Numerical : Operator
            {
                public class Addition : Numerical { }
                public class Subtraction : Numerical { }
                public class Multiplication : Numerical { }
                public class Division : Numerical { }
            }
            public abstract class Equatable : Operator
            {
                public class Equal : Equatable { }
                public class NotEqual : Equatable { }

                public class LessThan : Equatable { }
                public class GreaterThan : Equatable { }

                public class LessThanOrEqual : Equatable { }
                public class GreaterThanOrEqual : Equatable { }
            }
            public abstract class Logical : Operator
            {
                public class And : Logical { }
                public class Or : Logical { }
                public class Not : Logical { }
            }
            public abstract class Bitwise : Operator
            {
                public class And : Bitwise { }
                public class Or : Bitwise { }
                public class ShiftLeft : Bitwise { }
                public class ShiftRight : Bitwise { }
            }
            public abstract class Structural : Operator
            {
                public class Member : Structural { }
            }
        }
    }
}
