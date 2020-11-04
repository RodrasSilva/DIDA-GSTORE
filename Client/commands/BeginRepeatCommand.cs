using System;
using System.Collections.Generic;
using System.IO;
using DIDA_GSTORE.grpcService;

namespace DIDA_GSTORE.commands{
    public class BeginRepeatCommand : ICommand{
        private const int NumberOfRepeatsPosition = 0;
        private const string EndRepeatCommand = "end-repeat";
        private const string ReplaceSymbol = "$i";
        private readonly int _numberOfRepeats;
        private readonly StreamReader _operationsFileReader;

        private BeginRepeatCommand(int numberOfRepeats, StreamReader operationsFileReader){
            _numberOfRepeats = numberOfRepeats;
            _operationsFileReader = operationsFileReader;
        }

        public void Execute(GrpcService grpcService){
            string line;
            var commands = new List<string>();
            while ((line = _operationsFileReader.ReadLine()) != null && !line.Equals(EndRepeatCommand))
                commands.Add(line);

            for (var i = 0; i < _numberOfRepeats; ++i){
                var counter = i.ToString();
                commands.ForEach(cmd => {
                    cmd = cmd.Replace(ReplaceSymbol, counter);
                    var command = ClientCommands.GetCommand(cmd);
                    command.Execute(grpcService);
                });
            }
        }

        public static BeginRepeatCommand ParseCommandLine(string[] arguments, StreamReader operationsFileReader){
            if (arguments.Length != 1) throw new Exception("Invalid Begin Repeat Command ");

            var numberOfRepeats = int.Parse(arguments[NumberOfRepeatsPosition]);
            return new BeginRepeatCommand(numberOfRepeats, operationsFileReader
            );
        }
    }
}