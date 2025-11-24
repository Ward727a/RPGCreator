using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace RPGCreator.Generators
{
    /// <summary>
    /// This source generator creates context classes for methods marked with the ExposeToPluginAttribute.<br/>
    /// These context classes group methods by their specified region and provide a structured way to access them from CSharp modules.
    /// </summary>
    [Generator]
    public class ContextGenerator: IIncrementalGenerator 
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, _) => IsSyntaxTargetForGeneration(node),
                    transform: (ctx, _) => GetSemanticTargetForGeneration(ctx)
                )
                .Where(m => m != null);

            context.RegisterSourceOutput(classDeclarations, (spc, source) => Execute(spc, source));
        }

        private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        {
            return node is ClassDeclarationSyntax c && c.Members.Count > 0;
        }

        private static INamedTypeSymbol? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;

            var hasAttribute = classDeclaration.Members.OfType<MethodDeclarationSyntax>()
                .Any(m => m.AttributeLists.Count > 0);

            if (!hasAttribute) return null;

            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
            
            if (classSymbol is not INamedTypeSymbol namedSymbol) return null;

            bool containsTargetAttribute = namedSymbol.GetMembers().OfType<IMethodSymbol>()
                .Any(m => m.GetAttributes().Any(a => a.AttributeClass?.Name == "ExposeToPluginAttribute"));

            return containsTargetAttribute ? namedSymbol : null;
        }

        private static void Execute(SourceProductionContext context, INamedTypeSymbol? classSymbol)
        {
            if (classSymbol == null) return;

            var sourceCode = GenerateContextClasses(classSymbol); 
            
            context.AddSource($"{classSymbol.Name}Context.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
        }

        private static string GenerateContextClasses(INamedTypeSymbol classSymbol)
        {
            var sb = new StringBuilder();
            var baseNamespace = "RPGCreator.Core.Contexts";

            sb.AppendLine("using System;");
            sb.AppendLine("using Avalonia.Controls;");
            sb.AppendLine($"namespace {baseNamespace}");
            sb.AppendLine("{");

            var methods = classSymbol.GetMembers().OfType<IMethodSymbol>()
                .Where(m => m.GetAttributes().Any(a => a.AttributeClass?.Name == "ExposeToPluginAttribute"));
            var properties = classSymbol.GetMembers().OfType<IPropertySymbol>()
                .Where(p => p.GetAttributes().Any(a => a.AttributeClass?.Name == "ExposePropToPluginAttribute"));

            var allMembers = methods.Select(m => new { 
                    Symbol = (ISymbol)m, 
                    Region = m.GetAttributes().First(a => a.AttributeClass.Name == "ExposeToPluginAttribute").ConstructorArguments[0].Value.ToString(),
                    Type = "Method"
                })
                .Concat(properties.Select(p => new { 
                    Symbol = (ISymbol)p, 
                    Region = p.GetAttributes().First(a => a.AttributeClass.Name == "ExposePropToPluginAttribute").ConstructorArguments[0].Value.ToString(),
                    Type = "Property"
                }));
            var groupedMembers = allMembers.GroupBy(x => x.Region);

            foreach (var group in groupedMembers)
            {
                string regionId = group.Key;
                
                string safeRegionName = regionId.Replace(".", "").Replace(" ", "");
                string contextName = $"{safeRegionName}Context";

                sb.AppendLine($"    /// <summary>Context for region: {regionId}</summary>");
                sb.AppendLine($"    public class {contextName}");
                sb.AppendLine("    {");
                
                // Make Fields
                foreach (var item in group.Where(x => x.Type == "Method"))
                {
                    var method = (IMethodSymbol)item.Symbol;
                    var paramTypes = string.Join(", ", method.Parameters.Select(p => p.Type.ToString()));
                    string actionType;
                    if (method.ReturnsVoid)
                    {
                        if (method.Parameters.Length == 0)
                        {
                            actionType = "Action";
                        }
                        else
                        {
                            actionType = $"Action<{paramTypes}>";
                        }
                    }
                    else
                    {
                        if(method.Parameters.Length == 0)
                        {
                            actionType = $"Func<{method.ReturnType}>";
                        }
                        else
                        {
                            actionType = $"Func<{paramTypes}, {method.ReturnType}>";
                        }
                    }
                    sb.AppendLine($"        private readonly {actionType} _{method.Name};");
                }

                // Make Property Fields
                foreach (var item in group.Where(x => x.Type == "Property"))
                {
                    var prop = (IPropertySymbol)item.Symbol;
                    var propType = prop.Type.ToString();
                    var attrArgs = prop.GetAttributes().First(a => a.AttributeClass.Name == "ExposePropToPluginAttribute").ConstructorArguments;
                    bool canSet = (bool)attrArgs[1].Value;

                    // Champ pour le Getter : Func<Type>
                    sb.AppendLine($"        private readonly Func<{propType}> _get{prop.Name};");

                    // Champ pour le Setter : Action<Type> (Seulement si CanSet)
                    if (canSet)
                    {
                        sb.AppendLine($"        private readonly Action<{propType}> _set{prop.Name};");
                    }
                }
                // Make Constructors
                sb.AppendLine($"        public {contextName}(");
                var ctorParams = new List<string>();
                foreach (var item in group.Where(x => x.Type == "Method"))
                {
                    var method = (IMethodSymbol)item.Symbol;
                    var paramTypes = string.Join(", ", method.Parameters.Select(p => p.Type.ToString()));
                    string actionType;
                    if (method.ReturnsVoid)
                    {
                        if (method.Parameters.Length == 0)
                        {
                            actionType = "Action";
                        }
                        else
                        {
                            actionType = $"Action<{paramTypes}>";
                        }
                    }
                    else
                    {
                        if(method.Parameters.Length == 0)
                        {
                            actionType = $"Func<{method.ReturnType}>";
                        }
                        else
                        {
                            actionType = $"Func<{paramTypes}, {method.ReturnType}>";
                        }
                    }
                    ctorParams.Add($"            {actionType} {method.Name}Action");
                }

                // Make Properties Functions
                foreach (var item in group.Where(x => x.Type == "Property"))
                {
                    var prop = (IPropertySymbol)item.Symbol;
                    var propType = prop.Type.ToString();
                    bool canSet = (bool)prop.GetAttributes().First(a => a.AttributeClass.Name == "ExposePropToPluginAttribute").ConstructorArguments[1].Value;

                    ctorParams.Add($"            Func<{propType}> get{prop.Name}");
                    if (canSet)
                    {
                        ctorParams.Add($"            Action<{propType}> set{prop.Name}");
                    }
                }
                sb.AppendLine(string.Join(",\n", ctorParams));
                sb.AppendLine("        )");
                sb.AppendLine("        {");
                foreach (var item in group.Where(x => x.Type == "Method"))
                {
                    var method = (IMethodSymbol)item.Symbol;
                    sb.AppendLine($"            _{method.Name} = {method.Name}Action;");
                }
                // Assign Property Fields
                foreach (var item in group.Where(x => x.Type == "Property"))
                {
                    var prop = (IPropertySymbol)item.Symbol;
                    bool canSet = (bool)prop.GetAttributes().First(a => a.AttributeClass.Name == "ExposePropToPluginAttribute").ConstructorArguments[1].Value;

                    sb.AppendLine($"            _get{prop.Name} = get{prop.Name};");
                    if (canSet) sb.AppendLine($"            _set{prop.Name} = set{prop.Name};");
                }

                sb.AppendLine("        }");

                // Make Methods
                foreach (var item in group.Where(x => x.Type == "Method"))
                {
                    var method = (IMethodSymbol)item.Symbol;
                    string docXml = method.GetDocumentationCommentXml();

                    if (!string.IsNullOrWhiteSpace(docXml))
                    {
                        try
                        {
                            var xElement = XElement.Parse(docXml);
                            var summary = xElement.Element("summary")?.Value.Trim();

                            if (!string.IsNullOrWhiteSpace(summary))
                            {
                                sb.AppendLine("        /// <summary>");

                                var lines = summary.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var line in lines)
                                {
                                    sb.AppendLine($"        /// {line.Trim()}");
                                }

                                sb.AppendLine("        /// </summary>");
                            }

                            var paramsElements = xElement.Elements("param");
                            foreach (var param in paramsElements)
                            {
                                var paramName = param.Attribute("name")?.Value;
                                var paramDesc = param.Value.Trim();
                                if (!string.IsNullOrWhiteSpace(paramName) && !string.IsNullOrWhiteSpace(paramDesc))
                                {
                                    sb.AppendLine($"        /// <param name=\"{paramName}\">{paramDesc}</param>");
                                }
                            }

                            var returnsElement = xElement.Element("returns");
                            if (returnsElement != null)
                            {
                                var returnsDesc = returnsElement.Value.Trim();
                                if (!string.IsNullOrWhiteSpace(returnsDesc))
                                {
                                    sb.AppendLine($"        /// <returns>{returnsDesc}</returns>");
                                }
                            }
                        }
                        catch
                        {
                            // IGNORED
                        }
                    }

                    var obsoleteAttr = method.GetAttributes()
                        .FirstOrDefault(a =>
                            a.AttributeClass?.Name == "ObsoleteAttribute" &&
                            a.AttributeClass?.ContainingNamespace?.Name == "System");

                    if (obsoleteAttr != null)
                    {
                        string message = null;
                        bool isError = false;

                        if (obsoleteAttr.ConstructorArguments.Length > 0)
                        {
                            message = obsoleteAttr.ConstructorArguments[0].Value?.ToString();
                        }
                        
                        if (obsoleteAttr.ConstructorArguments.Length > 1)
                        {
                            isError = (bool)obsoleteAttr.ConstructorArguments[1].Value!;
                        }
                        
                        if (message != null)
                        {
                            string safeMessage = message.Replace("\"", "\\\"");
                            string errorBool = isError.ToString().ToLower();
                            sb.AppendLine($"        [Obsolete(\"{safeMessage}\", {errorBool})]");
                        }
                        else
                        {
                            sb.AppendLine("        [Obsolete]");
                        }
                    }
                    var parameters = string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}"));
                    var callArgs = string.Join(", ", method.Parameters.Select(p => p.Name));
                    var returnType = method.ReturnsVoid ? "void" : method.ReturnType.ToString();

                    sb.AppendLine($"        public {returnType} {method.Name}({parameters})");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            {(method.ReturnsVoid ? "" : "return ")}_{method.Name}({callArgs});");
                    sb.AppendLine("        }");
                }

                // Make Properties
                foreach (var item in group.Where(x => x.Type == "Property"))
                {
                    var prop = (IPropertySymbol)item.Symbol;
                    var propType = prop.Type.ToString();
                    bool canSet = (bool)prop.GetAttributes().First(a => a.AttributeClass.Name == "ExposePropToPluginAttribute").ConstructorArguments[1].Value;

                    sb.AppendLine($"        public {propType} {prop.Name}");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            get => _get{prop.Name}();");
                    if (canSet)
                    {
                        sb.AppendLine($"            set => _set{prop.Name}(value);");
                    }
                    sb.AppendLine("        }");
                }
                
                sb.AppendLine("    }"); // Class closure
                sb.AppendLine();
            }

            sb.AppendLine("}"); // Namespace closure
            return sb.ToString();
        }
    }
}