namespace Chittak.Mobile;

public partial class MainPage : ContentPage
{
	int count;

	public MainPage()
	{
		InitializeComponent();

		// S0-08 spike: NSec (libsodium) Android'da ishlaydimi? Natija ekranda va logcat'da (tag: DOTNET).
		SpikeLabel.Text = Chittak.Protocol.Spike.NsecSpike.Run();
		Console.WriteLine($"CHITTAK-SPIKE {SpikeLabel.Text}");
	}

	private void OnCounterClicked(object? sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);
	}
}
