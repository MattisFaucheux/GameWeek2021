using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatInfoManager : MonoBehaviour
{
    public StatInfo[] gdStat = new StatInfo[3];
    public StatInfo[] gdProf = new StatInfo[3];
    public StatInfo[] gpStat = new StatInfo[2];
    public StatInfo[] gpProf = new StatInfo[3];
    public StatInfo[] artStat = new StatInfo[4];
    public StatInfo[] artProf = new StatInfo[3];
    public StatInfo directeur;

    public StatInfo GetStatInfo(enPlayerClass currentPlayerClass, int currentLvl)
    {
        switch (currentPlayerClass)
        {
            case enPlayerClass.GD:
                return GetGDStat(currentLvl);

            case enPlayerClass.GP:
                return GetGPStat(currentLvl);

            case enPlayerClass.ART:
                return GetArtStat(currentLvl);

            default:
                return GetGDStat(currentLvl);
        }
    }

    StatInfo GetGDStat(int currentLvl)
    {
        if (currentLvl < gdStat.Length)
        {
            return gdStat[currentLvl];
        }
        else if(currentLvl == gdStat.Length || (currentLvl > gdStat.Length && currentLvl < 5))
        {
            return gdProf[Random.Range(0, gdProf.Length)];
        }
        else
        {
            return directeur;
        }
    }

    StatInfo GetGPStat(int currentLvl)
    {
        if (currentLvl < gpStat.Length)
        {
            return gpStat[currentLvl];
        }
        else if (currentLvl == gpStat.Length || (currentLvl > gpStat.Length && currentLvl < 5))
        {
            return gpProf[Random.Range(0, gpProf.Length)];
        }
        else
        {
            return directeur;
        }
    }

    StatInfo GetArtStat(int currentLvl)
    {
        if (currentLvl < artStat.Length)
        {
            return artStat[currentLvl];
        }
        else if (currentLvl == artStat.Length || (currentLvl > artStat.Length && currentLvl < 5))
        {
            return artProf[Random.Range(0, artProf.Length)];
        }
        else
        {
            return directeur;
        }
    }
}
