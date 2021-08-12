using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;

using BlockStructure.Logic;

namespace BlockStructure.Schemas
{
    public class FieldSchema
    {
        public string Name { get; set; }
        public string Type { get; set; }

        public int? MinVersion { get; set; }
        public int? MaxVersion { get; set; }

        public List<string> Dimensions { get; set; }

        public string Condition { get; set; }
        public string VersionCondition { get; set; }
        public string OnlyT { get; set; }
        public string ExcludeT { get; set; }

        public string Argument { get; set; }
        public string Template { get; set; }

        public bool IsMultiDimensional => Dimensions.Count > 0;

        public FieldSchema(XElement element)
        {
            Name = element.Attribute("name").Value;
            Type = element.Attribute("type").Value;

            MinVersion = VersionParser.Parse(element.Attribute("ver1"));
            MaxVersion = VersionParser.Parse(element.Attribute("ver2"));

            Dimensions = new string[]
            {
                element.Attribute("arr1")?.Value,
                element.Attribute("arr2")?.Value,
            }.Where(a => a != null).ToList();

            Condition = element.Attribute("cond")?.Value;
            VersionCondition = element.Attribute("vercond")?.Value;
            OnlyT = element.Attribute("onlyT")?.Value;
            ExcludeT = element.Attribute("excludeT")?.Value;

            Argument = element.Attribute("arg")?.Value;
            Template = element.Attribute("template")?.Value;
        }

        public override string ToString()
        {
            return $"{Name} ({Type}{(Template == null ? "" : $"<{Template}>")}{(IsMultiDimensional ? "[]" : "")})";
        }

        public Expression BuildConditionExpression(TokenLookup tokens)
        {
            if (Condition == null)
                return null;
            var subsitutedSource = tokens.ResolveSubsitutions(Condition, "cond");
            var lexedTokens = Lexer.ReadSource(subsitutedSource);
            var expression = Parser.Parse(lexedTokens);
            return expression;
        }

        public Expression BuildVersionConditionExpression(TokenLookup tokens)
        {
            if (VersionCondition == null)
                return null;
            var subsitutedSource = tokens.ResolveSubsitutions(VersionCondition, "vercond");
            var lexedTokens = Lexer.ReadSource(subsitutedSource);
            var expression = Parser.Parse(lexedTokens);
            return expression;
        }

        public bool InVersionRange(VersionKey key)
        {
            if (MaxVersion != null && key.NifVersion > MaxVersion)
                return false;

            if (MinVersion != null && key.NifVersion < MinVersion)
                return false;

            return true;
        }
    }
}
