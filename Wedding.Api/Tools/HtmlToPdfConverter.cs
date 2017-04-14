using System;
using System.Diagnostics;
using System.IO;

namespace Wedding.Api.Tools
{
	public class HtmlToPdfConverter
	{
		private static bool _initialized;
		private static string _programPath;
		private static int _timeout;

		public static void Setup(string programPath, int timeout)
		{
			_programPath = programPath;
			_timeout = timeout;
			_initialized = true;
		}

		public static string Convert(string htmlPath)
		{
			if (!_initialized)
			{
				throw new Exception("Html to pdf converter is not initialized");
			}
			if (Path.GetExtension(htmlPath) != ".html")
			{
				throw new Exception("Extension of file to convert must be .html");
			}
			string pdfPath = Path.Combine(Path.GetDirectoryName(htmlPath), Path.GetFileNameWithoutExtension(htmlPath) + ".pdf");
			try
			{
				Process p = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						RedirectStandardError = true,
						UseShellExecute = false,
						Arguments = $"{htmlPath} {pdfPath}",
						FileName = _programPath
					}
				};
				p.Start();
				p.WaitForExit((int)TimeSpan.FromSeconds(_timeout).TotalMilliseconds);
				if (!p.HasExited)
				{
					throw new Exception("Html to pdf conversion timeout");
				}
				if (p.ExitCode != 0)
				{
					throw new Exception(p.StandardError.ReadToEnd());
				}
			}
			catch (Exception)
			{
				File.Delete(pdfPath);
				throw;
			}
			return pdfPath;
		}
	}
}
