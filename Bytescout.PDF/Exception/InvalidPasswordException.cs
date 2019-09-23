using System.Runtime.InteropServices;

namespace Bytescout.PDF
{
#if PDFSDK_EMBEDDED_SOURCES
	internal class InvalidPasswordException : PDFException
#else
	/// <summary>
	/// Represents the exception thrown when a wrong password is used to open the PDF document.
	/// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class InvalidPasswordException : PDFException
#endif
	{
		private readonly PasswordManager _manager;

		/// <summary>
		/// Gets the Bytescout.PDF.Exceptions.PasswordManager for verifying the password.
		/// </summary>
		/// <value cref="PDF.PasswordManager"></value>
		public PasswordManager PasswordManager { get { return _manager; } }

		internal InvalidPasswordException(PasswordManager manager) : base("Wrong password.")
		{
			_manager = manager;
		}
	}

#if PDFSDK_EMBEDDED_SOURCES
	internal class PasswordManager
#else
	/// <summary>
	/// Represents the class for verifying the password.
	/// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class PasswordManager
#endif
	{
		private readonly Encryptor _encryptor;

		internal PasswordManager(Encryptor encryptor)
		{
			_encryptor = encryptor;
		}

		/// <summary>
		/// Verifies the password.
		/// </summary>
		/// <param name="password" href="http://msdn.microsoft.com/en-us/library/system.string.aspx">The password to check.</param>
		/// <returns cref="bool" href="http://msdn.microsoft.com/en-us/library/system.boolean.aspx">true if the password is correct; otherwise, false.</returns>
		public bool CheckPassword(string password)
		{
			return _encryptor.AuthenticatePassword(password);
		}
	}
}