using System.Collections.Generic;
using UnityEngine;

public static class TankUtil
{
    /// <summary>
    /// ���� ����Ʈ���� ��/�� ���� ����մϴ�.
    /// </summary>
    public static (int wins, int losses) GetWinLoss(List<UserBattleInfo> battleInfos)
    {
        int win = 0, lose = 0;

        if (battleInfos == null)
            return (0, 0);

        foreach (var info in battleInfos)
        {
            if (info.result == "win") win++;
            else if (info.result == "lose") lose++;
        }

        return (win, lose);
    }

    /// <summary>
    /// ���� ��ũ Ű�� �ش��ϴ� ��ũ ��������Ʈ�� ���ҽ����� ã�� ��ȯ�մϴ�.
    /// </summary>
    public static Sprite GetTankSprite(string tankKey)
    {
        if (string.IsNullOrEmpty(tankKey)) return null;

        TankDataSO[] tankArray = Resources.LoadAll<TankDataSO>("Tank");

        foreach (var tank in tankArray)
        {
            if (tank._tankName == tankKey)
                return tank._tankSprite;
        }

        return null;
    }

    /// <summary>
    /// ���� ��ũ Ű�� �ش��ϴ� ��ũ ������ ��ü�� ��ȯ�մϴ�.
    /// </summary>
    public static TankDataSO GetTankData(string tankKey)
    {
        if (string.IsNullOrEmpty(tankKey)) return null;

        TankDataSO[] tankArray = Resources.LoadAll<TankDataSO>("Tank");

        foreach (var tank in tankArray)
        {
            if (tank._tankName == tankKey)
                return tank;
        }

        return null;
    }
}