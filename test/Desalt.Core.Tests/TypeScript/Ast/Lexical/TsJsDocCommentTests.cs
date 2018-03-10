// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsJsDocCommentTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.TypeScript.Ast.Lexical
{
    using System.Collections.Immutable;
    using System.Linq;
    using Desalt.Core.TypeScript.Ast.Lexical;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Factory = Desalt.Core.TypeScript.Ast.TsAstFactory;

    [TestClass]
    public class TsJsDocCommentTests
    {
        private static readonly TsJsDocComment s_fullComment = new TsJsDocComment(
            fileTag: Factory.JsDocBlock("File"),
            copyrightTag: Factory.JsDocBlock("Copyright"),
            isPackagePrivate: true,
            paramsTags: new[] { ("p1", Factory.JsDocBlock("Param1")), ("p2", Factory.JsDocBlock("Param2")) },
            returnsTag: Factory.JsDocBlock("Returns"),
            throwsTags: new[] { ("Error", Factory.JsDocBlock("Error1")), ("ErrorEx", Factory.JsDocBlock("Error2")) },
            exampleTags: new[] { Factory.JsDocBlock("Example1"), Factory.JsDocBlock("Example2") },
            description: Factory.JsDocBlock("Description"),
            summaryTag: Factory.JsDocBlock("Summary"),
            seeTags: new[] { Factory.JsDocBlock("See1"), Factory.JsDocBlock("See2") });

        private static readonly TsJsDocComment s_fullCommentPristine = new TsJsDocComment(
            fileTag: Factory.JsDocBlock("File"),
            copyrightTag: Factory.JsDocBlock("Copyright"),
            isPackagePrivate: true,
            paramsTags: new[] { ("p1", Factory.JsDocBlock("Param1")), ("p2", Factory.JsDocBlock("Param2")) },
            returnsTag: Factory.JsDocBlock("Returns"),
            throwsTags: new[] { ("Error", Factory.JsDocBlock("Error1")), ("ErrorEx", Factory.JsDocBlock("Error2")) },
            exampleTags: new[] { Factory.JsDocBlock("Example1"), Factory.JsDocBlock("Example2") },
            description: Factory.JsDocBlock("Description"),
            summaryTag: Factory.JsDocBlock("Summary"),
            seeTags: new[] { Factory.JsDocBlock("See1"), Factory.JsDocBlock("See2") });

        [TestMethod]
        public void With_methods_should_not_create_a_new_instance_if_the_same()
        {
            s_fullComment.WithFileTag(Factory.JsDocBlock("File")).Should().BeSameAs(s_fullComment);
            s_fullComment.WithCopyrightTag(Factory.JsDocBlock("Copyright")).Should().BeSameAs(s_fullComment);
            s_fullComment.WithIsPackagePrivate(true).Should().BeSameAs(s_fullComment);
            s_fullComment.WithReturnsTag(Factory.JsDocBlock("Returns")).Should().BeSameAs(s_fullComment);
            s_fullComment.WithDescription(Factory.JsDocBlock("Description")).Should().BeSameAs(s_fullComment);
            s_fullComment.WithSummaryTag(Factory.JsDocBlock("Summary")).Should().BeSameAs(s_fullComment);

            s_fullComment
                .WithExampleTags(
                    new[] { Factory.JsDocBlock("Example1"), Factory.JsDocBlock("Example2") }.ToImmutableArray())
                .Should()
                .BeSameAs(s_fullComment);

            s_fullComment
                .WithSeeTags(new[] { Factory.JsDocBlock("See1"), Factory.JsDocBlock("See2") }.ToImmutableArray())
                .Should()
                .BeSameAs(s_fullComment);

            s_fullComment
                .WithParamTags(
                    new[] { ("p1", Factory.JsDocBlock("Param1")), ("p2", Factory.JsDocBlock("Param2")) }
                        .ToImmutableArray())
                .Should()
                .BeSameAs(s_fullComment);

            s_fullComment
                .WithThrowsTags(
                    new[] { ("Error", Factory.JsDocBlock("Error1")), ("ErrorEx", Factory.JsDocBlock("Error2")) }
                        .ToImmutableArray())
                .Should()
                .BeSameAs(s_fullComment);
        }

        [TestMethod]
        public void With_methods_should_not_modify_the_original_object()
        {
            s_fullComment.WithFileTag(Factory.JsDocBlock("FileX"));
            s_fullComment.WithCopyrightTag(Factory.JsDocBlock("CopyrightX"));
            s_fullComment.WithIsPackagePrivate(false);
            s_fullComment.WithReturnsTag(Factory.JsDocBlock("ReturnsX"));
            s_fullComment.WithDescription(Factory.JsDocBlock("DescriptionX"));
            s_fullComment.WithSummaryTag(Factory.JsDocBlock("SummaryX"));

            s_fullComment.WithExampleTags(
                new[] { Factory.JsDocBlock("Example1X"), Factory.JsDocBlock("Example2X") }.ToImmutableArray());

            s_fullComment.WithSeeTags(
                new[] { Factory.JsDocBlock("See1X"), Factory.JsDocBlock("See2X") }.ToImmutableArray());

            s_fullComment.WithParamTags(
                new[] { ("p1", Factory.JsDocBlock("Param1X")), ("p2", Factory.JsDocBlock("Param2X")) }
                    .ToImmutableArray());

            s_fullComment.WithThrowsTags(
                new[] { ("Error", Factory.JsDocBlock("Error1X")), ("ErrorEx", Factory.JsDocBlock("Error2X")) }
                    .ToImmutableArray());

            s_fullComment.Should().Be(s_fullCommentPristine);
        }

        [TestMethod]
        public void With_methods_should_return_different_objects()
        {
            s_fullComment.WithFileTag(Factory.JsDocBlock("FileX")).Should().NotBe(s_fullComment);
            s_fullComment.WithCopyrightTag(Factory.JsDocBlock("CopyrightX")).Should().NotBe(s_fullComment);
            s_fullComment.WithIsPackagePrivate(false).Should().NotBe(s_fullComment);
            s_fullComment.WithReturnsTag(Factory.JsDocBlock("ReturnsX")).Should().NotBe(s_fullComment);
            s_fullComment.WithDescription(Factory.JsDocBlock("DescriptionX")).Should().NotBe(s_fullComment);
            s_fullComment.WithSummaryTag(Factory.JsDocBlock("SummaryX")).Should().NotBe(s_fullComment);

            s_fullComment.WithExampleTags(
                new[] { Factory.JsDocBlock("Example1X"), Factory.JsDocBlock("Example2X") }.ToImmutableArray())
                .Should()
                .NotBe(s_fullComment);

            s_fullComment.WithSeeTags(
                new[] { Factory.JsDocBlock("See1X"), Factory.JsDocBlock("See2X") }.ToImmutableArray())
                .Should()
                .NotBe(s_fullComment);

            s_fullComment.WithParamTags(
                new[] { ("p1", Factory.JsDocBlock("Param1X")), ("p2", Factory.JsDocBlock("Param2X")) }
                    .ToImmutableArray())
                .Should()
                .NotBe(s_fullComment);

            s_fullComment.WithThrowsTags(
                new[] { ("Error", Factory.JsDocBlock("Error1X")), ("ErrorEx", Factory.JsDocBlock("Error2X")) }
                    .ToImmutableArray())
                .Should()
                .NotBe(s_fullComment);

            s_fullComment.Should().Be(s_fullCommentPristine);
        }

        [TestMethod]
        public void With_methods_should_change_the_value()
        {
            s_fullComment.WithFileTag(Factory.JsDocBlock("FileX")).FileTag.Content.First().Text.Should().Be("FileX");

            s_fullComment.WithCopyrightTag(Factory.JsDocBlock("CopyrightX"))
                .CopyrightTag.Content.First().Text
                .Should()
                .Be("CopyrightX");

            s_fullComment.WithIsPackagePrivate(false).IsPackagePrivate.Should().BeFalse();

            s_fullComment.WithReturnsTag(Factory.JsDocBlock("ReturnsX"))
                .ReturnsTag.Content.First()
                .Text.Should()
                .Be("ReturnsX");

            s_fullComment.WithDescription(Factory.JsDocBlock("DescriptionX"))
                .Description.Content.First()
                .Text.Should()
                .Be("DescriptionX");

            s_fullComment.WithSummaryTag(Factory.JsDocBlock("SummaryX"))
                .SummaryTag.Content.First()
                .Text.Should()
                .Be("SummaryX");

            s_fullComment
                .WithExampleTags(
                    new[] { Factory.JsDocBlock("Example1X"), Factory.JsDocBlock("Example2X") }.ToImmutableArray())
                .ExampleTags.Select(x => x.Content.First().Text)
                .Should()
                .BeEquivalentTo("Example1X", "Example2X");

            s_fullComment
                .WithSeeTags(new[] { Factory.JsDocBlock("See1X"), Factory.JsDocBlock("See2X") }.ToImmutableArray())
                .SeeTags.Select(x => x.Content.First().Text)
                .Should()
                .BeEquivalentTo("See1X", "See2X");

            s_fullComment
                .WithParamTags(
                    new[] { ("p1", Factory.JsDocBlock("Param1X")), ("p2", Factory.JsDocBlock("Param2X")) }
                        .ToImmutableArray())
                .ParamsTags.Select(x => x.text.Content.First().Text)
                .Should()
                .BeEquivalentTo("Param1X", "Param2X");

            s_fullComment
                .WithThrowsTags(
                    new[] { ("Error", Factory.JsDocBlock("Error1X")), ("ErrorEx", Factory.JsDocBlock("Error2X")) }
                        .ToImmutableArray())
                .ThrowsTags.Select(x => x.text.Content.First().Text)
                .Should()
                .BeEquivalentTo("Error1X", "Error2X");
        }
    }
}
