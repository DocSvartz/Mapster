using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapster.Compile.Configuration.Inspectors
{
    public class TypeInspector : IEquatable<TypeInspector>
    {
        public TypeInspector (ITypeSymbol typeSymbol)
        {
            IsPartial = getPartial (typeSymbol);
            Type = (INamedTypeSymbol)typeSymbol;
        }

        public INamedTypeSymbol Type { get; }
        public bool IsPartial { get; }
        public IEnumerable<ISymbol> Members { get => Type.GetMembers().Where(x => (x is IPropertySymbol || x is IFieldSymbol) && !x.MetadataName.EndsWith(">k__BackingField")); }
        public IEnumerable<IMethodSymbol> Constructors { get => Type.Constructors; }
        public IEnumerable<IFieldSymbol> BackFields { get => Type.GetMembers().OfType<IFieldSymbol>().Where(x=> x.MetadataName.EndsWith(">k__BackingField")); }

        public override bool Equals(object obj)
        {
            return Equals(obj as TypeInspector);
        }

        public bool Equals(TypeInspector other)
        {
            return other is not null &&
                   EqualityComparer<INamedTypeSymbol>.Default.Equals(Type, other.Type);
        }

        public override int GetHashCode()
        {
            return 2049151605 + EqualityComparer<INamedTypeSymbol>.Default.GetHashCode(Type);
        }

        private bool getPartial (ITypeSymbol type)
        {
            foreach (var reference in type.DeclaringSyntaxReferences)
            {
                var node = reference.GetSyntax();
                if (node is ClassDeclarationSyntax classDecl &&
                    classDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool operator ==(TypeInspector left, TypeInspector right)
        {
            return EqualityComparer<TypeInspector>.Default.Equals(left, right);
        }

        public static bool operator !=(TypeInspector left, TypeInspector right)
        {
            return !(left == right);
        }
    }
}
