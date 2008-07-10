using System;
using Castle.Core.Interceptor;
using Rhino.Mocks.Interfaces;

namespace Rhino.Mocks
{
	/// <summary>
	/// This is a data structure that is used by 
	/// <seealso cref="IMethodOptions{T}.Do(Action{MethodInvocation})"/> to pass
	/// the current method to the relevant delegate
	/// </summary>
	public class MethodInvocation
	{
		private readonly IInvocation invocation;

		/// <summary>
		/// Initializes a new instance of the <see cref="MethodInvocation"/> class.
		/// </summary>
		/// <param name="invocation">The invocation.</param>
		internal MethodInvocation(IInvocation invocation)
		{
			this.invocation = invocation;
		}

		/// <summary>
		/// Gets the args for this method invocation
		/// </summary>
		public object[] Arguments
		{
			get { return invocation.Arguments; }
		}

		/// <summary>
		/// Gets or sets the return value for this method invocation
		/// </summary>
		/// <value>The return value.</value>
		public object ReturnValue
		{
			get { return invocation.ReturnValue; }
			set { invocation.ReturnValue = value; }
		}


	}
}