namespace Zebble
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public partial class ItemPicker : Picker, FormField.IPlaceHolderControl, FormField.IControl
    {
        public readonly AsyncEvent SelectionChanged = new AsyncEvent(ConcurrentEventRaisePolicy.Queue);
        public readonly OptionsDataSource Source = new OptionsDataSource();
        public readonly AsyncEvent<Dialog> DialogOpenning = new AsyncEvent<Dialog>();

        public bool ButtonsAtTop { get; set; } = false;

        public bool? Searchable { get; set; }

        public int SearchCharacterCount { get; set; } = 3;

        object FormField.IControl.Value
        {
            get => Source.Value;
            set => SelectedValue = value;
        }

        /// <summary>
        /// It will set both the source value, and the selected text.
        /// </summary>
        public object SelectedValue
        {
            get => Source.SelectedValue;
            set
            {
                Source.Value = value;
                SelectedText = Source.SelectedValues.OrEmpty().ToString(", ");
            }
        }

        public IEnumerable<OptionsDataSource.DataItem> SelectedItem => Source.SelectedItems;

        public IEnumerable<object> DataSource
        {
            get => Source.DataSource;
            set { Source.DataSource = value; SelectedValue = SelectedValue; }
        }

        public bool MultiSelect { get => Source.MultiSelect; set => Source.MultiSelect = value; }

        protected override Zebble.Dialog CreateDialog()
        {
            var result = new Dialog(this).Set(x => x.Accepted.Handle(OnSelectionChanged));

            DialogOpenning.Raise(result);

            return result;
        }

        async Task OnSelectionChanged()
        {
            SelectedText = Source.SelectedItems.Select(i => i.Text).ToString(", ");

            if (ActualHeight < Label.ActualHeight || ActualHeight > Label.ActualHeight)
            {
                Height.BindTo(Label.Height, Padding.Top, Padding.Bottom, (h, pt, pb) => h +
                Math.Max(pt, Border.Top) + Math.Max(pb, Border.Bottom));

                Height.UpdateOn(BorderChanged);
            }

            await SelectionChanged.Raise();
            await Nav.HidePopUp();
        }

        public override void Dispose()
        {
            SelectionChanged?.Dispose();
            base.Dispose();
        }
    }
}