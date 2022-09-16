using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

public class Hud : RootPanel
{
	public Hud()
	{
		Local.Hud = this;

		StyleSheet.Load( "/UI/Hud.scss" );
		AddClass( "panel" );
		AddClass( "fullscreen" );

		Init();
	}

	private void Init()
	{
		AddChild<HintDisplay>();
		AddChild<PlayerInfo>();
		AddChild<InventoryWrapper>();
		AddChild<ChatBox>();
		AddChild<VoiceChatDisplay>();
		AddChild<RoundTimer>();
		AddChild<VoiceList>();
		AddChild<Scoreboard>();
		AddChild<FullScreenHintMenu>();
		AddChild<CarriableHint>();
		AddChild<DamageIndicator>();
	}

	[Event.Hotload]
	private void OnHotReload()
	{
		DeleteChildren( true );
		Init();
	}
}