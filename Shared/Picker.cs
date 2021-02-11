namespace Zebble
{
    using System.Threading.Tasks;
    using Olive;

    public abstract class Picker : Stack, IBindableInput
    {
        IBindable SelectedTextBindable;
        IBinding SelectedTextBinding;

        public readonly AsyncEvent SelectedTextChanged = new AsyncEvent(ConcurrentEventRaisePolicy.Queue);

        public readonly TextView Label = new TextView { Id = "Label" }.Ignored();
        public readonly TextView PlaceholderLabel = new TextView { Id = "PlaceholderLabel", Text = "Please select" };
        public readonly ImageView Caret = new ImageView { Id = "Caret" };

        protected Picker() : base(RepeatDirection.Horizontal) { }

        public string Placeholder { get => PlaceholderLabel.Text; set => PlaceholderLabel.Text = value; }

        public bool AllowNull { get; set; }

        public object SelectedText
        {
            get => Label.Text;
            set
            {
                if (value is IBindable bindable)
                {
                    SelectedTextBinding?.Remove();

                    SelectedTextBindable = bindable;
                    SelectedTextBinding = bindable.AddBinding(this, nameof(SelectedText));
                }
                else
                {
                    var @string = value as string;

                    if (@string == Label.Text) return;

                    Label.Text = @string;
                    Label.Style.Ignored = @string.IsEmpty();
                    PlaceholderLabel.Style.Ignored = @string.HasValue();

                    SelectedTextChanged.Raise();
                }
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

        public override void Dispose()
        {
            SelectedTextChanged?.Dispose();
            base.Dispose();
        }

        public virtual void AddBinding(Bindable bindable)
        {
            if (bindable == SelectedTextBindable)
                SelectedTextChanged.Handle(() => bindable.SetUserValue(SelectedText));
        }
    }
}
