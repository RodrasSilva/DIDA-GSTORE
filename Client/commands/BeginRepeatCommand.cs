using System;
using System.Collections.Generic;
using System.IO;
using DIDA_GSTORE.grpcService;

namespace DIDA_GSTORE.commands {
    public class BeginRepeatCommand : ICommand {
        private const int NumberOfRepeatsPosition = 0;
        private const string EndRepeatCommand = "end-repeat";
        private const string ReplaceSymbol = "$i";
        private readonly int _numberOfRepeats;
        private List<string> _commands;

        private BeginRepeatCommand(int numberOfRepeats, List<string> commands) {
            _numberOfRepeats = numberOfRepeats;
            _commands = commands;
        }

        public void Execute(GrpcService grpcService) {
            Console.WriteLine("Begin Repeat: \n");
            for (var i = 1; i <= _numberOfRepeats; ++i) {
                var counter = i.ToString();
                _commands.ForEach(cmd => {
                    cmd = cmd.Replace(ReplaceSymbol, counter);
                    var command = ClientCommands.GetCommand(cmd);
                    command.Execute(grpcService);
                });
            }
        }

        public static BeginRepeatCommand ParseCommandLine(string[] arguments, StreamReader operationsFileReader) {
            if (arguments.Length != 1) throw new Exception("Invalid Begin Repeat Command ");

            var numberOfRepeats = int.Parse(arguments[NumberOfRepeatsPosition]);


            string line;
            var commands = new List<string>();

            while ((line = operationsFileReader.ReadLine()) != null && !line.Equals(EndRepeatCommand))
                //Console.WriteLine(line);
                //Console.WriteLine(EndRepeatCommand);
                commands.Add(line);

            return new BeginRepeatCommand(numberOfRepeats, commands);
        }
    }
}