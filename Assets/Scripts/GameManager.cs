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
        public List<GameObject> cityUIList;

        [Header("库存UI")]
        public Transform inventoryPanel;
        public GameObject inventoryItemPrefab;

        [Header("事件UI")]
        public GameObject eventPanel;
        public TextMeshProUGUI eventTitleText;
        public TextMeshProUGUI eventDescriptionText;
        public Transform eventChoicesPanel;
        public GameObject eventChoiceButtonPrefab;

        [Header("结束UI")]
        public GameObject endingPanel;
        public TextMeshProUGUI endingMessageText;
        public TextMeshProUGUI finalScoreText;
        public TextMeshProUGUI finalRankText;

        [Header("银行和医院UI")]
        public GameObject bankPanel;
        public InputField bankAmountInput;
        public GameObject hospitalPanel;

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

        // 医院治疗
        public void HospitalTreat()
        {
            gameState = Engine.HospitalTreatment(gameState);
            UpdateUI();
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
                currentCityText.text = currentCity != null ? $"当前城市: {currentCity.Name}" : "当前城市: 未知";
            }
            if (depositText != null) depositText.text = $"0两";

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
                                texts[1].text = marketPrice.CurrentPrice.ToString() + "两";
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
                        Toggle cityToggle = cityUI.GetComponent<Toggle>();
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
                if (gameState.CurrentEvent != null)
                {
                    eventPanel.SetActive(true);
                    if (eventTitleText != null) eventTitleText.text = gameState.CurrentEvent.Title;
                    if (eventDescriptionText != null) eventDescriptionText.text = gameState.CurrentEvent.Description;

                    if (eventChoicesPanel != null && eventChoiceButtonPrefab != null)
                    {
                        // 清空现有选项
                        foreach (Transform child in eventChoicesPanel)
                        {
                            Destroy(child.gameObject);
                        }

                        // 添加事件选项
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
                }
                else
                {
                    eventPanel.SetActive(false);
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

            // 隐藏银行和医院面板
            if (bankPanel != null) bankPanel.SetActive(false);
            if (hospitalPanel != null) hospitalPanel.SetActive(false);
        }

        // 打开银行面板
        public void OpenBankPanel()
        {
            if (bankPanel != null) bankPanel.SetActive(true);
        }

        // 打开医院面板
        public void OpenHospitalPanel()
        {
            if (hospitalPanel != null) hospitalPanel.SetActive(true);
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

        // 获取当前游戏状态（供TradeDialog使用）
        public GameState GetGameState()
        {
            return gameState;
        }
    }
}
