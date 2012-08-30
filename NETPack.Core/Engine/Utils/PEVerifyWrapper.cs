using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NETPack.Core.Engine.Utils.Extensions;

namespace NETPack.Core.Engine.Utils
{
    public class PEVerifyWrapper
    {
        public struct VerificationError
        {
            public string ErrorType { get; set; }
            public string MDToken { get; set; }
            public string @File { get; set; }
            public string Method { get; set; }
            public string Offset { get; set; }
            public string Message { get; set; }
        }

        public bool IsSuccess { get; private set; }
        public List<VerificationError> Errors { get; private set; }

        private Process _peverifyProc;

        public PEVerifyWrapper()
        {
            if (!File.Exists(Path.Combine(Globals.Context.LocalPath, "1033\\PEverify.exe")))
                throw new FileNotFoundException("PEVerify missing");

            Errors = new List<VerificationError>();

            _peverifyProc = new Process
                        {
                            StartInfo =
                                {
                                    FileName = Path.Combine(Globals.Context.LocalPath, "1033\\PEverify.exe"),
                                    Arguments = Globals.Context.OutPath.ParameterFriendly() + " /md /il /verbose",
                                    RedirectStandardOutput = true,
                                    UseShellExecute = false,
                                    CreateNoWindow = true
                                }
                        };
        }

        public void VerifyPE(string filename)
        {
            _peverifyProc.Start();
            _peverifyProc.WaitForExit();

            ParseOutput(_peverifyProc.StandardOutput.ReadToEnd());
        }

        private void ParseOutput(string output)
        {
            var rgxSuccess = new Regex("All Classes and Methods in (.*) Verified");

            if ((IsSuccess = rgxSuccess.IsMatch(output)))
                return;

            Errors = ParseErrors(output).ToList();
            Errors.RemoveAll(x => x.ErrorType == null);
        }

        private IEnumerable<VerificationError> ParseErrors(string output)
        {
            var rgxError = new Regex(@"\[(?<type>.+)\]: Error: \[(?<file>.+) : (?<location>.+?)\](\[.+\])?\[offset (?<offset>0x[a-fA-F\d]+)\](?<message>.+)");
            var rgxMDError = new Regex(@"\[(?<type>IL|MD)\]: (?<message>.+)");

            foreach (var line in output.Split('\n'))
            {
                if (rgxError.IsMatch(line))
                {
                    var match = rgxError.Match(line);

                    yield return new VerificationError
                                     {
                                         ErrorType = match.Groups[2].Value,
                                         MDToken = match.Groups[1].Value,
                                         File = match.Groups[3].Value,
                                         Method = match.Groups[4].Value,
                                         Offset = match.Groups[5].Value,
                                         Message = match.Groups[6].Value
                                     };
                }
                else if (rgxMDError.IsMatch(line))
                {
                    var match = rgxError.Match(line);

                    yield return new VerificationError
                                     {
                                         ErrorType = match.Groups[2].Value,
                                         MDToken = match.Groups[1].Value,
                                     };
                }
            }
        }
    }
}
