using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.IO.Stores;
using osuTK;

namespace PerformanceCalculator.Gui
{
    public class PerformanceCalculatorGameBase : osu.Framework.Game
    {
        // Anything in this class is shared between the test browser and the game implementation.
        // It allows for caching global dependencies that should be accessible to tests, or changing
        // the screen scaling for all components including the test browser and framework overlays.

        protected override Container<Drawable> Content { get; }

        protected PerformanceCalculatorGameBase()
        {
            base.Content.Add(Content = new Container
            {
                RelativeSizeAxes = Axes.Both,
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
        }
    }
}
