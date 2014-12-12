﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EssentialsPlugin.Utility;
using Sandbox.ModAPI;
using SEModAPIInternal.API.Common;
using Sandbox.Common.ObjectBuilders;
using VRageMath;
using System.Threading;
using SEModAPIInternal.API.Entity;
using SEModAPIInternal.API.Entity.Sector.SectorObject;
using SEModAPIInternal.API.Entity.Sector.SectorObject.CubeGrid;
using SEModAPIInternal.API.Entity.Sector.SectorObject.CubeGrid.CubeBlock;

namespace EssentialsPlugin.ProcessHandler
{
	public class ProcessWaypoints : ProcessHandlerBase
	{
		private List<ulong> m_waypointAdd = new List<ulong>();

		public ProcessWaypoints()
		{

		}

		public override int GetUpdateResolution()
		{
			return 5000;
		}

		public override void Handle()
		{
			lock (m_waypointAdd)
			{
				if(m_waypointAdd.Count < 1)
					return;
			}

			if (MyAPIGateway.Players == null)
				return;

			List<IMyPlayer> players = new List<IMyPlayer>();
			bool result = false;
//			Wrapper.GameAction(() =>
//			{
				try
				{
					MyAPIGateway.Players.GetPlayers(players, null);
					result = true;
				}
				catch (Exception ex)
				{
					Logging.WriteLineAndConsole(string.Format("Waypoints(): Unable to get player list: {0}", ex.ToString()));
				}
//			});

			if (!result)
				return;

			lock (m_waypointAdd)
			{
				for (int r = m_waypointAdd.Count - 1; r >= 0; r--)
				{
					ulong steamId = m_waypointAdd[r];

					IMyPlayer player = players.FirstOrDefault(x => x.SteamUserId == steamId && x.Controller != null && x.Controller.ControlledEntity != null);
					if (player != null)
					{
						Logging.WriteLineAndConsole("Player in game, creating waypoints");
						m_waypointAdd.Remove(steamId);

						List<WaypointItem> waypointItems = Waypoints.Instance.Get(steamId);
						foreach (WaypointItem item in waypointItems)
						{
							Communication.SendClientMessage(steamId, string.Format("/waypoint add \"{0}\" \"{1}\" {2} {3} {4} {5}", item.Name.ToLower(), item.Text, item.WaypointType.ToString(), item.Position.X, item.Position.Y, item.Position.Z));
						}
					}
				}
			}

			base.Handle();
		}

		public override void OnPlayerJoined(ulong remoteUserId)
		{
			if (!PluginSettings.Instance.WaypointsEnabled)
				return;

			lock(m_waypointAdd)
			{
				if (Waypoints.Instance.Get(remoteUserId).Count < 1)
					return;

				m_waypointAdd.Add(remoteUserId);
			}

			base.OnPlayerJoined(remoteUserId);
		}

		public override void OnPlayerLeft(ulong remoteUserId)
		{
			lock (m_waypointAdd)
			{
				m_waypointAdd.RemoveAll(x => x == remoteUserId);
			}

			base.OnPlayerLeft(remoteUserId);
		}

	}
}
