namespace CodingMonkey.CodeExecutor.Security
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    enum PreExecutionSecurityCodes
    {
        FailedSanitisation = 9001
    }

    public class PreExecutionSecurity
    {
        public int LinesOfCodeAdded => SecurityLists.SafeNamespaces.Count;

        public string SanitiseCode(string codeToSanitise, int santisationLoopLimit = 5)
        {
            string sanitisedCode = codeToSanitise;
            string sanitisationFailedMessage = $"Compilation / Execution of code failed. Code: {(int)PreExecutionSecurityCodes.FailedSanitisation}";

            int totalSanitisationReplacementsMade = -1;
            int tryCount = 0;

            while (totalSanitisationReplacementsMade != 0)
            {
                if(tryCount >= santisationLoopLimit)
                {
                    throw new TimeoutException(sanitisationFailedMessage);
                }

                totalSanitisationReplacementsMade = -1;
                int noOfTypeReplacementsMade = 0;
                int noOfNamespaceReplacementsMade = 0;

                sanitisedCode = this.SanitiseTypes(sanitisedCode, out noOfTypeReplacementsMade);
                sanitisedCode = this.SanitiseNamespaces(sanitisedCode, out noOfNamespaceReplacementsMade);

                totalSanitisationReplacementsMade = noOfTypeReplacementsMade + noOfNamespaceReplacementsMade;

                tryCount++;
            }

            int noOfSafeUsingStatementsAdded = 0;
            sanitisedCode = this.AddMissingSafeUsings(sanitisedCode, out noOfSafeUsingStatementsAdded);

            return sanitisedCode;
        }

        /// <summary>
        /// Adds any safe using statements which are not currently in
        /// the code.
        /// </summary>
        /// <param name="codeToSanitise"></param>
        /// <returns></returns>
        private string AddMissingSafeUsings(string codeToSanitise, out int noOfAdditionsMade)
        {
            string sanitisedCode = codeToSanitise;
            noOfAdditionsMade = 0;

            IList<string> safeUsingStatements = SecurityLists.SafeUsingStatements;

            foreach (var safeUsingStatement in safeUsingStatements)
            {
                // Add any using statements which are safe that the user hasn't added
                // Makes things easier for new developers who might not yet understand
                // using statements - makes things a little more script like.
                if (!sanitisedCode.Contains(safeUsingStatement))
                {
                    sanitisedCode = safeUsingStatement + Environment.NewLine + sanitisedCode;
                    noOfAdditionsMade++;
                }
            }

            return sanitisedCode;
        }

        private string SanitiseNamespaces(string codeToSanitise, out int noOfReplacementsMade)
        {
            string sanitisedCode = codeToSanitise;
            noOfReplacementsMade = 0;

            foreach (var bannedNamespace in SecurityLists.BannedNamespaces)
            {
                string nsPatternIncludingExtraWhitespace = this.GetNamspaceRegexPatternIgnoreSpaces(bannedNamespace);
                string nsPatternIncludingExtraWhitespaceAndTrailingDot = nsPatternIncludingExtraWhitespace + @"\s*[.]";

                var nsMatch = Regex.Matches(sanitisedCode, nsPatternIncludingExtraWhitespace);
                var nsWithTrailingDotMatch = Regex.Matches(sanitisedCode, nsPatternIncludingExtraWhitespaceAndTrailingDot);
                int totalMatches = nsMatch.Count + nsWithTrailingDotMatch.Count;

                if(totalMatches > 0)
                {
                    var matches = this.CombineMatchCollections(nsMatch, nsWithTrailingDotMatch);
                    sanitisedCode = this.RemoveMatchesInMatchCollectionFromCode(sanitisedCode, matches);
                }

                noOfReplacementsMade += totalMatches;
            }

            return sanitisedCode;
        }

        private string SanitiseTypes(string codeToSanitise, out int noOfReplacementsMade)
        {
            string sanitisedCode = codeToSanitise;
            noOfReplacementsMade = 0;

            foreach (var bannedType in SecurityLists.BannedTypes)
            {
                // Remove the notiation that this type takes generics args
                string bannedTypeToSearchFor = Regex.Replace(bannedType, @"`\d", "");
                string bannedTypeToSearchForPatternWithDot = $"{bannedType}\\s*[.]";

                var typeWithTrailingDotMatches = Regex.Matches(sanitisedCode, bannedTypeToSearchForPatternWithDot);
                var typeMatches = Regex.Matches(sanitisedCode, bannedTypeToSearchFor);
                int totalMatches = typeWithTrailingDotMatches.Count + typeMatches.Count;

                if (totalMatches > 0)
                {
                    var matches = this.CombineMatchCollections(typeMatches, typeWithTrailingDotMatches);
                    sanitisedCode = this.RemoveMatchesInMatchCollectionFromCode(sanitisedCode, matches);
                }

                noOfReplacementsMade += totalMatches;
            }

            return sanitisedCode;
        }

        private string GetNamspaceRegexPatternIgnoreSpaces(string namespaceToSearchFor)
        {
            string[] namespaceParts = namespaceToSearchFor.Split('.');

            string regexPattern = namespaceParts[0] + @"\s*";

            for (int i = 1; i < namespaceParts.Length; i++)
            {
                regexPattern = regexPattern + "[.]" + @"\s*" + namespaceParts[i];
            }

            return regexPattern;
        }

        private string RemoveMatchesInMatchCollectionFromCode(string code, IEnumerable<Match> matches)
        {
            if (matches.Count() > 0)
            {
                foreach (var match in matches)
                {
                    code = code.Replace(match.Value, "");
                }
            }

            return code;
        }

        private IEnumerable<Match> CombineMatchCollections(MatchCollection firstMatchCollection, MatchCollection secondMatchCollection)
        {
            return firstMatchCollection.OfType<Match>()
                                       .Concat(secondMatchCollection.OfType<Match>())
                                       .Where(m => m.Success);
        }
    }
}
