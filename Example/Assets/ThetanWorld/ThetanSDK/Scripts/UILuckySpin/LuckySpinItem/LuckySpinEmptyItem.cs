namespace ThetanSDK.UI.LuckySpin
{
    internal class LuckySpinEmptyItem : LuckySpinItem
    {
        public override void ShowUI(LuckySpinItemUIData data)
        {
            _txtItemName.text = "Good Luck!";
        }
    }
}