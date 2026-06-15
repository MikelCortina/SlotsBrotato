using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    public int bonusCoins = 50;
    public int bonusVouchers = 1;

    public void GiveBossReward()
    {
        PlayerWallet.Instance?.AddCoins(bonusCoins);
        WaveVoucherManager.Instance?.AddVoucher(bonusVouchers);

        Debug.Log($"Boss reward: +{bonusCoins} monedas, +{bonusVouchers} vale");
    }
}