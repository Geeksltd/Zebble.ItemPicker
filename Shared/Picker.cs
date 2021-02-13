namespace Zebble
{
    using System.Threading.Tasks;
    using Olive;

    public abstract class Picker : Stack, IBindableInput
    {
        event InputChanged InputChanged;

        event InputChanged IBindableInput.InputChanged { add => InputChanged += value; remove => InputChanged -= value; }

        public readonly TextView Label = new TextView { Id = "Label" };
        public readonly ImageView Caret = new ImageView { Id = "Caret" };

        protected Picker() : base(RepeatDirection.Horizontal) { }

        public string Placeholder { get; set; }

        public bool AllowNull { get; set; }

        protected virtual void SetSelectedText(string text)
        {
            if (text == Label.Text) return;

            Label.Text = text.Or(Placeholder).OrEmpty();
            Label.PseudoCssState = "placeholder".OnlyWhen(text.IsEmpty());
        }

        protected virtual void RaiseInputChanged(string property) => InputChanged?.Invoke(property);

        public override async Task OnInitializing()
        {
            await base.OnInitializing();
            await AddRange(new View[] { Label, Caret });
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