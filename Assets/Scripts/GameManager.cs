using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FuSheng
{
    public class GameManager : MonoBehaviour
    {
        private GameState gameState;

        // UI元素 - 这些将在Unity编辑器中手动拖入
        [Header("游戏状态UI")]
        public TextMeshProUGUI dayText;
        public TextMeshProUGUI goldText;
        public TextMeshProUGUI depositText;
        public TextMeshProUGUI debtText;
        public TextMeshProUGUI healthText;
        public TextMeshProUGUI capacityText;
        public TextMeshProUGUI currentCityText;
   

        [Header("商品UI")]
        public Transform commodityPanel;
        public GameObject commodityItemPrefab;
        public List<Sprite> commodityIcons;

        [Header("城市UI")]
        public List<Toggle> cityUIList;

        [Header("库存UI")]
        public Transform inventoryPanel;
        public GameObject inventoryItemPrefab;

        [Header("事件UI")]
        public GameObject eventPanel;
        public TextMeshProUGUI eventTitleText;
        public TextMeshProUGUI eventDescriptionText;
        public TextMeshProUGUI eventEffectText;
        public Transform eventChoicesPanel;
        public GameObject eventChoiceButtonPrefab;
        public GameObject eventInputPanel;
        public TextMeshProUGUI eventInputPromptText;
        public TMP_InputField eventAnswerInput;
        public Button eventSubmitButton;

        [Header("结束UI")]
        public GameObject endingPanel;
        public TextMeshProUGUI endingMessageText;
        public TextMeshProUGUI finalScoreText;
        public TextMeshProUGUI finalRankText;

        [Header("银行和医院UI")]
        public GameObject bankPanel;
        public InputField bankAmountInput;
        public GameObject hospitalPanel;

        [Header("医馆UI")]
        public TextMeshProUGUI hospitalHealthText;      // 显示当前健康值
        public TextMeshProUGUI hospitalCostText;        // 显示治疗花费
        public TextMeshProUGUI hospitalDayText;         // 显示当前阶段
        public Button hospitalTreatOnceButton;          // 治疗一次按钮
        public Button hospitalTreatAllButton;           // 治疗全部按钮

        [Header("扩容UI")]
        public GameObject expansionPanel;               // 扩容面板
        public TextMeshProUGUI expansionRuleText;       // 规则
        public TextMeshProUGUI expansionCostText;       // 花费文本
        public TMP_InputField expansionCapacityInput;   // 数量输入框
        public Button expansionDecreaseButton;          // 减少按钮
        public Button expansionIncreaseButton;          // 增加按钮
        public Button expansionMaxButton;               // 最大按钮
        public Button expansionConfirmButton;           // 确认按钮
        public Button expansionCancelButton;            // 取消按钮
        public TextMeshProUGUI expansionCapacityText;   // 显示当前容量
        private int expansionQuantity = 1;              // 当前扩容数量

        [Header("底部功能按钮")]
        public Button bankButton;         // 钱庄按钮
        public Button hospitalButton;     // 医馆按钮
        public Button repayButton;        // 还债按钮
        public Button expansionButton;    // 扩容按钮
        public Button rankingButton;      // 排行按钮

        [Header("钱庄UI")]
        public TextMeshProUGUI bankDepositText;   // 显示存款
        public TextMeshProUGUI bankGoldText;      // 显示资金
        public Button bankDepositTabButton;       // 存入标签按钮
        public Button bankWithdrawTabButton;      // 取出标签按钮
        public TextMeshProUGUI bankAmountText;    // 显示当前操作金额
        public Button bankDecreaseButton;         // 减少按钮
        public Button bankIncreaseButton;         // 增加按钮
        public Button bankAllButton;              // 全部按钮
        public Button bankCancelButton;           // 取消按钮
        public Button bankConfirmButton;          // 确认按钮
        
        private bool isBankDepositMode = true;    // 当前模式：true=存入, false=取出
        private bool isBankRepayMode = false;     // 当前是否为还债模式
        private int bankOperationAmount = 0;      // 当前操作金额

        [Header("交易对话框")]
        public TradeDialog tradeDialog;

        [Header("交易按钮")]
        public Button buyButton;
        public Button sellButton;
        
        [Header("当前选中的商品")]
        private string selectedCommodityId;
        private int selectedCommodityPrice;

        private void Start()
        {
            // 初始化游戏状态
            gameState = Engine.CreateGame();
            
            // 绑定交易按钮事件
            if (buyButton != null)
                buyButton.onClick.AddListener(OnBuyButtonClick);
            if (sellButton != null)
                sellButton.onClick.AddListener(OnSellButtonClick);

            if (eventSubmitButton != null)
                eventSubmitButton.onClick.AddListener(OnEventSubmitButtonClick);
            
            // 绑定顶部功能按钮事件
            if (bankButton != null)
                bankButton.onClick.AddListener(OnBankButtonClick);
            if (hospitalButton != null)
                hospitalButton.onClick.AddListener(OnHospitalButtonClick);
            if (repayButton != null)
                repayButton.onClick.AddListener(OnRepayButtonClick);
            if (expansionButton != null)
                expansionButton.onClick.AddListener(OnExpansionButtonClick);
            if (rankingButton != null)
                rankingButton.onClick.AddListener(OnRankingButtonClick);
            
            // 绑定扩容面板按钮事件
            if (expansionDecreaseButton != null)
                expansionDecreaseButton.onClick.AddListener(OnExpansionDecrease);
            if (expansionIncreaseButton != null)
                expansionIncreaseButton.onClick.AddListener(OnExpansionIncrease);
            if (expansionMaxButton != null)
                expansionMaxButton.onClick.AddListener(OnExpansionMax);
            if (expansionConfirmButton != null)
                expansionConfirmButton.onClick.AddListener(OnExpansionConfirm);
            if (expansionCancelButton != null)
                expansionCancelButton.onClick.AddListener(OnExpansionCancel);
            
            // 初始禁用交易按钮
            UpdateTradeButtons();
            
            UpdateUI();
        }

        // 开始游戏
        public void StartGame()
        {
            gameState = Engine.StartGame(gameState);
            UpdateUI();
        }

        // 重新开始游戏
        public void RestartGame()
        {
            gameState = Engine.RestartGame();
            UpdateUI();
        }

        // 移动到城市
        public void MoveToCity(string cityId)
        {
            gameState = Engine.MoveToLocation(gameState, cityId);
            UpdateUI();
        }

        // 购买商品
        public void BuyCommodity(string commodityId, int quantity)
        {
            gameState = Engine.BuyCommodity(gameState, commodityId, gameState.CurrentCity, quantity);
            UpdateUI();
        }

        // 出售商品
        public void SellCommodity(string commodityId, int quantity)
        {
            gameState = Engine.SellCommodity(gameState, commodityId, gameState.CurrentCity, quantity);
            UpdateUI();
        }

        // 银行操作
        public void BankRepay()
        {
            if (int.TryParse(bankAmountInput.text, out int amount))
            {
                gameState = Engine.BankOperation(gameState, "repay", amount);
                UpdateUI();
                bankPanel.SetActive(false);
            }
        }

        public void BankLoan()
        {
            if (int.TryParse(bankAmountInput.text, out int amount))
            {
                gameState = Engine.BankOperation(gameState, "loan", amount);
                UpdateUI();
                bankPanel.SetActive(false);
            }
        }

        // 医院治疗 - 已废弃，请使用 OnHospitalTreatOnce 或 OnHospitalTreatAll
        // 保留此方法以兼容旧代码，实际功能已移至新方法
        public void HospitalTreat()
        {
            // 默认使用治疗一次
            OnHospitalTreatOnce();
            hospitalPanel.SetActive(false);
        }

        // 结束一天
        public void EndDay()
        {
            var (isDanger, message) = Engine.CheckDangerStatus(gameState);
            if (isDanger)
            {
                // 显示危险提示
                Debug.Log(message);
                // 这里可以添加UI提示
            }
            else
            {
                gameState = Engine.EndDay(gameState);
                UpdateUI();
            }
        }

        // 处理事件选择
        public void ResolveEvent(int choiceIndex)
        {
            gameState = Engine.ResolveEvent(gameState, choiceIndex);
            UpdateUI();
        }

        public void ResolveInputEvent()
        {
            if (eventAnswerInput == null)
                return;

            gameState = Engine.ResolveInputEvent(gameState, eventAnswerInput.text);
            UpdateUI();
        }

        // 更新UI
        private void UpdateUI()
        {
            // 更新游戏状态UI
            if (dayText != null) dayText.text = $"第 {gameState.Day}/{gameState.MaxDays} 天";
            if (goldText != null) goldText.text = $"{gameState.Stats.Gold}两";
            if (debtText != null) debtText.text = $"{gameState.Stats.Debt}两";
            if (healthText != null) healthText.text = $"{gameState.Stats.Health}";
            if (capacityText != null) capacityText.text = $"我的容量: {Engine.CalculateCurrentCapacity(gameState.Inventory)}/{gameState.Stats.Capacity}";
            if (currentCityText != null)
            {
                var currentCity = Data.GetCity(gameState.CurrentCity);
                cityUIList[currentCity.Index].isOn = true;
                currentCityText.text = currentCity != null ? $"当前城市: {currentCity.Name}" : "当前城市: 未知";
            }
            if (depositText != null) depositText.text = $"{gameState.Stats.Deposit}两";

            // 更新商品UI
            if (commodityPanel != null && commodityItemPrefab != null)
            {
                // 清空现有商品
                foreach (Transform child in commodityPanel)
                {
                    Destroy(child.gameObject);
                }

                // 获取当前城市可交易的商品（从MarketPrices中获取当前城市的商品）
                var currentCity = Data.GetCity(gameState.CurrentCity);
                if (currentCity != null)
                {
                    // 从MarketPrices中获取当前城市的商品
                    var currentCityCommodities = gameState.MarketPrices
                        .Where(p => p.CityId == gameState.CurrentCity)
                        .Select(p => Data.GetCommodity(p.CommodityId))
                        .Where(c => c != null)
                        .OrderBy(c => c.SortID)
                        .ToList();
                    
                    for (int i = 0; i < currentCityCommodities.Count; i++)
                    {
                        var commodity = currentCityCommodities[i];
                        var marketPrice = gameState.MarketPrices.Find(p => p.CommodityId == commodity.Id && p.CityId == gameState.CurrentCity);
                        if (marketPrice != null)
                        {
                            var item = Instantiate(commodityItemPrefab, commodityPanel);
                            item.SetActive(true);

                            // 设置商品图标
                            Transform iconTransform = item.transform.Find("Icon");
                            if (iconTransform != null)
                            {
                                Image iconImage = iconTransform.GetComponent<Image>();
                                if (iconImage != null && commodityIcons != null)
                                {
                                    int iconIndex = Data.COMMODITIES.FindIndex(c => c.Id == commodity.Id);
                                    if (iconIndex >= 0 && iconIndex < commodityIcons.Count && commodityIcons[iconIndex] != null)
                                    {
                                        iconImage.sprite = commodityIcons[iconIndex];
                                        iconImage.SetNativeSize();
                                        iconImage.enabled = true;
                                    }
                                }
                            }

                            // 更新商品名称和价格
                            TextMeshProUGUI[] texts = item.GetComponentsInChildren<TextMeshProUGUI>();
                            if (texts.Length >= 2)
                            {
                                texts[0].text = commodity.Name;
                                texts[1].text = marketPrice.CurrentPrice.ToString() + " 两";
                            }

                            // 添加商品选择事件（Toggle组件）
                            Toggle itemToggle = item.GetComponent<Toggle>();
                            if (itemToggle != null)
                            {
                                itemToggle.onValueChanged.RemoveAllListeners();
                                string commodityId = commodity.Id;
                                int price = marketPrice.CurrentPrice;
                                itemToggle.onValueChanged.AddListener((isOn) => OnCommodityToggleChanged(isOn, commodityId, price));
                            }
                        }
                    }
                }
            }

            // 更新城市UI
            if (cityUIList != null && cityUIList.Count > 0)
            {
                // 确保城市UI数量与城市数据匹配
                for (int i = 0; i < cityUIList.Count; i++)
                {
                    var cityUI = cityUIList[i];
                    if (cityUI != null && i < Data.CITIES.Count)
                    {
                        var city = Data.CITIES[i];
                        // 这里需要根据城市信息更新UI
                        // 例如：城市名称、描述、图标等
                        
                        // 添加点击事件
                        Toggle cityToggle = cityUI;
                        if (cityToggle != null)
                        {
                            // 清除现有的点击事件
                            cityToggle.onValueChanged.RemoveAllListeners();
                            // 添加新的点击事件
                            string cityId = city.Id;
                            cityToggle.onValueChanged.AddListener((isOn) => { if (isOn) MoveToCity(cityId); });
                        }
                    }
                }
            }

            // 更新库存UI
            if (inventoryPanel != null && inventoryItemPrefab != null)
            {
                // 清空现有库存
                foreach (Transform child in inventoryPanel)
                {
                    Destroy(child.gameObject);
                }

                // 添加库存物品
                for (int i = 0; i < gameState.Inventory.Count; i++)
                {
                    var inventoryItemData = gameState.Inventory[i];
                    var commodity = Data.GetCommodity(inventoryItemData.CommodityId);
                    if (commodity != null)
                    {
                        var inventoryItem = Instantiate(inventoryItemPrefab, inventoryPanel);
                        inventoryItem.SetActive(true);

                        // 设置库存商品图标
                        Transform iconTransform = inventoryItem.transform.Find("Icon");
                        if (iconTransform != null)
                        {
                            Image iconImage = iconTransform.GetComponent<Image>();
                            if (iconImage != null && commodityIcons != null)
                            {
                                int iconIndex = Data.COMMODITIES.FindIndex(c => c.Id == commodity.Id);
                                if (iconIndex >= 0 && iconIndex < commodityIcons.Count && commodityIcons[iconIndex] != null)
                                {
                                    iconImage.sprite = commodityIcons[iconIndex];
                                    iconImage.SetNativeSize();
                                    iconImage.enabled = true;
                                }
                            }
                        }

                        // 更新库存商品信息
                        TextMeshProUGUI[] texts = inventoryItem.GetComponentsInChildren<TextMeshProUGUI>();
                        if (texts.Length >= 3)
                        {
                            texts[0].text = commodity.Name;
                            texts[1].text = $"{inventoryItemData.PurchasePrice}两";
                            texts[2].text = $"{inventoryItemData.Quantity}";                        
                        }

                        // 添加商品选择事件（Toggle组件）
                        Toggle itemToggle = inventoryItem.GetComponent<Toggle>();
                        if (itemToggle != null)
                        {
                            itemToggle.onValueChanged.RemoveAllListeners();
                            string commodityId = commodity.Id;
                            int price = gameState.MarketPrices.Find(p => 
                                p.CommodityId == commodity.Id && p.CityId == gameState.CurrentCity
                            )?.CurrentPrice ?? 0;
                            itemToggle.onValueChanged.AddListener((isOn) => OnCommodityToggleChanged(isOn, commodityId, price));
                        }
                    }
                }
            }

            // 更新事件UI
            if (eventPanel != null)
            {
                bool hasCurrentEvent = gameState.CurrentEvent != null;
                bool hasResult = !string.IsNullOrEmpty(gameState.LastEventResultText);

                if (hasCurrentEvent || hasResult)
                {
                    if (hasResult && !hasCurrentEvent)
                    {
                        eventPanel.SetActive(true);
                        if (eventTitleText != null) eventTitleText.text = "事件结果";
                        if (eventDescriptionText != null) eventDescriptionText.text = gameState.LastEventResultText;
                        if (eventEffectText != null) {
                            eventEffectText.gameObject.SetActive(true);
                            eventEffectText.text = gameState.LastEventEffectText;
                        }

                        if (eventInputPanel != null)
                            eventInputPanel.SetActive(false);

                        if (eventChoicesPanel != null)
                            eventChoicesPanel.gameObject.SetActive(true);

                        if (eventChoicesPanel != null && eventChoiceButtonPrefab != null)
                        {
                            foreach (Transform child in eventChoicesPanel)
                            {
                                Destroy(child.gameObject);
                            }

                            var button = Instantiate(eventChoiceButtonPrefab, eventChoicesPanel);
                            button.SetActive(true);
                            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                            if (buttonText != null) buttonText.text = "继续";
                            button.GetComponent<Button>().onClick.AddListener(OnCloseEventResultClick);
                        }

                        // 结果显示时不再继续构建事件选项
                        return;
                    }

                    bool hasInputUI = eventInputPanel != null
                        && eventInputPromptText != null
                        && eventAnswerInput != null
                        && eventSubmitButton != null;
                    bool shouldUseInputPanel = gameState.CurrentEvent.Type == EventType.Input && hasInputUI;

                    eventPanel.SetActive(true);
                    if (eventTitleText != null) eventTitleText.text = gameState.CurrentEvent.Title;
                    if (eventEffectText != null) { eventEffectText.gameObject.SetActive(false); eventEffectText.text = string.Empty; }
                    if (eventDescriptionText != null)
                    {
                        eventDescriptionText.text = shouldUseInputPanel
                            ? gameState.CurrentEvent.Description
                            : gameState.CurrentEvent.Type == EventType.Input
                                ? $"{gameState.CurrentEvent.Description}\n\n{gameState.CurrentEvent.InputPrompt}"
                                : gameState.CurrentEvent.Description;
                    }

                    if (eventInputPanel != null)
                        eventInputPanel.SetActive(shouldUseInputPanel);

                    if (eventChoicesPanel != null)
                        eventChoicesPanel.gameObject.SetActive(gameState.CurrentEvent.Type == EventType.Choice || !shouldUseInputPanel);

                    if (eventChoicesPanel != null && eventChoiceButtonPrefab != null)
                    {
                        // 清空现有选项
                        foreach (Transform child in eventChoicesPanel)
                        {
                            Destroy(child.gameObject);
                        }

                        // 添加事件选项
                        if (gameState.CurrentEvent.Type == EventType.Choice)
                        {
                            for (int i = 0; i < gameState.CurrentEvent.Choices.Count; i++)
                            {
                                var choice = gameState.CurrentEvent.Choices[i];
                                var button = Instantiate(eventChoiceButtonPrefab, eventChoicesPanel);
                                button.SetActive(true);
                                var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                                if (buttonText != null) buttonText.text = choice.Text;

                                int choiceIndex = i;
                                button.GetComponent<Button>().onClick.AddListener(() => ResolveEvent(choiceIndex));
                            }
                        }
                        else if (!shouldUseInputPanel)
                        {
                            var button = Instantiate(eventChoiceButtonPrefab, eventChoicesPanel);
                            button.SetActive(true);
                            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                            if (buttonText != null) buttonText.text = "我算不出来，认罚";
                            button.GetComponent<Button>().onClick.AddListener(() => ResolveInputEvent());
                        }
                    }

                    if (shouldUseInputPanel)
                    {
                        if (eventInputPromptText != null)
                            eventInputPromptText.text = gameState.CurrentEvent.InputPrompt;

                        if (eventAnswerInput != null)
                            eventAnswerInput.text = string.Empty;

                        if (eventSubmitButton != null)
                            eventSubmitButton.interactable = true;
                    }
                }
                else
                {
                    eventPanel.SetActive(false);
                    if (eventEffectText != null) { eventEffectText.gameObject.SetActive(false); eventEffectText.text = string.Empty; }
                }
            }

            // 更新结束UI
            if (endingPanel != null)
            {
                if (gameState.Phase == GamePhase.Ending)
                {
                    endingPanel.SetActive(true);
                    if (endingMessageText != null) endingMessageText.text = gameState.EndMessage;
                    if (finalScoreText != null) finalScoreText.text = $"最终得分: {gameState.FinalScore}";
                    if (finalRankText != null) finalRankText.text = gameState.FinalRank;
                }
                else
                {
                    endingPanel.SetActive(false);
                }
            }

            // 隐藏银行、医院和扩容面板
            if (bankPanel != null) bankPanel.SetActive(false);
            if (hospitalPanel != null) hospitalPanel.SetActive(false);
            if (expansionPanel != null) expansionPanel.SetActive(false);
        }

        // 打开银行面板
        public void OpenBankPanel()
        {
            if (bankPanel != null)
            {
                bankPanel.SetActive(true);
                UpdateBankUI();
            }
        }

        // 更新钱庄UI
        private void UpdateBankUI()
        {
            if (bankDepositText != null)
                bankDepositText.text = $"存款{gameState.Stats.Deposit}两";
            if (bankGoldText != null)
                bankGoldText.text = $"资金{gameState.Stats.Gold}两";

            // 更新标签按钮状态
            if (bankDepositTabButton != null)
            {
                var colors = bankDepositTabButton.colors;
                // 如果是还债模式，禁用存入标签的视觉效果或交互
                if (isBankRepayMode)
                {
                    colors.normalColor = Color.gray;
                    bankDepositTabButton.interactable = false;
                }
                else
                {
                    colors.normalColor = isBankDepositMode ? new Color(0.3f, 0.8f, 0.3f) : Color.gray;
                    bankDepositTabButton.interactable = true;
                }
                bankDepositTabButton.colors = colors;
            }
            if (bankWithdrawTabButton != null)
            {
                var colors = bankWithdrawTabButton.colors;
                // 如果是还债模式，高亮取出标签或显示为还债状态
                if (isBankRepayMode)
                {
                    colors.normalColor = new Color(0.8f, 0.3f, 0.3f); // 红色表示还债/取出
                    bankWithdrawTabButton.interactable = false; // 还债模式下不允许手动切换标签
                }
                else
                {
                    colors.normalColor = !isBankDepositMode ? new Color(0.8f, 0.3f, 0.3f) : Color.gray;
                    bankWithdrawTabButton.interactable = true;
                }
                bankWithdrawTabButton.colors = colors;
            }

            // 重置操作金额
            bankOperationAmount = 0;
            UpdateBankAmountDisplay();
        }

        // 更新金额显示
        private void UpdateBankAmountDisplay()
        {
            if (bankAmountText != null)
                bankAmountText.text = bankOperationAmount.ToString();

            // 更新按钮状态
            int maxAmount = isBankDepositMode ? gameState.Stats.Gold : gameState.Stats.Deposit;
            if (bankDecreaseButton != null)
                bankDecreaseButton.interactable = bankOperationAmount > 0;
            if (bankIncreaseButton != null)
                bankIncreaseButton.interactable = bankOperationAmount < maxAmount;
            if (bankConfirmButton != null)
                bankConfirmButton.interactable = bankOperationAmount > 0;
        }

        // 切换到存入模式
        public void OnBankDepositTabClick()
        {
            isBankDepositMode = true;
            UpdateBankUI();
        }

        // 切换到取出模式
        public void OnBankWithdrawTabClick()
        {
            isBankDepositMode = false;
            UpdateBankUI();
        }

        // 减少金额
        public void OnBankDecreaseClick()
        {
            if (bankOperationAmount > 0)
            {
                bankOperationAmount--;
                UpdateBankAmountDisplay();
            }
        }

        // 增加金额
        public void OnBankIncreaseClick()
        {
            int maxAmount = isBankDepositMode ? gameState.Stats.Gold : gameState.Stats.Deposit;
            if (bankOperationAmount < maxAmount)
            {
                bankOperationAmount++;
                UpdateBankAmountDisplay();
            }
        }

        // 设置全部金额
        public void OnBankAllClick()
        {
            bankOperationAmount = isBankDepositMode ? gameState.Stats.Gold : gameState.Stats.Deposit;
            UpdateBankAmountDisplay();
        }

        // 取消操作
        public void OnBankCancelClick()
        {
            bankPanel.SetActive(false);
            bankOperationAmount = 0;
            isBankRepayMode = false; // 重置还债模式
        }

        // 确认操作
        public void OnBankConfirmClick()
        {
            if (bankOperationAmount <= 0)
                return;

            if (isBankDepositMode)
            {
                // 存入
                gameState = Engine.BankOperation(gameState, "deposit", bankOperationAmount);
            }
            else
            {
                // 取出（用于还债或普通取款）
                gameState = Engine.BankOperation(gameState, "withdraw", bankOperationAmount);
            }

            UpdateUI();
            bankPanel.SetActive(false);
            bankOperationAmount = 0;
            isBankRepayMode = false; // 重置还债模式
        }

        // 打开医院面板
        public void OpenHospitalPanel()
        {
            if (hospitalPanel != null)
            {
                hospitalPanel.SetActive(true);
                UpdateHospitalUI();
            }
        }

        // 更新医馆UI
        private void UpdateHospitalUI()
        {
            int currentHealth = gameState.Stats.Health;
            int day = gameState.Day;
            int costPer10 = Engine.GetTreatmentCost(day);

            // 显示当前健康值
            if (hospitalHealthText != null)
                hospitalHealthText.text = $"健康:{currentHealth}";

            // 显示当前阶段和价格
            string stageText = "";
            if (day <= 10)
                stageText = "第1阶段(1-10天)";
            else if (day <= 20)
                stageText = "第2阶段(11-20天)";
            else if (day <= 30)
                stageText = "第3阶段(21-30天)";
            else
                stageText = "第4阶段(31-40天)";

            if (hospitalDayText != null)
                hospitalDayText.text = $"{stageText} - 每10点{costPer10}两";

            // 计算治疗全部的花费
            int healthToRecover = 100 - currentHealth;
            int totalCost = Engine.CalculateTreatmentCost(day, healthToRecover);

            if (hospitalCostText != null)
            {
                if (currentHealth >= 100)
                    hospitalCostText.text = "健康已满";
                else
                    hospitalCostText.text = $"治疗{healthToRecover}点花费{totalCost}两";
            }

            // 更新按钮状态
            if (hospitalTreatOnceButton != null)
                hospitalTreatOnceButton.interactable = currentHealth < 100 && gameState.Stats.Gold >= costPer10;

            if (hospitalTreatAllButton != null)
                hospitalTreatAllButton.interactable = currentHealth < 100 && gameState.Stats.Gold >= costPer10;
        }

        // 治疗一次
        public void OnHospitalTreatOnce()
        {
            int previousHealth = gameState.Stats.Health;
            gameState = Engine.HospitalTreatmentOnce(gameState);

            if (gameState.Stats.Health > previousHealth)
            {
                UpdateHospitalUI();
                UpdateUI();
            }
        }

        // 治疗全部
        public void OnHospitalTreatAll()
        {
            int previousHealth = gameState.Stats.Health;
            gameState = Engine.HospitalTreatmentAll(gameState);

            if (gameState.Stats.Health > previousHealth)
            {
                UpdateHospitalUI();
                UpdateUI();
            }
        }

        // 关闭银行面板
        public void CloseBankPanel()
        {
            if (bankPanel != null) bankPanel.SetActive(false);
        }

        // 关闭医院面板
        public void CloseHospitalPanel()
        {
            if (hospitalPanel != null) hospitalPanel.SetActive(false);
        }

        // 钱庄按钮点击事件
        private void OnBankButtonClick()
        {
            OpenBankPanel();
        }

        // 医馆按钮点击事件
        private void OnHospitalButtonClick()
        {
            OpenHospitalPanel();
        }

        // 还债按钮点击事件
        private void OnRepayButtonClick()
        {
            // 直接还债，使用当前所有资金还债
            if (gameState.Stats.Debt > 0 && gameState.Stats.Gold > 0)
            {
                int repayAmount = Mathf.Min(gameState.Stats.Gold, gameState.Stats.Debt);
                gameState = Engine.BankOperation(gameState, "repay", repayAmount);
                UpdateUI();
            }
        }

        // 扩容按钮点击事件
        private void OnExpansionButtonClick()
        {
            OpenExpansionPanel();
        }

        // 排行按钮点击事件
        private void OnRankingButtonClick()
        {
            // TODO: 打开排行面板
            Debug.Log("排行榜功能待实现");
        }

        // 打开扩容面板
        public void OpenExpansionPanel()
        {
            if (expansionPanel != null)
            {
                expansionPanel.SetActive(true);
                expansionQuantity = 1;
                UpdateExpansionUI();
            }
        }

        // 关闭扩容面板
        public void CloseExpansionPanel()
        {
            if (expansionPanel != null) expansionPanel.SetActive(false);
        }

        // 更新扩容UI
        private void UpdateExpansionUI()
        {
            int currentCapacity = gameState.Stats.Capacity;
            int expansionCount = gameState.Stats.ExpansionCount;

            // 更新规则文本，替换{0}为当前容量
            if (expansionRuleText != null)
            {
                string ruleText = expansionRuleText.text;
                if (ruleText.Contains("{0}"))
                {
                    expansionRuleText.text = ruleText.Replace("{0}", currentCapacity.ToString());
                }
            }

            // 计算当前扩容数量的总花费（固定25000，无折扣）
            int totalCost = expansionQuantity * Engine.CalculateExpansionCost(expansionCount);

            // 显示价格
            if (expansionCostText != null)
            {
                if (expansionQuantity == 0)
                    expansionCostText.text = "花费：0 金钱";
                else
                    expansionCostText.text = $"花费：{totalCost} 金钱";
            }

            // 显示数量
            if (expansionCapacityInput != null)
                expansionCapacityInput.text = expansionQuantity.ToString();

            // 更新容量显示
            if (expansionCapacityText != null)
                expansionCapacityText.text = $"当前容量：{currentCapacity}";

            // 按钮不再置灰，始终可点击
            if (expansionDecreaseButton != null)
                expansionDecreaseButton.interactable = true;

            if (expansionIncreaseButton != null)
                expansionIncreaseButton.interactable = true;

            if (expansionMaxButton != null)
                expansionMaxButton.interactable = true;

            if (expansionConfirmButton != null)
                expansionConfirmButton.interactable = true;
        }

        // 计算最大可扩容数量
        private int CalculateMaxExpansionQuantity()
        {
            int maxQuantity = 0;
            int remainingGold = gameState.Stats.Gold;
            int expansionCount = gameState.Stats.ExpansionCount;
            int unitCost = Engine.CalculateExpansionCost(expansionCount); // 固定25000

            while (remainingGold >= unitCost)
            {
                remainingGold -= unitCost;
                maxQuantity++;
            }

            return maxQuantity;
        }

        // 减少扩容数量
        public void OnExpansionDecrease()
        {
            if (expansionQuantity > 1)
            {
                expansionQuantity--;
                UpdateExpansionUI();
            }
        }

        // 增加扩容数量
        public void OnExpansionIncrease()
        {
            expansionQuantity++;
            UpdateExpansionUI();
        }

        // 设置最大扩容数量
        public void OnExpansionMax()
        {
            expansionQuantity = CalculateMaxExpansionQuantity();
            if (expansionQuantity == 0)
            {
                Message.Show("资金不足", null, null, false, "提示", "确定", "取消");
            }
            UpdateExpansionUI();
        }

        // 确认扩容
        public void OnExpansionConfirm()
        {
            if (expansionQuantity <= 0)
            {
                Message.Show("请选择扩容数量", null, null, false, "提示", "确定", "取消");
                return;
            }

            if (!Engine.CanExpand(gameState, expansionQuantity))
            {
                Message.Show("资金不足", null, null, false, "提示", "确定", "取消");
                return;
            }

            gameState = Engine.ExpandCapacity(gameState, expansionQuantity);
            UpdateUI();
            expansionPanel.SetActive(false);
        }

        // 取消扩容
        public void OnExpansionCancel()
        {
            expansionPanel.SetActive(false);
            expansionQuantity = 1;
        }

        // 交易确认回调
        private void OnTradeConfirmed(string commodityId, string cityId, int quantity, bool isBuying)
        {
            if (isBuying)
            {
                // 执行买入逻辑
                gameState = Engine.BuyCommodity(gameState, commodityId, cityId, quantity);
            }
            else
            {
                // 执行卖出逻辑
                gameState = Engine.SellCommodity(gameState, commodityId, cityId, quantity);
            }
            
            // 更新UI
            UpdateUI();
        }

        // 商品Toggle选择事件
        private void OnCommodityToggleChanged(bool isOn, string commodityId, int price)
        {
            if (isOn)
            {
                // 选中当前商品
                selectedCommodityId = commodityId;
                selectedCommodityPrice = price;
            }
            else
            {
                // 取消选中，如果取消的是当前选中的商品
                if (selectedCommodityId == commodityId)
                {
                    selectedCommodityId = null;
                    selectedCommodityPrice = 0;
                }
            }
            UpdateTradeButtons();
        }

        // 清除所有商品的选中状态
        private void ClearAllCommoditySelections()
        {
            // 清除商品列表的选中状态
            if (commodityPanel != null)
            {
                foreach (Transform child in commodityPanel)
                {
                    Toggle toggle = child.GetComponent<Toggle>();
                    if (toggle != null && toggle.isOn)
                    {
                        toggle.isOn = false;
                    }
                }
            }
            
            // 清除库存列表的选中状态
            if (inventoryPanel != null)
            {
                foreach (Transform child in inventoryPanel)
                {
                    Toggle toggle = child.GetComponent<Toggle>();
                    if (toggle != null && toggle.isOn)
                    {
                        toggle.isOn = false;
                    }
                }
            }
        }

        // 更新交易按钮状态
        private void UpdateTradeButtons()
        {
            bool hasSelectedCommodity = !string.IsNullOrEmpty(selectedCommodityId);
            
            if (buyButton != null)
            {
                buyButton.interactable = hasSelectedCommodity;
            }
            
            if (sellButton != null)
            {
                // 卖出按钮需要检查是否有库存
                bool hasInventory = false;
                if (hasSelectedCommodity)
                {
                    var inventoryItem = gameState.Inventory.Find(item => item.CommodityId == selectedCommodityId);
                    hasInventory = inventoryItem != null && inventoryItem.Quantity > 0;
                }
                sellButton.interactable = hasSelectedCommodity && hasInventory;
            }
        }

        // 独立的买入按钮点击事件
        private void OnBuyButtonClick()
        {
            if (!string.IsNullOrEmpty(selectedCommodityId) && tradeDialog != null)
            {
                tradeDialog.ShowTradeDialog(selectedCommodityId, gameState.CurrentCity, true, selectedCommodityPrice, OnTradeConfirmed);
            }
        }

        // 独立的卖出按钮点击事件
        private void OnSellButtonClick()
        {
            if (!string.IsNullOrEmpty(selectedCommodityId) && tradeDialog != null)
            {
                tradeDialog.ShowTradeDialog(selectedCommodityId, gameState.CurrentCity, false, selectedCommodityPrice, OnTradeConfirmed);
            }
        }

        private void OnEventSubmitButtonClick()
        {
            ResolveInputEvent();
        }

        private void OnCloseEventResultClick()
        {
            gameState.LastEventResultText = null;
            gameState.LastEventEffectText = null;
            UpdateUI();
        }

        // 获取当前游戏状态（供TradeDialog使用）
        public GameState GetGameState()
        {
            return gameState;
        }
    }
}
