using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Xunit;

namespace Rhino.Mocks.Tests.FieldsProblem
{
	public class FieldProblem_Joe
	{
		[Fact]
		public void MockingConcreteForm()
		{
            Form frm = MockRepository.Partial<Form>();
			Assert.NotNull(frm);
		}
	}
}
