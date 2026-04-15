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
                SpawnProbability = 0.7f  // 常见商品，高概率
            },
            new Commodity
            {
                Id = "rouge",
                Name = "胭脂",
                Icon = "💄",
                BasePrice = 150,
                SortID = 2,
                Volatility = 0.4f,
                SpawnProbability = 0.6f  // 较常见商品
            },
            new Commodity
            {
                Id = "tea",
                Name = "铁观音",
                Icon = "🍵",
                BasePrice = 200,
                SortID = 3,
                Volatility = 0.5f,
                SpawnProbability = 0.5f  // 特色商品，中等概率
            },
            new Commodity
            {
                Id = "fur",
                Name = "狐裘",
                Icon = "🧥",
                BasePrice = 350,
                SortID = 4,
                Volatility = 0.6f,
                SpawnProbability = 0.5f  // 普通商品
            },
            new Commodity
            {
                Id = "cup",
                Name = "琉璃盏",
                Icon = "🥂",
                BasePrice = 950,
                SortID = 5,
                Volatility = 0.7f,
                SpawnProbability = 0.4f  // 特色商品，中等概率
            },          
            new Commodity
            {
                Id = "wine",
                Name = "杜康",
                Icon = "🍷",
                BasePrice = 1250,
                SortID = 6,
                Volatility = 0.8f,
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
                PriceMultiplier = 1.0f,
                Index = 0
            },
            new City
            {
                Id = "zijin",
                Name = "紫禁城",
                Icon = "🏛️",
                Desc = "皇宫禁地，权贵云集",
                Commodities = new List<string> { "rouge", "cup", "painting", "mechanism" },
                PriceMultiplier = 1.3f,
                Index = 1
            },
            new City
            {
                Id = "changan",
                Name = "长安城",
                Icon = "🏰",
                Desc = "盛世古都，文化中心",
                Commodities = new List<string> { "tea", "cup", "wine", "painting" },
                PriceMultiplier = 1.1f,
                Index = 2
            },
            new City
            {
                Id = "luoyang",
                Name = "洛阳城",
                Icon = "🏺",
                Desc = "九朝古都，牡丹花城",
                Commodities = new List<string> { "wine", "tea", "rouge", "candy" },
                PriceMultiplier = 1.0f,
                Index = 3
            },
            new City
            {
                Id = "xuzhou",
                Name = "徐州城",
                Icon = "⚔️",
                Desc = "兵家必争，军事要地",
                Commodities = new List<string> { "fur", "wine", "tea", "mechanism" },
                PriceMultiplier = 0.9f,
                Index = 4
            },
            new City
            {
                Id = "suzhou",
                Name = "姑苏城",
                Icon = "🏘️",
                Desc = "江南水乡，丝绸之乡",
                Commodities = new List<string> { "painting", "rouge", "tea", "candy" },
                PriceMultiplier = 0.95f,
                Index = 5
            },
            new City
            {
                Id = "hangzhou",
                Name = "杭州城",
                Icon = "🌸",
                Desc = "人间天堂，茶叶名产",
                Commodities = new List<string> { "tea", "painting", "cup", "rouge" },
                PriceMultiplier = 1.0f,
                Index = 6
            },
            new City
            {
                Id = "linan",
                Name = "临安城",
                Icon = "🏮",
                Desc = "南宋都城，商品齐全",
                Commodities = new List<string> { "candy", "rouge", "tea", "fur", "cup", "wine", "painting", "mechanism" },
                PriceMultiplier = 1.15f,
                Index = 7
            },
            new City
            {
                Id = "yizhou",
                Name = "益州城",
                Icon = "🐼",
                Desc = "天府之国，物产丰富",
                Commodities = new List<string> { "mechanism", "tea", "wine", "fur" },
                PriceMultiplier = 0.85f,
                Index = 8
            },
            new City
            {
                Id = "yanmen",
                Name = "雁门关",
                Icon = "🏔️",
                Desc = "边塞重镇，皮毛交易",
                Commodities = new List<string> { "fur", "wine", "candy", "mechanism" },
                PriceMultiplier = 0.8f,
                Index = 9
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
            float randomFactor = 0.6f + UnityEngine.Random.value * 1f; // 0.6-1.6
            float dayFactor = 1 + (day * 0.01f); // 随时间轻微上涨

            return Mathf.RoundToInt(basePrice * randomFactor * dayFactor);
        }

        // 所有商品概率生成算法：特色商品有80%概率加成
        public static List<Commodity> GenerateCommoditiesByDoubleWeight(string cityId)
        {
            var result = new List<Commodity>();
            var city = GetCity(cityId);
            if (city == null) return result;

            // 获取该城市的特色商品列表
            var specialCommodityIds = city.Commodities;
            
            // 遍历所有商品，为每个商品计算生成概率
            foreach (var commodity in COMMODITIES)
            {
                // 基础概率：商品自带概率
                float baseProbability = commodity.SpawnProbability;
                
                // 如果是特色商品，添加80%概率加成
                if (specialCommodityIds.Contains(commodity.Id))
                {
                    // 特色商品概率 = 基础概率 + 60%
                    float finalProbability = baseProbability + 0.6f;
                    
                    // 确保概率不超过100%
                    finalProbability = Mathf.Min(finalProbability, 1.0f);
                    
                    // 根据最终概率决定是否生成该商品
                    if (UnityEngine.Random.value <= finalProbability)
                    {
                        result.Add(commodity);
                    }
                }
                else
                {
                    // 非特色商品：只使用基础概率
                    if (UnityEngine.Random.value <= baseProbability)
                    {
                        result.Add(commodity);
                    }
                }
            }

            return result;
        }
    }
}
