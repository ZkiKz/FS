using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace FuSheng
{
    public class Message : MonoBehaviour
    {
        [Header("UI组件")]
        public GameObject panel;                    // 弹窗面板
        public TextMeshProUGUI titleText;           // 标题文本
        public TextMeshProUGUI contentText;         // 内容文本
        public Button confirmButton;                // 确认按钮
        public Button cancelButton;                 // 取消按钮
        public TextMeshProUGUI confirmButtonText;   // 确认按钮文字
        public TextMeshProUGUI cancelButtonText;    // 取消按钮文字

        // 回调事件
        private Action onConfirmCallback;
        private Action onCancelCallback;

        private static Message instance;

        private void Awake()
        {
            // 单例模式
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // 绑定按钮事件
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmClick);
            }
            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(OnCancelClick);
            }

            // 初始隐藏
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        /// <summary>
        /// 显示提示弹窗（最简单用法）
        /// </summary>
        /// <param name="content">提示内容</param>
        public static void Show(string content)
        {
            Show(content, null, null, false);
        }

        /// <summary>
        /// 显示提示弹窗（带确认回调）
        /// </summary>
        /// <param name="content">提示内容</param>
        /// <param name="onConfirm">确认按钮回调</param>
        public static void Show(string content, Action onConfirm)
        {
            Show(content, onConfirm, null, false);
        }

        /// <summary>
        /// 显示提示弹窗（完整参数）
        /// </summary>
        /// <param name="content">提示内容</param>
        /// <param name="onConfirm">确认按钮回调</param>
        /// <param name="onCancel">取消按钮回调</param>
        /// <param name="showCancel">是否显示取消按钮</param>
        /// <param name="title">标题（可选，默认为"提示"）</param>
        /// <param name="confirmText">确认按钮文字（可选，默认为"确定"）</param>
        /// <param name="cancelText">取消按钮文字（可选，默认为"取消"）</param>
        public static void Show(
            string content, 
            Action onConfirm = null, 
            Action onCancel = null, 
            bool showCancel = false,
            string title = "提示",
            string confirmText = "确定",
            string cancelText = "取消")
        {
            if (instance == null)
            {
                Debug.LogError("Message实例未找到！请确保场景中有Message组件。");
                return;
            }

            // 设置回调
            instance.onConfirmCallback = onConfirm;
            instance.onCancelCallback = onCancel;

            // 设置内容
            if (instance.contentText != null)
            {
                instance.contentText.text = content;
            }

            // 设置标题
            if (instance.titleText != null)
            {
                instance.titleText.text = title;
            }

            // 设置按钮文字
            if (instance.confirmButtonText != null)
            {
                instance.confirmButtonText.text = confirmText;
            }
            if (instance.cancelButtonText != null)
            {
                instance.cancelButtonText.text = cancelText;
            }

            // 控制取消按钮显示
            if (instance.cancelButton != null)
            {
                instance.cancelButton.gameObject.SetActive(showCancel);
            }

            // 显示面板
            if (instance.panel != null)
            {
                instance.panel.SetActive(true);
            }
        }

        /// <summary>
        /// 关闭弹窗
        /// </summary>
        public static void Hide()
        {
            if (instance != null && instance.panel != null)
            {
                instance.panel.SetActive(false);
            }
        }

        /// <summary>
        /// 确认按钮点击事件
        /// </summary>
        private void OnConfirmClick()
        {
            // 执行确认回调
            onConfirmCallback?.Invoke();
            
            // 关闭弹窗
            Hide();
        }

        /// <summary>
        /// 取消按钮点击事件
        /// </summary>
        private void OnCancelClick()
        {
            // 执行取消回调
            onCancelCallback?.Invoke();
            
            // 关闭弹窗
            Hide();
        }
    }
}
