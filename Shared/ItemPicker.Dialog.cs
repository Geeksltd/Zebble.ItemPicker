namespace Zebble
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Olive;

    partial class ItemPicker<TSource>
    {
        public class Dialog : Zebble.Dialog
        {
            const int AUTO_SEARCH_THRESHOLD = 10;
            ItemPicker<TSource> Picker;
            Stack Stack = new();
            public readonly OptionsList<TSource> List = new();
            TextInput Search = new();

            public readonly AsyncEvent Accepted = new();
            public readonly Button RemoveButton = new Button { Text = "Remove", Id = "RemoveButton" };
            public readonly Button CancelButton = new Button { Text = "Cancel", Id = "CancelButton" };
            public readonly Button OkButton = new Button { Text = "OK", Id = "OkButton", CssClass = "primary-button" };

            public Dialog(ItemPicker<TSource> picker)
            {
                Title.Text("Please select");
                Picker = picker;
                List.Source = picker.Source;
                List.SelectedItems = picker.SelectedItems;
                List.MultiSelect = picker.MultiSelect;
                ButtonsAtTop = picker.ButtonsAtTop;
            }

            public override async Task OnInitializing()
            {
                await base.OnInitializing();

                Content.Css.Height = new Length.BindingLengthRequest(lengths: new Length[]
                {
                    Root.Height, Title.Height, Search.Height, ButtonsRow.Height,
                    Padding.Top, Padding.Bottom, Margin.Top, Margin.Bottom,
                    Title.Margin.Top, Title.Margin.Bottom,
                    Search.Margin.Top, Search.Margin.Bottom,
                    ButtonsRow.Margin.Top, ButtonsRow.Margin.Bottom
                }, expression: l => { return l[0] - l.Except(l[0]).Sum(); });

                var autoHeightCalculator = List as IAutoContentHeightProvider;
                autoHeightCalculator?.Changed.Handle(() =>
                {
                    var height = autoHeightCalculator.Calculate();
                    List.Height(height);
                });

                if (NeedsSearching()) await EnableSearching();

                await Content.Add(Stack);

                (List as IBindableInput).InputChanged += OnSelectedItemChanged;

                await Stack.Add(List);
                await ButtonsRow.Add(CancelButton.On(x => x.Tapped, () => Nav.HidePopUp()));

                if (Picker.AllowNull && !Picker.MultiSelect && Picker.SelectedItem != null)
                    await ButtonsRow.Add(RemoveButton.On(x => x.Tapped, RemoveButtonTapped));

                if (Picker.MultiSelect) await ButtonsRow.Add(OkButton.On(x => x.Tapped, Complete));
            }

            Task EnableSearching()
            {
                Search.On(x => x.UserTextChanged, OnSearch);
                Search.On(x => x.UserTextChanged, OnSearch);
                return AddAfter(Title, Search);
            }

            void OnSearch()
            {
                if (Search.Text.Length < Picker.SearchCharacterCount && Search.Text.Length != 0) return;

                var keywords = Search.Text.Split(' ').Trim().ToArray();

                var alreadySelected = List.SelectedItems.ToArray();
                var searchMatch = Picker.Source.Where(i => i.ToString().ContainsAll(keywords, caseSensitive: false)).ToArray();
                List.Source = alreadySelected.Concat(searchMatch).Distinct().ToArray();

                List.SelectedItems = alreadySelected;
            }

            bool NeedsSearching()
            {
                if (Picker.Searchable.HasValue) return Picker.Searchable.Value;
                return Picker.Source.Count() >= AUTO_SEARCH_THRESHOLD;
            }

            void OnSelectedItemChanged(string changedProperty)
            {
                if (List.MultiSelect) return;
                if (changedProperty != nameof(List.SelectedItem)) return;
                if (List.SelectedItem != null) Complete();
            }

            void Complete()
            {
                Picker.SelectedItems = List.SelectedItems;
                Accepted.Raise();
            }

            void RemoveButtonTapped()
            {
                List.SelectedItems = null;
                Complete();
            }

            public override void Dispose()
            {
                Accepted?.Dispose();
                base.Dispose();
            }
        }
    }
}