using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FuSheng
{
    public static class Engine
    {
        /** 初始游戏状态 */
        public static GameState CreateGame()
        {
            var initialStats = new PlayerStatsImpl(100, 1000, 500, 0, 100, 1);

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
                Mathf.Max(0, s.Deposit),
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

            // 移动到不同城池：消耗健康并增加天数，同时结算负债/存款利息
            int debtInterest = Mathf.RoundToInt(gs.Stats.Debt * 0.10f);
            int depositInterest = Mathf.RoundToInt(gs.Stats.Deposit * 0.01f);
            var newStats = ClampStats(new PlayerStatsImpl(gs.Stats)
            {
                Health = gs.Stats.Health - 2,
                Days = gs.Stats.Days + 1,
                Debt = gs.Stats.Debt + debtInterest,
                Deposit = gs.Stats.Deposit + depositInterest
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

            // 更新库存 - 每次购买都创建新记录，不合并
            var newInventory = new List<InventoryItem>(gs.Inventory);
            
            // 添加新的购买记录
            newInventory.Add(new InventoryItem
            {
                CommodityId = commodityId,
                Quantity = quantity,
                PurchasePrice = currentPrice,
                PurchaseDay = gs.Day
            });

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
            // 获取所有该商品的库存记录
            var commodityItems = gs.Inventory.Where(item => item.CommodityId == commodityId).ToList();
            
            // 计算总数量
            int totalQuantity = commodityItems.Sum(item => item.Quantity);
            
            // 检查库存是否足够
            if (totalQuantity < quantity)
                return gs;
            
            // 检查是否有非当天购买的商品
            var availableItems = commodityItems.Where(item => item.PurchaseDay != gs.Day).ToList();
            int availableQuantity = availableItems.Sum(item => item.Quantity);
            if (availableQuantity < quantity)
                return gs; // 可出售的数量不足

            var commodity = Data.GetCommodity(commodityId);
            var currentPrice = gs.MarketPrices.Find(p => 
                p.CommodityId == commodityId && p.CityId == cityId
            )?.CurrentPrice ?? 0;

            if (commodity == null || currentPrice <= 0)
                return gs;

            // 按购买日期排序，优先出售最早购买的（FIFO）
            var sortedItems = availableItems.OrderBy(item => item.PurchaseDay).ThenBy(item => item.PurchasePrice).ToList();
            
            // 计算总收入和更新库存
            int remainingToSell = quantity;
            int totalRevenue = 0;
            var newInventory = new List<InventoryItem>();
            
            // 先处理需要出售的商品
            foreach (var item in sortedItems)
            {
                if (remainingToSell <= 0)
                {
                    // 已经满足出售数量，保留剩余商品
                    newInventory.Add(item);
                    continue;
                }
                
                if (item.Quantity <= remainingToSell)
                {
                    // 全部出售这个批次
                    totalRevenue += currentPrice * item.Quantity;
                    remainingToSell -= item.Quantity;
                    // 不添加到新库存中（已出售）
                }
                else
                {
                    // 部分出售这个批次
                    totalRevenue += currentPrice * remainingToSell;
                    newInventory.Add(new InventoryItem
                    {
                        CommodityId = item.CommodityId,
                        Quantity = item.Quantity - remainingToSell,
                        PurchasePrice = item.PurchasePrice,
                        PurchaseDay = item.PurchaseDay
                    });
                    remainingToSell = 0;
                }
            }
            
            // 添加其他种类的商品
            foreach (var item in gs.Inventory)
            {
                if (item.CommodityId != commodityId)
                {
                    newInventory.Add(item);
                }
            }

            // 更新资金
            var newStats = ClampStats(new PlayerStatsImpl(gs.Stats)
            {
                Gold = gs.Stats.Gold + totalRevenue
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
            else if (operation == "deposit")
            {
                // 存款
                if (gs.Stats.Gold < amount)
                    return gs;
                
                var newStats = ClampStats(new PlayerStatsImpl(gs.Stats)
                {
                    Gold = gs.Stats.Gold - amount,
                    Deposit = gs.Stats.Deposit + amount
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
            else if (operation == "withdraw")
            {
                // 取款
                if (gs.Stats.Deposit < amount)
                    return gs;
                
                var newStats = ClampStats(new PlayerStatsImpl(gs.Stats)
                {
                    Gold = gs.Stats.Gold + amount,
                    Deposit = gs.Stats.Deposit - amount
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

        /** 获取治疗价格（每10点健康） */
        public static int GetTreatmentCost(int day)
        {
            if (day <= 10)
                return 100;
            else if (day <= 20)
                return 500;
            else if (day <= 30)
                return 1000;
            else
                return 10000;
        }

        /** 计算治疗花费 */
        public static int CalculateTreatmentCost(int day, int healthToRecover)
        {
            int costPer10 = GetTreatmentCost(day);
            return (healthToRecover + 9) / 10 * costPer10; // 向上取整到10的倍数
        }

        /** 医院治疗 - 治疗一次（恢复10点健康） */
        public static GameState HospitalTreatmentOnce(GameState gs)
        {
            int cost = GetTreatmentCost(gs.Day);
            
            if (gs.Stats.Gold < cost)
                return gs;

            if (gs.Stats.Health >= 100)
                return gs; // 健康已满

            var newStats = ClampStats(new PlayerStatsImpl(gs.Stats)
            {
                Gold = gs.Stats.Gold - cost,
                Health = Mathf.Min(100, gs.Stats.Health + 10)
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
                FinalScore = gs.FinalScore,
                FinalRank = gs.FinalRank,
                EndMessage = gs.EndMessage
            };
        }

        /** 医院治疗 - 治疗全部（恢复到100健康） */
        public static GameState HospitalTreatmentAll(GameState gs)
        {
            if (gs.Stats.Health >= 100)
                return gs; // 健康已满

            int healthToRecover = 100 - gs.Stats.Health;
            int totalCost = CalculateTreatmentCost(gs.Day, healthToRecover);

            if (gs.Stats.Gold < totalCost)
            {
                // 钱不够，治疗能治疗的部分
                int costPer10 = GetTreatmentCost(gs.Day);
                int affordableUnits = gs.Stats.Gold / costPer10;
                if (affordableUnits == 0)
                    return gs; // 钱不够治疗一次

                healthToRecover = affordableUnits * 10;
                totalCost = affordableUnits * costPer10;
            }

            var newStats = ClampStats(new PlayerStatsImpl(gs.Stats)
            {
                Gold = gs.Stats.Gold - totalCost,
                Health = Mathf.Min(100, gs.Stats.Health + healthToRecover)
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

            if (gs.CurrentEvent.Type != EventType.Choice)
                return gs;

            if (choiceIndex < 0 || choiceIndex >= gs.CurrentEvent.Choices.Count)
                return gs;

            var choice = gs.CurrentEvent.Choices[choiceIndex];

            var newStats = ApplyEventEffects(gs.Stats, choice.Effects, choice.PercentEffects);
            var effectText = BuildEffectText(choice.Effects, choice.PercentEffects);

            return CreateResolvedEventState(gs, newStats, choice.ResultText, effectText);
        }

        /** 处理输入型事件 */
        public static GameState ResolveInputEvent(GameState gs, string answerText)
        {
            if (gs.CurrentEvent == null)
                return gs;

            if (gs.CurrentEvent.Type != EventType.Input)
                return gs;

            bool isCorrect = int.TryParse(answerText, out int answer)
                && Mathf.Abs(answer - gs.CurrentEvent.CorrectAnswer) <= gs.CurrentEvent.AnswerTolerance;

            var newStats = ApplyEventEffects(
                gs.Stats,
                isCorrect ? gs.CurrentEvent.CorrectEffects : gs.CurrentEvent.WrongEffects,
                isCorrect ? gs.CurrentEvent.CorrectPercentEffects : gs.CurrentEvent.WrongPercentEffects);
            var effectText = BuildEffectText(
                isCorrect ? gs.CurrentEvent.CorrectEffects : gs.CurrentEvent.WrongEffects,
                isCorrect ? gs.CurrentEvent.CorrectPercentEffects : gs.CurrentEvent.WrongPercentEffects);

            var resultText = isCorrect
                ? gs.CurrentEvent.CorrectResultText
                : $"{gs.CurrentEvent.WrongResultText}\n正确答案是 {gs.CurrentEvent.CorrectAnswer}。";

            return CreateResolvedEventState(gs, newStats, resultText, effectText);
        }

        private static PlayerStatsImpl ApplyEventEffects(
            PlayerStats sourceStats,
            Dictionary<string, int> fixedEffects,
            Dictionary<string, float> percentEffects)
        {
            var newStats = new PlayerStatsImpl(sourceStats);

            if (fixedEffects != null)
            {
                foreach (var effect in fixedEffects)
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
                        case "Deposit":
                            newStats.Deposit += effect.Value;
                            break;
                        case "Days":
                            newStats.Days += effect.Value;
                            break;
                    }
                }
            }

            if (percentEffects != null)
            {
                foreach (var effect in percentEffects)
                {
                    switch (effect.Key)
                    {
                        case "Gold":
                            newStats.Gold += Mathf.RoundToInt(sourceStats.Gold * effect.Value);
                            break;
                        case "Debt":
                            newStats.Debt += Mathf.RoundToInt(sourceStats.Debt * effect.Value);
                            break;
                        case "Deposit":
                            newStats.Deposit += Mathf.RoundToInt(sourceStats.Deposit * effect.Value);
                            break;
                    }
                }
            }

            return (PlayerStatsImpl)ClampStats(newStats);
        }

        private static GameState CreateResolvedEventState(GameState gs, PlayerStatsImpl newStats, string resultText, string effectText)
        {
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
                FinalRank = "",
                LastEventResultText = resultText,
                LastEventEffectText = effectText
            };
        }

        private static string BuildEffectText(Dictionary<string, int> fixedEffects, Dictionary<string, float> percentEffects)
        {
            var parts = new List<string>();

            if (percentEffects != null)
            {
                foreach (var effect in percentEffects)
                {
                    string label = GetEffectLabel(effect.Key);
                    string sign = effect.Value >= 0 ? "+" : "-";
                    int percent = Mathf.RoundToInt(Mathf.Abs(effect.Value) * 100f);
                    parts.Add($"{label} {sign} {percent}%");
                }
            }

            if (fixedEffects != null)
            {
                foreach (var effect in fixedEffects)
                {
                    string label = GetEffectLabel(effect.Key);
                    string sign = effect.Value >= 0 ? "+" : "-";
                    int value = Mathf.Abs(effect.Value);
                    string unit = effect.Key == "Health" ? "点" : "";
                    parts.Add($"{label} {sign} {value}{unit}");
                }
            }

            if (parts.Count == 0)
                return "无属性变化";

            return string.Join("，", parts);
        }

        private static string GetEffectLabel(string key)
        {
            switch (key)
            {
                case "Gold":
                    return "银两";
                case "Debt":
                    return "负债";
                case "Deposit":
                    return "存款";
                case "Health":
                    return "健康";
                case "Days":
                    return "天数";
                default:
                    return key;
            }
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
            
            // 负债利息：每天10%，每日结算累加
            int debtInterest = Mathf.RoundToInt(gs.Stats.Debt * 0.10f);
            
            // 存款利息：每天1%
            int depositInterest = Mathf.RoundToInt(gs.Stats.Deposit * 0.01f);
            
            var newStats = ClampStats(new PlayerStatsImpl(gs.Stats)
            {
                Debt = gs.Stats.Debt + debtInterest,
                Deposit = gs.Stats.Deposit + depositInterest,
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

            // 检查游戏结束条件 - 健康<=0或余额<0或天数>=40立即结束
            if (newStats.Health <= 0 || newStats.Gold < 0 || newStats.Days >= gs.MaxDays)
            {
                var (score, rank) = CalcFinalScore(newStats, gs.Inventory);
                
                // 不同的结束提示语
                string endMessage = "";
                if (newStats.Health <= 0)
                {
                    endMessage = "不治而亡！你的健康值已归零，游戏结束。";
                }
                else if (newStats.Gold < 0)
                {
                    endMessage = "破产！你的银两已为负数，游戏结束。";
                }
                else if (newStats.Days >= gs.MaxDays)
                {
                    endMessage = "时间到！四十天期限已满，游戏结束。";
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

        /** 计算扩容价格 */
        public static int CalculateExpansionCost(int expansionCount)
        {
            // 固定价格25000
            return 25000;
        }

        /** 检查是否可以扩容 */
        public static bool CanExpand(GameState gs, int quantity)
        {
            int totalCost = quantity * CalculateExpansionCost(gs.Stats.ExpansionCount);
            return gs.Stats.Gold >= totalCost;
        }

        /** 执行扩容操作 */
        public static GameState ExpandCapacity(GameState gs, int quantity)
        {
            if (quantity <= 0)
                return gs;

            // 计算总花费（固定价格，无折扣）
            int totalCost = quantity * CalculateExpansionCost(gs.Stats.ExpansionCount);

            // 检查资金是否足够
            if (gs.Stats.Gold < totalCost)
                return gs;

            // 更新状态
            var newStats = new PlayerStatsImpl(gs.Stats)
            {
                Gold = gs.Stats.Gold - totalCost,
                Capacity = gs.Stats.Capacity + (quantity * 10),
                ExpansionCount = gs.Stats.ExpansionCount + quantity
            };

            newStats = (PlayerStatsImpl)ClampStats(newStats);

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
                FinalScore = gs.FinalScore,
                FinalRank = gs.FinalRank,
                EndMessage = gs.EndMessage
            };
        }
    }
}
