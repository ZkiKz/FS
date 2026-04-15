using System.Collections.Generic;
using UnityEngine;

namespace FuSheng
{
    public static class Events
    {
        public static Dictionary<string, GameEvent> EVENTS = new Dictionary<string, GameEvent>
        {
            {
                "e_hometown_letter",
                Choice(
                    "e_hometown_letter",
                    "村长飞鸽传信",
                    "🕊️",
                    "一别五载，村长忽然飞鸽传信：你当年借了乡里修桥银 15564 两，如今账房把旧账翻了出来。鸽子脚上还绑着一句话：‘人可以不回来，银子总得回来。’",
                    new EventChoice
                    {
                        Text = "先还些",
                        PercentEffects = EffectPercents(("Gold", -0.18f)),
                        ResultText = "你托人先送回一笔银子，村口总算没人再念你的名字。"
                    },
                    new EventChoice
                    {
                        Text = "装作不见",
                        PercentEffects = EffectPercents(("Debt", 0.12f)),
                        ResultText = "你把信塞进袖里，债没少，心口却像压了块石头。"
                    },
                    new EventChoice
                    {
                        Text = "写信缓债",
                        PercentEffects = EffectPercents(("Gold", -0.05f)),
                        ResultText = "你洋洋洒洒写了三页苦情书，村长回信只批了四个字：下不为例。"
                    })
            },
            {
                "e_yamen_donation",
                Choice(
                    "e_yamen_donation",
                    "赵大官人摇头不止",
                    "🏛️",
                    "赵大官人见你当街讲价讲得唾沫横飞，连连摇头，说你斯文扫地，非要你为衙门修门廊出一笔体面银。他拍着肚皮道：不多，按你手头银票折个一成二便好。",
                    new EventChoice
                    {
                        Text = "认捐体面",
                        PercentEffects = EffectPercents(("Gold", -0.12f)),
                        ResultText = "你咬牙认了，赵大官人这才露出满意的官样笑。"
                    },
                    new EventChoice
                    {
                        Text = "据理少捐",
                        PercentEffects = EffectPercents(("Gold", -0.08f)),
                        ResultText = "你舌战半晌，总算少掏了些，只是气得头疼。"
                    })
            },
            {
                "e_porcelain_clearance",
                Calc(
                    "e_porcelain_clearance",
                    "钱如命清仓瓷器",
                    "🏺",
                    "钱如命囤了52件瓷器，今日行情大好，货栈老板愿意按每件172两全收。他把算盘递给你，笑得像只老狐狸：快替我算算，这一趟总共进账多少？",
                    "请输入总收入",
                    52 * 172,
                    "算盘珠子噼啪作响，你一口报出答案，钱如命当场请你喝了碗热汤。",
                    "你算错了，钱如命赶紧把算盘抢了回去，嘴里嘟囔着就这还敢走商路。",
                    null,
                    EffectPercents(("Gold", 0.16f)),
                    null,
                    EffectPercents(("Gold", -0.05f)))
            },
            {
                "e_tea_tax",
                Calc(
                    "e_tea_tax",
                    "茶引补税",
                    "🍵",
                    "茶马司突然清查旧税单。你手上有36筐茶，每筐要补税18两，若一次交清可减免40两。小吏把笔塞进你手里：算明白了再缴。",
                    "请输入需要补缴的总额",
                    36 * 18 - 40,
                    "你把账算得分毫不差，小吏连连点头，甚至帮你把队伍往前挪了两位。",
                    "你一时算岔了，多排了半天队，还补了点冤枉银子。",
                    null,
                    EffectPercents(("Gold", -0.09f)),
                    null,
                    EffectPercents(("Gold", -0.15f)))
            },
            {
                "e_lucky_wedding",
                Choice(
                    "e_lucky_wedding",
                    "喜宴搭台",
                    "🎎",
                    "城东豪绅办喜事，临时缺个会说吉祥话又懂摆货摊的人。管家看你面相机灵，要么去摆席卖货，要么去后厨帮忙，要么干脆不掺和。",
                    new EventChoice
                    {
                        Text = "前堂摆摊",
                        PercentEffects = EffectPercents(("Gold", 0.14f)),
                        ResultText = "你一边说喜话一边兜售小玩意，宾客们掏钱比掏红包还利索。"
                    },
                    new EventChoice
                    {
                        Text = "后厨帮忙",
                        PercentEffects = EffectPercents(("Debt", -0.08f)),
                        ResultText = "你帮着抬了半天蒸笼，累是累了点，却结识了能说得上话的账房先生。"
                    },
                    new EventChoice
                    {
                        Text = "继续赶路",
                        Effects = EffectInts(("Gold", 0)),
                        ResultText = "你错过了热闹，也错过了顺手的财运。"
                    })
            },
            {
                "e_midnight_inn",
                Choice(
                    "e_midnight_inn",
                    "深夜客栈赌约",
                    "🏮",
                    "深夜住店，几个客商围着油灯打赌，说谁敢连喝三碗烈酒再背出今日粮价，谁就能拿走桌上的彩头。掌柜朝你挤眼，像在等你出丑。",
                    new EventChoice
                    {
                        Text = "接下赌约",
                        PercentEffects = EffectPercents(("Gold", 0.12f)),
                        ResultText = "你硬着头皮灌了酒，居然真把粮价背得八九不离十，满桌人都服了。"
                    },
                    new EventChoice
                    {
                        Text = "小赢即收",
                        PercentEffects = EffectPercents(("Gold", 0.04f)),
                        ResultText = "你浅尝辄止，顺手蹭了两碟下酒菜，还赚了点零碎彩头。"
                    },
                    new EventChoice
                    {
                        Text = "装醉回房",
                        Effects = EffectInts(("Health", 5)),
                        ResultText = "你回房睡了个安稳觉，第二天精神倒是不错。"
                    })
            },
            {
                "e_escort_shortage",
                Calc(
                    "e_escort_shortage",
                    "镖局临时抽成",
                    "🧾",
                    "镖局说近来路险，临时加抽车马费。你这趟共托运18箱货，每箱加收27两，若一次付清再给你抹掉66两零头。账房先生把算盘往你面前一推。",
                    "请输入应付车马费",
                    18 * 27 - 66,
                    "你算得又快又准，账房先生都忍不住夸你像半个先生。",
                    "你心算慢了一步，被账房先生抢先报数，还嫌你耽误了队伍。",
                    null,
                    EffectPercents(("Gold", -0.07f)),
                        null,
                    EffectPercents(("Gold", -0.12f)))
            },
            {
                "e_old_friend",
                Choice(
                    "e_old_friend",
                    "旧友拦路认亲",
                    "🤝",
                    "你刚出城门，就被一个声音喊住。原来是当年一块偷桃子的旧友，如今混成了小商会的跑腿。他非要请你喝茶，还想拉你入伙。",
                    new EventChoice
                    {
                        Text = "入伙试水",
                        PercentEffects = EffectPercents(("Gold", 0.10f)),
                        ResultText = "旧友给你介绍了条偏门生意，银子不算多，却很实在。"
                    },
                    new EventChoice
                    {
                        Text = "只叙旧情",
                        Effects = EffectInts(("Health", 6)),
                        ResultText = "你们聊到夜深，心里竟轻快了不少。"
                    })
            },
            {
                "e_temple_lamp",
                Choice(
                    "e_temple_lamp",
                    "古寺点灯",
                    "🪔",
                    "山路边的古寺募香火钱，老僧说你面带奔波相，点一盏长明灯可保商路少灾。殿外雨正下大，香客们都看着你。",
                    new EventChoice
                    {
                        Text = "捐灯祈安",
                        PercentEffects = EffectPercents(("Gold", -0.05f)),
                        ResultText = "你点了长明灯，心里像也亮堂了几分。"
                    },
                    new EventChoice
                    {
                        Text = "少捐香油",
                        PercentEffects = EffectPercents(("Gold", -0.02f)),
                        ResultText = "老僧看穿不说穿，只叹你财缘深、佛缘浅。"
                    },
                    new EventChoice
                    {
                        Text = "扭头就走",
                        Effects = EffectInts(("Health", -6)),
                        ResultText = "你冒雨离去，山风灌得人骨头都凉了。"
                    })
            },
            {
                "e_salt_ledger",
                Calc(
                    "e_salt_ledger",
                    "盐行对账",
                    "🧂",
                    "盐行少东家抱着账本追上你，说你上月代卖的盐砖共有24块，每块分成83两，其中盐行先拿走120两作保本。请你现在就把自己该得的收入算出来。",
                    "请输入你应得的收入",
                    24 * 83 - 120,
                    "你报数时连停顿都没有，少东家立刻把银票拍在你手里。",
                    "你少算了几笔，少东家把账本一合，硬是扣了你一笔辛苦钱。",
                    null,
                    EffectPercents(("Gold", 0.15f)),
                    null,
                    EffectPercents(("Gold", -0.06f)))
            },
            {
                "e_ticket_house_dividend",
                Choice(
                    "e_ticket_house_dividend",
                    "票号忽送红利",
                    "💴",
                    "你在票号存了些面子，掌柜忽然笑眯眯迎你进门，说近日拆借得利，愿按旧情分你一笔红利。只是要不要继续留在票号滚利，全看你一句话。",
                    new EventChoice
                    {
                        Text = "留存滚利",
                        PercentEffects = EffectPercents(("Deposit", 0.18f)),
                        ResultText = "你把红利又压回票号，账本上的数字看着比酒楼招牌还顺眼。"
                    },
                    new EventChoice
                    {
                        Text = "提走一半",
                        PercentEffects = EffectPercents(("Deposit", -0.10f)),
                        ResultText = "你把一半红利换成现银，袖袋一沉，心也稳了。"
                    },
                    new EventChoice
                    {
                        Text = "顺势借款",
                        PercentEffects = EffectPercents(("Gold", 0.12f)),
                        ResultText = "掌柜果然爽快借你一笔，只是笑容里明显带着利息。"
                    })
            },
            {
                "e_flood_relief",
                Choice(
                    "e_flood_relief",
                    "河堤募捐榜",
                    "🌊",
                    "城外河堤决口，官府在城门口立了募捐榜。榜首空着，旁边的书吏故意把笔往你手边推，像是认定你是个要脸面的商人。",
                    new EventChoice
                    {
                        Text = "大额捐赠",
                        PercentEffects = EffectPercents(("Gold", -0.15f)),
                        ResultText = "你把名字写在了榜首，过路百姓都多看了你两眼。"
                    },
                    new EventChoice
                    {
                        Text = "小额捐赠",
                        PercentEffects = EffectPercents(("Gold", -0.06f)),
                        ResultText = "你捐得不多不少，既不出头，也不算失礼。"
                    },
                    new EventChoice
                    {
                        Text = "借口溜走",
                        PercentEffects = EffectPercents(("Debt", 0.05f)),
                        ResultText = "你趁人不注意溜了，可背后多少还是落了几句闲话。"
                    })
            },
            {
                "e_brokerage_math",
                Calc(
                    "e_brokerage_math",
                    "牙行抽佣快算",
                    "📚",
                    "牙行替你撮合了一笔买卖，共卖出45匹绸缎，每匹收价96两，牙行抽两成佣金。牙郎翘着腿问你：那你最后实得多少？",
                    "请输入最后实得",
                    45 * 96 * 8 / 10,
                    "你把账一报，牙郎都愣了一下，连说你这脑子比算盘还快。",
                    "你没算明白，牙郎便顺手抹了点零头，说是替你省心。",
                    null,
                    EffectPercents(("Gold", 0.14f)),
                    null,
                    EffectPercents(("Gold", -0.04f)))
            },
            {
                "e_black_market_invite",
                Choice(
                    "e_black_market_invite",
                    "黑市递来帖子",
                    "🕶️",
                    "夜里有人往你窗缝塞了张帖子，说子时三刻黑市开局，货路隐秘，价高得吓人，但若出了事，谁也不会替你作证。",
                    new EventChoice
                    {
                        Text = "冒险入场",
                        PercentEffects = EffectPercents(("Gold", 0.22f)),
                        ResultText = "你冒险去了，银票涨得快，心跳得更快。"
                    },
                    new EventChoice
                    {
                        Text = "先探风声",
                        PercentEffects = EffectPercents(("Gold", 0.05f)),
                        ResultText = "你在黑市门口听了半宿风声，虽未大赚，却摸清了不少门道。"
                    },
                    new EventChoice
                    {
                        Text = "不去黑市",
                        Effects = EffectInts(("Health", 3)),
                        ResultText = "你把帖子烧进灯芯里，睡得倒还踏实。"
                    })
            },
            {
                "e_bride_price_dispute",
                Choice(
                    "e_bride_price_dispute",
                    "彩礼当街扯皮",
                    "💍",
                    "两家人在街口为彩礼争得脸红脖子粗，媒婆一把拽住你，要你这个过路商人来评一评价。你若说得服众，有赏；若说错了，免不了两头得罪。",
                    new EventChoice
                    {
                        Text = "帮男方砍",
                        PercentEffects = EffectPercents(("Gold", 0.06f)),
                        ResultText = "男方喜得直拍你肩膀，临走还偷偷塞了你一张银票。"
                    },
                    new EventChoice
                    {
                        Text = "帮女方抬",
                        PercentEffects = EffectPercents(("Gold", 0.07f)),
                        ResultText = "女方家里高兴得很，嘴上喊你公道，手上也没亏待你。"
                    },
                    new EventChoice
                    {
                        Text = "两边安抚",
                        PercentEffects = EffectPercents(("Gold", 0.03f)),
                        ResultText = "你把场面圆过去了，只是费心费口舌。"
                    })
            },
            {
                "e_grain_warehouse",
                Calc(
                    "e_grain_warehouse",
                    "粮仓盘点",
                    "🌾",
                    "粮仓掌事临时抓你帮忙盘库，说仓里有28车粟米，每车113两，另需扣去装卸脚钱144两。若你算得快，当场就给你辛苦钱。",
                    "请输入盘点后的总货值",
                    28 * 113 - 144,
                    "你报数利落，掌事喜得把辛苦钱直接拍在你掌心。",
                    "你盘了半天还是错，掌事嫌你碍事，只让你帮着搬了两趟麻袋。",
                    null,
                    EffectPercents(("Gold", 0.11f)),
                    null,
                    EffectPercents(("Gold", -0.03f)))
            },
            {
                "e_debt_rollover",
                Choice(
                    "e_debt_rollover",
                    "债主递来新契",
                    "📜",
                    "旧债主摇着新契来找你，说若现在续一笔新约，可以缓你眼下的催逼；若不续，月底就得清账。你看那张契纸，越看越像张网。",
                    new EventChoice
                    {
                        Text = "续约缓压",
                        PercentEffects = EffectPercents(("Gold", 0.10f)),
                        ResultText = "你眼前宽松了些，可账却被拉得更长更重。"
                    },
                    new EventChoice
                    {
                        Text = "先还一截",
                        PercentEffects = EffectPercents(("Gold", -0.12f)),
                        ResultText = "你咬牙先砍掉一截债，夜里总算能睡得安生一点。"
                    },
                    new EventChoice
                    {
                        Text = "再拖几日",
                        PercentEffects = EffectPercents(("Debt", 0.08f)),
                        ResultText = "你暂时搪塞过去，可利滚利这种事，从来不等人。"
                    })
            },
            {
                "e_scholar_borrow",
                Choice(
                    "e_scholar_borrow",
                    "穷书生借路费",
                    "📖",
                    "破庙里，一个穷书生捧着卷子朝你作揖，说赶考只差几分路费。看他眼神诚恳得很，可这世道也最会拿诚恳骗人。",
                    new EventChoice
                    {
                        Text = "借他路费",
                        PercentEffects = EffectPercents(("Gold", -0.05f)),
                        ResultText = "你塞了些银子给他，书生感激得险些给你磕头。"
                    },
                    new EventChoice
                    {
                        Text = "立据再借",
                        PercentEffects = EffectPercents(("Gold", -0.03f)),
                        ResultText = "你让他立了字据，虽显得凉薄，却也把账说清了。"
                    },
                    new EventChoice
                    {
                        Text = "婉拒离开",
                        Effects = EffectInts(("Health", -1)),
                        ResultText = "你摆手离开，心里却总像留了根刺。"
                    })
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
            int randomIndex = Random.Range(0, eventKeys.Count);
            return EVENTS[eventKeys[randomIndex]];
        }

        private static GameEvent Choice(string id, string title, string icon, string description, params EventChoice[] choices)
        {
            return new GameEvent
            {
                Id = id,
                Title = title,
                Icon = icon,
                Description = description,
                Type = EventType.Choice,
                Choices = new List<EventChoice>(choices)
            };
        }

        private static GameEvent Calc(
            string id,
            string title,
            string icon,
            string description,
            string inputPrompt,
            int correctAnswer,
            string correctResultText,
            string wrongResultText,
            Dictionary<string, int> correctEffects,
            Dictionary<string, float> correctPercentEffects,
            Dictionary<string, int> wrongEffects,
            Dictionary<string, float> wrongPercentEffects)
        {
            return new GameEvent
            {
                Id = id,
                Title = title,
                Icon = icon,
                Description = description,
                Type = EventType.Input,
                InputPrompt = inputPrompt,
                CorrectAnswer = correctAnswer,
                AnswerTolerance = 0,
                CorrectResultText = correctResultText,
                WrongResultText = wrongResultText,
                CorrectEffects = correctEffects,
                CorrectPercentEffects = correctPercentEffects,
                WrongEffects = wrongEffects,
                WrongPercentEffects = wrongPercentEffects
            };
        }

        private static Dictionary<string, int> EffectInts(params (string key, int value)[] items)
        {
            var effects = new Dictionary<string, int>();
            foreach (var item in items)
            {
                effects[item.key] = item.value;
            }

            return effects;
        }

        private static Dictionary<string, float> EffectPercents(params (string key, float value)[] items)
        {
            var effects = new Dictionary<string, float>();
            foreach (var item in items)
            {
                effects[item.key] = item.value;
            }

            return effects;
        }
    }
}
