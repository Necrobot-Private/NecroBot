using CommandLine;
using CommandLine.Text;
using System;

namespace RocketBot2.Helpers
{

    class Options
    {
        [Option('a', "autostart", Default = false, Required = false, HelpText = "Auto start bot")]
        public bool AutoStart { get; set; }

        [Option('i', "init", Required = false, HelpText = "Init account")]
        public bool Init { get; set; }

        [Option('t', "template", Default = null, Required = false, HelpText = "Prints all messages to standard output.")]
        public string Template { get; set; }

        [Option('p', "password", Default = null, Required = false, HelpText = "pasword")]
        public string Password { get; set; }

        [Option('g', "google", Default = false, Required = false, HelpText = "is google account")]
        public bool IsGoogle { get; set; }

        [Option('s', "start", Default = 1, HelpText = "Start account", Required = false)]
        public int Start { get; set; }

        [Option('e', "end", Default = 10, HelpText = "End account", Required = false)]
        public int End { get; set; }

        /* Outed or depracated
         * [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(HelpText.AutoBuild(this,
               (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current)));
            Console.ReadKey();
            Environment.Exit(0);
            return null;
        }
        */
    }
}
