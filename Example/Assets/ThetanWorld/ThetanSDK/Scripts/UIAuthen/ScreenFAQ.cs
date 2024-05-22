using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Screen = ThetanSDK.UI.Screen;

namespace ThetanSDK.UI
{
    public class ScreenFAQ : Screen
    {
        [SerializeField] private ItemFAQ _prefabItemFAQ;
        [SerializeField] private Transform _contentParent;

        private List<ItemFAQ> _listSpawnedFAQ = new List<ItemFAQ>();

        private List<(string, string)> _listFAQ = new List<(string, string)>()
        {
            (
                "What's Thetan World?",
                "Thetan ID là hệ thống login dành cho tất cả các game trong hệ thống. Người chơi có thể làm gì đó với gì đó"
            ),
            (
                "What's Thetan World?",
                "Thetan ID là hệ thống login dành cho tất cả các game trong hệ thống. Người chơi có thể làm gì đó với gì đó"
            ),
            (
                "What's Thetan World?",
                "Thetan ID là hệ thống login dành cho tất cả các game trong hệ thống. Người chơi có thể làm gì đó với gì đó"
            ),
            (
                "What's Thetan World?",
                "Thetan ID là hệ thống login dành cho tất cả các game trong hệ thống. Người chơi có thể làm gì đó với gì đó"
            ),
            (
                "What's Thetan World?",
                "Thetan ID là hệ thống login dành cho tất cả các game trong hệ thống. Người chơi có thể làm gì đó với gì đó"
            ),
            (
                "What's Thetan World?",
                "Thetan ID là hệ thống login dành cho tất cả các game trong hệ thống. Người chơi có thể làm gì đó với gì đó"
            ),
        };
        
        private void Awake()
        {
            foreach (var faq in _listFAQ)
            {
                var faqItem = Instantiate(_prefabItemFAQ, _contentParent);
                faqItem.transform.SetAsLastSibling();
                faqItem.Initialize(faq.Item1, faq.Item2);
                _listSpawnedFAQ.Add(faqItem);
            }
        }

        public override void OnBeforePushScreen()
        {
            base.OnBeforePushScreen();

            foreach (var faqItem in _listSpawnedFAQ)
            {
                faqItem.CollapseDescription();
            }
        }
    }
}