using System.Collections.Generic;
using UnityEngine;

namespace FuSheng
{
    public static class Engine
    {
        /** 初始游戏状态 */
        public static GameState CreateGame()
        {
            var initialStats = new PlayerStatsImpl(100, 1000, 500, 50, 1);

            // 随机选择初始城市
            var randomCity = Data.CITIES[Random.Range(0, Data.CITIES.Count)];

            // 初始化市场价格
            var marketPrices = new List<MarketPrice>();
            foreach (var city in Data.CITIES)
            {
                foreach (var commodityId in city.Commodities)
                {
                    marketPrices.Add(new MarketPrice
                    {
                        CommodityId = commodityId,
                        CityId = city.Id,
                        CurrentPrice = Data.GetCommodityPrice(commodityId, city.Id, 1),
                        Trend = Random.value > 0.5 ? "up" : "down"
                    });
                }
            }

            return new GameState
            {
                Phase = GamePhase.Intro,
                Day = 1,
                MaxDays = 40,
                Stats = initialStats,
                CurrentCity = randomCity.Id,
                Inventory = new List<InventoryItem>(),
                MarketPrices = marketPrices,
                Logs = new List<DayLog>(),
                CurrentEvent = null,
                DayResult = null,
                FinalScore = 0,
                FinalRank = ""
            };
        }

        /** 开始游戏 */
        public static GameState StartGame(GameState gs)
        {
            return new GameState
            {
                Phase = GamePhase.Playing,
                Day = gs.Stats.Days,
                MaxDays = gs.MaxDays,
                Stats = new PlayerStatsImpl(gs.Stats),
                CurrentCity = gs.CurrentCity,
                Inventory = new List<InventoryItem>(gs.Inventory),
                MarketPrices = new List<MarketPrice>(gs.MarketPrices),
                Logs = new List<DayLog>(gs.Logs),
                CurrentEvent = null,
                DayResult = null,
                FinalScore = 0,
                FinalRank = ""
            };
        }

        /** 重新开始游戏 */
        public static GameState RestartGame()
        {
            return CreateGame();
        }

        /** 限制属性在合理范围 */
        private static PlayerStats ClampStats(PlayerStats s)
        {
            return new PlayerStatsImpl(
                Mathf.Max(0, Mathf.Min(100, s.Health)),
                Mathf.Max(0, s.Gold),
                Mathf.Max(0, s.Debt),
                Mathf.Max(10, s.Capacity),
                Mathf.Max(0, s.Days)
            );
        }

        /** 计算当前已用容量 */
        public static int CalculateCurrentCapacity(List<InventoryItem> inventory)
        {
            int total = 0;
            foreach (var item in inventory)
            {
                total += item.Quantity; // 每个商品占用1个容量单位
            }
            return total;
        }

        /** 移动到其他城池 */
        public static GameState MoveToLocation(GameState gs, string cityId)
        {
            var city = Data.GetCity(cityId);
            if (city == null)
                return gs;

            // 移动到同一城池不消耗健康，也不增加天数
            if (gs.CurrentCity == cityId)
                return gs;

            // 移动到不同城池：消耗健康并增加天数
            var newStats = ClampStats(new PlayerStatsImpl(gs.Stats)
            {
                Health = gs.Stats.Health - 2,
                Days = gs.Stats.Days + 1
            });

            // 为目标城市重新生成商品列表（根据概率）
            var newlyGeneratedCommodities = Data.GenerateCommoditiesByDoubleWeight(cityId);

            // 更新市场价格
            var newMarketPrices = new List<MarketPrice>();
            
            // 1. 保留其他城市的现有价格
            foreach (var price in gs.MarketPrices)
            {
                if (price.CityId != cityId)
                {
                    newMarketPrices.Add(new MarketPrice
                    {
                        CommodityId = price.CommodityId,
                        CityId = price.CityId,
                        CurrentPrice = Data.GetCommodityPrice(price.CommodityId, price.CityId, newStats.Days),
                        Trend = price.Trend
                    });
                }
            }

            // 2. 为新生成的商品添加价格
            foreach (var commodity in newlyGeneratedCommodities)
            {
                newMarketPrices.Add(new MarketPrice
                {
                    CommodityId = commodity.Id,
                    CityId = cityId,
                    CurrentPrice = Data.GetCommodityPrice(commodity.Id, cityId, newStats.Days),
                    Trend = Random.value > 0.5 ? "up" : "down"
                });
            }

            // 触发随机事件（30%概率）
            GameEvent currentEvent = null;
            if (Random.value < 0.3f)
            {
                currentEvent = Events.GetRandomEvent();
            }

            return new GameState
            {
                Phase = currentEvent != null ? GamePhase.Event : GamePhase.Playing,
                Day = newStats.Days,
                MaxDays = gs.MaxDays,
                Stats = newStats,
                CurrentCity = cityId,
                Inventory = new List<InventoryItem>(gs.Inventory),
                MarketPrices = newMarketPrices,
                Logs = new List<DayLog>(gs.Logs),
                CurrentEvent = currentEvent,
                DayResult = null,
                FinalScore = 0,
                FinalRank = ""
            };
        }

        /** 购买商品 */
        public static GameState BuyCommodity(GameState gs, string commodityId, string cityId, int quantity)
        {
            var commodity = Data.GetCommodity(commodityId);
            var currentPrice = gs.MarketPrices.Find(p => 
                p.CommodityId == commodityId && p.CityId == cityId
            )?.CurrentPrice ?? 0;

            if (commodity == null || currentPrice <= 0)
                return gs;

            var totalCost = currentPrice * quantity;
            var capacityNeeded = quantity; // 每个商品占用1个容量单位
            var currentCapacity = CalculateCurrentCapacity(gs.Inventory);

            // 检查条件
            if (gs.Stats.Gold < totalCost)
            {
                // 资金不足
                return gs;
            }
            if (currentCapacity + capacityNeeded > gs.Stats.Capacity)
            {
                // 容量不足
                return gs;
            }

            // 更新库存
            var newInventory = new List<InventoryItem>();
            bool found = false;
            foreach (var item in gs.Inventory)
            {
                if (item.CommodityId == commodityId)
                {
                    newInventory.Add(new InventoryItem
                    {
                        CommodityId = item.CommodityId,
                        Quantity = item.Quantity + quantity,
                        PurchasePrice = item.PurchasePrice,
                        PurchaseDay = item.PurchaseDay
                    });
                    found = true;
                }
                else
                {
                    newInventory.Add(item);
                }
            }

            if (!found)
            {
                newInventory.Add(new InventoryItem
                {
                    CommodityId = commodityId,
                    Quantity = quantity,
                    PurchasePrice = currentPrice,
                    PurchaseDay = gs.Day
                });
            }

            // 更新资金
            var newStats = ClampStats(new PlayerStatsImpl(gs.Stats)
            {
                Gold = gs.Stats.Gold - totalCost
            });

            return new GameState
            {
                Phase = gs.Phase,
                Day = gs.Day,
                MaxDays = gs.MaxDays,
                Stats = newStats,
                CurrentCity = gs.CurrentCity,
                Inventory = newInventory,
                MarketPrices = new List<MarketPrice>(gs.MarketPrices),
                Logs = new List<DayLog>(gs.Logs),
                CurrentEvent = gs.CurrentEvent,
                DayResult = null,
                FinalScore = 0,
                FinalRank = ""
            };
        }

        /** 出售商品 */
        public static GameState SellCommodity(GameState gs, string commodityId, string cityId, int quantity)
        {
            var existingItem = gs.Inventory.Find(item => item.CommodityId == commodityId);
            
            // 检查库存是否足够
            if (existingItem == null || existingItem.Quantity < quantity)
                return gs;
            
            // 检查是否当天购买的商品（防止刷钱漏洞）
            if (existingItem.PurchaseDay == gs.Day)
                return gs;

            var commodity = Data.GetCommodity(commodityId);
            var currentPrice = gs.MarketPrices.Find(p => 
                p.CommodityId == commodityId && p.CityId == cityId
            )?.CurrentPrice ?? 0;

            if (commodity == null || currentPrice <= 0)
                return gs;

            // 计算利润
            var purchaseCost = existingItem.PurchasePrice * quantity;
            var saleRevenue = currentPrice * quantity;

            // 更新库存
            var newInventory = new List<InventoryItem>();
            foreach (var item in gs.Inventory)
            {
                if (item.CommodityId == commodityId)
                {
                    if (item.Quantity > quantity)
                    {
                        newInventory.Add(new InventoryItem
                        {
                            CommodityId = item.CommodityId,
                            Quantity = item.Quantity - quantity,
                            PurchasePrice = item.PurchasePrice,
                            PurchaseDay = item.PurchaseDay
                        });
                    }
                }
                else
                {
                    newInventory.Add(item);
                }
            }

            // 更新资金
            var newStats = ClampStats(new PlayerStatsImpl(gs.Stats)
            {
                Gold = gs.Stats.Gold + saleRevenue
            });

            return new GameState
            {
                Phase = gs.Phase,
                Day = gs.Day,
                MaxDays = gs.MaxDays,
                Stats = newStats,
                CurrentCity = gs.CurrentCity,
                Inventory = newInventory,
                MarketPrices = new List<MarketPrice>(gs.MarketPrices),
                Logs = new List<DayLog>(gs.Logs),
                CurrentEvent = gs.CurrentEvent,
                DayResult = null,
                FinalScore = 0,
                FinalRank = ""
            };
        }

        /** 银行操作 - 还债或贷款 */
        public static GameState BankOperation(GameState gs, string operation, int amount)
        {
            if (operation == "repay")
            {
                // 还债
                if (gs.Stats.Gold < amount || gs.Stats.Debt < amount)
                    return gs;
                
                var newStats = ClampStats(new PlayerStatsImpl(gs.Stats)
                {
                    Gold = gs.Stats.Gold - amount,
                    Debt = gs.Stats.Debt - amount
                });

                return new GameState
                {
                    Phase = gs.Phase,
                    Day = gs.Day,
                    MaxDays = gs.MaxDays,
                    Stats = newStats,
                    CurrentCity = gs.CurrentCity,
                    Inventory = new List<InventoryItem>(gs.Inventory),
                    MarketPrices = new List<MarketPrice>(gs.MarketPrices),
                    Logs = new List<DayLog>(gs.Logs),
                    CurrentEvent = gs.CurrentEvent,
                    DayResult = null,
                    FinalScore = 0,
                    FinalRank = ""
                };
            }
            else if (operation == "loan")
            {
                // 贷款
                var maxLoan = Mathf.Min(1000, gs.Stats.Debt / 2); // 最多可贷当前负债的50%
                var actualAmount = Mathf.Min(amount, maxLoan);
                
                var newStats = ClampStats(new PlayerStatsImpl(gs.Stats)
                {
                    Gold = gs.Stats.Gold + actualAmount,
                    Debt = gs.Stats.Debt + actualAmount
                });

                return new GameState
                {
                    Phase = gs.Phase,
                    Day = gs.Day,
                    MaxDays = gs.MaxDays,
                    Stats = newStats,
                    CurrentCity = gs.CurrentCity,
                    Inventory = new List<InventoryItem>(gs.Inventory),
                    MarketPrices = new List<MarketPrice>(gs.MarketPrices),
                    Logs = new List<DayLog>(gs.Logs),
                    CurrentEvent = gs.CurrentEvent,
                    DayResult = null,
                    FinalScore = 0,
                    FinalRank = ""
                };
            }
            return gs;
        }

        /** 医院治疗 */
        public static GameState HospitalTreatment(GameState gs)
        {
            const int cost = 30; // 治疗费用
            if (gs.Stats.Gold < cost)
                return gs;

            var newStats = ClampStats(new PlayerStatsImpl(gs.Stats)
            {
                Gold = gs.Stats.Gold - cost,
                Health = Mathf.Min(100, gs.Stats.Health + 40)
            });

            return new GameState
            {
                Phase = gs.Phase,
                Day = gs.Day,
                MaxDays = gs.MaxDays,
                Stats = newStats,
                CurrentCity = gs.CurrentCity,
                Inventory = new List<InventoryItem>(gs.Inventory),
                MarketPrices = new List<MarketPrice>(gs.MarketPrices),
                Logs = new List<DayLog>(gs.Logs),
                CurrentEvent = gs.CurrentEvent,
                DayResult = null,
                FinalScore = 0,
                FinalRank = ""
            };
        }

        /** 触发随机事件 */
        public static GameState TriggerRandomEvent(GameState gs)
        {
            // 根据千古风流原版玩法：事件概率适中，约30%-40%
            const float baseChance = 0.3f; // 基础30%概率
            float dayFactor = Mathf.Min((float)gs.Day / gs.MaxDays, 1f); // 0到1的进度因子
            float eventChance = baseChance + (dayFactor * 0.1f); // 30%到40%概率
            
            if (Random.value > eventChance)
                return gs;

            var randomEvent = Events.GetRandomEvent();

            return new GameState
            {
                Phase = GamePhase.Event,
                Day = gs.Day,
                MaxDays = gs.MaxDays,
                Stats = new PlayerStatsImpl(gs.Stats),
                CurrentCity = gs.CurrentCity,
                Inventory = new List<InventoryItem>(gs.Inventory),
                MarketPrices = new List<MarketPrice>(gs.MarketPrices),
                Logs = new List<DayLog>(gs.Logs),
                CurrentEvent = randomEvent,
                DayResult = null,
                FinalScore = 0,
                FinalRank = ""
            };
        }

        /** 处理事件选择 */
        public static GameState ResolveEvent(GameState gs, int choiceIndex)
        {
            if (gs.CurrentEvent == null)
                return gs;

            if (choiceIndex < 0 || choiceIndex >= gs.CurrentEvent.Choices.Count)
                return gs;

            var choice = gs.CurrentEvent.Choices[choiceIndex];

            // 应用事件效果
            var newStats = new PlayerStatsImpl(gs.Stats);
            foreach (var effect in choice.Effects)
            {
                switch (effect.Key)
                {
                    case "Health":
                        newStats.Health += effect.Value;
                        break;
                    case "Gold":
                        newStats.Gold += effect.Value;
                        break;
                    case "Debt":
                        newStats.Debt += effect.Value;
                        break;
                    case "Capacity":
                        newStats.Capacity += effect.Value;
                        break;
                    case "Days":
                        newStats.Days += effect.Value;
                        break;
                }
            }

            newStats = (PlayerStatsImpl)ClampStats(newStats);

            return new GameState
            {
                Phase = GamePhase.Playing,
                Day = gs.Day,
                MaxDays = gs.MaxDays,
                Stats = newStats,
                CurrentCity = gs.CurrentCity,
                Inventory = new List<InventoryItem>(gs.Inventory),
                MarketPrices = new List<MarketPrice>(gs.MarketPrices),
                Logs = new List<DayLog>(gs.Logs),
                CurrentEvent = null,
                DayResult = null,
                FinalScore = 0,
                FinalRank = ""
            };
        }

        /** 检查游戏危险状态 */
        public static (bool isDanger, string message) CheckDangerStatus(GameState gs)
        {
            bool healthDanger = gs.Stats.Health <= 20;
            bool debtDanger = gs.Stats.Debt > gs.Stats.Gold * 5; // 负债超过资金的5倍
            
            // 有库存且余额>=0时，不需要弹窗确认
            bool hasInventory = gs.Inventory.Count > 0;
            if (hasInventory && gs.Stats.Gold >= 0)
            {
                return (false, "");
            }
            
            if (healthDanger && debtDanger)
            {
                return (true, "⚠️ 极度危险！健康值过低且负债累累，继续结束当天可能导致游戏结束！");
            }
            else if (healthDanger)
            {
                return (true, "⚠️ 健康危险！你的健康值过低，建议先治疗再结束当天。");
            }
            else if (debtDanger)
            {
                return (true, "⚠️ 负债危险！你的负债过高，建议先还债再结束当天。");
            }
            
            return (false, "");
        }

        /** 结束一天 */
        public static GameState EndDay(GameState gs)
        {
            // 根据千古风流原版玩法：健康值低于60需要治疗，否则每天下降5点
            int healthLoss = gs.Stats.Health <= 60 ? 5 : 2;
            
            // 根据千古风流原版玩法：负债利息较高，需要尽快还清
            int interest = Mathf.Max(10, Mathf.RoundToInt(gs.Stats.Debt * 0.03f));
            
            var newStats = ClampStats(new PlayerStatsImpl(gs.Stats)
            {
                Debt = gs.Stats.Debt + interest,
                Health = gs.Stats.Health - healthLoss,
                Days = gs.Stats.Days + 1
            });

            // 更新市场价格
            var newMarketPrices = new List<MarketPrice>();
            foreach (var price in gs.MarketPrices)
            {
                var commodity = Data.GetCommodity(price.CommodityId);
                if (commodity == null)
                {
                    newMarketPrices.Add(price);
                    continue;
                }

                // 价格波动
                float volatility = commodity.Volatility * 0.1f;
                float change = (Random.value - 0.5f) * 2 * volatility * price.CurrentPrice;
                int newPrice = Mathf.Max(1, Mathf.RoundToInt(price.CurrentPrice + change));

                string trend = change > 0 ? "up" : change < 0 ? "down" : "stable";

                newMarketPrices.Add(new MarketPrice
                {
                    CommodityId = price.CommodityId,
                    CityId = price.CityId,
                    CurrentPrice = newPrice,
                    Trend = trend
                });
            }

            // 检查游戏结束条件 - 健康<=0或余额<0立即结束
            if (newStats.Health <= 0 || newStats.Gold < 0 || gs.Day >= gs.MaxDays)
            {
                var (score, rank) = CalcFinalScore(newStats, gs.Inventory);
                
                // 不同的结束提示语
                string endMessage = "";
                if (newStats.Health <= 0)
                {
                    endMessage = "💀 不治而亡！你的健康值已归零，游戏结束。";
                }
                else if (newStats.Gold < 0)
                {
                    endMessage = "💸 破产！你的银两已为负数，游戏结束。";
                }
                else if (gs.Day >= gs.MaxDays)
                {
                    endMessage = "⏰ 时间到！四十天期限已满，游戏结束。";
                }
                
                return new GameState
                {
                    Phase = GamePhase.Ending,
                    Day = gs.Day + 1,
                    MaxDays = gs.MaxDays,
                    Stats = newStats,
                    CurrentCity = gs.CurrentCity,
                    Inventory = new List<InventoryItem>(gs.Inventory),
                    MarketPrices = newMarketPrices,
                    Logs = new List<DayLog>(gs.Logs),
                    CurrentEvent = null,
                    DayResult = null,
                    FinalScore = score,
                    FinalRank = rank,
                    EndMessage = endMessage
                };
            }

            // 触发随机事件
            var eventGs = TriggerRandomEvent(new GameState
            {
                Phase = GamePhase.Playing,
                Day = gs.Day + 1,
                MaxDays = gs.MaxDays,
                Stats = newStats,
                CurrentCity = gs.CurrentCity,
                Inventory = new List<InventoryItem>(gs.Inventory),
                MarketPrices = newMarketPrices,
                Logs = new List<DayLog>(gs.Logs),
                CurrentEvent = null,
                DayResult = null,
                FinalScore = 0,
                FinalRank = ""
            });

            return eventGs;
        }

        /** 最终评分 */
        private static (int score, string rank) CalcFinalScore(PlayerStats stats, List<InventoryItem> inventory)
        {
            // 计算库存价值
            int inventoryValue = 0;
            foreach (var item in inventory)
            {
                var commodity = Data.GetCommodity(item.CommodityId);
                if (commodity != null)
                {
                    int avgPrice = Mathf.RoundToInt(commodity.BasePrice * 1.5f); // 按1.5倍基础价格估算
                    inventoryValue += avgPrice * item.Quantity;
                }
            }

            int netWorth = stats.Gold + inventoryValue - stats.Debt;
            
            int score = Mathf.RoundToInt(
                netWorth * 0.1f + // 净资产
                stats.Health * 2 + // 健康
                (100 - Mathf.Min(stats.Debt, 100)) * 5 // 负债情况
            );

            string rank;
            if (score >= 1000)
                rank = "🏆 商业巨贾 · 金面具";
            else if (score >= 800)
                rank = "🥇 富甲一方 · 银面具";
            else if (score >= 600)
                rank = "🥈 小有成就 · 铜面具";
            else if (score >= 400)
                rank = "🥉 平凡商人 · 木面具";
            else if (score >= 200)
                rank = "😅 勉强糊口 · 草面具";
            else
                rank = "💀 破产逃亡 · 无面具";

            return (score, rank);
        }
    }
}
