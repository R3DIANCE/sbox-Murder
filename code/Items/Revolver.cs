using Sandbox;
using SandboxEditor;
using System.Collections.Generic;

namespace Murder;

[Category( "Weapons" )]
[ClassName( "murder_weapon_revolver" )]
[EditorModel( "models/weapons/w_mr96.vmdl" )]
[HammerEntity]
[Title( "Revolver" )]
public partial class Revolver : Carriable
{
	[Net, Predicted]
	public bool BulletInClip { get; protected set; } = true;

	[Net, Local, Predicted]
	public bool IsReloading { get; protected set; }

	[Net, Local, Predicted]
	public TimeSince TimeSinceReload { get; protected set; }

	public override float DeployTime => 1.2f;
	public override string ViewModelPath { get; } = "models/weapons/v_mr96.vmdl";
	public override string WorldModelPath { get; } = "models/weapons/w_mr96.vmdl";

	public override void ActiveStart( Player player )
	{
		base.ActiveStart( player );

		IsReloading = false;
	}

	public override void Simulate( Client client )
	{
		if ( !BulletInClip && !IsReloading )
		{
			Reload();
			return;
		}

		if ( !IsReloading )
		{
			if ( Input.Pressed( InputButton.PrimaryAttack ) )
			{
				using ( LagCompensation() )
				{
					AttackPrimary();
				}
			}
		}
		else if ( TimeSinceReload > 2.2f )
		{
			IsReloading = false;
			BulletInClip = true;
		}
	}

	private void AttackPrimary()
	{
		BulletInClip = false;
		Owner.SetAnimParameter( "b_attack", true );
		ShootEffects();
		PlaySound( "sounds/weapons/mr96/mr96_fire-1.sound" );

		ShootBullet( 0.05f, 1.5f, 200, 3.0f );
	}

	private void Reload()
	{
		TimeSinceReload = 0;
		IsReloading = true;

		Owner.SetAnimParameter( "b_reload", true );
		ReloadEffects();
	}

	public override void SimulateAnimator( PawnAnimator animator )
	{
		base.SimulateAnimator( animator );

		animator.SetAnimParameter( "holdtype", 1 );
	}

	public override bool CanCarry( Player carrier )
	{
		return carrier.Role == Role.Bystander && carrier.TimeUntilClean && base.CanCarry( carrier );
	}

	protected void ShootBullet( float spread, float force, float damage, float bulletSize )
	{
		// Seed rand using the tick, so bullet cones match on client and server
		Rand.SetSeed( Time.Tick );

		var forward = Owner.EyeRotation.Forward;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
		forward = forward.Normal;

		foreach ( var trace in TraceBullet( Owner.EyePosition, Owner.EyePosition + forward * 20000f, bulletSize ) )
		{
			trace.Surface.DoBulletImpact( trace );

			var fullEndPosition = trace.EndPosition + trace.Direction * bulletSize;

			if ( !IsServer )
				continue;

			if ( !trace.Entity.IsValid() )
				continue;

			using ( Prediction.Off() )
			{
				var damageInfo = DamageInfo.FromBullet( trace.EndPosition, forward * 100f * force, damage )
					.UsingTraceResult( trace )
					.WithAttacker( Owner )
					.WithWeapon( this );

				trace.Entity.TakeDamage( damageInfo );
			}
		}
	}

	/// <summary>
	/// Does a trace from start to end, does bullet impact effects. Coded as an IEnumerable so you can return multiple
	/// hits, like if you're going through layers or ricocet'ing or something.
	/// </summary>
	protected IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius = 2.0f )
	{
		var underWater = Trace.TestPoint( start, "water" );

		var trace = Trace.Ray( start, end )
				.UseHitboxes()
				.WithAnyTags( "solid", "player", "glass", "interactable" )
				.Ignore( this )
				.Size( radius );

		//
		// If we're not underwater then we can hit water
		//
		if ( !underWater )
			trace = trace.WithAnyTags( "water" );

		var tr = trace.Run();

		if ( tr.Hit )
			yield return tr;

		//
		// Another trace, bullet going through thin material, penetrating water surface?
		//
	}

	[ClientRpc]
	protected virtual void ShootEffects()
	{
		Particles.Create( "particles/muzzle/flash_small.vpcf", EffectEntity, "muzzle" );

		ViewModelEntity?.SetAnimParameter( "fire", true );
		// CurrentRecoil += RecoilOnShoot;
	}

	[ClientRpc]
	protected virtual void ReloadEffects()
	{
		ViewModelEntity?.SetAnimParameter( "reload", true );
	}
}
