﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CSharp.Symbols;
using System.Collections.Immutable;
using System.Diagnostics;
using Roslyn.Utilities;
using Microsoft.CodeAnalysis.ExpressionEvaluator;
using System;

namespace Microsoft.CodeAnalysis.CSharp.ExpressionEvaluator
{
    /// <summary>
    /// A display class field representing a local, exposed
    /// as a local on the original method.
    /// </summary>
    internal sealed class EEDisplayClassFieldLocalSymbol : EELocalSymbolBase
    {
        private readonly DisplayClassVariable _variable;

        public EEDisplayClassFieldLocalSymbol(DisplayClassVariable variable)
        {
            _variable = variable;

            // Verify all type parameters are substituted.
            Debug.Assert(this.ContainingSymbol.IsContainingSymbolOfAllTypeParameters(this.Type));
        }

        internal override EELocalSymbolBase ToOtherMethod(MethodSymbol method, TypeMap typeMap)
        {
            return new EEDisplayClassFieldLocalSymbol(_variable.ToOtherMethod(method, typeMap));
        }

        internal override LocalAndMethodKind LocalAndMethodKind
        {
            get
            {
                switch (_variable.Kind)
                {
                case DisplayClassVariableKind.Local:
                    return LocalAndMethodKind.Local;
                case DisplayClassVariableKind.Parameter:
                    return LocalAndMethodKind.Parameter;
                case DisplayClassVariableKind.This:
                    return LocalAndMethodKind.This;
                default:
                    throw new InvalidOperationException();
                }
            }
        }

        internal override int Index
        {
            get
            {
                switch (_variable.Kind)
                {
                case DisplayClassVariableKind.Local:
                    return -1;
                case DisplayClassVariableKind.Parameter:
                    return -1;
                case DisplayClassVariableKind.This:
                    return 0;
                default:
                    throw new InvalidOperationException();
                }
            }
        }

        public override string Name
        {
            get { return _variable.Name; }
        }

        internal override LocalDeclarationKind DeclarationKind
        {
            get { return LocalDeclarationKind.RegularVariable; }
        }

        internal override SyntaxToken IdentifierToken
        {
            get { throw ExceptionUtilities.Unreachable; }
        }

        public override Symbol ContainingSymbol
        {
            get { return _variable.ContainingSymbol; }
        }

        public override TypeSymbol Type
        {
            get { return _variable.Type; }
        }

        internal override bool IsPinned
        {
            get { return false; }
        }

        internal override bool IsCompilerGenerated
        {
            get { return false; }
        }

        public override RefKind RefKind
        {
            get { return RefKind.None; }
        }

        public override ImmutableArray<Location> Locations
        {
            get { return NoLocations; }
        }

        public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
        {
            get { return ImmutableArray<SyntaxReference>.Empty; }
        }
    }
}
