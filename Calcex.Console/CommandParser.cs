using System;
using System.Collections.Generic;
using System.Linq;

namespace Calcex.CalcexConsole
{
    public class CommandParser
    {
        Dictionary<string, Command> commandDict;

        public Command[] Commands { get => commandDict.Values.ToArray(); } 

        public CommandParser(params Command[] commands)
        {
            commandDict = new Dictionary<string, Command>();
            foreach(var command in commands)
            {
                commandDict.Add(command.Name, command);
            }
        }

        public void ParseCommand(string input) => ParseCommand(input.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries));

        public void ParseCommand(string[] input, bool isInteractive = true)
        {
            if (commandDict.ContainsKey(input[0])
                && (isInteractive || !commandDict[input[0]].InteractiveOnly))
            {
                var command = commandDict[input[0]];
                var argCount = input.Length-1;
                if (argCount < command.MinArgs || argCount > command.MaxArgs)
                {
                    throw new ArgumentException($"Command '{command}' cannot take {argCount} argument(s).");
                }
                var args = input.Skip(1).ToArray();
                command.Action.Invoke(args);
            }
            else
            {
                throw new ArgumentException($"No command with name '{input[0]}' was found.");
            }
        }
    }
}