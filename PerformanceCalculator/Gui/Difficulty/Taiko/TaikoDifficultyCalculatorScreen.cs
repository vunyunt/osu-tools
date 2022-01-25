using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Screens;
using osu.Framework.Bindables;
using osuTK.Graphics;


namespace PerformanceCalculator.Gui.Difficulty.Taiko
{
    public class TaikoDifficultyCalculatorScreen : Screen
    {
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
                new SpriteText {
                    Text = "Parameter 1",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Font = new FontUsage(size: 30),
                    Colour = Color4.White,
                },
                new BasicSliderBar<double> {
                    Position = new osuTK.Vector2(0, 100),
                    Current = new BindableDouble(0.5) { MinValue = 0, MaxValue = 1 },
                    Height = 50,
                    Width = 300,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                }
            };
        }
    }
}