using System;
using System.Collections.Generic;
using Cosmic.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cosmic.SourceGenerator;

public class UIStructReciever: ISyntaxReceiver
{
    public HashSet<StructDeclarationSyntax> Structs { get; private set; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is AttributeSyntax attribute)
        {
            if (attribute.Name.ToString().EndsWith(GetAttributeShortName<UIStructAttribute>())
                && attribute.Parent is AttributeListSyntax attributes)

                if (attributes.Parent is StructDeclarationSyntax members)
                {
                    Structs.Add(members);
                }
        }
    }

    internal static string GetAttributeShortName<T>() where T : Attribute =>
         typeof(T).Name.Replace("Attribute", string.Empty);

}
