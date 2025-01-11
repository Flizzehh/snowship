using System.Collections;
using System.Collections.Generic;
using Snowship.NPersistence;
using UnityEngine;

public class UniverseManager : IManager {

	public Universe universe = null;

	private readonly PUniverse pUniverse = new PUniverse();

	public static string GetRandomUniverseName() {
		//return GameManager.resourceM.GetRandomLocationName();
		return "Universe " + System.DateTime.Today.ToString("ddMMyyyy");
	}

	public Universe CreateUniverse(string name) {
		Universe universe = new Universe(name);

		this.universe = universe;

		pUniverse.CreateUniverse(universe);

		return universe;
	}

	public void SetUniverse(Universe universe) {
		this.universe = universe;
	}

	public class Universe {
		public string name;
		public string directory;
		public string lastSaveDateTime;
		public string lastSaveTimeChunk;

		public Universe(string name) {
			this.name = name;

			lastSaveDateTime = PersistenceHandler.GenerateSaveDateTimeString();
			lastSaveTimeChunk = PersistenceHandler.GenerateDateTimeString();
		}

		public void SetDirectory(string directory) {
			this.directory = directory;
		}

		public void SetLastSaveDateTime(string lastSaveDateTime, string lastSaveTimeChunk) {
			this.lastSaveDateTime = lastSaveDateTime;
			this.lastSaveTimeChunk = lastSaveTimeChunk;
		}
	}
}
