namespace Zebble
{
    using System;
    using System.Threading.Tasks;

    public abstract class Picker : Stack
    {
        public readonly TextView Label = new TextView { Id = "Label" }.Ignored();
        public readonly TextView PlaceholderLabel = new TextView { Id = "PlaceholderLabel", Text = "Please select" };
        public readonly ImageView Caret = new ImageView { Id = "Caret" };

        protected Picker() : base(RepeatDirection.Horizontal) { }

        public string Placeholder { get => PlaceholderLabel.Text; set => PlaceholderLabel.Text = value; }

        public bool AllowNull { get; set; }

        public string SelectedText
        {
            get => Label.Text;
            set
            {
                Label.Text = value;
                Label.Style.Ignored = value.LacksValue();
                PlaceholderLabel.Style.Ignored = value.HasValue();
            }
        }

        public override async Task OnInitializing()
        {
            await base.OnInitializing();
            await AddRange(new View[] { Label, PlaceholderLabel, Caret });
            this.On(x => x.Tapped, ShowOptionsDialog);
        }

        protected virtual async Task ShowOptionsDialog()
        {
            await Flash();
            await Nav.ShowPopUp(CreateDialog());
        }

        protected abstract Dialog CreateDialog();
    }
}
