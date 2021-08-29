using System;

namespace Reading
{
    public interface IBasicReader
    {
        long ReadInt64();
        bool ReadBool();
    }

    public enum Values : long
    {
        A = 1,
        B = 2
    }

    public class BhkThing<T>
    {
        public bool IsProp;
        public long PropCount;
        public long[,] Props;

        public long ComparA;
        public long ComparB;

        public Values MyValue;

        public BhkThing(IBasicReader reader)
        {
            IsProp = reader.ReadBool();
            MyValue = (Values)reader.ReadInt64();
            if (IsProp && true && (!false))
            {
                PropCount = reader.ReadInt64();
                Props = new long[PropCount, 2];
                for (long i = 0; i < PropCount; i++)
                {
                    Props[i, 0] = reader.ReadInt64();
                }
            }
            ComparA = reader.ReadInt64();
            ComparB = reader.ReadInt64();
            if ((long)MyValue == 1L)
                Console.WriteLine(1);

            if (ComparA > ComparB)
                Console.WriteLine("A");

            if (ComparA < ComparB)
                Console.WriteLine("B");

            if (ComparA <= ComparB)
                Console.WriteLine("AA");

            if (ComparA >= ComparB)
                Console.WriteLine("BB");

            if (ComparA != ComparB)
                Console.WriteLine("AAA");

            if (ComparA == ComparB)
                Console.WriteLine("BBB");

            if (IsProp && ComparA == 22)
                Console.WriteLine("C");

            if (ComparA != ComparB)
                Console.WriteLine("D");
        }
    }

    public class NiRoot
    {
        public NiRoot(IBasicReader reader)
        {

        }
    }

    public class NiThing : NiRoot
    {
        BhkThing<int> Thing;

        public NiThing(IBasicReader reader) : base(reader)
        {
            Thing = new BhkThing<int>(reader);
        }
    }
}
