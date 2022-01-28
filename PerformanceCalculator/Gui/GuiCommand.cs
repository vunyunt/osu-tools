using McMaster.Extensions.CommandLineUtils;
using osu.Framework.Platform;
using osu.Framework;

namespace PerformanceCalculator.Gui
{
    [Command(Name = "gui", Description = "A GUI for the PerformanceCalculator.")]
    public class GuiCommand : ProcessorCommand
    {
        public override void Execute()
        {
            GameHost host = Host.GetSuitableDesktopHost(@"PerformanceCalculator");
            Game game = new PerformanceCalculatorGame();
            host.Run(game);
        }
    }
}