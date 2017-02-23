﻿using System.Text.RegularExpressions;

namespace Lombiq.Vsix.Orchard.Services
{
    public class FieldNameFromGenericTypeGenerator : FieldNameFromGenericTypeGeneratorBase
    {
        private const string GenericTypeNameRegexPattern = @"^[A-Z_]+[a-zA-Z0-9_]*[<]+[a-zA-Z_]+[a-zA-Z0-9_]*[>]+$";


        public override double Priority { get { return 10; } }


        public override bool CanGenerate(string dependency) { return Regex.IsMatch(dependency, GenericTypeNameRegexPattern); }

        public override string Generate(string dependency, bool useShortName)
        {
            var segments = GetGenericTypeSegments(dependency);

            return useShortName ? 
                    GetShortNameWithUnderscore(segments.CleanedGenericParameterName) + GetShortName(segments.CleanedGenericTypeName) : 
                    GetStringWithUnderscore(GetCamelCased(segments.CleanedGenericParameterName)) + segments.CleanedGenericTypeName;
        }
    }
}
