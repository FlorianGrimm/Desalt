// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsEmitter.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Emit
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using Desalt.Core.Emit;
    using Desalt.TypeScript.Ast;

    /// <summary>
    /// Takes an <see cref="ITsAstNode"/> and converts it to text.
    /// </summary>
    public partial class TsEmitter : TsVisitor, IDisposable
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly Emitter<ITsAstNode> _emitter;
        private readonly EmitOptions _options;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsEmitter(Stream outputStream, Encoding encoding = null, EmitOptions options = null)
        {
            _emitter = new Emitter<ITsAstNode>(outputStream, encoding, options);
            _options = options;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public Encoding Encoding => _emitter.Encoding;

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Dispose()
        {
            _emitter.Dispose();
        }

        public override void VisitIdentifier(ITsIdentifier model)
        {
            _emitter.Write(model.Text);
        }

        public override void VisitNullLiteral(ITsNullLiteral node)
        {
            _emitter.Write("null");
        }

        public override void VisitBooleanLiteral(ITsBooleanLiteral node)
        {
            _emitter.Write(node.Value ? "true" : "false");
        }

        public override void VisitNumericLiteral(ITsNumericLiteral node)
        {
            switch (node.Kind)
            {
                case TsNumericLiteralKind.Decimal:
                    _emitter.Write(node.Value.ToString(CultureInfo.InvariantCulture));
                    break;

                case TsNumericLiteralKind.BinaryInteger:
                    _emitter.Write("0b" + Convert.ToString((long)node.Value, 2));
                    break;

                case TsNumericLiteralKind.OctalInteger:
                    _emitter.Write("0o" + Convert.ToString((long)node.Value, 8));
                    break;

                case TsNumericLiteralKind.HexInteger:
                    string hex = Convert.ToString((long)node.Value, 16);
                    if (!_options.LowerCaseHexLetters)
                    {
                        hex = hex.ToUpperInvariant();
                    }

                    _emitter.Write("0x" + hex);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void VisitRegularExpressionLiteral(ITsRegularExpressionLiteral node)
        {
            _emitter.Write("/");
            _emitter.Write(node.Body);
            _emitter.Write("/");
            _emitter.Write(node.Flags);
        }

        public override void VisitArrayLiteral(ITsArrayLiteral node)
        {
            _emitter.Write("[");
            _emitter.WriteCommaList(node.Elements, Visit);
            _emitter.Write("]");
        }

        public override void VisitArrayElement(ITsArrayElement node)
        {
            Visit(node.Element);

            if (node.IsSpreadElement)
            {
                _emitter.Write(" ...");
            }
        }

        public override void VisitTemplateLiteral(ITsTemplateLiteral node)
        {
            _emitter.Write("`");

            foreach (TsTemplatePart part in node.Parts)
            {
                if (part.Template != null)
                {
                    _emitter.Write(part.Template);
                }

                if (part.Expression != null)
                {
                    _emitter.Write("${");
                    Visit(part.Expression);
                    _emitter.Write("}");
                }
            }

            _emitter.Write("`");
        }
    }
}
