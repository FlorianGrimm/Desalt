// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstNodeListTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Tests.Ast
{
    public class TsAstNodeListTests
    {
        //// ===========================================================================================================
        //// Create Tests
        //// ===========================================================================================================

        //[Test]
        //public void Create_with_no_arguments_should_use_the_Empty_instance()
        //{
        //    var list = TsAstNodeListExtensions.Create<ITsArrayElement>();
        //    list.Should().BeSameAs(TsAstNodeList<ITsArrayElement>.Empty);
        //}

        //[Test]
        //public void Create_with_a_different_separator_should_NOT_use_the_Empty_instance()
        //{
        //    var list = TsAstNodeListExtensions.Create<ITsArrayElement>(";");
        //    list.Should().NotBeSameAs(TsAstNodeList<ITsArrayElement>.Empty);
        //}

        //[Test]
        //public void Create_should_accept_a_single_item()
        //{
        //    var list = TsAstNodeListExtensions.Create(Factory.True);
        //    list.Should().HaveCount(1).And.ContainInOrder(Factory.True);
        //}

        //[Test]
        //public void Create_should_accept_a_list_of_tokens()
        //{
        //    var list = TsAstNodeListExtensions.Create(Factory.Number(1), Factory.Number(2));
        //    list.Should().HaveCount(2).And.ContainInOrder(Factory.Number(1), Factory.Number(2));
        //}

        //[Test]
        //public void CreateRange_should_accept_a_list_of_tokens()
        //{
        //    var list = TsAstNodeListExtensions.CreateRange(new[] { Factory.True, Factory.False });
        //    list.Should().HaveCount(2).And.ContainInOrder(Factory.True, Factory.False);
        //}

        //[Test]
        //public void ToNodeList_should_convert_the_enumerable_to_a_TsAstNodeList()
        //{
        //    var items = new[] { Factory.True, Factory.False };
        //    var list = items.ToNodeList();
        //    list.Should().HaveCount(2).And.ContainInOrder(Factory.True, Factory.False);
        //}

        //[Test]
        //public void ToNodeList_should_return_the_instance_if_it_is_already_a_node_list()
        //{
        //    var list = TsAstNodeListExtensions.CreateRange(new[] { Factory.True, Factory.False });
        //    list.ToNodeList().Should().BeSameAs(list);
        //}

        //// ===========================================================================================================
        //// Indexer Tests
        //// ===========================================================================================================

        //[Test]
        //public void Indexer_should_return_the_node_at_each_index()
        //{
        //    var list = TsAstNodeListExtensions.Create(Factory.False, Factory.True, Factory.False);
        //    list[0].Should().Be(Factory.False);
        //    list[1].Should().Be(Factory.True);
        //    list[2].Should().Be(Factory.False);
        //}

        //[Test]
        //public void Indexer_should_throw_if_the_index_is_out_of_bounds()
        //{
        //    var list = TsAstNodeListExtensions.Create(Factory.True, Factory.False);
        //    Action action = () => list[-1].Should().Be('x');
        //    action.Should().ThrowExactly<IndexOutOfRangeException>();

        //    action = () => list[2].Should().Be('x');
        //    action.Should().ThrowExactly<IndexOutOfRangeException>();
        //}

        //// ===========================================================================================================
        //// Enumerator and Count Tests
        //// ===========================================================================================================

        //[Test]
        //public void Enumerators_should_only_enumerate_the_nodes_and_not_tokens()
        //{
        //    var list = TsAstNodeListExtensions.Create(Factory.True, Factory.False);
        //    list.Select(x => x.Value).Should().HaveCount(2).And.ContainInOrder(true, false);

        //    var enumerator = ((IEnumerable)list).GetEnumerator();
        //    var copy = new List<ITsBooleanLiteral>();
        //    while (enumerator.MoveNext())
        //    {
        //        copy.Add((ITsBooleanLiteral)enumerator.Current!);
        //    }

        //    copy.Should().HaveCount(2).And.ContainInOrder(Factory.True, Factory.False);
        //}

        //[Test]
        //public void Count_should_only_count_the_nodes_and_not_the_tokens()
        //{
        //    for (int i = 1; i <= 5; i++)
        //    {
        //        var list = TsAstNodeListExtensions.CreateRange(Enumerable.Range(1, i).Select(_ => Factory.True));
        //        list.Count.Should().Be(i);
        //    }
        //}

        //// ===========================================================================================================
        //// Separators Tests
        //// ===========================================================================================================

        //[Test]
        //public void Separators_should_only_return_the_separators()
        //{
        //    var list = TsAstNodeListExtensions.Create(Factory.True, Factory.False, Factory.True);
        //    list.Separators.Should().HaveCount(2).And.ContainInOrder(Factory.CommaSpaceToken, Factory.CommaSpaceToken);
        //}

        //// ===========================================================================================================
        //// GetSeparator Tests
        //// ===========================================================================================================

        //[Test]
        //public void GetSeparator_should_get_each_separator_in_the_list()
        //{
        //    var list = TsAstNodeListExtensions.Create(Factory.True, Factory.False, Factory.True);
        //    list.GetSeparator(0).Should().Be(Factory.CommaSpaceToken);
        //    list.GetSeparator(1).Should().Be(Factory.CommaSpaceToken);
        //}

        //[Test]
        //public void GetSeparator_should_throw_if_the_index_is_out_of_bounds()
        //{
        //    var list = TsAstNodeListExtensions.Create(Factory.True, Factory.False, Factory.True);
        //    Action action = () => list.GetSeparator(-1).Should().Be('x');
        //    action.Should().ThrowExactly<ArgumentOutOfRangeException>().And.ParamName.Should().Be("index");

        //    action = () => list.GetSeparator(2).Should().Be('x');
        //    action.Should().ThrowExactly<ArgumentOutOfRangeException>().And.ParamName.Should().Be("index");
        //}

        //// ===========================================================================================================
        //// WithSeparators Tests
        //// ===========================================================================================================

        //[Test]
        //public void WithSeparators_should_return_a_new_list()
        //{
        //    var list = TsAstNodeListExtensions.Create(Factory.True, Factory.False, Factory.True);
        //    list.WithSeparators(new[] { Factory.SemicolonToken, Factory.Token("xx") }).Should().NotBeSameAs(list);
        //}

        //[Test]
        //public void WithSeparators_should_replace_the_separators()
        //{
        //    var list = TsAstNodeListExtensions.Create(Factory.True, Factory.False, Factory.True);
        //    var newList = list.WithSeparators(new[] { Factory.SemicolonToken, Factory.Token("xx") });
        //    newList.Separators.Should().HaveCount(2).And.ContainInOrder(Factory.SemicolonToken, Factory.Token("xx"));
        //}

        //[Test]
        //public void WithSeparators_should_throw_if_the_counts_do_not_match()
        //{
        //    var list = TsAstNodeListExtensions.Create(Factory.True, Factory.False, Factory.True);
        //    Action action = () => list.WithSeparators(new[] { Factory.SemicolonToken }).Should().HaveCount(-1);
        //    action.Should().ThrowExactly<ArgumentException>().And.ParamName.Should().Be("separators");
        //}

        //// ===========================================================================================================
        //// Emit Tests
        //// ===========================================================================================================

        //[Test]
        //public void Emit_should_emit_an_empty_list()
        //{
        //    var list = TsAstNodeList<ITsExpression>.Empty;
        //    list.EmitAsString().Should().Be("()");
        //}

        //[Test]
        //public void Emit_should_print_a_single_node()
        //{
        //    var list = TsAstNodeListExtensions.Create(Factory.Null);
        //    list.EmitAsString().Should().Be("(null)");
        //}

        //[Test]
        //public void Emit_should_print_all_nodes_separated_with_the_separator()
        //{
        //    var list = TsAstNodeListExtensions.Create(Factory.AnyType, Factory.StringType, Factory.BooleanType);
        //    list.EmitAsString().Should().Be("(any, string, boolean)");
        //}

        //// ===========================================================================================================
        //// Equality Tests
        //// ===========================================================================================================

        //        [Test]
        //        public void Same_instances_should_equal()
        //        {
        //            var list = TsAstNodeListExtensions.Create(Factory.Null);
        //            list.Equals(list).Should().BeTrue();
        //            ((ITsAstNodeList<ITsNullLiteral>)list).Equals(list).Should().BeTrue();
        //            ((object)list).Equals(list).Should().BeTrue();

        //#pragma warning disable CS1718 // Comparison made to same variable

        //            // ReSharper disable EqualExpressionComparison
        //            (list == list).Should().BeTrue();
        //            (list != list).Should().BeFalse();

        //            ((ITsAstNodeList<ITsNullLiteral>)list == list).Should().BeTrue();
        //            ((ITsAstNodeList<ITsNullLiteral>)list != list).Should().BeFalse();

        //            (list == (ITsAstNodeList<ITsNullLiteral>)list).Should().BeTrue();
        //            (list != (ITsAstNodeList<ITsNullLiteral>)list).Should().BeFalse();

        //            // ReSharper restore EqualExpressionComparison
        //#pragma warning restore CS1718 // Comparison made to same variable
        //        }

        //        [Test]
        //        public void Equality_against_null_should_be_false()
        //        {
        //            var list = TsAstNodeListExtensions.Create(Factory.Null);
        //            list.Equals(null).Should().BeFalse();
        //        }

        //        [Test]
        //        public void Different_instances_with_identical_elements_should_not_equal()
        //        {
        //            var list1 = TsAstNodeListExtensions.Create(Factory.Null);
        //            var list2 = TsAstNodeListExtensions.Create(Factory.Null);

        //            list1.Equals(list2).Should().BeFalse();
        //            list1.GetHashCode().Should().NotBe(list2.GetHashCode());
        //        }

        //// ===========================================================================================================
        //// Add Tests
        //// ===========================================================================================================

        //// ===========================================================================================================
        //// Insert Tests
        //// ===========================================================================================================

        //[Test]
        //public void Insert_with_an_empty_list_of_nodes_should_return_the_same_instance()
        //{
        //    var list = TsAstNodeListExtensions.Create(Factory.Null);
        //    list.Insert(0).Should().BeSameAs(list);
        //}

        //[Test]
        //public void Insert_should_put_the_nodes_into_the_existing_array()
        //{
        //    var list = TsAstNodeListExtensions.Create(Factory.True);
        //    list.Insert(0, Factory.False);
        //    list.Should().HaveCount(2).And.ContainInOrder(Factory.False, Factory.True);
        //}
    }
}
