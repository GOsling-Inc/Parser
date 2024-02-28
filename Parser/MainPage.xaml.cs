using System.Text;

namespace Parser
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
            var template = new DataTemplate(() =>
            {
                var index = new Label { FontSize = 14, TextColor = Color.FromArgb("#000"), Margin = 10, HorizontalOptions = LayoutOptions.Center };
                index.SetBinding(Label.TextProperty, new Binding { Path = "Index", StringFormat = "{0}" });

                var name = new Label { FontSize = 14, TextColor = Color.FromArgb("#000"), Margin = 10, HorizontalOptions=LayoutOptions.Center};
                name.SetBinding(Label.TextProperty, new Binding { Path = "Name", StringFormat = "{0}" });

                var count = new Label { FontSize = 14, TextColor = Color.FromArgb("#000"), Margin = 10, HorizontalOptions = LayoutOptions.Center };
                count.SetBinding(Label.TextProperty, new Binding { Path = "Count", StringFormat = "{0}" });

                var grid = new Grid
                {
                    BackgroundColor = Color.FromArgb("#ffffff"),
                    Padding = 5
                };
                grid.Add(index, 0, 0);
                grid.Add(name, 1, 0);
                grid.Add(count, 3, 0);
                grid.SetColumnSpan(name, 2);
                return grid;
            });
            List1.ItemTemplate = template;
            List2.ItemTemplate = template;
        }

        private async void OnClicked(object sender, EventArgs e)
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Pick source code"
            });
            if (file == null) return;
            string text;
            using (FileStream fstream = new(file.FullPath.ToString(), FileMode.Open))
            {
                byte[] buffer = new byte[fstream.Length];
                await fstream.ReadAsync(buffer);
                text = Encoding.Default.GetString(buffer);
            }
            var lists = new Parser().ParseScala(text);
            List1.ItemsSource = lists[0];
            List2.ItemsSource = lists[1];
            prog_dict.Text = $"Словарь программы: {lists[2][0].Index}";
            prog_len.Text = $"Длина программы: {lists[2][0].Name}";
            prog_volume.Text = $"Объём программы: {lists[2][0].Count}";
        }
    }

}
