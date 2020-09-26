using System;
using System.IO;

namespace DIDA_GSTORE.commands {
    public class BeginRepeatCommand : ICommand {
        private const int NumberOfRepeatsPosition = 0;

        private readonly int _numberOfRepeats;
        private readonly StreamReader _operationsFileReader;

        private BeginRepeatCommand(int numberOfRepeats, StreamReader operationsFileReader) {
            _numberOfRepeats = numberOfRepeats;
            _operationsFileReader = operationsFileReader;
        }

        public static BeginRepeatCommand ParseCommandLine(string[] arguments, StreamReader operationsFileReader) {
            if (arguments.Length != 1) {
                throw new Exception("Invalid Begin Repeat Command ");
            }

            var numberOfRepeats = int.Parse(arguments[NumberOfRepeatsPosition]);
            return new BeginRepeatCommand(numberOfRepeats, operationsFileReader
            );
        }

        public void Execute() {
            throw new System.NotImplementedException();
        }
    }
}