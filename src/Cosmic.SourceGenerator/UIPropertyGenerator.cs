using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Cosmic.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Cosmic.SourceGenerator;

[Generator]
public class UIPropertyGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not UIStructReciever reciever)
            return;

        var structs = reciever.Structs;

        foreach(var @struct in structs)
        {
            if (!ValidatePartialKeyword(@struct, context))
                continue;

            GenerateUIProperties(@struct, context);
        }
    }

    private void GenerateUIProperties(StructDeclarationSyntax @struct, GeneratorExecutionContext context)
    {
        using StringWriter ss = new();
        using IndentedTextWriter writer = new(ss);

        var properties = new List<PropertyDeclarationSyntax>();
        foreach(var prop in @struct.Members.OfType<PropertyDeclarationSyntax>())
        {
            var attribs = prop.DescendantNodes().OfType<AttributeSyntax>();
            foreach(var attrib in attribs)
            {
                if(attrib.Name.ToString().EndsWith(UIStructReciever.GetAttributeShortName<UIPropertyAttribute>()))
                {
                    properties.Add(prop);
                }
            }
        }

        if (properties.Count == 0)
            return;

        writer.WriteLine(GetUsings(@struct));

        var @namespace = GetNamespace(@struct);
        
        if(@namespace != null)
        {
            writer.WriteLine($"namespace {@namespace};");
        }

        writer.WriteLine($"{@struct.Modifiers} {@struct.Keyword.ValueText} {@struct.Identifier}");
        writer.WriteLine("{");
        writer.Indent++;

         
        foreach (var prop in properties)
        {
            writer.WriteLine($"public ref {@struct.Identifier.ValueText} {ToTitleCase(prop.Identifier.ValueText)}({prop.Type} {prop.Identifier.ValueText})");
            writer.WriteLine("{");
            writer.Indent++;
            writer.WriteLine($"this.{prop.Identifier.ValueText} = {prop.Identifier.ValueText};");
            writer.WriteLine($"return ref this;");
            writer.Indent--;
            writer.WriteLine("}");

            writer.WriteLine();
        }

        writer.Indent--;
        writer.WriteLine("}");
        context.AddSource($"{@struct.Identifier.ValueText}.gen.cs", SourceText.From(ss.ToString(), Encoding.UTF8));
    }
    private bool ValidatePartialKeyword(StructDeclarationSyntax @struct, GeneratorExecutionContext context)
    {

        if (!@struct.Modifiers.Any(x => x.ValueText == "partial"))
        {
            ReportDiagonostics("No partial access modifiers found on type ", @struct, context);
            return false;
        }
        return true;
    }

    public string GetUsings(StructDeclarationSyntax syntax)
    {
        var usings = syntax.SyntaxTree.GetRoot().DescendantNodes().OfType<UsingDirectiveSyntax>();
        return string.Join(string.Empty, usings.Select(u => u.ToFullString()));
    }

    public string GetNamespace(StructDeclarationSyntax @struct)
    {
        return GetNamespaceInternal(@struct);
    }

    private string GetNamespaceInternal<T>(T node) where T : SyntaxNode
    {
        var parent = node.Parent;
        while (parent.IsKind(SyntaxKind.ClassDeclaration) || parent.IsKind(SyntaxKind.StructDeclaration)
            || parent.IsKind(SyntaxKind.RecordDeclaration) || parent.IsKind(SyntaxKind.RecordStructDeclaration))
        {
            parent = parent.Parent;
        }
        if (parent is NamespaceDeclarationSyntax ns)
            return ns.Name.ToString();
        if (parent is FileScopedNamespaceDeclarationSyntax fs)
            return fs.Name.ToString();
        return null;
    }
    public static string ToTitleCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(input.ToLower());
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new UIStructReciever());
    }

    private void ReportDiagonostics(string Msg, StructDeclarationSyntax data, GeneratorExecutionContext context)
    {
        ISymbol symbol = context.Compilation
            .GetSemanticModel(data.SyntaxTree).GetDeclaredSymbol(data);

        context.ReportDiagnostic(Diagnostic.Create(
            new DiagnosticDescriptor(
                "SG0001",
                "partial attribute missing",
                $"{Msg} -> '{symbol.Name}'",
                "Error",
                DiagnosticSeverity.Error,
                true), symbol.Locations.FirstOrDefault(),
            symbol.Name, symbol.Name));
    }
}
