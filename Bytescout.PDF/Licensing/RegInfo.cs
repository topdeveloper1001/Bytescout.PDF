using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Bytescout.PDF
{
    internal class RegInfo
    {
        internal bool IsRegistered { get; set; } = false;
        internal bool IsWorkstationLicense { get; set; } = false;

        internal const string PRODUCT_NAME = "Bytescout PDF SDK";
#if FULL
		internal const short UNIQUE_PRODUCT_ID = 12345;
#endif
		private string _registrationName;
		private string _registrationKey;

        internal string RegistrationName
		{
			get { return _registrationName; }
			set
			{
				_registrationName = value;

				if (_registrationName != null)
					_registrationName = _registrationName.Trim();

				CheckRegInfo(_registrationName, _registrationKey);
			}
		}

		///<summary>
		/// Registration key.
		///</summary>
        internal string RegistrationKey
		{
			get { return _registrationKey; }
			set
			{
				_registrationKey = value;

				if (_registrationKey != null)
					_registrationKey = _registrationKey.Trim();

				CheckRegInfo(_registrationName, _registrationKey);
			}
		}


        internal RegInfo()
		{
        }

	    internal string GetDemoWarningString()
        {
            return "(" + PRODUCT_NAME + " " + getProductVersion() + "DEMO)";
        }

	    internal string getProductVersion()
        {
			return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

	    private void CheckRegInfo(string name, string key)
        {
#if FULL
			IsRegistered = true;
#else
			IsRegistered = false;
#endif
		}

        internal void AddProcessingDelayForWorkstationLicense(long processingDurationMs)
        {
            if (IsRegistered && IsWorkstationLicense)
            {
                int waitMs = (int) processingDurationMs;
                if (waitMs < 10000)
                    waitMs = 10000;
                else if (waitMs > 120000)
                    waitMs = 120000;

                AutoResetEvent @event = new AutoResetEvent(false);
                System.Diagnostics.Debug.WriteLine($"Added {waitMs}ms delay...");
                @event.WaitOne(waitMs);
            }
        }
    }
}
