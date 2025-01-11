using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Snowship.NCaravan;
using Snowship.NResource.Models;
using UnityEngine;

namespace Snowship.NPersistence {
	public partial class PCaravan : PersistenceHandler {

		private readonly PInventory pInventory = new PInventory();
		private readonly PLife pLife = new PLife();
		private readonly PHuman pHuman = new PHuman();

		public enum CaravanProperty {
			CaravanTimer,
			Caravan,
			Type,
			Location,
			TargetTile,
			ResourceGroup,
			LeaveTimer,
			Leaving,
			Inventory,
			ResourcesToTrade,
			ConfirmedResourcesToTrade,
			Traders
		}

		public enum LocationProperty {
			Location,
			Name,
			Wealth,
			ResourceRichness,
			CitySize,
			BiomeType
		}

		public enum TraderProperty {
			Trader,
			Life,
			Human,
			LeaveTile,
			TradingPosts
		}

		public enum TradeResourceAmountProperty {
			TradeResourceAmount,
			Type,
			CaravanAmount,
			TradeAmount,
			Price
		}

		public enum ConfirmedTradeResourceAmountProperty {
			ConfirmedTradeResourceAmount,
			Type,
			TradeAmount,
			AmountRemaining
		}

		public void SaveCaravans(string saveDirectoryPath) {

			StreamWriter file = CreateFileAtDirectory(saveDirectoryPath, "caravans.snowship");

			file.WriteLine(CreateKeyValueString(CaravanProperty.CaravanTimer, GameManager.caravanM.caravanTimer, 0));

			foreach (Caravan caravan in GameManager.caravanM.caravans) {
				file.WriteLine(CreateKeyValueString(CaravanProperty.Caravan, string.Empty, 0));

				file.WriteLine(CreateKeyValueString(CaravanProperty.Type, caravan.caravanType, 1));

				file.WriteLine(CreateKeyValueString(CaravanProperty.Location, string.Empty, 1));
				file.WriteLine(CreateKeyValueString(LocationProperty.Name, caravan.location.name, 2));
				file.WriteLine(CreateKeyValueString(LocationProperty.Wealth, caravan.location.wealth, 2));
				file.WriteLine(CreateKeyValueString(LocationProperty.ResourceRichness, caravan.location.resourceRichness, 2));
				file.WriteLine(CreateKeyValueString(LocationProperty.CitySize, caravan.location.citySize, 2));
				file.WriteLine(CreateKeyValueString(LocationProperty.BiomeType, caravan.location.biomeType, 2));

				file.WriteLine(CreateKeyValueString(CaravanProperty.TargetTile, FormatVector2ToString(caravan.targetTile.obj.transform.position), 1));

				file.WriteLine(CreateKeyValueString(CaravanProperty.ResourceGroup, caravan.resourceGroup.type, 1));

				file.WriteLine(CreateKeyValueString(CaravanProperty.LeaveTimer, caravan.leaveTimer, 1));
				file.WriteLine(CreateKeyValueString(CaravanProperty.Leaving, caravan.leaving, 1));

				pInventory.WriteInventoryLines(file, caravan.GetInventory(), 1);

				List<TradeResourceAmount> tradeResourceAmounts = caravan.GenerateTradeResourceAmounts();
				if (tradeResourceAmounts.Count > 0) {
					file.WriteLine(CreateKeyValueString(CaravanProperty.ResourcesToTrade, string.Empty, 1));
					foreach (TradeResourceAmount tra in caravan.GenerateTradeResourceAmounts()) {
						file.WriteLine(CreateKeyValueString(TradeResourceAmountProperty.TradeResourceAmount, string.Empty, 2));

						file.WriteLine(CreateKeyValueString(TradeResourceAmountProperty.Type, tra.resource.type, 3));

						file.WriteLine(CreateKeyValueString(TradeResourceAmountProperty.CaravanAmount, tra.caravanAmount, 3));

						file.WriteLine(CreateKeyValueString(TradeResourceAmountProperty.TradeAmount, tra.GetTradeAmount(), 3));

						file.WriteLine(CreateKeyValueString(TradeResourceAmountProperty.Price, tra.caravanResourcePrice, 3));
					}
				}

				if (caravan.confirmedResourcesToTrade.Count > 0) {
					file.WriteLine(CreateKeyValueString(CaravanProperty.ConfirmedResourcesToTrade, string.Empty, 1));
					foreach (ConfirmedTradeResourceAmount ctra in caravan.confirmedResourcesToTrade) {
						file.WriteLine(CreateKeyValueString(ConfirmedTradeResourceAmountProperty.ConfirmedTradeResourceAmount, string.Empty, 2));

						file.WriteLine(CreateKeyValueString(ConfirmedTradeResourceAmountProperty.Type, ctra.resource.type, 3));

						file.WriteLine(CreateKeyValueString(ConfirmedTradeResourceAmountProperty.TradeAmount, ctra.tradeAmount, 3));

						file.WriteLine(CreateKeyValueString(ConfirmedTradeResourceAmountProperty.AmountRemaining, ctra.amountRemaining, 3));
					}
				}

				file.WriteLine(CreateKeyValueString(CaravanProperty.Traders, string.Empty, 1));
				foreach (Trader trader in caravan.traders) {
					file.WriteLine(CreateKeyValueString(TraderProperty.Trader, string.Empty, 2));

					pLife.WriteLifeLines(file, trader, 3);

					pHuman.WriteHumanLines(file, trader, 3);

					if (trader.leaveTile != null) {
						file.WriteLine(CreateKeyValueString(TraderProperty.LeaveTile, FormatVector2ToString(trader.leaveTile.obj.transform.position), 3));
					}

					if (trader.tradingPosts != null && trader.tradingPosts.Count > 0) {
						file.WriteLine(CreateKeyValueString(TraderProperty.TradingPosts, string.Join(";", trader.tradingPosts.Select(tp => FormatVector2ToString(tp.zeroPointTile.obj.transform.position)).ToArray()), 3));
					}
				}
			}

			file.Close();
		}

		public List<PersistenceCaravan> LoadCaravans(string path) {
			List<PersistenceCaravan> persistenceCaravans = new List<PersistenceCaravan>();

			List<KeyValuePair<string, object>> properties = GetKeyValuePairsFromFile(path);
			foreach (KeyValuePair<string, object> property in properties) {
				switch ((CaravanProperty)Enum.Parse(typeof(CaravanProperty), property.Key)) {
					case CaravanProperty.CaravanTimer:
						GameManager.caravanM.caravanTimer = int.Parse((string)property.Value);
						break;
					case CaravanProperty.Caravan:

						List<KeyValuePair<string, object>> caravanProperties = (List<KeyValuePair<string, object>>)property.Value;

						CaravanType? type = null;
						Location location = null;
						Vector2? targetTilePosition = null;
						ResourceManager.ResourceGroup resourceGroup = null;
						int? leaveTimer = null;
						bool? leaving = null;
						PInventory.PersistenceInventory persistenceInventory = null;
						List<PersistenceTradeResourceAmount> persistenceResourcesToTrade = new List<PersistenceTradeResourceAmount>();
						List<PersistenceConfirmedTradeResourceAmount> persistenceConfirmedResourcesToTrade = new List<PersistenceConfirmedTradeResourceAmount>();
						List<PersistenceTrader> persistenceTraders = new List<PersistenceTrader>();

						foreach (KeyValuePair<string, object> caravanProperty in caravanProperties) {
							switch ((CaravanProperty)Enum.Parse(typeof(CaravanProperty), caravanProperty.Key)) {
								case CaravanProperty.Type:
									type = (CaravanType)Enum.Parse(typeof(CaravanType), (string)caravanProperty.Value);
									break;
								case CaravanProperty.Location:

									string locationName = null;
									string locationWealth = null;
									string locationResourceRichness = null;
									string locationCitySize = null;
									TileManager.Biome.TypeEnum? locationBiomeType = null;

									foreach (KeyValuePair<string, object> locationProperty in (List<KeyValuePair<string, object>>)caravanProperty.Value) {
										switch ((LocationProperty)Enum.Parse(typeof(LocationProperty), locationProperty.Key)) {
											case LocationProperty.Name:
												locationName = (string)locationProperty.Value;
												break;
											case LocationProperty.Wealth:
												locationWealth = (string)locationProperty.Value;
												break;
											case LocationProperty.ResourceRichness:
												locationResourceRichness = (string)locationProperty.Value;
												break;
											case LocationProperty.CitySize:
												locationCitySize = (string)locationProperty.Value;
												break;
											case LocationProperty.BiomeType:
												locationBiomeType = (TileManager.Biome.TypeEnum)Enum.Parse(typeof(TileManager.Biome.TypeEnum), (string)locationProperty.Value);
												break;
											default:
												Debug.LogError("Unknown location property: " + locationProperty.Key + " " + locationProperty.Value);
												break;
										}
									}

									location = new Location(
										locationName,
										locationWealth,
										locationResourceRichness,
										locationCitySize,
										locationBiomeType.Value
									);
									break;
								case CaravanProperty.TargetTile:
									targetTilePosition = new Vector2(float.Parse(((string)caravanProperty.Value).Split(',')[0]), float.Parse(((string)caravanProperty.Value).Split(',')[1]));
									break;
								case CaravanProperty.ResourceGroup:
									resourceGroup = GameManager.resourceM.GetResourceGroupByEnum((ResourceManager.ResourceGroupEnum)Enum.Parse(typeof(ResourceManager.ResourceGroupEnum), (string)caravanProperty.Value));
									break;
								case CaravanProperty.LeaveTimer:
									leaveTimer = int.Parse((string)caravanProperty.Value);
									break;
								case CaravanProperty.Leaving:
									leaving = bool.Parse((string)caravanProperty.Value);
									break;
								case CaravanProperty.Inventory:
									persistenceInventory = pInventory.LoadPersistenceInventory((List<KeyValuePair<string, object>>)caravanProperty.Value);
									break;
								case CaravanProperty.ResourcesToTrade:
									foreach (KeyValuePair<string, object> resourceToTradeProperty in (List<KeyValuePair<string, object>>)caravanProperty.Value) {
										switch ((TradeResourceAmountProperty)Enum.Parse(typeof(TradeResourceAmountProperty), resourceToTradeProperty.Key)) {
											case TradeResourceAmountProperty.TradeResourceAmount:

												ResourceManager.ResourceEnum? resourceToTradeType = null;
												int? resourceToTradeCaravanAmount = null;
												int? resourceToTradeTradeAmount = null;
												int? resourceToTradeCaravanPrice = null;

												foreach (KeyValuePair<string, object> resourceToTradeSubProperty in (List<KeyValuePair<string, object>>)resourceToTradeProperty.Value) {
													switch ((TradeResourceAmountProperty)Enum.Parse(typeof(TradeResourceAmountProperty), resourceToTradeSubProperty.Key)) {
														case TradeResourceAmountProperty.Type:
															resourceToTradeType = (ResourceManager.ResourceEnum)Enum.Parse(typeof(ResourceManager.ResourceEnum), (string)resourceToTradeSubProperty.Value);
															break;
														case TradeResourceAmountProperty.CaravanAmount:
															resourceToTradeCaravanAmount = int.Parse((string)resourceToTradeSubProperty.Value);
															break;
														case TradeResourceAmountProperty.TradeAmount:
															resourceToTradeTradeAmount = int.Parse((string)resourceToTradeSubProperty.Value);
															break;
														case TradeResourceAmountProperty.Price:
															resourceToTradeCaravanPrice = int.Parse((string)resourceToTradeSubProperty.Value);
															break;
														default:
															Debug.LogError("Unknown trade resource amount property: " + resourceToTradeSubProperty.Key + " " + resourceToTradeSubProperty.Value);
															break;
													}
												}

												persistenceResourcesToTrade.Add(
													new PersistenceTradeResourceAmount(
														resourceToTradeType,
														resourceToTradeCaravanAmount,
														resourceToTradeTradeAmount,
														resourceToTradeCaravanPrice
													));
												break;
											default:
												Debug.LogError("Unknown trade resource amount property: " + resourceToTradeProperty.Key + " " + resourceToTradeProperty.Value);
												break;
										}
									}
									break;
								case CaravanProperty.ConfirmedResourcesToTrade:
									foreach (KeyValuePair<string, object> confirmedResourceToTradeProperty in (List<KeyValuePair<string, object>>)caravanProperty.Value) {
										switch ((ConfirmedTradeResourceAmountProperty)Enum.Parse(typeof(ConfirmedTradeResourceAmountProperty), confirmedResourceToTradeProperty.Key)) {
											case ConfirmedTradeResourceAmountProperty.ConfirmedTradeResourceAmount:

												ResourceManager.ResourceEnum? confirmedResourceToTradeType = null;
												int? confirmedResourceToTradeTradeAmount = null;
												int? confirmedResourceToTradeAmountRemaining = null;

												foreach (KeyValuePair<string, object> confirmedResourceToTradeSubProperty in (List<KeyValuePair<string, object>>)confirmedResourceToTradeProperty.Value) {
													switch ((ConfirmedTradeResourceAmountProperty)Enum.Parse(typeof(ConfirmedTradeResourceAmountProperty), confirmedResourceToTradeSubProperty.Key)) {
														case ConfirmedTradeResourceAmountProperty.Type:
															confirmedResourceToTradeType = (ResourceManager.ResourceEnum)Enum.Parse(typeof(ResourceManager.ResourceEnum), (string)confirmedResourceToTradeSubProperty.Value);
															break;
														case ConfirmedTradeResourceAmountProperty.TradeAmount:
															confirmedResourceToTradeTradeAmount = int.Parse((string)confirmedResourceToTradeSubProperty.Value);
															break;
														case ConfirmedTradeResourceAmountProperty.AmountRemaining:
															confirmedResourceToTradeAmountRemaining = int.Parse((string)confirmedResourceToTradeSubProperty.Value);
															break;
														default:
															Debug.LogError("Unknown confirmed trade resource amount property: " + confirmedResourceToTradeSubProperty.Key + " " + confirmedResourceToTradeSubProperty.Value);
															break;
													}
												}

												persistenceConfirmedResourcesToTrade.Add(
													new PersistenceConfirmedTradeResourceAmount(
														confirmedResourceToTradeType,
														confirmedResourceToTradeTradeAmount,
														confirmedResourceToTradeAmountRemaining
													));
												break;
											default:
												Debug.LogError("Unknown confirmed trade resource amount property: " + confirmedResourceToTradeProperty.Key + " " + confirmedResourceToTradeProperty.Value);
												break;
										}
									}
									break;
								case CaravanProperty.Traders:
									foreach (KeyValuePair<string, object> traderProperty in (List<KeyValuePair<string, object>>)caravanProperty.Value) {
										switch ((TraderProperty)Enum.Parse(typeof(TraderProperty), traderProperty.Key)) {
											case TraderProperty.Trader:

												PLife.PersistenceLife persistenceLife = null;
												PHuman.PersistenceHuman persistenceHuman = null;
												TileManager.Tile traderLeaveTile = null;
												List<ResourceManager.TradingPost> traderTradingPosts = new List<ResourceManager.TradingPost>();

												foreach (KeyValuePair<string, object> traderSubProperty in (List<KeyValuePair<string, object>>)traderProperty.Value) {
													switch ((TraderProperty)Enum.Parse(typeof(TraderProperty), traderSubProperty.Key)) {
														case TraderProperty.Life:
															persistenceLife = pLife.LoadPersistenceLife((List<KeyValuePair<string, object>>)traderSubProperty.Value);
															break;
														case TraderProperty.Human:
															persistenceHuman = pHuman.LoadPersistenceHuman((List<KeyValuePair<string, object>>)traderSubProperty.Value);
															break;
														case TraderProperty.LeaveTile:
															traderLeaveTile = GameManager.colonyM.colony.map.GetTileFromPosition(new Vector2(float.Parse(((string)traderSubProperty.Value).Split(',')[0]), float.Parse(((string)traderSubProperty.Value).Split(',')[1])));
															break;
														case TraderProperty.TradingPosts:
															foreach (string vector2String in ((string)traderSubProperty.Value).Split(';')) {
																TileManager.Tile tradingPostZeroPointTile = GameManager.colonyM.colony.map.GetTileFromPosition(new Vector2(float.Parse(vector2String.Split(',')[0]), float.Parse(vector2String.Split(',')[1])));
																traderTradingPosts.Add(GameManager.resourceM.tradingPosts.Find(tp => tp.zeroPointTile == tradingPostZeroPointTile));
															}
															break;
														default:
															Debug.LogError("Unknown trader property: " + traderSubProperty.Key + " " + traderSubProperty.Value);
															break;
													}
												}

												persistenceTraders.Add(
													new PersistenceTrader(
														persistenceLife,
														persistenceHuman,
														traderLeaveTile,
														traderTradingPosts
													));
												break;
											default:
												Debug.LogError("Unknown trader property: " + traderProperty.Key + " " + traderProperty.Value);
												break;
										}
									}
									break;
								default:
									Debug.LogError("Unknown caravan property: " + caravanProperty.Key + " " + caravanProperty.Value);
									break;
							}
						}

						persistenceCaravans.Add(
							new PersistenceCaravan(
								type,
								location,
								targetTilePosition,
								resourceGroup,
								leaveTimer,
								leaving,
								persistenceInventory,
								persistenceResourcesToTrade,
								persistenceConfirmedResourcesToTrade,
								persistenceTraders
							));
						break;
					default:
						Debug.LogError("Unknown caravan property: " + property.Key + " " + property.Value);
						break;
				}
			}

			GameManager.persistenceM.loadingState = PersistenceManager.LoadingState.LoadedCaravans;
			return persistenceCaravans;
		}

		public void ApplyLoadedCaravans(List<PersistenceCaravan> persistenceCaravans) {
			foreach (PersistenceCaravan persistenceCaravan in persistenceCaravans) {
				Caravan caravan = new Caravan() { numTraders = persistenceCaravan.persistenceTraders.Count, caravanType = persistenceCaravan.type.Value, location = persistenceCaravan.location, targetTile = GameManager.colonyM.colony.map.GetTileFromPosition(persistenceCaravan.targetTilePosition.Value), resourceGroup = persistenceCaravan.resourceGroup, leaveTimer = persistenceCaravan.leaveTimer.Value, leaving = persistenceCaravan.leaving.Value };

				caravan.GetInventory().maxWeight = persistenceCaravan.persistenceInventory.maxWeight.Value;
				caravan.GetInventory().maxVolume = persistenceCaravan.persistenceInventory.maxVolume.Value;
				foreach (ResourceManager.ResourceAmount resourceAmount in persistenceCaravan.persistenceInventory.resources) {
					caravan.GetInventory().ChangeResourceAmount(resourceAmount.resource, resourceAmount.amount, false);
				}

				foreach (PersistenceTradeResourceAmount persistenceTradeResourceAmount in persistenceCaravan.persistenceResourcesToTrade) {
					TradeResourceAmount tradeResourceAmount = new TradeResourceAmount(
						GameManager.resourceM.GetResourceByEnum(persistenceTradeResourceAmount.type.Value),
						persistenceTradeResourceAmount.caravanAmount.Value,
						0,
						caravan
					) { caravanResourcePrice = persistenceTradeResourceAmount.caravanPrice.Value };
					tradeResourceAmount.SetTradeAmount(persistenceTradeResourceAmount.tradeAmount.Value); // Added to caravan through tra.SetTradeAmount() -> caravan.SetSelectedResource()
				}

				foreach (PersistenceConfirmedTradeResourceAmount persistenceConfirmedTradeResourceAmount in persistenceCaravan.persistenceConfirmedResourcesToTrade) {
					caravan.confirmedResourcesToTrade.Add(
						new ConfirmedTradeResourceAmount(
							GameManager.resourceM.GetResourceByEnum(persistenceConfirmedTradeResourceAmount.type.Value),
							persistenceConfirmedTradeResourceAmount.tradeAmount.Value
						) { amountRemaining = persistenceConfirmedTradeResourceAmount.amountRemaining.Value }
					);
				}

				foreach (PersistenceTrader persistenceTrader in persistenceCaravan.persistenceTraders) {
					Trader trader = new Trader(
						GameManager.colonyM.colony.map.GetTileFromPosition(persistenceTrader.persistenceLife.position.Value),
						persistenceTrader.persistenceLife.health.Value,
						caravan
					) { gender = persistenceTrader.persistenceLife.gender.Value, previousPosition = persistenceTrader.persistenceLife.previousPosition.Value };

					trader.obj.transform.position = persistenceTrader.persistenceLife.position.Value;

					trader.SetName(persistenceTrader.persistenceHuman.name);

					trader.bodyIndices[HumanManager.Human.Appearance.Skin] = persistenceTrader.persistenceHuman.skinIndex.Value;
					trader.moveSprites = GameManager.humanM.humanMoveSprites[trader.bodyIndices[HumanManager.Human.Appearance.Skin]];
					trader.bodyIndices[HumanManager.Human.Appearance.Hair] = persistenceTrader.persistenceHuman.hairIndex.Value;

					trader.GetInventory().maxWeight = persistenceTrader.persistenceHuman.persistenceInventory.maxWeight.Value;
					trader.GetInventory().maxVolume = persistenceTrader.persistenceHuman.persistenceInventory.maxVolume.Value;
					foreach (ResourceManager.ResourceAmount resourceAmount in persistenceTrader.persistenceHuman.persistenceInventory.resources) {
						trader.GetInventory().ChangeResourceAmount(resourceAmount.resource, resourceAmount.amount, false);
					}

					foreach (KeyValuePair<HumanManager.Human.Appearance, ResourceManager.Clothing> appearanceToClothingKVP in persistenceTrader.persistenceHuman.clothes) {
						trader.GetInventory().ChangeResourceAmount(GameManager.resourceM.GetResourceByEnum(appearanceToClothingKVP.Value.type), 1, false);
						trader.ChangeClothing(appearanceToClothingKVP.Key, appearanceToClothingKVP.Value);
					}

					if (persistenceTrader.persistenceLife.pathEndPosition.HasValue) {
						trader.MoveToTile(GameManager.colonyM.colony.map.GetTileFromPosition(persistenceTrader.persistenceLife.pathEndPosition.Value), true);
					}

					trader.leaveTile = persistenceTrader.leaveTile;

					trader.tradingPosts = persistenceTrader.tradingPosts;

					caravan.traders.Add(trader);
				}

				GameManager.caravanM.AddCaravan(caravan);
			}
		}

	}

}
