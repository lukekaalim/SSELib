using System;
using System.IO;
using BlockStructure.Schemas;

namespace BlockStructure
{
    public delegate BasicData ReadBasicData(TypeKey key);

    public class Reader
    {
        SchemaDocument Document;
        ReadBasicData ReadBasicFunc;

        public Reader(SchemaDocument document, ReadBasicData readBasicFunc)
        {
            Document = document;
            ReadBasicFunc = readBasicFunc;
        }

        public Data Read(string typeName, string typeTemplate = null)
        {

        }

        public Data ReadCompound(CompoundSchema compound)
        {

        }

        public Data ReadNiObject()
        {

        }

        protected Data ReadField()
        {

        }
    }
}
