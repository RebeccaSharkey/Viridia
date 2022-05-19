using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum PowerUpType { health, attack, speed }

[CreateAssetMenu(fileName = "New Power Up", menuName = "Abilities/New Power Up")]
public class PowerUpSO : AbilitySO
{
    public int powerUpPower;
    public PowerUpType powerUpType;

    public PowerUpSO()
    {
        powerUpPower = 1;
        powerUpType = PowerUpType.attack;
    }
}

[System.Serializable]
public class PowerUp : Ability
{
    public string abilityName;

    public int powerUpPower;
    public PowerUpType powerUpType;

    public PowerUp()
    {
        thisName = "Defualt Power Up";
        abilityName = thisName;
        powerUpPower = 1;
        powerUpType = PowerUpType.attack;
    }

    public PowerUp(PowerUpSO referencePowerUp)
    {
        thisName = referencePowerUp.abilityName;
        abilityName = thisName;

        abilityName = referencePowerUp.abilityName;
        powerUpPower = referencePowerUp.powerUpPower;
        powerUpType = referencePowerUp.powerUpType;
    }
}
