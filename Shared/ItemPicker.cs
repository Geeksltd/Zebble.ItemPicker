namespace Zebble
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Olive;
    using System.Threading.Tasks;

    public abstract class ItemPicker : Picker { }

    public partial class ItemPicker<TSource> : ItemPicker
    {
        public readonly AsyncEvent<Dialog> DialogOpenning = new();

        readonly Mvvm.CollectionViewModel<SelectableItem<TSource>> Items = new();

        public IEnumerable<TSource> Source
        {
            get => Items.Select(v => v.Source.Value);
            set => Items.Replace(value.OrEmpty().ExceptNull());
        }

        public IEnumerable<TSource> SelectedItems
        {
            get => Items.Where(v => v.Selected.Value).Select(v => v.Source.Value);
            set
            {
                Items.Do(i => i.Selected.Value = value.OrEmpty().Contains(i.Source.Value));
                SetSelectedText(value.OrEmpty().ToString(", "));
            }
        }

        public TSource SelectedItem
        {
            get => SelectedItems.FirstOrDefault();
            set => SelectedItems = new[] { value }.ExceptNull();
        }

        public bool MultiSelect { get; set; }

        public bool ButtonsAtTop { get; set; }

        public bool? Searchable { get; set; }

        public int SearchCharacterCount { get; set; } = 1;

        protected override Dialog CreateDialog()
        {
            var result = new ItemPickerDialog<TSource>(this).Set(x => x.Accepted.Handle(OnSelectionChanged));
            DialogOpenning.Raise(result);
            return result;
        }

        async Task OnSelectionChanged()
        {
            SetSelectedText(SelectedItems.ToString(", "));
            RaiseInputChanged(nameof(SelectedItem));
            RaiseInputChanged(nameof(SelectedItems));

            if (ActualHeight < Label.ActualHeight || ActualHeight > Label.ActualHeight)
            {
                Height.BindTo(Label.Height, Padding.Top, Padding.Bottom, (h, pt, pb) => h +
                Math.Max(pt, Border.Top) + Math.Max(pb, Border.Bottom));
                Height.UpdateOn(BorderChanged);
            }

            await Nav.HidePopUp();
        }

        public override void Dispose()
        {
            Items.ClearBindings();
            base.Dispose();
        }
    }
}