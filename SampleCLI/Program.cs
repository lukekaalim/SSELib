using System;

namespace SampleCLI
{
    class B : IBasicReader
    {
        public int ReadBlockTypeIndex()
        {
            throw new NotImplementedException();
        }

        public bool ReadBool()
        {
            throw new NotImplementedException();
        }

        public byte ReadByte()
        {
            return 1;
            throw new NotImplementedException();
        }

        public char ReadChar()
        {
            throw new NotImplementedException();
        }

        public int ReadFileVersion()
        {
            return 1;
            throw new NotImplementedException();
        }

        public float ReadFloat()
        {
            throw new NotImplementedException();
        }

        public string ReadHeaderString()
        {
            return "1";
            throw new NotImplementedException();
        }

        public float ReadHfloat()
        {
            throw new NotImplementedException();
        }

        public int ReadInt()
        {
            throw new NotImplementedException();
        }

        public long ReadInt64()
        {
            throw new NotImplementedException();
        }

        public string ReadLineString()
        {
            throw new NotImplementedException();
        }

        public uint ReadNiFixedString()
        {
            throw new NotImplementedException();
        }

        public int ReadPtr()
        {
            return 3;
        }

        public int ReadRef()
        {
            throw new NotImplementedException();
        }

        public short ReadShort()
        {
            throw new NotImplementedException();
        }

        public uint ReadStringOffset()
        {
            throw new NotImplementedException();
        }

        public uint ReadUint()
        {
            return 2;
        }

        public ulong ReadUint64()
        {
            return 1;
        }

        public uint ReadUlittle32()
        {
            return 1;
            throw new NotImplementedException();
        }

        public ushort ReadUshort()
        {
            return 1;
            throw new NotImplementedException();
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var b = new B();
            var bhk = new Header(b, 0);
            Console.WriteLine("Hello World!");
        }
    }
}
