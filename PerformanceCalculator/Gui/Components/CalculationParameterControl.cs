using System;
using System.Linq;
using System.Reflection;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;

using osu.Game.Rulesets.Difficulty;

namespace PerformanceCalculator.Gui.Components
{
    public class CalculationParameterControl<T> : FillFlowContainer
        where T : struct, IComparable<T>, IConvertible, IEquatable<T>
    {
        private readonly CalculationParameters obj;
        // The field of the CalculationParameters object
        public FieldInfo field { get; }
        // The CalculationParameterAttribute of the field
        private readonly CalculationParameterAttribute attribute;

        // The label of the control
        private readonly SpriteText label;
        // The slider of the control
        private readonly SliderBar<T> slider;
        // Text input
        private readonly BasicTextBox textBox;

        // The bindable value of the field
        public BindableNumber<T> sliderValue { get; }
        private readonly Bindable<string> textboxContent;

        // Constructor
        public CalculationParameterControl(CalculationParameters obj, FieldInfo field, CalculationParameterAttribute attribute)
        {
            this.obj = obj;
            this.field = field;
            this.attribute = attribute;

            // Label
            label = new SpriteText
            {
                Text = attribute.Name,
                Font = FrameworkFont.Regular,
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft
            };

            // Slider
            sliderValue = new BindableNumber<T>
            {
                MinValue = attribute.Min,
                MaxValue = attribute.Max,
                Precision = attribute.Step,
                Default = attribute.DefaultValue,
                Value = (T)field.GetValue(obj)
            };
            slider = new BasicSliderBar<T>
            {
                Current = sliderValue,
                RelativeSizeAxes = Axes.X,
                Width = 1,
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
                Height = 20,
                Margin = new MarginPadding { Top = 5, Bottom = 5 }
            };

            // Handle slider value changes
            sliderValue.ValueChanged += (e) =>
            {
                field.SetValue(obj, e.NewValue);
                this.textboxContent.Value = e.NewValue.ToString();
            };

            this.textboxContent = new Bindable<string>
            {
                Value = sliderValue.Value.ToString()
            };
            textBox = new BasicTextBox
            {
                Current = this.textboxContent,
                RelativeSizeAxes = Axes.X,
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
                Width = 1,
                Height = 30
            };
            // Handle textbox changes
            textboxContent.ValueChanged += (e) =>
            {
                try
                {
                    T castedValue = (T)Convert.ChangeType(e.NewValue, typeof(T));
                    sliderValue.Value = castedValue;
                }
                catch (Exception)
                {
                    return;
                }
            };

            this.RelativeSizeAxes = Axes.X;
            this.AutoSizeAxes = Axes.Y;
            this.Children = new Drawable[] {
                label,
                textBox,
                slider,
            };
        }
    }


}