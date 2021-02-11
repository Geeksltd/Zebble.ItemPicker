namespace Zebble
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Olive;

    public partial class ItemPicker : Picker, FormField.IPlaceHolderControl, FormField.IControl
    {
        IBindable DataSourceBindable;
        IBinding DataSourceBinding;
        IBindable SelectedValueBindable;
        IBinding SelectedValueBinding;

        public readonly AsyncEvent DataSourceChanged = new AsyncEvent(ConcurrentEventRaisePolicy.Queue);
        public readonly AsyncEvent SelectionChanged = new AsyncEvent(ConcurrentEventRaisePolicy.Queue);
        public readonly OptionsDataSource Source = new OptionsDataSource();
        public readonly AsyncEvent<Dialog> DialogOpenning = new AsyncEvent<Dialog>();

        public bool ButtonsAtTop { get; set; }

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
                if (value is IBindable bindable)
                {
                    SelectedValueBinding?.Remove();

                    SelectedValueBindable = bindable;
                    SelectedValueBinding = bindable.AddBinding(this, nameof(SelectedValue));
                }
                else
                {
                    if (Source.Value == value) return;

                    Source.Value = value;
                    SelectedText = Source.SelectedValues.OrEmpty().ToString(", ");

                    SelectionChanged.Raise();
                }
            }
        }

        public IEnumerable<OptionsDataSource.DataItem> SelectedItem => Source.SelectedItems;

        public dynamic DataSource
        {
            get => Source.DataSource;
            set
            {
                if (value is IBindable bindable)
                {
                    DataSourceBinding?.Remove();

                    DataSourceBindable = bindable;
                    DataSourceBinding = bindable.AddBinding(this, nameof(DataSource));
                }
                else
                {
                    if (value == Source.DataSource) return;

                    Source.DataSource = value;
                    SelectedValue = SelectedValue;

                    DataSourceChanged.Raise();
                }
            }
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
            DataSourceChanged?.Dispose();
            SelectionChanged?.Dispose();
            base.Dispose();
        }

        public override void AddBinding(Bindable bindable)
        {
            if (bindable == DataSourceBindable)
                DataSourceChanged.Handle(() => bindable.SetUserValue(DataSource));
            else if (bindable == SelectedValueBindable)
                SelectionChanged.Handle(() => bindable.SetUserValue(SelectedValue));
            base.AddBinding(bindable);
        }
    }
}