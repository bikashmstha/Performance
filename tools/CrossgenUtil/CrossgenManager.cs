using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace CrossgenUtil
{
    /// <summary>
    /// This class use crossgen executable file to crossgen modules
    /// </summary>
    public class CrossgenManager
    {
        private readonly string CrossgenPath;
        private readonly string SharedFrameworkPath;
        private readonly string AppDir;
        private readonly ICollection<string> Excludes;

        public CrossgenManager(string crossgenPath, string sharedFrameworkPath, string appDir, ICollection<string> excludes)
        {
            CrossgenPath = crossgenPath;
            SharedFrameworkPath = sharedFrameworkPath;
            AppDir = appDir;
            Excludes = excludes;
        }

        public void RunCrossgen(bool symbols)
        {
            foreach (var file in Directory.GetFiles(AppDir, "*.dll"))
            {
                var fileName = Path.GetFileName(file);
                var moduleName = fileName.Substring(0, fileName.Length - 4);

                if (Excludes == null || !Excludes.Contains(moduleName))
                {
                    var niName = Path.Combine(AppDir, moduleName + ".ni.dll");
                    RunCommandEmbedded(CrossgenPath, $"/Platform_Assemblies_Paths \"{SharedFrameworkPath}\" /App_Paths \"{AppDir}\" /out {moduleName}.ni.dll {moduleName}.dll", AppDir);

                    var niDllLocation = Path.Combine(AppDir, $"{moduleName}.ni.dll");
                    if (File.Exists(niDllLocation))
                    {
                        var dllLocation = Path.Combine(AppDir, $"{moduleName}.dll");

                        if (symbols)
                        {
                            RunCommandEmbedded(CrossgenPath, $"/Platform_Assemblies_Paths \"{SharedFrameworkPath}\" /App_Paths \"{AppDir}\" /CreatePDB {AppDir} {niDllLocation}", AppDir);
                        }

                        File.Delete(dllLocation);
                        File.Move(niDllLocation, dllLocation);
                        Console.WriteLine($"Successfully replaced module {moduleName} with its native image.");
                    }
                    else
                    {
                        WriteError($"Failed to generate native image for {moduleName}");
                    }
                }
            }
        }

        private static void RunCommandEmbedded(string cmdLocation, string parameters, string workingDir)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = cmdLocation,
                    Arguments = parameters,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDir
                }
            };
            proc.OutputDataReceived += (object callingProc, DataReceivedEventArgs data) =>
            {
                Console.WriteLine(data.Data);
            };
            proc.ErrorDataReceived += (object callingProc, DataReceivedEventArgs data) =>
            {
                WriteError(data.Data);
            };
            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            proc.WaitForExit();

            if (proc.ExitCode != 0)
            {
                WriteError($"command:\n\"{cmdLocation} {parameters}\" exited with code: {proc.ExitCode}");
            }
        }

        private static void WriteError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.Write(msg);
            Console.ResetColor();
        }
    }
}
