namespace Zebble
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    partial class ItemPicker
    {
        public class Dialog : Zebble.Dialog
        {
            const int AUTO_SEARCH_THRESHOLD = 10;
            ItemPicker Picker;
            Stack Stack = new Stack();
            public readonly OptionsList List = new OptionsList();
            SearchInput Search = new SearchInput();

            public readonly AsyncEvent Accepted = new AsyncEvent();

            public readonly Button RemoveButton = new Button { Text = "Remove" };
            public readonly Button CancelButton = new Button { Text = "Cancel" };
            public readonly Button OkButton = new Button { Text = "OK", CssClass = "primary-button" };

            public Dialog(ItemPicker picker)
            {
                Title.Text("Please select");
                Picker = picker;
                List.Source = Picker.Source;
            }

            public override async Task OnInitializing()
            {
                await base.OnInitializing();

                await Content.Add(Stack);

                if (NeedsSearching()) await EnableSearching();

                List.Set(x => x.SelectedItemChanged.Handle(OnSelectedItemChanged));
                await Stack.Add(List);
                await ButtonsRow.Add(CancelButton.On(x => x.Tapped, () => Nav.HidePopUp()));

                if (Picker.AllowNull && !Picker.Source.MultiSelect && Picker.Source.SelectedValue != null)
                    await ButtonsRow.Add(RemoveButton.On(x => x.Tapped, (Action)RemoveButtonTapped));

                if (Picker.Source.MultiSelect) await ButtonsRow.Add(OkButton.On(x => x.Tapped, Accepted.Raise));
            }

            async Task EnableSearching()
            {
                await Stack.Add(Search.On(x => x.Searched, OnSearched));
                //List.List.LazyLoad = true;
            }

            async Task OnSearched()
            {
                await Waiting.Show();

                var keywords = Search.Text.Split(' ').Trim().ToArray();

                var selectedItems = List.List.ItemViews.Where(r => r.IsSelected).ToArray();

                var toShow = Picker.Source.Items.Where(i =>
                 selectedItems.Any(s => s.Value == i.Value)
                 ||
                 i.Text.ContainsAll(keywords, caseSensitive: false)).ToArray();

                await List.List.UpdateSource(toShow);

                // Re-select the items
                List.List.ItemViews.Where(i => selectedItems.Any(s => s.Value == i.Value))
                    .Do(i => i.IsSelected = true);

                await Waiting.Hide();
            }

            bool NeedsSearching()
            {
                if (Picker.Searchable.HasValue) return Picker.Searchable.Value;
                return Picker.Source.Items.Count >= AUTO_SEARCH_THRESHOLD;
            }

            Task OnSelectedItemChanged(OptionsList.Option row)
            {
                if (row.IsSelected && !List.Source.MultiSelect && row.Native != null)
                    return Accepted.Raise();
                else return Task.CompletedTask;
            }

            void RemoveButtonTapped()
            {
                List.List.ItemViews.Where(x => x.IsSelected).Do(i => i.IsSelected = false);
                Accepted.Raise();
            }

            public override void Dispose()
            {
                Accepted?.Dispose();
                base.Dispose();
            }
        }
    }
}