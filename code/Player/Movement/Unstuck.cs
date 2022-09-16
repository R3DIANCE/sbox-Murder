using Sandbox;

namespace Murder;

public class Unstuck
{
	public WalkController Controller;

	private int _stuckTries = 0;

	public Unstuck( WalkController controller )
	{
		Controller = controller;
	}

	public virtual bool TestAndFix()
	{
		var result = Controller.TraceBBox( Controller.Position, Controller.Position );

		// Not stuck, we cool
		if ( !result.StartedSolid )
		{
			_stuckTries = 0;
			return false;
		}

		if ( result.StartedSolid )
		{
			if ( BasePlayerController.Debug )
			{
				DebugOverlay.Text( $"[stuck in {result.Entity}]", Controller.Position, Color.Red );
				DebugOverlay.Box( result.Entity, Color.Red );
			}
		}

		//
		// Client can't jiggle its way out, needs to wait for
		// server correction to come
		//
		if ( Host.IsClient )
			return true;

		var AttemptsPerTick = 20;

		for ( var i = 0; i < AttemptsPerTick; i++ )
		{
			var pos = Controller.Position + Vector3.Random.Normal * (((float)_stuckTries) / 2.0f);

			// First try the up direction for moving platforms
			if ( i == 0 )
				pos = Controller.Position + Vector3.Up * 5;

			result = Controller.TraceBBox( pos, pos );

			if ( !result.StartedSolid )
			{
				if ( BasePlayerController.Debug )
				{
					DebugOverlay.Text( $"unstuck after {_stuckTries} tries ({_stuckTries * AttemptsPerTick} tests)", Controller.Position, Color.Green, 5.0f );
					DebugOverlay.Line( pos, Controller.Position, Color.Green, 5.0f, false );
				}

				Controller.Position = pos;
				return false;
			}
			else
			{
				if ( BasePlayerController.Debug )
					DebugOverlay.Line( pos, Controller.Position, Color.Yellow, 0.5f, false );
			}
		}

		_stuckTries++;

		return true;
	}
}