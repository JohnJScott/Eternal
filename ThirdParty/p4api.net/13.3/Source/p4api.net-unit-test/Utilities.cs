using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text;

namespace p4api.net.unit.test
{
	class Utilities
	{
		public static void ClobberDirectory( String path )
		{
			DirectoryInfo di = new DirectoryInfo( path );

			ClobberDirectory( di );
		}

		public static void ClobberDirectory(DirectoryInfo di)
		{
			bool worked = false;
			int retries = 0;
			do
			{
				if (!di.Exists)
					return;

				try
				{
					FileInfo[] files = di.GetFiles();

					foreach (FileInfo fi in files)
					{
						if (fi.IsReadOnly)
							fi.IsReadOnly = false;
						fi.Delete();
					}
					DirectoryInfo[] subDirs = di.GetDirectories();
					foreach (DirectoryInfo sdi in subDirs)
					{
						ClobberDirectory(sdi);
					}

					di.Delete();

					worked = true;
				}
				catch (Exception)
				{
					System.Threading.Thread.Sleep(1000);
				}
				retries++;
			}
			while (!worked && retries < 3);
		}

		static String p4d_cmd = "-p 6666 -Id UnitTestServer -r {0}";
		static String restore_cmd = "-r {0} -jr checkpoint.{1}";
		static String upgrade_cmd = "-r {0} -xu";


		public static Process DeployP4TestServer(string path, bool Unicode)
		{
			return DeployP4TestServer(path, 1, Unicode);
		}

		public static Process DeployP4TestServer(string path, int checkpointRev, bool Unicode)
		{
			String zippedFile = "a.exe";
			if (Unicode)
			{
				zippedFile = "u.exe";
			}

			return DeployP4TestServer(path, checkpointRev, zippedFile);
		}

		public static Process DeployP4TestServer(string path, int checkpointRev, string zippedFile)
		{
			return DeployP4TestServer(path, checkpointRev, zippedFile, null);
		}

		public static Process DeployP4TestServer(string path, int checkpointRev, string zippedFile, string P4DCmd  )
		{
			if (Directory.Exists(path))
				Utilities.ClobberDirectory(path);

			Directory.CreateDirectory(path);

			string assemblyFile = typeof(Utilities).Assembly.CodeBase;
			String unitTestDir = Path.GetDirectoryName(assemblyFile);

			String EnvPath = Environment.GetEnvironmentVariable("path");
			String CurWDir = Environment.CurrentDirectory;

			Environment.CurrentDirectory = path;

			using (StreamWriter sw = new StreamWriter("CmdLog.txt", false))
			{
				int idx;
				if (unitTestDir.ToLower().StartsWith("file:\\"))
				{
					// cut off the file:\\
					idx = unitTestDir.IndexOf("\\") + 1;
					unitTestDir = unitTestDir.Substring(idx);
				}
				if ((idx = unitTestDir.IndexOf("TestResults")) > 0)
				{
					unitTestDir = unitTestDir.Substring(0, idx);
					if (unitTestDir.ToLower().Contains("bin\\debug") == false)
					{
						unitTestDir = Path.Combine(unitTestDir, "bin\\debug");
					}
				}

				string unitTestZip = Path.Combine(unitTestDir, zippedFile);
				string targetTestZip = Path.Combine(path, zippedFile);
				File.Copy(unitTestZip, targetTestZip);

				FileInfo fi = new FileInfo(targetTestZip);

				Process Unzipper = new Process();

				// unpack the zip
				ProcessStartInfo si = new ProcessStartInfo(zippedFile);
				si.WorkingDirectory = path;

				String msg = String.Format("{0} {1}", si.FileName, si.Arguments);
				sw.WriteLine(msg);

				Unzipper.StartInfo = si;
				Unzipper.Start();
				Unzipper.WaitForExit();

				//String checkPointFile = "a_checkpoint.1";
				//if (Unicode)
				//{
				//    checkPointFile = "u_checkpoint.1";
				//}
				//string unitTestCheckPointFile = Path.Combine(unitTestDir, checkPointFile);
				//string targetCheckPointFile = Path.Combine(path, "checkpoint.1");
				//File.Copy(unitTestCheckPointFile, targetCheckPointFile);

				//fi = new FileInfo(targetCheckPointFile);

				Process RestoreCheckPoint = new Process();

				// restore the checkpoint
				si = new ProcessStartInfo("p4d");
				si.Arguments = String.Format(restore_cmd, path, checkpointRev);
				si.WorkingDirectory = path;
				si.UseShellExecute = false;

				msg = String.Format("{0} {1}", si.FileName, si.Arguments);
				sw.WriteLine(msg);

				RestoreCheckPoint.StartInfo = si;
				RestoreCheckPoint.Start();
				RestoreCheckPoint.WaitForExit();

				Process UpgradeTables = new Process();

				// upgrade the db tables
				si = new ProcessStartInfo("p4d");
				si.WorkingDirectory = path;
				si.UseShellExecute = false;

				msg = String.Format("{0} {1}", si.FileName, si.Arguments);
				sw.WriteLine(msg);

				UpgradeTables.StartInfo = si;
				UpgradeTables.Start();
				UpgradeTables.WaitForExit();

				Process p4d = new Process();

				if (P4DCmd != null)
				{
					string P4DCmdSrc = Path.Combine(unitTestDir, P4DCmd);
					string P4DCmdTarget = Path.Combine(path, P4DCmd);
					File.Copy(P4DCmdSrc, P4DCmdTarget);

					// run the command to start p4d
					si = new ProcessStartInfo(P4DCmdTarget);
					si.Arguments = String.Format(path);
					si.WorkingDirectory = path;
					si.UseShellExecute = false;

					msg = String.Format("{0} {1}", si.FileName, si.Arguments);
					sw.WriteLine(msg);

					p4d.StartInfo = si;
					p4d.Start();
				}
				else
				{
					//start p4d
					si = new ProcessStartInfo("p4d");
					si.Arguments = String.Format(p4d_cmd, path);
					si.WorkingDirectory = path;
					si.UseShellExecute = false;

					msg = String.Format("{0} {1}", si.FileName, si.Arguments);
					sw.WriteLine(msg);

					p4d.StartInfo = si;
					p4d.Start();
				}
				Environment.CurrentDirectory = CurWDir;

				return p4d;
			}
		}

		public static Process DeployP4TestServerZip(string path, bool Unicode)
		{
			if (Directory.Exists(path))
				Utilities.ClobberDirectory(path);

			Directory.CreateDirectory(path);

			string assemblyFile = typeof(Utilities).Assembly.CodeBase;
			String unitTestDir = Path.GetDirectoryName(assemblyFile);
			//Environment.CurrentDirectory;

			int idx;
			if (unitTestDir.ToLower().StartsWith("file:\\"))
			{
				// cut off the file:\\
				idx = unitTestDir.IndexOf("\\") + 1;
				unitTestDir = unitTestDir.Substring(idx);
			}
			if ((idx = unitTestDir.IndexOf("TestResults")) > 0)
			{
				unitTestDir = Path.Combine(unitTestDir.Substring(0, idx), "bin\\debug");
			}

			String zippedFile = "a.exe";
			if (Unicode)
			{
				zippedFile = "u.exe";
			}
			string unitTestZip = Path.Combine(unitTestDir, zippedFile);
			string targetTestZip = Path.Combine(path, zippedFile);
			File.Copy(unitTestZip, targetTestZip);

			FileInfo fi = new FileInfo(targetTestZip);

			Process Unzipper = new Process();

			// unpack the zip
			ProcessStartInfo si = new ProcessStartInfo(zippedFile);
			si.WorkingDirectory = path;

			//String EnvPath = Environment.GetEnvironmentVariable("path");

			Unzipper.StartInfo = si;
			Unzipper.Start();
			Unzipper.WaitForExit();

			Process p4d = new Process();

			//start p4d
			si = new ProcessStartInfo("p4d");
			si.Arguments = p4d_cmd;
			si.WorkingDirectory = path;
			si.UseShellExecute = false;

			p4d.StartInfo = si;
			p4d.Start();

			return p4d;
		}

		public static void RemoveTestServer( Process p, String path )
		{
			if( p != null )
			{
				if (!p.HasExited)
					p.Kill();
				p.WaitForExit();

				// sleep for a second to let the system clean up
				System.Threading.Thread.Sleep(1000);
			}
			if( Directory.Exists( path ) )
				Utilities.ClobberDirectory( path );
		}
	}
}
