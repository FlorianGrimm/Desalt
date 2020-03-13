// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentationCommentCrefTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Translation
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using Desalt.Core.Translation;
    using FluentAssertions;
    using NUnit.Framework;
    using Cref = Desalt.Core.Translation.DocumentationCommentCref;

    /// <summary>
    /// I'm putting the following references below to see how C# generates the XML documentation
    /// comment for them.
    ///
    /// Enum:       <see cref="Environment.SpecialFolder"/>
    ///     => T:System.Environment.SpecialFolder
    /// Delegate:   <see cref="Action"/>
    ///     => T:System.Action
    /// Method:     <see cref="Console.WriteLine(char[], int, int)"/>
    ///     => M:System.Console.WriteLine(System.Char[],System.Int32,System.Int32)
    /// Property:   <see cref="Environment.CurrentDirectory"/>
    ///     => P:System.Environment.CurrentDirectory
    /// Indexer:    <see cref="ArrayList.this"/>
    ///     => P:System.Collections.ArrayList.Item(System.Int32)
    /// Event:      <see cref="INotifyPropertyChanged.PropertyChanged"/>
    ///     => E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    /// Field:      <see cref="_thisField"/>
    ///     => F:Desalt.Core.Tests.Translation.DocumentationCommentCrefTests._thisField
    /// </summary>

    public class DocumentationCommentCrefTests
    {
#pragma warning disable 169, 414
        private readonly string? _thisField = null;
#pragma warning restore 169, 414

        [Test]
        public void Parse_should_throw_on_an_unknown_cref_type()
        {
            Action action = () => Cref.Parse("!:Unknown");
            action.Should()
                .ThrowExactly<InvalidOperationException>()
                .And.Message.Should()
                .Be("Invalid cref attribute: !:Unknown");
        }

        [Test]
        public void Parse_should_accept_a_full_type_reference()
        {
            var cref = Cref.Parse("T:System.Console");
            cref.FullTypeName.Should().Be("System.Console");
            cref.TypeName.Should().Be("Console");
            cref.MemberName.Should().Be(null);
            cref.Kind.Should().Be(DocumentationCommentCrefKind.Type);
        }

        [Test]
        public void Parse_should_accept_a_short_type_reference()
        {
            var cref = Cref.Parse("T:Console");
            cref.FullTypeName.Should().Be("Console");
            cref.TypeName.Should().Be("Console");
            cref.MemberName.Should().Be(null);
            cref.Kind.Should().Be(DocumentationCommentCrefKind.Type);
        }

        [Test]
        public void Parse_should_accept_a_method_reference()
        {
            var cref = Cref.Parse("M:System.Console.WriteLine(System.Char[],System.Int32,System.Int32)");
            cref.FullTypeName.Should().Be("System.Console");
            cref.TypeName.Should().Be("Console");
            cref.MemberName.Should().Be("WriteLine");
            cref.Kind.Should().Be(DocumentationCommentCrefKind.Method);
        }

        [Test]
        public void Parse_should_accept_a_property_reference()
        {
            var cref = Cref.Parse("P:System.Environment.CurrentDirectory");
            cref.FullTypeName.Should().Be("System.Environment");
            cref.TypeName.Should().Be("Environment");
            cref.MemberName.Should().Be("CurrentDirectory");
            cref.Kind.Should().Be(DocumentationCommentCrefKind.Property);
        }

        [Test]
        public void Parse_should_accept_an_indexer_reference()
        {
            var cref = Cref.Parse("P:System.Collections.ArrayList.Item(System.Int32)");
            cref.FullTypeName.Should().Be("System.Collections.ArrayList");
            cref.TypeName.Should().Be("ArrayList");
            cref.MemberName.Should().Be("Item");
            cref.Kind.Should().Be(DocumentationCommentCrefKind.Property);
        }

        [Test]
        public void Parse_should_accept_an_event_reference()
        {
            var cref = Cref.Parse("E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged");
            cref.FullTypeName.Should().Be("System.ComponentModel.INotifyPropertyChanged");
            cref.TypeName.Should().Be("INotifyPropertyChanged");
            cref.MemberName.Should().Be("PropertyChanged");
            cref.Kind.Should().Be(DocumentationCommentCrefKind.Event);
        }

        [Test]
        public void Parse_should_accept_a_field_reference()
        {
            var cref = Cref.Parse("F:Desalt.Core.Tests.Translation.DocumentationCommentCrefTests._thisField");
            cref.FullTypeName.Should().Be("Desalt.Core.Tests.Translation.DocumentationCommentCrefTests");
            cref.TypeName.Should().Be("DocumentationCommentCrefTests");
            cref.MemberName.Should().Be("_thisField");
            cref.Kind.Should().Be(DocumentationCommentCrefKind.Field);
        }

        [Test]
        public void ToString_should_return_the_short_form_of_the_reference()
        {
            var cref = Cref.Parse("T:System.Console");
            cref.ToString().Should().Be("Console");

            cref = Cref.Parse("M:System.Console.WriteLine(System.Char[],System.Int32,System.Int32)");
            cref.ToString().Should().Be("Console.WriteLine");

            cref = Cref.Parse("P:System.Environment.CurrentDirectory");
            cref.ToString().Should().Be("Environment.CurrentDirectory");

            cref = Cref.Parse("E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged");
            cref.ToString().Should().Be("INotifyPropertyChanged.PropertyChanged");

            cref = Cref.Parse("F:Desalt.Core.Tests.Translation.DocumentationCommentCrefTests._thisField");
            cref.ToString().Should().Be("DocumentationCommentCrefTests._thisField");
        }

        [Test]
        public void ToFullString_should_return_the_long_form_of_the_reference()
        {
            var cref = Cref.Parse("T:System.Console");
            cref.ToFullString().Should().Be("System.Console");

            cref = Cref.Parse("M:System.Console.WriteLine(System.Char[],System.Int32,System.Int32)");
            cref.ToFullString().Should().Be("System.Console.WriteLine");

            cref = Cref.Parse("P:System.Environment.CurrentDirectory");
            cref.ToFullString().Should().Be("System.Environment.CurrentDirectory");

            cref = Cref.Parse("E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged");
            cref.ToFullString().Should().Be("System.ComponentModel.INotifyPropertyChanged.PropertyChanged");

            cref = Cref.Parse("F:Desalt.Core.Tests.Translation.DocumentationCommentCrefTests._thisField");
            cref.ToFullString().Should().Be("Desalt.Core.Tests.Translation.DocumentationCommentCrefTests._thisField");
        }
    }
}
