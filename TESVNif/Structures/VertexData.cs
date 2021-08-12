using System;

namespace SSE.TESVNif.Structures
{
	public readonly struct BSVertexDesc
    {
		public readonly ulong Value;
	}

	public class BSVertexData
    {

		/*
         * 
		<field name="Vertex" type="Vector3" cond="(#ARG# #BITAND# 0x401) == 0x401" />
		<field name="Bitangent X" type="float" cond="(#ARG# #BITAND# 0x411) == 0x411" />
		<field name="Unused W" type="uint" cond="(#ARG# #BITAND# 0x411) == 0x401" />

		<field name="Vertex" type="HalfVector3" cond="(#ARG# #BITAND# 0x401) == 0x1" />
		<field name="Bitangent X" type="hfloat" cond="(#ARG# #BITAND# 0x411) == 0x11" />
		<field name="Unused W" type="ushort" cond="(#ARG# #BITAND# 0x411) == 0x1" />

		<field name="UV" type="HalfTexCoord" cond="#ARG# #BITAND# 0x2" />
		<field name="Normal" type="ByteVector3" cond="#ARG# #BITAND# 0x8" />
		<field name="Bitangent Y" type="byte" cond="#ARG# #BITAND# 0x8" />
		<field name="Tangent" type="ByteVector3" cond="(#ARG# #BITAND# 0x18) == 0x18" />
		<field name="Bitangent Z" type="byte" cond="(#ARG# #BITAND# 0x18) == 0x18" />
		<field name="Vertex Colors" type="ByteColor4" cond="#ARG# #BITAND# 0x20" />
		<field name="Bone Weights" type="hfloat" arr1="4" cond="#ARG# #BITAND# 0x40" />
		<field name="Bone Indices" type="byte" arr1="4" cond="#ARG# #BITAND# 0x40" />
		<field name="Eye Data" type="float" cond="#ARG# #BITAND# 0x100" />
         */

		public Vector3 Vertex { get; set; }
		public HalfTexCoord UV { get; set; }
		public ByteVector3 Normal { get; set; }

		public BSVertexData(BlockStructure.CompoundData data)
		{
			Vertex = new Vector3(data.GetCompound("Vertex"));
			UV = data.Fields.ContainsKey("UV") ? new HalfTexCoord(data.GetCompound("UV")) : default(HalfTexCoord);
			Normal = data.Fields.ContainsKey("Normal") ? new ByteVector3(data.GetCompound("Normal")) : default(ByteVector3);
		}
    }

    public class BSVertexDataSSE
    {
        /*
         * 
		<field name="Vertex" type="Vector3" cond="#ARG# #BITAND# 0x1" />
		<field name="Bitangent X" type="float" cond="(#ARG# #BITAND# 0x11) == 0x11" />
		<field name="Unused W" type="uint" cond="(#ARG# #BITAND# 0x11) == 0x1" />
		<field name="UV" type="HalfTexCoord" cond="#ARG# #BITAND# 0x2" />
		<field name="Normal" type="ByteVector3" cond="#ARG# #BITAND# 0x8" />
		<field name="Bitangent Y" type="byte" cond="#ARG# #BITAND# 0x8" />
		<field name="Tangent" type="ByteVector3" cond="(#ARG# #BITAND# 0x18) == 0x18" />
		<field name="Bitangent Z" type="byte" cond="(#ARG# #BITAND# 0x18) == 0x18" />
		<field name="Vertex Colors" type="ByteColor4" cond="#ARG# #BITAND# 0x20" />
		<field name="Bone Weights" type="hfloat" arr1="4" cond="#ARG# #BITAND# 0x40" />
		<field name="Bone Indices" type="byte" arr1="4" cond="#ARG# #BITAND# 0x40" />
		<field name="Eye Data" type="float" cond="#ARG# #BITAND# 0x100" />
         */

        public Vector3 Vertex { get; set; }
		public HalfTexCoord UV { get; set; }
		public ByteVector3 Normal { get; set; }

		public BSVertexDataSSE(BlockStructure.CompoundData data)
        {
            Vertex = new Vector3(data.GetCompound("Vertex"));
			UV = new HalfTexCoord(data.GetCompound("UV"));
			Normal = data.Fields.ContainsKey("Normal") ? new ByteVector3(data.GetCompound("Normal")) : default(ByteVector3);
		}
    }
}
