using Sandbox;
using System;
using System.Collections.Generic;

namespace Murder;

public static class Utils
{
	public static List<Player> GetAlivePlayers() => GetPlayers( ( pl ) => pl.IsAlive() );
	public static int MinimumPlayerCount() => GetPlayers( ( pl ) => !pl.IsForcedSpectator ).Count;
	public static bool HasMinimumPlayers() => MinimumPlayerCount() >= Game.MinPlayers;

	public static List<Client> GetAliveClientsWithRole( Role role ) => GetClients( ( pl ) => pl.IsAlive() && pl.Role == role );
	public static List<Client> GetDeadClients() => GetClients( ( pl ) => !pl.IsAlive() );

	private static List<Player> GetPlayers( Func<Player, bool> predicate )
	{
		List<Player> players = new();
		foreach ( var client in Client.All )
			if ( client.Pawn is Player player && predicate.Invoke( player ) )
				players.Add( player );

		return players;
	}

	private static List<Client> GetClients( Func<Player, bool> predicate )
	{
		List<Client> clients = new();
		foreach ( var client in Client.All )
			if ( client.Pawn is Player player && predicate.Invoke( player ) )
				clients.Add( client );

		return clients;
	}

	public static async void DelayAction( float seconds, Action callback )
	{
		await GameTask.DelaySeconds( seconds );
		callback?.Invoke();
	}
}
