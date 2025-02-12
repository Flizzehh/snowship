using System;
using System.Collections.Generic;
using UnityEngine;

namespace Snowship.NJob
{
	public interface IJobParams
	{
		virtual List<Func<TileManager.Tile, int, bool>> SelectionConditions => null;
		Sprite JobPreviewSprite { get; }
	}
}