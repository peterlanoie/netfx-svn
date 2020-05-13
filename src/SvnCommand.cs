using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml;
using Common.Diagnostics;
using Common.SVN.Revisions;
using System.Linq;
using System.IO;

namespace Common.SVN
{
	internal class SvnCommand
	{
		private EmbeddedProcessExecutor _process;
		private StringBuilder _output = new StringBuilder();
		private bool _bubbleOutput = true; // whether or not to bubble command output to the OutputMessage event;
		private List<String> _errorOutput = new List<string>();

		public event EventHandler<EventArgs<string>> ErrorMessage;
		public event EventHandler<EventArgs<string>> OutputMessage;
		public event EventHandler<EventArgs<string>> DebugMessage;

		public SvnCommand()
		{
			_process = new EmbeddedProcessExecutor();
			_process.StandardErrorMessageReceived += new EventHandler<EventArgs<string>>(_process_StandardErrorMessageReceived);
			_process.StandardOutputMessageReceived += new EventHandler<EventArgs<string>>(_process_StandardOutputMessageReceived);
			_process.DebugMessage += new EventHandler<EventArgs<string>>(_process_DebugMessage);
		}

		void _process_StandardOutputMessageReceived(object sender, EventArgs<string> e)
		{
			_output.Append(e.Data);
			if (OutputMessage != null && _bubbleOutput)
			{
				OutputMessage(this, e);
			}
		}

		void _process_StandardErrorMessageReceived(object sender, EventArgs<string> e)
		{
			if (ErrorMessage != null)
			{
				_errorOutput.Add(e.Data);
				ErrorMessage(this, e);
			}
		}

		void _process_DebugMessage(object sender, EventArgs<string> e)
		{
			if (DebugMessage != null)
			{
				DebugMessage(this, e);
			}
		}

		public string Run(string command, SvnCommandOptions options)
		{
			return DoSvnAction(delegate()
			{
				string result;
				result = Run(command, options, false);
				return result;
			});
		}

		public XmlDocument GetXml(string command, SvnCommandOptions options)
		{
			return DoSvnAction(delegate()
			{
				XmlDocument objXml = new XmlDocument();
				string output = null;
				try
				{
					_bubbleOutput = false;
					_errorOutput.Clear();
					output = Run(command, options, true);
					objXml.LoadXml(output);
				}
				catch (Exception ex)
				{
					_process_StandardErrorMessageReceived(this, new EventArgs<string>(output ?? ""));
					throw new Exception(string.Format("Error occurred while attempting to load SVN result XML. {0}", string.Join(" ", _errorOutput.ToArray())), ex);
				}
				finally
				{
					_bubbleOutput = true;
				}
				return objXml;
			});
		}

		public XmlReader GetXmlReader(string command, SvnCommandOptions options)
		{
			return DoSvnAction(delegate()
			{
				XmlReader xReader;

				string output = null;
				try
				{
					_bubbleOutput = false;
					_errorOutput.Clear();
					output = Run(command, options, true);

					xReader = new XmlTextReader(new StringReader(output));

					xReader = XmlReader.Create(new StringReader(output), new XmlReaderSettings() { CloseInput = true });
				}
				catch (Exception ex)
				{
					_process_StandardErrorMessageReceived(this, new EventArgs<string>(output ?? ""));
					throw new Exception(string.Format("Error occurred while attempting to load SVN result XML. {0}", string.Join(" ", _errorOutput.ToArray())), ex);
				}
				finally
				{
					_bubbleOutput = true;
				}
				return xReader;
			});
		}

		private T DoSvnAction<T>(Func<T> action)
		{
			T result;
			result = action();
			if (_errorOutput.Count > 0)
			{
				throw new SvnException(string.Format("An error occurred while running an SVN command. {0}", string.Join(" ", _errorOutput.ToArray())));
			}
			return result;
		}

		private string Run(string command, SvnCommandOptions options, bool asXml)
		{
			EmbeddedProcessStartInfo startInfo = new EmbeddedProcessStartInfo();
			StringBuilder sbArguments = new StringBuilder();
			bool blnIncludeAuth = false;
			string strArg;

			_output = new StringBuilder();
			startInfo.FileName = "svn.exe";
			startInfo.WorkingDirectory = options.WorkingDirectory;

			blnIncludeAuth = !IfCommandIs(command, "status", "add", "delete", "move", "revert");

			sbArguments.Append(command);

			if (IfCommandIs(command, "log"))
			{
				sbArguments.Append(" --verbose");
			}

			if (asXml)
			{
				sbArguments.Append(" --xml");
			}
			if (IfCommandIs(command, "update", "checkout", "info", "export", "log"))
			{
				sbArguments.AppendFormat(" --revision {0}", options.Revision.ToString());
			}

			if (!options.IgnoreStandardExcludes)
			{
				sbArguments.Append(" --no-ignore");
			}
			sbArguments.Append(" --non-interactive");
			if (blnIncludeAuth)
			{
				if (!options.CacheAuthCredentials)
				{
					sbArguments.Append(" --no-auth-cache");
				}
				if (!string.IsNullOrEmpty(options.Username))
				{
					sbArguments.AppendFormat(" --username \"{0}\"", options.Username);
				}
				if (!string.IsNullOrEmpty(options.Password))
				{
					sbArguments.AppendFormat(" --password \"{0}\"", options.Password);
				}
			}
			if (!string.IsNullOrEmpty(options.Comment))
			{
				sbArguments.AppendFormat(" --message \"{0}\"", options.Comment);
			}

			if (options.Depth != DepthType.NotSet)
			{
				switch (options.Depth)
				{
					case DepthType.Empty: strArg = "empty"; break;
					case DepthType.Files: strArg = "files"; break;
					case DepthType.Infinity: strArg = "infinity"; break;
					case DepthType.Immediates:
					default: strArg = "immediates"; break;
				}
				sbArguments.AppendFormat(" --depth={0}", strArg);
			}

			foreach (string arg in options.MiscArgs)
			{
				if (!string.IsNullOrEmpty(arg))
				{
					sbArguments.AppendFormat(" {0}", arg);
				}
			}

			foreach (SvnPath path in options.Paths)
			{
				if (!string.IsNullOrEmpty(path.Value))
				{
					if (path.Value.Contains(' '))
					{
						strArg = " \"{0}\"";
					}
					else
					{
						strArg = " {0}";
					}
					sbArguments.AppendFormat(strArg, path.AsArgument(command, options.Revision).Trim().Trim('"'));
				}
			}
			startInfo.Arguments = sbArguments.ToString();

			try
			{
				_process.Run(startInfo);
			}
			catch (Win32Exception win32Ex)
			{
				throw new Exception("Exception caught executing process: most probably 'svn.exe' was not found", win32Ex);
			}
			catch (Exception ex)
			{
				throw new Exception("An exception occurred running an SVN command.", ex);
			}

			if (_errorOutput.Any(e => e.Contains("not a working copy")))
			{
				throw new NotWorkingCopyException();
			}
			if (_process.ExitCode == 9009 && _output.ToString().Contains("not a working copy"))
			{
				throw new NotWorkingCopyException();
			}
			else if (_process.ExitCode != 0)
			{
				throw new Exception("Failed while running 'svn.exe'. Error code " + _process.ExitCode);
			}

			return _output.ToString();
		}

		private bool IfCommandIs(string command, params string[] choices)
		{
			List<string> lstChoices = new List<string>(choices);
			lstChoices.ForEach(c => c = c.ToLower());
			return lstChoices.Contains(command.ToLower());
		}

		//private string SetPathRev(string command, string path, Revision revision)
		//{
		//    Uri uri = new Uri(path);
		//    if(uri.Scheme.ToLower() == "svn")
		//    {

		//    }

		//    if((new string[] { "info", "export" }).Contains(command))
		//    {
		//        return string.Format("{0}@{1}", path, revision.ToString());
		//    }
		//    else
		//    {
		//        return path;
		//    }
		//}

	}
}
