using System;
using System.Collections.Generic;
using Orion.Util.NamedValues;

namespace FpML.V5r10.CodeListTool
{
    public class TemplateScope
    {
        public readonly int NestLevel;
        public readonly NamedValueSet Tokens;
        public readonly TemplateIterator Iterator;
        public readonly int IterationStartLine;
        public readonly int IterationNumber;
        public TemplateScope(
            int nestLevel, NamedValueSet tokens, TemplateIterator iterator, 
            int iterationStartLine, int iterationNumber)
        {
            NestLevel = nestLevel;
            Tokens = new NamedValueSet(tokens);
            Iterator = iterator;
            IterationStartLine = iterationStartLine;
            IterationNumber = iterationNumber;
        }
    }

    public class TemplateIteration
    {
        public readonly NamedValueSet Tokens;
        public readonly Dictionary<string, TemplateIterator> SubIterators;
        public TemplateIteration()
        {
            Tokens = new NamedValueSet();
            SubIterators = new Dictionary<string, TemplateIterator>();
        }
    }
    public class TemplateIterator
    {
        public readonly TemplateIteration[] Iterations;
        public TemplateIterator(int iterationCount)
        {
            Iterations = new TemplateIteration[iterationCount];
            for (int i = 0; i < iterationCount; i++)
                Iterations[i] = new TemplateIteration();
        }
    }

    public class TemplateProcessor
    {
        // default config for C# files
        public string CommentPrefix = "//"; // use "<!--" for XML/XSD
        public string CommentSuffix = null; // use "-->" for XML/XSD

        public string[] ProcessTemplate(string[] template, TemplateIterator outerIterator)
        {
            var output = new List<string>();
            var savedScopes = new Stack<TemplateScope>();
            TemplateIterator currentIterator = outerIterator;
            int iterationStartLine = 0;
            int iterationNumber = 0;
            var currentTokens = new NamedValueSet(currentIterator.Iterations[iterationNumber].Tokens);
            int currentLine = 0;
            int nestLevel = 0;
            while (currentLine < template.Length)
            {
                string trimmedLine = template[currentLine].Trim();
                // check for template directive
                bool specialLine = false;
                if (((CommentPrefix == null) || trimmedLine.StartsWith(CommentPrefix))
                    && ((CommentSuffix == null) || trimmedLine.EndsWith(CommentSuffix)))
                {
                    // comment line - might have a template directive in it
                    if (CommentPrefix != null)
                        trimmedLine = trimmedLine.Substring(CommentPrefix.Length);
                    if (CommentSuffix != null)
                        trimmedLine = trimmedLine.Substring(0, trimmedLine.Length - CommentSuffix.Length);
                    trimmedLine = trimmedLine.Trim();
                    if (trimmedLine.StartsWith("##"))
                    {
                        // directive found
                        specialLine = true;
                        string[] directiveParts = trimmedLine.Substring(2).Split(':');
                        string directive = directiveParts[0].ToLower().Trim();
                        switch (directive)
                        {
                            case "foreach":
                                // start of for loop block
                                // - save scope and loop
                                {
                                    string iteratorName = directiveParts[1].Trim();
                                    savedScopes.Push(new TemplateScope(nestLevel, currentTokens, 
                                        currentIterator, iterationStartLine, iterationNumber));
                                    iterationStartLine = currentLine;
                                    if(!currentIterator.Iterations[iterationNumber].SubIterators.ContainsKey(iteratorName))
                                        throw new ApplicationException(
                                            $"Unknown iterator name '{iteratorName}' in template (line {currentLine}).");
                                    currentIterator = currentIterator.Iterations[iterationNumber].SubIterators[iteratorName];
                                    // 1st iteration at new level
                                    nestLevel++;
                                    iterationNumber = 0;
                                    currentTokens.Add(currentIterator.Iterations[iterationNumber].Tokens);
                                }
                                break;
                            case "end":
                                // end of block - exit or loop
                                if ((currentIterator != null) && ((iterationNumber + 1) < currentIterator.Iterations.Length))
                                {
                                    // restart loop
                                    currentLine = iterationStartLine;
                                    iterationNumber++;
                                    currentTokens.Add(currentIterator.Iterations[iterationNumber].Tokens);
                                }
                                else
                                {
                                    // pop scope
                                    TemplateScope oldScope = savedScopes.Pop();
                                    nestLevel = oldScope.NestLevel;
                                    currentTokens = oldScope.Tokens;
                                    currentIterator = oldScope.Iterator;
                                    iterationStartLine = oldScope.IterationStartLine;
                                    iterationNumber = oldScope.IterationNumber;
                                }
                                break;
                            default:
                                throw new ApplicationException(
                                    $"Unknown directive '{directive}' in template (line {currentLine}).");
                        }
                    }
                }
                if (!specialLine)
                {
                    // ordinary input line
                    currentTokens.Set("SourceLine", currentLine);
                    currentTokens.Set("OutputLine", output.Count);
                    currentTokens.Set("NestLevel", nestLevel);
                    output.Add(currentTokens.ReplaceTokens(template[currentLine]));
                }
                currentLine++;
            }
            if (savedScopes.Count > 0)
                throw new ApplicationException("Not enough matching ##end directives in template!");
            return output.ToArray();
        }
    }
}
