using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RPGCreator.Generators
{
    class SyntaxReceiver: ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classDeclaration &&
                classDeclaration.Members.OfType<MethodDeclarationSyntax>()
                    .Any(m => m.AttributeLists.Count > 0))
            {
                CandidateClasses.Add(classDeclaration);
            }
        }
    }
}