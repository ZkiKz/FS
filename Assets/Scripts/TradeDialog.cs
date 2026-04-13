using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FuSheng
{
    public class TradeDialog : MonoBehaviour
    {
        [Header("UI组件")]
        public GameObject dialogPanel;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI nameText;
        public TMP_InputField quantityText;
        public TextMeshProUGUI priceText;
        public TextMeshProUGUI balanceText;
        public TextMeshProUGUI totalText;
        public Button confirmButton;
        public Button cancelButton;
        public Button increaseButton;
        public Button decreaseButton;
        public Button allButton;
        
        [Header("交易信息")]
        private string currentCommodityId;
        private string currentCityId;
        private bool isBuying; // true: 买入, false: 卖出
        private int currentQuantity = 1;
        private int currentPrice;
        private System.Action<string, string, int, bool> onConfirmCallback;
        
        void Start()
        {
            // 绑定按钮事件
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirm);
            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancel);
            if (increaseButton != null)
                increaseButton.onClick.AddListener(OnIncreaseQuantity);
            if (decreaseButton != null)
                decreaseButton.onClick.AddListener(OnDecreaseQuantity);
            if (allButton != null)
                allButton.onClick.AddListener(OnAllButtonClick);
            
            // 绑定输入字段事件
            if (quantityText != null)
                quantityText.onValueChanged.AddListener(OnQuantityInputChanged);
        }
        
        public void ShowTradeDialog(string commodityId, string cityId, bool buying, int price, System.Action<string, string, int, bool> callback)
        {
            currentCommodityId = commodityId;
            currentCityId = cityId;
            isBuying = buying;
            currentPrice = price;
            currentQuantity = 1;
            onConfirmCallback = callback;
            
            // 更新UI
            UpdateDialogUI();
            
            // 显示弹框
            if (dialogPanel != null)
                dialogPanel.SetActive(true);
        }
        
        private void UpdateDialogUI()
        {
            var commodity = Data.GetCommodity(currentCommodityId);
            if (commodity == null) return;
            
            // 更新标题和消息
            if (titleText != null)
                titleText.text = isBuying ? "买入商品" : "卖出商品";
            if (nameText != null)
                nameText.text = commodity.Name;
            
            // 更新数量
            if (quantityText != null)
                quantityText.text = currentQuantity.ToString();
            
            // 更新价格信息
            if (priceText != null)
                priceText.text = $"{currentPrice} 两";
            
            // 获取当前余额
            int currentGold = GetCurrentGold();
            if (balanceText != null)
                balanceText.text = $"{currentGold} 两";
            
            if (totalText != null)
                totalText.text = $"{currentQuantity * currentPrice} 两";
        }
        
        private void OnIncreaseQuantity()
        {
            currentQuantity++;
            UpdateDialogUI();
        }
        
        private void OnDecreaseQuantity()
        {
            if (currentQuantity > 1)
            {
                currentQuantity--;
                UpdateDialogUI();
            }
        }
        
        private void OnConfirm()
        {
            onConfirmCallback?.Invoke(currentCommodityId, currentCityId, currentQuantity, isBuying);
            HideDialog();
        }
        
        private void OnCancel()
        {
            HideDialog();
        }
        
        public void HideDialog()
        {
            if (dialogPanel != null)
                dialogPanel.SetActive(false);
        }

        // 全部按钮点击事件
        private void OnAllButtonClick()
        {
            if (isBuying)
            {
                // 计算最大可购买数量
                int maxQuantity = CalculateMaxBuyQuantity();
                currentQuantity = maxQuantity;
            }
            else
            {
                // 卖出时，最大数量为库存数量
                int maxQuantity = CalculateMaxSellQuantity();
                currentQuantity = maxQuantity;
            }
            UpdateDialogUI();
        }

        // 计算最大可购买数量
        private int CalculateMaxBuyQuantity()
        {
            // 获取当前游戏状态（需要通过GameManager获取）
            var gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null) return 1;
            
            var gameState = gameManager.GetGameState();
            if (gameState == null) return 1;
            
            // 计算基于余额的最大数量
            int maxByGold = gameState.Stats.Gold / currentPrice;
            
            // 计算基于容量的最大数量
            int currentCapacity = Engine.CalculateCurrentCapacity(gameState.Inventory);
            int maxByCapacity = gameState.Stats.Capacity - currentCapacity;
            
            // 取两者中的较小值
            int maxQuantity = Mathf.Min(maxByGold, maxByCapacity);
            
            // 确保至少为1
            return Mathf.Max(1, maxQuantity);
        }

        // 计算最大可卖出数量
        private int CalculateMaxSellQuantity()
        {
            // 获取当前游戏状态
            var gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null) return 1;
            
            var gameState = gameManager.GetGameState();
            if (gameState == null) return 1;
            
            // 查找库存中的商品
            var inventoryItem = gameState.Inventory.Find(item => item.CommodityId == currentCommodityId);
            if (inventoryItem == null) return 0;
            
            // 返回库存数量
            return inventoryItem.Quantity;
        }

        // 数量输入变化事件
        private void OnQuantityInputChanged(string input)
        {
            if (int.TryParse(input, out int newQuantity))
            {
                // 验证输入的数量
                int maxQuantity = isBuying ? CalculateMaxBuyQuantity() : CalculateMaxSellQuantity();
                
                if (newQuantity < 1)
                {
                    currentQuantity = 1;
                }
                else if (newQuantity > maxQuantity)
                {
                    currentQuantity = maxQuantity;
                }
                else
                {
                    currentQuantity = newQuantity;
                }
                
                // 更新UI显示
                UpdateDialogUI();
            }
            else if (!string.IsNullOrEmpty(input))
            {
                // 输入无效，恢复为当前数量
                if (quantityText != null)
                    quantityText.text = currentQuantity.ToString();
            }
        }

        // 获取当前游戏状态（需要在GameManager中添加此方法）
        public void SetGameManager(GameManager manager)
        {
            // 这个方法将在GameManager中调用，用于设置引用
        }

        // 获取当前资金余额
        private int GetCurrentGold()
        {
            var gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null) return 0;
            
            var gameState = gameManager.GetGameState();
            if (gameState == null) return 0;
            
            return gameState.Stats.Gold;
        }
    }
}