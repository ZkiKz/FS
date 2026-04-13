using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FuSheng
{
    public static class Data
    {
        public static List<Commodity> COMMODITIES = new List<Commodity>
        {
            new Commodity
            {
                Id = "candy",
                Name = "冰糖葫芦",
                Icon = "🍡",
                BasePrice = 20,
                SortID = 1,
                Volatility = 0.3f,
                IsSpecial = false,
                SpawnProbability = 0.8f  // 常见商品，高概率
            },
            new Commodity
            {
                Id = "rouge",
                Name = "胭脂",
                Icon = "💄",
                BasePrice = 150,
                SortID = 2,
                Volatility = 0.4f,
                IsSpecial = false,
                SpawnProbability = 0.6f  // 较常见商品
            },
            new Commodity
            {
                Id = "tea",
                Name = "铁观音",
                Icon = "🍵",
                BasePrice = 180,
                SortID = 3,
                Volatility = 0.5f,
                IsSpecial = true,  // 特色商品
                SpawnProbability = 0.4f  // 特色商品，中等概率
            },
            new Commodity
            {
                Id = "cup",
                Name = "琉璃盏",
                Icon = "🥂",
                BasePrice = 950,
                SortID = 4,
                Volatility = 0.7f,
                IsSpecial = true,  // 特色商品
                SpawnProbability = 0.3f  // 特色商品，中等概率
            },
            new Commodity
            {
                Id = "fur",
                Name = "狐裘",
                Icon = "🧥",
                BasePrice = 350,
                SortID = 5,
                Volatility = 0.6f,
                IsSpecial = false,
                SpawnProbability = 0.5f  // 普通商品
            },
            new Commodity
            {
                Id = "wine",
                Name = "杜康",
                Icon = "🍷",
                BasePrice = 1250,
                SortID = 6,
                Volatility = 0.8f,
                IsSpecial = true,  // 特色商品
                SpawnProbability = 0.3f  // 特色商品，中等概率
            },
            new Commodity
            {
                Id = "painting",
                Name = "洛神图",
                Icon = "🖼️",
                BasePrice = 7500,
                SortID = 7,
                Volatility = 0.9f,
                IsSpecial = false,
                SpawnProbability = 0.2f  // 稀有商品，低概率
            },
            new Commodity
            {
                Id = "mechanism",
                Name = "木牛流马",
                Icon = "🤖",
                BasePrice = 18500,
                SortID = 8,
                Volatility = 1.0f,
                IsSpecial = true,  // 特色商品
                SpawnProbability = 0.1f  // 稀有特色商品，很低概率
            }
        };

        public static List<City> CITIES = new List<City>
        {
            new City
            {
                Id = "yanjing",
                Name = "燕京城",
                Icon = "🏯",
                Desc = "北方重镇，商贸繁华",
                Commodities = new List<string> { "candy", "fur", "wine", "painting" },
                PriceMultiplier = 1.0f
            },
            new City
            {
                Id = "zijin",
                Name = "紫禁城",
                Icon = "🏛️",
                Desc = "皇宫禁地，权贵云集",
                Commodities = new List<string> { "rouge", "cup", "painting", "mechanism" },
                PriceMultiplier = 1.3f
            },
            new City
            {
                Id = "changan",
                Name = "长安城",
                Icon = "🏰",
                Desc = "盛世古都，文化中心",
                Commodities = new List<string> { "tea", "cup", "wine", "painting" },
                PriceMultiplier = 1.1f
            },
            new City
            {
                Id = "luoyang",
                Name = "洛阳城",
                Icon = "🏺",
                Desc = "九朝古都，牡丹花城",
                Commodities = new List<string> { "wine", "tea", "rouge", "candy" },
                PriceMultiplier = 1.0f
            },
            new City
            {
                Id = "xuzhou",
                Name = "徐州城",
                Icon = "⚔️",
                Desc = "兵家必争，军事要地",
                Commodities = new List<string> { "fur", "wine", "tea", "mechanism" },
                PriceMultiplier = 0.9f
            },
            new City
            {
                Id = "suzhou",
                Name = "姑苏城",
                Icon = "🏘️",
                Desc = "江南水乡，丝绸之乡",
                Commodities = new List<string> { "painting", "rouge", "tea", "candy" },
                PriceMultiplier = 0.95f
            },
            new City
            {
                Id = "hangzhou",
                Name = "杭州城",
                Icon = "🌸",
                Desc = "人间天堂，茶叶名产",
                Commodities = new List<string> { "tea", "painting", "cup", "rouge" },
                PriceMultiplier = 1.0f
            },
            new City
            {
                Id = "linan",
                Name = "临安城",
                Icon = "🏮",
                Desc = "南宋都城，商品齐全",
                Commodities = new List<string> { "candy", "rouge", "tea", "fur", "cup", "wine", "painting", "mechanism" },
                PriceMultiplier = 1.15f
            },
            new City
            {
                Id = "yizhou",
                Name = "益州城",
                Icon = "🐼",
                Desc = "天府之国，物产丰富",
                Commodities = new List<string> { "mechanism", "tea", "wine", "fur" },
                PriceMultiplier = 0.85f
            },
            new City
            {
                Id = "yanmen",
                Name = "雁门关",
                Icon = "🏔️",
                Desc = "边塞重镇，皮毛交易",
                Commodities = new List<string> { "fur", "wine", "candy", "mechanism" },
                PriceMultiplier = 0.8f
            }
        };

        public static Commodity GetCommodity(string id)
        {
            return COMMODITIES.Find(c => c.Id == id);
        }

        public static City GetCity(string id)
        {
            return CITIES.Find(c => c.Id == id);
        }

        public static int GetCommodityPrice(string commodityId, string cityId, int day)
        {
            var commodity = GetCommodity(commodityId);
            var city = GetCity(cityId);

            if (commodity == null || city == null) return 0;

            // 基础价格 + 城市乘数 + 随机波动
            float basePrice = commodity.BasePrice * city.PriceMultiplier;
            float volatility = commodity.Volatility * 0.5f; // 降低波动幅度
            float randomFactor = 0.8f + UnityEngine.Random.value * 0.4f; // 0.8-1.2
            float dayFactor = 1 + (day * 0.01f); // 随时间轻微上涨

            return Mathf.RoundToInt(basePrice * randomFactor * dayFactor);
        }

        // 双重权重商品生成算法（Commodities作为特色商品）
        public static List<Commodity> GenerateCommoditiesByDoubleWeight(string cityId, int minCount = 2, int maxCount = 6)
        {
            var result = new List<Commodity>();
            var city = GetCity(cityId);
            if (city == null) return result;

            // 获取该城市可交易的商品（同时作为特色商品）
            var availableCommodities = city.Commodities
                .Select(id => GetCommodity(id))
                .Where(c => c != null)
                .ToList();

            if (availableCommodities.Count == 0) return result;

            // 计算每个商品的双重权重
            var weightedCommodities = new List<(Commodity Commodity, float Weight)>();
            foreach (var commodity in availableCommodities)
            {
                // 第一重权重：基础概率（SpawnProbability）
                float baseWeight = commodity.SpawnProbability;
                
                // 第二重权重：城市特色商品额外权重
                // 所有在Commodities中的商品都视为特色商品
                float specialWeight = 3.0f;
                
                // 双重权重组合
                float finalWeight = baseWeight * specialWeight;
                weightedCommodities.Add((commodity, finalWeight));
            }

            // 按权重降序排序
            weightedCommodities.Sort((a, b) => b.Weight.CompareTo(a.Weight));

            // 确保至少生成minCount个商品，优先选择权重高的
            int generatedCount = 0;
            for (int i = 0; i < weightedCommodities.Count && generatedCount < maxCount; i++)
            {
                var (commodity, weight) = weightedCommodities[i];
                
                // 对于前minCount个高权重商品，有较高概率生成
                float threshold = generatedCount < minCount ? 0.7f : weight;
                
                if (UnityEngine.Random.value <= threshold)
                {
                    result.Add(commodity);
                    generatedCount++;
                }
                // 如果还没达到最小数量，强制生成
                else if (generatedCount < minCount && i < minCount)
                {
                    result.Add(commodity);
                    generatedCount++;
                }
            }

            // 兜底逻辑：如果没有生成足够的商品，补足到最小数量
            if (result.Count < minCount && availableCommodities.Count > 0)
            {
                // 从权重最高的商品中补足
                for (int i = 0; i < weightedCommodities.Count && result.Count < minCount; i++)
                {
                    var commodity = weightedCommodities[i].Commodity;
                    if (!result.Contains(commodity))
                    {
                        result.Add(commodity);
                    }
                }
            }

            return result;
        }
    }
}
