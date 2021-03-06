#region license
// Copyright (c) 2005 - 2007 Ayende Rahien (ayende@ayende.com)
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
// 
//     * Redistributions of source code must retain the above copyright notice,
//     this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice,
//     this list of conditions and the following disclaimer in the documentation
//     and/or other materials provided with the distribution.
//     * Neither the name of Ayende Rahien nor the names of its
//     contributors may be used to endorse or promote products derived from this
//     software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion


using System;
using System.Collections.Generic;
using System.Data;
using Xunit;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using Rhino.Mocks.Exceptions;
using Rhino.Mocks.Helpers;

namespace Rhino.Mocks.Tests
{
	// Interface to create mocks for
	public interface ITestInterface
	{
		event EventHandler<EventArgs> AnEvent;
		void RefOut(string str, out int i, string str2, ref int j, string str3);
		void VoidList(List<string> list);
		void VoidObject (object obj);
	}

	public class ArgConstraintTests
	{
		private IDemo demoMock;
		private ITestInterface testMock;

		private delegate string StringDelegateWithParams(int a, string b);

		public ArgConstraintTests()
		{
			demoMock = MockRepository.Mock<IDemo>();
            testMock = MockRepository.Mock<ITestInterface>();
		}

		[Fact]
		public void ThreeArgs_Pass()
		{
            demoMock.Expect(x =>
                x.VoidThreeArgs(Arg<int>.Is.Anything, Arg.Text.Contains("eine"), Arg<float>.Is.LessThan(2.5f)));

            demoMock.VoidThreeArgs(3, "Steinegger", 2.4f);
            demoMock.VerifyExpectations();
		}

		[Fact]
		public void ThreeArgs_Fail()
		{
            demoMock.Expect(x =>
                x.VoidThreeArgs(Arg<int>.Is.Anything, Arg.Text.Contains("eine"), Arg<float>.Is.LessThan(2.5f)));

            demoMock.VoidThreeArgs(2, "Steinegger", 2.6f);
            Assert.Throws<ExpectationViolationException>(
                () => demoMock.VerifyExpectations());
		}

		[Fact]
		public void Matches()
		{
            demoMock.Expect(x =>
                x.VoidStringArg(Arg<string>.Matches(Is.Equal("hallo") || Text.EndsWith("b"))))
                .Repeat.Times(3);

            demoMock.VoidStringArg("hallo");
            demoMock.VoidStringArg("ab");
            demoMock.VoidStringArg("bb");

            demoMock.VerifyExpectations();
		}

		[Fact]
		public void ConstraintsThatWerentCallCauseVerifyFailure()
		{
            demoMock.Expect(x => x.VoidStringArg(Arg.Text.Contains("World")));

            Assert.Throws<ExpectationViolationException>(
                () => demoMock.VerifyExpectations());
		}

		[Fact]
		public void RefAndOutArgs()
		{
            testMock.Expect(x => x.RefOut(
                Arg<string>.Is.Anything, out Arg<int>.Out(3).Dummy,
                Arg<string>.Is.Equal("Steinegger"), ref Arg<int>.Ref(Is.Equal(2), 7).Dummy,
                Arg<string>.Is.NotNull));

            int iout = 0;
            int iref = 2;

            testMock.RefOut("hallo", out iout, "Steinegger", ref iref, "notnull");

            Assert.Equal(3, iout);
            Assert.Equal(7, iref);

            testMock.VerifyExpectations();
		}

		[Fact]
		public void Event()
		{
            ITestInterface eventMock = MockRepository.Mock<ITestInterface>();

            eventMock.Expect(x => x.AnEvent += Arg<EventHandler<EventArgs>>.Is.Anything);
            eventMock.AnEvent += handler;

            eventMock.VerifyExpectations();
		}

		[Fact]
		public void ListTest()
		{
            ITestInterface testMock = MockRepository.Mock<ITestInterface>();

            testMock.Expect(x => x.VoidList(Arg<List<string>>.List.Count(Is.GreaterThan(3))));
            testMock.Expect(x => x.VoidList(Arg<List<string>>.List.IsIn("hello")));

            testMock.VoidList(new List<string>(new string[] { "1", "2", "4", "5" }));
            testMock.VoidList(new List<string>(new string[] { "1", "3" }));

            Assert.Throws<ExpectationViolationException>(
                () => testMock.VerifyExpectations());
		}
		
		[Fact]
		public void ConstraintWithTooFewArguments_ThrowsException()
		{
            Assert.Throws<InvalidOperationException>(
                () => demoMock.Expect(x => x.VoidThreeArgs(
                        Arg<int>.Is.Equal(4), Arg.Text.Contains("World"), 3.14f)));
		}

        [Fact]
		public void ConstraintToManyArgs_ThrowsException()
		{
            Arg<int>.Is.Equal(4);
            Assert.Throws<InvalidOperationException>(
                () => demoMock.Expect(x => 
                    x.VoidThreeArgs(Arg<int>.Is.Equal(4), Arg.Text.Contains("World"), Arg<float>.Is.Equal(3.14f))));
		}

		[Fact]
		public void MockRepositoryClearsArgData()
		{
			Arg<int>.Is.Equal(4);
			Arg<int>.Is.Equal(4);

			demoMock = MockRepository.Mock<IDemo>();

            demoMock.Expect(x => x.VoidThreeArgs(
                Arg<int>.Is.Equal(4), Arg.Text.Contains("World"), Arg<float>.Is.Equal(3.14f)));
		}
		
		[Fact]
		public void TooFewOutArgs()
		{
			int iout = 2;

            Assert.Throws<InvalidOperationException>(
                () => testMock.Expect(x => x.RefOut(
                        Arg<string>.Is.Anything, out iout,
                        Arg.Text.Contains("Steinegger"), ref Arg<int>.Ref(Is.Equal(2), 7).Dummy,
                        Arg<string>.Is.NotNull)));
		}

		[Fact]
		public void RefInsteadOfOutArg()
		{
            Assert.Throws<InvalidOperationException>(
                () => testMock.Expect(x => x.RefOut(
                        Arg<string>.Is.Anything, out Arg<int>.Ref(Is.Equal(2), 7).Dummy,
                        Arg.Text.Contains("Steinegger"), ref Arg<int>.Ref(Is.Equal(2), 7).Dummy,
                        Arg<string>.Is.NotNull)));
		}

		[Fact]
		public void OutInsteadOfRefArg()
		{
            Assert.Throws<InvalidOperationException>(
                () => testMock.Expect(x => x.RefOut(
                        Arg<string>.Is.Anything, out Arg<int>.Out(7).Dummy,
                        Arg.Text.Contains("Steinegger"), ref Arg<int>.Out(7).Dummy,
                        Arg<string>.Is.NotNull)));
		}

		[Fact]
		public void OutInsteadOfInArg()
		{
            Assert.Throws<InvalidOperationException>(
                () => testMock.Expect(x => x.VoidObject(Arg<object>.Out(null))));
		}
		
		[Fact]
		public void Is_EqualsThrowsException()
		{
			Assert.Throws<InvalidOperationException>(
				() => Arg<object>.Is.Equals(null));
		}

		[Fact]
		public void List_EqualsThrowsException()
		{
			Assert.Throws<InvalidOperationException>(
				() => Arg<object>.List.Equals(null));
		}

		[Fact]
		public void Text_EqualsThrowsException()
		{
			Assert.Throws<InvalidOperationException>(
				() => Arg.Text.Equals(null));
		}
		
		[Fact]
		public void MockStringDelegateWithParams()
		{
            StringDelegateWithParams d = MockRepository.Mock<StringDelegateWithParams>(null);

            d.Expect(x => x(Arg<int>.Is.Equal(1), Arg<string>.Is.Equal("111")))
                .Return("abc");

            d.Expect(x => x(Arg<int>.Is.Equal(2), Arg<string>.Is.Equal("222")))
                .Return("def");

			Assert.Equal("abc", d(1, "111"));
			Assert.Equal("def", d(2, "222"));

            d(3, "333");

            Assert.Throws<ExpectationViolationException>(() => d.VerifyExpectations(true));
		}
		
		[Fact]
        public void Mock_object_using_ExpectMethod_with_ArgConstraints_allow_for_multiple_calls_as_default_behavior()
        {
            // Arrange
            var mock = MockRepository.Mock<IDemo>();
            mock.Expect(x => x.StringArgString(Arg<string>.Is.Equal("input")))
                .Return("output");

            // Act
            var firstCallResult = mock.StringArgString("input");
            var secondCallResult = mock.StringArgString("input");

            // Assert
            Assert.Equal("output", firstCallResult);
            Assert.Equal(firstCallResult, secondCallResult);
        }

        [Fact]
        public void Stub_object_using_ExpectMethod_with_ArgConstraints_allow_for_multiple_calls_as_default_behavior()
        {
            // Arrange
            var mock = MockRepository.Mock<IDemo>();
            mock.Expect(x => x.StringArgString(Arg<string>.Is.Equal("input")))
                .Return("output");

            // Act
            var firstCallResult = mock.StringArgString("input");
            var secondCallResult = mock.StringArgString("input");

            // Assert
            Assert.Equal("output", firstCallResult);
            Assert.Equal(firstCallResult, secondCallResult);
        }

        [Fact]
        public void Stub_object_using_StubMethod_with_ArgConstraints_allow_for_multiple_calls_as_default_behavior()
        {
            // Arrange
            var stub = MockRepository.Mock<IDemo>();
            stub.Stub(x => x.StringArgString(Arg<string>.Is.Equal("input")))
                .Return("output");

            // Act
            var firstCallResult = stub.StringArgString("input");
            var secondCallResult = stub.StringArgString("input");

            // Assert
            Assert.Equal("output", firstCallResult);
            Assert.Equal(firstCallResult, secondCallResult);
        }

        [Fact]
        public void Mock_object_using_StubMethod_with_ArgConstraints_allow_for_multiple_calls_as_default_behavior()
        {
            // Arrange
            var mock = MockRepository.Mock<IDemo>();
            mock.Stub(x => x.StringArgString(Arg<string>.Is.Equal("input")))
                .Return("output");

            // Act
            var firstCallResult = mock.StringArgString("input");
            var secondCallResult = mock.StringArgString("input");

            // Assert
            Assert.Equal("output", firstCallResult);
            Assert.Equal(firstCallResult, secondCallResult);
        }

        [Fact]
        public void ImplicitlyConverted_parameter_is_properly_compared_when_using_IsEqual()
        {
            // Arrange
            var stub = MockRepository.Mock<ITestService>();
            stub.Stub(x => x.GetUser(Arg<long>.Is.Equal(1))).Return("test"); // 1 is inferred as Int32 (not Int64)

            // Assert
            Assert.Equal(null, stub.GetUser(0));
            Assert.Equal("test", stub.GetUser(1));
        }

        [Fact]
        public void ImplicitlyConverted_parameter_is_properly_compared_when_using_IsNotEqual()
        {
            // Arrange
            var stub = MockRepository.Mock<ITestService>();
            stub.Stub(x => x.GetUser(Arg<long>.Is.NotEqual(1))).Return("test"); // 1 is inferred as Int32 (not Int64)

            var actual = stub.GetUser(0);

            // Assert
            Assert.Equal("test", actual);
            Assert.Equal(null, stub.GetUser(1));
        }

        [Fact]
        public void ImplicitlyConverted_parameter_is_properly_compared_when_using_IsGreaterThan()
        {
            // Arrange
            var stub = MockRepository.Mock<ITestService>();
            stub.Stub(x => x.GetUser(Arg<long>.Is.GreaterThan(1))).Return("test"); // 1 is inferred as Int32 (not Int64)

            // Assert
            Assert.Equal(null, stub.GetUser(0));
            Assert.Equal(null, stub.GetUser(1));
            Assert.Equal("test", stub.GetUser(2));
        }

        [Fact]
        public void ImplicitlyConverted_parameter_is_properly_compared_when_using_IsGreaterThanOrEqual()
        {
            // Arrange
            var stub = MockRepository.Mock<ITestService>();
            stub.Stub(x => x.GetUser(Arg<long>.Is.GreaterThanOrEqual(2))).Return("test"); // 1 is inferred as Int32 (not Int64)

            // Assert
            Assert.Equal(null, stub.GetUser(1));
            Assert.Equal("test", stub.GetUser(2));
            Assert.Equal("test", stub.GetUser(3));
        }

        [Fact]
        public void ImplicitlyConverted_parameter_is_properly_compared_when_using_IsLessThan()
        {
            // Arrange
            var stub = MockRepository.Mock<ITestService>();
            stub.Stub(x => x.GetUser(Arg<long>.Is.LessThan(2))).Return("test"); // 1 is inferred as Int32 (not Int64)

            // Assert
            Assert.Equal("test", stub.GetUser(1));
            Assert.Equal(null, stub.GetUser(2));
            Assert.Equal(null, stub.GetUser(3));
        }

        [Fact]
        public void ImplicitlyConverted_parameter_is_properly_compared_when_using_IsLessThanOrEqual()
        {
            // Arrange
            var stub = MockRepository.Mock<ITestService>();
            stub.Stub(x => x.GetUser(Arg<long>.Is.LessThanOrEqual(2))).Return("test"); // 1 is inferred as Int32 (not Int64)

            // Assert
            Assert.Equal("test", stub.GetUser(1));
            Assert.Equal("test", stub.GetUser(2));
            Assert.Equal(null, stub.GetUser(3));
        }

        public interface ITestService
        {
            string GetUser(long id);
        }

        private void handler(object o, EventArgs e)
        {

        }
	}
}
