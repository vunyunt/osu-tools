using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Screens;
using osuTK;
using osuTK.Graphics;

using PerformanceCalculator.Gui.Difficulty.Taiko;

namespace PerformanceCalculator.Gui
{
    public class MainScreen : Screen
    {
        private Menu createMenu()
        {
            BasicMenu basicMenu = new BasicMenu(Direction.Vertical, true)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Items = new[]
                    {
                        new MenuItem("Difficulty Calculator") {
                            Items = new[]
                            {
                                new MenuItem("Standard"),
                                new MenuItem("Catch the Beat"),
                                new MenuItem("Mania"),
                                new MenuItem("Taiko", () => {
                                    this.Push(new TaikoDifficultyCalculatorScreen());
                                }),
                            }
                        },
                        new MenuItem("Performance Calculator")
                    }
            };

            return basicMenu;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both,
                },
                this.createMenu()
            };
        }
    }
}
