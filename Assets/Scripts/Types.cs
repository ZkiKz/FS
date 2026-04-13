using System.Collections.Generic;

namespace FuSheng
{
    public interface PlayerStats
    {
        int Health { get; set; }    // 健康 0-100
        int Gold { get; set; }      // 金银（两）
        int Debt { get; set; }      // 负债（两）
        int Capacity { get; set; }  // 负重容量
        int Days { get; set; }      // 已过天数
        int? Mood { get; set; }     // 心情
        int? Fame { get; set; }     // 声望
        int? Cultivation { get; set; } // 修为
    }

    public class PlayerStatsImpl : PlayerStats
    {
        public int Health { get; set; }
        public int Gold { get; set; }
        public int Debt { get; set; }
        public int Capacity { get; set; }
        public int Days { get; set; }
        public int? Mood { get; set; }
        public int? Fame { get; set; }
        public int? Cultivation { get; set; }

        public PlayerStatsImpl(int health, int gold, int debt, int capacity, int days)
        {
            Health = health;
            Gold = gold;
            Debt = debt;
            Capacity = capacity;
            Days = days;
        }

        public PlayerStatsImpl(PlayerStats other)
        {
            Health = other.Health;
            Gold = other.Gold;
            Debt = other.Debt;
            Capacity = other.Capacity;
            Days = other.Days;
            Mood = other.Mood;
            Fame = other.Fame;
            Cultivation = other.Cultivation;
        }
    }

    public class Commodity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public int BasePrice { get; set; }    // 基础价格
        public int SortID { get; set; }       // 排序ID，用于商品显示排序
        public float Volatility { get; set; } // 价格波动率 0-1
        public bool IsSpecial { get; set; }   // 是否为特色商品
        public float SpawnProbability { get; set; } // 出现概率 0-1
    }

    public class City
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Desc { get; set; }
        public List<string> Commodities { get; set; } // 可交易的商品ID（同时作为特色商品）
        public float PriceMultiplier { get; set; } // 价格乘数
    }

    public class InventoryItem
    {
        public string CommodityId { get; set; }
        public int Quantity { get; set; }
        public int PurchasePrice { get; set; } // 购买价格
        public int PurchaseDay { get; set; }   // 购买日期（游戏天数）
    }

    public class MarketPrice
    {
        public string CommodityId { get; set; }
        public string CityId { get; set; }
        public int CurrentPrice { get; set; }
        public string Trend { get; set; } // 价格趋势: 'up' | 'down' | 'stable'
    }

    public class GameAction
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Desc { get; set; }
        public string Type { get; set; } // 'buy' | 'sell' | 'travel' | 'bank' | 'hospital'
        public Dictionary<string, int> Cost { get; set; }
        public Dictionary<string, int> Reward { get; set; }
        public Dictionary<string, int> Requirement { get; set; }
        public List<string> RandomEvents { get; set; }
        public float? EventChance { get; set; }
    }

    public class EventChoice
    {
        public string Text { get; set; }
        public Dictionary<string, int> Effects { get; set; }
        public string ResultText { get; set; }
    }

    public class GameEvent
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public List<EventChoice> Choices { get; set; }
    }

    public class DayLog
    {
        public int Day { get; set; }
        public string City { get; set; }
        public List<string> Actions { get; set; }
        public List<string> Events { get; set; }
        public int Profit { get; set; }
        public PlayerStats StatsBefore { get; set; }
        public PlayerStats StatsAfter { get; set; }
    }

    public enum GamePhase
    {
        Intro,
        Playing,
        Event,
        DayResult,
        Ending
    }

    public class Location
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Desc { get; set; }
        public List<GameAction> Actions { get; set; }
    }

    public class GameState
    {
        public GamePhase Phase { get; set; }
        public int Day { get; set; }
        public int MaxDays { get; set; }
        public PlayerStats Stats { get; set; }
        public string CurrentCity { get; set; }
        public List<InventoryItem> Inventory { get; set; }
        public List<MarketPrice> MarketPrices { get; set; }
        public List<DayLog> Logs { get; set; }
        public GameEvent CurrentEvent { get; set; }
        public DayLog DayResult { get; set; }
        public int FinalScore { get; set; }
        public string FinalRank { get; set; }
        public string EndMessage { get; set; }

        public GameState()
        {
            Inventory = new List<InventoryItem>();
            MarketPrices = new List<MarketPrice>();
            Logs = new List<DayLog>();
        }
    }
}
