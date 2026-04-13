using System.Collections.Generic;
using UnityEngine;

namespace FuSheng
{
    public static class Events
    {
        public static Dictionary<string, GameEvent> EVENTS = new Dictionary<string, GameEvent>
        {
            // ── 市场机遇事件 (40% 正面, 30% 中性, 30% 负面) ──
            { "e_price_boom", new GameEvent
                {
                    Id = "e_price_boom",
                    Title = "物价飞涨",
                    Icon = "📈",
                    Description = "由于商路受阻，某些商品价格大幅上涨！",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Text = "继续观望",
                            Effects = new Dictionary<string, int> { { "Gold", 0 } },
                            ResultText = "你选择继续观望市场变化。"
                        },
                        new EventChoice
                        {
                            Text = "趁机抛售",
                            Effects = new Dictionary<string, int> { { "Gold", 150 } },
                            ResultText = "你趁机抛售了部分库存，赚了一笔！"
                        }
                    }
                }
            },
            { "e_price_crash", new GameEvent
                {
                    Id = "e_price_crash",
                    Title = "市场崩盘",
                    Icon = "📉",
                    Description = "大量商品涌入市场，价格暴跌！",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Text = "低价抄底",
                            Effects = new Dictionary<string, int> { { "Gold", -80 } },
                            ResultText = "你低价抄底了一些商品，等待价格回升。"
                        },
                        new EventChoice
                        {
                            Text = "止损离场",
                            Effects = new Dictionary<string, int> { { "Gold", -30 } },
                            ResultText = "你及时止损，减少了损失。"
                        }
                    }
                }
            },
            { "e_big_order", new GameEvent
                {
                    Id = "e_big_order",
                    Title = "天降大单",
                    Icon = "📦",
                    Description = "一位富商急需大量货物，愿出高价收购！",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Text = "全力供货",
                            Effects = new Dictionary<string, int> { { "Gold", 250 } },
                            ResultText = "你全力供货，赚了一大笔！"
                        },
                        new EventChoice
                        {
                            Text = "谨慎合作",
                            Effects = new Dictionary<string, int> { { "Gold", 120 } },
                            ResultText = "你谨慎合作，获得了稳定收益。"
                        }
                    }
                }
            },
            { "e_market_opportunity", new GameEvent
                {
                    Id = "e_market_opportunity",
                    Title = "商机涌现",
                    Icon = "💡",
                    Description = "市场出现新的商机，有商品供不应求！",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Text = "大胆投资",
                            Effects = new Dictionary<string, int> { { "Gold", 180 } },
                            ResultText = "你大胆投资，获得了丰厚回报！"
                        },
                        new EventChoice
                        {
                            Text = "稳健经营",
                            Effects = new Dictionary<string, int> { { "Gold", 80 } },
                            ResultText = "你稳健经营，获得了稳定收益。"
                        }
                    }
                }
            },
            
            // ── 旅途冒险事件 (40% 正面, 30% 中性, 30% 负面) ──
            { "e_robber", new GameEvent
                {
                    Id = "e_robber",
                    Title = "路遇劫匪",
                    Icon = "🗡️",
                    Description = "一群蒙面劫匪拦住去路，索要买路钱！",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Text = "乖乖交钱",
                            Effects = new Dictionary<string, int> { { "Gold", -60 } },
                            ResultText = "你交了买路钱，安全通过。"
                        },
                        new EventChoice
                        {
                            Text = "奋力反抗",
                            Effects = new Dictionary<string, int> { { "Gold", -30 }, { "Health", -15 } },
                            ResultText = "你奋力反抗，受了些伤但保住了部分钱财。"
                        }
                    }
                }
            },
            { "e_bandit_raid", new GameEvent
                {
                    Id = "e_bandit_raid",
                    Title = "山贼洗劫",
                    Icon = "⚔️",
                    Description = "山贼趁夜洗劫了你的货物！",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Text = "报官追查",
                            Effects = new Dictionary<string, int> { { "Gold", -90 } },
                            ResultText = "你报官追查，追回部分损失。"
                        },
                        new EventChoice
                        {
                            Text = "自认倒霉",
                            Effects = new Dictionary<string, int> { { "Gold", -120 } },
                            ResultText = "你自认倒霉，损失了全部货物。"
                        }
                    }
                }
            },
            { "e_travel_merchant", new GameEvent
                {
                    Id = "e_travel_merchant",
                    Title = "偶遇行商",
                    Icon = "🚶‍♂️",
                    Description = "路上遇到一位行商，愿意与你交易！",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Text = "热情交易",
                            Effects = new Dictionary<string, int> { { "Gold", 100 } },
                            ResultText = "你与行商热情交易，获得了优惠价格！"
                        },
                        new EventChoice
                        {
                            Text = "谨慎交易",
                            Effects = new Dictionary<string, int> { { "Gold", 50 } },
                            ResultText = "你谨慎交易，获得了合理收益。"
                        }
                    }
                }
            },
            { "e_ancient_temple", new GameEvent
                {
                    Id = "e_ancient_temple",
                    Title = "古庙奇遇",
                    Icon = "🏯",
                    Description = "在荒废的古庙中发现了一些有价值的物品！",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Text = "仔细搜寻",
                            Effects = new Dictionary<string, int> { { "Gold", 200 } },
                            ResultText = "你在古庙中发现了珍贵的古董！"
                        },
                        new EventChoice
                        {
                            Text = "简单查看",
                            Effects = new Dictionary<string, int> { { "Gold", 80 } },
                            ResultText = "你简单查看，找到了一些有价值的物品。"
                        }
                    }
                }
            },
            
            // ── 人际关系事件 (50% 正面, 30% 中性, 20% 负面) ──
            { "e_inheritance", new GameEvent
                {
                    Id = "e_inheritance",
                    Title = "意外继承",
                    Icon = "💰",
                    Description = "远房亲戚去世，你继承了一笔遗产！",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Text = "接受遗产",
                            Effects = new Dictionary<string, int> { { "Gold", 400 }, { "Debt", -150 } },
                            ResultText = "你继承了遗产，还清了部分债务。"
                        },
                        new EventChoice
                        {
                            Text = "捐赠慈善",
                            Effects = new Dictionary<string, int> { { "Gold", 150 } },
                            ResultText = "你将部分遗产捐赠给慈善机构。"
                        }
                    }
                }
            },
            { "e_noble_patron", new GameEvent
                {
                    Id = "e_noble_patron",
                    Title = "贵人相助",
                    Icon = "👑",
                    Description = "一位贵族看中你的经商才能，愿意资助你！",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Text = "接受资助",
                            Effects = new Dictionary<string, int> { { "Gold", 300 }, { "Debt", -100 } },
                            ResultText = "你接受了贵族的资助，生意蒸蒸日上！"
                        },
                        new EventChoice
                        {
                            Text = "婉言谢绝",
                            Effects = new Dictionary<string, int> { { "Gold", 100 } },
                            ResultText = "你婉言谢绝，但贵族还是给了你一些帮助。"
                        }
                    }
                }
            },
            { "e_business_partner", new GameEvent
                {
                    Id = "e_business_partner",
                    Title = "商业伙伴",
                    Icon = "🤝",
                    Description = "一位商人想与你合作做生意！",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Text = "深度合作",
                            Effects = new Dictionary<string, int> { { "Gold", 180 } },
                            ResultText = "你与商人深度合作，生意越做越大！"
                        },
                        new EventChoice
                        {
                            Text = "简单合作",
                            Effects = new Dictionary<string, int> { { "Gold", 80 } },
                            ResultText = "你与商人简单合作，获得了稳定收益。"
                        }
                    }
                }
            },
            
            // ── 健康养生事件 (60% 正面, 30% 中性, 10% 负面) ──
            { "e_illness", new GameEvent
                {
                    Id = "e_illness",
                    Title = "突发疾病",
                    Icon = "🤒",
                    Description = "长途跋涉让你染上了风寒！",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Text = "立即就医",
                            Effects = new Dictionary<string, int> { { "Health", 25 }, { "Gold", -35 } },
                            ResultText = "你及时就医，恢复了健康。"
                        },
                        new EventChoice
                        {
                            Text = "硬撑过去",
                            Effects = new Dictionary<string, int> { { "Health", -35 } },
                            ResultText = "你硬撑过去，病情加重了。"
                        }
                    }
                }
            },
            { "e_miracle", new GameEvent
                {
                    Id = "e_miracle",
                    Title = "神医妙手",
                    Icon = "💊",
                    Description = "遇到一位神医，免费为你治疗！",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Text = "接受治疗",
                            Effects = new Dictionary<string, int> { { "Health", 45 } },
                            ResultText = "神医妙手回春，你完全康复了！"
                        },
                        new EventChoice
                        {
                            Text = "婉言谢绝",
                            Effects = new Dictionary<string, int> { { "Health", 15 } },
                            ResultText = "你婉言谢绝，只接受了简单治疗。"
                        }
                    }
                }
            },
            { "e_herbal_garden", new GameEvent
                {
                    Id = "e_herbal_garden",
                    Title = "药园奇遇",
                    Icon = "🌿",
                    Description = "在山中发现一片野生药园！",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Text = "采摘草药",
                            Effects = new Dictionary<string, int> { { "Health", 30 } },
                            ResultText = "你采摘了珍贵草药，身体更加健康！"
                        },
                        new EventChoice
                        {
                            Text = "少量采摘",
                            Effects = new Dictionary<string, int> { { "Health", 15 } },
                            ResultText = "你少量采摘，获得了养生效果。"
                        }
                    }
                }
            },
            
            // ── 负债管理事件 (40% 正面, 40% 中性, 20% 负面) ──
            { "e_loan_shark", new GameEvent
                {
                    Id = "e_loan_shark",
                    Title = "高利贷催债",
                    Icon = "💸",
                    Description = "债主上门催债，威胁要加倍利息！",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Text = "协商还款",
                            Effects = new Dictionary<string, int> { { "Debt", 60 } },
                            ResultText = "你与债主协商，增加了部分利息。"
                        },
                        new EventChoice
                        {
                            Text = "强硬拒绝",
                            Effects = new Dictionary<string, int> { { "Debt", 120 }, { "Health", -15 } },
                            ResultText = "你强硬拒绝，债主威胁要采取行动。"
                        }
                    }
                }
            },
            { "e_debt_forgiveness", new GameEvent
                {
                    Id = "e_debt_forgiveness",
                    Title = "债务减免",
                    Icon = "📜",
                    Description = "官府颁布新政，减免部分债务！",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Text = "申请减免",
                            Effects = new Dictionary<string, int> { { "Debt", -250 } },
                            ResultText = "你成功申请到债务减免。"
                        },
                        new EventChoice
                        {
                            Text = "继续承担",
                            Effects = new Dictionary<string, int> { { "Debt", -120 } },
                            ResultText = "你选择继续承担原有债务。"
                        }
                    }
                }
            },
            { "e_financial_windfall", new GameEvent
                {
                    Id = "e_financial_windfall",
                    Title = "意外之财",
                    Icon = "🎁",
                    Description = "你意外获得了一笔意外之财！",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Text = "全部收下",
                            Effects = new Dictionary<string, int> { { "Gold", 350 }, { "Debt", -180 } },
                            ResultText = "你获得了意外之财，还清了部分债务！"
                        },
                        new EventChoice
                        {
                            Text = "部分收下",
                            Effects = new Dictionary<string, int> { { "Gold", 200 } },
                            ResultText = "你只收下部分钱财。"
                        }
                    }
                }
            },
            
            // ── 特殊机遇事件 (70% 正面, 20% 中性, 10% 风险) ──
            { "e_treasure", new GameEvent
                {
                    Id = "e_treasure",
                    Title = "发现宝藏",
                    Icon = "💎",
                    Description = "在路边发现一个被遗弃的宝箱！",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Text = "打开宝箱",
                            Effects = new Dictionary<string, int> { { "Gold", 400 } },
                            ResultText = "宝箱里装满了金银财宝！"
                        },
                        new EventChoice
                        {
                            Text = "交给官府",
                            Effects = new Dictionary<string, int> { { "Gold", 150 } },
                            ResultText = "你将宝箱交给官府，获得奖励。"
                        }
                    }
                }
            },
            { "e_lucky_strike", new GameEvent
                {
                    Id = "e_lucky_strike",
                    Title = "幸运降临",
                    Icon = "🍀",
                    Description = "你的运气特别好，事事顺利！",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Text = "把握机会",
                            Effects = new Dictionary<string, int> { { "Gold", 280 }, { "Health", 20 } },
                            ResultText = "你把握住机会，获得了双重好运！"
                        },
                        new EventChoice
                        {
                            Text = "顺其自然",
                            Effects = new Dictionary<string, int> { { "Gold", 120 } },
                            ResultText = "你顺其自然，也获得了不错的结果。"
                        }
                    }
                }
            },
            { "e_fortune_teller", new GameEvent
                {
                    Id = "e_fortune_teller",
                    Title = "算命先生",
                    Icon = "🔮",
                    Description = "遇到一位算命先生，为你指点迷津！",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Text = "诚心请教",
                            Effects = new Dictionary<string, int> { { "Gold", 180 }, { "Health", 15 } },
                            ResultText = "算命先生为你指点迷津，运势大好！"
                        },
                        new EventChoice
                        {
                            Text = "简单咨询",
                            Effects = new Dictionary<string, int> { { "Gold", 80 } },
                            ResultText = "你简单咨询，获得了一些建议。"
                        }
                    }
                }
            },
            
            // ── 新增事件类型 (平衡性调整) ──
            { "e_weather_blessing", new GameEvent
                {
                    Id = "e_weather_blessing",
                    Title = "风调雨顺",
                    Icon = "🌈",
                    Description = "今年风调雨顺，农作物丰收，商品供应充足！",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Text = "大量采购",
                            Effects = new Dictionary<string, int> { { "Gold", 120 }, { "Capacity", 5 } },
                            ResultText = "你大量采购，获得了优惠价格。"
                        },
                        new EventChoice
                        {
                            Text = "适量采购",
                            Effects = new Dictionary<string, int> { { "Gold", 60 } },
                            ResultText = "你适量采购，获得了合理价格。"
                        }
                    }
                }
            },
            { "e_festival_celebration", new GameEvent
                {
                    Id = "e_festival_celebration",
                    Title = "节日庆典",
                    Icon = "🎉",
                    Description = "城中举办盛大节日庆典，商业活动繁荣！",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Text = "积极参与",
                            Effects = new Dictionary<string, int> { { "Gold", 160 }, { "Health", 10 } },
                            ResultText = "你积极参与庆典，生意兴隆！"
                        },
                        new EventChoice
                        {
                            Text = "简单参与",
                            Effects = new Dictionary<string, int> { { "Gold", 80 } },
                            ResultText = "你简单参与，获得了一些收益。"
                        }
                    }
                }
            },
            { "e_skill_improvement", new GameEvent
                {
                    Id = "e_skill_improvement",
                    Title = "技艺精进",
                    Icon = "🎯",
                    Description = "你的经商技巧有所提升，能够更好地把握商机！",
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            Text = "专注提升",
                            Effects = new Dictionary<string, int> { { "Gold", 140 }, { "Capacity", 8 } },
                            ResultText = "你专注提升技艺，能力大幅提升。"
                        },
                        new EventChoice
                        {
                            Text = "稳步提升",
                            Effects = new Dictionary<string, int> { { "Gold", 70 } },
                            ResultText = "你稳步提升，能力有所增强。"
                        }
                    }
                }
            }
        };

        public static GameEvent GetEvent(string id)
        {
            if (EVENTS.TryGetValue(id, out var ev))
                return ev;
            return null;
        }

        public static GameEvent GetRandomEvent()
        {
            var eventKeys = new List<string>(EVENTS.Keys);
            int randomIndex = UnityEngine.Random.Range(0, eventKeys.Count);
            string randomEventKey = eventKeys[randomIndex];
            return EVENTS[randomEventKey];
        }
    }
}
