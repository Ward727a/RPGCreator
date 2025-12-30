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
                .Any(m => m.AttributeLists.Count > 0) ||
                classDeclaration.Members.OfType<PropertyDeclarationSyntax>()
                .Any(p => p.AttributeLists.Count > 0) ||
                classDeclaration.Members.OfType<EventDeclarationSyntax>()
                .Any(e => e.AttributeLists.Count > 0);

            if (!hasAttribute) return null;

            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
            
            if (classSymbol is not INamedTypeSymbol namedSymbol) return null;

            bool containsTargetAttribute = 
                namedSymbol.GetMembers().OfType<IMethodSymbol>()
                .Any(m => m.GetAttributes().Any(a => a.AttributeClass?.Name is "ExposeToPluginAttribute")) ||
                namedSymbol.GetMembers().OfType<IPropertySymbol>()
                    .Any(p => p.GetAttributes().Any(a => a.AttributeClass?.Name is "ExposePropToPluginAttribute")) ||
                namedSymbol.GetMembers().OfType<IEventSymbol>()
                    .Any(e => e.GetAttributes().Any(a => a.AttributeClass?.Name is "ExposeEventToPluginAttribute"));

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
            var baseNamespace = "RPGCreator.SDK.Contexts";

            sb.AppendLine("using System;");
            sb.AppendLine("using Avalonia.Controls;");
            sb.AppendLine($"namespace {baseNamespace}");
            sb.AppendLine("{");

            var methods = classSymbol.GetMembers().OfType<IMethodSymbol>()
                .Where(m => m.GetAttributes().Any(a => a.AttributeClass?.Name == "ExposeToPluginAttribute"));
            var properties = classSymbol.GetMembers().OfType<IPropertySymbol>()
                .Where(p => p.GetAttributes().Any(a => a.AttributeClass?.Name == "ExposePropToPluginAttribute"));
            var events = classSymbol.GetMembers().OfType<IEventSymbol>()
                .Where(e => e.GetAttributes().Any(a => a.AttributeClass?.Name == "ExposeEventToPluginAttribute"));

            // Combine all members
            var allMembers = methods.Select(m => new { 
                    Symbol = (ISymbol)m, 
                    Region = m.GetAttributes().First(a => a.AttributeClass.Name == "ExposeToPluginAttribute").ConstructorArguments[0].Value.ToString(),
                    Type = "Method"
                })
                .Concat(properties.Select(p => new { 
                    Symbol = (ISymbol)p, 
                    Region = p.GetAttributes().First(a => a.AttributeClass.Name == "ExposePropToPluginAttribute").ConstructorArguments[0].Value.ToString(),
                    Type = "Property"
                })
                .Concat(events.Select(e => new { 
                    Symbol = (ISymbol)e, 
                    Region = e.GetAttributes().First(a => a.AttributeClass.Name == "ExposeEventToPluginAttribute").ConstructorArguments[0].Value.ToString(),
                    Type = "Event"
            })));
            
            // Group by Region
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
                
                // Make Event Fields
                foreach (var item in group.Where(x => x.Type == "Event"))
                {
                    var evt = (IEventSymbol)item.Symbol;
                    var delegateType = evt.Type.ToString(); // ex: System.EventHandler
            
                    // On stocke les actions d'abonnement/désabonnement
                    sb.AppendLine($"        private readonly Action<{delegateType}> _add{evt.Name};");
                    sb.AppendLine($"        private readonly Action<{delegateType}> _remove{evt.Name};");
                }

                sb.AppendLine();
                
                // Make Config Class
                sb.AppendLine($"        /// <summary>Configuration object to initialize the context.</summary>");
                sb.AppendLine($"        public class Config");
                sb.AppendLine("        {");
                // A. Config pour Méthodes
                foreach (var item in group.Where(x => x.Type == "Method"))
                {
                    var method = (IMethodSymbol)item.Symbol;
                    var paramTypes = string.Join(", ", method.Parameters.Select(p => p.Type.ToString()));
                    string actionType = "";
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
                    // On utilise 'required' (C# 11) ou juste public pour forcer l'assignation
                    sb.AppendLine($"            public {actionType} {method.Name} {{ get; set; }} = null!;");
                }

                foreach (var item in group.Where(x => x.Type == "Property"))
                {
                    var prop = (IPropertySymbol)item.Symbol;
                    var propType = prop.Type.ToString();
                    bool canSet = (bool)prop.GetAttributes().First(a => a.AttributeClass.Name == "ExposePropToPluginAttribute").ConstructorArguments[1].Value;

                    sb.AppendLine($"            public Func<{propType}> Get{prop.Name} {{ get; set; }} = null!;");
                    if (canSet)
                    {
                        sb.AppendLine($"            public Action<{propType}> Set{prop.Name} {{ get; set; }} = null!;");
                    }
                }

                foreach (var item in group.Where(x => x.Type == "Event"))
                {
                    var evt = (IEventSymbol)item.Symbol;
                    var delegateType = evt.Type.ToString();
                    sb.AppendLine($"            public Action<{delegateType}> Add{evt.Name} {{ get; set; }} = null!;");
                    sb.AppendLine($"            public Action<{delegateType}> Remove{evt.Name} {{ get; set; }} = null!;");
                }

                sb.AppendLine("        }");
                sb.AppendLine();
                
                // Make Constructors
                sb.AppendLine($"        public {contextName}(Config config)");
                sb.AppendLine("        {");
                sb.AppendLine("            if (config == null) throw new ArgumentNullException(nameof(config));");
                sb.AppendLine();
                
                // Assign Methods
                foreach (var item in group.Where(x => x.Type == "Method"))
                {
                    var method = (IMethodSymbol)item.Symbol;
                    sb.AppendLine($"            _{method.Name} = config.{method.Name};");
                }

                // Assign Properties
                foreach (var item in group.Where(x => x.Type == "Property"))
                {
                    var prop = (IPropertySymbol)item.Symbol;
                    bool canSet = (bool)prop.GetAttributes().First(a => a.AttributeClass.Name == "ExposePropToPluginAttribute").ConstructorArguments[1].Value;

                    sb.AppendLine($"            _get{prop.Name} = config.Get{prop.Name};");
                    if (canSet) sb.AppendLine($"            _set{prop.Name} = config.Set{prop.Name};");
                }

                // Assign Events
                foreach (var item in group.Where(x => x.Type == "Event"))
                {
                    var evt = (IEventSymbol)item.Symbol;
                    sb.AppendLine($"            _add{evt.Name} = config.Add{evt.Name};");
                    sb.AppendLine($"            _remove{evt.Name} = config.Remove{evt.Name};");
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
                
                // Make Events
                foreach (var item in group.Where(x => x.Type == "Event"))
                {
                    var evt = (IEventSymbol)item.Symbol;
                    var delegateType = evt.Type.ToString();

                    // On génère un event proxy
                    sb.AppendLine($"        public event {delegateType} {evt.Name}");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            add => _add{evt.Name}(value);");
                    sb.AppendLine($"            remove => _remove{evt.Name}(value);");
                    sb.AppendLine("        }");
                    sb.AppendLine();
                }
                
                sb.AppendLine("    }"); // Class closure
                sb.AppendLine();
            }

            sb.AppendLine("}"); // Namespace closure
            return sb.ToString();
        }
    }
}