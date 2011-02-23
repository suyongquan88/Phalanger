/*

 Copyright (c) 2004-2006 Ladislav Prosek.

 The use and distribution terms for this software are contained in the file named License.txt, 
 which can be found in the root of the Phalanger distribution. By using this software 
 in any fashion, you are agreeing to be bound by the terms of this license.
 
 You must not remove this notice from this software.

*/

using System;
using System.Resources;
using System.Reflection;

namespace PHP.Core
{
	/// <summary>
	/// Manages resources of the ShmChannel.
	/// </summary>
	internal sealed class ShmResources
	{
		/// <summary>
		/// Resource manager of strings.
		/// </summary>
		static ResourceManager strings;

		/// <summary>
		/// Private constructor to prevent instantiation.
		/// </summary>
		private ShmResources()
		{ }

		/// <summary>
		/// Initializes the resource manager.
		/// </summary>
		static ShmResources()
		{
			strings = new ResourceManager("PHP.Core.Strings", Assembly.GetExecutingAssembly());
		}

		/// <summary>
		/// Retrieves a string resource.
		/// </summary>
		/// <param name="id">The string identifier.</param>
		/// <returns>The string.</returns>
		public static string GetString(string id)
		{
			return strings.GetString(id);
		}

		/// <summary>
		/// Retrieves formatted string resource.
		/// </summary>
		/// <param name="id">The string identifier.</param>
		/// <param name="args">An <see cref="System.Object"/> array containing zero or more objects to format.</param>
		/// <returns>The formatted string resource.</returns>
		/// <exception cref="ArgumentNullException">The string resource with <paramref name="id"/> doesn't exist.</exception>
		/// <exception cref="FormatException">The string resource is not valid formatting string for specified arguments.</exception>
		public static string GetString(string id, params object[] args)
		{
			return String.Format(strings.GetString(id), args);
		}

		/// <summary>
		/// Retrieves formatted string resource.
		/// </summary>
		/// <param name="id">The string identifier.</param>
		/// <param name="arg">An <see cref="System.Object"/> to format.</param>
		/// <returns>The formatted string resource.</returns>
		/// <exception cref="ArgumentNullException">The string resource with <paramref name="id"/> doesn't exist.</exception>
		/// <exception cref="FormatException">The string resource is not valid formatting string for specified arguments.</exception>
		public static string GetString(string id, object arg)
		{
			return String.Format(strings.GetString(id), arg);
		}

		/// <summary>
		/// Retrieves formatted string resource.
		/// </summary>
		/// <param name="id">The string identifier.</param>
		/// <param name="arg1">An <see cref="System.Object"/> to format.</param>
		/// <param name="arg2">An <see cref="System.Object"/> to format.</param>
		/// <returns>The formatted string resource.</returns>
		/// <exception cref="ArgumentNullException">The string resource with <paramref name="id"/> doesn't exist.</exception>
		/// <exception cref="FormatException">The string resource is not valid formatting string for specified arguments.</exception>
		public static string GetString(string id, object arg1, object arg2)
		{
			return String.Format(strings.GetString(id), arg1, arg2);
		}

		/// <summary>
		/// Retrieves formatted string resource.
		/// </summary>
		/// <param name="id">The string identifier.</param>
		/// <param name="arg1">An <see cref="System.Object"/> to format.</param>
		/// <param name="arg2">An <see cref="System.Object"/> to format.</param>
		/// <param name="arg3">An <see cref="System.Object"/> to format.</param>
		/// <returns>The formatted string resource.</returns>
		/// <exception cref="ArgumentNullException">The string resource with <paramref name="id"/> doesn't exist.</exception>
		/// <exception cref="FormatException">The string resource is not valid formatting string for specified arguments.</exception>
		public static string GetString(string id, object arg1, object arg2, object arg3)
		{
			return String.Format(strings.GetString(id), arg1, arg2, arg3);
		}
	}
}